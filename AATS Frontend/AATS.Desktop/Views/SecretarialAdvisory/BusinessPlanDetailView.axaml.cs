using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AATS.Desktop.ViewModels.SecretarialAdvisory;
using System.Linq;

namespace AATS.Desktop.Views.SecretarialAdvisory;

public partial class BusinessPlanDetailView : UserControl
{
    public BusinessPlanDetailView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is BusinessPlanDetailViewModel vm)
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
