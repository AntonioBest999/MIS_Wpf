using Microsoft.EntityFrameworkCore;
using MisWpf.Infrastructure;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.Services
{
    /// <summary>Набор лабораторных показателей, который назначается одной кнопкой.</summary>
    public class LabProfile
    {
        public string Name { get; init; } = "";
        public string[] Codes { get; init; } = Array.Empty<string>();
    }

    public class InstrumentalKind
    {
        public string Name { get; init; } = "";
        public AttachmentType Type { get; init; }
    }

    public static class OrderService
    {
        /// <summary>Профили по кодам показателей из таблицы LabParameters.</summary>
        public static readonly LabProfile[] Profiles =
        {
            new() { Name = "ОАК", Codes = new[] { "HGB", "RBC", "CP", "HCT", "PLT", "WBC", "ESR", "NEUT_P", "NEUT_S", "EOS", "LYM", "MON", "BASO" } },
            new() { Name = "ОАМ", Codes = new[] { "VOL", "COLOR", "TRANS", "SG", "PH", "PROT", "GLU", "ACET", "EPITH_F", "WBC_U", "RBC_U", "CYL_H", "BACT" } },
            new() { Name = "Глюкоза", Codes = new[] { "GLI" } },
            new() { Name = "Тропонин", Codes = new[] { "TROP" } },
            new() { Name = "Биохимия", Codes = new[] { "GLI", "UREA", "CREA", "BIL_T", "BIL_D", "ALT", "AST", "TP", "ALB" } },
            new() { Name = "Коагулограмма", Codes = new[] { "PT", "INR", "FIB" } },
        };

        public static readonly InstrumentalKind[] Instrumental =
        {
            new() { Name = "ЭКГ", Type = AttachmentType.ECG },
            new() { Name = "Рентген ОГК", Type = AttachmentType.ChestXRay },
            new() { Name = "УЗИ", Type = AttachmentType.Ultrasound },
            new() { Name = "КТ", Type = AttachmentType.CT },
        };

        public static string InstrumentalName(AttachmentType t) => t switch
        {
            AttachmentType.ECG => "ЭКГ",
            AttachmentType.ChestXRay => "Рентген ОГК",
            AttachmentType.Ultrasound => "УЗИ",
            AttachmentType.CT => "КТ",
            AttachmentType.MRI => "МРТ",
            _ => "Исследование"
        };

        /// <summary>Пометка, по которой отличаем направление (без файла) от загруженного снимка.</summary>
        public const string DirectionMark = "(направление)";

        /// <summary>Создаёт заказ в лабораторию. Возвращает null, если ни один код не найден в справочнике.</summary>
        public static async Task<LabOrder?> CreateLabOrderAsync(MisDbContext db, string patientId, IEnumerable<string> codes, int? hospitalizationId = null)
        {
            var codeList = codes.Distinct().ToList();
            var ids = await db.LabParameters
                .Where(p => p.IsActive && codeList.Contains(p.Code))
                .Select(p => p.LabParameterId)
                .ToListAsync();
            if (ids.Count == 0) return null;

            var order = new LabOrder
            {
                PatientId = patientId,
                HospitalizationId = hospitalizationId,
                OrderedById = Session.EmployeeId,
                OrderedAt = DateTime.UtcNow,
                Status = LabOrderStatus.Ordered
            };
            foreach (var id in ids) order.Items.Add(new LabOrderItem { LabParameterId = id });
            db.LabOrders.Add(order);
            return order;
        }

        /// <summary>
        /// Направление на инструментальное исследование. Отдельной таблицы для направлений в БД нет,
        /// поэтому создаём запись Attachments без файла со статусом Draft («назначено»).
        /// Когда исследование выполнено — статус Finalized и заключение в InterpretationText.
        /// </summary>
        public static Attachment CreateInstrumental(MisDbContext db, string patientId, int admissionId, AttachmentType type)
        {
            var a = new Attachment
            {
                PatientId = patientId,
                AdmissionId = admissionId,
                DocumentType = type,
                OriginalFileName = DirectionMark,
                StoredFileName = "",
                RelativePath = "",
                FileSizeBytes = 0,
                Description = "Направление: " + InstrumentalName(type),
                Status = AttachmentStatus.Draft,
                UploadedAt = DateTime.UtcNow
            };
            db.Attachments.Add(a);
            return a;
        }

        public static async Task SendLabAsync(string patientId, IEnumerable<string> codes)
        {
            await using var db = Db.Create();
            var order = await CreateLabOrderAsync(db, patientId, codes);
            if (order == null) throw new InvalidOperationException("Выбранных показателей нет в справочнике LabParameters.");
            await db.SaveChangesAsync();
        }

        public static async Task SendInstrumentalAsync(string patientId, int admissionId, AttachmentType type)
        {
            await using var db = Db.Create();
            CreateInstrumental(db, patientId, admissionId, type);
            await db.SaveChangesAsync();
        }

        /// <summary>Отметить инструментальное исследование выполненным (пока у ЭКГ/рентгена нет своего рабочего места).</summary>
        public static async Task CompleteInstrumentalAsync(int attachmentId, string conclusion)
        {
            await using var db = Db.Create();
            var a = await db.Attachments.FirstOrDefaultAsync(x => x.Id == attachmentId)
                    ?? throw new InvalidOperationException("Исследование не найдено.");
            a.InterpretationText = conclusion.Trim();
            a.InterpretationDate = DateTime.UtcNow;
            a.InterpretingDoctorId = Session.EmployeeId;
            a.Status = AttachmentStatus.Finalized;
            await db.SaveChangesAsync();
        }

        public static bool IsInstrumentalDone(Attachment a)
            => a.Status == AttachmentStatus.Finalized || !string.IsNullOrEmpty(a.StoredFileName);
    }
}
