using System;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Resultado da execução de um comportamento
    /// </summary>
    public class BehaviorResult
    {
        /// <summary>
        /// Se a execução foi bem-sucedida
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Mensagem de resultado
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Dados adicionais do resultado
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new();
        
        /// <summary>
        /// Tempo de execução
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        
        /// <summary>
        /// Timestamp da execução
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Cria um resultado de sucesso
        /// </summary>
        public static BehaviorResult CreateSuccess(string message = "", Dictionary<string, object>? data = null)
        {
            return new BehaviorResult
            {
                Success = true,
                Message = message,
                Data = data ?? new Dictionary<string, object>()
            };
        }
        
        /// <summary>
        /// Cria um resultado de falha
        /// </summary>
        public static BehaviorResult CreateFailure(string message, Dictionary<string, object>? data = null)
        {
            return new BehaviorResult
            {
                Success = false,
                Message = message,
                Data = data ?? new Dictionary<string, object>()
            };
        }
    }
}
