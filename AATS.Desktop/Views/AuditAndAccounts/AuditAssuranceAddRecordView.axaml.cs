using System.Linq;
using System.Threading.Tasks;
using AATS.Desktop.ViewModels.AuditAndAccounts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace AATS.Desktop.Views.AuditAndAccounts
{
    public partial class AuditAssuranceAddRecordView : UserControl
    {
        public AuditAssuranceAddRecordView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(System.EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is AuditAssuranceAddRecordViewModel vm)
            {
                vm.RequestFilePicker = async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel != null)
                    {
                        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                        {
                            Title = "Select Source Documents",
                            AllowMultiple = true
                        });

                        if (files.Count > 0)
                        {
                            return files.Select(f => f.Path.LocalPath).ToArray();
                        }
                    }
                    return null;
                };

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
            }
        }
    }
}
