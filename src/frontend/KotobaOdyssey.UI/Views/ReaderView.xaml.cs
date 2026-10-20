using System.Windows.Controls;
using KotobaOdyssey.UI.ViewModels;

namespace KotobaOdyssey.UI.Views;

public partial class ReaderView : UserControl
{
    public ReaderView()
    {
        InitializeComponent();
        ReadingScrollViewer.ScrollChanged += OnReadingScrollChanged;
    }

    private void OnReadingScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (DataContext is ReaderViewModel vm && vm.CurrentChapter != null)
        {
            if (ReadingScrollViewer.ScrollableHeight > 0)
            {
                double ratio = ReadingScrollViewer.VerticalOffset / ReadingScrollViewer.ScrollableHeight;
                int pos = (int)Math.Round(ratio * vm.CurrentChapter.WordCount);
                _ = vm.SaveCurrentProgressAsync(pos);
            }
        }
    }
}
