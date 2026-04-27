using AATS.Desktop.ViewModels;
using AATS.Desktop.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System.Linq;

namespace AATS.Desktop
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            IconProvider.Current.Register<FontAwesomeIconProvider>();
            AATS.Desktop.Utils.NumericBehavior.Register();
            Services.ThemeService.Instance.Initialize();
        }

        private MainShellViewModel? _shellViewModel;

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();
                
                _shellViewModel = new MainShellViewModel();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _shellViewModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void SwitchToMainWindow()
        {
            _shellViewModel?.ShowDashboardCommand.Execute(null);
        }

        public void SwitchToLoginWindow()
        {
            _shellViewModel?.ShowLoginCommand.Execute(null);
        }



        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}