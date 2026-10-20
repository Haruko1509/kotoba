using System.IO;
using System.Windows;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Data;
using KotobaOdyssey.Data.Repositories;
using KotobaOdyssey.UI.Services;
using KotobaOdyssey.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KotobaOdyssey.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("ServiceProvider is not initialized.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();

        // 1. Initialize SQLite Database & Seed Data
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
            await dbInitializer.InitializeAsync();
        }

        // 2. Launch Main Window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // SQLite Database Path
        string appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KotobaOdyssey");

        Directory.CreateDirectory(appDataDir);
        string dbPath = Path.Combine(appDataDir, "kotoba.db");

        services.AddDbContext<KotobaDbContext>(options =>
        {
            options.UseSqlite($"Data Source={dbPath}");
        });

        // Repositories & Services
        services.AddScoped<INovelRepository, NovelRepository>();
        services.AddScoped<IVocabularyRepository, VocabularyRepository>();
        services.AddScoped<IUserProgressRepository, UserProgressRepository>();
        services.AddScoped<IDailyQuestRepository, DailyQuestRepository>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        // Navigation
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<ReaderViewModel>();
        services.AddTransient<VocabularyViewModel>();
        services.AddTransient<VisualNovelViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
