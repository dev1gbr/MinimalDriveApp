using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MinimalDriveApp.Data;
using MinimalDriveApp.Models;
using MinimalDriveApp.Services;

namespace MinimalDriveApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDriveDetectionService _detection;
    private readonly IDriveRepository _repository;
    private readonly IHotPlugService _hotPlug;

    [ObservableProperty]
    private ObservableCollection<DriveInfo> _drives = new();

    public MainViewModel(
        IDriveDetectionService detection,
        IDriveRepository repository,
        IHotPlugService hotPlug)
    {
        _detection = detection;
        _repository = repository;
        _hotPlug = hotPlug;

        _hotPlug.DriveConnected += OnDriveConnected;
        _hotPlug.DriveDisconnected += OnDriveDisconnected;
    }

    public void Initialize()
    {
        LoadDrives();
        _hotPlug.Start();
    }

    private void LoadDrives()
    {
        var wmiDrives = _detection.GetConnectedDrives();
        var enriched = wmiDrives.Select(Enrich).ToList();
        Drives = new ObservableCollection<DriveInfo>(enriched);
    }

    private void OnDriveConnected(object? sender, string serial)
    {
        _repository.Upsert(serial);
        var wmiDrives = _detection.GetConnectedDrives();
        var drive = wmiDrives.FirstOrDefault(d => d.SerialNumber == serial);
        if (drive is null) return;

        var existing = Drives.FirstOrDefault(d => d.SerialNumber == serial);
        if (existing is not null) return;

        Drives.Add(Enrich(drive));
    }

    private void OnDriveDisconnected(object? sender, string serial)
    {
        var drive = Drives.FirstOrDefault(d => d.SerialNumber == serial);
        if (drive is not null)
            Drives.Remove(drive);
    }

    private DriveInfo Enrich(DriveInfo drive)
    {
        _repository.Upsert(drive.SerialNumber);
        var known = _repository.GetBySerial(drive.SerialNumber);

        drive.Status = known switch
        {
            { UserName: { Length: > 0 } } => DriveStatus.KnownNamedDrive,
            { UserName: _ } => DriveStatus.PreviouslySeenUnnamed,
            _ => DriveStatus.BrandNewNeverSeen
        };

        drive.LastUpdated = known?.LastUpdated;
        drive.LastBackedUp = known?.LastBackedUp;
        return drive;
    }
}
