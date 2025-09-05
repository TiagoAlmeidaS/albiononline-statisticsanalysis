using System;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Resultado de uma decisão
    /// </summary>
    public class DecisionResult
    {
        /// <summary>
        /// Ação a ser executada
        /// </summary>
        public string Action { get; set; } = string.Empty;
        
        /// <summary>
        /// Razão da decisão
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// Confiança na decisão (0-1)
        /// </summary>
        public double Confidence { get; set; } = 1.0;
        
        /// <summary>
        /// Prioridade da decisão
        /// </summary>
        public int Priority { get; set; } = 0;
        
        /// <summary>
        /// Se a decisão foi bem-sucedida
        /// </summary>
        public bool Success { get; set; } = true;
        
        /// <summary>
        /// Dados adicionais da decisão
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new();
        
        /// <summary>
        /// Timestamp da decisão
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Cria um resultado de sucesso
        /// </summary>
        public static DecisionResult CreateSuccess(string action, string reason = "", double confidence = 1.0)
        {
            return new DecisionResult
            {
                Action = action,
                Reason = reason,
                Confidence = confidence,
                Success = true
            };
        }
        
        /// <summary>
        /// Cria um resultado de falha
        /// </summary>
        public static DecisionResult CreateFailure(string reason, string action = "")
        {
            return new DecisionResult
            {
                Action = action,
                Reason = reason,
                Success = false,
                Confidence = 0.0
            };
        }
        
        /// <summary>
        /// Cria um resultado de não ação
        /// </summary>
        public static DecisionResult CreateNoAction(string reason = "")
        {
            return new DecisionResult
            {
                Action = "NO_ACTION",
                Reason = reason,
                Success = true,
                Confidence = 1.0
            };
        }
    }
}
