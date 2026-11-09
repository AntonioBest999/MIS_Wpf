using Microsoft.EntityFrameworkCore;
using MisWpf.Exam;
using MisWpf.Infrastructure;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.Services
{
    public static class InspectionService
    {
        /// <summary>
        /// Сохраняет первичный осмотр и решение по поступлению.
        /// Возвращает текст для уведомления.
        /// </summary>
        public static async Task<string> SaveDecisionAsync(ExamState exam)
        {
            if (exam.Decision == DecisionKind.None || exam.Decision == DecisionKind.Observe)
                throw new InvalidOperationException("Не выбрано решение.");

            await using var db = Db.Create();
            await using var tx = await db.Database.BeginTransactionAsync();

            var admission = await db.Set<Admission>().Include(a => a.Patient).FirstAsync(a => a.Id == exam.AdmissionId);

            // 1. Первичный осмотр и его разделы
            var inspection = ExamMapper.BuildInspection(exam, admission, Session.EmployeeId);
            db.PrimaryInspections.Add(inspection);
            await db.SaveChangesAsync();

            var general = ExamMapper.General(exam); general.PrimaryInspectionId = inspection.Id;
            var skin = ExamMapper.Skin(exam); skin.PrimaryInspectionId = inspection.Id;
            var resp = ExamMapper.Respiratory(exam); resp.PrimaryInspectionId = inspection.Id;
            var cardio = ExamMapper.Cardiology(exam); cardio.PrimaryInspectionId = inspection.Id;
            var abdomen = ExamMapper.Abdomen(exam); abdomen.PrimaryInspectionId = inspection.Id;
            var neuro = ExamMapper.Neuro(exam); neuro.PrimaryInspectionId = inspection.Id;
            var anam = ExamMapper.AnamnesisVitae(exam); anam.PrimaryInspectionId = inspection.Id;
            db.GeneralData.Add(general);
            db.SkinData.Add(skin);
            db.RespiratoryData.Add(resp);
            db.CardiologyData.Add(cardio);
            db.AbdomenData.Add(abdomen);
            db.NeuroData.Add(neuro);
            db.AnamnesisVitaeData.Add(anam);

            // 2. Привязываем к осмотру лабораторные заказы этого поступления
            var since = Format.AsUtc(admission.AdmissionDateTime).AddMinutes(-10);
            var orders = await db.LabOrders
                .Where(o => o.PatientId == admission.PatientId && o.OrderedAt >= since && o.PrimaryInspectionId == null)
                .ToListAsync();
            foreach (var o in orders) o.PrimaryInspectionId = inspection.Id;

            // 3. Решение
            string message;
            switch (exam.Decision)
            {
                case DecisionKind.Hospitalize:
                    if (exam.DepartmentId == null) throw new InvalidOperationException("Выберите отделение.");
                    var hosp = new Hospitalization
                    {
                        PatientId = admission.PatientId,
                        DepartmentId = exam.DepartmentId.Value,
                        MedicalRecordNumber = await NextRecordNumberAsync(db),
                        AdmissionDate = DateTime.UtcNow,
                        AdmissionDiagnosis = Clip(string.IsNullOrWhiteSpace(exam.Diagnosis) ? admission.DiagnosisOnAdmission : exam.Diagnosis, 250)
                                             ?? "Предварительный диагноз не указан",
                        SourceType = Format.Source(admission.SourceType),
                        SourceDetails = admission.SourceDetails,
                        DeliveredBy = Clip(admission.DeliveredBy, 200),
                        AccompanyingSheetNumber = admission.AccompanyingSheetNumber,
                        PrimaryInspectionId = inspection.Id,
                        Status = QueueService.HospActive
                    };
                    db.Hospitalizations.Add(hosp);
                    admission.Status = QueueService.StatusSentToDepartment;
                    await db.SaveChangesAsync();
                    foreach (var o in orders) o.HospitalizationId = hosp.Id;
                    var deptName = await db.Departments.Where(d => d.DepartmentId == hosp.DepartmentId).Select(d => d.Name).FirstOrDefaultAsync();
                    message = $"{admission.Patient.FullName}: история болезни № {hosp.MedicalRecordNumber}, направлен(а) в {QueueService.ShortDept(deptName)}. Ждёт решения заведующего.";
                    break;

                case DecisionKind.NotNeeded:
                    admission.Status = QueueService.StatusRefused;
                    admission.IsActive = false;
                    message = $"{admission.Patient.FullName}: оформлен отказ в госпитализации.";
                    break;

                case DecisionKind.TransferOut:
                    if (string.IsNullOrWhiteSpace(exam.TransferTo)) throw new InvalidOperationException("Укажите учреждение, куда переводится пациент.");
                    admission.Status = QueueService.StatusTransferredOut;
                    admission.IsActive = false;
                    message = $"{admission.Patient.FullName}: оформлен перевод в {exam.TransferTo.Trim()}.";
                    break;

                default:
                    throw new InvalidOperationException("Неизвестное решение.");
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return message;
        }

        /// <summary>
        /// Пациент, которого вернул заведующий: направить в другое отделение (та же история болезни)
        /// или оформить отказ.
        /// </summary>
        public static async Task<string> ResolveReturnedAsync(ExamState exam, int rejectedHospitalizationId)
        {
            await using var db = Db.Create();
            await using var tx = await db.Database.BeginTransactionAsync();

            var admission = await db.Set<Admission>().Include(a => a.Patient).FirstAsync(a => a.Id == exam.AdmissionId);
            var hosp = await db.Hospitalizations.FirstAsync(h => h.Id == rejectedHospitalizationId);
            string message;

            switch (exam.Decision)
            {
                case DecisionKind.Hospitalize:
                    if (exam.DepartmentId == null) throw new InvalidOperationException("Выберите отделение.");
                    if (exam.DepartmentId == hosp.DepartmentId) throw new InvalidOperationException("Это отделение уже отказало. Выберите другое.");
                    hosp.DepartmentId = exam.DepartmentId.Value;
                    hosp.Status = QueueService.HospActive;
                    if (!string.IsNullOrWhiteSpace(exam.Diagnosis)) hosp.AdmissionDiagnosis = Clip(exam.Diagnosis, 250);
                    admission.Status = QueueService.StatusSentToDepartment;
                    var deptName = await db.Departments.Where(d => d.DepartmentId == exam.DepartmentId).Select(d => d.Name).FirstOrDefaultAsync();
                    message = $"{admission.Patient.FullName}: направлен(а) в {QueueService.ShortDept(deptName)}, история болезни № {hosp.MedicalRecordNumber}.";
                    break;

                case DecisionKind.NotNeeded:
                case DecisionKind.TransferOut:
                    hosp.Status = "Отменена";
                    hosp.DischargeDate = DateTime.UtcNow;
                    admission.Status = exam.Decision == DecisionKind.NotNeeded ? QueueService.StatusRefused : QueueService.StatusTransferredOut;
                    admission.IsActive = false;
                    message = $"{admission.Patient.FullName}: госпитализация отменена, " +
                              (exam.Decision == DecisionKind.NotNeeded ? "оформлен отказ." : $"перевод в {exam.TransferTo.Trim()}.");
                    break;

                default:
                    throw new InvalidOperationException("Не выбрано решение.");
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return message;
        }

        /// <summary>Номер истории болезни в том же формате, что и в WinForms-версии: "N:ГГГГ".</summary>
        private static async Task<string> NextRecordNumberAsync(MisDbContext db)
        {
            int year = DateTime.UtcNow.Year;
            string suffix = ":" + year;
            var numbers = await db.Hospitalizations
                .IgnoreQueryFilters()
                .Where(h => h.MedicalRecordNumber.EndsWith(suffix))
                .Select(h => h.MedicalRecordNumber)
                .ToListAsync();

            int next = 1;
            foreach (var n in numbers)
            {
                var head = n.Split(':')[0];
                if (int.TryParse(head, out int v) && v >= next) next = v + 1;
            }
            return next + suffix;
        }

        private static string? Clip(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return s.Length <= max ? s : s[..max];
        }
    }
}
