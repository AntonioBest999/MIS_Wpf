using Microsoft.EntityFrameworkCore;
using MisWpf.Infrastructure;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.Services
{
    /// <summary>
    /// Заказ для лаборанта. По требованиям лаборант видит только ФИО, возраст, номер
    /// (истории болезни или поступления) и список показателей — больше ничего о пациенте.
    /// </summary>
    public class LabWorkItem
    {
        public LabOrder Order { get; init; } = null!;
        public string Fio { get; init; } = "";
        public string Age { get; init; } = "";
        public bool IsFemale { get; init; }
        public string Number { get; init; } = "";
        public DateTime? CompletedAt { get; init; }
    }

    public static class LabWorkService
    {
        public static async Task<List<LabWorkItem>> LoadAsync(bool completedThisShift)
        {
            await using var db = Db.Create();
            var shiftStart = Session.ShiftStartUtc;

            IQueryable<LabOrder> q = db.LabOrders
                .Include(o => o.Patient)
                .Include(o => o.Items).ThenInclude(i => i.Parameter)
                .AsNoTracking();

            q = completedThisShift
                ? q.Where(o => o.Status == LabOrderStatus.Completed && o.Items.Any(i => i.PerformedAt >= shiftStart))
                : q.Where(o => o.Status == LabOrderStatus.Ordered || o.Status == LabOrderStatus.InProgress);

            var orders = await q.OrderBy(o => o.OrderedAt).ToListAsync();
            orders = orders.Where(o => o.Patient != null).ToList();

            var hospIds = orders.Where(o => o.HospitalizationId != null).Select(o => o.HospitalizationId!.Value).Distinct().ToList();
            var records = await db.Hospitalizations.AsNoTracking()
                .Where(h => hospIds.Contains(h.Id))
                .Select(h => new { h.Id, h.MedicalRecordNumber })
                .ToListAsync();

            var patientIds = orders.Select(o => o.PatientId).Distinct().ToList();
            var admissions = await db.Set<Admission>().AsNoTracking()
                .Where(a => patientIds.Contains(a.PatientId))
                .Select(a => new { a.Id, a.PatientId, a.AdmissionDateTime })
                .ToListAsync();

            var list = new List<LabWorkItem>();
            foreach (var o in orders)
            {
                string number;
                var rec = o.HospitalizationId == null ? null : records.FirstOrDefault(r => r.Id == o.HospitalizationId);
                if (rec != null)
                {
                    number = "И/б № " + rec.MedicalRecordNumber;
                }
                else
                {
                    var orderedAt = Format.AsUtc(o.OrderedAt).AddMinutes(10);
                    var adm = admissions.Where(a => a.PatientId == o.PatientId && Format.AsUtc(a.AdmissionDateTime) <= orderedAt)
                                        .OrderByDescending(a => a.AdmissionDateTime).FirstOrDefault();
                    number = adm != null ? "Поступление " + Format.AdmissionNumber(adm.Id) : "Заказ № " + o.LabOrderId;
                }

                list.Add(new LabWorkItem
                {
                    Order = o,
                    Fio = o.Patient.FullName,
                    Age = Format.Age(o.Patient),
                    IsFemale = Format.IsFemale(o.Patient.Gender),
                    Number = number,
                    CompletedAt = o.Items.Where(i => i.PerformedAt != null).Select(i => i.PerformedAt).Max()
                });
            }
            return completedThisShift ? list.OrderByDescending(i => i.CompletedAt).ToList() : list;
        }

        public static async Task TakeInWorkAsync(int orderId)
        {
            await using var db = Db.Create();
            var o = await db.LabOrders.FirstAsync(x => x.LabOrderId == orderId);
            if (o.Status == LabOrderStatus.Ordered)
            {
                o.Status = LabOrderStatus.InProgress;
                await db.SaveChangesAsync();
            }
        }

        /// <summary>Сохраняет введённые значения. complete = true закрывает заказ; после этого изменить ничего нельзя.</summary>
        public static async Task SaveAsync(int orderId, IReadOnlyDictionary<int, (string Result, string? Comment)> values, bool complete)
        {
            await using var db = Db.Create();
            var o = await db.LabOrders.Include(x => x.Items).FirstAsync(x => x.LabOrderId == orderId);
            if (o.Status == LabOrderStatus.Completed)
                throw new InvalidOperationException("Заказ уже выполнен, результаты изменить нельзя.");

            foreach (var item in o.Items)
            {
                if (!values.TryGetValue(item.LabOrderItemId, out var v)) continue;
                var result = (v.Result ?? "").Trim();
                if (result.Length == 0) continue;
                item.Result = result.Length > 50 ? result[..50] : result;
                item.Comment = string.IsNullOrWhiteSpace(v.Comment) ? null : v.Comment.Trim();
                item.PerformedAt = DateTime.UtcNow;
                item.PerformedById = Session.EmployeeId;
            }

            if (complete)
            {
                if (o.Items.Any(i => string.IsNullOrWhiteSpace(i.Result)))
                    throw new InvalidOperationException("Заполнены не все показатели.");
                o.Status = LabOrderStatus.Completed;
            }
            else if (o.Status == LabOrderStatus.Ordered)
            {
                o.Status = LabOrderStatus.InProgress;
            }

            await db.SaveChangesAsync();
        }

        /// <summary>Норма с учётом пола; null, если для показателя нормы нет (качественные показатели).</summary>
        public static (decimal Low, decimal High)? Range(LabParameter p, bool female)
        {
            decimal? lo = female && p.RefLowFemale.HasValue ? p.RefLowFemale : p.RefLow;
            decimal? hi = female && p.RefHighFemale.HasValue ? p.RefHighFemale : p.RefHigh;
            if (lo == null || hi == null) return null;
            if (lo == 0 && hi == 0) return null;
            return (lo.Value, hi.Value);
        }

        /// <summary>"↑", "↓" или "" для введённого значения.</summary>
        public static string Flag(LabParameter p, bool female, string? value)
        {
            var r = Range(p, female);
            if (r == null || string.IsNullOrWhiteSpace(value)) return "";
            if (!decimal.TryParse(value.Trim().Replace(',', '.'), System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out var v)) return "";
            if (v < r.Value.Low) return "↓";
            if (v > r.Value.High) return "↑";
            return "";
        }

        public static string RangeText(LabParameter p, bool female)
        {
            var r = Range(p, female);
            if (r == null) return "";
            return $"{r.Value.Low.ToString("0.###", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"))}–{r.Value.High.ToString("0.###", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"))}";
        }
    }
}
