using System;
using System.Linq;

namespace AATS.Desktop.Services
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return email.Contains("@") && email.Split('@').Length == 2 && email.Split('@')[1].Contains(".");
        }

        public static bool IsValidPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            // Support formats like +94, etc., but essentially should be digits
            var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
            return digitsOnly.Length >= 10;
        }

        public static bool IsValidName(string? name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length >= 3;
        }
    }
}
