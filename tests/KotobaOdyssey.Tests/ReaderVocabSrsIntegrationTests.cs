using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Core.Services;
using KotobaOdyssey.Data;
using KotobaOdyssey.Data.Repositories;
using KotobaOdyssey.Data.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KotobaOdyssey.Tests;

public class ReaderVocabSrsIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KotobaDbContext _context;

    public ReaderVocabSrsIntegrationTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<KotobaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new KotobaDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task DictionaryLookup_ExactAndDeinflected_ShouldReturnValidEntry()
    {
        // 1. Arrange
        var dictService = new JapaneseDictionaryService();

        // 2. Exact match
        var exact = await dictService.LookupAsync("銀河");
        Assert.NotNull(exact);
        Assert.Equal("銀河", exact.Term);
        Assert.Equal("ぎんが", exact.Reading);
        Assert.Equal(JlptLevel.N3, exact.JlptLevel);

        // 3. De-inflected match (見ました -> 見る)
        var deinflected = await dictService.LookupAsync("見ました");
        Assert.NotNull(deinflected);
        Assert.Equal("見る", deinflected.Term);
        Assert.Equal("みる", deinflected.Reading);
    }

    [Fact]
    public async Task ReaderToVocab_SaveWord_ShouldPersistWithContextAndPreventDuplicates()
    {
        // 1. Arrange
        var vocabRepo = new VocabularyRepository(_context);
        var progressRepo = new UserProgressRepository(_context);

        string word = "銀河";
        string reading = "ぎんが";
        string contextSentence = "白くけむった銀河の帯のようなところを指しながら。";
        string sourceNovel = "銀河鉄道の夜";
        int chapterId = 1;

        // Verify initially does not exist
        Assert.False(await vocabRepo.ExistsAsync(word));

        // 2. Act: Save word
        var item = new VocabularyItem
        {
            Word = word,
            Reading = reading,
            Meaning = "Dải Ngân Hà",
            JlptLevel = JlptLevel.N3,
            ContextSentence = contextSentence,
            SourceNovelTitle = sourceNovel,
            SourceChapterId = chapterId,
            CreatedAt = DateTime.UtcNow,
            NextReviewDate = DateTime.UtcNow,
            IntervalDays = 0,
            EaseFactor = 2.5,
            Repetitions = 0
        };

        var initialProgress = await progressRepo.GetProgressAsync();
        int initialWords = initialProgress.TotalWordsLearned;

        var saved = await vocabRepo.AddAsync(item);
        await progressRepo.IncrementWordsLearnedAsync();

        // 3. Assert
        Assert.True(saved.Id > 0);
        Assert.True(await vocabRepo.ExistsAsync(word));
        Assert.True(await vocabRepo.ExistsAsync("  銀河  ")); // case / whitespace check

        var retrieved = await vocabRepo.GetByIdAsync(saved.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(contextSentence, retrieved.ContextSentence);
        Assert.Equal(sourceNovel, retrieved.SourceNovelTitle);
        Assert.Equal(chapterId, retrieved.SourceChapterId);

        var progress = await progressRepo.GetProgressAsync();
        Assert.Equal(initialWords + 1, progress.TotalWordsLearned);

        // 4. Duplicate Check: Cannot add duplicate if ExistsAsync is checked
        bool alreadyExists = await vocabRepo.ExistsAsync(word);
        Assert.True(alreadyExists);
    }

    [Fact]
    public async Task VocabReview_SM2Algorithm_ShouldUpdateIntervalAndRecordLog()
    {
        // 1. Arrange
        var vocabRepo = new VocabularyRepository(_context);

        var item = new VocabularyItem
        {
            Word = "星座",
            Reading = "せいざ",
            Meaning = "Chòm sao",
            JlptLevel = JlptLevel.N3,
            ContextSentence = "黒板に吊した大きな黒い星座の図。",
            SourceNovelTitle = "銀河鉄道の夜",
            CreatedAt = DateTime.UtcNow,
            NextReviewDate = DateTime.UtcNow,
            IntervalDays = 0,
            EaseFactor = 2.5,
            Repetitions = 0
        };

        var saved = await vocabRepo.AddAsync(item);

        // 2. Act: First Review - Grade 'Good' (3)
        var calc1 = SpacedRepetitionCalculator.CalculateNextReview(
            saved.IntervalDays, saved.EaseFactor, saved.Repetitions, ReviewRating.Good, DateTime.UtcNow);

        var log1 = await vocabRepo.RecordReviewAsync(saved.Id, ReviewRating.Good, calc1);

        // 3. Assert First Review
        Assert.NotNull(log1);
        Assert.Equal(1, calc1.NewIntervalDays);
        Assert.Equal(1, calc1.NewRepetitions);
        Assert.Equal(ReviewRating.Good, log1.Rating);

        var updated1 = await vocabRepo.GetByIdAsync(saved.Id);
        Assert.NotNull(updated1);
        Assert.Equal(1, updated1.IntervalDays);
        Assert.Equal(1, updated1.Repetitions);

        // 4. Act: Second Review - Grade 'Easy' (4)
        var calc2 = SpacedRepetitionCalculator.CalculateNextReview(
            updated1.IntervalDays, updated1.EaseFactor, updated1.Repetitions, ReviewRating.Easy, DateTime.UtcNow);

        var log2 = await vocabRepo.RecordReviewAsync(saved.Id, ReviewRating.Easy, calc2);

        // 5. Assert Second Review
        Assert.Equal(6, calc2.NewIntervalDays);
        Assert.Equal(2, calc2.NewRepetitions);
        Assert.True(calc2.NewEaseFactor >= 2.5);

        var updated2 = await vocabRepo.GetByIdAsync(saved.Id);
        Assert.NotNull(updated2);
        Assert.Equal(6, updated2.IntervalDays);
        Assert.Equal(2, updated2.Repetitions);
        Assert.Equal(2, updated2.ReviewHistory.Count);
    }
}
