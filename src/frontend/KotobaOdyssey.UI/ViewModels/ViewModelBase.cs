using CommunityToolkit.Mvvm.ComponentModel;

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
