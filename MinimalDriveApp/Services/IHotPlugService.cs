namespace MinimalDriveApp.Services;

public interface IHotPlugService : IDisposable
{
    event EventHandler<string> DriveConnected;
    event EventHandler<string> DriveDisconnected;

    void Start();
}
