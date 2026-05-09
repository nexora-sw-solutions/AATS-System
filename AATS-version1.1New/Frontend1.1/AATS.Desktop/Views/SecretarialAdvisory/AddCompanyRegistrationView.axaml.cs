using System.Threading.Tasks;
using AATS.Desktop.ViewModels.SecretarialAdvisory;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AATS.Desktop.Views.SecretarialAdvisory;

public partial class AddCompanyRegistrationView : UserControl
{
    public AddCompanyRegistrationView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is AddCompanyRegistrationViewModel vm)
        {
            vm.RequestNicPicker = async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Select NIC Document",
                        AllowMultiple = false,
                        FileTypeFilter = new[] { FilePickerFileTypes.ImageAll, FilePickerFileTypes.Pdf }
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
