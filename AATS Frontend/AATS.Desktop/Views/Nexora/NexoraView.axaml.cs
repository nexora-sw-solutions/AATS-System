using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using AATS.Desktop.ViewModels.Nexora;

namespace AATS.Desktop.Views.Nexora;

public partial class NexoraView : UserControl
{
    public NexoraView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is NexoraViewModel vm)
        {
            vm.RequestFilePicker = async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return new List<string>();

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    Title = "Select Source Documents",
                    AllowMultiple = true
                });

                return files?.Select(f => f.Path.LocalPath).ToList() ?? new List<string>();
            };
        }
    }
}
