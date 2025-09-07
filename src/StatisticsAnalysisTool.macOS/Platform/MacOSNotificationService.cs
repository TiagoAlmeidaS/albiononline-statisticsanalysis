using System;
using System.Diagnostics;
using StatisticsAnalysisTool.Core.Interfaces;

namespace StatisticsAnalysisTool.macOS.Platform;

public class MacOSNotificationService : INotificationService
{
    public void ShowNotification(string title, string message)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'display notification \"{message}\" with title \"{title}\"'",
                UseShellExecute = false
            }
        };
        
        process.Start();
    }
    
    public void ShowError(string title, string message)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'display dialog \"{message}\" with title \"{title}\" buttons {{\"OK\"}} default button \"OK\" with icon stop'",
                UseShellExecute = false
            }
        };
        
        process.Start();
    }
    
    public void ShowSuccess(string title, string message)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'display dialog \"{message}\" with title \"{title}\" buttons {{\"OK\"}} default button \"OK\" with icon note'",
                UseShellExecute = false
            }
        };
        
        process.Start();
    }
}
