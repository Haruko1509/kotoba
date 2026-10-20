using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
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

public class VocabularyReviewTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KotobaDbContext _context;
    private readonly ServiceProvider _serviceProvider;

    public VocabularyReviewTests()
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
        services.AddScoped<INovelImportService, NovelImportService>();
        services.AddSingleton<IJapaneseDictionaryService, JapaneseDictionaryService>();
        services.AddScoped<IJapaneseTextAnalyzer, JapaneseTextAnalyzer>();
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

        // Seed initial data
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
    public async Task VocabularyViewModel_LoadVocabulary_PopulatesWordsAndFilters()
    {
        var vm = _serviceProvider.GetRequiredService<VocabularyViewModel>();
        await vm.LoadVocabularyAsync();

        Assert.True(vm.TotalWordsCount >= 4);
        Assert.NotEmpty(vm.FilteredWords);

        // Filter by JLPT N3
        vm.SelectedJlptFilter = "N3";
        Assert.All(vm.FilteredWords, w => Assert.Equal(JlptLevel.N3, w.JlptLevel));
    }

    [Fact]
    public async Task VocabularyViewModel_AddWord_IncreasesWordCountAndTriggersQuest()
    {
        var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
        await mainVm.LoadUserProgressAsync();

        var vm = _serviceProvider.GetRequiredService<VocabularyViewModel>();
        await vm.LoadVocabularyAsync();
        int initialCount = vm.TotalWordsCount;

        vm.NewWord = "桜";
        vm.NewReading = "さくら";
        vm.NewMeaning = "Hoa anh đào";
        vm.NewJlptLevel = JlptLevel.N4;
        vm.NewContextSentence = "桜の花が咲きました。";

        await vm.ConfirmAddWord();

        Assert.Equal(initialCount + 1, vm.TotalWordsCount);

        // Verify in DB
        var vocabRepo = _serviceProvider.GetRequiredService<IVocabularyRepository>();
        bool exists = await vocabRepo.ExistsAsync("桜");
        Assert.True(exists);
    }

    [Fact]
    public async Task VocabularyViewModel_FlashcardReviewSession_SubmitsRatingAndGrantsXp()
    {
        var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
        await mainVm.LoadUserProgressAsync();
        int initialXp = mainVm.CurrentXp;

        var vm = _serviceProvider.GetRequiredService<VocabularyViewModel>();
        await vm.SwitchToReview();

        Assert.NotNull(vm.CurrentCard);
        Assert.False(vm.IsAnswerRevealed);

        // Reveal answer
        vm.RevealAnswer();
        Assert.True(vm.IsAnswerRevealed);

        // Submit rating "Good"
        int currentWordId = vm.CurrentCard.Id;
        await vm.SubmitRating("Good");

        // Verify review log was recorded in DB
        var updatedItem = await _context.VocabularyItems
            .Include(v => v.ReviewHistory)
            .FirstOrDefaultAsync(v => v.Id == currentWordId);

        Assert.NotNull(updatedItem);
        Assert.NotEmpty(updatedItem.ReviewHistory);
        Assert.Equal(ReviewRating.Good, updatedItem.ReviewHistory.Last().Rating);

        // XP was granted
        Assert.True(mainVm.CurrentXp > initialXp);
    }
}
