using AATS.Desktop.ViewModels.SecretarialAdvisory;
using Avalonia.Controls;

namespace AATS.Desktop.Views.SecretarialAdvisory;

public partial class AddSecretarialOthersView : UserControl
{
    public AddSecretarialOthersView()
    {
        InitializeComponent();
    }

    private void OnPointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
    {
        e.Handled = true;
    }
}
