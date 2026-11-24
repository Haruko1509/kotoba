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

public class ChapterQuizAndProgressAcceptanceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KotobaDbContext _context;
    private readonly ServiceProvider _serviceProvider;
    private readonly NovelRepository _novelRepo;
    private readonly VocabularyRepository _vocabRepo;
    private readonly UserProgressRepository _progressRepo;
    private readonly DailyQuestRepository _questRepo;
    private readonly ChapterPracticeService _practiceService;

    public ChapterQuizAndProgressAcceptanceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<KotobaDbContext>(options => options.UseSqlite(_connection));

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

        // Seed demo novels, chapters, vocabulary, progress
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

    /// <summary>
    /// LUỒNG 1: Làm Quiz của một chương và xem giải thích chi tiết từng câu.
    /// </summary>
    [Fact]
    public async Task Flow1_TakeChapterQuiz_AndReviewExplanationsForEachQuestion()
    {
        // 1. Lấy bài Quiz của tác phẩm "銀河鉄道の夜" - Chương 1
        var quiz = await _practiceService.GetChapterQuizAsync(novelId: 1, chapterNumber: 1);
        Assert.NotNull(quiz);
        Assert.True(quiz.Questions.Count >= 5, "Bài Quiz phải có tối thiểu 5 câu hỏi.");

        // 2. Người học làm bài: trả lời một số câu đúng và cố tình chọn sai một số câu
        var userAnswers = new Dictionary<int, int>();
        for (int i = 0; i < quiz.Questions.Count; i++)
        {
            var q = quiz.Questions[i];
            // Trả lời đúng các câu chẵn, trả lời sai câu lẻ
            userAnswers[q.QuestionId] = (i % 2 == 0) ? q.CorrectIndex : (q.CorrectIndex + 1) % q.Options.Count;
        }

        var submitResult = await _practiceService.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = userAnswers
        });

        // 3. Kiểm tra kết quả phản hồi sư phạm (Question Review)
        Assert.True(submitResult.Success);
        Assert.Equal(quiz.Questions.Count, submitResult.QuestionReviews.Count);

        foreach (var review in submitResult.QuestionReviews)
        {
            // Từng câu hỏi phải có đầy đủ:
            Assert.False(string.IsNullOrWhiteSpace(review.QuestionJp), "Phải có nội dung câu hỏi tiếng Nhật.");
            Assert.False(string.IsNullOrWhiteSpace(review.Explanation), "Phải có lời giải thích sư phạm chi tiết.");
            Assert.False(string.IsNullOrWhiteSpace(review.TargetKnowledge), "Phải có từ khóa kiến thức mục tiêu.");
            Assert.True(review.UserChoiceIndex >= 0, "Phải ghi nhận lựa chọn của người học.");
            Assert.True(review.CorrectIndex >= 0, "Phải có đáp án chuẩn xác.");

            if (review.IsCorrect)
            {
                Assert.Equal(review.CorrectIndex, review.UserChoiceIndex);
            }
            else
            {
                Assert.NotEqual(review.CorrectIndex, review.UserChoiceIndex);
            }
        }

        // Báo cáo tỷ lệ đúng theo từng nhóm
        Assert.True(submitResult.ReadingTotal > 0 || submitResult.VocabTotal > 0 || submitResult.GrammarTotal > 0);
    }

    /// <summary>
    /// LUỒNG 2: Làm lại Quiz nhưng không nhận XP trùng lặp (Anti-duplicate XP exploit).
    /// </summary>
    [Fact]
    public async Task Flow2_RetakeQuiz_DoesNotAwardDuplicateXp()
    {
        var quiz = await _practiceService.GetChapterQuizAsync(novelId: 1, chapterNumber: 1);
        var allCorrectAnswers = quiz.Questions.ToDictionary(q => q.QuestionId, q => q.CorrectIndex);

        var progressBefore = await _progressRepo.GetProgressAsync();
        int initialXp = progressBefore.CurrentXp;

        // --- LẦN 1: Nộp bài đạt 100% ---
        var result1 = await _practiceService.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = allCorrectAnswers
        });

        Assert.True(result1.Passed);
        Assert.True(result1.XpEarned > 0, "Lần đầu hoàn thành bài Quiz phải được nhận XP thưởng.");

        var progressAfterFirst = await _progressRepo.GetProgressAsync();
        Assert.Equal(initialXp + result1.XpEarned, progressAfterFirst.CurrentXp);

        // --- LẦN 2: Người học làm lại bài (để ôn tập) đạt 100% ---
        var result2 = await _practiceService.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = allCorrectAnswers
        });

        Assert.True(result2.Passed);
        Assert.Equal(0, result2.XpEarned); // Không cấp XP trùng lặp
        Assert.Contains("trước đó", result2.FeedbackSummary);

        // XP của tài khoản không tăng thêm
        var progressAfterSecond = await _progressRepo.GetProgressAsync();
        Assert.Equal(progressAfterFirst.CurrentXp, progressAfterSecond.CurrentXp);

        // Cả 2 lượt làm bài đều được lưu vào SQLite để theo dõi lịch sử
        var attempts = await _context.ChapterQuizAttempts
            .Where(a => a.NovelId == 1 && a.ChapterNumber == 1)
            .ToListAsync();
        Assert.Equal(2, attempts.Count);
    }

    /// <summary>
    /// LUỒNG 3: Hoàn thành hoạt động học và thấy Dashboard cập nhật đúng từ SQLite.
    /// </summary>
    [Fact]
    public async Task Flow3_CompleteLearningActivities_DashboardUpdatesAccurately()
    {
        var dashboardVm = _serviceProvider.GetRequiredService<DashboardViewModel>();

        // 1. Nạp Dashboard ban đầu
        await dashboardVm.LoadDashboardAsync();
        int initialQuizzesAttempted = dashboardVm.TotalQuizzesAttempted;
        int initialCompletedChapters = dashboardVm.CompletedChaptersCount;

        // 2. Thực hiện hoạt động học tập thực tế:
        // A. Đọc và hoàn thành Quiz chương 1
        var quiz = await _practiceService.GetChapterQuizAsync(novelId: 1, chapterNumber: 1);
        var answers = quiz.Questions.ToDictionary(q => q.QuestionId, q => q.CorrectIndex);
        await _practiceService.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = answers
        });

        // B. Lưu và hoàn thành ôn tập 1 từ vựng đạt chuẩn Mastered (Repetitions >= 3, Interval >= 21)
        await _vocabRepo.AddAsync(new VocabularyItem
        {
            Word = "光速",
            Reading = "こうそく",
            Meaning = "tốc độ ánh sáng",
            Repetitions = 4,
            IntervalDays = 28,
            EaseFactor = 2.6,
            NextReviewDate = DateTime.UtcNow.AddDays(15)
        });

        // 3. Tải lại Dashboard
        await dashboardVm.LoadDashboardAsync();

        // 4. Kiểm tra số liệu Dashboard đã cập nhật chuẩn xác từ SQLite:
        Assert.Equal(initialQuizzesAttempted + 1, dashboardVm.TotalQuizzesAttempted);
        Assert.True(dashboardVm.QuizzesPassed >= 1);
        Assert.True(dashboardVm.CompletedChaptersCount >= initialCompletedChapters);
        Assert.True(dashboardVm.MasteredWordsCount >= 1);
        Assert.True(dashboardVm.AverageQuizScore > 0);
        Assert.True(dashboardVm.OverallAccuracyPercent > 0);
        Assert.False(string.IsNullOrWhiteSpace(dashboardVm.PrimaryStrength));
        Assert.False(string.IsNullOrWhiteSpace(dashboardVm.PrimaryWeakness));
        Assert.False(string.IsNullOrWhiteSpace(dashboardVm.PrimaryRecommendation));
    }

    /// <summary>
    /// LUỒNG 4: Xem được điểm yếu thực tế về từ vựng, ngữ pháp hoặc đọc hiểu dựa trên lịch sử làm bài.
    /// </summary>
    [Fact]
    public async Task Flow4_DetectRealWeaknesses_BasedOnQuizAttemptHistory()
    {
        // 1. Giả lập tình huống: người học trả lời tốt phần Đọc hiểu (100%) và Từ vựng (100%),
        // nhưng liên tục trả lời sai toàn bộ các câu hỏi Ngữ pháp (Grammar = 0%).
        var quiz = await _practiceService.GetChapterQuizAsync(novelId: 1, chapterNumber: 1);
        var answers = new Dictionary<int, int>();

        foreach (var q in quiz.Questions)
        {
            if (q.Category == "Grammar")
            {
                // Chọn sai câu ngữ pháp
                answers[q.QuestionId] = (q.CorrectIndex + 1) % q.Options.Count;
            }
            else
            {
                // Chọn đúng câu đọc hiểu và từ vựng
                answers[q.QuestionId] = q.CorrectIndex;
            }
        }

        await _practiceService.SubmitQuizAsync(new QuizSubmitRequest
        {
            NovelId = 1,
            ChapterNumber = 1,
            Answers = answers
        });

        // 2. Truy vấn phân tích năng lực từ database
        var analytics = await _practiceService.GetLearningAnalyticsAsync();

        // 3. Hệ thống phải tự động nhận diện chính xác "Ngữ pháp" là điểm yếu
        Assert.NotEmpty(analytics.Weaknesses);
        bool hasGrammarWeakness = analytics.Weaknesses.Any(w => 
            w.Contains("Ngữ pháp", StringComparison.OrdinalIgnoreCase) || 
            w.Contains("ngữ pháp", StringComparison.OrdinalIgnoreCase));

        Assert.True(hasGrammarWeakness, "Hệ thống phải phát hiện điểm yếu Ngữ pháp dựa trên lịch sử làm bài thực tế.");

        // Kiểm tra đề xuất sư phạm tương ứng
        Assert.Contains(analytics.ActionableRecommendations, r => 
            r.Contains("ngữ pháp", StringComparison.OrdinalIgnoreCase) || 
            r.Contains("AI Tutor", StringComparison.OrdinalIgnoreCase));

        // Kiểm tra Dashboard ViewModel cũng phản ánh điểm yếu này
        var dashboardVm = _serviceProvider.GetRequiredService<DashboardViewModel>();
        await dashboardVm.LoadDashboardAsync();

        Assert.Contains("Ngữ pháp", dashboardVm.PrimaryWeakness, StringComparison.OrdinalIgnoreCase);
    }
}

