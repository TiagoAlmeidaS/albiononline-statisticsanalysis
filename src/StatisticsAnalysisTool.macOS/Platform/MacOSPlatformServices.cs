using StatisticsAnalysisTool.Core.Interfaces;

namespace StatisticsAnalysisTool.macOS.Platform;

public class MacOSPlatformServices : IPlatformServices
{
    public IAutomationService Automation { get; }
    public IScreenCapture ScreenCapture { get; }
    public IWebViewService WebView { get; }
    public IDialogService Dialogs { get; }
    public INotificationService Notifications { get; }
    
    public MacOSPlatformServices()
    {
        Automation = new MacOSAutomationService();
        ScreenCapture = new MacOSScreenCapture();
        WebView = new MacOSWebViewService();
        Dialogs = new MacOSDialogService();
        Notifications = new MacOSNotificationService();
    }
}
