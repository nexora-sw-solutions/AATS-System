using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models;

public partial class TeamMember : ObservableObject
{
    [ObservableProperty] private string? _id;
    [ObservableProperty] private bool _isSelected;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(Initial))]
    private string? _username;

    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private string? _branch;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsAdmin), nameof(IsStaff), nameof(RoleIcon), nameof(RoleBackground), nameof(RoleForeground))]
    private string? _role; // Admin, Staff
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CreatedAtFormatted))]
    private DateTime _createdAt;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActive), nameof(StatusText), nameof(StatusColor))]
    private string? _status; // Active, Inactive

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLogo))]
    private string? _logoUrl;

    public bool HasLogo => !string.IsNullOrEmpty(LogoUrl);
    public bool IsActive => string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase);
    public string StatusText => IsActive ? "Active" : "Inactive";
    public string StatusColor => IsActive ? "#10B981" : "#EF4444"; // Emerald-500, Red-500
    
    public string Initial => !string.IsNullOrEmpty(Username) ? Username[0].ToString().ToUpper() : "?";
    
    public string RoleBackground => Role switch
    {
        "Admin" => "#E9D5FF",
        "Staff" => "#DBEAFE",
        _ => "#F1F5F9"
    };

    public string RoleForeground => Role switch
    {
        "Admin" => "#7E22CE",
        "Staff" => "#1E40AF",
        _ => "#475569"
    };

    public string RoleIcon => Role switch
    {
        "Admin" => "fa-solid fa-user-shield",
        "Staff" => "fa-solid fa-user",
        _ => "fa-solid fa-user"
    };

    public string CreatedAtFormatted => CreatedAt.ToString("MMM dd, yyyy");

    public bool IsAdmin => Role == "Admin";
    public bool IsStaff => Role == "Staff";
}
