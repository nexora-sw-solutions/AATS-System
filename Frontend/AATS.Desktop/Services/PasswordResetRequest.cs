namespace AATS.Desktop.Services
{
    public class PasswordResetRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string LastPassword { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? Branch { get; set; }
    }
}
