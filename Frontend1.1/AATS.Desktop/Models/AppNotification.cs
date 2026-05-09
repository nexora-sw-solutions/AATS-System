using System;

namespace AATS.Desktop.Models;

public class AppNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }
    
    public string DisplayTime => Timestamp.ToString("MMM dd, hh:mm tt");
    public string Summary => $"{UserName} {Action}";
    public string Icon => Action.ToLower().Contains("login") ? "fa-solid fa-right-to-bracket" : "fa-solid fa-right-from-bracket";
    public string IconColor => Action.ToLower().Contains("login") ? "#10B981" : "#EF4444";
}
