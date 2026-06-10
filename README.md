# MinimalDriveApp

A minimal WPF desktop demo application for real-time Windows drive detection and monitoring.

## What it does

- **Automatic drive detection** — scans all connected drives on startup with no manual refresh required
- **Real-time hot-plug** — drives appear and disappear in the list automatically within seconds of being connected or removed
- **Drive table** — displays drive letter, label, type, file system, capacity, used space, free space, health status, and connection type
- **Three drive statuses** — distinguishes Known Named Drives, Previously Seen Unnamed Drives, and Brand New Never Seen Drives using a local SQLite database keyed on hardware serial number
- **Row highlighting** — yellow rows for drives with unbacked changes, orange rows for drives with a health warning
- **New drive alert** — Windows 10/11 toast notification with a "Set Up Now" action when an unknown drive is connected

## Technology

| Layer | Technology |
|---|---|
| UI framework | WPF (.NET 6, `net6.0-windows`) |
| MVVM | CommunityToolkit.Mvvm |
| Drive data | WMI — Microsoft.Management.Infrastructure (MI API) |
| Persistence | EF Core + SQLite (Microsoft.EntityFrameworkCore.Sqlite) |
| Notifications | Microsoft.Toolkit.Uwp.Notifications |

## NuGet packages

```
Microsoft.Management.Infrastructure
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Tools
CommunityToolkit.Mvvm
Microsoft.Toolkit.Uwp.Notifications
```

## Architecture

```
MinimalDriveApp/
├── Models/          # DriveInfo (WMI DTO), KnownDrive (EF entity), DriveStatus enum
├── Data/            # DbContext, DriveRepository
├── Services/        # DriveDetectionService (WMI), HotPlugService (CIM subscription)
├── ViewModels/      # MainViewModel — merges WMI data with SQLite history
├── Views/           # MainWindow.xaml — DataGrid with row styles
└── Migrations/      # EF Core migrations
```

## Build & Run

```powershell
dotnet build MinimalDriveApp.sln
dotnet run --project MinimalDriveApp\MinimalDriveApp.csproj
```

Requires Windows 10 or Windows 11.
