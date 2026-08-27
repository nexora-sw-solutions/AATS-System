using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AATS.Desktop.ViewModels.SecretarialAdvisory;
using System.Linq;
using System;

namespace AATS.Desktop.Views.SecretarialAdvisory;

public partial class EPFETFDetailView : UserControl
{
    public EPFETFDetailView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is EPFETFDetailViewModel vm)
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
                            Title = "Select EPF/ETF Documents",
                            AllowMultiple = true
                        });

                        if (files != null && files.Count > 0)
                        {
                            return files.Select(f => f.Path.LocalPath).ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] StorageProvider.OpenFilePickerAsync exception: {ex}");
                    }
                }
                return null;
            };
        }
    }
}
