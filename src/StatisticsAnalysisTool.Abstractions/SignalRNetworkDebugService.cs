using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Abstractions;

public class SignalRNetworkDebugService
{
    private static SignalRNetworkDebugService? _instance;
    private static readonly object _lock = new object();
    private bool _isConnected = false;
    
    // Filtros de eventos
    private bool _showEvents = true;
    private bool _showRequests = true;
    private bool _showResponses = true;

    public static SignalRNetworkDebugService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new SignalRNetworkDebugService();
                }
            }
            return _instance;
        }
    }

    private SignalRNetworkDebugService()
    {
        System.Diagnostics.Debug.WriteLine("SignalRNetworkDebugService inicializado");
        // Auto-start para garantir que está conectado
        _ = StartAsync();
    }

    public async Task StartAsync()
    {
        try
        {
            _isConnected = true;
            System.Diagnostics.Debug.WriteLine("SignalRNetworkDebugService conectado");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao conectar SignalRNetworkDebugService: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        try
        {
            _isConnected = false;
            System.Diagnostics.Debug.WriteLine("SignalRNetworkDebugService desconectado");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao desconectar SignalRNetworkDebugService: {ex.Message}");
        }
    }

    public async Task SendLogEntryAsync(string type, string message, string color)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"SignalR SendLogEntryAsync chamado - Connected: {_isConnected}, Type: {type}");
            
            if (!_isConnected)
            {
                System.Diagnostics.Debug.WriteLine($"SignalR não conectado - Log não enviado: {type} - {message}");
                await Task.CompletedTask;
                return;
            }

            // Aplicar filtros baseado no tipo
            bool shouldSend = type switch
            {
                "OnEvent" => _showEvents,
                "OnRequest" => _showRequests,
                "OnResponse" => _showResponses,
                _ => true // Outros tipos sempre enviados (como "Test")
            };

            if (!shouldSend)
            {
                System.Diagnostics.Debug.WriteLine($"Log filtrado - Type: {type} não está habilitado");
                await Task.CompletedTask;
                return;
            }

            // Disparar evento local (simulando SignalR)
            OnLogEntryReceived?.Invoke(type, message, color);
            System.Diagnostics.Debug.WriteLine($"Log enviado via SignalR: {type} - {message}");
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao enviar log via SignalR: {ex.Message}");
        }
    }

    public bool IsConnected => _isConnected;

    // Métodos para controlar filtros
    public void SetEventFilter(bool showEvents, bool showRequests, bool showResponses)
    {
        _showEvents = showEvents;
        _showRequests = showRequests;
        _showResponses = showResponses;
        System.Diagnostics.Debug.WriteLine($"Filtros atualizados - Events: {_showEvents}, Requests: {_showRequests}, Responses: {_showResponses}");
    }

    public void SetShowEvents(bool show)
    {
        _showEvents = show;
        System.Diagnostics.Debug.WriteLine($"Filtro Events: {_showEvents}");
    }

    public void SetShowRequests(bool show)
    {
        _showRequests = show;
        System.Diagnostics.Debug.WriteLine($"Filtro Requests: {_showRequests}");
    }

    public void SetShowResponses(bool show)
    {
        _showResponses = show;
        System.Diagnostics.Debug.WriteLine($"Filtro Responses: {_showResponses}");
    }

    public event Action<string, string, string>? OnLogEntryReceived;
}
