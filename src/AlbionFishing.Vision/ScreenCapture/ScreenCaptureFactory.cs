using System.Runtime.InteropServices;
using System.Linq;

namespace AlbionFishing.Vision.ScreenCapture;

/// <summary>
/// Factory que cria automaticamente a implementação correta de captura baseada no SO
/// </summary>
public static class ScreenCaptureFactory
{
    private static IScreenCapture? _cachedProvider;
    private static readonly object _lock = new object();

    /// <summary>
    /// Obter o provider de captura adequado para o sistema atual
    /// </summary>
    public static IScreenCapture GetProvider()
    {
        if (_cachedProvider != null)
            return _cachedProvider;

        lock (_lock)
        {
            if (_cachedProvider != null)
                return _cachedProvider;

            _cachedProvider = CreateProvider();
            
            var now = DateTime.Now;
            Console.WriteLine($"[{now:HH:mm:ss}] 🏭 ScreenCaptureFactory: Provider criado - {_cachedProvider.ProviderName} v{_cachedProvider.Version}");
            Console.WriteLine($"   Disponível: {_cachedProvider.IsAvailable}");
            System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] ScreenCaptureFactory: Provider - {_cachedProvider.ProviderName}");

            return _cachedProvider;
        }
    }

    /// <summary>
    /// Forçar recriação do provider (útil para testes)
    /// </summary>
    public static void ResetProvider()
    {
        lock (_lock)
        {
            _cachedProvider = null;
        }
    }

    /// <summary>
    /// Listar todos os providers disponíveis
    /// </summary>
    public static IScreenCapture[] GetAllProviders()
    {
        return new IScreenCapture[]
        {
            new WindowsScreenCapture(),
            new LinuxScreenCapture(),
            // Adicionar outros providers aqui no futuro (macOS, etc.)
        };
    }

    /// <summary>
    /// Obter informações sobre o provider atual
    /// </summary>
    public static (string name, string version, bool available) GetProviderInfo()
    {
        var provider = GetProvider();
        return (provider.ProviderName, provider.Version, provider.IsAvailable);
    }

    private static IScreenCapture CreateProvider()
    {
        // Tentar criar provider específico baseado no SO
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var windowsProvider = new WindowsScreenCapture();
            if (windowsProvider.IsAvailable)
                return windowsProvider;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var linuxProvider = new LinuxScreenCapture();
            if (linuxProvider.IsAvailable)
                return linuxProvider;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // TODO: Implementar provider para macOS no futuro
            // var macProvider = new MacOSScreenCapture();
            // if (macProvider.IsAvailable)
            //     return macProvider;
        }

        // Fallback: retornar o primeiro provider disponível
        var providers = GetAllProviders();
        var availableProvider = providers.FirstOrDefault(p => p.IsAvailable);
        
        if (availableProvider != null)
            return availableProvider;

        // Último fallback: retornar provider não funcional mas que não quebra
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ⚠️ ScreenCaptureFactory: Nenhum provider disponível, usando fallback");
        return new WindowsScreenCapture(); // Mesmo que não disponível, evita null
    }
}


