using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace StatisticsAnalysisTool.macOS.Controls;

/// <summary>
/// Warning bar control for displaying warning messages
/// </summary>
public partial class WarningBarControl : UserControl
{
    public WarningBarControl()
    {
        InitializeComponent();
    }

    public string WarningText
    {
        get => GetValue(WarningTextProperty);
        set => SetValue(WarningTextProperty, value);
    }

    public static readonly StyledProperty<string> WarningTextProperty =
        AvaloniaProperty.Register<WarningBarControl, string>(nameof(WarningText));

    private void BtnWarningBar_Click(object? sender, RoutedEventArgs e)
    {
        // Hide the warning bar
        this.IsVisible = false;
    }
}
