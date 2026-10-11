using System.Management;

namespace ScreenControls.Services;

internal sealed class BrightnessController
{
    private const string WmiNamespace = @"\\.\root\WMI";

    public int GetBrightness()
    {
        var scope = new ManagementScope(WmiNamespace);
        scope.Connect();
        using var searcher = new ManagementObjectSearcher(scope,
            new ObjectQuery("SELECT CurrentBrightness FROM WmiMonitorBrightness WHERE Active = TRUE"));
        using var results = searcher.Get();

        foreach (ManagementObject monitor in results)
        {
            using (monitor)
                return Convert.ToInt32(monitor["CurrentBrightness"]);
        }

        throw new NotSupportedException("O Windows não encontrou uma tela integrada com controle de brilho compatível.");
    }

    public void SetBrightness(byte brightness)
    {
        var scope = new ManagementScope(WmiNamespace);
        scope.Connect();
        using var searcher = new ManagementObjectSearcher(scope,
            new ObjectQuery("SELECT * FROM WmiMonitorBrightnessMethods WHERE Active = TRUE"));
        using var results = searcher.Get();

        var applied = false;
        foreach (ManagementObject monitor in results)
        {
            using (monitor)
            using (var method = monitor.GetMethodParameters("WmiSetBrightness"))
            {
                method["Timeout"] = 1;
                method["Brightness"] = brightness;
                using var output = monitor.InvokeMethod("WmiSetBrightness", method, null);
                var returnCode = Convert.ToUInt32(output?["ReturnValue"] ?? 1u);
                if (returnCode != 0)
                    throw new InvalidOperationException($"O Windows recusou o ajuste de brilho (código {returnCode}).");
                applied = true;
            }
        }

        if (!applied)
            throw new NotSupportedException("O Windows não encontrou uma tela compatível com controle de brilho.");
    }
}
