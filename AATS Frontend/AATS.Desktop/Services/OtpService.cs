using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AATS.Desktop.Services;
using AATS.Desktop.Models;

namespace AATS.Desktop.Services
{
    public class OtpResponse
    {
        public string Otp { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class OtpService
    {
        private static OtpService? _instance;
        public static OtpService Instance => _instance ??= new OtpService();

        private string? _currentOtp;
        private DateTime? _otpExpiry;
        private bool _isAuthorized;
        private readonly HashSet<string> _usedOtps = new();

        public bool IsAuthorized
        {
            get => _isAuthorized;
            set => _isAuthorized = value;
        }

        public int ExpiryMinutes { get; set; } = 5;

        private OtpService()
        {
        }

        public void ResetAuth()
        {
            IsAuthorized = false;
        }

        public async Task<string> GenerateOtpAsync(string username)
        {
            try
            {
                var response = await ApiService.Instance.PostAsync<object, ApiResponse<OtpResponse>>("/api/v1/auth/request-otp", new { Username = username });
                if (response?.Success == true && response.Data != null)
                {
                    _currentOtp = response.Data.Otp;
                    _otpExpiry = DateTime.Now.AddMinutes(ExpiryMinutes);

                    LogService.Instance.AddLog(
                        action: "OTP Request",
                        module: "Authorization",
                        branch: "Central",
                        details: $"OTP requested for record editing by user '{username}'. OTP sent to administrators."
                    );

                    NotificationService.Instance.AddNotification(
                        userName: "System",
                        action: $"generated Edit Authorization OTP: {_currentOtp} (Valid for {ExpiryMinutes} mins)"
                    );

                    return _currentOtp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to request OTP from server: {ex.Message}");
            }

            // Fallback for local testing if API fails or is unreachable
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();
            
            _currentOtp = otpCode;
            _otpExpiry = DateTime.Now.AddMinutes(ExpiryMinutes);

            LogService.Instance.AddLog(
                action: "OTP Request (Fallback)",
                module: "Authorization",
                branch: "Central",
                details: $"OTP requested locally (fallback) for record editing by user '{username}'."
            );

            NotificationService.Instance.AddNotification(
                userName: "System",
                action: $"generated Edit Authorization OTP (Fallback): {otpCode} (Valid for {ExpiryMinutes} mins)"
            );

            return otpCode;
        }

        public async Task<(bool Success, string Message)> VerifyOtpAsync(string username, string enteredOtp)
        {
            if (string.IsNullOrWhiteSpace(enteredOtp))
            {
                return (false, "Please enter the OTP.");
            }

            try
            {
                var response = await ApiService.Instance.PostAsync<object, ApiResponse<object>>("/api/v1/auth/verify-otp", new { Username = username, Otp = enteredOtp });
                if (response?.Success == true)
                {
                    _usedOtps.Add(enteredOtp);
                    _currentOtp = null;
                    _otpExpiry = null;
                    IsAuthorized = true;

                    LogService.Instance.AddLog(
                        action: "OTP Verified",
                        module: "Authorization",
                        branch: "Central",
                        details: $"OTP verified successfully on server for user '{username}'. Edit access granted."
                    );

                    return (true, "OTP verified successfully. Access granted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] OTP verification failed on server: {ex.Message}");
            }

            // Fallback to local memory verification if backend is unreachable/fails
            if (string.IsNullOrEmpty(_currentOtp) || _otpExpiry == null)
            {
                return (false, "No active OTP request found. Please request a new OTP.");
            }

            if (DateTime.Now > _otpExpiry.Value)
            {
                _currentOtp = null;
                _otpExpiry = null;
                LogService.Instance.AddLog(
                    action: "OTP Expired (Fallback)",
                    module: "Authorization",
                    branch: "Central",
                    details: $"Expired OTP entered during local fallback. Access denied."
                );
                return (false, "OTP has expired. Please request a new one.");
            }

            if (_usedOtps.Contains(enteredOtp))
            {
                return (false, "OTP has already been used. Please request a new one.");
            }

            if (enteredOtp == _currentOtp)
            {
                _usedOtps.Add(enteredOtp);
                _currentOtp = null;
                _otpExpiry = null;
                IsAuthorized = true;

                LogService.Instance.AddLog(
                    action: "OTP Verified (Fallback)",
                    module: "Authorization",
                    branch: "Central",
                    details: $"OTP verified successfully via local fallback. Edit access granted."
                );

                return (true, "OTP verified successfully. Access granted.");
            }

            LogService.Instance.AddLog(
                action: "OTP Failed (Fallback)",
                module: "Authorization",
                branch: "Central",
                details: $"Invalid OTP entered during local fallback: '{enteredOtp}'. Access denied."
            );

            return (false, "Invalid OTP. Access denied. Please try again.");
        }
    }
}
