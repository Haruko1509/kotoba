using System.IO;
using System.Windows;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Data;
using KotobaOdyssey.Data.Repositories;
using KotobaOdyssey.Data.Services;
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

        // Prevent premature shutdown while awaiting async initialization
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        DispatcherUnhandledException += (s, args) =>
        {
            try
            {
                File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] DispatcherUnhandledException: {args.Exception}\n");
                MessageBox.Show($"Dispatcher Error: {args.Exception.Message}\n{args.Exception.StackTrace}", "Kotoba Odyssey Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            }
            catch { }
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            try
            {
                File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] AppDomain UnhandledException: {args.ExceptionObject}\n");
                File.WriteAllText("crash.log", args.ExceptionObject?.ToString() ?? "Unknown error");
            }
            catch { }
        };

        try
        {
            File.WriteAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] Starting Kotoba Odyssey...\n");

            var services = new ServiceCollection();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();
            File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] ServiceProvider built.\n");

            // 1. Initialize SQLite Database & Seed Data
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
                File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] Initializing DB...\n");
                await dbInitializer.InitializeAsync();
                File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] DB initialized.\n");
            }

            // 2. Launch Main Window
            File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] Creating MainWindow...\n");
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Closing += (s, args) =>
            {
                File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] MainWindow.Closing called! Cancel={args.Cancel}\n");
            };
            mainWindow.Closed += (s, args) =>
            {
                File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] MainWindow.Closed called!\n");
            };
            // Keep ShutdownMode as OnExplicitShutdown until window is actually shown!
            File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] Showing MainWindow...\n");
            mainWindow.Show();
            File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] MainWindow.Show() called successfully! Handle={new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle}\n");
        }
        catch (Exception ex)
        {
            try
            {
                File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] EXCEPTION: {ex}\n");
                MessageBox.Show($"Lỗi khởi động Kotoba Odyssey:\n\n{ex.Message}\n\nXem chi tiết tại crash.log", 
                    "Kotoba Odyssey - Khởi động thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
            Shutdown();
        }
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
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<INovelImportService, NovelImportService>();
        services.AddSingleton<IJapaneseDictionaryService, JapaneseDictionaryService>();
        services.AddScoped<IJapaneseTextAnalyzer, JapaneseTextAnalyzer>();
        services.AddScoped<IAiTutorService, GeminiAiTutorService>();
        services.AddScoped<IVocabularyRepository, VocabularyRepository>();
        services.AddScoped<IUserProgressRepository, UserProgressRepository>();
        services.AddScoped<IDailyQuestRepository, DailyQuestRepository>();
        services.AddScoped<IChapterPracticeService, ChapterPracticeService>();
        services.AddScoped<ICharacterDialogueService, GeminiCharacterDialogueService>();
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
        File.AppendAllText(@"d:\kotoba\app_startup.log", $"[{DateTime.Now:HH:mm:ss.fff}] App.OnExit called! ExitCode={e.ApplicationExitCode}\nEnvironment.StackTrace:\n{Environment.StackTrace}\n");
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
