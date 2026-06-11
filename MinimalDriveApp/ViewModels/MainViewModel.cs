using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MinimalDriveApp.Data;
using MinimalDriveApp.Models;
using MinimalDriveApp.Services;

namespace MinimalDriveApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDriveDetectionService _detection;
    private readonly IDriveRepository _repository;
    private readonly IHotPlugService _hotPlug;
    private readonly IToastService _toast;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<DriveInfo> _drives = new();

    [ObservableProperty]
    private DriveInfo? _selectedDrive;

    public DriveDashboardViewModel Dashboard { get; } = new();

    partial void OnSelectedDriveChanged(DriveInfo? value)
    {
        Dashboard.Drive = value;
    }

    public MainViewModel(
        IDriveDetectionService detection,
        IDriveRepository repository,
        IHotPlugService hotPlug,
        IToastService toast,
        ILogger<MainViewModel> logger)
    {
        _detection = detection;
        _repository = repository;
        _hotPlug = hotPlug;
        _toast = toast;
        _logger = logger;

        _hotPlug.DriveConnected += OnDriveConnected;
        _hotPlug.DriveDisconnected += OnDriveDisconnected;
    }

    public void Initialize()
    {
        _logger.LogInformation("MainViewModel initializing");
        LoadDrives();
        _hotPlug.Start();
    }

    private void LoadDrives()
    {
        var wmiDrives = _detection.GetConnectedDrives();
        _logger.LogInformation("Initial scan found {Count} logical drive(s)", wmiDrives.Count);
        var enriched = wmiDrives.Select(Enrich).ToList();
        Drives = new ObservableCollection<DriveInfo>(enriched);
    }

    private void OnDriveConnected(object? sender, string serial)
    {
        _logger.LogInformation("OnDriveConnected triggered for serial={Serial}", serial);
        var wmiDrives = _detection.GetConnectedDrives();
        var drive = wmiDrives.FirstOrDefault(d => d.SerialNumber == serial);
        if (drive is null)
        {
            _logger.LogWarning("Drive serial={Serial} not found in WMI scan after connect event", serial);
            return;
        }

        var existing = Drives.FirstOrDefault(d => d.SerialNumber == serial);
        if (existing is not null)
        {
            _logger.LogDebug("Drive serial={Serial} already in list, skipping", serial);
            return;
        }

        var enriched = Enrich(drive);
        Drives.Add(enriched);
        _logger.LogInformation("Drive serial={Serial} status={Status} added to list", serial, enriched.Status);

        if (enriched.Status == DriveStatus.BrandNewNeverSeen)
        {
            _logger.LogInformation("BrandNew drive serial={Serial} — firing toast", serial);
            _toast.ShowNewDriveAlert(enriched.DriveLetter, enriched.SerialNumber);
        }
        else
        {
            _logger.LogInformation("Drive serial={Serial} is {Status} — no toast", serial, enriched.Status);
        }
    }

    private void OnDriveDisconnected(object? sender, string serial)
    {
        _logger.LogInformation("OnDriveDisconnected triggered for serial={Serial}", serial);
        var drive = Drives.FirstOrDefault(d => d.SerialNumber == serial);
        if (drive is not null)
        {
            Drives.Remove(drive);
            _logger.LogInformation("Drive serial={Serial} removed from list", serial);
        }
        else
        {
            _logger.LogWarning("Drive serial={Serial} not found in list on disconnect", serial);
        }
    }

    private DriveInfo Enrich(DriveInfo drive)
    {
        // check existence BEFORE upsert — otherwise a brand-new drive would always
        // resolve to PreviouslySeenUnnamed (upsert inserts with UserName=null, then
        // GetBySerial returns a non-null KnownDrive that matches { UserName: _ })
        var preExisting = _repository.GetBySerial(drive.SerialNumber);
        _repository.Upsert(drive.SerialNumber);
        var known = _repository.GetBySerial(drive.SerialNumber);

        drive.Status = preExisting is null
            ? DriveStatus.BrandNewNeverSeen
            : known switch
            {
                { UserName: { Length: > 0 } } => DriveStatus.KnownNamedDrive,
                _ => DriveStatus.PreviouslySeenUnnamed
            };

        drive.LastUpdated = known?.LastUpdated;
        drive.LastBackedUp = known?.LastBackedUp;

        _logger.LogDebug("Enrich serial={Serial} preExisting={PreExisting} → status={Status}",
            drive.SerialNumber, preExisting is not null, drive.Status);
        return drive;
    }
}
