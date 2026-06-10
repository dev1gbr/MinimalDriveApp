using MinimalDriveApp.Data;
using MinimalDriveApp.Models;
using MinimalDriveApp.Services;
using MinimalDriveApp.ViewModels;
using Moq;

namespace MinimalDriveApp.Tests.ViewModels;

public class MainViewModelTests
{
    private static DriveInfo MakeDrive(string serial) => new() { SerialNumber = serial };

    private static (MainViewModel vm, Mock<IDriveDetectionService> detection, Mock<IDriveRepository> repo, Mock<IHotPlugService> hotPlug)
        Build(IEnumerable<DriveInfo>? drives = null)
    {
        var detection = new Mock<IDriveDetectionService>();
        detection.Setup(s => s.GetConnectedDrives())
                 .Returns((drives ?? Array.Empty<DriveInfo>()).ToList().AsReadOnly());

        var repo = new Mock<IDriveRepository>();
        var hotPlug = new Mock<IHotPlugService>();
        var toast = new Mock<IToastService>();

        var vm = new MainViewModel(detection.Object, repo.Object, hotPlug.Object, toast.Object);
        return (vm, detection, repo, hotPlug);
    }

    // --- Status assignment ---

    [Fact]
    public void LoadDrives_AssignsBrandNewNeverSeen_WhenSerialNotInDb()
    {
        var (vm, _, repo, _) = Build(new[] { MakeDrive("SN-NEW") });
        repo.Setup(r => r.GetBySerial("SN-NEW")).Returns((KnownDrive?)null);

        vm.Initialize();

        Assert.Single(vm.Drives);
        Assert.Equal(DriveStatus.BrandNewNeverSeen, vm.Drives[0].Status);
    }

    [Fact]
    public void LoadDrives_AssignsPreviouslySeenUnnamed_WhenUserNameIsNull()
    {
        var (vm, _, repo, _) = Build(new[] { MakeDrive("SN-OLD") });
        repo.Setup(r => r.GetBySerial("SN-OLD"))
            .Returns(new KnownDrive { SerialNumber = "SN-OLD", UserName = null });

        vm.Initialize();

        Assert.Equal(DriveStatus.PreviouslySeenUnnamed, vm.Drives[0].Status);
    }

    [Fact]
    public void LoadDrives_AssignsPreviouslySeenUnnamed_WhenUserNameIsEmpty()
    {
        var (vm, _, repo, _) = Build(new[] { MakeDrive("SN-OLD") });
        repo.Setup(r => r.GetBySerial("SN-OLD"))
            .Returns(new KnownDrive { SerialNumber = "SN-OLD", UserName = "" });

        vm.Initialize();

        Assert.Equal(DriveStatus.PreviouslySeenUnnamed, vm.Drives[0].Status);
    }

    [Fact]
    public void LoadDrives_AssignsKnownNamedDrive_WhenUserNameIsSet()
    {
        var (vm, _, repo, _) = Build(new[] { MakeDrive("SN-NAMED") });
        repo.Setup(r => r.GetBySerial("SN-NAMED"))
            .Returns(new KnownDrive { SerialNumber = "SN-NAMED", UserName = "My Backup" });

        vm.Initialize();

        Assert.Equal(DriveStatus.KnownNamedDrive, vm.Drives[0].Status);
    }

    // --- Hot-plug: connect ---

    [Fact]
    public void OnDriveConnected_AddsDriveToCollection()
    {
        var newDrive = MakeDrive("SN-HOT");
        var (vm, detection, repo, hotPlug) = Build();
        repo.Setup(r => r.GetBySerial("SN-HOT")).Returns((KnownDrive?)null);
        detection.Setup(s => s.GetConnectedDrives()).Returns(new[] { newDrive }.ToList().AsReadOnly());

        vm.Initialize();
        hotPlug.Raise(h => h.DriveConnected += null, vm, "SN-HOT");

        Assert.Single(vm.Drives);
        Assert.Equal("SN-HOT", vm.Drives[0].SerialNumber);
    }

    [Fact]
    public void OnDriveConnected_DoesNotDuplicate_IfAlreadyPresent()
    {
        var drive = MakeDrive("SN-DUP");
        var (vm, detection, repo, hotPlug) = Build(new[] { drive });
        repo.Setup(r => r.GetBySerial("SN-DUP")).Returns((KnownDrive?)null);
        detection.Setup(s => s.GetConnectedDrives()).Returns(new[] { drive }.ToList().AsReadOnly());

        vm.Initialize();
        hotPlug.Raise(h => h.DriveConnected += null, vm, "SN-DUP");

        Assert.Single(vm.Drives);
    }

    // --- Hot-plug: disconnect ---

    [Fact]
    public void OnDriveDisconnected_RemovesDriveFromCollection()
    {
        var drive = MakeDrive("SN-GONE");
        var (vm, _, repo, hotPlug) = Build(new[] { drive });
        repo.Setup(r => r.GetBySerial("SN-GONE")).Returns((KnownDrive?)null);

        vm.Initialize();
        hotPlug.Raise(h => h.DriveDisconnected += null, vm, "SN-GONE");

        Assert.Empty(vm.Drives);
    }

    [Fact]
    public void OnDriveDisconnected_DoesNothing_IfSerialNotInCollection()
    {
        var (vm, _, _, hotPlug) = Build();
        vm.Initialize();

        // should not throw
        hotPlug.Raise(h => h.DriveDisconnected += null, vm, "SN-MISSING");

        Assert.Empty(vm.Drives);
    }

    // --- Upsert called ---

    [Fact]
    public void LoadDrives_CallsUpsert_ForEachDrive()
    {
        var drives = new[] { MakeDrive("SN-A"), MakeDrive("SN-B") };
        var (vm, _, repo, _) = Build(drives);

        vm.Initialize();

        repo.Verify(r => r.Upsert("SN-A"), Times.Once);
        repo.Verify(r => r.Upsert("SN-B"), Times.Once);
    }
}
