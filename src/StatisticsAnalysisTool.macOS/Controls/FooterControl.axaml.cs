using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;

namespace StatisticsAnalysisTool.macOS.Controls;

/// <summary>
/// Footer control for displaying status and links
/// </summary>
public partial class FooterControl : UserControl
{
    public FooterControl()
    {
        InitializeComponent();
    }

    private void GitHubLink_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/your-repo/albiononline-statisticsanalysis",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Error opening GitHub: {ex.Message}");
        }
    }

    private void DiscordLink_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/your-discord",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Error opening Discord: {ex.Message}");
        }
    }

    private void DonateLink_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.paypal.com/donate/your-donation-link",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Error opening donation link: {ex.Message}");
        }
    }
}
