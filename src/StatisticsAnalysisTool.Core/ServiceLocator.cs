using Microsoft.Extensions.DependencyInjection;

namespace StatisticsAnalysisTool.Core;

public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    public static void Configure(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static T Resolve<T>() where T : notnull
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("ServiceLocator has not been configured. Call Configure() first.");

        return _serviceProvider.GetRequiredService<T>();
    }

    public static T? ResolveOptional<T>() where T : class
    {
        if (_serviceProvider == null)
            return null;

        return _serviceProvider.GetService<T>();
    }
}
