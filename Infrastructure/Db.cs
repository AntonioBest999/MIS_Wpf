using Microsoft.EntityFrameworkCore;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.Infrastructure
{
    /// <summary>
    /// Подключение к той же базе, что и у WinForms-приложения.
    /// Модель (папка Model) скопирована из WinForms-проекта без изменений,
    /// миграции здесь НЕ создаются и НЕ применяются: схемой по-прежнему управляет WinForms-проект.
    /// </summary>
    public static class Db
    {
        private static DbContextOptions<MisDbContext>? _options;

        public static void Init(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<MisDbContext>();
            builder.UseNpgsql(connectionString);
            _options = builder.Options;
        }

        public static MisDbContext Create()
        {
            if (_options == null) throw new InvalidOperationException("База данных не инициализирована.");
            return new MisDbContext(_options);
        }
    }
}
