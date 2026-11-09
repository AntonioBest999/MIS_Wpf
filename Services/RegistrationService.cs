using Microsoft.EntityFrameworkCore;
using MisWpf.Infrastructure;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.Services
{
    public class RegistrationRequest
    {
        public Patient? ExistingPatient { get; init; }
        public Patient? NewPatient { get; init; }
        public PatientContact? Contact { get; init; }

        public AdmissionSourceType Source { get; init; }
        public string? SourceDetails { get; init; }
        public string? DeliveredBy { get; init; }
        public string? SheetNumber { get; init; }
        public string? Complaint { get; init; }
        public string? SmpDiagnosis { get; init; }
        public string? ProvidedCare { get; init; }

        public List<string> LabCodes { get; init; } = new();
        public List<AttachmentType> Instrumental { get; init; } = new();
    }

    public class TodayAdmission
    {
        public int Id { get; init; }
        public string Fio { get; init; } = "";
        public DateTime Time { get; init; }
        public string Status { get; init; } = "";
    }

    public static class RegistrationService
    {
        public static async Task<List<Patient>> SearchAsync(string text)
        {
            text = (text ?? "").Trim();
            if (text.Length < 2) return new List<Patient>();

            await using var db = Db.Create();
            var q = db.Patients.AsNoTracking();

            if (DateTime.TryParse(text, System.Globalization.CultureInfo.GetCultureInfo("ru-RU"), System.Globalization.DateTimeStyles.None, out var dt))
            {
                var d = DateOnly.FromDateTime(dt);
                q = q.Where(p => p.BirthDate == d);
            }
            else
            {
                string pattern = "%" + text.Replace("%", "").Replace("_", "") + "%";
                q = q.Where(p => EF.Functions.ILike(p.FullName, pattern)
                              || (p.PhoneNumber != null && EF.Functions.ILike(p.PhoneNumber, pattern))
                              || (p.PassportNumber != null && EF.Functions.ILike(p.PassportNumber, pattern)));
            }

            return await q.OrderBy(p => p.FullName).Take(40).ToListAsync();
        }

        /// <summary>Создаёт (при необходимости) пациента, поступление и назначения — одной транзакцией.</summary>
        public static async Task<int> RegisterAsync(RegistrationRequest r)
        {
            await using var db = Db.Create();
            await using var tx = await db.Database.BeginTransactionAsync();

            string patientId;
            if (r.NewPatient != null)
            {
                db.Patients.Add(r.NewPatient);
                await db.SaveChangesAsync();
                patientId = r.NewPatient.PatientId;

                if (r.Contact != null && !string.IsNullOrWhiteSpace(r.Contact.FullName))
                {
                    r.Contact.PatientId = patientId;
                    db.Set<PatientContact>().Add(r.Contact);
                }
            }
            else if (r.ExistingPatient != null)
            {
                patientId = r.ExistingPatient.PatientId;
            }
            else throw new InvalidOperationException("Не выбран пациент.");

            var admission = new Admission
            {
                PatientId = patientId,
                AdmissionDateTime = DateTime.UtcNow,
                SourceType = r.Source,
                SourceDetails = Clip(r.SourceDetails, 500),
                DeliveredBy = Clip(r.DeliveredBy, 300),
                AccompanyingSheetNumber = Clip(r.SheetNumber, 50),
                Complaint = Clip(r.Complaint, 1000),
                DiagnosisOnAdmission = Clip(r.SmpDiagnosis, 300),
                ProvidedCare = Clip(r.ProvidedCare, 500),
                IsActive = true,
                Status = "Поступил"
            };
            db.Set<Admission>().Add(admission);
            await db.SaveChangesAsync();

            if (r.LabCodes.Count > 0)
                await OrderService.CreateLabOrderAsync(db, patientId, r.LabCodes);
            foreach (var t in r.Instrumental.Distinct())
                OrderService.CreateInstrumental(db, patientId, admission.Id, t);

            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return admission.Id;
        }

        public static async Task<List<TodayAdmission>> LoadShiftAdmissionsAsync()
        {
            var since = Session.ShiftStartUtc;
            await using var db = Db.Create();
            return await db.Set<Admission>()
                .AsNoTracking()
                .Where(a => a.AdmissionDateTime >= since)
                .OrderByDescending(a => a.AdmissionDateTime)
                .Select(a => new TodayAdmission { Id = a.Id, Fio = a.Patient.FullName, Time = a.AdmissionDateTime, Status = a.Status })
                .ToListAsync();
        }

        private static string? Clip(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return s.Length <= max ? s : s[..max];
        }
    }
}
