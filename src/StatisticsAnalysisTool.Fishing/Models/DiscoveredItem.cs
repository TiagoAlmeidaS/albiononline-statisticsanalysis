using System;

namespace StatisticsAnalysisTool.Fishing.Models
{
    /// <summary>
    /// Item descoberto durante a pesca
    /// </summary>
    public class DiscoveredItem
    {
        /// <summary>
        /// ID único do item
        /// </summary>
        public long ItemId { get; set; }
        
        /// <summary>
        /// Nome do item
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Quantidade do item
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Qualidade do item
        /// </summary>
        public int Quality { get; set; }
        
        /// <summary>
        /// Nível do item
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// Tipo do item
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Categoria do item
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Valor estimado do item
        /// </summary>
        public long EstimatedValue { get; set; }
        
        /// <summary>
        /// Timestamp de quando o item foi descoberto
        /// </summary>
        public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Se o item é raro
        /// </summary>
        public bool IsRare { get; set; }
        
        /// <summary>
        /// Se o item é épico
        /// </summary>
        public bool IsEpic { get; set; }
        
        /// <summary>
        /// Se o item é lendário
        /// </summary>
        public bool IsLegendary { get; set; }
        
        /// <summary>
        /// Se o item é único
        /// </summary>
        public bool IsUnique { get; set; }
        
        /// <summary>
        /// Descrição do item
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Caminho da imagem do item
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;
    }
}
