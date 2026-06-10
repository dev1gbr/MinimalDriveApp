using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace MinimalDriveApp.Services;

public class ToastService : IToastService
{
    public void ShowNewDriveAlert(string driveLetter, string serialNumber)
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
    }
}
