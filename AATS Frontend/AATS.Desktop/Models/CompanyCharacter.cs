using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AATS.Desktop.Models;

public partial class CompanyCharacter : ObservableObject
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _role;
    [ObservableProperty] private double _sharePercentage;
    [ObservableProperty] private string? _note;
    [ObservableProperty] private string? _tIN;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private string? _address;

    // For "Others" section
    [ObservableProperty] private string? _detail;

    // For NIC Upload
    [ObservableProperty] private string? _nicFileName;
    [ObservableProperty] private bool _hasNicFile;
    // For Expand/Collapse UI
    [ObservableProperty] private bool _isExpanded = false;
    
    [RelayCommand]
    private void ToggleExpand() => IsExpanded = !IsExpanded;
}
