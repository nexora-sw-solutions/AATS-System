using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AATS.Desktop.ViewModels.Shared;
using System.Linq;

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

        private void Calendar_OnSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is Calendar calendar && DataContext is TaxTableViewModelBase vm)
            {
                var dates = calendar.SelectedDates.OrderBy(d => d).ToList();
                if (dates.Any())
                {
                    vm.PeriodStartDate = dates.First();
                    vm.PeriodEndDate = dates.Last();
                }
            }
        }
    }
}
