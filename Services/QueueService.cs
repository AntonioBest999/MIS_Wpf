using Microsoft.EntityFrameworkCore;
using MisWpf.Infrastructure;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.Services
{
    public enum QueueStage { Waiting, Examining, WaitingResults, ReadyForDecision, Returned }

    /// <summary>Поступление в очереди приёмного отделения со всем, что нужно для карточки.</summary>
    public class QueueItem
    {
        public Admission Admission { get; init; } = null!;
        public Patient Patient => Admission.Patient;
        public List<LabOrder> LabOrders { get; init; } = new();
        public List<Attachment> Instrumental { get; init; } = new();
        public Hospitalization? RejectedHospitalization { get; init; }
        public string? PriorStay { get; init; }

        public bool IsReturned => RejectedHospitalization != null;
        public bool HasOrders => LabOrders.Count > 0 || Instrumental.Count > 0;
        public bool AllOrdersDone =>
            HasOrders
            && LabOrders.All(o => o.Status == LabOrderStatus.Completed)
            && Instrumental.All(OrderService.IsInstrumentalDone);
    }

    public class ShiftStats
    {
        public int Total { get; init; }
        public int Hospitalized { get; init; }
        public int Refused { get; init; }
    }

    public static class QueueService
    {
        public const string StatusArrived = "Поступил";
        public const string StatusBackInReception = "В приёмном отделении";
        public const string StatusSentToDepartment = "Направлен в отделение";
        public const string StatusRefused = "Отказ в госпитализации";
        public const string StatusTransferredOut = "Переведён в другое ЛПУ";
        public const string HospRejected = "Отклонён заведующим";
        public const string HospActive = "Госпитализирован";

        private static readonly TimeSpan Slack = TimeSpan.FromMinutes(10);

        public static async Task<List<QueueItem>> LoadAsync()
        {
            var since = DateTime.UtcNow.AddDays(-AppConfig.QueueDays);
            await using var db = Db.Create();

            var admissions = await db.Set<Admission>()
                .Include(a => a.Patient)
                .AsNoTracking()
                .Where(a => a.IsActive
                         && (a.Status == StatusArrived || a.Status == StatusBackInReception)
                         && a.AdmissionDateTime >= since)
                .OrderBy(a => a.AdmissionDateTime)
                .ToListAsync();

            admissions = admissions.Where(a => a.Patient != null).ToList();
            if (admissions.Count == 0) return new List<QueueItem>();

            var ids = admissions.Select(a => a.PatientId).Distinct().ToList();
            var early = since.AddDays(-1);

            var labs = await db.LabOrders
                .Include(o => o.Items).ThenInclude(i => i.Parameter)
                .AsNoTracking()
                .Where(o => ids.Contains(o.PatientId) && o.OrderedAt >= early && o.Status != LabOrderStatus.Cancelled)
                .ToListAsync();

            var attachments = await db.Attachments
                .AsNoTracking()
                .Where(a => ids.Contains(a.PatientId) && a.UploadedAt >= early)
                .ToListAsync();

            var inspections = await db.PrimaryInspections
                .AsNoTracking()
                .Where(p => ids.Contains(p.PatientId) && p.InspectionDateTime >= early)
                .Select(p => new { p.PatientId, p.InspectionDateTime })
                .ToListAsync();

            var hosps = await db.Hospitalizations
                .Include(h => h.Department)
                .AsNoTracking()
                .Where(h => ids.Contains(h.PatientId))
                .OrderByDescending(h => h.AdmissionDate)
                .ToListAsync();

            var result = new List<QueueItem>();
            foreach (var a in admissions)
            {
                var start = Format.AsUtc(a.AdmissionDateTime) - Slack;

                Hospitalization? rejected = null;
                if (a.Status == StatusBackInReception)
                {
                    rejected = hosps.FirstOrDefault(h => h.PatientId == a.PatientId
                                                         && h.Status == HospRejected
                                                         && Format.AsUtc(h.AdmissionDate) >= start);
                }
                else
                {
                    // Пациент, осмотр которого уже сохранён в WinForms-версии, в очередь не попадает
                    bool decided = inspections.Any(p => p.PatientId == a.PatientId && Format.AsUtc(p.InspectionDateTime) >= start);
                    if (decided) continue;
                }

                var prior = hosps.FirstOrDefault(h => h.PatientId == a.PatientId && Format.AsUtc(h.AdmissionDate) < start);

                result.Add(new QueueItem
                {
                    Admission = a,
                    LabOrders = labs.Where(o => o.PatientId == a.PatientId && Format.AsUtc(o.OrderedAt) >= start)
                                    .OrderBy(o => o.OrderedAt).ToList(),
                    Instrumental = attachments.Where(x => x.AdmissionId == a.Id
                                                          || (x.AdmissionId == null && x.PatientId == a.PatientId && Format.AsUtc(x.UploadedAt) >= start))
                                              .OrderBy(x => x.UploadedAt).ToList(),
                    RejectedHospitalization = rejected,
                    PriorStay = prior == null ? null
                        : $"{ShortDept(prior.Department?.Name)}, {Format.AsUtc(prior.AdmissionDate).ToLocalTime():MM.yyyy}"
                });
            }
            return result;
        }

        public static async Task<ShiftStats> LoadShiftStatsAsync()
        {
            var since = Session.ShiftStartUtc;
            await using var db = Db.Create();
            int total = await db.Set<Admission>().CountAsync(a => a.AdmissionDateTime >= since);
            int refused = await db.Set<Admission>().CountAsync(a => a.AdmissionDateTime >= since && a.Status == StatusRefused);
            int hosp = await db.Hospitalizations.CountAsync(h => h.AdmissionDate >= since && h.Status != HospRejected);
            return new ShiftStats { Total = total, Hospitalized = hosp, Refused = refused };
        }

        /// <summary>Клинические отделения, куда можно направить из приёмного, и сколько в них сейчас пациентов.</summary>
        public static async Task<List<(Department Dept, int Patients)>> LoadDepartmentsAsync()
        {
            var clinical = new[]
            {
                Department.DepartmentType.Surgical, Department.DepartmentType.PurulentSeptic,
                Department.DepartmentType.HeadNeckReconstructiveSurgery, Department.DepartmentType.Oncology,
                Department.DepartmentType.Urology, Department.DepartmentType.OtolaryngologyOphthalmology,
                Department.DepartmentType.Pulmonology, Department.DepartmentType.Cardiology,
                Department.DepartmentType.Infectious, Department.DepartmentType.Psychiatric,
                Department.DepartmentType.Gastroenterology, Department.DepartmentType.Chemotherapy,
                Department.DepartmentType.Hematology, Department.DepartmentType.MixedTrauma,
                Department.DepartmentType.Endocrinology, Department.DepartmentType.ResuscitationAnesthesiology
            };

            await using var db = Db.Create();
            var depts = await db.Departments.AsNoTracking()
                .Where(d => clinical.Contains(d.Type))
                .OrderBy(d => d.Name)
                .ToListAsync();

            var counts = await db.Hospitalizations.AsNoTracking()
                .Where(h => h.DischargeDate == null && h.Status != HospRejected)
                .GroupBy(h => h.DepartmentId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            return depts.Select(d => (d, counts.FirstOrDefault(c => c.Key == d.DepartmentId)?.Count ?? 0)).ToList();
        }

        public static string ShortDept(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "отделение";
            var n = name.Trim().ToLowerInvariant();
            n = n.Replace(" отделение", "").Replace("отделение ", "");
            return n;
        }
    }
}
