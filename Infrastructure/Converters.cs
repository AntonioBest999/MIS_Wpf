using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MisWpf.Infrastructure
{
    /// <summary>true → Visible. С параметром "invert" — наоборот. Строка/коллекция/объект: непустое → Visible.</summary>
    public class VisibleIfConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool v = value switch
            {
                null => false,
                bool b => b,
                string s => !string.IsNullOrWhiteSpace(s),
                int i => i != 0,
                System.Collections.ICollection c => c.Count > 0,
                _ => true
            };
            if (parameter is string p && p == "invert") v = !v;
            return v ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
