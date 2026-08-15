using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AATS.Desktop.Views.AuditAndAccounts
{
    public partial class InternalAuditAddRecordView : UserControl
    {
        public InternalAuditAddRecordView()
        {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (DataContext is ViewModels.AuditAndAccounts.InternalAuditAddRecordViewModel vm)
            {
                vm.RequestLogoPicker = async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel != null)
                    {
                        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                        {
                            Title = "Select Client Logo",
                            AllowMultiple = false,
                            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
                        });

                        if (files.Count > 0)
                        {
                            return files[0].Path.LocalPath;
                        }
                    }
                    return null;
                };

                vm.RequestFilePicker = async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel == null) return null;

                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Select Source Documents",
                        AllowMultiple = true
                    });

                    return files.Select(f => f.Path.LocalPath).ToList();
                };
            }
        }
    }
}
