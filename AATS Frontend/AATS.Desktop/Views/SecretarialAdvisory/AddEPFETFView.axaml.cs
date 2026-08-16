using AATS.Desktop.ViewModels.SecretarialAdvisory;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Linq;

namespace AATS.Desktop.Views.SecretarialAdvisory;

public partial class AddEPFETFView : UserControl
{
    public AddEPFETFView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
    }
}
