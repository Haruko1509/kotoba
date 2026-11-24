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

public class CoreLearningLoopTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KotobaDbContext _context;
    private readonly JapaneseDictionaryService _dictService;
    private readonly JapaneseTextAnalyzer _textAnalyzer;

    public CoreLearningLoopTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<KotobaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new KotobaDbContext(options);
        _context.Database.EnsureCreated();

        _dictService = new JapaneseDictionaryService();
        _textAnalyzer = new JapaneseTextAnalyzer(_dictService);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Theory]
    [InlineData("抜けると", "抜ける", "Thể điều kiện と")]
    [InlineData("青く", "青い", "Dạng phó từ")]
    [InlineData("走った", "走る", "Thể quá khứ")]
    [InlineData("走って", "走る", "Thể Te")]
    [InlineData("読めば", "読む", "Thể điều kiện ば")]
    [InlineData("読んだら", "読む", "Thể điều kiện たら")]
    [InlineData("書かれる", "書く", "Thể bị động")]
    [InlineData("食べさせる", "食べる", "Thể sai khiến")]
    [InlineData("読める", "読む", "Thể khả năng")]
    [InlineData("寒かった", "寒い", "Dạng quá khứ của tính từ")]
    public async Task DictionaryLookup_Deinflection_ReturnsBaseFormAndInflectionNote(
        string surface, string expectedBase, string expectedNoteSubstring)
    {
        var entry = await _dictService.LookupAsync(surface);

        Assert.NotNull(entry);
        Assert.Equal(expectedBase, entry.Term);
        Assert.Equal(surface, entry.OriginalSurface);
        Assert.False(string.IsNullOrWhiteSpace(entry.InflectionNote));
        Assert.Contains(expectedNoteSubstring, entry.InflectionNote);
    }

    [Fact]
    public void JapaneseTextAnalyzer_ExtractSentence_ExtractsExactSentenceAccurately()
    {
        string paragraph = "国境の長いトンネルを抜けると雪国であった。夜の底が白くなった！信号所に汽車が止まった。";

        // Sentence 1 containing 抜ける / 抜けると
        string sentence1 = _textAnalyzer.ExtractSentence(paragraph, "抜けると");
        Assert.Equal("国境の長いトンネルを抜けると雪国であった。", sentence1);

        // Sentence 2 containing 白く
        string sentence2 = _textAnalyzer.ExtractSentence(paragraph, "白く");
        Assert.Equal("夜の底が白くなった！", sentence2);

        // Sentence 3 containing 汽車
        string sentence3 = _textAnalyzer.ExtractSentence(paragraph, "汽車");
        Assert.Equal("信号所に汽車が止まった。", sentence3);
    }

    [Fact]
    public void JapaneseTextAnalyzer_ExtractSentence_HandlesEmptyOrNotFoundGracefully()
    {
        string paragraph = "ジョバンニは、学校の門を出ました。";

        // Unknown target word falls back to trimmed full text
        string fallback = _textAnalyzer.ExtractSentence(paragraph, "カムパネルラ");
        Assert.Equal(paragraph, fallback);

        // Empty paragraph
        string empty = _textAnalyzer.ExtractSentence("", "抜ける");
        Assert.Equal("", empty);
    }

    [Fact]
    public async Task VocabularyRepository_GetByWordAsync_FindsItemCaseInsensitiveAndTrimmed()
    {
        var repo = new VocabularyRepository(_context);

        var item = new VocabularyItem
        {
            Word = "夜空",
            Reading = "よぞら",
            Meaning = "Bầu trời đêm",
            JlptLevel = JlptLevel.N4,
            ContextSentence = "美しい夜空を見上げる。",
            CreatedAt = DateTime.UtcNow,
            NextReviewDate = DateTime.UtcNow,
            IntervalDays = 0,
            EaseFactor = 2.5,
            Repetitions = 0
        };
        await repo.AddAsync(item);

        var found1 = await repo.GetByWordAsync("夜空");
        var found2 = await repo.GetByWordAsync("  夜空  ");

        Assert.NotNull(found1);
        Assert.NotNull(found2);
        Assert.Equal("夜空", found1.Word);
        Assert.Equal(found1.Id, found2.Id);
    }

    [Fact]
    public async Task VocabularyRepository_ContextEnrichment_AppendsNewContextWithoutCreatingDuplicate()
    {
        var repo = new VocabularyRepository(_context);

        var original = new VocabularyItem
        {
            Word = "信号所",
            Reading = "しんごうじょ",
            Meaning = "Trạm tín hiệu",
            JlptLevel = JlptLevel.N2,
            ContextSentence = "信号所に汽車が止まった。",
            CreatedAt = DateTime.UtcNow,
            NextReviewDate = DateTime.UtcNow,
            IntervalDays = 0,
            EaseFactor = 2.5,
            Repetitions = 0
        };
        await repo.AddAsync(original);

        // Second encounter with a new context sentence
        string newContext = "向こうの信号所から赤い明かりが見えた。";
        var existing = await repo.GetByWordAsync("信号所");
        Assert.NotNull(existing);

        if (!existing.ContextSentence.Contains(newContext))
        {
            existing.ContextSentence = $"{existing.ContextSentence}\n\n{newContext}";
            await repo.UpdateAsync(existing);
        }

        var allItems = await repo.GetAllAsync();
        Assert.Single(allItems); // No duplicate entity

        var reloaded = await repo.GetByIdAsync(original.Id);
        Assert.NotNull(reloaded);
        Assert.Contains("信号所に汽車が止まった。", reloaded.ContextSentence);
        Assert.Contains("向こうの信号所から赤い明かりが見えた。", reloaded.ContextSentence);
    }

    [Fact]
    public async Task Vocabulary_NewWord_IsImmediatelyDueForReview()
    {
        var repo = new VocabularyRepository(_context);

        var item = new VocabularyItem
        {
            Word = "銀河",
            Reading = "ぎんが",
            Meaning = "Ngân hà",
            JlptLevel = JlptLevel.N3,
            ContextSentence = "銀河ステーションの放送が響いた。",
            CreatedAt = DateTime.UtcNow,
            NextReviewDate = DateTime.UtcNow, // Immediate reviewability
            IntervalDays = 0,
            EaseFactor = 2.5,
            Repetitions = 0
        };
        await repo.AddAsync(item);

        var dueItems = await repo.GetDueReviewsAsync();
        Assert.Contains(dueItems, i => i.Word == "銀河");
    }

    [Theory]
    [InlineData(ReviewRating.Again, 1, 0, 1.96)]
    [InlineData(ReviewRating.Hard, 1, 0, 2.18)]
    [InlineData(ReviewRating.Good, 1, 1, 2.50)]
    [InlineData(ReviewRating.Easy, 1, 1, 2.60)]
    public void SpacedRepetitionCalculator_AllRatings_ProduceCorrectIntervalsAndEase(
        ReviewRating rating, int expectedInterval, int expectedRepetitions, double expectedEase)
    {
        // First review starting from unreviewed card
        var result = SpacedRepetitionCalculator.CalculateNextReview(
            currentIntervalDays: 0,
            currentEaseFactor: 2.5,
            currentRepetitions: 0,
            rating: rating,
            reviewDateUtc: DateTime.UtcNow);

        Assert.Equal(expectedInterval, result.NewIntervalDays);
        Assert.Equal(expectedRepetitions, result.NewRepetitions);
        Assert.InRange(result.NewEaseFactor, expectedEase - 0.05, expectedEase + 0.05);
    }
}
