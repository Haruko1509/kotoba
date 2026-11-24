using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace KotobaOdyssey.Tests;

public class CharacterDialogueServiceTests
{
    private readonly GeminiCharacterDialogueService _service;

    public CharacterDialogueServiceTests()
    {
        var config = new DummyConfiguration();
        var logger = NullLogger<GeminiCharacterDialogueService>.Instance;
        _service = new GeminiCharacterDialogueService(config, logger);
    }

    [Fact]
    public async Task GetProfilesAsync_ShouldReturnDefaultCast()
    {
        var profiles = await _service.GetProfilesAsync();

        Assert.NotNull(profiles);
        Assert.True(profiles.Count >= 3);
        Assert.Contains(profiles, p => p.CharacterId == "giovanni");
        Assert.Contains(profiles, p => p.CharacterId == "campanella");
        Assert.Contains(profiles, p => p.CharacterId == "conductor");
    }

    [Fact]
    public async Task ChatAsync_EmptyMessage_ShouldReturnError()
    {
        var response = await _service.ChatAsync(new CharacterChatRequest
        {
            CharacterId = "giovanni",
            UserMessage = ""
        });

        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
    }

    [Theory]
    [InlineData("giovanni", "銀河ステーションはどこですか？", "銀河")]
    [InlineData("campanella", "本当の幸いって何だろう？", "幸い")]
    [InlineData("conductor", "切符を拝見していただけますか？", "切符")]
    public async Task ChatAsync_FallbackPersona_ShouldProvideRelevantReplies(string charId, string userMsg, string expectedKeyword)
    {
        var response = await _service.ChatAsync(new CharacterChatRequest
        {
            CharacterId = charId,
            UserMessage = userMsg,
            JlptLevel = "N3"
        });

        Assert.True(response.Success);
        Assert.Equal(charId, response.CharacterId);
        Assert.False(string.IsNullOrWhiteSpace(response.ReplyJapanese));
        Assert.False(string.IsNullOrWhiteSpace(response.ReplyVietnamese));
        Assert.False(string.IsNullOrWhiteSpace(response.GrammarFeedback));
        Assert.NotEmpty(response.KeyVocabulary);
        Assert.Contains(response.KeyVocabulary, v => v.Word.Contains(expectedKeyword) || response.ReplyJapanese.Contains(expectedKeyword));
        Assert.False(response.IsAiGenerated); // Local fallback without API Key
    }

    [Fact]
    public async Task GetProfilesAsync_WithNovelId_ShouldFilterCastByNovel()
    {
        // Novel 1: Ginga Tetsudou no Yoru
        var gingaCast = await _service.GetProfilesAsync(1);
        Assert.NotNull(gingaCast);
        Assert.Equal(3, gingaCast.Count);
        Assert.All(gingaCast, p => Assert.Equal(1, p.NovelId));
        Assert.Contains(gingaCast, p => p.CharacterId == "giovanni");
        Assert.Contains(gingaCast, p => p.CharacterId == "campanella");
        Assert.Contains(gingaCast, p => p.CharacterId == "conductor");

        // Novel 3: Gongitsune
        var gonCast = await _service.GetProfilesAsync(3);
        Assert.NotNull(gonCast);
        Assert.Single(gonCast);
        Assert.Equal("gon", gonCast[0].CharacterId);
        Assert.Equal(3, gonCast[0].NovelId);
    }

    [Fact]
    public async Task ChatAsync_WithChapterContextAndHistory_ShouldPreserveParameters()
    {
        var request = new CharacterChatRequest
        {
            CharacterId = "giovanni",
            NovelId = 1,
            ChapterNumber = 2,
            UserMessage = "活版所での仕事はどうだった？",
            JlptLevel = "N4",
            History = new List<ChatMessageHistoryDto>
            {
                new() { Role = "user", Text = "こんにちは、ジョバンニ。" },
                new() { Role = "character", Text = "やあ、こんにちは！" }
            }
        };

        var response = await _service.ChatAsync(request);

        Assert.True(response.Success);
        Assert.Equal("giovanni", response.CharacterId);
        Assert.False(response.IsAiGenerated);
        Assert.Equal("RuleBased-PersonaEngine", response.Provider);
        Assert.False(string.IsNullOrWhiteSpace(response.ReplyJapanese));
        Assert.NotEmpty(response.SuggestedFollowups);
    }

    private class DummyConfiguration : IConfiguration
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
