using System.Windows;
using System.Windows.Threading;
using MisWpf.Infrastructure;
using MisWpf.Views;

namespace MisWpf
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += OnDispatcherUnhandledException;

            try
            {
                AppConfig.Load();
                Db.Init(AppConfig.ConnectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось прочитать appsettings.json:\n" + ex.Message, "Ошибка запуска",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            ShowLogin();
        }

        public static void ShowLogin()
        {
            var login = new LoginWindow();
            login.Show();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Ui.ShowError("Непредвиденная ошибка", e.Exception);
            e.Handled = true;
        }
    }
}
