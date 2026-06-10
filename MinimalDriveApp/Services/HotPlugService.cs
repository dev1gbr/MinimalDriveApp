using Microsoft.Management.Infrastructure;

namespace MinimalDriveApp.Services;

public sealed class HotPlugService : IHotPlugService
{
    private CimSession? _session;
    private IDisposable? _connectSubscription;
    private IDisposable? _disconnectSubscription;

    public event EventHandler<string>? DriveConnected;
    public event EventHandler<string>? DriveDisconnected;

    // WQL polls every 2 seconds — lowest practical interval without flooding WMI
    private const string ConnectQuery =
        "SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_DiskDrive'";

    private const string DisconnectQuery =
        "SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_DiskDrive'";

    public void Start()
    {
        _session = CimSession.Create("localhost");

        _connectSubscription = _session
            .SubscribeAsync(@"root\cimv2", "WQL", ConnectQuery)
            .Subscribe(new CimObserver(OnDriveConnected));

        _disconnectSubscription = _session
            .SubscribeAsync(@"root\cimv2", "WQL", DisconnectQuery)
            .Subscribe(new CimObserver(OnDriveDisconnected));
    }

    private void OnDriveConnected(CimSubscriptionResult result)
    {
        var serial = ExtractSerial(result);
        if (serial is null) return;
        App.Current.Dispatcher.BeginInvoke(() => DriveConnected?.Invoke(this, serial));
    }

    private void OnDriveDisconnected(CimSubscriptionResult result)
    {
        var serial = ExtractSerial(result);
        if (serial is null) return;
        App.Current.Dispatcher.BeginInvoke(() => DriveDisconnected?.Invoke(this, serial));
    }

    private static string? ExtractSerial(CimSubscriptionResult result)
    {
        var target = result.Instance.CimInstanceProperties["TargetInstance"]?.Value as CimInstance;
        return target?.CimInstanceProperties["SerialNumber"]?.Value?.ToString()?.Trim();
    }

    public void Dispose()
    {
        _connectSubscription?.Dispose();
        _disconnectSubscription?.Dispose();
        _session?.Dispose();
    }

    private sealed class CimObserver : IObserver<CimSubscriptionResult>
    {
        private readonly Action<CimSubscriptionResult> _onNext;
        public CimObserver(Action<CimSubscriptionResult> onNext) => _onNext = onNext;
        public void OnNext(CimSubscriptionResult value) => _onNext(value);
        public void OnError(Exception error) { }
        public void OnCompleted() { }
    }
}
