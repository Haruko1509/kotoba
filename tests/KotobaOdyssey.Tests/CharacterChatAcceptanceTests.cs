using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Data;
using KotobaOdyssey.Data.Repositories;
using KotobaOdyssey.Data.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace KotobaOdyssey.Tests;

public class CharacterChatAcceptanceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly KotobaDbContext _context;
    private readonly VocabularyRepository _vocabRepo;
    private readonly GeminiCharacterDialogueService _service;

    public CharacterChatAcceptanceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<KotobaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new KotobaDbContext(options);
        _context.Database.EnsureCreated();

        _vocabRepo = new VocabularyRepository(_context);

        var config = new DummyConfig();
        var logger = NullLogger<GeminiCharacterDialogueService>.Instance;
        _service = new GeminiCharacterDialogueService(config, logger, httpClient: null, dbContext: _context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    /// <summary>
    /// TEST 1: Hồ sơ nhân vật (Character Profiles) phong phú với đại từ xưng hô, tính cách, quan hệ và mốc spoiler.
    /// </summary>
    [Fact]
    public async Task GetProfilesAsync_ShouldReturnRichPersonaMetadata()
    {
        var profiles = await _service.GetProfilesAsync();

        Assert.NotNull(profiles);
        Assert.True(profiles.Count >= 10, "Hệ thống phải có ít nhất 10 nhân vật văn học kinh điển.");

        var giovanni = profiles.FirstOrDefault(p => p.CharacterId == "giovanni");
        Assert.NotNull(giovanni);
        Assert.Equal("僕 (boku)", giovanni.PronounFirstPerson);
        Assert.Equal("君 (kimi)", giovanni.PronounSecondPerson);
        Assert.False(string.IsNullOrWhiteSpace(giovanni.Bio));
        Assert.False(string.IsNullOrWhiteSpace(giovanni.Relationships));
        Assert.Contains("Campanella", giovanni.Relationships);
        Assert.Equal(3, giovanni.MaxSafeChapter);

        var botchan = profiles.FirstOrDefault(p => p.CharacterId == "botchan");
        Assert.NotNull(botchan);
        Assert.Equal("おれ (ore)", botchan.PronounFirstPerson);
        Assert.Contains("Edo", botchan.SpeakingStyle);
        Assert.Contains("Kiyo", botchan.Relationships);

        var conductor = profiles.FirstOrDefault(p => p.CharacterId == "conductor");
        Assert.NotNull(conductor);
        Assert.Equal("私 (watakushi)", conductor.PronounFirstPerson);
        Assert.Contains("kính ngữ", conductor.SpeakingStyle, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// TEST 2: Cơ chế bảo vệ cốt truyện (Spoiler Protection) - Không tiết lộ biến cố sau chương hiện tại.
    /// </summary>
    [Fact]
    public async Task SpoilerProtection_BlocksFuturePlotEvents_WhenReadingEarlyChapters()
    {
        // Người học đang đọc Chương 2 của Ginga Tetsudou no Yoru
        // Hỏi về cái chết hoặc sự chia ly của Campanella (biến cố ở Chương 9)
        var request = new CharacterChatRequest
        {
            CharacterId = "giovanni",
            NovelId = 1,
            ChapterNumber = 2,
            UserMessage = "カムパネルラが川で溺れて死ぬって本当ですか？",
            JlptLevel = "N3"
        };

        var response = await _service.ChatAsync(request);

        Assert.True(response.Success);
        // Giovanni ở đầu chuyến tàu phải ngạc nhiên, phủ nhận và khẳng định Campanella đang ngồi cạnh mình
        Assert.Contains("カムパネルラは今、僕の隣で", response.ReplyJapanese);
        Assert.Contains("不吉", response.ReplyJapanese);
        Assert.Equal("surprised", response.Expression);
    }

    /// <summary>
    /// TEST 3: Ba chế độ hội thoại (Free Conversation, Japanese Practice, Scenario Roleplay).
    /// </summary>
    [Theory]
    [InlineData("free")]
    [InlineData("practice")]
    [InlineData("scenario")]
    public async Task ConversationModes_ShouldOperateCorrectly(string mode)
    {
        var request = new CharacterChatRequest
        {
            CharacterId = "giovanni",
            NovelId = 1,
            ChapterNumber = 1,
            ConversationMode = mode,
            ScenarioDescription = "Gặp gỡ tại trạm chờ ga Ngân Hà",
            UserMessage = "こんにちは！",
            JlptLevel = "N3"
        };

        var response = await _service.ChatAsync(request);

        Assert.True(response.Success);
        Assert.Equal(mode, response.ConversationMode);
        Assert.False(string.IsNullOrWhiteSpace(response.ReplyJapanese));
        Assert.False(string.IsNullOrWhiteSpace(response.ReplyVietnamese));

        if (mode == "practice")
        {
            Assert.Contains("練習", response.ReplyJapanese);
        }
        else if (mode == "scenario")
        {
            Assert.Contains("待合室", response.ReplyJapanese);
        }
    }

    /// <summary>
    /// TEST 4: Sửa lỗi nhẹ nhàng (Gentle Correction) khi người học dùng sai trợ từ tiếng Nhật.
    /// </summary>
    [Fact]
    public async Task GentleCorrection_DetectsParticleErrors_AndOffersConstructiveAdvice()
    {
        var request = new CharacterChatRequest
        {
            CharacterId = "giovanni",
            UserMessage = "私は駅に本を読みます。", // Sai trợ từ: phải là '駅で' thay vì '駅に'
            JlptLevel = "N4",
            CorrectionMode = "gentle"
        };

        var response = await _service.ChatAsync(request);

        Assert.True(response.Success);
        Assert.NotNull(response.Correction);
        Assert.True(response.Correction.HasCorrection);
        Assert.Contains("駅で", response.Correction.CorrectedSentence);
        Assert.False(string.IsNullOrWhiteSpace(response.Correction.Explanation));
    }

    /// <summary>
    /// TEST 5: Nổi bật từ vựng (Vocabulary Highlight) & Lưu 1-click vào Flashcard SRS với câu ngữ cảnh.
    /// </summary>
    [Fact]
    public async Task VocabularyHighlight_EnablesOneClickSrsSaving_WithContextSentence()
    {
        var request = new CharacterChatRequest
        {
            CharacterId = "giovanni",
            UserMessage = "銀河ステーションについて教えて！",
            JlptLevel = "N3"
        };

        var response = await _service.ChatAsync(request);

        Assert.True(response.Success);
        Assert.NotEmpty(response.KeyVocabulary);

        var firstWord = response.KeyVocabulary[0];
        Assert.False(string.IsNullOrWhiteSpace(firstWord.Word));
        Assert.False(string.IsNullOrWhiteSpace(firstWord.Reading));
        Assert.False(string.IsNullOrWhiteSpace(firstWord.Meaning));

        // Lưu từ vựng vào SQLite SRS
        var savedItem = await _vocabRepo.AddAsync(new VocabularyItem
        {
            Word = firstWord.Word,
            Reading = firstWord.Reading,
            Meaning = firstWord.Meaning,
            JlptLevel = JlptLevel.N3,
            ContextSentence = response.ReplyJapanese,
            SourceNovelTitle = "銀河鉄道の夜",
            EaseFactor = 2.5
        });

        Assert.True(savedItem.Id > 0);
        var retrieved = await _vocabRepo.GetByWordAsync(firstWord.Word);
        Assert.NotNull(retrieved);
        Assert.Equal(response.ReplyJapanese, retrieved.ContextSentence);
    }

    /// <summary>
    /// TEST 6: Lưu trữ và khôi phục lịch sử hội thoại trong SQLite (Conversation Memory).
    /// </summary>
    [Fact]
    public async Task ConversationHistory_PersistsAndRetrievesInSqlite()
    {
        string sessionId = "test_session_giovanni_101";

        // Gửi lượt chat 1
        await _service.ChatAsync(new CharacterChatRequest
        {
            CharacterId = "giovanni",
            SessionId = sessionId,
            UserMessage = "こんばんは、ジョバンニ。"
        });

        // Gửi lượt chat 2
        await _service.ChatAsync(new CharacterChatRequest
        {
            CharacterId = "giovanni",
            SessionId = sessionId,
            UserMessage = "どこまで行くの？"
        });

        // Khôi phục lịch sử từ SQLite
        var history = await _service.GetHistoryAsync("giovanni", sessionId);

        Assert.NotNull(history);
        Assert.Equal(4, history.Count); // 2 user messages + 2 character responses

        Assert.Equal("user", history[0].Role);
        Assert.Equal("こんばんは、ジョバンニ。", history[0].ContentJapanese);
        Assert.Equal("character", history[1].Role);

        Assert.Equal("user", history[2].Role);
        Assert.Equal("どこまで行くの？", history[2].ContentJapanese);
        Assert.Equal("character", history[3].Role);

        // Xóa lịch sử khi bắt đầu phiên mới
        await _service.ClearHistoryAsync("giovanni", sessionId);
        var clearedHistory = await _service.GetHistoryAsync("giovanni", sessionId);
        Assert.Empty(clearedHistory);
    }

    /// <summary>
    /// TEST 7: Tổng kết sau buổi trò chuyện (Conversation Review).
    /// </summary>
    [Fact]
    public async Task GenerateReviewAsync_ProducesPedagogicalSummary()
    {
        var messages = new List<ChatMessageHistoryDto>
        {
            new() { Role = "user", Text = "こんにちは、ジョバンニ。" },
            new() { Role = "character", Text = "やあ、こんにちは！ 銀河ステーションへようこそ。" },
            new() { Role = "user", Text = "本当の幸いって何だろう？" },
            new() { Role = "character", Text = "本当の幸いって、誰かのために自分の命を使えることかもしれないよ。" }
        };

        var review = await _service.GenerateReviewAsync(new ConversationReviewRequest
        {
            CharacterId = "giovanni",
            JlptLevel = "N3",
            Messages = messages
        });

        Assert.True(review.Success);
        Assert.Equal("giovanni", review.CharacterId);
        Assert.Equal(4, review.TotalTurns);
        Assert.NotEmpty(review.VocabularyLearned);
        Assert.NotEmpty(review.GrammarPointsUsed);
        Assert.NotEmpty(review.KeyImprovements);
        Assert.False(string.IsNullOrWhiteSpace(review.SummaryVietnamese));
        Assert.False(string.IsNullOrWhiteSpace(review.EncouragementMessage));
    }

    private class DummyConfig : IConfiguration
    {
        public string? this[string key] { get => null; set { } }
        public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();
        public IChangeToken GetReloadToken() => new DummyChangeToken();
        public IConfigurationSection GetSection(string key) => null!;
    }

    private class DummyChangeToken : IChangeToken
    {
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => null!;
    }
}

