using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models;

public partial class ClientRecord : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    
    [ObservableProperty] 
    [property: JsonPropertyName("id")]
    private string? _id;
    
    [ObservableProperty]
    [property: JsonPropertyName("branchId")]
    private Guid _branchId;

    [ObservableProperty]
    [property: JsonPropertyName("clientCode")]
    private string? _clientCode;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(Initial))] 
    [property: JsonPropertyName("name")]
    private string? _name;
    
    public string Initial => !string.IsNullOrEmpty(Name) ? Name[0].ToString().ToUpper() : "?";
    
    [ObservableProperty] 
    [property: JsonPropertyName("email")]
    private string? _email;

    [ObservableProperty] 
    [property: JsonPropertyName("phone")]
    private string? _phone;
    
    [ObservableProperty] 
    [property: JsonPropertyName("branchName")] 
    private string? _branch;
    
    [ObservableProperty] 
    [property: JsonPropertyName("createdAt")] 
    private DateTime _date = DateTime.UtcNow;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(CategoryIcon))] 
    [NotifyPropertyChangedFor(nameof(CategoryColor))]
    [property: JsonPropertyName("category")]
    private string? _category; // Loyal, Blacklisted, Suspend
    
    public string CategoryIcon => Category switch
    {
        "Loyal" => "fa-solid fa-crown",
        "Blacklisted" => "fa-solid fa-ban",
        "Suspend" => "fa-solid fa-hourglass-half",
        "Corporate" => "fa-solid fa-hotel",
        _ => "fa-solid fa-briefcase"
    };

    public string CategoryColor => Category switch
    {
        "Loyal" => "#75de33",      // Custom Green
        "Blacklisted" => "#696b68", // Custom Gray
        "Suspend" => "#c96363",     // Custom Red
        _ => "Transparent"
    };

    [ObservableProperty] 
    [property: JsonPropertyName("totalRevenue")]
    private decimal _totalRevenue;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(HasDueAmount))] 
    [property: JsonPropertyName("outstandingBalance")]
    private decimal _outstandingBalance;
    
    public decimal DueAmount => OutstandingBalance;
    public bool HasDueAmount => OutstandingBalance > 0;

    private string _status = "Active";
    
    [JsonPropertyName("status")]
    public string Status
    {
        get => _status;
        set
        {
            if (value == "1") _status = "Active";
            else if (value == "2") _status = "Inactive";
            else _status = value;
            
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsActiveStatus));
        }
    }

    [JsonIgnore]
    public bool IsActiveStatus
    {
        get => Status == "Active" || Status == "1";
        set
        {
            Status = value ? "Active" : "Inactive";
        }
    }
}
