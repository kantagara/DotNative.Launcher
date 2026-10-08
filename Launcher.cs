using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNative.Launcher;

public interface ILauncher
{
    Task OpenAsync(Uri uri, CancellationToken cancellationToken = default);
}

public sealed class SystemLauncher : ILauncher
{
    private readonly PresentationTarget target;

    public SystemLauncher(PresentationTarget? target = null)
    {
        this.target = target ?? PresentationTarget.Local;
        PlatformGuard.Desktop(this.target);
    }

    public Task OpenAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        cancellationToken.ThrowIfCancellationRequested();
        if (
            !uri.IsAbsoluteUri
            || uri.UserInfo.Length != 0
            || uri.Scheme.ToLowerInvariant() is not ("https" or "http" or "mailto" or "tel")
        )
            throw new ArgumentException(
                "Only absolute HTTP, HTTPS, mailto and tel URLs can be opened.",
                nameof(uri)
            );
        var start = target.Platform switch
        {
            NativePlatform.MacOS => new ProcessStartInfo("/usr/bin/open"),
            NativePlatform.Linux => new ProcessStartInfo("xdg-open"),
            NativePlatform.Windows => new ProcessStartInfo(uri.AbsoluteUri)
            {
                UseShellExecute = true,
            },
            _ => throw new PlatformNotSupportedException(),
        };
        if (target.Platform != NativePlatform.Windows)
            start.ArgumentList.Add(uri.AbsoluteUri);
        var process =
            Process.Start(start)
            ?? throw new IOException("The operating system did not start a URL handler.");
        if (target.Platform == NativePlatform.Windows)
            process.Dispose();
        else
            _ = Reap(process);
        return Task.CompletedTask;
    }

    private static async Task Reap(Process p)
    {
        using (p)
        {
            try
            {
                await p.WaitForExitAsync().ConfigureAwait(false);
            }
            catch { }
        }
    }
}

public static class LauncherServices
{
    public static IServiceCollection AddLauncher(this IServiceCollection services)
    {
        services.TryAddSingleton<ILauncher>(p => new SystemLauncher(
            p.GetService<PresentationTarget>()
        ));
        return services;
    }
}
