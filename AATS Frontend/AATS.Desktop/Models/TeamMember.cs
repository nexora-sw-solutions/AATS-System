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
    [ObservableProperty] private string? _currentPassword;
    
    [ObservableProperty] 
    [property: JsonPropertyName("branchName")]
    private string? _branch;

    [ObservableProperty]
    [property: JsonPropertyName("branchId")]
    private Guid _branchId;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsAdmin), nameof(IsAuditAndAssurance), nameof(IsSecretarialAndAdvisory), nameof(IsTaxFiling), nameof(IsAll), nameof(RoleIcon), nameof(RoleBackground), nameof(RoleForeground))]
    private string? _role; // Admin, Audit and Assurance, Secretarial and Advisory, Tax Filing, All
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CreatedAtFormatted))]
    private DateTime _createdAt;

    [ObservableProperty] private string? _password;
    
    public string Initial => !string.IsNullOrEmpty(Username) ? Username[0].ToString().ToUpper() : "?";
    
    public string RoleBackground => Role switch
    {
        "Admin" => "#E9D5FF",
        "Audit and Assurance" => "#DBEAFE",
        "Secretarial and Advisory" => "#D1FAE5",
        "Tax Filing" => "#FFEDD5",
        "All" => "#FCE7F3",
        _ => "#F1F5F9"
    };

    public string RoleForeground => Role switch
    {
        "Admin" => "#7E22CE",
        "Audit and Assurance" => "#1E40AF",
        "Secretarial and Advisory" => "#065F46",
        "Tax Filing" => "#9A3412",
        "All" => "#9D174D",
        _ => "#475569"
    };

    public string RoleIcon => Role switch
    {
        "Admin" => "fa-solid fa-user-shield",
        "Audit and Assurance" => "fa-solid fa-file-invoice",
        "Secretarial and Advisory" => "fa-solid fa-scale-balanced",
        "Tax Filing" => "fa-solid fa-calculator",
        "All" => "fa-solid fa-users-gear",
        _ => "fa-solid fa-user"
    };

    public string CreatedAtFormatted => CreatedAt.ToString("MMM dd, yyyy");

    public bool IsAdmin => Role == "Admin";
    public bool IsAuditAndAssurance => Role == "Audit and Assurance";
    public bool IsSecretarialAndAdvisory => Role == "Secretarial and Advisory";
    public bool IsTaxFiling => Role == "Tax Filing";
    public bool IsAll => Role == "All";
}
