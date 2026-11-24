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

public class ParagraphModel
{
    public int ParagraphIndex { get; set; }
    public string RawText { get; set; } = string.Empty;
    public ObservableCollection<TokenItemViewModel> Tokens { get; set; } = new();
}

public class TokenItemViewModel
{
    public string Surface { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string BaseForm { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
    public JlptLevel JlptLevel { get; set; }
    public bool HasDictionaryEntry { get; set; }
    public bool IsPunctuation { get; set; }
    public string ContextSentence { get; set; } = string.Empty;

    public bool HasFurigana => !string.IsNullOrWhiteSpace(Reading) &&
                               Reading != Surface &&
                               Surface.Any(c => c >= '\u4e00' && c <= '\u9faf');
}

public partial class ReaderViewModel : ViewModelBase
{
    private readonly INovelRepository _novelRepository;
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly IJapaneseDictionaryService _dictionaryService;
    private readonly IJapaneseTextAnalyzer _textAnalyzer;
    private readonly IVocabularyRepository _vocabularyRepository;
    private readonly IDailyQuestRepository _dailyQuestRepository;
    private readonly INavigationService _navigationService;
    private readonly MainViewModel _mainViewModel;
    private readonly IAiTutorService? _aiTutorService;

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

    [ObservableProperty]
    private bool _isFuriganaVisible = true;

    // Drawers / Panels
    [ObservableProperty]
    private bool _isSettingsOpen;

    [ObservableProperty]
    private bool _isBookmarkDrawerOpen;

    [ObservableProperty]
    private string _newBookmarkNote = string.Empty;

    [ObservableProperty]
    private string _selectedSnippet = string.Empty;

    // AI Tutor Drawer
    [ObservableProperty]
    private bool _isAiTutorOpen;

    [ObservableProperty]
    private bool _isAiTutorLoading;

    [ObservableProperty]
    private string _aiSentenceJp = string.Empty;

    [ObservableProperty]
    private string _aiTranslationVn = string.Empty;

    [ObservableProperty]
    private string _aiGrammarInsight = string.Empty;

    [ObservableProperty]
    private string _aiStructureInfo = string.Empty;

    [ObservableProperty]
    private string _aiProviderInfo = string.Empty;

    [ObservableProperty]
    private string _aiFollowUpQuestion = string.Empty;

    [ObservableProperty]
    private string _aiFollowUpAnswer = string.Empty;

    [ObservableProperty]
    private string? _aiErrorMessage;

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

    [ObservableProperty]
    private string _currentContextSentence = string.Empty;

    [ObservableProperty]
    private string _savedFeedbackMessage = string.Empty;

    [ObservableProperty]
    private string _savedWordExistingContext = string.Empty;

    // Progress and Session stats
    [ObservableProperty]
    private int _readingProgressPercent = 0;

    [ObservableProperty]
    private string _sessionReadingTime = "00:00";

    [ObservableProperty]
    private bool _hasParagraphTokens;

    public ObservableCollection<Chapter> Chapters { get; } = new();
    public ObservableCollection<Bookmark> Bookmarks { get; } = new();
    public ObservableCollection<ParagraphModel> Paragraphs { get; } = new();

    public string SaveButtonText => IsWordAlreadySaved
        ? "✓ Đã có trong sổ từ (Cập nhật ngữ cảnh)"
        : "＋ Lưu vào sổ từ & Flashcard";

    public ReaderViewModel(
        INovelRepository novelRepository,
        IBookmarkRepository bookmarkRepository,
        IUserProgressRepository userProgressRepository,
        IJapaneseDictionaryService dictionaryService,
        IJapaneseTextAnalyzer textAnalyzer,
        IVocabularyRepository vocabularyRepository,
        IDailyQuestRepository dailyQuestRepository,
        INavigationService navigationService,
        MainViewModel mainViewModel,
        IAiTutorService? aiTutorService = null)
    {
        _novelRepository = novelRepository;
        _bookmarkRepository = bookmarkRepository;
        _userProgressRepository = userProgressRepository;
        _dictionaryService = dictionaryService;
        _textAnalyzer = textAnalyzer;
        _vocabularyRepository = vocabularyRepository;
        _dailyQuestRepository = dailyQuestRepository;
        _navigationService = navigationService;
        _mainViewModel = mainViewModel;
        _aiTutorService = aiTutorService;

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

        // Interactive Tokenized Paragraphs
        Paragraphs.Clear();
        if (!string.IsNullOrWhiteSpace(chapter.Content))
        {
            var rawParas = chapter.Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int pIdx = 0;
            foreach (var para in rawParas)
            {
                if (string.IsNullOrWhiteSpace(para)) continue;

                var pModel = new ParagraphModel
                {
                    ParagraphIndex = pIdx++,
                    RawText = para.Trim()
                };

                var tokens = await _textAnalyzer.AnalyzeTextAsync(pModel.RawText);
                foreach (var t in tokens)
                {
                    bool isPunc = char.IsPunctuation(t.Surface[0]) ||
                                  t.Surface == "、" || t.Surface == "。" || t.Surface == "「" || t.Surface == "」" ||
                                  t.Surface == "『" || t.Surface == "』" || t.Surface == "・" || t.Surface == "…";

                    string sentence = _textAnalyzer.ExtractSentence(pModel.RawText, t.Surface, t.StartIndex);

                    pModel.Tokens.Add(new TokenItemViewModel
                    {
                        Surface = t.Surface,
                        Reading = t.Reading,
                        BaseForm = t.BaseForm,
                        PartOfSpeech = t.PartOfSpeech,
                        JlptLevel = t.JlptLevel,
                        HasDictionaryEntry = t.HasDictionaryEntry,
                        IsPunctuation = isPunc,
                        ContextSentence = sentence
                    });
                }

                Paragraphs.Add(pModel);
            }
        }
        HasParagraphTokens = Paragraphs.Count > 0;

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

    [RelayCommand]
    public void ToggleFurigana()
    {
        IsFuriganaVisible = !IsFuriganaVisible;
    }

    // Dictionary Lookup Actions
    [RelayCommand]
    public async Task LookupToken(TokenItemViewModel? token)
    {
        if (token == null || string.IsNullOrWhiteSpace(token.Surface)) return;
        if (token.IsPunctuation) return;

        await LookupWordWithContext(token.Surface, token.ContextSentence);
    }

    [RelayCommand]
    public async Task LookupWord(string? term = null)
    {
        await LookupWordWithContext(term, null);
    }

    public async Task LookupWordWithContext(string? term, string? contextSentence)
    {
        string lookupText = term ?? LookupInputText;
        if (string.IsNullOrWhiteSpace(lookupText)) return;

        LookupTerm = lookupText.Trim();
        LookupResult = await _dictionaryService.LookupAsync(LookupTerm);

        IsLookupNotFound = LookupResult == null;
        string wordToCheck = LookupResult?.Term ?? LookupTerm;

        var existing = await _vocabularyRepository.GetByWordAsync(wordToCheck);
        IsWordAlreadySaved = existing != null;
        SavedWordExistingContext = existing?.ContextSentence ?? string.Empty;
        SavedFeedbackMessage = existing != null ? "Từ này đã có trong sổ từ." : string.Empty;

        if (!string.IsNullOrWhiteSpace(contextSentence))
        {
            CurrentContextSentence = contextSentence;
        }
        else if (CurrentChapter != null)
        {
            CurrentContextSentence = _textAnalyzer.ExtractSentence(CurrentChapter.Content, LookupTerm);
        }
        else
        {
            CurrentContextSentence = LookupResult?.ExampleSentence ?? string.Empty;
        }

        OnPropertyChanged(nameof(SaveButtonText));
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
        if (LookupResult == null && string.IsNullOrWhiteSpace(LookupTerm)) return;

        string wordToSave = LookupResult?.Term ?? LookupTerm;
        string readingToSave = LookupResult?.Reading ?? wordToSave;
        string meaningToSave = LookupResult?.Meaning ?? "Từ vựng tiếng Nhật";
        var jlptToSave = LookupResult?.JlptLevel ?? JlptLevel.N5;
        string sentenceToSave = !string.IsNullOrWhiteSpace(CurrentContextSentence)
            ? CurrentContextSentence
            : (LookupResult?.ExampleSentence ?? "");

        var existing = await _vocabularyRepository.GetByWordAsync(wordToSave);
        if (existing != null)
        {
            // Enrich context if not already present
            if (!string.IsNullOrWhiteSpace(sentenceToSave) &&
                (string.IsNullOrWhiteSpace(existing.ContextSentence) || !existing.ContextSentence.Contains(sentenceToSave)))
            {
                existing.ContextSentence = string.IsNullOrWhiteSpace(existing.ContextSentence)
                    ? sentenceToSave
                    : $"{existing.ContextSentence} \n{sentenceToSave}";

                if (CurrentChapter != null && !existing.SourceChapterId.HasValue)
                {
                    existing.SourceChapterId = CurrentChapter.Id;
                }

                await _vocabularyRepository.UpdateAsync(existing);
                SavedFeedbackMessage = "Đã cập nhật thêm ngữ cảnh mới vào sổ từ vựng!";
            }
            else
            {
                SavedFeedbackMessage = "Từ này đã có sẵn trong sổ từ vựng.";
            }

            IsWordAlreadySaved = true;
            OnPropertyChanged(nameof(SaveButtonText));
            return;
        }

        var item = new VocabularyItem
        {
            Word = wordToSave,
            Reading = readingToSave,
            Meaning = meaningToSave,
            JlptLevel = jlptToSave,
            ContextSentence = sentenceToSave,
            SourceNovelTitle = Novel?.Title ?? "Tác phẩm văn học",
            SourceChapterId = CurrentChapter?.Id,
            CreatedAt = DateTime.UtcNow,
            NextReviewDate = DateTime.UtcNow, // Due immediately for review
            IntervalDays = 0,
            EaseFactor = 2.5,
            Repetitions = 0
        };

        await _vocabularyRepository.AddAsync(item);
        await _userProgressRepository.IncrementWordsLearnedAsync();
        await _dailyQuestRepository.IncrementQuestProgressAsync(QuestType.AddVocabulary, 1);
        await _mainViewModel.LoadUserProgressAsync();

        IsWordAlreadySaved = true;
        SavedFeedbackMessage = "Đã lưu vào sổ từ vựng & Flashcard!";
        OnPropertyChanged(nameof(SaveButtonText));
    }

    [RelayCommand]
    public async Task AddBookmark()
    {
        if (CurrentChapter == null) return;

        string snippet = !string.IsNullOrWhiteSpace(SelectedSnippet)
            ? SelectedSnippet
            : (CurrentChapter.Content.Length > 120 ? CurrentChapter.Content.Substring(0, 120) + "..." : CurrentChapter.Content);

        var bm = new Bookmark
        {
            ChapterId = CurrentChapter.Id,
            CharacterPosition = CurrentChapter.LastReadPosition,
            Snippet = snippet,
            Note = string.IsNullOrWhiteSpace(NewBookmarkNote) ? $"Dấu trang tại {DateTime.Now:dd/MM/yyyy HH:mm}" : NewBookmarkNote.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _bookmarkRepository.AddAsync(bm);
        Bookmarks.Insert(0, bm);
        NewBookmarkNote = string.Empty;
        IsBookmarkDrawerOpen = false;
    }

    [RelayCommand]
    public async Task DeleteBookmark(Bookmark? bm)
    {
        if (bm == null) return;
        await _bookmarkRepository.DeleteAsync(bm.Id);
        Bookmarks.Remove(bm);
    }

    [RelayCommand]
    public void JumpToBookmark(Bookmark? bm)
    {
        if (bm == null) return;
        _ = SaveCurrentProgressAsync(bm.CharacterPosition);
        IsBookmarkDrawerOpen = false;
    }

    // Customization Drawers Toggle
    [RelayCommand]
    public void ToggleSettings()
    {
        IsSettingsOpen = !IsSettingsOpen;
        if (IsSettingsOpen)
        {
            IsBookmarkDrawerOpen = false;
            IsLookupOpen = false;
        }
    }

    [RelayCommand]
    public void ToggleBookmarks()
    {
        IsBookmarkDrawerOpen = !IsBookmarkDrawerOpen;
        if (IsBookmarkDrawerOpen)
        {
            IsSettingsOpen = false;
            IsLookupOpen = false;
        }
    }

    [RelayCommand]
    public void SetTheme(string themeName)
    {
        SelectedTheme = themeName;
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        switch (SelectedTheme)
        {
            case "Slate":
                ReaderBackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
                ReaderForegroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
                break;
            case "Paper":
                ReaderBackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F2EB"));
                ReaderForegroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#292524"));
                break;
            case "Cosmic":
            default:
                ReaderBackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C1120"));
                ReaderForegroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                break;
        }
    }

    [RelayCommand]
    public void IncreaseFontSize()
    {
        FontSize = Math.Min(FontSize + 2, 36);
        LineSpacing = FontSize * 1.6;
    }

    [RelayCommand]
    public void DecreaseFontSize()
    {
        FontSize = Math.Max(FontSize - 2, 12);
        LineSpacing = FontSize * 1.6;
    }

    [RelayCommand]
    public async Task AnalyzeSentenceWithAi(string? customSentence = null)
    {
        string target = !string.IsNullOrWhiteSpace(customSentence)
            ? customSentence
            : (!string.IsNullOrWhiteSpace(CurrentContextSentence) ? CurrentContextSentence : CurrentChapter?.Title ?? "");

        if (string.IsNullOrWhiteSpace(target)) return;

        IsAiTutorOpen = true;
        IsAiTutorLoading = true;
        AiErrorMessage = null;
        AiSentenceJp = target;
        AiTranslationVn = "Đang phân tích cấu trúc cú pháp...";
        AiGrammarInsight = "Đang kết nối AI Tutor Engine...";
        AiStructureInfo = string.Empty;
        AiFollowUpAnswer = string.Empty;

        if (_aiTutorService == null)
        {
            AiTranslationVn = "Dịch vụ AI Tutor chưa được kích hoạt trong phiên bản này.";
            AiGrammarInsight = "Vui lòng kiểm tra cấu hình kết nối AI.";
            IsAiTutorLoading = false;
            return;
        }

        try
        {
            var req = new AiSentenceExplanationRequest
            {
                Sentence = target,
                NovelTitle = Novel?.Title ?? "Tiểu thuyết",
                ChapterTitle = CurrentChapter?.Title ?? "Chương hiện tại",
                ChapterNumber = CurrentChapterNumber,
                JlptLevel = Novel != null ? Novel.JlptLevel.ToString() : "N3",
                RequestType = "explain"
            };

            var res = await _aiTutorService.ExplainSentenceAsync(req);
            if (res != null && res.Success)
            {
                AiTranslationVn = res.TranslationVn;
                AiGrammarInsight = res.GrammarInsight;
                AiProviderInfo = res.Provider;

                if (res.SentenceStructure != null)
                {
                    AiStructureInfo = $"Chủ ngữ: {res.SentenceStructure.Subject} | Vị ngữ: {res.SentenceStructure.Predicate}";
                }
            }
            else
            {
                AiErrorMessage = res?.ErrorMessage ?? "Không thể phân tích câu.";
            }
        }
        catch (Exception ex)
        {
            AiErrorMessage = ex.Message;
        }
        finally
        {
            IsAiTutorLoading = false;
        }
    }

    [RelayCommand]
    public async Task SendAiFollowUp()
    {
        if (string.IsNullOrWhiteSpace(AiFollowUpQuestion) || string.IsNullOrWhiteSpace(AiSentenceJp) || _aiTutorService == null)
            return;

        IsAiTutorLoading = true;
        try
        {
            var req = new AiSentenceExplanationRequest
            {
                Sentence = AiSentenceJp,
                NovelTitle = Novel?.Title ?? "",
                ChapterTitle = CurrentChapter?.Title ?? "",
                ChapterNumber = CurrentChapterNumber,
                RequestType = "follow_up",
                FollowUpQuestion = AiFollowUpQuestion,
                PreviousExplanation = AiGrammarInsight
            };

            var res = await _aiTutorService.ExplainSentenceAsync(req);
            if (res != null && res.Success)
            {
                AiFollowUpAnswer = res.GrammarInsight ?? res.TranslationVn;
            }
        }
        catch (Exception ex)
        {
            AiErrorMessage = ex.Message;
        }
        finally
        {
            IsAiTutorLoading = false;
        }
    }

    [RelayCommand]
    public void CloseAiTutor()
    {
        IsAiTutorOpen = false;
    }

    [RelayCommand]
    public void BackToLibrary()
    {
        _navigationService.NavigateTo("Library");
    }
}
