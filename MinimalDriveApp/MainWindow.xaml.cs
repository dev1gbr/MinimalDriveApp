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

        // Walk up from QAT until we reach a direct child of the Ribbon control —
        // that's the container row that holds the entire QAT strip.
        DependencyObject current = qat;
        while (current is not null)
        {
            var parent = VisualTreeHelper.GetParent(current);
            if (parent is Fluent.Ribbon && current is UIElement container)
            {
                container.Visibility = Visibility.Collapsed;
                return;
            }
            current = parent;
        }

        // Fallback: at least hide the control itself
        qat.Visibility = Visibility.Collapsed;
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
