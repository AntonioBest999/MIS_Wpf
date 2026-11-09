using System.Windows;

namespace MisWpf.Views
{
    public partial class ProtocolWindow : Window
    {
        private readonly List<(string Title, string Text)> _protocol;

        public ProtocolWindow(string subtitle, List<(string Title, string Text)> protocol)
        {
            InitializeComponent();
            _protocol = protocol;
            Subtitle.Text = subtitle;
            // ValueTuple отдаёт поля как Item1/Item2 — по ним и привязка в разметке
            Items.ItemsSource = protocol.Select(p => Tuple.Create(p.Title, p.Text)).ToList();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(string.Join(Environment.NewLine, _protocol.Select(p => $"{p.Title}. {p.Text}")));
        }
    }
}
