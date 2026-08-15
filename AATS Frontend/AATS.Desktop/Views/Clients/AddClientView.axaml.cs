using System.Linq;
using System.Threading.Tasks;
using AATS.Desktop.ViewModels.Clients;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AATS.Desktop.Views.Clients
{
    public partial class AddClientView : UserControl
    {
        public AddClientView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(System.EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is AddClientViewModel vm)
            {
                vm.RequestMultipleFilePicker = async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel != null)
                    {
                        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                        {
                            Title = "Select Documents",
                            AllowMultiple = true,
                            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll, FilePickerFileTypes.Pdf }
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
