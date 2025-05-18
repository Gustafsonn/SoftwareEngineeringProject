using SET09102.Common.Contracts;

namespace SET09102.Common.Services;

public class FirmwareService : IFirmwareService
{
    public string GetNextVersion(string currentVersion)
    {
        var versionParts = currentVersion.Split('.');

        if (versionParts.Length == 3)
        {
            if (int.TryParse(versionParts[0], out int major) &&
                int.TryParse(versionParts[1], out int minor) &&
                int.TryParse(versionParts[2], out int patch))
            {
                major++;
                return $"{major}.0.0";
            }
        }

        throw new Exception($"The firmware version was not in the expected format - {currentVersion}");
    }
}