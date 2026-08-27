using Avalonia.Controls;
using System.Linq;
using AATS.Desktop.ViewModels;

namespace AATS.Desktop.Views
{
    public partial class OutstandingBalancesView : UserControl
    {
        public OutstandingBalancesView()
        {
            InitializeComponent();
        }

        private void Calendar_OnSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is Calendar calendar && DataContext is OutstandingBalancesViewModel viewModel)
            {
                var dates = calendar.SelectedDates.OrderBy(d => d).ToList();
                if (dates.Any())
                {
                    viewModel.PeriodStartDate = dates.First();
                    viewModel.PeriodEndDate = dates.Last();
                }
            }
        }
    }
}
