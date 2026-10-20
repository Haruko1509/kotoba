using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;

namespace KotobaOdyssey.UI.ViewModels;

public partial class ReaderViewModel : ViewModelBase
{
    private readonly INovelRepository _novelRepository;
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly IJapaneseDictionaryService _dictionaryService;
    private readonly IVocabularyRepository _vocabularyRepository;
    private readonly IDailyQuestRepository _dailyQuestRepository;
    private readonly INavigationService _navigationService;
    private readonly MainViewModel _mainViewModel;

    private DispatcherTimer? _studyTimer;
    private int _elapsedSeconds;

    [ObservableProperty]
    private Novel? _novel;

    [ObservableProperty]
    private Chapter? _currentChapter;

    [ObservableProperty]
    private int _currentChapterNumber = 1;

    [ObservableProperty]
    private int _totalChapters = 1;

    [ObservableProperty]
    private bool _hasPreviousChapter;

    [ObservableProperty]
    private bool _hasNextChapter;

    // Reader Customization Preferences
    [ObservableProperty]
    private double _fontSize = 18;

    [ObservableProperty]
    private double _lineSpacing = 30;

    [ObservableProperty]
    private string _selectedFontFamily = "Yu Gothic";

    [ObservableProperty]
    private string _selectedTheme = "Cosmic";

    [ObservableProperty]
    private Brush _readerBackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C1120"));

    [ObservableProperty]
    private Brush _readerForegroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));

    // Drawers / Panels
    [ObservableProperty]
    private bool _isSettingsOpen;

    [ObservableProperty]
    private bool _isBookmarkDrawerOpen;

    [ObservableProperty]
    private string _newBookmarkNote = string.Empty;

    [ObservableProperty]
    private string _selectedSnippet = string.Empty;

    // Dictionary Quick Lookup Drawer
    [ObservableProperty]
    private bool _isLookupOpen;

    [ObservableProperty]
    private string _lookupInputText = string.Empty;

    [ObservableProperty]
    private string _lookupTerm = string.Empty;

    [ObservableProperty]
    private DictionaryEntry? _lookupResult;

    [ObservableProperty]
    private bool _isLookupNotFound;

    [ObservableProperty]
    private bool _isWordAlreadySaved;

    // Progress and Session stats
    [ObservableProperty]
    private int _readingProgressPercent = 0;

    [ObservableProperty]
    private string _sessionReadingTime = "00:00";

    public ObservableCollection<Chapter> Chapters { get; } = new();
    public ObservableCollection<Bookmark> Bookmarks { get; } = new();

    public ReaderViewModel(
        INovelRepository novelRepository,
        IBookmarkRepository bookmarkRepository,
        IUserProgressRepository userProgressRepository,
        IJapaneseDictionaryService dictionaryService,
        IVocabularyRepository vocabularyRepository,
        IDailyQuestRepository dailyQuestRepository,
        INavigationService navigationService,
        MainViewModel mainViewModel)
    {
        _novelRepository = novelRepository;
        _bookmarkRepository = bookmarkRepository;
        _userProgressRepository = userProgressRepository;
        _dictionaryService = dictionaryService;
        _vocabularyRepository = vocabularyRepository;
        _dailyQuestRepository = dailyQuestRepository;
        _navigationService = navigationService;
        _mainViewModel = mainViewModel;

        SetupStudyTimer();
        ApplyTheme();
    }

    private void SetupStudyTimer()
    {
        _studyTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _studyTimer.Tick += async (s, e) =>
        {
            _elapsedSeconds++;
            int minutes = _elapsedSeconds / 60;
            int seconds = _elapsedSeconds % 60;
            SessionReadingTime = $"{minutes:D2}:{seconds:D2}";

            // Every 60 seconds, record study minute to SQLite user progress
            if (_elapsedSeconds > 0 && _elapsedSeconds % 60 == 0)
            {
                await _userProgressRepository.AddStudyMinutesAsync(1);
            }
        };
        _studyTimer.Start();
    }

    public async Task LoadNovelAsync(Novel novel, int? chapterId = null)
    {
        IsBusy = true;
        StatusText = "Đang tải văn bản tiếng Nhật...";

        try
        {
            var detailed = await _novelRepository.GetByIdAsync(novel.Id);
            Novel = detailed ?? novel;

            Chapters.Clear();
            foreach (var ch in Novel.Chapters.OrderBy(c => c.ChapterNumber))
            {
                Chapters.Add(ch);
            }

            TotalChapters = Chapters.Count;

            if (chapterId.HasValue)
            {
                CurrentChapter = Chapters.FirstOrDefault(c => c.Id == chapterId.Value) ?? Chapters.FirstOrDefault();
            }
            else
            {
                CurrentChapter = Chapters.OrderByDescending(c => c.LastReadAt ?? DateTime.MinValue).FirstOrDefault()
                    ?? Chapters.FirstOrDefault();
            }

            if (CurrentChapter != null)
            {
                await LoadChapterDetailsAsync(CurrentChapter);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadChapterDetailsAsync(Chapter chapter)
    {
        CurrentChapter = chapter;
        CurrentChapterNumber = chapter.ChapterNumber;

        int index = Chapters.IndexOf(chapter);
        HasPreviousChapter = index > 0;
        HasNextChapter = index < Chapters.Count - 1;

        if (chapter.WordCount > 0 && chapter.LastReadPosition > 0)
        {
            ReadingProgressPercent = Math.Clamp(
                (int)Math.Round((double)chapter.LastReadPosition / chapter.WordCount * 100),
                0, 100);
        }
        else
        {
            ReadingProgressPercent = 0;
        }

        // Load bookmarks for current chapter
        var bms = await _bookmarkRepository.GetByChapterIdAsync(chapter.Id);
        Bookmarks.Clear();
        foreach (var b in bms)
        {
            Bookmarks.Add(b);
        }

        await _novelRepository.UpdateChapterProgressAsync(chapter.Id, chapter.LastReadPosition, chapter.IsCompleted);
    }

    [RelayCommand]
    public async Task SelectChapter(Chapter? chapter)
    {
        if (chapter == null || chapter == CurrentChapter) return;
        await SaveCurrentProgressAsync();
        await LoadChapterDetailsAsync(chapter);
    }

    [RelayCommand]
    public async Task NextChapter()
    {
        if (CurrentChapter == null) return;
        int index = Chapters.IndexOf(CurrentChapter);
        if (index < Chapters.Count - 1)
        {
            await SaveCurrentProgressAsync();
            await LoadChapterDetailsAsync(Chapters[index + 1]);
        }
    }

    [RelayCommand]
    public async Task PreviousChapter()
    {
        if (CurrentChapter == null) return;
        int index = Chapters.IndexOf(CurrentChapter);
        if (index > 0)
        {
            await SaveCurrentProgressAsync();
            await LoadChapterDetailsAsync(Chapters[index - 1]);
        }
    }

    [RelayCommand]
    public async Task SaveCurrentProgressAsync(int? newPosition = null)
    {
        if (CurrentChapter == null) return;

        int pos = newPosition ?? CurrentChapter.LastReadPosition;
        CurrentChapter.LastReadPosition = pos;

        bool isCompleted = CurrentChapter.WordCount > 0 && pos >= (CurrentChapter.WordCount * 0.9);
        CurrentChapter.IsCompleted = isCompleted;

        if (CurrentChapter.WordCount > 0)
        {
            ReadingProgressPercent = Math.Clamp(
                (int)Math.Round((double)pos / CurrentChapter.WordCount * 100),
                0, 100);
        }

        await _novelRepository.UpdateChapterProgressAsync(CurrentChapter.Id, pos, isCompleted);
    }

    // Dictionary Lookup Actions
    [RelayCommand]
    public async Task LookupWord(string? term = null)
    {
        string lookupText = term ?? LookupInputText;
        if (string.IsNullOrWhiteSpace(lookupText)) return;

        LookupTerm = lookupText.Trim();
        LookupResult = await _dictionaryService.LookupAsync(LookupTerm);

        IsLookupNotFound = LookupResult == null;
        IsWordAlreadySaved = await _vocabularyRepository.ExistsAsync(LookupTerm);

        IsLookupOpen = true;
        IsSettingsOpen = false;
        IsBookmarkDrawerOpen = false;
    }

    [RelayCommand]
    public void CloseLookup()
    {
        IsLookupOpen = false;
    }

    [RelayCommand]
    public async Task SaveWordFromReader()
    {
        if (LookupResult == null || IsWordAlreadySaved) return;

        string context = CurrentChapter != null
            ? (CurrentChapter.Content.Length > 80 ? CurrentChapter.Content.Substring(0, 80) + "..." : CurrentChapter.Content)
            : "";

        var item = new VocabularyItem
        {
            Word = LookupResult.Term,
            Reading = LookupResult.Reading,
            Meaning = LookupResult.Meaning,
            JlptLevel = LookupResult.JlptLevel,
            ContextSentence = string.IsNullOrWhiteSpace(LookupResult.ExampleSentence) ? context : LookupResult.ExampleSentence,
            SourceNovelTitle = Novel?.Title ?? "Reading Workspace",
            SourceChapterId = CurrentChapter?.Id,
            CreatedAt = DateTime.UtcNow,
            NextReviewDate = DateTime.UtcNow.AddHours(4),
            IntervalDays = 0,
            EaseFactor = 2.5,
            Repetitions = 0
        };

        await _vocabularyRepository.AddAsync(item);
        await _userProgressRepository.IncrementWordsLearnedAsync();
        await _dailyQuestRepository.IncrementQuestProgressAsync(QuestType.AddVocabulary, 1);
        await _mainViewModel.LoadUserProgressAsync();

        IsWordAlreadySaved = true;
    }

    [RelayCommand]
    public async Task AddBookmark()
    {
        if (CurrentChapter == null) return;

        string note = string.IsNullOrWhiteSpace(NewBookmarkNote) ? "Đoạn văn quan trọng" : NewBookmarkNote.Trim();
        string snippet = string.IsNullOrWhiteSpace(SelectedSnippet)
            ? (CurrentChapter.Content.Length > 40 ? CurrentChapter.Content.Substring(0, 40) + "..." : CurrentChapter.Content)
            : SelectedSnippet.Trim();

        var bm = new Bookmark
        {
            ChapterId = CurrentChapter.Id,
            CharacterPosition = CurrentChapter.LastReadPosition,
            Snippet = snippet,
            Note = note,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _bookmarkRepository.AddAsync(bm);
        Bookmarks.Add(saved);

        NewBookmarkNote = string.Empty;
        SelectedSnippet = string.Empty;
    }

    [RelayCommand]
    public async Task DeleteBookmark(Bookmark? bookmark)
    {
        if (bookmark == null) return;
        await _bookmarkRepository.DeleteAsync(bookmark.Id);
        Bookmarks.Remove(bookmark);
    }

    [RelayCommand]
    public void ToggleSettingsDrawer()
    {
        IsSettingsOpen = !IsSettingsOpen;
        if (IsSettingsOpen)
        {
            IsBookmarkDrawerOpen = false;
            IsLookupOpen = false;
        }
    }

    [RelayCommand]
    public void ToggleBookmarkDrawer()
    {
        IsBookmarkDrawerOpen = !IsBookmarkDrawerOpen;
        if (IsBookmarkDrawerOpen)
        {
            IsSettingsOpen = false;
            IsLookupOpen = false;
        }
    }

    [RelayCommand]
    public void SetTheme(string themeKey)
    {
        SelectedTheme = themeKey;
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        switch (SelectedTheme)
        {
            case "DeepNight":
                ReaderBackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#060911"));
                ReaderForegroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CBD5E1"));
                break;
            case "WarmSepia":
                ReaderBackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C1917"));
                ReaderForegroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDE68A"));
                break;
            case "Amethyst":
                ReaderBackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#150F28"));
                ReaderForegroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9D5FF"));
                break;
            case "Cosmic":
            default:
                ReaderBackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C1120"));
                ReaderForegroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                break;
        }
    }

    [RelayCommand]
    public async Task BackToLibrary()
    {
        await SaveCurrentProgressAsync();
        _studyTimer?.Stop();
        _navigationService.NavigateTo("Library");
    }
}
