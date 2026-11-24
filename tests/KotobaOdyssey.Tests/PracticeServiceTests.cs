using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Core.Services;
using KotobaOdyssey.Data.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KotobaOdyssey.Tests;

public class PracticeServiceTests
{
    private readonly FakeNovelRepository _novelRepo;
    private readonly FakeVocabRepository _vocabRepo;
    private readonly FakeProgressRepository _progressRepo;
    private readonly ChapterPracticeService _service;

    public PracticeServiceTests()
    {
        _novelRepo = new FakeNovelRepository();
        _vocabRepo = new FakeVocabRepository();
        _progressRepo = new FakeProgressRepository();
        var logger = NullLogger<ChapterPracticeService>.Instance;
        _service = new ChapterPracticeService(_novelRepo, _vocabRepo, _progressRepo, logger);
    }

    [Fact]
    public async Task GetChapterQuizAsync_ShouldReturnCuratedQuestions()
    {
        var quiz = await _service.GetChapterQuizAsync(1, 1);

        Assert.NotNull(quiz);
        Assert.NotEmpty(quiz.Questions);
        Assert.All(quiz.Questions, q =>
        {
            Assert.False(string.IsNullOrWhiteSpace(q.QuestionJp));
            Assert.NotEmpty(q.Options);
            Assert.True(q.CorrectIndex >= 0 && q.CorrectIndex < q.Options.Count);
            Assert.False(string.IsNullOrWhiteSpace(q.Explanation));
        });
    }

    [Fact]
    public async Task SubmitQuizAsync_AllCorrect_ShouldPassAndAwardXp()
    {
        var quiz = await _service.GetChapterQuizAsync(1, 1);
        var answers = new Dictionary<int, int>();
        foreach (var q in quiz.Questions)
        {
            answers[q.QuestionId] = q.CorrectIndex;
        }

        var result = await _service.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = answers
        });

        Assert.True(result.Success);
        Assert.True(result.Passed);
        Assert.Equal(100, result.ScorePercentage);
        Assert.Equal(quiz.Questions.Count, result.CorrectAnswers);
        Assert.True(result.XpEarned > 0);
        Assert.True(_progressRepo.AddedXp > 0);
    }

    [Fact]
    public async Task SubmitQuizAsync_ReSubmission_ShouldNotAwardDuplicateXp()
    {
        var quiz = await _service.GetChapterQuizAsync(1, 1);
        var answers = new Dictionary<int, int>();
        foreach (var q in quiz.Questions)
        {
            answers[q.QuestionId] = q.CorrectIndex;
        }

        // 1st submission
        var result1 = await _service.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = answers
        });
        Assert.True(result1.Passed);
        int initialXp = result1.XpEarned;
        Assert.True(initialXp > 0);

        // 2nd submission (Re-submit should award 0 XP)
        var result2 = await _service.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = answers
        });
        Assert.True(result2.Passed);
        Assert.Equal(0, result2.XpEarned); // No duplicate XP!
        Assert.Contains("trước đó", result2.FeedbackSummary);
    }

    [Fact]
    public async Task SubmitQuizAsync_FailedScore_ShouldNotAwardXpAndNotPass()
    {
        var quiz = await _service.GetChapterQuizAsync(1, 1);
        var wrongAnswers = new Dictionary<int, int>();
        foreach (var q in quiz.Questions)
        {
            // Choose an incorrect answer index
            wrongAnswers[q.QuestionId] = (q.CorrectIndex + 1) % q.Options.Count;
        }

        var result = await _service.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = wrongAnswers
        });

        Assert.False(result.Passed);
        Assert.Equal(0, result.XpEarned);
        Assert.Equal(0, result.CorrectAnswers);
        Assert.Contains("tối thiểu 60%", result.FeedbackSummary);
    }

    [Fact]
    public async Task GetDailyRecommendationAsync_ShouldReturnActiveRecommendations()
    {
        var rec = await _service.GetDailyRecommendationAsync();

        Assert.NotNull(rec);
        Assert.False(string.IsNullOrWhiteSpace(rec.RecommendedAction));
    }

    // Test Fakes
    private class FakeNovelRepository : INovelRepository
    {
        public Task<Novel?> GetByIdAsync(int id)
        {
            var novel = new Novel
            {
                Id = 1,
                Title = "銀河鉄道の夜",
                JlptLevel = JlptLevel.N3,
                Chapters = new List<Chapter>
                {
                    new Chapter { Id = 101, ChapterNumber = 1, Title = "午後の授業", Content = "星座の図" }
                }
            };
            return Task.FromResult<Novel?>(novel);
        }

        public Task<IReadOnlyList<Novel>> GetAllAsync()
        {
            var novel = new Novel
            {
                Id = 1,
                Title = "銀河鉄道の夜",
                Chapters = new List<Chapter>
                {
                    new Chapter { Id = 101, ChapterNumber = 1, Title = "午後の授業", IsCompleted = false }
                }
            };
            return Task.FromResult<IReadOnlyList<Novel>>(new List<Novel> { novel });
        }

        public Task<Novel?> GetLatestReadAsync() => Task.FromResult<Novel?>(null);
        public Task<IReadOnlyList<Novel>> GetFavoritesAsync() => Task.FromResult<IReadOnlyList<Novel>>(new List<Novel>());
        public Task<bool> ExistsWithTitleAsync(string title) => Task.FromResult(false);
        public Task<Novel> AddAsync(Novel novel) => Task.FromResult(novel);
        public Task UpdateAsync(Novel novel) => Task.CompletedTask;
        public Task DeleteAsync(int id) => Task.CompletedTask;
        public Task<Chapter?> GetChapterByIdAsync(int chapterId) => Task.FromResult<Chapter?>(null);
        public Task UpdateChapterProgressAsync(int chapterId, int lastReadPosition, bool isCompleted) => Task.CompletedTask;
    }

    private class FakeVocabRepository : IVocabularyRepository
    {
        public Task<IReadOnlyList<VocabularyItem>> GetDueReviewsAsync()
        {
            return Task.FromResult<IReadOnlyList<VocabularyItem>>(new List<VocabularyItem>
            {
                new VocabularyItem { Id = 1, Word = "銀河" }
            });
        }
        public Task<IReadOnlyList<VocabularyItem>> GetAllAsync() => Task.FromResult<IReadOnlyList<VocabularyItem>>(new List<VocabularyItem>());
        public Task<int> GetCountAsync() => Task.FromResult(1);
        public Task<int> GetDueCountAsync() => Task.FromResult(1);
        public Task<VocabularyItem?> GetByIdAsync(int id) => Task.FromResult<VocabularyItem?>(null);
        public Task<VocabularyItem?> GetByWordAsync(string word) => Task.FromResult<VocabularyItem?>(null);
        public Task<bool> ExistsAsync(string word) => Task.FromResult(false);
        public Task<VocabularyItem> AddAsync(VocabularyItem item) => Task.FromResult(item);
        public Task UpdateAsync(VocabularyItem item) => Task.CompletedTask;
        public Task DeleteAsync(int id) => Task.CompletedTask;
        public Task<ReviewLog> RecordReviewAsync(int vocabularyItemId, ReviewRating rating, SpacedRepetitionResult result) => Task.FromResult(new ReviewLog());
    }

    private class FakeProgressRepository : IUserProgressRepository
    {
        public int AddedXp { get; private set; }
        public Task<UserProgress> GetProgressAsync() => Task.FromResult(new UserProgress());
        public Task UpdateProgressAsync(UserProgress progress) => Task.CompletedTask;
        public Task AddXpAsync(int xp)
        {
            AddedXp += xp;
            return Task.CompletedTask;
        }
        public Task UpdateStreakAsync() => Task.CompletedTask;
        public Task AddStudyMinutesAsync(int minutes) => Task.CompletedTask;
        public Task IncrementWordsLearnedAsync() => Task.CompletedTask;
    }
}
