using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MisWpf.Infrastructure;
using MisWpf.ViewModels;
using ЦРБ_и_все_все_все.Классы;
using static ЦРБ_и_все_все_все.Классы.Employee;

namespace MisWpf.Views
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _toastTimer = new() { Interval = TimeSpan.FromSeconds(4.5) };
        private readonly Employee _employee;
        private bool _loggingOut;
        private Action? _stop;

        public MainWindow(Employee employee)
        {
            InitializeComponent();
            _employee = employee;

            UserName.Text = ShortName(employee.FullName);
            Initials.Text = InitialsOf(employee.FullName);

            _toastTimer.Tick += (_, _) => { _toastTimer.Stop(); Toast.Visibility = Visibility.Collapsed; };
            Ui.ToastRequested += ShowToast;

            Loaded += async (_, _) => await OpenWorkspaceAsync();
            Closed += (_, _) =>
            {
                Ui.ToastRequested -= ShowToast;
                _stop?.Invoke();
                if (!_loggingOut) Application.Current.Shutdown();
            };
        }

        private async Task OpenWorkspaceAsync()
        {
            try
            {
                switch (_employee.Position)
                {
                    case PositionType.ReceptionDoctor:
                    {
                        SetRole("SidebarDoctor", "РЕЖИМ: ВРАЧ ПРИЁМНОГО", "Приёмное", "отделение");
                        var vm = new ReceptionDoctorViewModel();
                        vm.NewAdmissionRequested += () => OpenRegistrationWindow(vm);
                        vm.PreviewRequested += (title, protocol) => new ProtocolWindow(title, protocol) { Owner = this }.ShowDialog();
                        Host.Content = new ReceptionDoctorView { DataContext = vm };
                        AddNav("Очередь пациентов", true, null);
                        AddNav("Новое поступление", false, () => OpenRegistrationWindow(vm));
                        _stop = vm.Stop;
                        await vm.InitAsync();
                        break;
                    }
                    case PositionType.ReceptionNurse:
                    {
                        SetRole("SidebarNurse", "РЕЖИМ: МЕДСЕСТРА ПРИЁМНОГО", "Приёмное", "отделение");
                        var vm = new RegistrationViewModel();
                        Host.Content = new RegistrationView { DataContext = vm };
                        AddNav("Оформление поступления", true, null);
                        await vm.InitAsync();
                        break;
                    }
                    case PositionType.LabTechnician:
                    {
                        SetRole("SidebarLab", "РЕЖИМ: ЛАБОРАНТ", "Лаборатория", "клинико-диагностическая");
                        var vm = new LabTechnicianViewModel();
                        Host.Content = new LabTechnicianView { DataContext = vm };
                        AddNav("Лабораторные заказы", true, null);
                        _stop = vm.Stop;
                        await vm.InitAsync();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError("Не удалось загрузить рабочее место", ex);
            }
        }

        private void OpenRegistrationWindow(ReceptionDoctorViewModel doctorVm)
        {
            var regVm = new RegistrationViewModel();
            var win = new RegistrationWindow { Owner = this, DataContext = regVm };
            regVm.Registered += async _ => { win.Close(); await doctorVm.ReloadAsync(); };
            _ = regVm.InitAsync();
            win.ShowDialog();
        }

        private void SetRole(string brushKey, string mode, string title, string sub)
        {
            Sidebar.Background = (Brush)FindResource(brushKey);
            ModeText.Text = mode;
            DeptTitle.Text = title;
            DeptSub.Text = sub;
        }

        private void AddNav(string text, bool active, Action? click)
        {
            var b = new Button
            {
                Style = (Style)FindResource("NavButton"),
                Content = new TextBlock { Text = text, FontWeight = active ? FontWeights.SemiBold : FontWeights.Normal, Foreground = Brushes.White },
                Tag = active ? "active" : null,
                Margin = new Thickness(0, 0, 0, 2)
            };
            if (click != null) b.Click += (_, _) => click();
            NavPanel.Children.Add(b);
        }

        private void ShowToast(string text)
        {
            ToastText.Text = text;
            Toast.Visibility = Visibility.Visible;
            _toastTimer.Stop();
            _toastTimer.Start();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _loggingOut = true;
            _stop?.Invoke();
            Session.Employee = null;
            App.ShowLogin();
            Close();
        }

        private static string ShortName(string fio)
        {
            var p = (fio ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (p.Length == 0) return "";
            return p[0] + (p.Length > 1 ? " " + p[1][0] + "." : "") + (p.Length > 2 ? " " + p[2][0] + "." : "");
        }

        private static string InitialsOf(string fio)
        {
            var p = (fio ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(p.Take(2).Select(s => char.ToUpper(s[0])));
        }
    }
}
