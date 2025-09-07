using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace StatisticsAnalysisTool.macOS.Controls;

/// <summary>
/// Error bar control for displaying error messages
/// </summary>
public partial class ErrorBarControl : UserControl
{
    public ErrorBarControl()
    {
        InitializeComponent();
    }

    public string ErrorText
    {
        get => GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    public static readonly StyledProperty<string> ErrorTextProperty =
        AvaloniaProperty.Register<ErrorBarControl, string>(nameof(ErrorText));

    private void BtnErrorBar_Click(object? sender, RoutedEventArgs e)
    {
        // Hide the error bar
        this.IsVisible = false;
    }
}
