using MinimalDriveApp.Data;
using MinimalDriveApp.Models;
using MinimalDriveApp.Services;
using MinimalDriveApp.ViewModels;
using Moq;

namespace MinimalDriveApp.Tests.Services;

public class ToastServiceTriggerTests
{
    private static DriveInfo MakeDrive(string serial, string letter = "E:") =>
        new() { SerialNumber = serial, DriveLetter = letter };

    private static (MainViewModel vm, Mock<IDriveDetectionService> detection,
                    Mock<IDriveRepository> repo, Mock<IHotPlugService> hotPlug,
                    Mock<IToastService> toast)
        Build(IEnumerable<DriveInfo>? drives = null)
    {
        var detection = new Mock<IDriveDetectionService>();
        detection.Setup(s => s.GetConnectedDrives())
                 .Returns((drives ?? Array.Empty<DriveInfo>()).ToList().AsReadOnly());

        var repo = new Mock<IDriveRepository>();
        var hotPlug = new Mock<IHotPlugService>();
        var toast = new Mock<IToastService>();

        var vm = new MainViewModel(detection.Object, repo.Object, hotPlug.Object, toast.Object);
        return (vm, detection, repo, hotPlug, toast);
    }

    [Fact]
    public void Toast_IsShown_WhenBrandNewDriveConnectsViaHotPlug()
    {
        var drive = MakeDrive("SN-NEW", "F:");
        var (vm, detection, repo, hotPlug, toast) = Build();
        repo.Setup(r => r.GetBySerial("SN-NEW")).Returns((KnownDrive?)null);

        // initial scan returns nothing; after hot-plug WMI sees the new drive
        detection.SetupSequence(s => s.GetConnectedDrives())
                 .Returns(Array.Empty<DriveInfo>().ToList().AsReadOnly())
                 .Returns(new[] { drive }.ToList().AsReadOnly());

        vm.Initialize();
        hotPlug.Raise(h => h.DriveConnected += null, vm, "SN-NEW");

        toast.Verify(t => t.ShowNewDriveAlert("F:", "SN-NEW"), Times.Once);
    }

    [Fact]
    public void Toast_IsNotShown_WhenKnownNamedDriveConnects()
    {
        var drive = MakeDrive("SN-NAMED", "G:");
        var (vm, detection, repo, hotPlug, toast) = Build();
        repo.Setup(r => r.GetBySerial("SN-NAMED"))
            .Returns(new KnownDrive { SerialNumber = "SN-NAMED", UserName = "Backup" });
        detection.Setup(s => s.GetConnectedDrives()).Returns(new[] { drive }.ToList().AsReadOnly());

        vm.Initialize();
        hotPlug.Raise(h => h.DriveConnected += null, vm, "SN-NAMED");

        toast.Verify(t => t.ShowNewDriveAlert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Toast_IsNotShown_WhenPreviouslySeenUnnamedDriveConnects()
    {
        var drive = MakeDrive("SN-OLD", "H:");
        var (vm, detection, repo, hotPlug, toast) = Build();
        repo.Setup(r => r.GetBySerial("SN-OLD"))
            .Returns(new KnownDrive { SerialNumber = "SN-OLD", UserName = null });
        detection.Setup(s => s.GetConnectedDrives()).Returns(new[] { drive }.ToList().AsReadOnly());

        vm.Initialize();
        hotPlug.Raise(h => h.DriveConnected += null, vm, "SN-OLD");

        toast.Verify(t => t.ShowNewDriveAlert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Toast_IsNotShown_OnInitialLoad_EvenForBrandNewDrive()
    {
        var drive = MakeDrive("SN-STARTUP", "D:");
        var (vm, _, repo, _, toast) = Build(new[] { drive });
        repo.Setup(r => r.GetBySerial("SN-STARTUP")).Returns((KnownDrive?)null);

        vm.Initialize();

        // toast only fires on hot-plug connect, not on initial scan
        toast.Verify(t => t.ShowNewDriveAlert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
