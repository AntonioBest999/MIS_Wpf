using System.Windows;
using MisWpf.Infrastructure;
using MisWpf.Services;
using static ЦРБ_и_все_все_все.Классы.Employee;

namespace MisWpf.Views
{
    public partial class LoginWindow : Window
    {
        private bool _loggedIn;

        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => LoginBox.Focus();
            Closed += (_, _) => { if (!_loggedIn) Application.Current.Shutdown(); };
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            if (login.Length == 0 || password.Length == 0)
            {
                ShowError("Введите логин и пароль.");
                return;
            }

            LoginButton.IsEnabled = false;
            try
            {
                var emp = await AuthService.LoginAsync(login, password);
                if (emp == null)
                {
                    ShowError("Неверный логин или пароль.");
                    return;
                }

                if (emp.Position != PositionType.ReceptionDoctor
                    && emp.Position != PositionType.ReceptionNurse
                    && emp.Position != PositionType.LabTechnician)
                {
                    ShowError("В облегчённой версии пока есть рабочие места врача и медсестры приёмного отделения и лаборанта.");
                    return;
                }

                Session.Employee = emp;
                _loggedIn = true;
                new MainWindow(emp).Show();
                Close();
            }
            catch (Exception ex)
            {
                ShowError("Нет связи с базой данных: " + ex.Message);
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }

        private void ShowError(string text)
        {
            ErrorText.Text = text;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
