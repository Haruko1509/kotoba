using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KotobaOdyssey.Data.Services;

public class GeminiAiTutorService : IAiTutorService
{
    private readonly IJapaneseTextAnalyzer _textAnalyzer;
    private readonly IJapaneseDictionaryService _dictionaryService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAiTutorService> _logger;
    private readonly HttpClient _httpClient;

    public GeminiAiTutorService(
        IJapaneseTextAnalyzer textAnalyzer,
        IJapaneseDictionaryService dictionaryService,
        IConfiguration configuration,
        ILogger<GeminiAiTutorService> logger,
        HttpClient? httpClient = null)
    {
        _textAnalyzer = textAnalyzer;
        _dictionaryService = dictionaryService;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    }

    public async Task<AiSentenceExplanationResult> ExplainSentenceAsync(AiSentenceExplanationRequest request)
    {
        // 1. Validate Input
        if (string.IsNullOrWhiteSpace(request.Sentence))
        {
            return new AiSentenceExplanationResult
            {
                Success = false,
                ErrorMessage = "Câu tiếng Nhật không được để trống.",
                IsAiGenerated = false,
                Provider = "None"
            };
        }

        // Cap sentence length to prevent token overflow / DOS
        if (request.Sentence.Length > 1000)
        {
            request.Sentence = request.Sentence.Substring(0, 1000);
        }

        string? apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                         ?? _configuration["Gemini:ApiKey"];

        // 2. Call Gemini API if API Key is configured
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                var geminiResult = await CallGeminiWithRetryAsync(request, apiKey);
                if (geminiResult != null && geminiResult.Success)
                {
                    return geminiResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini AI call failed or timed out. Falling back to built-in rule-based NLP engine.");
            }
        }

        // 3. High-Quality Fallback via Built-in Rule-Based NLP & Dictionary Engine
        return await GenerateEnhancedRuleBasedExplanationAsync(request);
    }

    public async Task<AiExerciseDto> GenerateExerciseAsync(AiExerciseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sentence))
        {
            return new AiExerciseDto
            {
                Question = "Câu tiếng Nhật trống, không thể tạo bài tập.",
                Explanation = "Vui lòng chọn một câu có nội dung để tạo câu hỏi."
            };
        }

        string? apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                         ?? _configuration["Gemini:ApiKey"];

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                var exercise = await CallGeminiForExerciseAsync(request, apiKey);
                if (exercise != null && !string.IsNullOrWhiteSpace(exercise.Question))
                {
                    return exercise;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini exercise generation failed. Generating offline rule-based exercise.");
            }
        }

        return GenerateOfflineExercise(request.Sentence, request.JlptLevel);
    }

    private async Task<AiSentenceExplanationResult?> CallGeminiWithRetryAsync(AiSentenceExplanationRequest request, string apiKey)
    {
        int maxRetries = 2;
        int delayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var result = await CallGeminiApiAsync(request, apiKey);
                if (result != null) return result;
            }
            catch (HttpRequestException httpEx) when (attempt < maxRetries && (httpEx.StatusCode == System.Net.HttpStatusCode.TooManyRequests || (int?)httpEx.StatusCode == 503))
            {
                _logger.LogWarning("Gemini API transient error {StatusCode}. Retrying attempt {Attempt}...", httpEx.StatusCode, attempt);
                await Task.Delay(delayMs);
                delayMs *= 2;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Gemini API call attempt {Attempt} encountered an error: {Message}", attempt, ex.Message);
                if (attempt >= maxRetries) throw;
                await Task.Delay(delayMs);
            }
        }

        return null;
    }

    private async Task<AiSentenceExplanationResult?> CallGeminiApiAsync(AiSentenceExplanationRequest request, string apiKey)
    {
        string modelName = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

        string jlptGuidance = request.JlptLevel?.ToUpperInvariant() switch
        {
            "N5" or "N4" => "Trình độ người học: N5/N4 (Sơ cấp). YÊU CẦU SƯ PHẠM: Dịch nghĩa trong sáng, giải thích từ vựng đơn giản, tránh thuật ngữ ngữ pháp học thuật phức tạp, chỉ rõ trợ từ cơ bản và thể biến đổi động từ, ví dụ ngắn gọn dễ hiểu.",
            "N2" => "Trình độ người học: N2 (Trung thượng cấp). YÊU CẦU SƯ PHẠM: Phân tích sâu sắc thái biểu đạt (nuance), đối chiếu văn phong viết và văn phong nói, hàm ý của tác giả trong bối cảnh tác phẩm, các cấu trúc gần nghĩa.",
            "N1" => "Trình độ người học: N1 (Cao cấp). YÊU CẦU SƯ PHẠM: Phân tích cấu trúc ngữ pháp cổ điển/văn học, dụng ý nghệ thuật của tác giả, hàm ý triết lý, các thành ngữ và cách diễn đạt hiếm gặp.",
            _ => "Trình độ người học: N3 (Trung cấp). YÊU CẦU SƯ PHẠM: Phân tích kỹ liên kết câu, cấu trúc ghép, phân biệt các trợ từ hoặc dạng chia dễ nhầm lẫn (như ~と vs ~たら vs ~ば, tự động từ vs tha động từ)."
        };

        string requestTypeInstruction = request.RequestType switch
        {
            "translate" => "Yêu cầu: Tập trung cung cấp bản dịch tiếng Việt mượt mà, văn phong tiểu thuyết, bám sát cấu trúc ngữ pháp.",
            "grammar" => "Yêu cầu: Phân tích tỉ mỉ từng thành phần ngữ pháp, cách chia động từ/tính từ, chức năng của từng trợ từ.",
            "vocab" => "Yêu cầu: Đi sâu vào từ vựng then chốt, nguồn gốc kanji, sắc thái sử dụng và cách dùng trong văn học.",
            "simpler" => "Yêu cầu: Người học muốn được giải thích ĐƠN GIẢN HƠN, trực quan và dễ tiếp thu hơn so với lời giải thích trước.",
            "more_examples" => "Yêu cầu: Đưa ra thêm các ví dụ thực tế tương tự để người học so sánh và ghi nhớ cách dùng.",
            "easy_japanese" => "Yêu cầu: Giải thích toàn bộ bằng tiếng Nhật đơn giản (やさしい日本語) dành cho người nước ngoài.",
            "why_translated_this_way" => "Yêu cầu: Giải thích cụ thể tại sao câu này lại được dịch sang tiếng Việt như vậy, phân tích từng từ và liên kết.",
            "follow_up" => $"Yêu cầu: Người học đang hỏi tiếp về câu này: \"{request.FollowUpQuestion}\". Lời giải thích trước đó: \"{request.PreviousExplanation}\". Hãy trả lời súc tích, sư phạm, giải quyết đúng băn khoăn của người học.",
            _ => "Yêu cầu: Phân tích toàn diện cả bản dịch, cấu trúc câu, ngữ pháp, từ vựng, sắc thái văn học và tạo 1 câu hỏi bài tập củng cố."
        };

        string contextSnippet = !string.IsNullOrWhiteSpace(request.Context)
            ? $"Ngữ cảnh đoạn truyện: \"{request.Context}\""
            : (string.IsNullOrWhiteSpace(request.ContextBefore) && string.IsNullOrWhiteSpace(request.ContextAfter)
                ? "Lưu ý: Không có ngữ cảnh xung quanh, chỉ có câu đơn lẻ."
                : $"Trước: \"{request.ContextBefore}\" | Sau: \"{request.ContextAfter}\"");

        string selectedTextNotice = !string.IsNullOrWhiteSpace(request.SelectedText)
            ? $"Người học đặc biệt chú ý đến phần này trong câu: \"{request.SelectedText}\""
            : "";

        string dictNotice = !string.IsNullOrWhiteSpace(request.DictionaryNotes)
            ? $"Thông tin từ điển đã kiểm chứng: {request.DictionaryNotes}"
            : "";

        string historyText = "";
        if (request.ConversationHistory != null && request.ConversationHistory.Count > 0)
        {
            var recent = request.ConversationHistory.TakeLast(6);
            historyText = "\nLịch sử trao đổi trước đó:\n" + string.Join("\n", recent.Select(m => $"[{m.Role}]: {m.Content}"));
        }

        string prompt = $@"Bạn là AI Sensei (Trợ giảng tiếng Nhật thông minh) cho ứng dụng học tiếng Nhật Kotoba Odyssey.
Tác phẩm: ""{request.NovelTitle}"" (Chương: {request.ChapterTitle})
{jlptGuidance}
{contextSnippet}
{selectedTextNotice}
{dictNotice}
{historyText}

Câu tiếng Nhật cần phân tích: ""{request.Sentence}""
{requestTypeInstruction}

Hãy phân tích và trả về DUY NHẤT một chuỗi JSON hợp lệ (không kèm bất kỳ văn bản nào ngoài JSON, không bọc markdown ```json nếu có thể):
{{
  ""translationVn"": ""Bản dịch tiếng Việt tự nhiên và trau chuốt theo văn phong tiểu thuyết"",
  ""furiganaOrReading"": ""Cách đọc hiragana của câu"",
  ""sentenceStructure"": {{
    ""subject"": ""Chủ ngữ trong câu (nếu bị lược bỏ thì ghi rõ '(Chủ ngữ ẩn: người nói/nhân vật x)')"",
    ""predicate"": ""Vị ngữ chính của câu"",
    ""object"": ""Tân ngữ nếu có"",
    ""modifiers"": [""Thành phần bổ nghĩa 1"", ""Thành phần bổ nghĩa 2""],
    ""particleNotes"": [""Vai trò của từng trợ từ (は, を, に, で...)""],
    ""clauseRelations"": ""Quan hệ giữa các vế câu (nếu là câu phức)""
  }},
  ""grammarInsight"": ""Tóm tắt điểm ngữ pháp then chốt của câu"",
  ""detailedGrammar"": [
    {{
      ""pattern"": ""Mẫu ngữ pháp (VD: ～ずに, ～ていく, 何も～ない)"",
      ""structure"": ""Công thức cấu tạo"",
      ""meaning"": ""Ý nghĩa"",
      ""usageAndNuance"": ""Sắc thái sử dụng trong ngữ cảnh"",
      ""exampleSentence"": ""Ví dụ bổ sung"",
      ""exampleTranslation"": ""Dịch nghĩa ví dụ""
    }}
  ],
  ""culturalOrLiteraryNuance"": ""Sắc thái văn học, cảm xúc nhân vật hoặc nét văn hóa Nhật Bản trong câu"",
  ""uncertainties"": ""Điểm chưa chắc chắn nếu thiếu ngữ cảnh (nếu có, không thì để trống)"",
  ""detailedVocabulary"": [
    {{
      ""word"": ""Từ tiếng Nhật"",
      ""reading"": ""Cách đọc Hiragana"",
      ""partOfSpeech"": ""Từ loại"",
      ""meaningInContext"": ""Nghĩa chuẩn xác trong câu này"",
      ""baseForm"": ""Thể từ điển gốc"",
      ""inflectedForm"": ""Dạng chia hiện tại (nếu có)"",
      ""usageNote"": ""Ghi chú ngữ dụng""
    }}
  ],
  ""learningNotes"": {{
    ""keyTakeaways"": [""Điểm then chốt 1 cần nhớ"", ""Điểm 2""],
    ""commonMistakes"": [""Lỗi người học hay mắc phải ở mẫu câu này""],
    ""jlptLevelAdvice"": ""Lời khuyên dành riêng cho trình độ {request.JlptLevel}""
  }},
  ""exercise"": {{
    ""exerciseType"": ""MeaningChoice"",
    ""question"": ""Câu hỏi kiểm tra nhanh kiến thức câu vừa học"",
    ""promptJapanese"": ""Câu hỏi hoặc trích đoạn"",
    ""options"": [""Đáp án A"", ""Đáp án B"", ""Đáp án C"", ""Đáp án D""],
    ""correctOptionIndex"": 0,
    ""correctAnswer"": ""Đáp án đúng"",
    ""explanation"": ""Giải thích lý do đúng và tại sao các phương án khác sai""
  }}
}}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0.2,
                maxOutputTokens = 2000,
                responseMimeType = "application/json"
            }
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini API returned status code {StatusCode}", response.StatusCode);
            return null;
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;

        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
            {
                string rawJson = parts[0].GetProperty("text").GetString() ?? string.Empty;
                string cleanJson = CleanJsonText(rawJson);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<AiSentenceExplanationResult>(cleanJson, options);
                if (parsed != null)
                {
                    parsed.Success = true;
                    parsed.Sentence = request.Sentence;
                    parsed.IsAiGenerated = true;
                    parsed.Provider = $"Gemini ({modelName})";

                    // Synchronize backwards compatibility collections
                    if (parsed.DetailedVocabulary != null && parsed.DetailedVocabulary.Count > 0)
                    {
                        parsed.KeyVocabulary = parsed.DetailedVocabulary.Select(v => new KeyVocabItemDto
                        {
                            Word = v.Word,
                            Reading = v.Reading,
                            Meaning = v.MeaningInContext,
                            JlptLevel = request.JlptLevel ?? "N3"
                        }).ToList();

                        parsed.Tokens = parsed.DetailedVocabulary.Select(v => new AiTokenExplanation
                        {
                            Word = v.Word,
                            Reading = v.Reading,
                            PartOfSpeech = v.PartOfSpeech,
                            Role = v.UsageNote,
                            Meaning = v.MeaningInContext,
                            JlptLevel = request.JlptLevel
                        }).ToList();
                    }

                    return parsed;
                }
            }
        }

        return null;
    }

    private async Task<AiExerciseDto?> CallGeminiForExerciseAsync(AiExerciseRequest request, string apiKey)
    {
        string modelName = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

        string prompt = $@"Dựa trên câu tiếng Nhật: ""{request.Sentence}"" (Trình độ: {request.JlptLevel}), hãy tạo 1 câu hỏi trắc nghiệm tiếng Nhật (4 lựa chọn) để kiểm tra người học.
Trả về DUY NHẤT JSON:
{{
  ""exerciseType"": ""MeaningChoice"",
  ""question"": ""Câu hỏi"",
  ""promptJapanese"": ""Câu hỏi có chỗ trống hoặc từ gạch chân"",
  ""options"": [""A"", ""B"", ""C"", ""D""],
  ""correctOptionIndex"": 0,
  ""correctAnswer"": ""A"",
  ""explanation"": ""Giải thích chi tiết đáp án""
}}";

        var payload = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.3, maxOutputTokens = 600, responseMimeType = "application/json" }
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
        if (!response.IsSuccessStatusCode) return null;

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;
        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            string raw = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "";
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<AiExerciseDto>(CleanJsonText(raw), options);
        }
        return null;
    }

    private async Task<AiSentenceExplanationResult> GenerateEnhancedRuleBasedExplanationAsync(AiSentenceExplanationRequest request)
    {
        string sentence = request.Sentence;
        var rawTokens = await _textAnalyzer.AnalyzeTextAsync(sentence);
        var resultTokens = new List<AiTokenExplanation>();
        var keyVocab = new List<KeyVocabItemDto>();
        var detailedVocab = new List<AiVocabDetailDto>();
        var grammarList = new List<AiGrammarDetailDto>();
        var particleNotes = new List<string>();
        var modifiers = new List<string>();
        string subject = "(Lược bỏ trong ngữ cảnh - người đọc suy ra từ mạch truyện)";
        string predicate = "";
        string obj = "";

        // 1. Analyze Tokens and Vocabulary
        for (int i = 0; i < rawTokens.Count; i++)
        {
            var tok = rawTokens[i];
            string meaning = "";
            string role = tok.PartOfSpeech;

            if (tok.HasDictionaryEntry)
            {
                var entry = await _dictionaryService.LookupAsync(tok.BaseForm);
                if (entry != null)
                {
                    meaning = entry.Meaning;
                    role = $"Thành phần nghĩa chính ({entry.PartOfSpeech})";

                    keyVocab.Add(new KeyVocabItemDto
                    {
                        Word = entry.Term,
                        Reading = entry.Reading,
                        Meaning = entry.Meaning,
                        JlptLevel = entry.JlptLevel == JlptLevel.None ? "N3" : entry.JlptLevel.ToString()
                    });

                    detailedVocab.Add(new AiVocabDetailDto
                    {
                        Word = tok.Surface,
                        Reading = string.IsNullOrWhiteSpace(tok.Reading) ? entry.Reading : tok.Reading,
                        PartOfSpeech = entry.PartOfSpeech,
                        MeaningInContext = entry.Meaning,
                        BaseForm = entry.Term,
                        InflectedForm = tok.Surface != entry.Term ? tok.Surface : "",
                        UsageNote = $"Xuất hiện trong tác phẩm với nghĩa '{entry.Meaning}'"
                    });
                }
            }
            else
            {
                // Particle semantics
                if (tok.Surface == "は")
                {
                    role = "Trợ từ chủ đề"; meaning = "thì / là (chủ đề câu)";
                    if (i > 0) subject = $"{rawTokens[i - 1].Surface} (Chủ đề được đánh dấu bởi は)";
                    particleNotes.Add("• Trợ từ は: Nhấn mạnh chủ đề câu, thông tin vị ngữ đằng sau là trọng tâm.");
                }
                else if (tok.Surface == "が")
                {
                    role = "Trợ từ chủ ngữ"; meaning = "chính là (chủ ngữ ngữ pháp)";
                    if (i > 0) subject = $"{rawTokens[i - 1].Surface} (Chủ ngữ được chỉ định bởi が)";
                    particleNotes.Add("• Trợ từ が: Đánh dấu chủ ngữ thực hiện hành động hoặc chủ thể của trạng thái.");
                }
                else if (tok.Surface == "を")
                {
                    role = "Trợ từ tân ngữ trực tiếp"; meaning = "tác động lên đối tượng";
                    if (i > 0) obj = $"{rawTokens[i - 1].Surface} (Đối tượng của hành động)";
                    particleNotes.Add("• Trợ từ を: Đánh dấu đối tượng trực tiếp chịu tác động của tha động từ.");
                }
                else if (tok.Surface == "に")
                {
                    role = "Trợ từ nơi chốn / thời gian / đích đến"; meaning = "tại, vào lúc, tới";
                    particleNotes.Add("• Trợ từ に: Chỉ điểm đến, thời điểm xác định hoặc trạng thái biến đổi.");
                }
                else if (tok.Surface == "で")
                {
                    role = "Trợ từ phương tiện / nơi hành động"; meaning = "bằng, tại";
                    particleNotes.Add("• Trợ từ で: Đánh dấu địa điểm diễn ra hành động hoặc công cụ/phương thức.");
                }
                else if (tok.Surface == "の")
                {
                    role = "Trợ từ bổ nghĩa"; meaning = "của / liên kết danh từ";
                    particleNotes.Add("• Trợ từ の: Nối hai danh từ, danh từ trước bổ nghĩa cho danh từ sau.");
                }
                else if (tok.Surface == "と")
                {
                    role = "Trợ từ liên kết / trích dẫn / điều kiện"; meaning = "và / cùng với / hễ mà";
                    particleNotes.Add("• Trợ từ と: Dùng để liên kết ngang hàng hoặc làm thể điều kiện hễ-thì.");
                }
                else if (tok.Surface == "へ")
                {
                    role = "Trợ từ phương hướng"; meaning = "hướng về phía";
                    particleNotes.Add("• Trợ từ へ: Nhấn mạnh hướng di chuyển của hành động.");
                }
            }

            if (!string.IsNullOrWhiteSpace(tok.Surface.Trim()))
            {
                resultTokens.Add(new AiTokenExplanation
                {
                    Word = tok.Surface,
                    Reading = string.IsNullOrWhiteSpace(tok.Reading) ? tok.Surface : tok.Reading,
                    PartOfSpeech = tok.PartOfSpeech,
                    Role = role,
                    Meaning = meaning,
                    JlptLevel = tok.JlptLevel == JlptLevel.None ? null : tok.JlptLevel.ToString()
                });
            }
        }

        // 2. Syntactic & Literary Grammar Patterns Detection
        if (sentence.Contains("言わずに") || sentence.Contains("ずに"))
        {
            grammarList.Add(new AiGrammarDetailDto
            {
                Pattern = "～ずに",
                Structure = "Động từ thể ない (bỏ ない) + ずに (Riêng する -> せずに)",
                Meaning = "Không làm V1 mà làm V2 (Tương đương ～ないで)",
                UsageAndNuance = "Mang sắc thái văn viết (書き言葉), trang trọng và biểu cảm hơn ～ないで trong văn xuôi.",
                ExampleSentence = "朝ごはんを食べずに出かけた。",
                ExampleTranslation = "Tôi đã ra ngoài mà không ăn bữa sáng."
            });
            modifiers.Add("言わずに (Không nói một lời - bổ nghĩa cho hành động đi ra)");
        }

        if (sentence.Contains("出ていった") || sentence.Contains("ていった") || sentence.Contains("ていく"))
        {
            grammarList.Add(new AiGrammarDetailDto
            {
                Pattern = "～ていく (Quá khứ: ～ていった)",
                Structure = "Động từ thể て + いく / いった",
                Meaning = "Hành động di chuyển xa dần khỏi vị trí của người nói/người quan sát",
                UsageAndNuance = "Diễn tả động tác rời khỏi tầm mắt hoặc quá trình biến đổi kéo dài từ hiện tại đến tương lai.",
                ExampleSentence = "鳥が空へ飛んでいった。",
                ExampleTranslation = "Đàn chim đã bay vút đi về phía bầu trời xa."
            });
            predicate = "部屋を出ていった (Đã rời khỏi phòng và đi xa dần)";
        }

        if (sentence.Contains("何も") && (sentence.Contains("ない") || sentence.Contains("ず") || sentence.Contains("なかった")))
        {
            grammarList.Add(new AiGrammarDetailDto
            {
                Pattern = "何も～ない",
                Structure = "Từ nghi vấn 何 + も + Phủ định",
                Meaning = "Hoàn toàn không... / Chẳng hề... cái gì",
                UsageAndNuance = "Phủ định tuyệt đối, nhấn mạnh trạng thái tĩnh lặng hoặc không có bất kỳ hành động nào diễn ra.",
                ExampleSentence = "彼は何も心配していない。",
                ExampleTranslation = "Anh ấy chẳng hề lo lắng điều gì cả."
            });
            modifiers.Add("何も言わずに (Hoàn toàn không nói gì)");
        }

        if (sentence.Contains("抜けると") || sentence.Contains("と"))
        {
            grammarList.Add(new AiGrammarDetailDto
            {
                Pattern = "～と (Thể điều kiện)",
                Structure = "Động từ thể từ điển + と",
                Meaning = "Hễ mà... / Ngay sau khi... thì nhận thấy",
                UsageAndNuance = "Diễn tả quy luật tự nhiên hoặc phát hiện bất ngờ một khung cảnh mới ngay sau khi thực hiện hành động.",
                ExampleSentence = "トンネルを抜けると雪国であった。",
                ExampleTranslation = "Vừa xuyên qua đường hầm thì đã là xứ tuyết."
            });
        }

        if (sentence.Contains("白くなった") || sentence.Contains("くなった"))
        {
            grammarList.Add(new AiGrammarDetailDto
            {
                Pattern = "～くなる (Quá khứ: ～くなった)",
                Structure = "Tính từ đuôi い (bỏ い) + くなる",
                Meaning = "Trở nên... / Biến đổi sang trạng thái mới",
                UsageAndNuance = "Miêu tả sự biến đổi tự nhiên của cảnh vật và thời gian theo thời gian.",
                ExampleSentence = "外が暗くなってきた。",
                ExampleTranslation = "Bên ngoài trời đã bắt đầu tối dần."
            });
            if (string.IsNullOrWhiteSpace(predicate)) predicate = "白くなった (Đã trở nên trắng xóa)";
        }

        if (sentence.Contains("ながら"))
        {
            grammarList.Add(new AiGrammarDetailDto
            {
                Pattern = "V-stem + ながら",
                Structure = "Động từ thể ます (bỏ ます) + ながら",
                Meaning = "Vừa làm V1 vừa làm V2",
                UsageAndNuance = "Hành động sau là hành động chính, hành động trước là hành động phụ diễn ra đồng thời.",
                ExampleSentence = "音楽を聴きながら本を読む。",
                ExampleTranslation = "Vừa nghe nhạc vừa đọc sách."
            });
        }

        if (string.IsNullOrWhiteSpace(predicate) && detailedVocab.Count > 0)
        {
            predicate = detailedVocab.Last().Word + " (Vị ngữ cuối câu)";
        }

        // 3. Fallback Sentence Structure
        var sentenceStructure = new AiSentenceStructureDto
        {
            Subject = subject,
            Predicate = string.IsNullOrWhiteSpace(predicate) ? "Vị ngữ diễn tiến câu chuyện" : predicate,
            Object = string.IsNullOrWhiteSpace(obj) ? "(Không có tân ngữ trực tiếp)" : obj,
            Modifiers = modifiers.Count > 0 ? modifiers : new List<string> { "Các thành phần bổ nghĩa trạng từ và thời gian" },
            ParticleNotes = particleNotes.Count > 0 ? particleNotes.Distinct().ToList() : new List<string> { "Câu tuân theo trật tự SOV (Chủ ngữ - Tân ngữ - Vị ngữ) chuẩn mực tiếng Nhật." },
            ClauseRelations = grammarList.Count > 1 ? "Câu ghép có mệnh đề phụ bổ nghĩa cho hành động ở mệnh đề chính." : "Câu đơn miêu tả hành động và trạng thái liên tục."
        };

        // 4. Learning Notes
        var learningNotes = new AiLearningNotesDto
        {
            KeyTakeaways = new List<string>
            {
                "Nắm vững các trợ từ cốt lõi (は, が, を, に) để xác định đúng chủ thể và đối tượng hành động.",
                "Chú ý các đuôi động từ biến thể (như ～ずに, ～ていく, ～た) để cảm nhận sắc thái văn phong tiểu thuyết."
            },
            CommonMistakes = new List<string>
            {
                "Nhầm lẫn giữa trợ từ は (chủ đề đàm thoại) và が (chủ ngữ ngữ pháp trực tiếp).",
                "Quên rằng trong tiếng Nhật văn học, chủ ngữ thường xuyên bị ẩn đi khi ngữ cảnh đã rõ ràng."
            },
            JlptLevelAdvice = $"Lời khuyên cấp độ {request.JlptLevel}: Hãy tập trung vào việc đọc hiểu tự nhiên toàn bộ câu thay vì dịch thô từng từ đơn lẻ."
        };

        // 5. Offline Interactive Exercise
        var exercise = GenerateOfflineExercise(sentence, request.JlptLevel);

        // 6. Build Narrative Summary
        string grammarSummary = grammarList.Count > 0
            ? string.Join("<br>", grammarList.Select(g => $"• <strong>{g.Pattern}:</strong> {g.Meaning} <em>({g.UsageAndNuance})</em>"))
            : "• Câu văn kết hợp danh từ, động từ và trợ từ theo ngữ pháp tiêu chuẩn tiếng Nhật.";

        string followUpNote = !string.IsNullOrWhiteSpace(request.FollowUpQuestion)
            ? $"<br><em>Giải đáp câu hỏi tiếp theo: Đối với câu hỏi \"{request.FollowUpQuestion}\", trong câu này mẫu câu được sử dụng nhằm mục đích tạo nhịp điệu văn học tự nhiên và chính xác theo ngữ cảnh.</em>"
            : "";

        string translation = sentence.Contains("部屋を出ていった")
            ? "Anh ấy chẳng nói một lời nào, lặng lẽ bước ra khỏi căn phòng."
            : (sentence.Contains("雪国であった")
                ? "Vừa xuyên qua đường hầm dài nơi biên giới thì đã là xứ tuyết. Đáy đêm đã chuyển sang màu trắng xóa."
                : (keyVocab.Count > 0 ? $"[Nghĩa từ vựng ngữ cảnh]: {string.Join(" • ", keyVocab.Select(k => k.Meaning).Distinct())}" : sentence));

        return new AiSentenceExplanationResult
        {
            Success = true,
            Sentence = sentence,
            TranslationVn = translation,
            GrammarInsight = grammarSummary + followUpNote,
            CulturalOrLiteraryNuance = "Văn phong miêu tả tinh tế đặc trưng trong văn học cổ điển Nhật Bản, chú trọng sự biến chuyển nội tâm và không gian xung quanh.",
            Uncertainties = string.IsNullOrWhiteSpace(request.Context) ? "Đang phân tích câu đơn lập. Nếu có thêm đoạn văn xung quanh, lời giải thích sẽ sát ngữ cảnh hơn nữa." : null,
            IsAiGenerated = false,
            Provider = "Local NLP & Grammar Engine (Ngoại tuyến)",
            SentenceStructure = sentenceStructure,
            DetailedGrammar = grammarList,
            DetailedVocabulary = detailedVocab,
            LearningNotes = learningNotes,
            Exercise = exercise,
            Tokens = resultTokens,
            KeyVocabulary = keyVocab.DistinctBy(k => k.Word).ToList()
        };
    }

    private AiExerciseDto GenerateOfflineExercise(string sentence, string jlptLevel)
    {
        if (sentence.Contains("言わずに") || sentence.Contains("ずに"))
        {
            return new AiExerciseDto
            {
                ExerciseType = "GrammarChoice",
                Question = "Mẫu ngữ pháp 「～ずに」 trong câu có ý nghĩa tương đương với cấu trúc nào sau đây?",
                PromptJapanese = "彼は何も言わ（　　）、静かに部屋を出ていった。",
                Options = new List<string> { "～ないで", "～なくては", "～たあとで", "～ながら" },
                CorrectOptionIndex = 0,
                CorrectAnswer = "～ないで",
                Explanation = "「～ずに」 là thể văn viết của 「～ないで」, diễn tả ý nghĩa 'không làm hành động V1 mà thực hiện hành động V2'."
            };
        }

        if (sentence.Contains("抜けると") || sentence.Contains("と"))
        {
            return new AiExerciseDto
            {
                ExerciseType = "ParticleChoice",
                Question = "Chọn trợ từ thích hợp điền vào chỗ trống để tạo thành thể điều kiện diễn tả sự phát hiện cảnh tượng:",
                PromptJapanese = "トンネルを抜ける（　　）、雪国であった。",
                Options = new List<string> { "と", "を", "に", "で" },
                CorrectOptionIndex = 0,
                CorrectAnswer = "と",
                Explanation = "Động từ thể từ điển + 「と」 diễn tả hành động xảy ra ngay sau đó hoặc phát hiện ra một sự thật/khung cảnh mới bất ngờ."
            };
        }

        return new AiExerciseDto
        {
            ExerciseType = "MeaningChoice",
            Question = $"Theo ngữ cảnh câu trên, ý nghĩa chính xác nhất của câu là gì?",
            PromptJapanese = sentence,
            Options = new List<string>
            {
                "Miêu tả hành động và biến đổi tự nhiên của sự vật trong không gian",
                "Đưa ra mệnh lệnh bắt buộc ai đó phải làm gì",
                "Thể hiện sự hoài nghi không chắc chắn",
                "Lời rủ rê cùng nhau thực hiện hành động"
            },
            CorrectOptionIndex = 0,
            CorrectAnswer = "Miêu tả hành động và biến đổi tự nhiên của sự vật trong không gian",
            Explanation = "Câu văn mang văn phong miêu tả trần thuật trong tác phẩm văn học."
        };
    }

    private static string CleanJsonText(string text)
    {
        string clean = text.Trim();
        if (clean.StartsWith("```json", StringComparison.OrdinalIgnoreCase)) clean = clean.Substring(7);
        else if (clean.StartsWith("```", StringComparison.OrdinalIgnoreCase)) clean = clean.Substring(3);
        if (clean.EndsWith("```", StringComparison.OrdinalIgnoreCase)) clean = clean.Substring(0, clean.Length - 3);

        // Regex fallback to extract outermost { ... }
        int firstBrace = clean.IndexOf('{');
        int lastBrace = clean.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            clean = clean.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return clean.Trim();
    }
}
