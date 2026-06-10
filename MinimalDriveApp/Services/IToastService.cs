namespace MinimalDriveApp.Services;

public interface IToastService
{
    void ShowNewDriveAlert(string driveLetter, string serialNumber);
}
