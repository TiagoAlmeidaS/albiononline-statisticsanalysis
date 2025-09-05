using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AlbionFishing.Vision.Detectors;
using AlbionFishing.Vision.Configuration;
using AlbionFishing.Vision;
using Microsoft.Extensions.Logging;

namespace StatisticsAnalysisTool.Views;

/// <summary>
/// Janela principal do bot de pesca
/// </summary>
public partial class FishingWindow : Window
{
    private static bool _isWindowMaximized;
    private readonly FishingWindowViewModel _viewModel;
    private readonly ILogger<FishingWindow> _logger;
    private readonly IBobberDetector _bobberDetector;
    private readonly IVisionConfigurationService _visionConfigService;

    public FishingWindow(
        FishingWindowViewModel viewModel,
        ILogger<FishingWindow> logger,
        IBobberDetector bobberDetector,
        IVisionConfigurationService visionConfigService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _logger = logger;
        _bobberDetector = bobberDetector;
        _visionConfigService = visionConfigService;
        
        DataContext = _viewModel;
        
        // Configurar captura de tela
        SetupScreenCapture();
        
        // Configurar teclas de atalho
        SetupKeyboardShortcuts();
    }

    private void SetupScreenCapture()
    {
        try
        {
            // Configurar captura de tela para detecção do bobber
            _viewModel.StartScreenCapture();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar captura de tela");
        }
    }

    private void SetupKeyboardShortcuts()
    {
        // F5 - Parar Bot
        var stopCommand = new System.Windows.Input.RoutedCommand();
        stopCommand.InputGestures.Add(new KeyGesture(Key.F5));
        CommandBindings.Add(new CommandBinding(stopCommand, (s, e) => _viewModel.StopBotCommand.Execute(null)));
        
        // F6 - Iniciar Bot
        var startCommand = new System.Windows.Input.RoutedCommand();
        startCommand.InputGestures.Add(new KeyGesture(Key.F6));
        CommandBindings.Add(new CommandBinding(startCommand, (s, e) => _viewModel.StartBotCommand.Execute(null)));
        
        // F7 - Testar Detecção
        var testCommand = new System.Windows.Input.RoutedCommand();
        testCommand.InputGestures.Add(new KeyGesture(Key.F7));
        CommandBindings.Add(new CommandBinding(testCommand, (s, e) => TestDetection_Click(null, null)));
    }

    #region Event Handlers

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _viewModel.StopBotCommand.Execute(null);
            _viewModel.StopScreenCapture();
            Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fechar janela de pesca");
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isWindowMaximized)
        {
            RestoreWindow();
        }
        else
        {
            MaximizeWindow();
        }
    }

    private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && WindowState == WindowState.Normal)
        {
            WindowState = WindowState.Maximized;
            return;
        }

        if (e.ClickCount == 2 && WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ZoomIn();
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ZoomOut();
    }

    private void ResetZoom_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ResetZoom();
    }

    private void CaptureScreen_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _viewModel.CaptureScreen();
            _logger.LogInformation("Captura de tela executada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao capturar tela");
        }
    }

    private void TestDetection_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _viewModel.TestBobberDetection();
            _logger.LogInformation("Teste de detecção executado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao testar detecção");
        }
    }

    private void ClearLog_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearActivityLog();
    }

    #endregion

    #region Window Management

    private void MaximizeWindow()
    {
        WindowState = WindowState.Maximized;
        _isWindowMaximized = true;
        var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
        MaxHeight = screen.WorkingArea.Height;

        Visibility = Visibility.Hidden;
        ResizeMode = ResizeMode.NoResize;
        Visibility = Visibility.Visible;
        MaximizedButton.Content = "2";
    }

    private void RestoreWindow()
    {
        WindowState = WindowState.Normal;
        _isWindowMaximized = false;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        MaximizedButton.Content = "1";
    }

    #endregion

    #region Override Methods

    protected override void OnClosed(EventArgs e)
    {
        try
        {
            _viewModel?.StopScreenCapture();
            _viewModel?.StopBotCommand?.Execute(null);
            base.OnClosed(e);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fechar janela");
        }
    }

    #endregion
}
