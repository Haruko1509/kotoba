using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace KotobaOdyssey.Tests;

public class AiTutorFollowUpTests
{
    private readonly GeminiAiTutorService _service;
    private readonly JapaneseTextAnalyzer _textAnalyzer;
    private readonly JapaneseDictionaryService _dictService;

    public AiTutorFollowUpTests()
    {
        _dictService = new JapaneseDictionaryService();
        _textAnalyzer = new JapaneseTextAnalyzer(_dictService);
        var config = new DummyConfiguration();
        var logger = NullLogger<GeminiAiTutorService>.Instance;

        _service = new GeminiAiTutorService(_textAnalyzer, _dictService, config, logger);
    }

    [Fact]
    public async Task ExplainSentenceAsync_EmptySentence_ReturnsFailure()
    {
        var result = await _service.ExplainSentenceAsync(new AiSentenceExplanationRequest
        {
            Sentence = ""
        });

        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.False(result.IsAiGenerated);
    }

    [Fact]
    public async Task ExplainSentenceAsync_WithoutApiKey_ReturnsTransparentLocalEngineResult()
    {
        var request = new AiSentenceExplanationRequest
        {
            Sentence = "先生は、黒板に吊した大きな黒い星座の図を指しました。",
            NovelTitle = "銀河鉄道の夜",
            ChapterTitle = "一、午後の授業",
            ChapterNumber = 1,
            JlptLevel = "N3",
            RequestType = "explain"
        };

        var result = await _service.ExplainSentenceAsync(request);

        Assert.True(result.Success);
        Assert.False(result.IsAiGenerated); // Transparent: not disguised as real Gemini
        Assert.Contains("Local", result.Provider);
        Assert.NotEmpty(result.Tokens);
        Assert.False(string.IsNullOrWhiteSpace(result.GrammarInsight));
        Assert.NotNull(result.Uncertainties);
        Assert.NotEmpty(result.KeyVocabulary);
    }

    [Fact]
    public async Task ExplainSentenceAsync_FollowUpRequest_HandlesFollowUpContext()
    {
        var request = new AiSentenceExplanationRequest
        {
            Sentence = "銀河の帯のようなところを指しながら。",
            NovelTitle = "銀河鉄道の夜",
            ChapterTitle = "一、午後の授業",
            JlptLevel = "N3",
            RequestType = "follow_up",
            FollowUpQuestion = "Tại sao ở đây lại dùng cấu trúc ながら?",
            PreviousExplanation = "Câu văn miêu tả hành động của người thầy trong lớp học."
        };

        var result = await _service.ExplainSentenceAsync(request);

        Assert.True(result.Success);
        Assert.Contains("ながら", result.GrammarInsight);
        Assert.Contains(request.FollowUpQuestion, result.GrammarInsight);
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
