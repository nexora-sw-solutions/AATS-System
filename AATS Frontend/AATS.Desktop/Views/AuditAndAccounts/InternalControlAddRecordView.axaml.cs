using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AATS.Desktop.Views.AuditAndAccounts;

public partial class InternalControlAddRecordView : UserControl
{
    public InternalControlAddRecordView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is ViewModels.AuditAndAccounts.InternalControlAddRecordViewModel vm)
        {
            vm.RequestLogoPicker = async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return null;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Client Logo",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
                });

                return files.Count > 0 ? files[0].Path.LocalPath : null;
            };

            vm.RequestFilePicker = async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return null;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Internal Control Documents",
                    AllowMultiple = true
                });

                return files.Select(f => f.Path.LocalPath).ToArray();
            };
        }
    }
}
