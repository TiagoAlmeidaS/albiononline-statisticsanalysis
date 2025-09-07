using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using StatisticsAnalysisTool.Core.Interfaces;

namespace StatisticsAnalysisTool.macOS.Platform;

public class MacOSScreenCapture : IScreenCapture
{
    public bool IsAvailable => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public string ProviderName => "macOS Screencapture";
    public string Version => "1.0.0";

    public Bitmap? CaptureRegion(int x, int y, int width, int height)
    {
        if (!IsAvailable) return null;
        
        var tempFile = Path.Combine(Path.GetTempPath(), 
            $"capture_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
        
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "screencapture",
                    Arguments = $"-R {x},{y},{width},{height} \"{tempFile}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            
            process.Start();
            process.WaitForExit(3000);
            
            if (process.ExitCode == 0 && File.Exists(tempFile))
            {
                var bitmap = new Bitmap(tempFile);
                File.Delete(tempFile);
                return bitmap;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"macOS screen capture failed: {ex.Message}");
        }
        
        return null;
    }

    public Bitmap? CaptureRegion(Rectangle region)
    {
        return CaptureRegion(region.X, region.Y, region.Width, region.Height);
    }
}
