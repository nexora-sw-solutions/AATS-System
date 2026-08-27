using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models;

public partial class Branch : ObservableObject
{
    [ObservableProperty] private Guid _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _code = string.Empty;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private DateTime _updatedAt;

    public override string ToString() => Name;
}
