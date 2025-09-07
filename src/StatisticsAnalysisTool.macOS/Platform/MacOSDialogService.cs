using System;
using System.Diagnostics;
using StatisticsAnalysisTool.Core.Interfaces;

namespace StatisticsAnalysisTool.macOS.Platform;

public class MacOSDialogService : IDialogService
{
    public string? ShowOpenFileDialog(string title, string filter)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'tell application \"System Events\" to return POSIX path of (choose file with prompt \"{title}\")'",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        
        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        
        return process.ExitCode == 0 ? result.Trim() : null;
    }
    
    public string? ShowSaveFileDialog(string title, string filter)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'tell application \"System Events\" to return POSIX path of (choose file name with prompt \"{title}\")'",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        
        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        
        return process.ExitCode == 0 ? result.Trim() : null;
    }
    
    public void ShowMessageBox(string title, string message)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'display dialog \"{message}\" with title \"{title}\" buttons {{\"OK\"}} default button \"OK\"'",
                UseShellExecute = false
            }
        };
        
        process.Start();
    }
    
    public string? ShowFolderDialog(string title)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'tell application \"System Events\" to return POSIX path of (choose folder with prompt \"{title}\")'",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        
        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        
        return process.ExitCode == 0 ? result.Trim() : null;
    }
}
