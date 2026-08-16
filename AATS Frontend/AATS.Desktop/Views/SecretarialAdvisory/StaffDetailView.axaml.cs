using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AATS.Desktop.ViewModels.SecretarialAdvisory;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AATS.Desktop.Views.SecretarialAdvisory;

public partial class StaffDetailView : UserControl
{
    public StaffDetailView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is StaffDetailViewModel vm)
        {
            vm.RequestNicPicker = () => OpenPickerAsync("Select NIC Document");
            vm.RequestBrPicker = () => OpenPickerAsync("Select BR Document");
            vm.RequestR1Picker = () => OpenPickerAsync("Select R1 Document");
            vm.RequestArtPicker = () => OpenPickerAsync("Select ART Document");
            vm.RequestStaffNicPicker = () => OpenPickerAsync("Select Staff NIC Document");
        }
    }

    private async Task<string?> OpenPickerAsync(string title)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = new[] 
                { 
                    FilePickerFileTypes.Pdf, 
                    FilePickerFileTypes.ImageAll,
                    new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
                }
            });

            if (files.Count > 0)
            {
                return files[0].Path.LocalPath;
            }
        }
        return null;
    }
}
