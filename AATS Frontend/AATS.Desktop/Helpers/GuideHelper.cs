using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Reflection;

namespace AATS.Desktop.Helpers;

public static class GuideHelper
{
    public static readonly AttachedProperty<bool> EnableOutsideClickCloseProperty =
        AvaloniaProperty.RegisterAttached<Border, bool>("EnableOutsideClickClose", typeof(GuideHelper), false);

    public static bool GetEnableOutsideClickClose(Border element) => element.GetValue(EnableOutsideClickCloseProperty);
    
    public static void SetEnableOutsideClickClose(Border element, bool value) => element.SetValue(EnableOutsideClickCloseProperty, value);

    static GuideHelper()
    {
        EnableOutsideClickCloseProperty.Changed.Subscribe(e =>
        {
            if (e.Sender is Border border)
            {
                border.PointerPressed -= Border_PointerPressed;
                if (e.NewValue.Value)
                {
                    border.PointerPressed += Border_PointerPressed;
                }
            }
        });
    }

    private static void Border_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && e.Source == border)
        {
            var dc = border.DataContext;
            if (dc != null)
            {
                var prop = dc.GetType().GetProperty("IsGuideVisible", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite && prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(dc, false);
                    e.Handled = true;
                }
            }
        }
    }
}