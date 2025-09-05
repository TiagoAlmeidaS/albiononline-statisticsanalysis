using System;

namespace StatisticsAnalysisTool.Fishing.Utils
{
    /// <summary>
    /// Utilitários para análise visual do bobber
    /// </summary>
    public static class VisionUtils
    {
        /// <summary>
        /// Determina o lado do bobber baseado na posição
        /// </summary>
        /// <param name="posX">Posição X do bobber</param>
        /// <param name="width">Largura da área de detecção</param>
        /// <returns>Lado do bobber (esquerda ou direita)</returns>
        public static string GetBobberLado(float posX, float width)
        {
            var middle = width / 2.0f;
            return posX < middle ? "esquerda" : "direita";
        }
        
        /// <summary>
        /// Obtém um indicador visual do lado do bobber
        /// </summary>
        /// <param name="posX">Posição X do bobber</param>
        /// <param name="width">Largura da área de detecção</param>
        /// <returns>Indicador visual (← ou →)</returns>
        public static string GetBobberSideIndicator(float posX, float width)
        {
            var middle = width / 2.0f;
            return posX < middle ? "←" : "→";
        }
        
        /// <summary>
        /// Calcula a distância do bobber do centro
        /// </summary>
        /// <param name="posX">Posição X do bobber</param>
        /// <param name="width">Largura da área de detecção</param>
        /// <returns>Distância em pixels do centro</returns>
        public static float GetDistanceFromCenter(float posX, float width)
        {
            var middle = width / 2.0f;
            return Math.Abs(posX - middle);
        }
        
        /// <summary>
        /// Determina se o mouse deve ser pressionado baseado na posição do bobber
        /// </summary>
        /// <param name="posX">Posição X do bobber</param>
        /// <param name="width">Largura da área de detecção</param>
        /// <returns>True se deve pressionar o mouse</returns>
        public static bool ShouldHoldMouse(float posX, float width)
        {
            var middle = width / 2.0f;
            return posX < middle;
        }
        
        /// <summary>
        /// Calcula a porcentagem de posição do bobber na área
        /// </summary>
        /// <param name="posX">Posição X do bobber</param>
        /// <param name="width">Largura da área de detecção</param>
        /// <returns>Porcentagem de 0 a 100</returns>
        public static float GetPositionPercentage(float posX, float width)
        {
            if (width <= 0) return 0;
            return Math.Max(0, Math.Min(100, (posX / width) * 100));
        }
        
        /// <summary>
        /// Obtém uma descrição textual da posição do bobber
        /// </summary>
        /// <param name="posX">Posição X do bobber</param>
        /// <param name="width">Largura da área de detecção</param>
        /// <returns>Descrição da posição</returns>
        public static string GetPositionDescription(float posX, float width)
        {
            var percentage = GetPositionPercentage(posX, width);
            var side = GetBobberLado(posX, width);
            var indicator = GetBobberSideIndicator(posX, width);
            var distance = GetDistanceFromCenter(posX, width);
            
            return $"{side.ToUpper()} {indicator} ({percentage:F1}% - {distance:F1}px do centro)";
        }
    }
}
