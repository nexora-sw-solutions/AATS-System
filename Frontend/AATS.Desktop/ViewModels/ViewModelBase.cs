using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.ViewModels
{
    public abstract partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty] private bool _hasFormError = false;
        [ObservableProperty] private string _formErrorMessage = string.Empty;
    }
}
