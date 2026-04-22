using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AATS.Desktop.Views.Shared
{
    public partial class SharedTaxTableView : UserControl
    {
        public SharedTaxTableView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
