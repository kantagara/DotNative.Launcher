# DotNative.Launcher

Opens validated external URLs with the user's default OS handler on macOS, Windows and Linux. Mobile implementations are not connected yet and throw `PlatformNotSupportedException`.

```csharp
builder.Services.AddLauncher();
await provider.Launcher.OpenAsync(new Uri("https://example.com"), cancellationToken);
```

Accepted schemes are `https`, `http`, `mailto` and `tel`; credentials in a URL are rejected. macOS dispatches through `/usr/bin/open`, Linux through `xdg-open`, and Windows through the shell URL association. Linux must have `xdg-open` and an active desktop session. The call means the OS accepted a handler launch request; it does not prove the page loaded or mail was sent. Cancellation is honored before dispatch and does not terminate an already launched browser. No shell command string is interpolated.

Build locally: `dotnet build -p:DotNativeSourceRoot=../dotNative`. The managed API compiles for macOS, Windows 11 ARM64, and Linux ARM64 (Debian 12 container). Linux `xdg-open` argument dispatch was checked with a test stub; real desktop handler launch remains unverified on all platforms.

## Service access

Import `DotNative.Launcher` to access the plugin through `IServiceProvider`:

```csharp
using DotNative.Launcher;

var plugin = services.Launcher;
```

The getter calls `GetRequiredService<ILauncher>()` on every access, preserving
DI lifetimes and the usual missing-registration error. Register the plugin with
`AddLauncher(...)` before building the provider.

A `net10.0` application uses the property syntax with C# 14 or later. A
`net9.0` application uses only the method equivalent:

```csharp
var plugin = services.Launcher();
```

The package contains separate `net9.0` and `net10.0` assemblies. NuGet selects
the assembly matching the application target framework. `NET10_0_OR_GREATER`
selects the property; the `#else` branch selects the method.

Build and pack both targets with .NET 10 SDK. A source build using .NET 9 SDK
builds only `net9.0`; it does not produce the .NET 10 assembly.
