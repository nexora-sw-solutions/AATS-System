using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AATS.Desktop.ViewModels
{
    public partial class MainShellViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentView;

        public MainShellViewModel()
        {
            // Start with Login
            _currentView = new LoginViewModel();
        }

        [RelayCommand]
        public void ShowDashboard()
        {
            CurrentView = new MainViewModel();
        }

        [RelayCommand]
        public void ShowLogin()
        {
            CurrentView = new LoginViewModel();
        }
    }
}
