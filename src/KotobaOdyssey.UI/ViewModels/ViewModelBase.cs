using CommunityToolkit.Mvvm.ComponentModel;
using KotobaOdyssey.Core.Interfaces;

namespace KotobaOdyssey.UI.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _statusText = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }
}

public class LibraryViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public LibraryViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
}

public class ReaderViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public ReaderViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
}

public class VocabularyViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public VocabularyViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
}

public class VisualNovelViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public VisualNovelViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
}

public class SettingsViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public SettingsViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
}
