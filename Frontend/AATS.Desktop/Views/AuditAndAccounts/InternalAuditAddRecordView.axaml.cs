using System.Linq;
using Avalonia;
using Avalonia.Controls;

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
                    if (topLevel == null) return null;

                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                    {
                        Title = "Select Client Logo",
                        AllowMultiple = false,
                        FileTypeFilter = new[] { Avalonia.Platform.Storage.FilePickerFileTypes.ImageAll }
                    });

                    return files.Count > 0 ? files[0].Name : null;
                };

                vm.RequestFilePicker = async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel == null) return null;

                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                    {
                        Title = "Select Source Documents",
                        AllowMultiple = true
                    });

                    return files.Select(f => f.Name).ToList();
                };
            }
        }
    }
}
