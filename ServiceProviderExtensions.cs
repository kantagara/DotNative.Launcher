using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotNative.Launcher;

public static class LauncherServiceProviderExtensions
{
#if NET10_0_OR_GREATER
    extension(IServiceProvider services)
    {
        /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
        public ILauncher Launcher => services.GetRequiredService<ILauncher>();
    }
#else
    /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
    public static ILauncher Launcher(this IServiceProvider services) =>
        services.GetRequiredService<ILauncher>();
#endif
}
