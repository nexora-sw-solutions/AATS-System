using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace AATS.Desktop.Views.AuditAndAccounts;

public partial class ManagementAccountAddRecordView : UserControl
{
    public ManagementAccountAddRecordView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is ViewModels.AuditAndAccounts.ManagementAccountAddRecordViewModel vm)
        {
            vm.RequestLogoPicker = async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return null;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    Title = "Select Client Logo",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { Avalonia.Platform.Storage.FilePickerFileTypes.ImageAll }
                });

                return files.Count > 0 ? files[0].Path.LocalPath : null;
            };

            vm.RequestFilePicker = async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return null;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    Title = "Select Management Account Documents",
                    AllowMultiple = true
                });

                return files.Select(f => f.Path.LocalPath).ToArray();
            };
        }
    }
}
