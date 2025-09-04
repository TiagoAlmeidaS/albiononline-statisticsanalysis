using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Abstractions;

public class NetworkDebugService
{
    private static NetworkDebugService? _instance;
    private static readonly object _lock = new object();
    private bool _isConnected = false;

    public static NetworkDebugService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new NetworkDebugService();
                }
            }
            return _instance;
        }
    }

    private NetworkDebugService()
    {
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("NetworkDebugService inicializado");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao inicializar NetworkDebugService: {ex.Message}");
        }
    }

    public async Task StartAsync()
    {
        try
        {
            _isConnected = true;
            System.Diagnostics.Debug.WriteLine("NetworkDebugService conectado");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao conectar NetworkDebugService: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        try
        {
            _isConnected = false;
            System.Diagnostics.Debug.WriteLine("NetworkDebugService desconectado");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao desconectar NetworkDebugService: {ex.Message}");
        }
    }

    public async Task SendLogEntryAsync(string type, string message, string color)
    {
        try
        {
            if (_isConnected)
            {
                // Disparar evento local
                OnLogEntryReceived?.Invoke(type, message, color);
                System.Diagnostics.Debug.WriteLine($"Log enviado via NetworkDebugService: {type} - {message}");
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao enviar log: {ex.Message}");
        }
    }

    public bool IsConnected => _isConnected;

    public event Action<string, string, string>? OnLogEntryReceived;
}
