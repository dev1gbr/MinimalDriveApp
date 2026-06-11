using System.Windows;
using System.Windows.Media;
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
        var qat = FindVisualChild<QuickAccessToolBar>(this);
        if (qat is null) return;

        qat.Visibility = Visibility.Collapsed;

        // Also collapse the immediate parent container (the row/border wrapping the QAT strip)
        if (VisualTreeHelper.GetParent(qat) is UIElement directParent)
            directParent.Visibility = Visibility.Collapsed;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T found) return found;
            var result = FindVisualChild<T>(child);
            if (result is not null) return result;
        }
        return null;
    }
}
