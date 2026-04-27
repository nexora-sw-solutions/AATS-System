using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models
{
    public partial class AppDocument : ObservableObject
    {
        [ObservableProperty] private string _fileName = string.Empty;
        [ObservableProperty] private string _fileSize = string.Empty;
        [ObservableProperty] private string _category = "PROCESS";
        [ObservableProperty] private string _type = string.Empty;
        [ObservableProperty] private string _description = string.Empty;
        [ObservableProperty] private bool _isExisting = true;
        [ObservableProperty] private string _imagePath = "avares://AATS.Desktop/Assets/logo.png";
    }
}
