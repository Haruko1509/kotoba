using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Data;
using KotobaOdyssey.Data.Repositories;
using KotobaOdyssey.Data.Services;
using KotobaOdyssey.UI.Services;
using KotobaOdyssey.UI.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KotobaOdyssey.Tests;

public class ChapterQuizAndProgressTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KotobaDbContext _context;
    private readonly ServiceProvider _serviceProvider;
    private readonly NovelRepository _novelRepo;
    private readonly VocabularyRepository _vocabRepo;
    private readonly UserProgressRepository _progressRepo;
    private readonly DailyQuestRepository _questRepo;
    private readonly ChapterPracticeService _practiceService;

    public ChapterQuizAndProgressTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<KotobaDbContext>(options =>
        {
            options.UseSqlite(_connection);
        });

        services.AddScoped<INovelRepository, NovelRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<IVocabularyRepository, VocabularyRepository>();
        services.AddScoped<IUserProgressRepository, UserProgressRepository>();
        services.AddScoped<IDailyQuestRepository, DailyQuestRepository>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddScoped<IJapaneseDictionaryService, JapaneseDictionaryService>();
        services.AddScoped<IJapaneseTextAnalyzer, JapaneseTextAnalyzer>();
        services.AddScoped<IChapterPracticeService, ChapterPracticeService>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>(sp =>
            new DatabaseInitializer(
                sp.GetRequiredService<KotobaDbContext>(),
                NullLogger<DatabaseInitializer>.Instance));

        services.AddSingleton<MainViewModel>();
        services.AddTransient<DashboardViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        _context = _serviceProvider.GetRequiredService<KotobaDbContext>();
        _context.Database.EnsureCreated();

        // Seed initial novels, vocabulary, chapters
        var initializer = _serviceProvider.GetRequiredService<IDatabaseInitializer>();
        initializer.InitializeAsync().GetAwaiter().GetResult();

        _novelRepo = new NovelRepository(_context);
        _vocabRepo = new VocabularyRepository(_context);
        _progressRepo = new UserProgressRepository(_context);
        _questRepo = new DailyQuestRepository(_context, _progressRepo);

        _practiceService = new ChapterPracticeService(
            _novelRepo,
            _vocabRepo,
            _progressRepo,
            NullLogger<ChapterPracticeService>.Instance,
            _context);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetChapterQuizAsync_ReturnsCuratedQuestionsWithValidCategories()
    {
        // Act
        var quiz = await _practiceService.GetChapterQuizAsync(novelId: 1, chapterNumber: 1);

        // Assert
        Assert.NotNull(quiz);
        Assert.True(quiz.Questions.Count >= 5, "Chapter quiz should provide 5-10 questions.");
        var validCategories = new HashSet<string> { "ReadingComprehension", "Vocabulary", "Grammar" };

        foreach (var q in quiz.Questions)
        {
            Assert.Contains(q.Category, validCategories);
            Assert.False(string.IsNullOrWhiteSpace(q.TargetGrammarOrVocab));
            Assert.False(string.IsNullOrWhiteSpace(q.QuestionJp));
            Assert.True(q.Options.Count >= 2);
            Assert.True(q.CorrectIndex >= 0 && q.CorrectIndex < q.Options.Count);
            Assert.False(string.IsNullOrWhiteSpace(q.Explanation));
        }
    }

    [Fact]
    public async Task SubmitQuizAsync_CalculatesCategoryAccuracyBreakdown()
    {
        // Arrange
        var quiz = await _practiceService.GetChapterQuizAsync(1, 1);
        var answers = new Dictionary<int, int>();

        // Intentionally answer correctly for Reading and Vocabulary, but incorrectly for Grammar
        foreach (var q in quiz.Questions)
        {
            if (q.Category == "Grammar")
            {
                // Answer wrong option
                answers[q.QuestionId] = (q.CorrectIndex + 1) % q.Options.Count;
            }
            else
            {
                // Answer correctly
                answers[q.QuestionId] = q.CorrectIndex;
            }
        }

        // Act
        var result = await _practiceService.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = answers
        });

        // Assert
        Assert.True(result.Success);
        Assert.Equal(quiz.Questions.Count, result.TotalQuestions);
        Assert.NotEmpty(result.QuestionReviews);

        if (result.ReadingTotal > 0)
        {
            Assert.Equal(result.ReadingTotal, result.ReadingCorrect);
            Assert.Equal(100, result.ReadingScorePercent);
        }

        if (result.VocabTotal > 0)
        {
            Assert.Equal(result.VocabTotal, result.VocabCorrect);
            Assert.Equal(100, result.VocabScorePercent);
        }

        if (result.GrammarTotal > 0)
        {
            Assert.Equal(0, result.GrammarCorrect);
            Assert.Equal(0, result.GrammarScorePercent);
        }
    }

    [Fact]
    public async Task SubmitQuizAsync_PersistsQuizAttempt_ToDatabase()
    {
        // Arrange
        var quiz = await _practiceService.GetChapterQuizAsync(1, 1);
        var answers = quiz.Questions.ToDictionary(q => q.QuestionId, q => q.CorrectIndex);

        // Act
        var result = await _practiceService.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = answers
        });

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Passed);

        // Verify SQLite database persistence
        var attempts = await _context.ChapterQuizAttempts.ToListAsync();
        Assert.Single(attempts);
        var attempt = attempts[0];
        Assert.Equal(1, attempt.NovelId);
        Assert.Equal(1, attempt.ChapterNumber);
        Assert.Equal(quiz.Questions.Count, attempt.TotalQuestions);
        Assert.Equal(quiz.Questions.Count, attempt.CorrectAnswers);
        Assert.Equal(100, attempt.ScorePercentage);
        Assert.True(attempt.Passed);
        Assert.True(attempt.XpAwarded > 0);
    }

    [Fact]
    public async Task GetLearningAnalyticsAsync_AccuratelyComputesMetricsAndPedagogicalInsights()
    {
        // Arrange: Seed novel with chapters (auto-generated IDs)
        var novel = new Novel
        {
            Title = "テスト小説",
            Author = "作者",
            JlptLevel = JlptLevel.N4,
            Chapters = new List<Chapter>
            {
                new Chapter { ChapterNumber = 1, Title = "第1章", IsCompleted = true, LastReadPosition = 500, Content = "テスト内容" },
                new Chapter { ChapterNumber = 2, Title = "第2章", IsCompleted = false, LastReadPosition = 120, Content = "テスト内容2" }
            }
        };
        await _novelRepo.AddAsync(novel);

        // Mastered: Repetitions >= 3, IntervalDays >= 21
        await _vocabRepo.AddAsync(new VocabularyItem
        {
            Word = "星",
            Reading = "ほし",
            Meaning = "ngôi sao",
            Repetitions = 4,
            IntervalDays = 25,
            EaseFactor = 2.6,
            NextReviewDate = DateTime.UtcNow.AddDays(10)
        });

        // Learning: Repetitions > 0 but not yet mastered
        await _vocabRepo.AddAsync(new VocabularyItem
        {
            Word = "銀河",
            Reading = "ぎんが",
            Meaning = "ngân hà",
            Repetitions = 2,
            IntervalDays = 6,
            EaseFactor = 2.4,
            NextReviewDate = DateTime.UtcNow.AddDays(2)
        });

        // New: Repetitions == 0
        await _vocabRepo.AddAsync(new VocabularyItem
        {
            Word = "黒板",
            Reading = "こくばん",
            Meaning = "bảng đen",
            Repetitions = 0,
            IntervalDays = 0,
            EaseFactor = 2.5,
            NextReviewDate = DateTime.UtcNow
        });

        // Seed quiz attempt with 4 correct out of 6
        _context.ChapterQuizAttempts.Add(new ChapterQuizAttempt
        {
            NovelId = novel.Id,
            ChapterNumber = 1,
            TotalQuestions = 6,
            CorrectAnswers = 4,
            ScorePercentage = 67,
            Passed = true,
            XpAwarded = 40,
            AttemptedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var analytics = await _practiceService.GetLearningAnalyticsAsync();

        // Assert Reading Progress
        Assert.NotNull(analytics);
        Assert.True(analytics.CompletedChapters >= 1);
        Assert.True(analytics.TotalNovels >= 1);
        Assert.True(analytics.OverallReadingProgressPercent > 0);

        // Assert Vocabulary Mastery
        Assert.True(analytics.TotalSavedWords >= 3);
        Assert.True(analytics.MasteredWords >= 1);
        Assert.True(analytics.LearningWords >= 1);
        Assert.True(analytics.NewWords >= 1);
        Assert.True(analytics.VocabularyRetentionRate > 0);

        // Assert Quiz Performance
        Assert.Equal(1, analytics.TotalQuizzesAttempted);
        Assert.Equal(1, analytics.QuizzesPassed);
        Assert.Equal(67, analytics.AverageQuizScorePercent);
        Assert.Equal(67, analytics.OverallAccuracyPercent);

        // Assert Insights
        Assert.NotEmpty(analytics.Strengths);
        Assert.NotEmpty(analytics.Weaknesses);
        Assert.NotEmpty(analytics.ActionableRecommendations);
    }

    [Fact]
    public async Task DashboardViewModel_LoadsAndExposesLearningAnalytics()
    {
        // Arrange
        var userProgress = await _progressRepo.GetProgressAsync();
        userProgress.CurrentXp = 250;
        userProgress.Level = 3;
        userProgress.StreakDays = 7;
        userProgress.TodayStudyMinutes = 25;
        await _progressRepo.UpdateProgressAsync(userProgress);

        // Seed 1 mastered word
        await _vocabRepo.AddAsync(new VocabularyItem
        {
            Word = "先生",
            Reading = "せんせい",
            Meaning = "thầy cô giáo",
            Repetitions = 5,
            IntervalDays = 30,
            EaseFactor = 2.5
        });

        var dashboardVm = _serviceProvider.GetRequiredService<DashboardViewModel>();

        // Act
        await dashboardVm.LoadDashboardAsync();

        // Assert
        Assert.Equal(7, dashboardVm.StreakDays);
        Assert.Equal(25, dashboardVm.TodayStudyMinutes);
        Assert.True(dashboardVm.MasteredWordsCount >= 1);
        Assert.True(dashboardVm.VocabularyRetentionRate > 0);
        Assert.False(string.IsNullOrWhiteSpace(dashboardVm.PrimaryStrength));
        Assert.False(string.IsNullOrWhiteSpace(dashboardVm.PrimaryWeakness));
        Assert.False(string.IsNullOrWhiteSpace(dashboardVm.PrimaryRecommendation));
    }
}
