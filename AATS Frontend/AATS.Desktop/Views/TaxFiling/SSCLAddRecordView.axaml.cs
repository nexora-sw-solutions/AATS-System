using System.Linq;
using System.Threading.Tasks;
using AATS.Desktop.ViewModels.TaxFiling;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AATS.Desktop.Views.TaxFiling
{
    public partial class SSCLAddRecordView : UserControl
    {
        public SSCLAddRecordView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(System.EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is SSCLAddRecordViewModel vm)
            {
                vm.RequestFilePicker = async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel != null)
                    {
                        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                        {
                            Title = "Select Documents",
                            AllowMultiple = true
                        });

                        if (files.Count > 0)
                        {
                            return files.Select(f => f.Path.LocalPath).ToArray();
                        }
                    }
                    return null;
                };
            }
        }
    }
}

