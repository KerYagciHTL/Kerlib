namespace Kerlib.Native;

public static class PlatformGuard
{
    public static void EnsureSupported()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException($"Kerlib only supports Windows (OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}).");

        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 2))
            throw new PlatformNotSupportedException($"Kerlib needs Windows-Version > 6.1 (mind. 6.2). Found: {Environment.OSVersion.Version}");
    }
}
