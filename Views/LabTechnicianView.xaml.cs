using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MisWpf.Views
{
    public partial class LabTechnicianView : UserControl
    {
        public LabTechnicianView()
        {
            InitializeComponent();
            // Enter в поле результата — переход к следующему полю
            AddHandler(PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), true);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (e.OriginalSource is TextBox tb && !tb.AcceptsReturn)
            {
                tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }
    }
}
