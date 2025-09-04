using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using StatisticsAnalysisTool.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;

namespace StatisticsAnalysisTool.UserControls;

public partial class NetworkDebugPanel : UserControl
{
    private readonly List<LogEntry> _logEntries = new();
    private readonly DispatcherTimer _updateTimer;
    private readonly NetworkDebugService _debugService;
    private readonly SignalRNetworkDebugService _signalRService;
    private HubConnection? _hubConnection;
    private bool _isConnected = false;

    public NetworkDebugPanel()
    {
        InitializeComponent();
        
        // Inicializar serviços de debug
        _debugService = NetworkDebugService.Instance;
        _signalRService = SignalRNetworkDebugService.Instance;
        
        // Subscrever aos eventos de debug da rede
        NetworkDebugEvent.LogEntryAdded += OnLogEntryAdded;
        _debugService.OnLogEntryReceived += OnServiceLogEntryReceived;
        _signalRService.OnLogEntryReceived += OnSignalRLogEntryReceived;
        
        // Configurar filtros iniciais
        UpdateSignalRFilters();
        
        // Subscrever aos eventos dos checkboxes
        if (ShowEventsCheckBox != null)
            ShowEventsCheckBox.Checked += (s, e) => UpdateSignalRFilters();
        if (ShowEventsCheckBox != null)
            ShowEventsCheckBox.Unchecked += (s, e) => UpdateSignalRFilters();
        if (ShowRequestsCheckBox != null)
            ShowRequestsCheckBox.Checked += (s, e) => UpdateSignalRFilters();
        if (ShowRequestsCheckBox != null)
            ShowRequestsCheckBox.Unchecked += (s, e) => UpdateSignalRFilters();
        if (ShowResponsesCheckBox != null)
            ShowResponsesCheckBox.Checked += (s, e) => UpdateSignalRFilters();
        if (ShowResponsesCheckBox != null)
            ShowResponsesCheckBox.Unchecked += (s, e) => UpdateSignalRFilters();
        
        // Subscrever ao evento Unloaded
        Unloaded += OnUnloaded;
        
        // Timer para atualizar o painel a cada 100ms
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _updateTimer.Tick += UpdateTimer_Tick;
        _updateTimer.Start();
        
        // Iniciar serviço
        _ = _debugService.StartAsync();
        
        // Inicializar conexão SignalR
        _ = InitializeSignalRAsync();
    }

    private void OnLogEntryAdded(object sender, NetworkDebugEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"NETWORK DEBUG PANEL - Evento recebido via NetworkDebugEvent: {e.Type} - {e.Message}");
        AddLogEntry(e.Type, e.Message, e.Color);
    }

    private void OnServiceLogEntryReceived(string type, string message, string color)
    {
        System.Diagnostics.Debug.WriteLine($"NETWORK DEBUG PANEL - Evento recebido via NetworkDebugService: {type} - {message}");
        AddLogEntry(type, message, color);
    }

    private void OnSignalRLogEntryReceived(string type, string message, string color)
    {
        System.Diagnostics.Debug.WriteLine($"NETWORK DEBUG PANEL - Evento recebido via SignalR: {type} - {message}");
        AddLogEntry(type, message, color);
    }

    private void UpdateSignalRFilters()
    {
        try
        {
            bool showEvents = ShowEventsCheckBox?.IsChecked == true;
            bool showRequests = ShowRequestsCheckBox?.IsChecked == true;
            bool showResponses = ShowResponsesCheckBox?.IsChecked == true;
            
            _signalRService.SetEventFilter(showEvents, showRequests, showResponses);
            System.Diagnostics.Debug.WriteLine($"Filtros SignalR atualizados - Events: {showEvents}, Requests: {showRequests}, Responses: {showResponses}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao atualizar filtros SignalR: {ex.Message}");
        }
    }

    private async Task InitializeSignalRAsync()
    {
        try
        {
            // Para um aplicativo WPF, vamos usar um servidor SignalR local
            // Por enquanto, vamos simular a conexão
            System.Diagnostics.Debug.WriteLine("SignalR - Inicializando conexão...");
            
            // Simular conexão bem-sucedida
            _isConnected = true;
            System.Diagnostics.Debug.WriteLine("SignalR - Conexão simulada estabelecida");
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao inicializar SignalR: {ex.Message}");
        }
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
        // Atualizar status da conexão
        UpdateConnectionStatus();
        
        // Não adicionar logs fixos - apenas receber dados reais via eventos
        // Os logs virão através de:
        // 1. NetworkDebugEvent.LogEntryAdded (do AlbionParser)
        // 2. _debugService.OnLogEntryReceived (via NetworkDebugService)
    }

    public void AddLogEntry(string type, string message, string color)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now.ToString("HH:mm:ss.fff"),
                Type = type,
                Message = $"[{type}] {message}",
                BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color))
            };

            _logEntries.Add(logEntry);
            
            // Criar UI element diretamente
            var border = new Border
            {
                Background = logEntry.BackgroundColor,
                Margin = new Thickness(2),
                Padding = new Thickness(5),
                CornerRadius = new CornerRadius(3)
            };

            var stackPanel = new StackPanel();
            
            var timestampText = new TextBlock
            {
                Text = logEntry.Timestamp,
                Foreground = Brushes.Gray,
                FontSize = 10
            };
            
            var messageText = new TextBlock
            {
                Text = logEntry.Message,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(timestampText);
            stackPanel.Children.Add(messageText);
            border.Child = stackPanel;
            
            LogStackPanel.Children.Add(border);
            
            // Auto scroll se habilitado
            if (AutoScrollCheckBox?.IsChecked == true)
            {
                LogScrollViewer?.ScrollToEnd();
            }
            
            // Limitar a 1000 entradas para performance
            if (_logEntries.Count > 1000)
            {
                _logEntries.RemoveAt(0);
                LogStackPanel.Children.RemoveAt(0);
            }
        }), DispatcherPriority.Normal);
    }

    public void SetConnectionStatus(bool isConnected)
    {
        _isConnected = isConnected;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            StatusText.Text = isConnected ? "● Connected" : "● Disconnected";
            StatusText.Foreground = isConnected ? Brushes.Green : Brushes.Red;
        }), DispatcherPriority.Normal);
    }

    private void UpdateConnectionStatus()
    {
        var isServiceConnected = _debugService.IsConnected;
        SetConnectionStatus(isServiceConnected);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _logEntries.Clear();
            LogStackPanel.Children.Clear();
        }), DispatcherPriority.Normal);
    }

    private void TestButton_Click(object sender, RoutedEventArgs e)
    {
        // Testar se o sistema de eventos está funcionando
        System.Diagnostics.Debug.WriteLine("TESTE - Botão Test clicado");
        
        // Simular um evento de teste
        var testMessage = $"Teste: {DateTime.Now:HH:mm:ss.fff} - Sistema funcionando";
        AddLogEntry("Test", testMessage, "#28A745");
        
        // Também testar via NetworkDebugEvent
        NetworkDebugEvent.AddLogEntry("Test", $"NetworkDebugEvent Test: {DateTime.Now:HH:mm:ss.fff}", "#FFD700");
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchTextBox.Text.ToLower();
        
        Dispatcher.BeginInvoke(new Action(() =>
        {
            for (int i = 0; i < LogStackPanel.Children.Count; i++)
            {
                var border = LogStackPanel.Children[i] as Border;
                if (border != null)
                {
                    var stackPanel = border.Child as StackPanel;
                    if (stackPanel?.Children.Count > 1)
                    {
                        var messageText = stackPanel.Children[1] as TextBlock;
                        if (messageText != null)
                        {
                            var isVisible = string.IsNullOrEmpty(searchText) || 
                                          messageText.Text.ToLower().Contains(searchText);
                            border.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
            }
        }), DispatcherPriority.Normal);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _updateTimer?.Stop();
        NetworkDebugEvent.LogEntryAdded -= OnLogEntryAdded;
        _debugService.OnLogEntryReceived -= OnServiceLogEntryReceived;
        _signalRService.OnLogEntryReceived -= OnSignalRLogEntryReceived;
        _ = _debugService.StopAsync();
        _ = _signalRService.StopAsync();
        
        // Desconectar SignalR
        _ = DisconnectSignalRAsync();
    }

    private async Task DisconnectSignalRAsync()
    {
        try
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
            _isConnected = false;
            System.Diagnostics.Debug.WriteLine("SignalR - Desconectado");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao desconectar SignalR: {ex.Message}");
        }
    }
}

public class LogEntry
{
    public string Timestamp { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Brush BackgroundColor { get; set; } = Brushes.Transparent;
}
