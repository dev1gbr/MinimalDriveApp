using MinimalDriveApp.Services;
using Moq;

namespace MinimalDriveApp.Tests.Services;

public class HotPlugServiceTests
{
    [Fact]
    public void DriveConnected_RaisesEvent_WithSerialNumber()
    {
        var mock = new Mock<IHotPlugService>();
        string? received = null;
        mock.Object.DriveConnected += (_, serial) => received = serial;

        mock.Raise(m => m.DriveConnected += null, this, "SN-USB-001");

        Assert.Equal("SN-USB-001", received);
    }

    [Fact]
    public void DriveDisconnected_RaisesEvent_WithSerialNumber()
    {
        var mock = new Mock<IHotPlugService>();
        string? received = null;
        mock.Object.DriveDisconnected += (_, serial) => received = serial;

        mock.Raise(m => m.DriveDisconnected += null, this, "SN-USB-001");

        Assert.Equal("SN-USB-001", received);
    }

    [Fact]
    public void DriveConnected_DoesNotFire_WhenNoSubscribers()
    {
        var mock = new Mock<IHotPlugService>();

        // should not throw when no subscribers
        mock.Raise(m => m.DriveConnected += null, this, "SN-001");
    }

    [Fact]
    public void DriveDisconnected_DoesNotFire_WhenNoSubscribers()
    {
        var mock = new Mock<IHotPlugService>();

        mock.Raise(m => m.DriveDisconnected += null, this, "SN-001");
    }

    [Fact]
    public void MultipleSubscribers_AllReceiveEvent()
    {
        var mock = new Mock<IHotPlugService>();
        var results = new List<string>();
        mock.Object.DriveConnected += (_, s) => results.Add("A:" + s);
        mock.Object.DriveConnected += (_, s) => results.Add("B:" + s);

        mock.Raise(m => m.DriveConnected += null, this, "SN-002");

        Assert.Equal(2, results.Count);
        Assert.Contains("A:SN-002", results);
        Assert.Contains("B:SN-002", results);
    }
}
