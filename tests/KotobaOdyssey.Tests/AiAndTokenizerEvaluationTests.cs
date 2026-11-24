using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Xunit;
using Xunit.Abstractions;

namespace KotobaOdyssey.Tests;

public class AiAndTokenizerEvaluationTests
{
    private readonly ITestOutputHelper _output;
    private readonly JapaneseDictionaryService _dictService;
    private readonly JapaneseTextAnalyzer _analyzer;
    private readonly GeminiAiTutorService _aiTutorService;

    public AiAndTokenizerEvaluationTests(ITestOutputHelper output)
    {
        _output = output;
        _dictService = new JapaneseDictionaryService();
        _analyzer = new JapaneseTextAnalyzer(_dictService);
        var config = new DummyConfiguration();
        var logger = NullLogger<GeminiAiTutorService>.Instance;
        _aiTutorService = new GeminiAiTutorService(_analyzer, _dictService, config, logger);
    }

    public static readonly string[] EvaluationSentences = new[]
    {
        "午後の授業がはじまりました。",
        "先生は、黒板に吊した大きな黒い星座の図を指しました。",
        "ちかくの村へ出てきて、いたずらばかりしました。",
        "上から下へ白くけむった銀河の帯のようなところを指しながら。",
        "親譲りの無鉄砲で小供の時から損ばかりしている。",
        "国境の長いトンネルを抜けると雪国であった。",
        "一人の下人が、羅生門の下で雨やみを待っていた。",
        "極楽の蓮池のふちを独りで歩いていらっしゃいました。"
    };

    [Fact]
    public async Task Evaluate_Tokenizer_OnRealLiteratureSentences()
    {
        int totalTokens = 0;
        int dictionaryMatchedTokens = 0;

        foreach (var sentence in EvaluationSentences)
        {
            var tokens = await _analyzer.AnalyzeTextAsync(sentence);
            _output.WriteLine($"\n--- Sentence: {sentence} ---");
            foreach (var tok in tokens)
            {
                totalTokens++;
                if (tok.HasDictionaryEntry) dictionaryMatchedTokens++;
                _output.WriteLine($"  [{tok.Surface}] Reading:{tok.Reading} Base:{tok.BaseForm} POS:{tok.PartOfSpeech} JLPT:{tok.JlptLevel} Dict:{tok.HasDictionaryEntry}");
            }
            Assert.NotEmpty(tokens);
        }

        double matchRate = (double)dictionaryMatchedTokens / totalTokens * 100;
        _output.WriteLine($"\nTotal Tokens: {totalTokens}, Dict Matched: {dictionaryMatchedTokens}, Match Rate: {matchRate:F1}%");
        Assert.True(totalTokens > 0);
    }

    [Fact]
    public async Task Evaluate_AiTutor_RuleBasedQualityAndStructure()
    {
        foreach (var sentence in EvaluationSentences)
        {
            var result = await _aiTutorService.ExplainSentenceAsync(new AiSentenceExplanationRequest
            {
                Sentence = sentence,
                NovelTitle = "Văn học Nhật Bản",
                ChapterTitle = "Trích đoạn",
                JlptLevel = "N3",
                RequestType = "explain"
            });

            Assert.True(result.Success);
            Assert.False(string.IsNullOrWhiteSpace(result.Sentence));
            Assert.False(string.IsNullOrWhiteSpace(result.TranslationVn));
            Assert.False(string.IsNullOrWhiteSpace(result.GrammarInsight));
            Assert.False(string.IsNullOrWhiteSpace(result.CulturalOrLiteraryNuance));
            Assert.NotNull(result.Uncertainties);
            Assert.False(result.IsAiGenerated); // Local evaluation without key
        }
    }

    [Fact]
    public async Task Evaluate_AiTutor_FollowUpConsistency()
    {
        var req = new AiSentenceExplanationRequest
        {
            Sentence = "国境の長いトンネルを抜けると雪国であった。",
            NovelTitle = "雪国",
            ChapterTitle = "Chương 1",
            JlptLevel = "N2",
            RequestType = "follow_up",
            FollowUpQuestion = "Tại sao lại dùng trợ từ と sau 抜ける?",
            PreviousExplanation = "Câu văn mở đầu trứ danh của Kawabata Yasunari."
        };

        var result = await _aiTutorService.ExplainSentenceAsync(req);

        Assert.True(result.Success);
        Assert.Contains(req.FollowUpQuestion, result.GrammarInsight);
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
