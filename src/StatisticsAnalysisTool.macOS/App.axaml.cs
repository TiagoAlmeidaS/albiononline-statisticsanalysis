using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using StatisticsAnalysisTool.macOS.ViewModels;
using StatisticsAnalysisTool.macOS.Views;
using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.Core;
using StatisticsAnalysisTool.Core.Interfaces;
using StatisticsAnalysisTool.macOS.Platform;

namespace StatisticsAnalysisTool.macOS;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            // Configurar DI
            ConfigureServices();
            
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Platform services
        services.AddSingleton<IPlatformServices, MacOSPlatformServices>();
        
        // ViewModels
        services.AddTransient<StatisticsAnalysisTool.Core.ViewModels.FishingViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();
        ServiceLocator.Configure(serviceProvider);
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}