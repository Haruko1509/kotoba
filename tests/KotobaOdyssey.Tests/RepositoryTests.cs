using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Data;
using KotobaOdyssey.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace KotobaOdyssey.Tests;

public class RepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KotobaDbContext _context;

    public RepositoryTests()
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
    public async Task DatabaseInitializer_SeedsDemoDataProperly()
    {
        var initializer = new DatabaseInitializer(_context, NullLogger<DatabaseInitializer>.Instance);
        await initializer.InitializeAsync();

        var novels = await _context.Novels.Include(n => n.Chapters).ToListAsync();
        var vocabulary = await _context.VocabularyItems.ToListAsync();
        var userProgress = await _context.UserProgresses.FirstOrDefaultAsync();
        var dailyQuests = await _context.DailyQuests.ToListAsync();

        Assert.NotEmpty(novels);
        Assert.True(novels.Count >= 4);
        Assert.All(novels, n => Assert.True(n.IsDemoData));
        Assert.NotEmpty(vocabulary);
        Assert.NotNull(userProgress);
        Assert.NotEmpty(dailyQuests);
    }

    [Fact]
    public async Task NovelRepository_CRUD_FunctionsCorrectly()
    {
        var repo = new NovelRepository(_context);

        var novel = new Novel
        {
            Title = "テスト小説",
            JapaneseTitle = "テスト小説",
            RomajiTitle = "Test Shousetsu",
            Author = "Tester",
            Genre = "Mystery",
            JlptLevel = JlptLevel.N4,
            IsFavorite = true,
            IsDemoData = false
        };

        // Add
        var added = await repo.AddAsync(novel);
        Assert.True(added.Id > 0);

        // GetById
        var fetched = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal("テスト小説", fetched.Title);

        // Update
        fetched.Title = "テスト小説 (改)";
        await repo.UpdateAsync(fetched);
        var updated = await repo.GetByIdAsync(added.Id);
        Assert.Equal("テスト小説 (改)", updated!.Title);

        // Delete
        await repo.DeleteAsync(added.Id);
        var deleted = await repo.GetByIdAsync(added.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DailyQuestRepository_ClaimReward_PreventsDoubleClaimAndGrantsXp()
    {
        var progressRepo = new UserProgressRepository(_context);
        var questRepo = new DailyQuestRepository(_context, progressRepo);

        var progress = await progressRepo.GetProgressAsync();
        int initialXp = progress.CurrentXp;

        var quest = new DailyQuest
        {
            Title = "テストクエスト",
            Description = "Hoàn thành bài đọc",
            Type = QuestType.ReadMinutes,
            TargetCount = 10,
            CurrentCount = 10,
            RewardXp = 50,
            IsClaimed = false,
            QuestDate = DateTime.UtcNow.Date
        };
        _context.DailyQuests.Add(quest);
        await _context.SaveChangesAsync();

        // First claim -> should succeed
        bool firstClaim = await questRepo.ClaimRewardAsync(quest.Id);
        Assert.True(firstClaim);

        var updatedProgress = await progressRepo.GetProgressAsync();
        Assert.Equal(initialXp + 50, updatedProgress.CurrentXp);

        // Second claim attempt -> must fail (anti-duplicate claim!)
        bool secondClaim = await questRepo.ClaimRewardAsync(quest.Id);
        Assert.False(secondClaim);
    }
}
