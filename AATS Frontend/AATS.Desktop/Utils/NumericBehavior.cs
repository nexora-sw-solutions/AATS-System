using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AATS.Desktop.Utils;

public enum ValidationMode
{
    None,
    Decimal,
    Integer,
    Phone
}

public static class NumericBehavior
{
    public static readonly AttachedProperty<ValidationMode> ModeProperty =
        AvaloniaProperty.RegisterAttached<TextBox, ValidationMode>("Mode", typeof(NumericBehavior), ValidationMode.None);

    public static ValidationMode GetMode(TextBox element) => element.GetValue(ModeProperty);
    public static void SetMode(TextBox element, ValidationMode value) => element.SetValue(ModeProperty, value);

    public static void Register() { }

    static NumericBehavior()
    {
        ModeProperty.Changed.AddClassHandler<TextBox>(OnModeChanged);
    }

    private static void OnModeChanged(TextBox textBox, AvaloniaPropertyChangedEventArgs e)
    {
        // Unregister existing if any (simplification: just register if not None)
        textBox.RemoveHandler(InputElement.TextInputEvent, OnTextInput);
        textBox.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        textBox.LostFocus -= OnLostFocus;

        if (e.NewValue is ValidationMode mode && mode != ValidationMode.None)
        {
            // Use Tunneling to catch the event before the TextBox processes it
            textBox.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
            textBox.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            
            if (mode == ValidationMode.Decimal)
            {
                textBox.LostFocus += OnLostFocus;
            }
        }
    }

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            var textBox = sender as TextBox;
            if (textBox != null && GetMode(textBox) != ValidationMode.Phone)
            {
                e.Handled = true;
            }
        }
    }

    private static void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text)) return;

        var textBox = sender as TextBox;
        if (textBox == null) return;

        var mode = GetMode(textBox);
        if (mode == ValidationMode.None) return;

        string currentText = textBox.Text ?? "";
        char inputChar = e.Text[0];

        switch (mode)
        {
            case ValidationMode.Integer:
                if (!char.IsDigit(inputChar)) e.Handled = true;
                break;

            case ValidationMode.Decimal:
                if (!char.IsDigit(inputChar) && inputChar != '.')
                {
                    e.Handled = true;
                }
                else if (inputChar == '.' && currentText.Contains('.'))
                {
                    e.Handled = true;
                }
                break;

            case ValidationMode.Phone:
                if (!char.IsDigit(inputChar) && inputChar != '+' && inputChar != '-' && inputChar != '(' && inputChar != ')' && inputChar != ' ')
                {
                    e.Handled = true;
                }
                break;
        }
    }

    private static void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text)) return;

        if (GetMode(textBox) == ValidationMode.Decimal && decimal.TryParse(textBox.Text, out decimal val))
        {
            textBox.Text = val.ToString("N2");
        }
    }
}
