using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models;

public partial class CompanyCharacter : ObservableObject
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _role;
    [ObservableProperty] private double _sharePercentage;
    [ObservableProperty] private string? _note;
    
    // For "Others" section
    [ObservableProperty] private string? _detail;
}
