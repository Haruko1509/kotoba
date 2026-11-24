using System.Text.Json.Serialization;
using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Core.Services;
using KotobaOdyssey.Data;
using KotobaOdyssey.Data.Repositories;
using KotobaOdyssey.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Services & SQLite DB
string appDataDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "KotobaOdyssey");
Directory.CreateDirectory(appDataDir);
string dbPath = Path.Combine(appDataDir, "kotoba.db");

builder.Services.AddDbContext<KotobaDbContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
});

// JSON formatting
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Repositories & Services
builder.Services.AddScoped<INovelRepository, NovelRepository>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<INovelImportService, NovelImportService>();
builder.Services.AddSingleton<IJapaneseDictionaryService, JapaneseDictionaryService>();
builder.Services.AddScoped<IJapaneseTextAnalyzer, JapaneseTextAnalyzer>();
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();
builder.Services.AddScoped<IUserProgressRepository, UserProgressRepository>();
builder.Services.AddScoped<IDailyQuestRepository, DailyQuestRepository>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAiTutorService, GeminiAiTutorService>();
builder.Services.AddScoped<ICharacterDialogueService, GeminiCharacterDialogueService>();
builder.Services.AddScoped<IChapterPracticeService, ChapterPracticeService>();
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

// CORS for local web companion
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

// Auto-initialize DB schema & seed data on startup
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();
}

// ================= API ENDPOINTS =================

// Health check
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    app = "Kotoba Odyssey Minimal API",
    db = dbPath,
    timestamp = DateTime.UtcNow
}));

// Novels
app.MapGet("/api/novels", async (INovelRepository repo) =>
{
    var novels = await repo.GetAllAsync();
    var result = novels.Select(n => new
    {
        n.Id,
        n.Title,
        n.JapaneseTitle,
        n.RomajiTitle,
        n.Author,
        n.Description,
        n.CoverImagePath,
        jlptLevel = n.JlptLevel.ToString(),
        n.Genre,
        totalChapters = n.Chapters?.Count ?? 0,
        n.IsFavorite,
        n.LastReadAt,
        chapters = n.Chapters?.OrderBy(c => c.ChapterNumber).Select(c => new
        {
            c.Id,
            c.ChapterNumber,
            c.Title,
            c.WordCount,
            c.IsCompleted,
            c.LastReadPosition
        })
    });
    return Results.Ok(result);
});

app.MapGet("/api/novels/{id:int}", async (int id, INovelRepository repo) =>
{
    var novel = await repo.GetByIdAsync(id);
    if (novel == null) return Results.NotFound(new { message = $"Novel {id} not found" });

    return Results.Ok(new
    {
        novel.Id,
        novel.Title,
        novel.JapaneseTitle,
        novel.RomajiTitle,
        novel.Author,
        novel.Description,
        novel.CoverImagePath,
        jlptLevel = novel.JlptLevel.ToString(),
        novel.Genre,
        novel.IsFavorite,
        novel.LastReadAt,
        chapters = novel.Chapters?.OrderBy(c => c.ChapterNumber).Select(c => new
        {
            c.Id,
            c.ChapterNumber,
            c.Title,
            c.WordCount,
            c.IsCompleted,
            c.LastReadPosition,
            c.LastReadAt
        })
    });
});

app.MapGet("/api/novels/{id:int}/chapters/{chapterNum:int}", async (int id, int chapterNum, INovelRepository repo) =>
{
    var novel = await repo.GetByIdAsync(id);
    if (novel == null) return Results.NotFound(new { message = $"Novel {id} not found" });

    var chapter = novel.Chapters?.FirstOrDefault(c => c.ChapterNumber == chapterNum);
    if (chapter == null) return Results.NotFound(new { message = $"Chapter {chapterNum} not found in novel {id}" });

    // Update progress
    await repo.UpdateChapterProgressAsync(chapter.Id, chapter.LastReadPosition, chapter.IsCompleted);

    return Results.Ok(new
    {
        novelId = novel.Id,
        novelTitle = novel.Title,
        novelLevel = novel.JlptLevel.ToString(),
        chapter.Id,
        chapter.ChapterNumber,
        chapter.Title,
        chapter.Content,
        chapter.WordCount,
        chapter.IsCompleted,
        chapter.LastReadPosition,
        bookmarks = chapter.Bookmarks?.Select(b => new
        {
            b.Id,
            b.CharacterPosition,
            b.Snippet,
            b.Note,
            b.CreatedAt
        })
    });
});

// Import Novel
app.MapPost("/api/novels/import", async ([FromBody] ImportRequest request, INovelImportService importService) =>
{
    if (string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.BadRequest(new { message = "Content cannot be empty" });
    }

    var level = JlptLevel.N5;
    if (!string.IsNullOrWhiteSpace(request.JlptLevel) && Enum.TryParse<JlptLevel>(request.JlptLevel, true, out var l))
    {
        level = l;
    }

    string title = !string.IsNullOrWhiteSpace(request.Title) 
        ? request.Title 
        : Path.GetFileNameWithoutExtension(request.FileName ?? "novel.txt");

    var result = request.Format?.ToUpperInvariant() == "TXT"
        ? await importService.ImportFromTxtAsync(request.Content, title, request.Author, level, request.Genre)
        : await importService.ImportFromJsonAsync(request.Content);

    if (!result.IsSuccess)
    {
        return Results.BadRequest(new { message = result.ErrorMessage, errors = result.ValidationErrors });
    }

    return Results.Ok(new { message = "Novel imported successfully", novelId = result.ImportedNovel?.Id });
});

// Import Novel from URL (Kakuyomu, Syosetu, Web)
app.MapPost("/api/novels/import-url", async ([FromBody] ImportUrlRequest req, INovelImportService importService) =>
{
    if (string.IsNullOrWhiteSpace(req.Url))
    {
        return Results.BadRequest(new { message = "Đường dẫn URL không được để trống." });
    }

    var level = JlptLevel.N3;
    if (!string.IsNullOrWhiteSpace(req.JlptLevel) && Enum.TryParse<JlptLevel>(req.JlptLevel, true, out var l))
    {
        level = l;
    }

    var result = await importService.ImportFromUrlAsync(req.Url, level, req.Genre, req.CustomTitle);
    if (!result.IsSuccess)
    {
        return Results.BadRequest(new { message = result.ErrorMessage, errors = result.ValidationErrors });
    }

    return Results.Ok(new
    {
        message = "Novel imported successfully from URL",
        novelId = result.ImportedNovel?.Id,
        novelTitle = result.ImportedNovel?.Title,
        genre = result.ImportedNovel?.Genre,
        chaptersCount = result.ImportedNovel?.Chapters?.Count ?? 0
    });
});

// Vocabulary & SRS
app.MapGet("/api/vocab", async (IVocabularyRepository repo) =>
{
    var items = await repo.GetAllAsync();
    return Results.Ok(items.Select(v => new
    {
        v.Id,
        v.Word,
        v.Reading,
        v.Meaning,
        jlptLevel = v.JlptLevel.ToString(),
        v.ContextSentence,
        v.SourceNovelTitle,
        v.SourceChapterId,
        v.IntervalDays,
        v.EaseFactor,
        v.Repetitions,
        v.NextReviewDate,
        isDue = v.NextReviewDate <= DateTime.UtcNow
    }));
});

app.MapGet("/api/vocab/due", async (IVocabularyRepository repo) =>
{
    var dueItems = await repo.GetDueReviewsAsync();
    return Results.Ok(dueItems.Select(v => new
    {
        v.Id,
        v.Word,
        v.Reading,
        v.Meaning,
        jlptLevel = v.JlptLevel.ToString(),
        v.ContextSentence,
        v.SourceNovelTitle,
        v.SourceChapterId,
        v.IntervalDays,
        v.EaseFactor,
        v.Repetitions,
        v.NextReviewDate
    }));
});

app.MapPost("/api/vocab", async ([FromBody] CreateVocabRequest req, IVocabularyRepository repo, IUserProgressRepository progressRepo, IDailyQuestRepository questRepo) =>
{
    if (string.IsNullOrWhiteSpace(req.Word))
    {
        return Results.BadRequest(new { message = "Word cannot be empty" });
    }

    var existing = await repo.GetByWordAsync(req.Word);
    if (existing != null)
    {
        // Enrich context if not already present
        if (!string.IsNullOrWhiteSpace(req.ContextSentence) &&
            (string.IsNullOrWhiteSpace(existing.ContextSentence) || !existing.ContextSentence.Contains(req.ContextSentence.Trim())))
        {
            existing.ContextSentence = string.IsNullOrWhiteSpace(existing.ContextSentence)
                ? req.ContextSentence.Trim()
                : $"{existing.ContextSentence} \n{req.ContextSentence.Trim()}";

            if (req.SourceChapterId.HasValue && !existing.SourceChapterId.HasValue)
            {
                existing.SourceChapterId = req.SourceChapterId;
            }

            await repo.UpdateAsync(existing);
        }

        return Results.Ok(new
        {
            item = existing,
            isExisting = true,
            message = $"Từ '{req.Word}' đã có trong kho từ vựng. Đã cập nhật thêm ngữ cảnh mới!"
        });
    }

    var level = JlptLevel.N5;
    if (!string.IsNullOrWhiteSpace(req.JlptLevel) && Enum.TryParse<JlptLevel>(req.JlptLevel, true, out var l))
    {
        level = l;
    }

    var item = new VocabularyItem
    {
        Word = req.Word.Trim(),
        Reading = req.Reading?.Trim() ?? string.Empty,
        Meaning = req.Meaning?.Trim() ?? string.Empty,
        JlptLevel = level,
        ContextSentence = req.ContextSentence?.Trim() ?? string.Empty,
        SourceNovelTitle = req.SourceNovelTitle?.Trim() ?? string.Empty,
        SourceChapterId = req.SourceChapterId,
        CreatedAt = DateTime.UtcNow,
        NextReviewDate = DateTime.UtcNow, // Available immediately for SRS
        IntervalDays = 0,
        EaseFactor = 2.5,
        Repetitions = 0
    };

    var created = await repo.AddAsync(item);
    await progressRepo.IncrementWordsLearnedAsync();
    await questRepo.IncrementQuestProgressAsync(QuestType.AddVocabulary, 1);
    return Results.Created($"/api/vocab/{created.Id}", created);
});

app.MapPost("/api/vocab/{id:int}/review", async (int id, [FromBody] ReviewRequest req, IVocabularyRepository repo, IUserProgressRepository progressRepo) =>
{
    var item = await repo.GetByIdAsync(id);
    if (item == null) return Results.NotFound(new { message = $"VocabularyItem {id} not found" });

    // Review rating 1..4 (Again=1, Hard=2, Good=3, Easy=4)
    var rating = req.Rating switch
    {
        1 => ReviewRating.Again,
        2 => ReviewRating.Hard,
        3 => ReviewRating.Good,
        4 => ReviewRating.Easy,
        _ => ReviewRating.Good
    };

    var result = SpacedRepetitionCalculator.CalculateNextReview(
        item.IntervalDays,
        item.EaseFactor,
        item.Repetitions,
        rating,
        DateTime.UtcNow);

    var log = await repo.RecordReviewAsync(item.Id, rating, result);

    // Reward XP for SRS review
    int xpEarned = rating switch
    {
        ReviewRating.Easy => 15,
        ReviewRating.Good => 10,
        ReviewRating.Hard => 5,
        _ => 2
    };
    await progressRepo.AddXpAsync(xpEarned);

    return Results.Ok(new
    {
        message = "Review recorded successfully",
        xpEarned,
        nextReviewDate = result.NextReviewDate,
        newIntervalDays = result.NewIntervalDays,
        newEaseFactor = result.NewEaseFactor,
        newRepetitions = result.NewRepetitions
    });
});

// User Progress & Gamification
app.MapGet("/api/progress", async (IUserProgressRepository repo) =>
{
    var p = await repo.GetProgressAsync();
    return Results.Ok(new
    {
        p.UserName,
        p.Title,
        p.Level,
        p.CurrentXp,
        p.RequiredXp,
        p.StreakDays,
        p.TodayStudyMinutes,
        p.TotalStudyMinutes,
        p.TotalWordsLearned,
        p.TotalChaptersRead
    });
});

// Text Analysis (NLP Tokenizer)
app.MapPost("/api/analyze", async ([FromBody] AnalyzeRequest req, IJapaneseTextAnalyzer analyzer) =>
{
    if (string.IsNullOrWhiteSpace(req.Text))
    {
        return Results.Ok(Array.Empty<object>());
    }

    var tokens = await analyzer.AnalyzeTextAsync(req.Text);
    return Results.Ok(tokens.Select(t => new
    {
        t.Surface,
        t.Reading,
        t.BaseForm,
        t.PartOfSpeech,
        jlptLevel = t.JlptLevel == JlptLevel.None ? null : t.JlptLevel.ToString(),
        t.HasDictionaryEntry,
        t.StartIndex,
        t.Length
    }));
});

// Dictionary Lookup & Suggestions
app.MapGet("/api/dictionary/lookup", async ([FromQuery] string query, IJapaneseDictionaryService dict) =>
{
    if (string.IsNullOrWhiteSpace(query)) return Results.BadRequest(new { message = "Query cannot be empty" });
    var entry = await dict.LookupAsync(query);
    if (entry == null) return Results.NotFound(new { message = $"Term '{query}' not found" });

    return Results.Ok(new
    {
        term = entry.Term,
        word = entry.Term,
        reading = entry.Reading,
        meaning = entry.Meaning,
        partOfSpeech = entry.PartOfSpeech,
        jlptLevel = entry.JlptLevel == JlptLevel.None ? "N3" : entry.JlptLevel.ToString(),
        exampleSentence = entry.ExampleSentence,
        exampleMeaning = entry.ExampleMeaning,
        isKnown = entry.IsKnown,
        originalSurface = entry.OriginalSurface,
        inflectionNote = entry.InflectionNote
    });
});

app.MapGet("/api/dictionary/search", async ([FromQuery] string query, [FromQuery] int? limit, IJapaneseDictionaryService dict) =>
{
    if (string.IsNullOrWhiteSpace(query)) return Results.Ok(Array.Empty<object>());
    var results = await dict.SearchSuggestionsAsync(query, limit ?? 15);
    return Results.Ok(results);
});

// AI Tutor Contextual Explanation & Multi-turn Chat
app.MapPost("/api/ai/explain", async ([FromBody] AiSentenceExplanationRequest req, IAiTutorService aiTutor) =>
{
    var result = await aiTutor.ExplainSentenceAsync(req);
    return Results.Ok(result);
});

app.MapPost("/api/ai/chat", async ([FromBody] AiSentenceExplanationRequest req, IAiTutorService aiTutor) =>
{
    if (string.IsNullOrWhiteSpace(req.RequestType) || req.RequestType == "explain")
    {
        req.RequestType = "follow_up";
    }
    var result = await aiTutor.ExplainSentenceAsync(req);
    return Results.Ok(result);
});

app.MapPost("/api/ai/exercise", async ([FromBody] AiExerciseRequest req, IAiTutorService aiTutor) =>
{
    var exercise = await aiTutor.GenerateExerciseAsync(req);
    return Results.Ok(exercise);
});

// AI Character Conversation
app.MapGet("/api/characters", async (ICharacterDialogueService charService, [FromQuery] int? novelId) =>
{
    var profiles = await charService.GetProfilesAsync(novelId);
    return Results.Ok(profiles);
});

app.MapGet("/api/characters/{characterId}", async (string characterId, ICharacterDialogueService charService) =>
{
    var profile = await charService.GetProfileByIdAsync(characterId);
    return profile != null ? Results.Ok(profile) : Results.NotFound(new { message = $"Character '{characterId}' not found" });
});

app.MapPost("/api/ai/character-chat", async ([FromBody] CharacterChatRequest req, ICharacterDialogueService charService, IUserProgressRepository progressRepo) =>
{
    var result = await charService.ChatAsync(req);
    
    // Reward XP for communicating in Japanese
    if (result.Success)
    {
        await progressRepo.AddXpAsync(5);
    }
    
    return Results.Ok(result);
});

app.MapGet("/api/characters/{characterId}/history", async (string characterId, ICharacterDialogueService charService, [FromQuery] string? sessionId, [FromQuery] int? limit) =>
{
    var history = await charService.GetHistoryAsync(characterId, sessionId, limit ?? 20);
    return Results.Ok(history);
});

app.MapDelete("/api/characters/{characterId}/history", async (string characterId, ICharacterDialogueService charService, [FromQuery] string? sessionId) =>
{
    await charService.ClearHistoryAsync(characterId, sessionId);
    return Results.Ok(new { success = true, message = $"History for '{characterId}' cleared." });
});

app.MapPost("/api/ai/character-review", async ([FromBody] ConversationReviewRequest req, ICharacterDialogueService charService) =>
{
    var review = await charService.GenerateReviewAsync(req);
    return Results.Ok(review);
});

// Chapter Comprehension Quiz & Practice
app.MapGet("/api/novels/{novelId:int}/chapters/{chapterNum:int}/quiz", async (int novelId, int chapterNum, IChapterPracticeService practiceService) =>
{
    var quiz = await practiceService.GetChapterQuizAsync(novelId, chapterNum);
    return Results.Ok(quiz);
});

app.MapPost("/api/novels/{novelId:int}/chapters/{chapterNum:int}/quiz/submit", async (int novelId, int chapterNum, [FromBody] QuizSubmitRequest req, IChapterPracticeService practiceService) =>
{
    req.NovelId = novelId;
    req.ChapterNumber = chapterNum;
    var result = await practiceService.SubmitQuizAsync(req);
    return Results.Ok(result);
});

app.MapGet("/api/practice/daily-recommendations", async (IChapterPracticeService practiceService) =>
{
    var rec = await practiceService.GetDailyRecommendationAsync();
    return Results.Ok(rec);
});

app.MapGet("/api/progress/analytics", async (IChapterPracticeService practiceService) =>
{
    var analytics = await practiceService.GetLearningAnalyticsAsync();
    return Results.Ok(analytics);
});

// Translation Endpoint with Contextual Vietnamese Translation
app.MapPost("/api/translate", async ([FromBody] TranslateRequest req, IHttpClientFactory httpFactory) =>
{
    if (string.IsNullOrWhiteSpace(req.Text))
    {
        return Results.Ok(new { translatedText = "", sourceText = "" });
    }

    try
    {
        var client = httpFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(6);
        string encoded = Uri.EscapeDataString(req.Text.Trim());
        string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=ja&tl=vi&dt=t&q={encoded}";

        var requestMsg = new HttpRequestMessage(HttpMethod.Get, url);
        requestMsg.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        var response = await client.SendAsync(requestMsg);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(content);
            var root = doc.RootElement;
            if (root.ValueKind == System.Text.Json.JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var sentences = root[0];
                var sb = new System.Text.StringBuilder();
                foreach (var s in sentences.EnumerateArray())
                {
                    if (s.ValueKind == System.Text.Json.JsonValueKind.Array && s.GetArrayLength() > 0)
                    {
                        var part = s[0].GetString();
                        if (!string.IsNullOrEmpty(part)) sb.Append(part);
                    }
                }
                return Results.Ok(new { translatedText = sb.ToString(), sourceText = req.Text });
            }
        }
    }
    catch
    {
        // Fallback gracefully
    }

    return Results.Ok(new { translatedText = req.Text, sourceText = req.Text, isFallback = true });
});

app.Run("http://localhost:5000");

// Request Records
public record ImportRequest(string? Content, string? Format, string? FileName, string? Title, string? Author, string? JlptLevel, string? Genre);
public record ImportUrlRequest(string? Url, string? JlptLevel, string? Genre, string? CustomTitle);
public record CreateVocabRequest(string? Word, string? Reading, string? Meaning, string? JlptLevel, string? ContextSentence, string? SourceNovelTitle, int? SourceChapterId);
public record ReviewRequest(int Rating);
public record AnalyzeRequest(string? Text);
public record TranslateRequest(string? Text);

