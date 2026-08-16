using System;
using System.Collections.Generic;
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
    private string? _category; // Active, Black Listed, Suspended
    
    public string CategoryIcon => Category switch
    {
        "Active" => "fa-solid fa-crown",
        "Black Listed" => "fa-solid fa-ban",
        "Suspended" => "fa-solid fa-hourglass-half",
        "Loyal" => "fa-solid fa-crown",
        "Blacklisted" => "fa-solid fa-ban",
        "Suspend" => "fa-solid fa-hourglass-half",
        "Corporate" => "fa-solid fa-hotel",
        _ => "fa-solid fa-briefcase"
    };

    public string CategoryColor => Category switch
    {
        "Active" => "#34D399",       // Muted Emerald/Sage Green
        "Black Listed" => "#94A3B8", // Muted Slate Grey/Ash
        "Suspended" => "#F87171",    // Muted Coral Red
        "Loyal" => "#34D399",        // Muted Green
        "Blacklisted" => "#94A3B8",   // Muted Slate Grey
        "Suspend" => "#F87171",      // Muted Red
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

    [ObservableProperty]
    [property: JsonPropertyName("logoStorageKey")]
    private string? _logoStorageKey;

    [JsonIgnore]
    public bool IsActiveStatus
    {
        get => Status == "Active" || Status == "1";
        set
        {
            Status = value ? "Active" : "Inactive";
        }
    }

    [ObservableProperty]
    [property: JsonPropertyName("notes")]
    private string? _notes;

    [ObservableProperty]
    [property: JsonPropertyName("brAttachments")]
    private List<SourceDocument>? _brAttachments = new();

    [ObservableProperty]
    [property: JsonPropertyName("tinAttachments")]
    private List<SourceDocument>? _tinAttachments = new();

    [ObservableProperty]
    [property: JsonPropertyName("form01Attachments")]
    private List<SourceDocument>? _form01Attachments = new();

    [ObservableProperty]
    [property: JsonPropertyName("articleOfAssociationAttachments")]
    private List<SourceDocument>? _articleOfAssociationAttachments = new();

    [ObservableProperty]
    [property: JsonPropertyName("nicAttachments")]
    private List<SourceDocument>? _nicAttachments = new();
}
