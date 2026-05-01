using Avalonia.Controls;
using System.Linq;
using AATS.Desktop.ViewModels.Clients;

namespace AATS.Desktop.Views.Clients;

public partial class ClientsView : UserControl
{
    public ClientsView()
    {
        InitializeComponent();
    }

    private void Calendar_OnSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is Calendar calendar && DataContext is ClientsViewModel viewModel)
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
