using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;

namespace KotobaOdyssey.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IUserProgressRepository _userProgressRepository;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _currentPageKey = "Dashboard";

    [ObservableProperty]
    private string _userName = "旅人 (Traveler)";

    [ObservableProperty]
    private string _userTitle = "Starlight Apprentice";

    [ObservableProperty]
    private int _level = 1;

    [ObservableProperty]
    private int _currentXp = 0;

    [ObservableProperty]
    private int _requiredXp = 250;

    [ObservableProperty]
    private double _xpProgressPercentage;

    [ObservableProperty]
    private int _streakDays = 1;

    [ObservableProperty]
    private string _statusMessage = "Ready • SQLite Local Database";

    [ObservableProperty]
    private string _appVersion = "v1.0.0-preview (Phase 1)";

    public MainViewModel(
        INavigationService navigationService,
        IUserProgressRepository userProgressRepository)
    {
        _navigationService = navigationService;
        _userProgressRepository = userProgressRepository;

        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        CurrentView = _navigationService.CurrentView;
    }

    private void OnCurrentViewChanged()
    {
        CurrentView = _navigationService.CurrentView;
        CurrentPageKey = _navigationService.CurrentPageKey;
    }

    [RelayCommand]
    public void Navigate(string pageKey)
    {
        _navigationService.NavigateTo(pageKey);
        CurrentPageKey = pageKey;
    }

    public async Task InitializeAsync()
    {
        await LoadUserProgressAsync();
        _navigationService.NavigateTo<DashboardViewModel>();
    }

    public async Task LoadUserProgressAsync()
    {
        var progress = await _userProgressRepository.GetProgressAsync();
        UserName = progress.UserName;
        UserTitle = progress.Title;
        Level = progress.Level;
        CurrentXp = progress.CurrentXp;
        RequiredXp = progress.RequiredXp;
        StreakDays = progress.StreakDays;

        if (RequiredXp > 0)
        {
            XpProgressPercentage = Math.Clamp((double)CurrentXp / RequiredXp * 100, 0, 100);
        }
    }
}
