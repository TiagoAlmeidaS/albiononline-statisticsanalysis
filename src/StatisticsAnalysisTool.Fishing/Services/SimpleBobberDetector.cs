using System.Drawing;
using StatisticsAnalysisTool.Fishing.Interfaces;

namespace StatisticsAnalysisTool.Fishing.Services
{
    /// <summary>
    /// Implementação simples do detector de bobber (placeholder)
    /// </summary>
    public class SimpleBobberDetector : IBobberDetector
    {
        private readonly Random _random = new Random();
        
        public BobberDetectionResult DetectBobber(Bitmap image)
        {
            // Implementação placeholder - simula detecção aleatória
            // Em uma implementação real, aqui seria feita a análise de imagem
            // usando OpenCV, Emgu.CV ou similar
            
            var shouldDetect = _random.NextDouble() > 0.3; // 70% de chance de detectar
            
            if (!shouldDetect)
            {
                return new BobberDetectionResult
                {
                    IsDetected = false
                };
            }
            
            // Simular posição aleatória do bobber
            var centerX = image.Width / 2;
            var centerY = image.Height / 2;
            var offsetX = _random.Next(-centerX / 2, centerX / 2);
            var offsetY = _random.Next(-centerY / 2, centerY / 2);
            
            return new BobberDetectionResult
            {
                IsDetected = true,
                Position = new Point(centerX + offsetX, centerY + offsetY),
                Size = new Size(20, 20), // Tamanho simulado do bobber
                Confidence = (float)(0.7 + _random.NextDouble() * 0.3) // 70-100% de confiança
            };
        }
    }
}
