using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System.Linq;
using AATS.Desktop.ViewModels.SecretarialAdvisory;

namespace AATS.Desktop.Views.SecretarialAdvisory;

public partial class TradeLicenseDetailView : UserControl
{
    public TradeLicenseDetailView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is TradeLicenseDetailViewModel vm)
        {
            vm.RequestFilePicker = async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return null;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Document",
                    AllowMultiple = true
                });

                if (files != null && files.Count > 0)
                {
                    return files.Select(f => f.Path.LocalPath).ToArray();
                }
                return null;
            };
        }
    }
}
