using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Core.Services;

namespace KotobaOdyssey.UI.ViewModels;

public partial class VocabularyViewModel : ViewModelBase
{
    private readonly IVocabularyRepository _vocabularyRepository;
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly IDailyQuestRepository _dailyQuestRepository;
    private readonly INavigationService _navigationService;
    private readonly MainViewModel _mainViewModel;

    private List<VocabularyItem> _allWords = new();
    private List<VocabularyItem> _dueWords = new();

    // Tabs: "Notebook" or "Review"
    [ObservableProperty]
    private string _activeTab = "Notebook";

    // Notebook State
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedJlptFilter = "Tất cả JLPT";

    [ObservableProperty]
    private bool _dueOnlyFilter;

    [ObservableProperty]
    private int _totalWordsCount;

    [ObservableProperty]
    private int _dueWordsCount;

    [ObservableProperty]
    private VocabularyItem? _selectedWord;

    public ObservableCollection<VocabularyItem> FilteredWords { get; } = new();

    // Manual Add Word Modal State
    [ObservableProperty]
    private bool _isAddModalOpen;

    [ObservableProperty]
    private string _newWord = string.Empty;

    [ObservableProperty]
    private string _newReading = string.Empty;

    [ObservableProperty]
    private string _newMeaning = string.Empty;

    [ObservableProperty]
    private JlptLevel _newJlptLevel = JlptLevel.N5;

    [ObservableProperty]
    private string _newContextSentence = string.Empty;

    [ObservableProperty]
    private string _newNotes = string.Empty;

    [ObservableProperty]
    private string _addWordErrorMessage = string.Empty;

    // Flashcard SRS Review Session State
    [ObservableProperty]
    private VocabularyItem? _currentCard;

    [ObservableProperty]
    private int _currentCardIndex = 0;

    [ObservableProperty]
    private int _totalReviewSessionCards = 0;

    [ObservableProperty]
    private bool _isAnswerRevealed;

    [ObservableProperty]
    private bool _isReviewSessionCompleted;

    [ObservableProperty]
    private int _sessionXpEarned;

    [ObservableProperty]
    private int _sessionReviewedCount;

    public VocabularyViewModel(
        IVocabularyRepository vocabularyRepository,
        IUserProgressRepository userProgressRepository,
        IDailyQuestRepository dailyQuestRepository,
        INavigationService navigationService,
        MainViewModel mainViewModel)
    {
        _vocabularyRepository = vocabularyRepository;
        _userProgressRepository = userProgressRepository;
        _dailyQuestRepository = dailyQuestRepository;
        _navigationService = navigationService;
        _mainViewModel = mainViewModel;
    }

    public async Task InitializeAsync()
    {
        await LoadVocabularyAsync();
    }

    [RelayCommand]
    public async Task LoadVocabularyAsync()
    {
        IsBusy = true;
        StatusText = "Đang tải sổ tay từ vựng...";

        try
        {
            var list = await _vocabularyRepository.GetAllAsync();
            _allWords = list.ToList();

            var due = await _vocabularyRepository.GetDueReviewsAsync();
            _dueWords = due.ToList();

            TotalWordsCount = _allWords.Count;
            DueWordsCount = _dueWords.Count;

            ApplyFilters();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilters();
    partial void OnSelectedJlptFilterChanged(string value) => ApplyFilters();
    partial void OnDueOnlyFilterChanged(bool value) => ApplyFilters();

    private void ApplyFilters()
    {
        IEnumerable<VocabularyItem> query = _allWords;

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.Trim().ToLower();
            query = query.Where(v =>
                v.Word.ToLower().Contains(q) ||
                v.Reading.ToLower().Contains(q) ||
                v.Meaning.ToLower().Contains(q) ||
                v.SourceNovelTitle.ToLower().Contains(q));
        }

        if (SelectedJlptFilter != "Tất cả JLPT" && !string.IsNullOrWhiteSpace(SelectedJlptFilter))
        {
            query = query.Where(v => v.JlptLevel.ToString() == SelectedJlptFilter);
        }

        if (DueOnlyFilter)
        {
            var now = DateTime.UtcNow;
            query = query.Where(v => v.NextReviewDate <= now);
        }

        FilteredWords.Clear();
        foreach (var item in query)
        {
            FilteredWords.Add(item);
        }

        if (SelectedWord == null && FilteredWords.Any())
        {
            SelectedWord = FilteredWords.First();
        }
    }

    [RelayCommand]
    public void SelectWord(VocabularyItem? item)
    {
        SelectedWord = item;
    }

    [RelayCommand]
    public async Task DeleteWord(VocabularyItem? item)
    {
        if (item == null) return;
        await _vocabularyRepository.DeleteAsync(item.Id);
        await LoadVocabularyAsync();
    }

    // Tab Switchers
    [RelayCommand]
    public void SwitchToNotebook()
    {
        ActiveTab = "Notebook";
    }

    [RelayCommand]
    public async Task SwitchToReview()
    {
        ActiveTab = "Review";
        await StartReviewSession();
    }

    // Modal Add Word Actions
    [RelayCommand]
    public void OpenAddModal()
    {
        NewWord = string.Empty;
        NewReading = string.Empty;
        NewMeaning = string.Empty;
        NewJlptLevel = JlptLevel.N5;
        NewContextSentence = string.Empty;
        NewNotes = string.Empty;
        AddWordErrorMessage = string.Empty;
        IsAddModalOpen = true;
    }

    [RelayCommand]
    public void CloseAddModal()
    {
        IsAddModalOpen = false;
        AddWordErrorMessage = string.Empty;
    }

    [RelayCommand]
    public async Task ConfirmAddWord()
    {
        AddWordErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(NewWord))
        {
            AddWordErrorMessage = "Vui lòng nhập từ vựng (Word).";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewMeaning))
        {
            AddWordErrorMessage = "Vui lòng nhập định nghĩa tiếng Việt.";
            return;
        }

        if (await _vocabularyRepository.ExistsAsync(NewWord.Trim()))
        {
            AddWordErrorMessage = $"Từ '{NewWord.Trim()}' đã có trong sổ tay của bạn.";
            return;
        }

        var item = new VocabularyItem
        {
            Word = NewWord.Trim(),
            Reading = string.IsNullOrWhiteSpace(NewReading) ? NewWord.Trim() : NewReading.Trim(),
            Meaning = NewMeaning.Trim(),
            JlptLevel = NewJlptLevel,
            ContextSentence = NewContextSentence.Trim(),
            SourceNovelTitle = "Thêm thủ công",
            Notes = NewNotes.Trim(),
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

        CloseAddModal();
        await LoadVocabularyAsync();
    }

    // Flashcard SRS Review Actions
    [RelayCommand]
    public async Task StartReviewSession()
    {
        var due = await _vocabularyRepository.GetDueReviewsAsync();
        _dueWords = due.ToList();

        if (_dueWords.Count == 0)
        {
            // If no words are due, provide all words for study practice
            var all = await _vocabularyRepository.GetAllAsync();
            _dueWords = all.Take(10).ToList();
        }

        TotalReviewSessionCards = _dueWords.Count;
        CurrentCardIndex = 0;
        SessionReviewedCount = 0;
        SessionXpEarned = 0;
        IsReviewSessionCompleted = false;

        AdvanceToNextCard();
    }

    private void AdvanceToNextCard()
    {
        IsAnswerRevealed = false;

        if (CurrentCardIndex < _dueWords.Count)
        {
            CurrentCard = _dueWords[CurrentCardIndex];
        }
        else
        {
            // Session completed!
            CurrentCard = null;
            IsReviewSessionCompleted = true;
        }
    }

    [RelayCommand]
    public void RevealAnswer()
    {
        IsAnswerRevealed = true;
    }

    [RelayCommand]
    public async Task SubmitRating(string ratingStr)
    {
        if (CurrentCard == null) return;

        if (!Enum.TryParse<ReviewRating>(ratingStr, true, out var rating))
        {
            rating = ReviewRating.Good;
        }

        // Calculate SM-2 interval and ease factor
        var srsResult = SpacedRepetitionCalculator.CalculateNextReview(
            currentIntervalDays: CurrentCard.IntervalDays,
            currentEaseFactor: CurrentCard.EaseFactor,
            currentRepetitions: CurrentCard.Repetitions,
            rating: rating,
            reviewDateUtc: DateTime.UtcNow);

        // Record to DB
        await _vocabularyRepository.RecordReviewAsync(CurrentCard.Id, rating, srsResult);

        // Reward XP based on rating
        int earnedXp = rating switch
        {
            ReviewRating.Easy => 15,
            ReviewRating.Good => 12,
            ReviewRating.Hard => 8,
            _ => 4
        };

        SessionXpEarned += earnedXp;
        SessionReviewedCount++;

        await _userProgressRepository.AddXpAsync(earnedXp);
        await _dailyQuestRepository.IncrementQuestProgressAsync(QuestType.ReviewCards, 1);
        await _mainViewModel.LoadUserProgressAsync();

        CurrentCardIndex++;
        AdvanceToNextCard();
    }
}
