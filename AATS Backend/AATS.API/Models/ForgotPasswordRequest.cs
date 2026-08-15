using System.ComponentModel.DataAnnotations;

namespace AATS.API.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string Branch { get; set; } = string.Empty;

        [Required]
        public string LastRememberedPassword { get; set; } = string.Empty;
    }
}
