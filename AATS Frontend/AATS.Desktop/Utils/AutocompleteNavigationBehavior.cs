using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using AATS.Desktop.ViewModels;
using AATS.Desktop.Models;
using Avalonia.Controls.Presenters;

namespace AATS.Desktop.Utils
{
    public static class AutocompleteNavigationBehavior
    {
        public static void Register()
        {
            InputElement.KeyDownEvent.AddClassHandler<TextBox>(
                (textBox, e) =>
                {
                    if (e is KeyEventArgs keyEventArgs)
                    {
                        OnTextBoxKeyDown(textBox, keyEventArgs);
                    }
                },
                RoutingStrategies.Tunnel);
        }

        private static void OnTextBoxKeyDown(TextBox textBox, KeyEventArgs e)
        {
            if (textBox.Name != "ClientIdTextBox") return;

            var vm = textBox.DataContext as ViewModelBase;
            if (vm == null || !vm.IsClientCodeDropdownOpen) return;

            int count = vm.ClientCodeSuggestions.Count;
            if (count == 0) return;

            if (e.Key == Key.Down)
            {
                int nextIndex = vm.HighlightedSuggestionIndex + 1;
                if (nextIndex >= count)
                {
                    nextIndex = count - 1;
                }
                vm.HighlightedSuggestionIndex = nextIndex;

                ScrollSuggestionIntoView(textBox, nextIndex);
                UpdateVisualHighlight(textBox, nextIndex, vm);

                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                int prevIndex = vm.HighlightedSuggestionIndex - 1;
                if (prevIndex < 0)
                {
                    prevIndex = 0;
                }
                vm.HighlightedSuggestionIndex = prevIndex;

                ScrollSuggestionIntoView(textBox, prevIndex);
                UpdateVisualHighlight(textBox, prevIndex, vm);

                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (vm.HighlightedSuggestionIndex >= 0 && vm.HighlightedSuggestionIndex < count)
                {
                    var selectedClient = vm.ClientCodeSuggestions[vm.HighlightedSuggestionIndex];
                    vm.SelectClientCode(selectedClient);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                vm.IsClientCodeDropdownOpen = false;
                e.Handled = true;
            }
        }

        private static Popup? FindClientPopup(TextBox textBox)
        {
            // Search in the logical tree first
            ILogical? currentLogical = textBox;
            while (currentLogical != null)
            {
                var popup = FindPopupInLogicalChildren(currentLogical, textBox);
                if (popup != null) return popup;
                currentLogical = currentLogical.LogicalParent;
            }

            // Search in the visual tree
            Visual? currentVisual = textBox;
            while (currentVisual != null)
            {
                var popup = FindPopupInVisualChildren(currentVisual, textBox);
                if (popup != null) return popup;
                currentVisual = currentVisual.GetVisualParent();
            }

            return null;
        }

        private static Popup? FindPopupInLogicalChildren(ILogical parent, TextBox textBox)
        {
            if (parent is Popup p && (p.PlacementTarget == textBox || p.PlacementTarget?.Name == "ClientIdTextBox" || p.PlacementTarget?.Name == "ClientIdPanel"))
            {
                return p;
            }

            foreach (var child in parent.LogicalChildren)
            {
                var pChild = FindPopupInLogicalChildren(child, textBox);
                if (pChild != null) return pChild;
            }

            return null;
        }

        private static Popup? FindPopupInVisualChildren(Visual parent, TextBox textBox)
        {
            if (parent is Popup p && (p.PlacementTarget == textBox || p.PlacementTarget?.Name == "ClientIdTextBox" || p.PlacementTarget?.Name == "ClientIdPanel"))
            {
                return p;
            }

            foreach (var child in parent.GetVisualChildren())
            {
                var pChild = FindPopupInVisualChildren(child, textBox);
                if (pChild != null) return pChild;
            }

            return null;
        }

        private static T? FindVisualChild<T>(Visual parent) where T : Visual
        {
            if (parent is T target) return target;

            if (parent is Popup p && p.Child != null)
            {
                var result = FindVisualChild<T>(p.Child);
                if (result != null) return result;
            }

            foreach (var child in parent.GetVisualChildren())
            {
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private static List<T> FindAllVisualChildren<T>(Visual parent) where T : Visual
        {
            var results = new List<T>();
            
            if (parent is Popup p && p.Child != null)
            {
                results.AddRange(FindAllVisualChildren<T>(p.Child));
            }

            foreach (var child in parent.GetVisualChildren())
            {
                if (child is T target) results.Add(target);
                results.AddRange(FindAllVisualChildren<T>(child));
            }
            return results;
        }

        private static void UpdateVisualHighlight(TextBox textBox, int highlightedIndex, ViewModelBase vm)
        {
            var popup = FindClientPopup(textBox);
            if (popup == null) return;

            var itemsControl = FindVisualChild<ItemsControl>(popup);
            if (itemsControl == null) return;

            var presenters = FindAllVisualChildren<ContentPresenter>(itemsControl);
            foreach (var container in presenters)
            {
                if (container.DataContext is ClientRecord record)
                {
                    var button = FindVisualChild<Button>(container);
                    if (button != null)
                    {
                        bool isHighlighted = false;
                        if (highlightedIndex >= 0 && highlightedIndex < vm.ClientCodeSuggestions.Count)
                        {
                            isHighlighted = (record == vm.ClientCodeSuggestions[highlightedIndex]);
                        }

                        if (isHighlighted)
                        {
                            if (!button.Classes.Contains("highlighted"))
                            {
                                button.Classes.Add("highlighted");
                            }
                        }
                        else
                        {
                            button.Classes.Remove("highlighted");
                        }
                    }
                }
            }
        }

        private static void ScrollSuggestionIntoView(TextBox textBox, int index)
        {
            var popup = FindClientPopup(textBox);
            if (popup == null) return;

            var scrollViewer = FindVisualChild<ScrollViewer>(popup);
            if (scrollViewer == null) return;

            double itemHeight = 36.0;
            double viewHeight = scrollViewer.Viewport.Height;

            if (viewHeight <= 0)
            {
                viewHeight = scrollViewer.MaxHeight > 0 ? scrollViewer.MaxHeight : 200.0;
            }

            double itemTop = index * itemHeight;
            double itemBottom = itemTop + itemHeight;

            double currentScroll = scrollViewer.Offset.Y;

            if (itemTop < currentScroll)
            {
                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, itemTop);
            }
            else if (itemBottom > currentScroll + viewHeight)
            {
                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, itemBottom - viewHeight);
            }
        }
    }
}
