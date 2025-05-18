namespace SET09102.Common.Contracts;
public interface IFirmwareService
{
    string GetNextVersion(string currentVersion);
}
