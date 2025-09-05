using System.Drawing;

namespace StatisticsAnalysisTool.Fishing.Interfaces
{
    /// <summary>
    /// Interface para detecção de bobber
    /// </summary>
    public interface IBobberDetector
    {
        /// <summary>
        /// Detecta o bobber na imagem fornecida
        /// </summary>
        /// <param name="image">Imagem para análise</param>
        /// <returns>Resultado da detecção</returns>
        BobberDetectionResult DetectBobber(Bitmap image);
    }
    
    /// <summary>
    /// Resultado da detecção do bobber
    /// </summary>
    public class BobberDetectionResult
    {
        public bool IsDetected { get; set; }
        public Point Position { get; set; }
        public Size Size { get; set; }
        public float Confidence { get; set; }
    }
}
