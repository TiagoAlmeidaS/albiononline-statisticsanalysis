using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace AlbionFishing.Vision.ScreenCapture;

/// <summary>
/// Implementação Linux da captura de tela usando FFmpeg
/// Suporta X11 e Wayland via FFmpeg subprocess
/// </summary>
public class LinuxScreenCapture : IScreenCapture
{
    private DateTime _lastLogTime = DateTime.Now;
    private readonly string _tempDir;
    private readonly bool _ffmpegAvailable;

    public bool IsAvailable => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && _ffmpegAvailable;
    public string ProviderName => "Linux FFmpeg";
    public string Version => "1.0.0";

    public LinuxScreenCapture()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "AlbionFishing", "screenshots");
        Directory.CreateDirectory(_tempDir);
        
        _ffmpegAvailable = CheckFFmpegAvailability();
        
        if (!_ffmpegAvailable)
        {
            LogWarning("FFmpeg não encontrado. Captura de tela não estará disponível.");
        }
    }

    public Bitmap? CaptureRegion(int x, int y, int width, int height)
    {
        if (!IsAvailable)
        {
            LogError("Linux screen capture not available on this platform");
            return null;
        }

        try
        {
            var tempFile = Path.Combine(_tempDir, $"capture_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
            
            // Tentar captura via FFmpeg
            if (CaptureWithFFmpeg(x, y, width, height, tempFile))
            {
                var bitmap = LoadBitmapFromFile(tempFile);
                if (bitmap != null)
                {
                    LogCapture(x, y, width, height);
                    return bitmap;
                }
            }
            
            // Fallback: tentar captura via X11 diretamente
            if (CaptureWithX11(x, y, width, height, tempFile))
            {
                var bitmap = LoadBitmapFromFile(tempFile);
                if (bitmap != null)
                {
                    LogCapture(x, y, width, height);
                    return bitmap;
                }
            }
            
            LogError($"Failed to capture region {x},{y} {width}x{height}");
            return null;
        }
        catch (Exception ex)
        {
            LogError($"Linux screen capture failed: {ex.Message}");
            return null;
        }
    }

    public Bitmap? CaptureRegion(Rectangle region)
    {
        return CaptureRegion(region.X, region.Y, region.Width, region.Height);
    }

    private bool CheckFFmpegAvailability()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            process.WaitForExit(3000); // 3 segundos timeout
            
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private bool CaptureWithFFmpeg(int x, int y, int width, int height, string outputFile)
    {
        try
        {
            // Detectar se estamos em Wayland ou X11
            var displayServer = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
            string arguments;
            
            if (!string.IsNullOrEmpty(displayServer))
            {
                // Wayland - usar pipewire
                arguments = $"-f pipewire -s {width}x{height} -i +{x},{y} -frames:v 1 -y \"{outputFile}\"";
            }
            else
            {
                // X11 - usar x11grab
                var display = Environment.GetEnvironmentVariable("DISPLAY") ?? ":0";
                arguments = $"-f x11grab -s {width}x{height} -i {display}+{x},{y} -frames:v 1 -y \"{outputFile}\"";
            }
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            process.WaitForExit(5000); // 5 segundos timeout
            
            return process.ExitCode == 0 && File.Exists(outputFile);
        }
        catch (Exception ex)
        {
            LogError($"FFmpeg capture failed: {ex.Message}");
            return false;
        }
    }

    private bool CaptureWithX11(int x, int y, int width, int height, string outputFile)
    {
        try
        {
            // Tentar usar import (ImageMagick) como fallback
            var arguments = $"x:{x},{y} {width}x{height} \"{outputFile}\"";
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "import",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            process.WaitForExit(3000); // 3 segundos timeout
            
            return process.ExitCode == 0 && File.Exists(outputFile);
        }
        catch (Exception ex)
        {
            LogError($"X11 capture failed: {ex.Message}");
            return false;
        }
    }

    private Bitmap? LoadBitmapFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;
                
            var bitmap = new Bitmap(filePath);
            
            // Limpar arquivo temporário
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // Ignorar erros de limpeza
            }
            
            return bitmap;
        }
        catch (Exception ex)
        {
            LogError($"Failed to load bitmap from file: {ex.Message}");
            return null;
        }
    }

    private void LogCapture(int x, int y, int width, int height)
    {
        var now = DateTime.Now;
        if ((now - _lastLogTime).TotalSeconds >= 5) // Log a cada 5 segundos
        {
            Console.WriteLine($"[{now:HH:mm:ss}] 📸 {ProviderName}: Captura - Region: {x},{y} {width}x{height}");
            System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] {ProviderName}: Captura - Region: {x},{y} {width}x{height}");
            _lastLogTime = now;
        }
    }

    private void LogWarning(string message)
    {
        var now = DateTime.Now;
        if ((now - _lastLogTime).TotalSeconds >= 5) // Log a cada 5 segundos
        {
            Console.WriteLine($"[{now:HH:mm:ss}] ⚠️ {ProviderName}: {message}");
            System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] {ProviderName}: {message}");
            _lastLogTime = now;
        }
    }

    private void LogError(string message)
    {
        var now = DateTime.Now;
        Console.WriteLine($"[{now:HH:mm:ss}] ❌ {ProviderName}: {message}");
        System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] {ProviderName} ERROR: {message}");
    }
}
