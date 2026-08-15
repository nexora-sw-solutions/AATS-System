using Avalonia.Controls;
using AATS.Desktop.ViewModels;
using System.Linq;

namespace AATS.Desktop.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private void Calendar_OnSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is Calendar calendar && DataContext is DashboardViewModel vm)
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
