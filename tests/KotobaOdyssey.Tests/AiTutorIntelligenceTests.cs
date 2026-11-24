using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace KotobaOdyssey.Tests;

public class AiTutorIntelligenceTests
{
    private readonly GeminiAiTutorService _aiTutorService;
    private readonly JapaneseTextAnalyzer _textAnalyzer;
    private readonly JapaneseDictionaryService _dictService;

    public AiTutorIntelligenceTests()
    {
        _dictService = new JapaneseDictionaryService();
        _textAnalyzer = new JapaneseTextAnalyzer(_dictService);
        var config = new DummyConfiguration();
        var logger = NullLogger<GeminiAiTutorService>.Instance;

        _aiTutorService = new GeminiAiTutorService(_textAnalyzer, _dictService, config, logger);
    }

    [Fact]
    public async Task ExplainSentenceAsync_StructuredAnalysis_ReturnsAllKeyComponents()
    {
        // Câu văn mẫu trong yêu cầu sản phẩm
        string sentence = "彼は何も言わずに、静かに部屋を出ていった。";

        var request = new AiSentenceExplanationRequest
        {
            Sentence = sentence,
            NovelTitle = "Văn học Nhật Bản hiện đại",
            ChapterTitle = "Chương 1",
            JlptLevel = "N3",
            RequestType = "explain",
            IncludeExercise = true
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.TranslationVn));

        // 1. Cấu trúc câu (Sentence Structure)
        Assert.NotNull(result.SentenceStructure);
        Assert.False(string.IsNullOrWhiteSpace(result.SentenceStructure.Subject));
        Assert.False(string.IsNullOrWhiteSpace(result.SentenceStructure.Predicate));
        Assert.NotEmpty(result.SentenceStructure.ParticleNotes);

        // 2. Ngữ pháp chi tiết (Detailed Grammar)
        Assert.NotEmpty(result.DetailedGrammar);
        Assert.Contains(result.DetailedGrammar, g => g.Pattern.Contains("ずに"));
        Assert.Contains(result.DetailedGrammar, g => g.Pattern.Contains("ていく") || g.Pattern.Contains("ていった"));
        Assert.Contains(result.DetailedGrammar, g => g.Pattern.Contains("何も"));

        // 3. Từ vựng trong ngữ cảnh (Detailed Vocabulary)
        Assert.NotEmpty(result.DetailedVocabulary);

        // 4. Ghi chú học tập (Learning Notes)
        Assert.NotNull(result.LearningNotes);
        Assert.NotEmpty(result.LearningNotes.KeyTakeaways);
        Assert.NotEmpty(result.LearningNotes.CommonMistakes);
        Assert.False(string.IsNullOrWhiteSpace(result.LearningNotes.JlptLevelAdvice));

        // 5. Bài tập micro-practice (Exercise)
        Assert.NotNull(result.Exercise);
        Assert.False(string.IsNullOrWhiteSpace(result.Exercise.Question));
        Assert.NotEmpty(result.Exercise.Options);
        Assert.False(string.IsNullOrWhiteSpace(result.Exercise.Explanation));
    }

    [Fact]
    public async Task ExplainSentenceAsync_OmittedSubject_IdentifiesImplicitSubjectGracefully()
    {
        // Câu tiếng Nhật thường lược bỏ chủ ngữ
        string sentence = "国境の長いトンネルを抜けると雪国であった。";

        var request = new AiSentenceExplanationRequest
        {
            Sentence = sentence,
            NovelTitle = "雪国",
            JlptLevel = "N3"
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.SentenceStructure);
        Assert.Contains("Lược bỏ", result.SentenceStructure.Subject);
        Assert.NotEmpty(result.DetailedGrammar);
        Assert.Contains(result.DetailedGrammar, g => g.Pattern.Contains("と"));
    }

    [Fact]
    public async Task ExplainSentenceAsync_MultipleClausesAndConjugations_AnalyzesBothClauses()
    {
        string sentence = "上から下へ白くけむった銀河の帯のようなところを指しながら。";

        var request = new AiSentenceExplanationRequest
        {
            Sentence = sentence,
            NovelTitle = "銀河鉄道の夜",
            JlptLevel = "N3"
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.SentenceStructure);
        Assert.NotEmpty(result.DetailedVocabulary);
        Assert.Contains(result.DetailedVocabulary, v => v.Word.Contains("銀河") || v.Word.Contains("帯"));
        Assert.Contains(result.DetailedGrammar, g => g.Pattern.Contains("ながら"));
    }

    [Theory]
    [InlineData("N5")]
    [InlineData("N3")]
    [InlineData("N1")]
    public async Task ExplainSentenceAsync_DifferentJlptLevels_GeneratesTargetedAdvice(string level)
    {
        var request = new AiSentenceExplanationRequest
        {
            Sentence = "先生は黒板に大きな図を描きました。",
            JlptLevel = level
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.LearningNotes);
        Assert.Contains(level, result.LearningNotes.JlptLevelAdvice);
    }

    [Fact]
    public async Task ExplainSentenceAsync_MultiTurnFollowUp_MaintainsQuestionContext()
    {
        var request = new AiSentenceExplanationRequest
        {
            Sentence = "彼は何も言わずに、静かに部屋を出ていった。",
            RequestType = "follow_up",
            FollowUpQuestion = "Tại sao không dùng 言わないで mà dùng 言わずに?",
            PreviousExplanation = "Mẫu ずに là dạng phủ định văn viết."
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.True(result.Success);
        Assert.Contains("Giải đáp câu hỏi tiếp theo", result.GrammarInsight);
        Assert.Contains(request.FollowUpQuestion, result.GrammarInsight);
    }

    [Fact]
    public async Task GenerateExerciseAsync_GeneratesInteractiveQuestionWithExplanation()
    {
        var request = new AiExerciseRequest
        {
            Sentence = "彼は何も言わずに、静かに部屋を出ていった。",
            JlptLevel = "N3"
        };

        var exercise = await _aiTutorService.GenerateExerciseAsync(request);

        Assert.NotNull(exercise);
        Assert.False(string.IsNullOrWhiteSpace(exercise.Question));
        Assert.Equal(4, exercise.Options.Count);
        Assert.InRange(exercise.CorrectOptionIndex, 0, 3);
        Assert.False(string.IsNullOrWhiteSpace(exercise.Explanation));
    }

    [Fact]
    public async Task ExplainSentenceAsync_MissingContext_ProvidesHelpfulUncertaintiesNotice()
    {
        var request = new AiSentenceExplanationRequest
        {
            Sentence = "走った。",
            Context = "" // Missing context
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Uncertainties);
        Assert.Contains("ngữ cảnh", result.Uncertainties);
    }

    [Fact]
    public async Task ExplainSentenceAsync_OversizedInput_TruncatesSafelyWithoutCrashing()
    {
        string giantSentence = new string('あ', 2500);

        var request = new AiSentenceExplanationRequest
        {
            Sentence = giantSentence
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.True(result.Success);
        Assert.True(result.Sentence.Length <= 1000); // Safely capped
    }

    [Fact]
    public async Task ExplainSentenceAsync_OfflineAndUnavailable_GracefullyFallsBackPreservingReaderAndDict()
    {
        // When AI is offline, Reader and Dictionary functions continue seamlessly
        var request = new AiSentenceExplanationRequest
        {
            Sentence = "銀河鉄道の夜。"
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.True(result.Success);
        Assert.False(result.IsAiGenerated);
        Assert.Contains("Ngoại tuyến", result.Provider);

        // Verify underlying dictionary and analyzer remain 100% operational
        var dictEntry = await _dictService.LookupAsync("銀河");
        Assert.NotNull(dictEntry);
        Assert.Equal("ぎんが", dictEntry.Reading);
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

