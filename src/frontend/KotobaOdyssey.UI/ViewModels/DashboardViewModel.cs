using KotobaOdyssey.Core.DTOs;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;

namespace KotobaOdyssey.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly INovelRepository _novelRepository;
    private readonly IVocabularyRepository _vocabularyRepository;
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly IDailyQuestRepository _dailyQuestRepository;
    private readonly INavigationService _navigationService;
    private readonly MainViewModel _mainViewModel;
    private readonly IChapterPracticeService? _practiceService;

    [ObservableProperty]
    private Novel? _currentNovel;

    [ObservableProperty]
    private Chapter? _currentChapter;

    [ObservableProperty]
    private int _currentChapterProgressPercent = 35;

    [ObservableProperty]
    private int _activeNovelsCount;

    [ObservableProperty]
    private int _savedWordsCount;

    [ObservableProperty]
    private int _dueReviewsCount;

    [ObservableProperty]
    private int _todayStudyMinutes;

    [ObservableProperty]
    private int _streakDays;

    // Learning Analytics & Mastery Properties
    [ObservableProperty]
    private int _completedChaptersCount;

    [ObservableProperty]
    private int _masteredWordsCount;

    [ObservableProperty]
    private int _learningWordsCount;

    [ObservableProperty]
    private int _newWordsCount;

    [ObservableProperty]
    private double _vocabularyRetentionRate = 100.0;

    [ObservableProperty]
    private int _totalQuizzesAttempted;

    [ObservableProperty]
    private int _quizzesPassed;

    [ObservableProperty]
    private int _averageQuizScore;

    [ObservableProperty]
    private int _overallAccuracyPercent;

    [ObservableProperty]
    private string _primaryStrength = string.Empty;

    [ObservableProperty]
    private string _primaryWeakness = string.Empty;

    [ObservableProperty]
    private string _primaryRecommendation = string.Empty;

    public ObservableCollection<RecentQuizAttemptDto> RecentQuizzes { get; } = new();

    public ObservableCollection<DailyQuest> DailyQuests { get; } = new();
    public ObservableCollection<Novel> RecentNovels { get; } = new();

    public DashboardViewModel(
        INovelRepository novelRepository,
        IVocabularyRepository vocabularyRepository,
        IUserProgressRepository userProgressRepository,
        IDailyQuestRepository dailyQuestRepository,
        INavigationService navigationService,
        MainViewModel mainViewModel,
        IChapterPracticeService? practiceService = null)
    {
        _novelRepository = novelRepository;
        _vocabularyRepository = vocabularyRepository;
        _userProgressRepository = userProgressRepository;
        _dailyQuestRepository = dailyQuestRepository;
        _navigationService = navigationService;
        _mainViewModel = mainViewModel;
        _practiceService = practiceService;
    }

    public async Task LoadDashboardAsync()
    {
        IsBusy = true;
        StatusText = "Loading celestial library...";

        try
        {
            // 1. Load User Progress
            var progress = await _userProgressRepository.GetProgressAsync();
            TodayStudyMinutes = progress.TodayStudyMinutes;
            StreakDays = progress.StreakDays;

            // 2. Load Novels
            var novels = await _novelRepository.GetAllAsync();
            ActiveNovelsCount = novels.Count;

            RecentNovels.Clear();
            foreach (var n in novels.Take(4))
            {
                RecentNovels.Add(n);
            }

            // Current / latest read novel
            CurrentNovel = await _novelRepository.GetLatestReadAsync();
            if (CurrentNovel?.Chapters.Any() == true)
            {
                CurrentChapter = CurrentNovel.Chapters
                    .OrderByDescending(c => c.LastReadAt ?? DateTime.MinValue)
                    .FirstOrDefault() ?? CurrentNovel.Chapters.First();

                if (CurrentChapter.WordCount > 0)
                {
                    CurrentChapterProgressPercent = Math.Clamp(
                        (int)Math.Round((double)CurrentChapter.LastReadPosition / CurrentChapter.WordCount * 100),
                        0, 100);
                }
            }

            // 3. Load Vocabulary Stats
            SavedWordsCount = await _vocabularyRepository.GetCountAsync();
            DueReviewsCount = await _vocabularyRepository.GetDueCountAsync();

            // 4. Load Daily Quests
            var quests = await _dailyQuestRepository.GetTodayQuestsAsync();
            DailyQuests.Clear();
            foreach (var q in quests)
            {
                DailyQuests.Add(q);
            }

            // 5. Load Real Learning Analytics
            if (_practiceService != null)
            {
                var analytics = await _practiceService.GetLearningAnalyticsAsync();
                CompletedChaptersCount = analytics.CompletedChapters;
                MasteredWordsCount = analytics.MasteredWords;
                LearningWordsCount = analytics.LearningWords;
                NewWordsCount = analytics.NewWords;
                VocabularyRetentionRate = analytics.VocabularyRetentionRate;
                TotalQuizzesAttempted = analytics.TotalQuizzesAttempted;
                QuizzesPassed = analytics.QuizzesPassed;
                AverageQuizScore = analytics.AverageQuizScorePercent;
                OverallAccuracyPercent = analytics.OverallAccuracyPercent;

                PrimaryStrength = analytics.Strengths.FirstOrDefault() ?? "Đang tích lũy vốn từ vựng văn học.";
                PrimaryWeakness = analytics.Weaknesses.FirstOrDefault(w => w.Contains("Ngữ pháp") || w.Contains("Từ vựng") || w.Contains("Đọc hiểu"))
                                  ?? analytics.Weaknesses.FirstOrDefault()
                                  ?? "Duy trì ôn tập đều đặn hàng ngày.";
                PrimaryRecommendation = analytics.ActionableRecommendations.FirstOrDefault() ?? "Đọc tiếp chương mới để rèn luyện.";

                RecentQuizzes.Clear();
                foreach (var q in analytics.RecentQuizHistory)
                {
                    RecentQuizzes.Add(q);
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void ContinueReading()
    {
        _navigationService.NavigateTo("Reader", CurrentNovel);
    }

    [RelayCommand]
    public void OpenLibrary()
    {
        _navigationService.NavigateTo("Library");
    }

    [RelayCommand]
    public void OpenVocabulary()
    {
        _navigationService.NavigateTo("Vocabulary");
    }

    [RelayCommand]
    public void SelectNovel(Novel? novel)
    {
        if (novel != null)
        {
            _navigationService.NavigateTo("Reader", novel);
        }
    }

    [RelayCommand]
    public async Task ClaimQuest(DailyQuest? quest)
    {
        if (quest == null || quest.IsClaimed || !quest.IsCompleted)
            return;

        bool success = await _dailyQuestRepository.ClaimRewardAsync(quest.Id);
        if (success)
        {
            quest.IsClaimed = true;
            // Notify UI collection change
            int index = DailyQuests.IndexOf(quest);
            if (index >= 0)
            {
                DailyQuests[index] = quest;
            }

            // Refresh user XP & Level in top bar
            await _mainViewModel.LoadUserProgressAsync();
        }
    }
}
