using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Data;
using KotobaOdyssey.Data.Repositories;
using KotobaOdyssey.Data.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace KotobaOdyssey.Tests;

public class NovelImportServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KotobaDbContext _context;
    private readonly NovelRepository _novelRepository;
    private readonly NovelImportService _importService;

    public NovelImportServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<KotobaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new KotobaDbContext(options);
        _context.Database.EnsureCreated();

        _novelRepository = new NovelRepository(_context);
        _importService = new NovelImportService(_novelRepository, NullLogger<NovelImportService>.Instance);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task ImportFromJson_ValidJson_SuccessfullyImportsNovelWithChapters()
    {
        string json = @"
{
    ""title"": ""風立ちぬ"",
    ""japaneseTitle"": ""風立ちぬ (The Wind Has Risen)"",
    ""author"": ""堀辰雄"",
    ""genre"": ""Romance / Classic"",
    ""jlptLevel"": ""N3"",
    ""chapters"": [
        {
            ""chapterNumber"": 1,
            ""title"": ""序曲"",
            ""content"": ""風立ちぬ、いざ生きめやも。""
        },
        {
            ""chapterNumber"": 2,
            ""title"": ""春"",
            ""content"": ""私たちは高原のサナトリウムにいた。""
        }
    ]
}";

        var result = await _importService.ImportFromJsonAsync(json);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ImportedNovel);
        Assert.Equal("風立ちぬ", result.ImportedNovel.Title);
        Assert.Equal("堀辰雄", result.ImportedNovel.Author);
        Assert.Equal(JlptLevel.N3, result.ImportedNovel.JlptLevel);
        Assert.False(result.ImportedNovel.IsDemoData); // User-imported!
        Assert.Equal(2, result.ImportedNovel.Chapters.Count);

        // Verify in DB
        var inDb = await _novelRepository.GetByIdAsync(result.ImportedNovel.Id);
        Assert.NotNull(inDb);
        Assert.Equal(2, inDb.Chapters.Count);
    }

    [Fact]
    public async Task ImportFromJson_MalformedJson_ReturnsFailureWithoutException()
    {
        string malformedJson = "{ \"title\": \"Broken JSON... ";

        var result = await _importService.ImportFromJsonAsync(malformedJson);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("không hợp lệ", result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromJson_MissingTitle_ReturnsValidationErrors()
    {
        string invalidJson = @"
{
    ""title"": """",
    ""chapters"": [
        { ""chapterNumber"": 1, ""title"": ""Ch 1"", ""content"": ""Some content"" }
    ]
}";

        var result = await _importService.ImportFromJsonAsync(invalidJson);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.Contains("Title"));
    }

    [Fact]
    public async Task ImportFromJson_DuplicateTitle_ReturnsFailure()
    {
        string json = @"
{
    ""title"": ""Duplicate Novel"",
    ""chapters"": [
        { ""chapterNumber"": 1, ""title"": ""Ch 1"", ""content"": ""Content here"" }
    ]
}";

        // First import -> success
        var first = await _importService.ImportFromJsonAsync(json);
        Assert.True(first.IsSuccess);

        // Second import with same title -> failure
        var second = await _importService.ImportFromJsonAsync(json);
        Assert.False(second.IsSuccess);
        Assert.Contains("đã tồn tại", second.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromTxt_WithChapterHeaders_DetectsAndSplitsChapters()
    {
        string txt = @"第一章 はじまり
これは最初の物語です。

第二章 たびだち
そして少年は旅に出ました。";

        var result = await _importService.ImportFromTxtAsync(
            textContent: txt,
            title: "少年の旅",
            author: "テスト作者",
            jlptLevel: JlptLevel.N4,
            genre: "Adventure");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ImportedNovel);
        Assert.Equal(2, result.ImportedNovel.Chapters.Count);
        Assert.Contains("第一章", result.ImportedNovel.Chapters.First().Title);
        Assert.Contains("第二章", result.ImportedNovel.Chapters.Last().Title);
    }

    [Fact]
    public async Task ChapterProgress_UpdateAndRestore_PersistsCorrectly()
    {
        var novel = new Novel
        {
            Title = "Progress Test",
            JapaneseTitle = "進捗テスト",
            IsDemoData = false
        };
        var chapter = new Chapter
        {
            ChapterNumber = 1,
            Title = "第1話",
            Content = "これは進捗のテストです。",
            WordCount = 20,
            LastReadPosition = 0
        };
        novel.Chapters.Add(chapter);
        await _novelRepository.AddAsync(novel);

        // Update progress
        await _novelRepository.UpdateChapterProgressAsync(chapter.Id, lastReadPosition: 12, isCompleted: true);

        // Restore & verify
        var updatedChapter = await _novelRepository.GetChapterByIdAsync(chapter.Id);
        Assert.NotNull(updatedChapter);
        Assert.Equal(12, updatedChapter.LastReadPosition);
        Assert.True(updatedChapter.IsCompleted);
        Assert.NotNull(updatedChapter.LastReadAt);
    }

    [Fact]
    public async Task Bookmark_AddAndRetrieve_WorksCorrectly()
    {
        var novel = new Novel { Title = "Bookmark Test", IsDemoData = false };
        var chapter = new Chapter { Title = "Chapter 1", Content = "Test text", WordCount = 10 };
        novel.Chapters.Add(chapter);
        await _novelRepository.AddAsync(novel);

        var bookmarkRepo = new BookmarkRepository(_context);
        var bm = new Bookmark
        {
            ChapterId = chapter.Id,
            CharacterPosition = 5,
            Snippet = "Test snippet",
            Note = "Nhớ từ này"
        };

        var added = await bookmarkRepo.AddAsync(bm);
        Assert.True(added.Id > 0);

        var bookmarks = await bookmarkRepo.GetByChapterIdAsync(chapter.Id);
        Assert.Single(bookmarks);
        Assert.Equal("Nhớ từ này", bookmarks[0].Note);

        // Delete
        await bookmarkRepo.DeleteAsync(added.Id);
        var remaining = await bookmarkRepo.GetByChapterIdAsync(chapter.Id);
        Assert.Empty(remaining);
    }
}
