using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinimalDriveApp.Data;
using MinimalDriveApp.Services;
using MinimalDriveApp.ViewModels;
using System.Windows;

namespace MinimalDriveApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton<ArchiveStackDbContext>(sp =>
                {
                    var opts = new DbContextOptionsBuilder<ArchiveStackDbContext>()
                        .UseSqlite("Data Source=archivestack.db")
                        .Options;
                    var db = new ArchiveStackDbContext(opts);
                    db.Database.Migrate();
                    return db;
                })
                .AddSingleton<IDriveRepository, DriveRepository>()
                .AddSingleton<IDriveDetectionService, DriveDetectionService>()
                .AddSingleton<IHotPlugService, HotPlugService>()
                .AddSingleton<IToastService, ToastService>()
                .AddSingleton<MainViewModel>()
                .AddSingleton<MainWindow>()
                .BuildServiceProvider());

        var window = Ioc.Default.GetRequiredService<MainWindow>();
        window.DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
        window.Show();

        Ioc.Default.GetRequiredService<MainViewModel>().Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Ioc.Default.GetRequiredService<IHotPlugService>().Dispose();
        base.OnExit(e);
    }
}
