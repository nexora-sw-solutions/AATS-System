using System;
using System.Text.Json.Serialization;
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
    
    [ObservableProperty] 
    [property: JsonPropertyName("branchName")]
    private string? _branch;

    [ObservableProperty]
    [property: JsonPropertyName("branchId")]
    private Guid _branchId;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsAdmin), nameof(IsStaff), nameof(RoleIcon), nameof(RoleBackground), nameof(RoleForeground))]
    private string? _role; // Admin, Staff
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CreatedAtFormatted))]
    private DateTime _createdAt;

    [ObservableProperty] private string? _password;
    
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
