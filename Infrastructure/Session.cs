using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.Infrastructure
{
    /// <summary>Текущий вошедший сотрудник.</summary>
    public static class Session
    {
        public static Employee? Employee { get; set; }

        public static string EmployeeId => Employee?.EmployeeId ?? "";
        public static string EmployeeName => Employee?.FullName ?? "";

        /// <summary>Начало текущих дежурных суток (с 8:00 до 8:00), в UTC.</summary>
        public static DateTime ShiftStartUtc
        {
            get
            {
                var now = DateTime.Now;
                var start = now.Date.AddHours(8);
                if (now < start) start = start.AddDays(-1);
                return start.ToUniversalTime();
            }
        }
    }
}
