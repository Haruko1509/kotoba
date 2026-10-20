using System.IO;
using System.Windows;
using KotobaOdyssey.UI.ViewModels;

namespace KotobaOdyssey.UI;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] MainWindow.OnLoaded started.\n");
            await _viewModel.InitializeAsync();
            File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] MainWindow.OnLoaded completed successfully!\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] ERROR in OnLoaded: {ex}\n");
            MessageBox.Show($"Lỗi giao diện: {ex.Message}\n{ex.StackTrace}", "Kotoba Odyssey Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}