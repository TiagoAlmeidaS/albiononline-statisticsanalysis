using System;
using StatisticsAnalysisTool.Core.Interfaces;

namespace StatisticsAnalysisTool.macOS.Platform;

public class MacOSWebViewService : IWebViewService
{
    private object? _webView;
    
    public void Initialize(object parent)
    {
        // TODO: Implementar WebView nativo do macOS ou CefSharp
        // Por enquanto, apenas armazenar a referência
        _webView = parent;
    }
    
    public void Navigate(string url)
    {
        // TODO: Implementar navegação
        Console.WriteLine($"WebView navigate to: {url}");
    }
    
    public void Dispose()
    {
        _webView = null;
    }
}
