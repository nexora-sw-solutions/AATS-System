using Avalonia.Controls;
using AATS.Desktop.ViewModels;
using System;

namespace AATS.Desktop.Views;

public partial class MainShellView : UserControl
{
    private MainViewModel? _viewModel;

    public MainShellView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        
        _viewModel = DataContext as MainViewModel;
        
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentView))
        {
            var notificationButton = this.FindControl<Button>("NotificationButton");
            if (notificationButton != null && notificationButton.Flyout != null)
            {
                notificationButton.Flyout.Hide();
            }
        }
    }

    private void SearchInput_OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Down)
        {
            var resultsList = this.FindControl<ListBox>("SearchResultsList");
            if (resultsList != null && resultsList.IsVisible && resultsList.ItemCount > 0)
            {
                resultsList.Focus();
                resultsList.SelectedIndex = 0;
                e.Handled = true;
            }
        }
    }

    private void SearchResultsList_OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            if (DataContext is MainViewModel vm && vm.SelectedSearchItem != null)
            {
                vm.NavigateToModuleCommand.Execute(vm.SelectedSearchItem);
                e.Handled = true;
            }
        }
        else if (e.Key == Avalonia.Input.Key.Escape)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.IsSearchDropdownOpen = false;
                var searchInput = this.FindControl<TextBox>("SearchInput");
                searchInput?.Focus();
                e.Handled = true;
            }
        }
    }
}
