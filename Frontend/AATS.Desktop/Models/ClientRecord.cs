using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models;

public partial class ClientRecord : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    
    [ObservableProperty] private string? _id;
    
    [ObservableProperty] 
    [property: JsonPropertyName("clientName")] 
    [NotifyPropertyChangedFor(nameof(Initial))] 
    private string? _name;
    
    public string Initial => !string.IsNullOrEmpty(Name) ? Name[0].ToString().ToUpper() : "?";
    
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _phone;
    
    [ObservableProperty] 
    [property: JsonPropertyName("branchName")] 
    private string? _branch;
    
    [ObservableProperty] 
    [property: JsonPropertyName("createdAt")] 
    private DateTime _date = DateTime.Now;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(CategoryIcon))] 
    private string? _category; // SME, Corporate
    
    public string CategoryIcon => Category == "Corporate" ? "fa-solid fa-hotel" : "fa-solid fa-briefcase";

    [ObservableProperty] private decimal _totalRevenue;
    
    [ObservableProperty] 
    [property: JsonPropertyName("outstandingBalance")] 
    [NotifyPropertyChangedFor(nameof(HasDueAmount))] 
    private decimal _dueAmount;
    
    public bool HasDueAmount => DueAmount > 0;

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsActive))] 
    [NotifyPropertyChangedFor(nameof(IsInactive))] 
    [NotifyPropertyChangedFor(nameof(IsActiveStatus))] 
    private string? _status; // Active, Inactive
    public bool IsActive => Status == "Active";
    public bool IsInactive => Status == "Inactive";

    public bool IsActiveStatus
    {
        get => Status == "Active";
        set => Status = value ? "Active" : "Inactive";
    }
}
