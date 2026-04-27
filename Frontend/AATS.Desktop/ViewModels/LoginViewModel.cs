using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using AATS.Desktop.Services;
using AATS.Desktop.Models;

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
        private string _errorMessage = string.Empty;

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
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both email and password.";
                return;
            }

            try
            {
                var req = new LoginRequest { Email = Username, Password = Password };
                var response = await ApiService.Instance.PostAsync<LoginRequest, ApiResponse<LoginResponse>>("/api/v1/auth/login", req);

                if (response?.Success == true && !string.IsNullOrEmpty(response.Data?.Token))
                {
                    ApiService.Instance.SetToken(response.Data.Token);
                    if (Application.Current is App app)
                    {
                        NotificationService.Instance.AddNotification(Username, "logged in");
                        app.SwitchToMainWindow();
                    }
                }
                else if (response != null && response.Error != null)
                {
                    // Remember: the backend mapped exception.Message into the Code field, and errorCode into the Message field.
                    ErrorMessage = !string.IsNullOrEmpty(response.Error.Code) && response.Error.Code != "ERROR" 
                        ? response.Error.Code 
                        : "Invalid credentials. Please try again.";
                }
                else
                {
                    ErrorMessage = "Invalid credentials. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Connection error: {ex.Message}";
                Console.WriteLine($"[DEBUG] Login exception: {ex.ToString()}");
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
        private void ConfirmResetSubmit()
        {
            // Simulate sending request
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
