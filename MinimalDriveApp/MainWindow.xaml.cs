using System.Windows;
using Fluent;

namespace MinimalDriveApp;

public partial class MainWindow : RibbonWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (MainRibbon?.QuickAccessToolBar is { } qat)
            qat.Visibility = Visibility.Collapsed;
    }
}
