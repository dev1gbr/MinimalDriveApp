using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace MinimalDriveApp.Services;

public class ToastService : IToastService
{
    private readonly ILogger<ToastService> _logger;

    public ToastService(ILogger<ToastService> logger)
    {
        _logger = logger;
    }

    public void ShowNewDriveAlert(string driveLetter, string serialNumber)
    {
        _logger.LogInformation("Showing new-drive toast for drive={DriveLetter} serial={Serial}", driveLetter, serialNumber);
        try
        {
            var content = new ToastContentBuilder()
                .AddText("New Drive Detected")
                .AddText($"Drive {driveLetter} connected for the first time.")
                .AddButton(new ToastButton()
                    .SetContent("Set Up Now")
                    .AddArgument("action", "setup")
                    .AddArgument("serial", serialNumber))
                .GetToastContent();

            var toast = new ToastNotification(content.GetXml());
            ToastNotificationManager.CreateToastNotifier("MinimalDriveApp").Show(toast);
            _logger.LogInformation("Toast shown successfully for serial={Serial}", serialNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show toast for serial={Serial}", serialNumber);
        }
    }
}
