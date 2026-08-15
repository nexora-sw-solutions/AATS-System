using Avalonia.Controls;

namespace AATS.Desktop.Views.AuditAndAccounts;

public partial class AuditAssuranceDetailView : UserControl
{
    public AuditAssuranceDetailView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is ViewModels.AuditAndAccounts.AuditAssuranceDetailViewModel vm)
        {
            vm.RequestFilePicker = async () =>
            {
                var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    try
                    {
                        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                        {
                            Title = "Select Documents",
                            AllowMultiple = false
                        });

                        if (files.Count > 0)
                        {
                            return System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(files, f => f.Path.LocalPath));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] OpenFilePickerAsync: {ex.Message}");
                    }
                }
                return null;
            };
        }
    }
}
