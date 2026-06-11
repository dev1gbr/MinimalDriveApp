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
        CollapseAll<QuickAccessToolBar>(this);
    }

    private static void CollapseAll<T>(DependencyObject parent) where T : UIElement
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T element)
                element.Visibility = Visibility.Collapsed;
            else
                CollapseAll<T>(child);
        }
    }
}
