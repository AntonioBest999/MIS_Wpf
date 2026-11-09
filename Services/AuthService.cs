using Microsoft.EntityFrameworkCore;
using MisWpf.Infrastructure;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.Services
{
    public static class AuthService
    {
        /// <summary>Вход по логину и паролю. Пароль проверяется так же, как в WinForms-приложении (BCrypt).</summary>
        public static async Task<Employee?> LoginAsync(string login, string password)
        {
            await using var db = Db.Create();
            var emp = await db.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.User != null && e.User.Login == login);

            if (emp?.User == null || string.IsNullOrEmpty(emp.User.PasswordHash)) return null;

            bool ok;
            try { ok = BCrypt.Net.BCrypt.Verify(password, emp.User.PasswordHash); }
            catch { ok = false; }
            return ok ? emp : null;
        }
    }
}
