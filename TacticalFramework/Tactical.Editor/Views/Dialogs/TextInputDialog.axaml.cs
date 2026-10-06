using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using Tactical.Core.Domain.Terrain;

namespace Tactical.Editor.Views.Dialogs
{
    public partial class TextInputDialog : Window
    {
        public TextInputDialog(string title, string prompt, string initialValue = "")
        {
            InitializeComponent();

            Title = title;
            PromptText.Text = prompt;
            ValueTextBox.Text = initialValue;

            ValueTextBox.AttachedToVisualTree += (_, _) =>
            {
                ValueTextBox.Focus();
                ValueTextBox.SelectAll();
            };
        }

        private void Ok_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string value = ValueTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(value))
                return;

            Close(value);
        }

        private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
