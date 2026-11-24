using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Xunit;
using Xunit.Abstractions;

namespace KotobaOdyssey.Tests;

public class AiTutorAcceptanceTests
{
    private readonly ITestOutputHelper _output;
    private readonly GeminiAiTutorService _aiTutorService;
    private readonly JapaneseTextAnalyzer _textAnalyzer;
    private readonly JapaneseDictionaryService _dictService;

    public AiTutorAcceptanceTests(ITestOutputHelper output)
    {
        _output = output;
        _dictService = new JapaneseDictionaryService();
        _textAnalyzer = new JapaneseTextAnalyzer(_dictService);
        var config = new DummyConfiguration();
        var logger = NullLogger<GeminiAiTutorService>.Instance;

        _aiTutorService = new GeminiAiTutorService(_textAnalyzer, _dictService, config, logger);
    }

    [Fact]
    public async Task Acceptance_Scenario1_SelectSentenceInNovel_AiAnalyzesAndExplainsGrammar()
    {
        _output.WriteLine("================================================================================");
        _output.WriteLine("TÌNH HUỐNG 1: CHỌN MỘT CÂU TRONG TRUYỆN -> AI PHÂN TÍCH VÀ GIẢI THÍCH NGỮ PHÁP");
        _output.WriteLine("================================================================================");

        string sentence = "彼は何も言わずに、静かに部屋を出ていった。";
        _output.WriteLine($"[1. Câu tiếng Nhật được chọn từ Reader]: \"{sentence}\"");

        var request = new AiSentenceExplanationRequest
        {
            Sentence = sentence,
            NovelTitle = "Văn học Nhật Bản",
            ChapterTitle = "Chương 1",
            JlptLevel = "N3",
            RequestType = "explain",
            IncludeExercise = true
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Success);

        _output.WriteLine($"\n[2. Bản dịch tiếng Việt tự nhiên]: {result.TranslationVn}");
        Assert.False(string.IsNullOrWhiteSpace(result.TranslationVn));

        _output.WriteLine("\n[3. Phân tích cấu trúc cú pháp (Sentence Structure)]:");
        Assert.NotNull(result.SentenceStructure);
        _output.WriteLine($" - Chủ ngữ:    {result.SentenceStructure.Subject}");
        _output.WriteLine($" - Vị ngữ:     {result.SentenceStructure.Predicate}");
        _output.WriteLine($" - Tân ngữ:    {result.SentenceStructure.Object}");
        _output.WriteLine($" - Bổ nghĩa:   {string.Join(", ", result.SentenceStructure.Modifiers)}");
        _output.WriteLine($" - Trợ từ:     {string.Join("; ", result.SentenceStructure.ParticleNotes)}");
        Assert.False(string.IsNullOrWhiteSpace(result.SentenceStructure.Predicate));

        _output.WriteLine("\n[4. Giải thích ngữ pháp chi tiết (Detailed Grammar)]:");
        Assert.NotEmpty(result.DetailedGrammar);
        foreach (var g in result.DetailedGrammar)
        {
            _output.WriteLine($" • Mẫu [{g.Pattern}]: {g.Meaning}");
            _output.WriteLine($"   - Công thức: {g.Structure}");
            _output.WriteLine($"   - Sắc thái:  {g.UsageAndNuance}");
            if (!string.IsNullOrWhiteSpace(g.ExampleSentence))
            {
                _output.WriteLine($"   - Ví dụ:     {g.ExampleSentence} ({g.ExampleTranslation})");
            }
        }
        Assert.Contains(result.DetailedGrammar, g => g.Pattern.Contains("ずに"));
        Assert.Contains(result.DetailedGrammar, g => g.Pattern.Contains("ていく") || g.Pattern.Contains("ていった"));

        _output.WriteLine($"\n[5. Sắc thái văn học]: {result.CulturalOrLiteraryNuance}");
        _output.WriteLine("=> TÌNH HUỐNG 1: HOÀN TOÀN ĐẠT TIÊU CHÍ!");
    }

    [Fact]
    public async Task Acceptance_Scenario2_WordWithMultipleMeanings_AiDeterminesContextualMeaning()
    {
        _output.WriteLine("================================================================================");
        _output.WriteLine("TÌNH HUỐNG 2: TỪ CÓ NHIỀU NGHĨA -> AI XÁC ĐỊNH NGHĨA PHÙ HỢP VỚI NGỮ CẢNH CÂU");
        _output.WriteLine("================================================================================");

        // Từ '抜ける' trong tiếng Nhật có rất nhiều nghĩa:
        // 1. Thoát ra, xuyên qua (đường hầm, con đường)
        // 2. Rụng (tóc, lông)
        // 3. Thiếu, sót (chữ, thông tin)
        // 4. Bay hơi, mất mùi vị
        string sentence = "国境の長いトンネルを抜けると雪国であった。";
        string targetWord = "抜ける";
        _output.WriteLine($"[1. Câu tiếng Nhật]: \"{sentence}\"");
        _output.WriteLine($"[2. Từ được tra cứu]: \"{targetWord}\" (trong câu ở dạng chia '抜けると')");

        var request = new AiSentenceExplanationRequest
        {
            Sentence = sentence,
            SelectedText = targetWord,
            NovelTitle = "雪国 (Xứ tuyết)",
            ChapterTitle = "Chương 1",
            JlptLevel = "N3",
            RequestType = "vocab"
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEmpty(result.DetailedVocabulary);

        var vocabEntry = result.DetailedVocabulary.FirstOrDefault(v => v.Word.Contains("抜ける") || v.BaseForm.Contains("抜ける"));
        Assert.NotNull(vocabEntry);

        _output.WriteLine($"\n[3. Kết quả xác định nghĩa trong ngữ cảnh]:");
        _output.WriteLine($" - Từ gốc:          {vocabEntry.BaseForm}");
        _output.WriteLine($" - Cách đọc:        {vocabEntry.Reading}");
        _output.WriteLine($" - Từ loại:         {vocabEntry.PartOfSpeech}");
        _output.WriteLine($" - Nghĩa ngữ cảnh:  {vocabEntry.MeaningInContext}");
        _output.WriteLine($" - Ghi chú ngữ dụng:{vocabEntry.UsageNote}");

        // Xác nhận nghĩa liên quan đến 'Vượt qua / Xuyên qua đường hầm', KHÔNG PHẢI 'rụng tóc' hay 'bỏ sót'
        Assert.True(
            vocabEntry.MeaningInContext.Contains("Vượt qua") || 
            vocabEntry.MeaningInContext.Contains("thoát ra") || 
            vocabEntry.MeaningInContext.Contains("xuyên qua") ||
            vocabEntry.MeaningInContext.Contains("đi qua"));

        _output.WriteLine("=> TÌNH HUỐNG 2: HOÀN TOÀN ĐẠT TIÊU CHÍ!");
    }

    [Fact]
    public async Task Acceptance_Scenario3_ChangeJlptN5ToN2_ExplanationChangesAccordingly()
    {
        _output.WriteLine("================================================================================");
        _output.WriteLine("TÌNH HUỐNG 3: ĐỔI JLPT TỪ N5 LÊN N2 -> CÁCH GIẢI THÍCH THAY ĐỔI TƯƠNG ỨNG");
        _output.WriteLine("================================================================================");

        string sentence = "彼は何も言わずに、静かに部屋を出ていった。";
        _output.WriteLine($"[Câu văn kiểm tra]: \"{sentence}\"");

        // 3.1 Yêu cầu với trình độ N5 (Sơ cấp)
        var requestN5 = new AiSentenceExplanationRequest
        {
            Sentence = sentence,
            JlptLevel = "N5"
        };
        var resultN5 = await _aiTutorService.ExplainSentenceAsync(requestN5);

        // 3.2 Yêu cầu với trình độ N2 (Trung thượng cấp)
        var requestN2 = new AiSentenceExplanationRequest
        {
            Sentence = sentence,
            JlptLevel = "N2"
        };
        var resultN2 = await _aiTutorService.ExplainSentenceAsync(requestN2);

        Assert.NotNull(resultN5.LearningNotes);
        Assert.NotNull(resultN2.LearningNotes);

        _output.WriteLine($"\n[1. Lời khuyên & Định hướng sư phạm cho N5]:");
        _output.WriteLine($"   {resultN5.LearningNotes.JlptLevelAdvice}");

        _output.WriteLine($"\n[2. Lời khuyên & Định hướng sư phạm cho N2]:");
        _output.WriteLine($"   {resultN2.LearningNotes.JlptLevelAdvice}");

        // Kiểm tra sự phân hóa rõ rệt về cấp độ trong kết quả
        Assert.Contains("N5", resultN5.LearningNotes.JlptLevelAdvice);
        Assert.Contains("N2", resultN2.LearningNotes.JlptLevelAdvice);
        Assert.NotEqual(resultN5.LearningNotes.JlptLevelAdvice, resultN2.LearningNotes.JlptLevelAdvice);

        _output.WriteLine("=> TÌNH HUỐNG 3: HOÀN TOÀN ĐẠT TIÊU CHÍ!");
    }

    [Fact]
    public async Task Acceptance_Scenario4_FollowUpQuestion_AiMaintainsSentenceContext()
    {
        _output.WriteLine("================================================================================");
        _output.WriteLine("TÌNH HUỐNG 4: HỎI TIẾP AI VỀ PHẦN VỪA GIẢI THÍCH -> AI VẪN NHỚ CÂU ĐANG HỌC");
        _output.WriteLine("================================================================================");

        string sentence = "彼は何も言わずに、静かに部屋を出ていった。";
        string followUp = "Tại sao ở đây tác giả lại dùng '言わずに' mà không dùng '言わないで'?";

        _output.WriteLine($"[1. Câu đang đọc trong Reader]: \"{sentence}\"");
        _output.WriteLine($"[2. Người học đặt câu hỏi tiếp theo]: \"{followUp}\"");

        var request = new AiSentenceExplanationRequest
        {
            Sentence = sentence,
            NovelTitle = "Văn học Nhật Bản",
            ChapterTitle = "Chương 1",
            JlptLevel = "N3",
            RequestType = "follow_up",
            FollowUpQuestion = followUp,
            PreviousExplanation = "Mẫu câu ～ずに mang nghĩa không làm V1 mà làm V2."
        };

        var result = await _aiTutorService.ExplainSentenceAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Success);

        _output.WriteLine($"\n[3. Lời giải đáp nối tiếp của AI Tutor]:");
        _output.WriteLine($"   {result.GrammarInsight}");

        // Khẳng định AI phản hồi đúng trọng tâm câu hỏi follow-up và vẫn neo chặt vào câu đang học
        Assert.Contains(followUp, result.GrammarInsight);
        Assert.Contains(sentence, result.Sentence);

        _output.WriteLine("=> TÌNH HUỐNG 4: HOÀN TOÀN ĐẠT TIÊU CHÍ!");
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

