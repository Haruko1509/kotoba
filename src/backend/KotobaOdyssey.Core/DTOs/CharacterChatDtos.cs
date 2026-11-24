namespace KotobaOdyssey.Core.DTOs;

public class CharacterProfileDto
{
    public string CharacterId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string JapaneseName { get; set; } = string.Empty;
    public string RomajiName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string SpriteUrl { get; set; } = string.Empty;
    public string NovelTitle { get; set; } = string.Empty;
    public int? NovelId { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string Personality { get; set; } = string.Empty;
    public string SpeakingStyle { get; set; } = string.Empty;
    public string PronounFirstPerson { get; set; } = "僕"; // 僕, 私, 俺, おいら, わし
    public string PronounSecondPerson { get; set; } = "君"; // 君, あなた, お前, 貴様
    public string Relationships { get; set; } = string.Empty;
    public string KnownTimeline { get; set; } = string.Empty;
    public int? MaxSafeChapter { get; set; } // Chapters beyond this contain critical spoilers
    public List<string> SuggestedPrompts { get; set; } = new();
}

public class CharacterChatRequest
{
    public string CharacterId { get; set; } = "giovanni";
    public int? NovelId { get; set; }
    public int? ChapterNumber { get; set; }
    public string UserMessage { get; set; } = string.Empty;
    public string JlptLevel { get; set; } = "N3";
    public string ConversationMode { get; set; } = "free"; // "free", "practice", "scenario"
    public string? ScenarioId { get; set; }
    public string? ScenarioDescription { get; set; }
    public string CorrectionMode { get; set; } = "gentle"; // "none", "gentle", "detailed"
    public string? SessionId { get; set; }
    public List<ChatMessageHistoryDto>? History { get; set; }
}

public class ChatMessageHistoryDto
{
    public string Role { get; set; } = "user"; // "user" or "character"
    public string Text { get; set; } = string.Empty;
}

public class CharacterChatResponse
{
    public bool Success { get; set; } = true;
    public string CharacterId { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public string CharacterJapaneseName { get; set; } = string.Empty;
    public string ReplyJapanese { get; set; } = string.Empty;
    public string ReplyVietnamese { get; set; } = string.Empty;
    public string Expression { get; set; } = "neutral"; // "happy", "thinking", "surprised", "neutral", "gentle", "sad"
    public string GrammarFeedback { get; set; } = string.Empty;
    public GentleCorrectionDto? Correction { get; set; }
    public string ConversationMode { get; set; } = "free";
    public string? SessionId { get; set; }
    public string Provider { get; set; } = "RuleBased-PersonaEngine"; // "Gemini-1.5-Flash" or "RuleBased-PersonaEngine"
    public bool IsAiGenerated { get; set; } = false;
    public List<string> SuggestedFollowups { get; set; } = new();
    public List<CharacterVocabItemDto> KeyVocabulary { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class GentleCorrectionDto
{
    public bool HasCorrection { get; set; }
    public string OriginalSentence { get; set; } = string.Empty;
    public string CorrectedSentence { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Severity { get; set; } = "gentle"; // "minor_nuance", "grammatical_error"
}

public class CharacterVocabItemDto
{
    public string Word { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = "Danh từ";
    public string JlptLevel { get; set; } = "N3";
    public string ContextSentence { get; set; } = string.Empty;
}

public class ConversationReviewRequest
{
    public string CharacterId { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string JlptLevel { get; set; } = "N3";
    public List<ChatMessageHistoryDto> Messages { get; set; } = new();
}

public class ConversationReviewResponse
{
    public bool Success { get; set; } = true;
    public string CharacterId { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public int TotalTurns { get; set; }
    public List<CharacterVocabItemDto> VocabularyLearned { get; set; } = new();
    public List<string> GrammarPointsUsed { get; set; } = new();
    public List<string> KeyImprovements { get; set; } = new();
    public string SummaryVietnamese { get; set; } = string.Empty;
    public string EncouragementMessage { get; set; } = string.Empty;
}
