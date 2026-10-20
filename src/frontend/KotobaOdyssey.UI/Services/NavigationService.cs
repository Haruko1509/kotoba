using KotobaOdyssey.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace KotobaOdyssey.UI.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private object? _currentView;
    private string _currentPageKey = "Dashboard";

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? CurrentView
    {
        get => _currentView;
        private set
        {
            if (_currentView != value)
            {
                _currentView = value;
                CurrentViewChanged?.Invoke();
            }
        }
    }

    public string CurrentPageKey
    {
        get => _currentPageKey;
        private set => _currentPageKey = value;
    }

    public event Action? CurrentViewChanged;

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var vm = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentView = vm;
        CurrentPageKey = typeof(TViewModel).Name.Replace("ViewModel", string.Empty);
    }

    public void NavigateTo(string pageKey, object? parameter = null)
    {
        CurrentPageKey = pageKey;
        object vm = pageKey.ToLowerInvariant() switch
        {
            "dashboard" => _serviceProvider.GetRequiredService<ViewModels.DashboardViewModel>(),
            "library" => _serviceProvider.GetRequiredService<ViewModels.LibraryViewModel>(),
            "reader" => _serviceProvider.GetRequiredService<ViewModels.ReaderViewModel>(),
            "vocabulary" => _serviceProvider.GetRequiredService<ViewModels.VocabularyViewModel>(),
            "visualnovel" => _serviceProvider.GetRequiredService<ViewModels.VisualNovelViewModel>(),
            "settings" => _serviceProvider.GetRequiredService<ViewModels.SettingsViewModel>(),
            _ => _serviceProvider.GetRequiredService<ViewModels.DashboardViewModel>()
        };

        CurrentView = vm;
    }
}
