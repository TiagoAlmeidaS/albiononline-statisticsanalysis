using StatisticsAnalysisTool.PhotonPackageParser;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Abstractions;
using System;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network;

internal sealed class AlbionParser : PhotonParser
{
    private readonly HandlersCollection _handlers = new();

    public void AddHandler<TPacket>(PacketHandler<TPacket> handler)
    {
        _handlers.Add(handler);
    }

    protected override void OnEvent(byte code, Dictionary<byte, object> parameters)
    {
        short eventCode = ParseEventCode(parameters);

        // Log debug de todos os eventos recebidos
        var eventMessage = $"OnEvent: Code={code}, EventCode={eventCode}, Parameters={string.Join(", ", parameters.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}";
        
        // Log adicional para debug
        System.Diagnostics.Debug.WriteLine($"ALBION PARSER - OnEvent chamado: {eventMessage}");

        if (eventCode <= -1)
        {
            DebugConsole.WriteInfo(typeof(AlbionParser), 
                $"OnEvent: EventCode inválido ({eventCode}) - evento ignorado", 
                "#FFA500");
            return;
        }

        var eventPacket = new EventPacket(eventCode, parameters);
        
        DebugConsole.WriteInfo(typeof(AlbionParser), 
            $"OnEvent: Processando evento {eventCode} com {parameters.Count} parâmetros", 
            "#32CD32");
        
        // Enviar via SignalR também
        _ = Task.Run(async () =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ALBION PARSER - Enviando via SignalR...");
                var signalRService = SignalRNetworkDebugService.Instance;
                await signalRService.SendLogEntryAsync("OnEvent", "EventPacket: " + eventMessage, "#FF6B6B");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao enviar via SignalR: {ex.Message}");
            }
        });

        _ = _handlers.HandleAsync(eventPacket);
    }

    protected override void OnRequest(byte operationCodeByte, Dictionary<byte, object> parameters)
    {
        short operationCode = ParseOperationCode(parameters);

        // Log debug de todas as requests recebidas
        var requestMessage = $"OnRequest: OperationCodeByte={operationCodeByte}, OperationCode={operationCode}, Parameters={string.Join(", ", parameters.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}";
        

        if (operationCode <= -1)
        {
            DebugConsole.WriteInfo(typeof(AlbionParser), 
                $"OnRequest: OperationCode inválido ({operationCode}) - request ignorada", 
                "#FFA500");
            return;
        }

        var requestPacket = new RequestPacket(operationCode, parameters);

                // Enviar via SignalR também
        _ = Task.Run(async () =>
        {
            try
            {
                var signalRService = SignalRNetworkDebugService.Instance;
                await signalRService.SendLogEntryAsync("OnRequest", requestMessage, "#4169E1");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao enviar via SignalR: {ex.Message}");
            }
        });
        
        _ = _handlers.HandleAsync(requestPacket);
    }

    protected override void OnResponse(byte operationCodeByte, short returnCode, string debugMessage, Dictionary<byte, object> parameters)
    {
        short operationCode = ParseOperationCode(parameters);

        // Log debug de todas as responses recebidas
        var responseMessage = $"OnResponse: OperationCodeByte={operationCodeByte}, ReturnCode={returnCode}, OperationCode={operationCode}, DebugMessage={debugMessage}, Parameters={string.Join(", ", parameters.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}";
        
        if (operationCode <= -1)
        {
            DebugConsole.WriteInfo(typeof(AlbionParser), 
                $"OnResponse: OperationCode inválido ({operationCode}) - response ignorada", 
                "#FFA500");
            return;
        }

        var responsePacket = new ResponsePacket(operationCode, parameters);

        _ = Task.Run(async () =>
        {
            try
            {
                var signalRService = SignalRNetworkDebugService.Instance;
                await signalRService.SendLogEntryAsync("OnResponse", responseMessage, "#9370DB");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao enviar via SignalR: {ex.Message}");
            }
        });
        
        _ = _handlers.HandleAsync(responsePacket);
    }

    private static short ParseOperationCode(Dictionary<byte, object> parameters)
    {
        if (!parameters.TryGetValue(253, out object value))
        {
            // Other values are returned as -1 code.
            //throw new InvalidOperationException();
            return -1;
        }

        return (short) value;
    }

    private static short ParseEventCode(Dictionary<byte, object> parameters)
    {
        if (!parameters.TryGetValue(252, out object value))
        {
            // Other values are returned as -1 code.
            //throw new InvalidOperationException();
            return -1;
        }

        return (short) value;
    }
}