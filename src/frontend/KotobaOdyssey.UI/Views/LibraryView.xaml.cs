using System.Windows.Controls;
using KotobaOdyssey.UI.ViewModels;

namespace KotobaOdyssey.UI.Views;

public partial class LibraryView : UserControl
{
    public LibraryView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LibraryViewModel vm)
        {
            await vm.LoadNovelsAsync();
        }
    }
}
