using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MinimalDriveApp.Data;
using MinimalDriveApp.Services;
using MinimalDriveApp.ViewModels;
using Serilog;
using System.IO;
using System.Windows;
using ControlzEx.Theming;

namespace MinimalDriveApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ThemeManager.Current.ChangeTheme(this, "Dark.Blue");

        var logPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "logs", "app-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddLogging(b => b.AddSerilog(dispose: true))
                .AddSingleton<MinimalDriveAppDbContext>(sp =>
                {
                    var opts = new DbContextOptionsBuilder<MinimalDriveAppDbContext>()
                        .UseSqlite("Data Source=minimaldriveapp.db")
                        .Options;
                    var db = new MinimalDriveAppDbContext(opts);
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

        Log.Information("Application starting up");

        var window = Ioc.Default.GetRequiredService<MainWindow>();
        window.DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
        window.Show();

        Ioc.Default.GetRequiredService<MainViewModel>().Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Application shutting down");
        Ioc.Default.GetRequiredService<IHotPlugService>().Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
