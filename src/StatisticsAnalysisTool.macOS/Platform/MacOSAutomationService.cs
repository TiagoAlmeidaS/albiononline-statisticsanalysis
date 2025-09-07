using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using StatisticsAnalysisTool.Core.Interfaces;

namespace StatisticsAnalysisTool.macOS.Platform;

public class MacOSAutomationService : IAutomationService
{
    private readonly string _xdotoolPath;
    
    public MacOSAutomationService()
    {
        _xdotoolPath = FindXdotool();
    }
    
    public bool IsSupported()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && 
               !string.IsNullOrEmpty(_xdotoolPath);
    }
    
    public void MoveTo(int x, int y)
    {
        ExecuteXdotool($"mousemove {x} {y}");
    }
    
    public void Click(string button = "left")
    {
        ExecuteXdotool($"click {button}");
    }
    
    public void ClickAt(int x, int y, string button = "left")
    {
        ExecuteXdotool($"mousemove {x} {y} click {button}");
    }
    
    public void KeyPress(string key)
    {
        ExecuteXdotool($"key {key}");
    }
    
    public void TypeText(string text)
    {
        ExecuteXdotool($"type \"{text}\"");
    }
    
    private string FindXdotool()
    {
        var possiblePaths = new[]
        {
            "/usr/local/bin/xdotool",
            "/opt/homebrew/bin/xdotool",
            "xdotool"
        };

        foreach (var path in possiblePaths)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return path;
                }
            }
            catch
            {
                // Continue searching
            }
        }

        return string.Empty;
    }
    
    private string ExecuteXdotool(string arguments)
    {
        if (string.IsNullOrEmpty(_xdotoolPath))
        {
            throw new InvalidOperationException("xdotool not found. Install with: brew install xdotool");
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _xdotoolPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"xdotool failed: {process.StandardError.ReadToEnd()}");
        }

        return output;
    }
}
