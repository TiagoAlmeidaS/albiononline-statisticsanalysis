using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Abstractions;

public static class NetworkDebugEvent
{
    public static event EventHandler<NetworkDebugEventArgs>? LogEntryAdded;

    public static void AddLogEntry(string type, string message, string color)
    {
        System.Diagnostics.Debug.WriteLine($"NETWORK DEBUG EVENT - AddLogEntry chamado: {type} - {message}");
        
        LogEntryAdded?.Invoke(null, new NetworkDebugEventArgs(type, message, color));
        
        // Também enviar via serviço se disponível
        _ = Task.Run(async () =>
        {
            try
            {
                var service = NetworkDebugService.Instance;
                await service.SendLogEntryAsync(type, message, color);
            }
            catch
            {
                // Ignorar erros se o serviço não estiver disponível
            }
        });
    }
}

public class NetworkDebugEventArgs : EventArgs
{
    public string Type { get; }
    public string Message { get; }
    public string Color { get; }

    public NetworkDebugEventArgs(string type, string message, string color)
    {
        Type = type;
        Message = message;
        Color = color;
    }
}
