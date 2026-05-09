using Avalonia.Controls;
using AATS.Desktop.ViewModels.ActivityLog;
using System.Linq;

namespace AATS.Desktop.Views.ActivityLog;

public partial class ActivityLogView : UserControl
{
    public ActivityLogView()
    {
        InitializeComponent();
    }

    private void Calendar_OnSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is Calendar calendar && DataContext is ActivityLogViewModel vm)
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
