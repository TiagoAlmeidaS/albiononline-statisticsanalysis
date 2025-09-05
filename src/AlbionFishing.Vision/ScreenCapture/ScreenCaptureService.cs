using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace AlbionFishing.Vision.ScreenCapture;

/// <summary>
/// Implementação do serviço de captura de tela
/// </summary>
public class ScreenCaptureService : IScreenCaptureService
{
    private readonly ILogger<ScreenCaptureService> _logger;

    public ScreenCaptureService(ILogger<ScreenCaptureService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Captura a tela atual
    /// </summary>
    /// <returns>Bitmap da captura de tela</returns>
    public Bitmap CaptureScreen()
    {
        try
        {
            // Usar GetSystemMetrics para obter dimensões da tela
            var width = GetSystemMetrics(0); // SM_CXSCREEN
            var height = GetSystemMetrics(1); // SM_CYSCREEN
            var screenBounds = new Rectangle(0, 0, width, height);
            return CaptureArea(screenBounds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao capturar tela completa");
            return null;
        }
    }

    /// <summary>
    /// Captura uma área específica da tela
    /// </summary>
    /// <param name="area">Área a ser capturada</param>
    /// <returns>Bitmap da área capturada</returns>
    public Bitmap CaptureArea(Rectangle area)
    {
        try
        {
            var bitmap = new Bitmap(area.Width, area.Height, PixelFormat.Format24bppRgb);
            
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(area.X, area.Y, 0, 0, area.Size, CopyPixelOperation.SourceCopy);
            }
            
            return bitmap;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao capturar área da tela: {Area}", area);
            return null;
        }
    }

    /// <summary>
    /// Captura uma área específica da tela com coordenadas
    /// </summary>
    /// <param name="x">Posição X</param>
    /// <param name="y">Posição Y</param>
    /// <param name="width">Largura</param>
    /// <param name="height">Altura</param>
    /// <returns>Bitmap da área capturada</returns>
    public Bitmap CaptureArea(int x, int y, int width, int height)
    {
        var area = new Rectangle(x, y, width, height);
        return CaptureArea(area);
    }
    
    // P/Invoke declarations para APIs do Windows
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}
