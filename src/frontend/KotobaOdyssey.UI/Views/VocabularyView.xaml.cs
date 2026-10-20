using System.Windows.Controls;
using KotobaOdyssey.UI.ViewModels;

namespace KotobaOdyssey.UI.Views;

public partial class VocabularyView : UserControl
{
    public VocabularyView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is VocabularyViewModel vm)
        {
            await vm.LoadVocabularyAsync();
        }
    }
}
