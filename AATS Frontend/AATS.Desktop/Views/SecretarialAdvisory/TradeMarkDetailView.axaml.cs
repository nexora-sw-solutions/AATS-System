using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AATS.Desktop.ViewModels.SecretarialAdvisory;

namespace AATS.Desktop.Views.SecretarialAdvisory;

public partial class TradeMarkDetailView : UserControl
{
    public TradeMarkDetailView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is TradeMarkDetailViewModel vm)
        {
            vm.RequestFilePicker = async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    try
                    {
                        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                        {
                            Title = "Select Documents",
                            AllowMultiple = false
                        });

                        if (files.Count > 0)
                        {
                            return files.Select(f => f.Path.LocalPath).ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] StorageProvider.OpenFilePickerAsync threw exception: {ex}");
                    }
                }
                return null;
            };
        }
    }
}
