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
        [ObservableProperty] private string _resetErrorMessage = string.Empty;
        [ObservableProperty] private string _successMessage = string.Empty;
        [ObservableProperty] private bool _isResetSubmitting;

        [ObservableProperty] private string _resetUsername = string.Empty;
        [ObservableProperty] private string _resetEmail = string.Empty;
        [ObservableProperty] private string _resetPhone = string.Empty;
        [ObservableProperty] private string _resetLastPassword = string.Empty;

        public ObservableCollection<string> Roles { get; } = new() { "Admin", "Audit and Assurance", "Secretarial and Advisory", "Tax Filing", "All" };
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
            SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both email/username and password.";
                return;
            }

            try
            {
                var req = new LoginRequest { Email = Username.Trim(), Password = Password };
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
                    ErrorMessage = response.Error.Message;
                }
                else
                {
                    ErrorMessage = "Invalid credentials";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Login exception: {ex}");
                var msg = ex.Message;
                if (msg.Contains("Server error (401)"))
                {
                    ErrorMessage = "Invalid username or password.";
                }
                else
                {
                    ErrorMessage = msg.Length < 100 ? msg : "Unable to connect to backend API server.";
                }
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
            ErrorMessage = string.Empty;
            ResetErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

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
            IsResetSubmitConfirmVisible = false;
            ResetErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(ResetUsername) ||
                string.IsNullOrWhiteSpace(ResetEmail) ||
                string.IsNullOrWhiteSpace(ResetPhone) ||
                string.IsNullOrWhiteSpace(ResetLastPassword) ||
                string.IsNullOrWhiteSpace(SelectedRole) ||
                string.IsNullOrWhiteSpace(SelectedBranch))
            {
                ResetErrorMessage = "Please fill in all details for the reset request.";
                return;
            }

            try
            {
                IsResetSubmitting = true;

                var req = new ForgotPasswordRequest
                {
                    Username = ResetUsername,
                    Email = ResetEmail,
                    Phone = ResetPhone,
                    Role = SelectedRole,
                    Branch = SelectedBranch,
                    LastRememberedPassword = ResetLastPassword
                };

                var response = await ApiService.Instance.PostAsync<ForgotPasswordRequest, ApiResponse<object>>("/api/v1/auth/forgot-password", req);

                if (response?.Success == true)
                {
                    // Clear form
                    ResetUsername = string.Empty;
                    ResetEmail = string.Empty;
                    ResetPhone = string.Empty;
                    ResetLastPassword = string.Empty;
                    SelectedRole = null;
                    SelectedBranch = null;

                    IsResetModalVisible = false;
                    SuccessMessage = "Password reset request submitted successfully.";
                }
                else
                {
                    ResetErrorMessage = response?.Error?.Message ?? "Failed to submit password reset request.";
                }
            }
            catch (Exception ex)
            {
                ResetErrorMessage = "Failed to connect to the server. Please try again.";
                Console.WriteLine($"[DEBUG] Reset submit exception: {ex.ToString()}");
            }
            finally
            {
                IsResetSubmitting = false;
            }
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
