using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AlbionFishing.Vision.ScreenCapture;

/// <summary>
/// Implementação Windows da captura de tela usando GDI+
/// </summary>
public class WindowsScreenCapture : IScreenCapture
{
    private DateTime _lastLogTime = DateTime.Now;

    public bool IsAvailable => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public string ProviderName => "Windows GDI+";
    public string Version => "1.0.0";

    public Bitmap? CaptureRegion(int x, int y, int width, int height)
    {
        if (!IsAvailable)
        {
            LogError("Windows screen capture not available on this platform");
            return null;
        }

        try
        {
            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(x, y, 0, 0, new Size(width, height));
            
            LogCapture(x, y, width, height);
            
            return new Bitmap(bitmap);
        }
        catch (Exception ex)
        {
            LogError($"Windows screen capture failed: {ex.Message}");
            return null;
        }
    }

    public Bitmap? CaptureRegion(Rectangle region)
    {
        return CaptureRegion(region.X, region.Y, region.Width, region.Height);
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

    private void LogError(string message)
    {
        var now = DateTime.Now;
        Console.WriteLine($"[{now:HH:mm:ss}] ❌ {ProviderName}: {message}");
        System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] {ProviderName} ERROR: {message}");
    }
}
