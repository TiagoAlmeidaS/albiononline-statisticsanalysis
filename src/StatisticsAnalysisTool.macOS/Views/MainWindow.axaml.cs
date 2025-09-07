using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace StatisticsAnalysisTool.macOS.Views;

/// <summary>
/// Main window for the Statistics Analysis Tool with Albion Online visual style
/// </summary>
public partial class MainWindow : Window
{
    private bool _isMaximized = false;

    public MainWindow()
    {
        InitializeComponent();
        SetupWindow();
    }

    private void SetupWindow()
    {
        // Configure window properties
        this.WindowState = WindowState.Normal;
        this.CanResize = true;
        this.ShowInTaskbar = true;
        this.Topmost = false;
    }

    #region Window Event Handlers

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_isMaximized)
        {
            this.WindowState = WindowState.Normal;
            MaximizeButton.Content = "□";
            _isMaximized = false;
        }
        else
        {
            this.WindowState = WindowState.Maximized;
            MaximizeButton.Content = "❐";
            _isMaximized = true;
        }
    }

    #endregion

    #region Window Management

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        // Center window on screen if not maximized
        if (WindowState != WindowState.Maximized)
        {
            var screen = Screens.Primary;
            if (screen != null)
            {
                var workingArea = screen.WorkingArea;
                this.Position = new PixelPoint(
                    (int)(workingArea.X + (workingArea.Width - this.Width) / 2),
                    (int)(workingArea.Y + (workingArea.Height - this.Height) / 2)
                );
            }
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Save window state and position
        SaveWindowState();
        base.OnClosing(e);
    }

    private void SaveWindowState()
    {
        // TODO: Implement settings persistence
        // This would save window position, size, and state to user settings
    }

    #endregion
}