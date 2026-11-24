using System.Text.Json;
using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Core.Services;
using KotobaOdyssey.Data;
using KotobaOdyssey.Data.Repositories;
using KotobaOdyssey.Data.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KotobaOdyssey.Tests;

public class FinalGraduationQaTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KotobaDbContext _context;

    public FinalGraduationQaTests()
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
    public async Task Flow1_EndToEnd_ReaderVocabSrsLearningLoop_Succeeds()
    {
        var novelRepo = new NovelRepository(_context);
        var vocabRepo = new VocabularyRepository(_context);
        var dictService = new JapaneseDictionaryService();
        var analyzer = new JapaneseTextAnalyzer(dictService);
        var importService = new NovelImportService(novelRepo, NullLogger<NovelImportService>.Instance);
        var config = new DummyConfiguration();
        var aiTutor = new GeminiAiTutorService(analyzer, dictService, config, NullLogger<GeminiAiTutorService>.Instance);

        // 1. Import a new novel from TXT
        string sampleTxt = @"第1章 銀河の夜
ジョバンニは静かに立ち上がって、銀河鉄道の切符をポケットに入れた。星空が美しく輝いていた。
第2章 ケンタウル祭の夜
ケンタウルの村祭りは明るい光で溢れていた。カンパネルラは笑顔で手を振った。";

        var importResult = await importService.ImportFromTxtAsync(
            sampleTxt, 
            "銀河鉄道の夜 (Graduation QA Test)", 
            "宮沢賢治", 
            JlptLevel.N3, 
            "Fantasy");

        Assert.True(importResult.IsSuccess);
        Assert.NotNull(importResult.ImportedNovel);
        Assert.Equal(2, importResult.ImportedNovel.Chapters.Count);

        // 2. Open Chapter 1 and read content
        var chapter1 = importResult.ImportedNovel.Chapters.First();
        Assert.Contains("銀河鉄道の切符", chapter1.Content);

        // 3. Analyze text for tokens & furigana
        var tokens = await analyzer.AnalyzeTextAsync(chapter1.Content);
        Assert.NotEmpty(tokens);
        Assert.Contains(tokens, t => t.Surface == "銀河" || t.Surface == "切符" || t.Surface == "静か");

        // 4. Look up word in dictionary with de-inflection
        var lookup = await dictService.LookupAsync("立ち上がって");
        Assert.NotNull(lookup);
        Assert.Equal("立ち上がる", lookup.Term);

        // 5. Ask AI Tutor for sentence explanation
        var explanation = await aiTutor.ExplainSentenceAsync(new AiSentenceExplanationRequest
        {
            Sentence = "ジョバンニは静かに立ち上がって、銀河鉄道の切符をポケットに入れた。",
            SelectedText = "切符",
            JlptLevel = "N3",
            NovelTitle = importResult.ImportedNovel.Title,
            ChapterTitle = chapter1.Title
        });

        Assert.NotNull(explanation);
        Assert.True(explanation.Success);
        Assert.False(string.IsNullOrWhiteSpace(explanation.TranslationVn));
        Assert.NotNull(explanation.DetailedVocabulary);
        Assert.False(explanation.IsAiGenerated); // Confirms fallback transparency
        Assert.Equal("Local NLP & Grammar Engine (Ngoại tuyến)", explanation.Provider);

        // 6. Save vocab into vocabulary repository
        var vocabItem = new VocabularyItem
        {
            Word = "切符",
            Reading = "きっぷ",
            Meaning = "Vé (xe buýt, tàu hỏa)",
            JlptLevel = JlptLevel.N5,
            ContextSentence = "銀河鉄道の切符をポケットに入れた。",
            SourceNovelTitle = importResult.ImportedNovel.Title,
            SourceChapterId = chapter1.Id,
            CreatedAt = DateTime.UtcNow,
            NextReviewDate = DateTime.UtcNow.AddMinutes(-5), // Due immediately
            IntervalDays = 0,
            EaseFactor = 2.5,
            Repetitions = 0
        };

        var savedVocab = await vocabRepo.AddAsync(vocabItem);
        Assert.True(savedVocab.Id > 0);

        // 7. Check word appears in SRS Due deck
        var dueReviews = await vocabRepo.GetDueReviewsAsync();
        Assert.Contains(dueReviews, v => v.Word == "切符");

        // 8. Review with SM-2 (Rating: Good = 3)
        var sm2Result = SpacedRepetitionCalculator.CalculateNextReview(
            savedVocab.IntervalDays,
            savedVocab.EaseFactor,
            savedVocab.Repetitions,
            ReviewRating.Good,
            DateTime.UtcNow);

        Assert.Equal(1, sm2Result.NewRepetitions);
        Assert.Equal(1, sm2Result.NewIntervalDays);
        Assert.True(sm2Result.NextReviewDate > DateTime.UtcNow);

        await vocabRepo.RecordReviewAsync(savedVocab.Id, ReviewRating.Good, sm2Result);

        // 9. Verify next review date is updated and no longer due
        var updatedItem = await vocabRepo.GetByIdAsync(savedVocab.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal(1, updatedItem.Repetitions);
        Assert.Equal(1, updatedItem.IntervalDays);
        Assert.True(updatedItem.NextReviewDate > DateTime.UtcNow);

        var dueAfterReview = await vocabRepo.GetDueReviewsAsync();
        Assert.DoesNotContain(dueAfterReview, v => v.Word == "切符");
    }

    [Fact]
    public async Task Flow2_EndToEnd_ChapterQuizAndProgressNoDuplicateXp_Succeeds()
    {
        var novelRepo = new NovelRepository(_context);
        var vocabRepo = new VocabularyRepository(_context);
        var progressRepo = new UserProgressRepository(_context);
        var questRepo = new DailyQuestRepository(_context, progressRepo);
        var practiceService = new ChapterPracticeService(
            novelRepo,
            vocabRepo,
            progressRepo,
            NullLogger<ChapterPracticeService>.Instance,
            _context);

        // 1. Seed novel with chapters
        var novel = new Novel
        {
            Title = "走れメロス (QA Test)",
            JapaneseTitle = "走れメロス",
            Author = "太宰治",
            JlptLevel = JlptLevel.N3
        };
        novel.Chapters.Add(new Chapter
        {
            ChapterNumber = 1,
            Title = "第1章 出発",
            Content = "メロスは激怒した。必ず、かの邪智暴虐の王を除かなければならぬと決意した。",
            WordCount = 42
        });
        await novelRepo.AddAsync(novel);

        // Seed initial user progress
        await progressRepo.AddXpAsync(50);
        var initialProgress = await progressRepo.GetProgressAsync();
        int initialXp = initialProgress.CurrentXp;

        // 2. Generate Chapter Quiz
        var quiz = await practiceService.GetChapterQuizAsync(novel.Id, 1);
        Assert.NotNull(quiz);
        Assert.Equal(novel.Id, quiz.NovelId);
        Assert.Equal(1, quiz.ChapterNumber);
        Assert.NotEmpty(quiz.Questions);

        // 3. Submit Quiz with correct answers
        var answers = quiz.Questions.ToDictionary(q => q.QuestionId, q => q.CorrectIndex);

        var submitReq = new QuizSubmitRequest
        {
            NovelId = novel.Id,
            ChapterNumber = 1,
            Answers = answers
        };

        var submitResult = await practiceService.SubmitQuizAsync(submitReq);
        Assert.NotNull(submitResult);
        Assert.True(submitResult.Passed);
        Assert.True(submitResult.XpEarned > 0);

        var progressAfterFirstQuiz = await progressRepo.GetProgressAsync();
        Assert.Equal(initialXp + submitResult.XpEarned, progressAfterFirstQuiz.CurrentXp);

        // 4. Retake Quiz (Must NOT earn duplicate XP)
        var submitResult2 = await practiceService.SubmitQuizAsync(submitReq);
        Assert.NotNull(submitResult2);
        Assert.True(submitResult2.Passed);
        Assert.Equal(0, submitResult2.XpEarned); // Zero duplicate XP!
        Assert.Contains("đã hoàn thành và nhận XP trước đó", submitResult2.FeedbackSummary);

        var progressAfterRetake = await progressRepo.GetProgressAsync();
        Assert.Equal(progressAfterFirstQuiz.CurrentXp, progressAfterRetake.CurrentXp);
    }

    [Fact]
    public async Task Flow3_EndToEnd_AiCharacterChatPersonaAndVocabSave_Succeeds()
    {
        var config = new DummyConfiguration();
        var charService = new GeminiCharacterDialogueService(config, NullLogger<GeminiCharacterDialogueService>.Instance, null, _context);
        var vocabRepo = new VocabularyRepository(_context);

        // 1. Get literature profiles
        var profiles = await charService.GetProfilesAsync(null);
        Assert.NotEmpty(profiles);
        var giovanni = profiles.FirstOrDefault(p => p.CharacterId == "giovanni");
        Assert.NotNull(giovanni);
        Assert.Contains("僕", giovanni.PronounFirstPerson);

        // 2. Chat with Giovanni in Practice mode (with intentional particle mistake)
        var chatReq = new CharacterChatRequest
        {
            CharacterId = "giovanni",
            UserMessage = "私は駅に本を読みます。", // Mistake: 駅に instead of 駅で
            JlptLevel = "N4",
            ConversationMode = "practice",
            CorrectionMode = "gentle",
            SessionId = "qa_session_001"
        };

        var response = await charService.ChatAsync(chatReq);
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("giovanni", response.CharacterId);
        Assert.Contains("僕", response.ReplyJapanese); // Character pronoun consistency
        Assert.False(string.IsNullOrWhiteSpace(response.ReplyVietnamese));
        Assert.False(response.IsAiGenerated); // Honest fallback reporting
        Assert.Equal("RuleBased-PersonaEngine", response.Provider);

        // Verify gentle correction is provided
        Assert.NotNull(response.Correction);
        Assert.False(string.IsNullOrWhiteSpace(response.Correction.Explanation));

        // 3. Verify chat history is persisted in SQLite
        var history = await charService.GetHistoryAsync("giovanni", "qa_session_001", 10);
        Assert.NotEmpty(history);
        Assert.Contains(history, m => m.ContentJapanese == "私は駅に本を読みます。");

        // 4. Extract and save new vocab from response into SRS
        var savedVocab = await vocabRepo.AddAsync(new VocabularyItem
        {
            Word = "切符",
            Reading = "きっぷ",
            Meaning = "Vé tàu",
            JlptLevel = JlptLevel.N5,
            ContextSentence = response.ReplyJapanese,
            SourceNovelTitle = "AI Character Chat: Giovanni"
        });

        Assert.True(savedVocab.Id > 0);
        var retrieved = await vocabRepo.GetByIdAsync(savedVocab.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("切符", retrieved.Word);
    }

    [Fact]
    public async Task Flow4_EndToEnd_MultiContextDataParity_Succeeds()
    {
        // 1. Write data using Context A (simulating API server)
        var novelRepoA = new NovelRepository(_context);
        var novel = new Novel
        {
            Title = "坊っちゃん (Parity Test)",
            JapaneseTitle = "坊っちゃん",
            Author = "夏目漱石",
            JlptLevel = JlptLevel.N4
        };
        novel.Chapters.Add(new Chapter
        {
            ChapterNumber = 1,
            Title = "第1章",
            Content = "親譲りの無鉄砲で小供の時から損ばかりしている。",
            WordCount = 26
        });
        await novelRepoA.AddAsync(novel);

        var vocabRepoA = new VocabularyRepository(_context);
        await vocabRepoA.AddAsync(new VocabularyItem
        {
            Word = "無鉄砲",
            Reading = "むてっぽう",
            Meaning = "Liều lĩnh, bồng bột",
            JlptLevel = JlptLevel.N1,
            ContextSentence = "親譲りの無鉄砲で小供の時から損ばかりしている。"
        });

        // 2. Read data using Context B connected to same underlying connection (simulating WPF Desktop app)
        var optionsB = new DbContextOptionsBuilder<KotobaDbContext>()
            .UseSqlite(_connection)
            .Options;
        using var contextB = new KotobaDbContext(optionsB);
        var novelRepoB = new NovelRepository(contextB);
        var vocabRepoB = new VocabularyRepository(contextB);

        var novels = await novelRepoB.GetAllAsync();
        var vocabList = await vocabRepoB.GetAllAsync();

        Assert.Contains(novels, n => n.Title == "坊っちゃん (Parity Test)");
        Assert.Contains(vocabList, v => v.Word == "無鉄砲" && v.Reading == "むてっぽう");
    }

    [Fact]
    public async Task Security_PathTraversalAndDuplicateHandling_ProtectsSystem()
    {
        var novelRepo = new NovelRepository(_context);
        var vocabRepo = new VocabularyRepository(_context);
        var importService = new NovelImportService(novelRepo, NullLogger<NovelImportService>.Instance);

        // 1. Attempt duplicate novel import
        var firstImport = await importService.ImportFromTxtAsync("第1章 テスト\nこれはテストです。", "Duplicate Novel", "Author", JlptLevel.N5, "General");
        Assert.True(firstImport.IsSuccess);

        var duplicateImport = await importService.ImportFromTxtAsync("第1章 テスト\nこれはテストです。", "Duplicate Novel", "Author", JlptLevel.N5, "General");
        Assert.False(duplicateImport.IsSuccess);
        Assert.Contains("đã tồn tại", duplicateImport.ErrorMessage);

        // 2. Empty / whitespace validation
        var emptyImport = await importService.ImportFromTxtAsync("", "", null, JlptLevel.N5, null);
        Assert.False(emptyImport.IsSuccess);

        // 3. Vocab duplicate context enrichment
        var vocab1 = await vocabRepo.AddAsync(new VocabularyItem
        {
            Word = "星",
            Reading = "ほし",
            Meaning = "Ngôi sao",
            ContextSentence = "星が光る。"
        });
        Assert.NotNull(vocab1);

        var existing = await vocabRepo.GetByWordAsync("星");
        Assert.NotNull(existing);
        existing.ContextSentence += " \n空に星がたくさんある。";
        await vocabRepo.UpdateAsync(existing);

        var updated = await vocabRepo.GetByIdAsync(vocab1.Id);
        Assert.NotNull(updated);
        Assert.Contains("星が光る。", updated.ContextSentence);
        Assert.Contains("空に星がたくさんある。", updated.ContextSentence);
    }

    private class DummyConfiguration : IConfiguration
    {
        public string? this[string key] { get => null; set { } }
        public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();
        public Microsoft.Extensions.Primitives.IChangeToken GetReloadToken() => null!;
        public IConfigurationSection GetSection(string key) => null!;
    }
}
