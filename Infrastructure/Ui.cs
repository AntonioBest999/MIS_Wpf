using System.Windows;

namespace MisWpf.Infrastructure
{
    public static class Ui
    {
        /// <summary>Короткое уведомление внизу окна. Окно-оболочка подписывается на это событие.</summary>
        public static event Action<string>? ToastRequested;

        public static void Toast(string message) => ToastRequested?.Invoke(message);

        public static void ShowError(string title, Exception ex)
        {
            string text = ex.Message;
            if (ex.InnerException != null) text += "\n\n" + ex.InnerException.Message;
            if (ex.InnerException?.InnerException != null) text += "\n\n" + ex.InnerException.InnerException.Message;
            MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool Confirm(string text, string title = "Подтверждение")
            => MessageBox.Show(text, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        public static void Info(string text, string title = "Информация")
            => MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
