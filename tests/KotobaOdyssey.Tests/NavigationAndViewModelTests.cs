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
using Microsoft.Extensions.Logging.Abstractions;

namespace KotobaOdyssey.Tests;

public class NavigationAndViewModelTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KotobaDbContext _context;
    private readonly ServiceProvider _serviceProvider;

    public NavigationAndViewModelTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var services = new ServiceCollection();

        services.AddDbContext<KotobaDbContext>(options =>
        {
            options.UseSqlite(_connection);
        });

        services.AddScoped<INovelRepository, NovelRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<INovelImportService, NovelImportService>(sp =>
            new NovelImportService(
                sp.GetRequiredService<INovelRepository>(),
                NullLogger<NovelImportService>.Instance));

        services.AddScoped<IVocabularyRepository, VocabularyRepository>();
        services.AddScoped<IUserProgressRepository, UserProgressRepository>();
        services.AddScoped<IDailyQuestRepository, DailyQuestRepository>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>(sp =>
            new DatabaseInitializer(
                sp.GetRequiredService<KotobaDbContext>(),
                NullLogger<DatabaseInitializer>.Instance));

        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<ReaderViewModel>();
        services.AddTransient<VocabularyViewModel>();
        services.AddTransient<VisualNovelViewModel>();
        services.AddTransient<SettingsViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        _context = _serviceProvider.GetRequiredService<KotobaDbContext>();
        _context.Database.EnsureCreated();

        // Seed initial test data
        var initializer = _serviceProvider.GetRequiredService<IDatabaseInitializer>();
        initializer.InitializeAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void NavigationService_NavigatingToLibrary_UpdatesStateAndRaisesEvent()
    {
        var navService = _serviceProvider.GetRequiredService<INavigationService>();
        bool eventFired = false;
        navService.CurrentViewChanged += () => eventFired = true;

        navService.NavigateTo<LibraryViewModel>();

        Assert.True(eventFired);
        Assert.NotNull(navService.CurrentView);
        Assert.IsType<LibraryViewModel>(navService.CurrentView);
        Assert.Equal("Library", navService.CurrentPageKey);
    }

    [Fact]
    public async Task DashboardViewModel_LoadDashboard_PopulatesRealDataFromDatabase()
    {
        var dashboardVm = _serviceProvider.GetRequiredService<DashboardViewModel>();

        await dashboardVm.LoadDashboardAsync();

        Assert.True(dashboardVm.ActiveNovelsCount >= 4);
        Assert.True(dashboardVm.SavedWordsCount >= 4);
        Assert.True(dashboardVm.DueReviewsCount >= 2);
        Assert.True(dashboardVm.StreakDays >= 5);
        Assert.NotNull(dashboardVm.CurrentNovel);
        Assert.NotEmpty(dashboardVm.DailyQuests);
        Assert.NotEmpty(dashboardVm.RecentNovels);
    }

    [Fact]
    public async Task DashboardViewModel_ClaimQuest_UpdatesProgressAndRefreshesXp()
    {
        var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
        await mainVm.LoadUserProgressAsync();
        int initialXp = mainVm.CurrentXp;

        var dashboardVm = _serviceProvider.GetRequiredService<DashboardViewModel>();
        await dashboardVm.LoadDashboardAsync();

        var claimableQuest = dashboardVm.DailyQuests.FirstOrDefault(q => q.IsCompleted && !q.IsClaimed);
        Assert.NotNull(claimableQuest);

        await dashboardVm.ClaimQuest(claimableQuest);

        Assert.True(claimableQuest.IsClaimed);
        Assert.Equal(initialXp + claimableQuest.RewardXp, mainVm.CurrentXp);
    }

    [Fact]
    public async Task LibraryViewModel_SearchAndFiltering_WorksCorrectly()
    {
        var libraryVm = _serviceProvider.GetRequiredService<LibraryViewModel>();
        await libraryVm.LoadNovelsAsync();

        Assert.True(libraryVm.FilteredNovels.Count >= 4);

        // Search for '銀河'
        libraryVm.SearchQuery = "銀河";
        Assert.Single(libraryVm.FilteredNovels);
        Assert.Equal("銀河鉄道の夜", libraryVm.FilteredNovels[0].Title);

        // Clear search
        libraryVm.SearchQuery = string.Empty;
        Assert.True(libraryVm.FilteredNovels.Count >= 4);

        // Filter by JLPT N5
        libraryVm.SelectedJlptFilter = "N5";
        Assert.All(libraryVm.FilteredNovels, n => Assert.Equal(Core.Enums.JlptLevel.N5, n.JlptLevel));
    }

    [Fact]
    public async Task ReaderViewModel_LoadNovelAndBookmark_OperatesCorrectly()
    {
        var novelRepo = _serviceProvider.GetRequiredService<INovelRepository>();
        var novels = await novelRepo.GetAllAsync();
        var targetNovel = novels.First();

        var readerVm = _serviceProvider.GetRequiredService<ReaderViewModel>();
        await readerVm.LoadNovelAsync(targetNovel);

        Assert.NotNull(readerVm.Novel);
        Assert.NotNull(readerVm.CurrentChapter);
        Assert.NotEmpty(readerVm.Chapters);

        // Add Bookmark
        readerVm.NewBookmarkNote = "Ghi nhớ cảnh học buổi chiều";
        await readerVm.AddBookmark();

        Assert.Single(readerVm.Bookmarks);
        Assert.Equal("Ghi nhớ cảnh học buổi chiều", readerVm.Bookmarks[0].Note);

        // Update progress
        await readerVm.SaveCurrentProgressAsync(100);
        Assert.True(readerVm.ReadingProgressPercent > 0);
    }
}
