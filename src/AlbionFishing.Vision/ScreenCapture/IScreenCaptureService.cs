using System.Drawing;

namespace AlbionFishing.Vision.ScreenCapture;

/// <summary>
/// Interface para serviço de captura de tela
/// </summary>
public interface IScreenCaptureService
{
    /// <summary>
    /// Captura a tela atual
    /// </summary>
    /// <returns>Bitmap da captura de tela</returns>
    Bitmap CaptureScreen();
    
    /// <summary>
    /// Captura uma área específica da tela
    /// </summary>
    /// <param name="area">Área a ser capturada</param>
    /// <returns>Bitmap da área capturada</returns>
    Bitmap CaptureArea(Rectangle area);
    
    /// <summary>
    /// Captura uma área específica da tela com coordenadas
    /// </summary>
    /// <param name="x">Posição X</param>
    /// <param name="y">Posição Y</param>
    /// <param name="width">Largura</param>
    /// <param name="height">Altura</param>
    /// <returns>Bitmap da área capturada</returns>
    Bitmap CaptureArea(int x, int y, int width, int height);
}
