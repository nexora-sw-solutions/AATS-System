using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AATS.Desktop.ViewModels.SecretarialAdvisory;
using System.Linq;
using System;

namespace AATS.Desktop.Views.SecretarialAdvisory
{
    public partial class CompanyRegistrationDetailView : UserControl
    {
        public CompanyRegistrationDetailView()
        {
            InitializeComponent();
        }

        public void Overlay_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (sender is Border border && e.Source == border)
            {
                if (DataContext is CompanyRegistrationDetailViewModel vm)
                {
                    vm.HandleOverlayClickCommand.Execute(null);
                }
            }
        }

        protected override void OnDataContextChanged(System.EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is CompanyRegistrationDetailViewModel vm)
            {
                vm.RequestFilePicker = async () =>
                {
                    System.Console.WriteLine("[DEBUG] RequestFilePicker inside View invoked!");
                    var topLevel = TopLevel.GetTopLevel(this);
                    System.Console.WriteLine($"[DEBUG] TopLevel window is null? {topLevel == null}");
                    if (topLevel != null)
                    {
                        try
                        {
                            System.Console.WriteLine("[DEBUG] Calling StorageProvider.OpenFilePickerAsync...");
                            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                            {
                                Title = "Select Documents",
                                AllowMultiple = false
                            });
                            System.Console.WriteLine($"[DEBUG] OpenFilePickerAsync returned files count: {files?.Count ?? 0}");

                            if (files.Count > 0)
                            {
                                return files.Select(f => f.Path.LocalPath).ToArray();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine($"[DEBUG] StorageProvider.OpenFilePickerAsync threw exception: {ex}");
                        }
                    }
                    return null;
                };
            }
        }
    }
}
