using System.Windows.Controls;
using KotobaOdyssey.UI.ViewModels;

namespace KotobaOdyssey.UI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
        {
            await vm.LoadDashboardAsync();
        }
    }
}
