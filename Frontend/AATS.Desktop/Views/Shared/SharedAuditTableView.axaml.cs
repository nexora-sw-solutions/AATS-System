using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AATS.Desktop.Views.Shared
{
    public partial class SharedAuditTableView : UserControl
    {
        public SharedAuditTableView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
