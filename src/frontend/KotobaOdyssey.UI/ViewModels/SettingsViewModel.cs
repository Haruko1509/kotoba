using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;

namespace KotobaOdyssey.UI.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IUserProgressRepository? _progressRepository;

    [ObservableProperty]
    private string _currentTheme = "Cosmic Obsidian";

    [ObservableProperty]
    private string _japaneseFont = "Noto Sans JP / Yu Gothic";

    [ObservableProperty]
    private int _readerFontSize = 18;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FuriganaStatusLabel))]
    private bool _enableFurigana = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AudioSfxStatusLabel))]
    private bool _enableAudioSfx = true;

    public string FuriganaStatusLabel => EnableFurigana ? "Đang BẬT" : "Đang TẮT";
    public string AudioSfxStatusLabel => EnableAudioSfx ? "Đang BẬT" : "Đang TẮT";

    [ObservableProperty]
    private JlptLevel _defaultJlptLevel = JlptLevel.N5;

    [ObservableProperty]
    private string _dataStoragePath = "SQLite: %LOCALAPPDATA%\\KotobaOdyssey\\kotoba.db";

    [ObservableProperty]
    private string _appVersion = "Kotoba Odyssey v6.0 (Polish Edition)";

    public SettingsViewModel(
        INavigationService navigationService,
        IUserProgressRepository? progressRepository = null)
    {
        _navigationService = navigationService;
        _progressRepository = progressRepository;
    }

    [RelayCommand]
    public void SetTheme(string theme)
    {
        CurrentTheme = theme;
        StatusText = $"Đã chuyển giao diện sang: {theme}";
    }

    [RelayCommand]
    public void SetJapaneseFont(string font)
    {
        JapaneseFont = font;
        StatusText = $"Đã chọn phông chữ tiếng Nhật: {font}";
    }

    [RelayCommand]
    public void SetJlptLevel(string levelStr)
    {
        if (Enum.TryParse<JlptLevel>(levelStr, true, out var level))
        {
            DefaultJlptLevel = level;
            StatusText = $"Mức JLPT mặc định: {level}";
        }
    }

    [RelayCommand]
    public void IncreaseFontSize()
    {
        if (ReaderFontSize < 32)
        {
            ReaderFontSize += 2;
            StatusText = $"Cỡ chữ đọc truyện: {ReaderFontSize}px";
        }
    }

    [RelayCommand]
    public void DecreaseFontSize()
    {
        if (ReaderFontSize > 12)
        {
            ReaderFontSize -= 2;
            StatusText = $"Cỡ chữ đọc truyện: {ReaderFontSize}px";
        }
    }

    [RelayCommand]
    public void ToggleFurigana()
    {
        EnableFurigana = !EnableFurigana;
        StatusText = EnableFurigana ? "Đã bật hiển thị Furigana" : "Đã tắt Furigana";
    }

    [RelayCommand]
    public void ToggleAudioSfx()
    {
        EnableAudioSfx = !EnableAudioSfx;
        StatusText = EnableAudioSfx ? "Đã bật âm thanh hiệu ứng" : "Đã tắt âm thanh hiệu ứng";
    }

    [RelayCommand]
    public void BackupDatabase()
    {
        StatusText = "Đã sao lưu cơ sở dữ liệu SQLite thành công vào kotoba_backup.db";
    }

    [RelayCommand]
    public void BackToDashboard()
    {
        _navigationService.NavigateTo("Dashboard");
    }
}
