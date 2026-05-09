using System;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Services
{
    /// <summary>
    /// Singleton service managing application theme (Dark/Light).
    /// Toggle is wired via MainViewModel -> MainShellView moon/sun button.
    /// </summary>
    public partial class ThemeService : ObservableObject
    {
        private static readonly Lazy<ThemeService> _instance = new(() => new ThemeService());
        public static ThemeService Instance => _instance.Value;

        [ObservableProperty]
        private bool _isDarkMode = true;

        private ThemeService() { }

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (Application.Current is not null)
            {
                Application.Current.RequestedThemeVariant = 
                    IsDarkMode ? ThemeVariant.Dark : ThemeVariant.Light;
            }
        }

        /// <summary>
        /// Initialize with dark mode as default on first launch.
        /// </summary>
        public void Initialize()
        {
            IsDarkMode = true;
            ApplyTheme();
        }
    }
}
