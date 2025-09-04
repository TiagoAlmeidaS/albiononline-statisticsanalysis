using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;
using Serilog;

namespace StatisticsAnalysisTool.Network.Events;

public class FishingCatchEvent
{
    public readonly long? EventId;
    public readonly int? ItemIndex;
    public readonly bool? IsSuccessful;
    public readonly long? Timestamp;

    public FishingCatchEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            // Log dos parâmetros brutos recebidos
            Log.Debug("FishingCatchEvent: Parâmetros brutos recebidos: {Parameters}", 
                string.Join(", ", parameters.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
            
            DebugConsole.WriteInfo(typeof(FishingCatchEvent), 
                $"FishingCatchEvent: Parâmetros brutos: {string.Join(", ", parameters.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}", 
                "#5196A6");

            // EventId - ID do evento de pesca
            if (parameters.TryGetValue(0, out object eventId))
            {
                EventId = eventId.ObjectToLong();
                Log.Debug("FishingCatchEvent: EventId extraído: {EventId}", EventId);
            }

            // ItemIndex - Índice do item fisgado (se houver)
            if (parameters.TryGetValue(1, out object itemIndex))
            {
                ItemIndex = itemIndex.ObjectToInt();
                Log.Debug("FishingCatchEvent: ItemIndex extraído: {ItemIndex}", ItemIndex);
            }

            // IsSuccessful - Se a fisgada foi bem-sucedida
            if (parameters.TryGetValue(2, out object isSuccessful))
            {
                IsSuccessful = isSuccessful.ObjectToBool();
                Log.Debug("FishingCatchEvent: IsSuccessful extraído: {IsSuccessful}", IsSuccessful);
            }

            // Timestamp - Momento da fisgada
            if (parameters.TryGetValue(3, out object timestamp))
            {
                Timestamp = timestamp.ObjectToLong();
                Log.Debug("FishingCatchEvent: Timestamp extraído: {Timestamp}", Timestamp);
            }

            // Log consolidado do evento processado
            Log.Information("FishingCatchEvent processado: EventId={EventId}, ItemIndex={ItemIndex}, IsSuccessful={IsSuccessful}, Timestamp={Timestamp}", 
                EventId, ItemIndex, IsSuccessful, Timestamp);
            
            DebugConsole.WriteInfo(typeof(FishingCatchEvent), 
                $"FishingCatchEvent processado: EventId={EventId}, ItemIndex={ItemIndex}, IsSuccessful={IsSuccessful}, Timestamp={Timestamp}", 
                "#5196A6");
        }
        catch (Exception e)
        {
            Log.Error(e, "Erro ao processar FishingCatchEvent com parâmetros: {Parameters}", 
                string.Join(", ", parameters.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}
