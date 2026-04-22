using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isPasswordVisible;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _hasLoginError;

        [ObservableProperty]
        private string _loginErrorMessage = string.Empty;

        public char PasswordChar => IsPasswordVisible ? '\0' : '•';

        // Reset Request Form Properties
        [ObservableProperty] private bool _isResetModalVisible;
        [ObservableProperty] private bool _isResetSubmitConfirmVisible;
        [ObservableProperty] private bool _isResetCancelConfirmVisible;

        [ObservableProperty] private string _resetUsername = string.Empty;
        [ObservableProperty] private string _resetEmail = string.Empty;
        [ObservableProperty] private string _resetPhone = string.Empty;
        [ObservableProperty] private string _resetLastPassword = string.Empty;

        public ObservableCollection<string> Roles { get; } = new() { "Admin", "Manager", "Staff", "Auditor" };
        [ObservableProperty] private string? _selectedRole;

        public ObservableCollection<string> Branches { get; } = new() { "Central", "South", "West", "Northeast" };
        [ObservableProperty] private string? _selectedBranch;

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
            OnPropertyChanged(nameof(PasswordChar));
        }

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                HasLoginError = true;
                LoginErrorMessage = "Enter your username and password to continue.";
                return;
            }

            HasLoginError = false;
            LoginErrorMessage = string.Empty;
            IsBusy = true;

            try
            {
                var success = await DataService.Instance.LoginAsync(Username, Password);
                if (!success)
                {
                    HasLoginError = true;
                    LoginErrorMessage = "Login failed. Check the username, password, or backend connection.";
                    return;
                }

                if (Application.Current is App app)
                {
                    app.SwitchToMainWindow();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ShowResetModal()
        {
            // Clear form
            ResetUsername = string.Empty;
            ResetEmail = string.Empty;
            ResetPhone = string.Empty;
            ResetLastPassword = string.Empty;
            SelectedRole = null;
            SelectedBranch = null;

            IsResetModalVisible = true;
        }

        [RelayCommand]
        private void RequestReset()
        {
            // Show confirmation before submitting
            IsResetSubmitConfirmVisible = true;
        }

        [RelayCommand]
        private async Task ConfirmResetSubmit()
        {
            await DataService.Instance.RequestPasswordResetAsync(new PasswordResetRequest
            {
                Username = ResetUsername,
                Email = ResetEmail,
                Phone = ResetPhone,
                LastPassword = ResetLastPassword,
                Role = SelectedRole,
                Branch = SelectedBranch
            });

            IsResetSubmitConfirmVisible = false;
            IsResetModalVisible = false;
        }

        [RelayCommand]
        private void CancelResetSubmit()
        {
            // Just close the submit confirmation, keep form open
            IsResetSubmitConfirmVisible = false;
        }

        [RelayCommand]
        private void CancelReset()
        {
            // Show cancel confirmation to avoid accidental loss of filled data
            IsResetCancelConfirmVisible = true;
        }

        [RelayCommand]
        private void ConfirmResetCancel()
        {
            IsResetCancelConfirmVisible = false;
            IsResetModalVisible = false;
        }

        [RelayCommand]
        private void BackToReset()
        {
            IsResetCancelConfirmVisible = false;
        }
    }
}
