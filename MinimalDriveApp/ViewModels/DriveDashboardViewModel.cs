using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MinimalDriveApp.Models;
using SkiaSharp;

namespace MinimalDriveApp.ViewModels;

public partial class DriveDashboardViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasDrive),
        nameof(HeaderTitle),
        nameof(HeaderSubtitle),
        nameof(DriveType),
        nameof(FileSystem),
        nameof(ConnectionType),
        nameof(SerialNumber),
        nameof(HealthStatus),
        nameof(HealthIcon),
        nameof(StatusLabel),
        nameof(DonutSeries),
        nameof(CapacityFormatted),
        nameof(UsedSpaceFormatted),
        nameof(FreeSpaceFormatted),
        nameof(UsedPercentFormatted),
        nameof(IsHealthOk))]
    private DriveInfo? _drive;

    public bool HasDrive => Drive is not null;

    public string HeaderTitle    => string.IsNullOrWhiteSpace(Drive?.VolumeName)
                                        ? Drive?.DriveLetter ?? "—"
                                        : $"{Drive.VolumeName} ({Drive.DriveLetter})";
    public string HeaderSubtitle => Drive is null ? string.Empty
                                        : $"{Drive.DriveType}  ·  {Drive.FileSystem}  ·  {Drive.ConnectionType}";

    public string DriveType      => Drive?.DriveType      ?? string.Empty;
    public string FileSystem     => Drive?.FileSystem     ?? string.Empty;
    public string ConnectionType => Drive?.ConnectionType ?? string.Empty;
    public string SerialNumber   => Drive?.SerialNumber   ?? string.Empty;
    public string HealthStatus   => Drive?.HealthStatus   ?? string.Empty;
    public string HealthIcon     => IsHealthOk ? "✔" : "⚠";

    public string StatusLabel => Drive?.Status switch
    {
        DriveStatus.KnownNamedDrive       => "Known Named Drive",
        DriveStatus.PreviouslySeenUnnamed => "Previously Seen Unnamed",
        DriveStatus.BrandNewNeverSeen     => "Brand New — Never Seen",
        _                                 => "—"
    };

    public ISeries[] DonutSeries
    {
        get
        {
            double used  = Drive?.UsedSpace ?? 0;
            double free  = Drive is not null && Drive.Capacity > 0
                               ? Drive.Capacity - Drive.UsedSpace
                               : 1;
            if (Drive is null) used = 0;

            return
            [
                new PieSeries<double>
                {
                    Values              = [used],
                    Name                = "Used",
                    Fill                = new SolidColorPaint(new SKColor(0x00, 0x78, 0xD4)),
                    InnerRadius         = 52,
                    MaxRadialColumnWidth = 28,
                    DataLabelsSize      = 0,
                    ToolTipLabelFormatter = p => FormatBytes((long)p.Model)
                },
                new PieSeries<double>
                {
                    Values              = [free],
                    Name                = "Free",
                    Fill                = new SolidColorPaint(new SKColor(0x2A, 0x2A, 0x4A)),
                    InnerRadius         = 52,
                    MaxRadialColumnWidth = 28,
                    DataLabelsSize      = 0,
                    ToolTipLabelFormatter = p => FormatBytes((long)p.Model)
                }
            ];
        }
    }

    public string CapacityFormatted    => FormatBytes(Drive?.Capacity   ?? 0);
    public string UsedSpaceFormatted   => FormatBytes(Drive?.UsedSpace  ?? 0);
    public string FreeSpaceFormatted   => FormatBytes(Drive?.FreeSpace  ?? 0);

    public string UsedPercentFormatted
    {
        get
        {
            if (Drive is null || Drive.Capacity <= 0) return "—";
            double pct = (double)Drive.UsedSpace / Drive.Capacity * 100;
            return pct.ToString("F0", CultureInfo.InvariantCulture) + "%";
        }
    }

    public bool IsHealthOk => !(Drive?.IsHealthWarning ?? false);

    internal static string FormatBytes(long bytes)
    {
        if (bytes <= 0) return "0 GB";
        if (bytes >= 1_099_511_627_776L)
            return (bytes / 1_099_511_627_776.0).ToString("F1", CultureInfo.InvariantCulture) + " TB";
        if (bytes >= 1_073_741_824L)
            return (bytes / 1_073_741_824.0).ToString("F1", CultureInfo.InvariantCulture) + " GB";
        if (bytes >= 1_048_576L)
            return (bytes / 1_048_576.0).ToString("F1", CultureInfo.InvariantCulture) + " MB";
        return (bytes / 1024.0).ToString("F1", CultureInfo.InvariantCulture) + " KB";
    }
}
