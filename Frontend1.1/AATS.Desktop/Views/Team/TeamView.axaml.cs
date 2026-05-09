using Avalonia.Controls;
using System.Linq;
using AATS.Desktop.ViewModels.Team;

namespace AATS.Desktop.Views.Team;

public partial class TeamView : UserControl
{
    public TeamView()
    {
        InitializeComponent();
    }

    private void Calendar_OnSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is Calendar calendar && DataContext is TeamViewModel vm)
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
