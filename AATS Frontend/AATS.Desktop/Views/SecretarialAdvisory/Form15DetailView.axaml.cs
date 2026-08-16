using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AATS.Desktop.ViewModels.SecretarialAdvisory;
using System.Linq;
using System;

namespace AATS.Desktop.Views.SecretarialAdvisory
{
    public partial class Form15DetailView : UserControl
    {
        public Form15DetailView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(System.EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is Form15DetailViewModel vm)
            {
                vm.RequestMultipleFiles = async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel != null)
                    {
                        try
                        {
                            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                            {
                                Title = "Select Documents",
                                AllowMultiple = true
                            });

                            if (files != null && files.Count > 0)
                            {
                                return files.Select(f => f.Path.LocalPath).ToArray();
                            }
                        }
                        catch (Exception)
                        {
                            // Ignored or logged
                        }
                    }
                    return null;
                };
            }
        }
    }
}
