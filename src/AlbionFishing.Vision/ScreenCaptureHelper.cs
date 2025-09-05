using System.Drawing;

namespace AlbionFishing.Vision;

/// <summary>
/// Wrapper estático para manter compatibilidade com código existente
/// Delega para o novo sistema modular de ScreenCapture
/// </summary>
public static class ScreenCaptureHelper
{
    public static System.Drawing.Bitmap? CaptureRegion(int x, int y, int width, int height)
    {
        var provider = AlbionFishing.Vision.ScreenCapture.ScreenCaptureFactory.GetProvider();
        return provider.CaptureRegion(x, y, width, height);
    }

    public static System.Drawing.Bitmap? CaptureRegion(System.Drawing.Rectangle region)
    {
        var provider = AlbionFishing.Vision.ScreenCapture.ScreenCaptureFactory.GetProvider();
        return provider.CaptureRegion(region);
    }

    public static bool IsAvailable => AlbionFishing.Vision.ScreenCapture.ScreenCaptureFactory.GetProvider().IsAvailable;
    
    public static string ProviderName => AlbionFishing.Vision.ScreenCapture.ScreenCaptureFactory.GetProvider().ProviderName;
} 