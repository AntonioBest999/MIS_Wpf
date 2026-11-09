using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.Infrastructure
{
    public static class Format
    {
        public static string Age(Patient p)
        {
            if (p.IsUnknown || p.BirthDate == DateOnly.MinValue)
                return string.IsNullOrWhiteSpace(p.ApproximateAge) ? "возраст неизвестен" : "≈ " + p.ApproximateAge;
            var today = DateOnly.FromDateTime(DateTime.Today);
            int age = today.Year - p.BirthDate.Year;
            if (p.BirthDate > today.AddYears(-age)) age--;
            return age + " " + Years(age);
        }

        public static string Years(int n)
        {
            int n10 = n % 10, n100 = n % 100;
            if (n10 == 1 && n100 != 11) return "год";
            if (n10 >= 2 && n10 <= 4 && (n100 < 10 || n100 >= 20)) return "года";
            return "лет";
        }

        public static string Sex(string? gender)
        {
            var g = (gender ?? "").Trim().ToLowerInvariant();
            if (g.StartsWith("м") || g == "male" || g == "m") return "муж.";
            if (g.StartsWith("ж") || g == "female" || g == "f") return "жен.";
            return "пол не указан";
        }

        public static bool IsFemale(string? gender)
        {
            var g = (gender ?? "").Trim().ToLowerInvariant();
            return g.StartsWith("ж") || g == "female" || g == "f";
        }

        /// <summary>Номер поступления. Отдельного поля в БД пока нет, поэтому показываем Id записи.</summary>
        public static string AdmissionNumber(int admissionId) => "П-" + admissionId;

        public static string Elapsed(TimeSpan t)
        {
            if (t < TimeSpan.Zero) t = TimeSpan.Zero;
            return $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}";
        }

        public static string Source(AdmissionSourceType s) => s switch
        {
            AdmissionSourceType.SMP => "СМП",
            AdmissionSourceType.SelfAppeal => "Самообращение",
            AdmissionSourceType.Transfer => "Перевод",
            _ => "Другое"
        };

        public static string SourceLong(AdmissionSourceType s) => s switch
        {
            AdmissionSourceType.SMP => "Доставлен бригадой СМП",
            AdmissionSourceType.SelfAppeal => "Самообращение",
            AdmissionSourceType.Transfer => "Переведён из другого учреждения",
            _ => "Другое"
        };

        public static string Local(DateTime utc) =>
            (utc.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(utc, DateTimeKind.Utc) : utc).ToLocalTime().ToString("HH:mm");

        public static DateTime AsUtc(DateTime dt) => dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }
}
