using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace StatisticsAnalysisTool.macOS.Controls;

/// <summary>
/// Information bar control for displaying information messages
/// </summary>
public partial class InformationBarControl : UserControl
{
    public InformationBarControl()
    {
        InitializeComponent();
    }

    public string InformationText
    {
        get => GetValue(InformationTextProperty);
        set => SetValue(InformationTextProperty, value);
    }

    public static readonly StyledProperty<string> InformationTextProperty =
        AvaloniaProperty.Register<InformationBarControl, string>(nameof(InformationText));

    private void BtnInformationBar_Click(object? sender, RoutedEventArgs e)
    {
        // Hide the information bar
        this.IsVisible = false;
    }
}
