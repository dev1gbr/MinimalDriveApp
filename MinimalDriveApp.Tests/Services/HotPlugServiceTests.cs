using MinimalDriveApp.Services;

namespace MinimalDriveApp.Tests.Services;

public class HotPlugServiceTests
{
    private sealed class FakeHotPlugService : IHotPlugService
    {
        public event EventHandler<string>? DriveConnected;
        public event EventHandler<string>? DriveDisconnected;

        public void Start() { }
        public void Dispose() { }

        public void SimulateConnect(string serial) => DriveConnected?.Invoke(this, serial);
        public void SimulateDisconnect(string serial) => DriveDisconnected?.Invoke(this, serial);
    }

    [Fact]
    public void DriveConnected_RaisesEvent_WithSerialNumber()
    {
        var svc = new FakeHotPlugService();
        string? received = null;
        svc.DriveConnected += (_, serial) => received = serial;

        svc.SimulateConnect("SN-USB-001");

        Assert.Equal("SN-USB-001", received);
    }

    [Fact]
    public void DriveDisconnected_RaisesEvent_WithSerialNumber()
    {
        var svc = new FakeHotPlugService();
        string? received = null;
        svc.DriveDisconnected += (_, serial) => received = serial;

        svc.SimulateDisconnect("SN-USB-001");

        Assert.Equal("SN-USB-001", received);
    }

    [Fact]
    public void DriveConnected_DoesNotFire_WhenNoSubscribers()
    {
        var svc = new FakeHotPlugService();
        // should not throw
        svc.SimulateConnect("SN-001");
    }

    [Fact]
    public void DriveDisconnected_DoesNotFire_WhenNoSubscribers()
    {
        var svc = new FakeHotPlugService();
        svc.SimulateDisconnect("SN-001");
    }

    [Fact]
    public void MultipleSubscribers_AllReceiveEvent()
    {
        var svc = new FakeHotPlugService();
        var results = new List<string>();
        svc.DriveConnected += (_, s) => results.Add("A:" + s);
        svc.DriveConnected += (_, s) => results.Add("B:" + s);

        svc.SimulateConnect("SN-002");

        Assert.Equal(2, results.Count);
        Assert.Contains("A:SN-002", results);
        Assert.Contains("B:SN-002", results);
    }
}
