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

        protected override void OnDataContextChanged(System.EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is CompanyRegistrationDetailViewModel vm)
            {
                vm.RequestFilePicker = async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel != null)
                    {
                        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                        {
                            Title = "Select Documents",
                            AllowMultiple = false
                        });

                        if (files.Count > 0)
                        {
                            return files.Select(f => f.Name).ToArray();
                        }
                    }
                    return null;
                };
            }
        }
    }
}
