using Microsoft.Extensions.Logging;
using Microsoft.Management.Infrastructure;

namespace MinimalDriveApp.Services;

public sealed class HotPlugService : IHotPlugService
{
    private readonly ILogger<HotPlugService> _logger;
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

    public HotPlugService(ILogger<HotPlugService> logger)
    {
        _logger = logger;
    }

    public void Start()
    {
        _logger.LogInformation("HotPlugService starting WMI subscriptions");

        // null = local machine via DCOM; avoids WinRM/WS-Man dependency
        _session = CimSession.Create(null);

        _connectSubscription = _session
            .SubscribeAsync(@"root\cimv2", "WQL", ConnectQuery)
            .Subscribe(new CimObserver(OnDriveConnected, OnSubscriptionError));

        _disconnectSubscription = _session
            .SubscribeAsync(@"root\cimv2", "WQL", DisconnectQuery)
            .Subscribe(new CimObserver(OnDriveDisconnected, OnSubscriptionError));

        _logger.LogInformation("HotPlugService WMI subscriptions active");
    }

    private void OnDriveConnected(CimSubscriptionResult result)
    {
        var serial = ExtractSerial(result);
        _logger.LogDebug("WMI DriveConnected event received, serial={Serial}", serial ?? "(null)");
        if (serial is null) return;
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            _logger.LogInformation("Raising DriveConnected for serial={Serial}", serial);
            DriveConnected?.Invoke(this, serial);
        });
    }

    private void OnDriveDisconnected(CimSubscriptionResult result)
    {
        var serial = ExtractSerial(result);
        _logger.LogDebug("WMI DriveDisconnected event received, serial={Serial}", serial ?? "(null)");
        if (serial is null) return;
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            _logger.LogInformation("Raising DriveDisconnected for serial={Serial}", serial);
            DriveDisconnected?.Invoke(this, serial);
        });
    }

    private void OnSubscriptionError(Exception ex)
    {
        _logger.LogError(ex, "WMI subscription error");
    }

    private static string? ExtractSerial(CimSubscriptionResult result)
    {
        var target = result.Instance.CimInstanceProperties["TargetInstance"]?.Value as CimInstance;
        return target?.CimInstanceProperties["SerialNumber"]?.Value?.ToString()?.Trim();
    }

    public void Dispose()
    {
        _logger.LogInformation("HotPlugService disposing");
        _connectSubscription?.Dispose();
        _disconnectSubscription?.Dispose();
        _session?.Dispose();
    }

    private sealed class CimObserver : IObserver<CimSubscriptionResult>
    {
        private readonly Action<CimSubscriptionResult> _onNext;
        private readonly Action<Exception> _onError;

        public CimObserver(Action<CimSubscriptionResult> onNext, Action<Exception> onError)
        {
            _onNext = onNext;
            _onError = onError;
        }

        public void OnNext(CimSubscriptionResult value) => _onNext(value);
        public void OnError(Exception error) => _onError(error);
        public void OnCompleted() { }
    }
}
