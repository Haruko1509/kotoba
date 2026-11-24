using KotobaOdyssey.Core.Enums;

namespace KotobaOdyssey.Core.DTOs;

public class AiChatMessageDto
{
    public string Role { get; set; } = "user"; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class AiSentenceExplanationRequest
{
    public string Sentence { get; set; } = string.Empty;
    public string? SelectedText { get; set; }
    public string Context { get; set; } = string.Empty;
    public string? ContextBefore { get; set; }
    public string? ContextAfter { get; set; }
    public string NovelTitle { get; set; } = string.Empty;
    public string ChapterTitle { get; set; } = string.Empty;
    public int? ChapterNumber { get; set; }
    public string JlptLevel { get; set; } = "N3";
    public string RequestType { get; set; } = "explain"; // "explain", "translate", "grammar", "vocab", "follow_up", "simpler", "more_examples", "easy_japanese"
    public string? FollowUpQuestion { get; set; }
    public string? PreviousExplanation { get; set; }
    public string? DictionaryNotes { get; set; }
    public string ExplanationLanguage { get; set; } = "Vietnamese";
    public List<AiChatMessageDto> ConversationHistory { get; set; } = new();
    public bool IncludeExercise { get; set; } = true;
}

public class AiSentenceStructureDto
{
    public string Subject { get; set; } = string.Empty;
    public string Predicate { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public List<string> Modifiers { get; set; } = new();
    public List<string> ParticleNotes { get; set; } = new();
    public string ClauseRelations { get; set; } = string.Empty;
}

public class AiGrammarDetailDto
{
    public string Pattern { get; set; } = string.Empty;
    public string Structure { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string UsageAndNuance { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public string? ExampleTranslation { get; set; }
}

public class AiVocabDetailDto
{
    public string Word { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
    public string MeaningInContext { get; set; } = string.Empty;
    public string BaseForm { get; set; } = string.Empty;
    public string InflectedForm { get; set; } = string.Empty;
    public string UsageNote { get; set; } = string.Empty;
}

public class AiLearningNotesDto
{
    public List<string> KeyTakeaways { get; set; } = new();
    public List<string> CommonMistakes { get; set; } = new();
    public string JlptLevelAdvice { get; set; } = string.Empty;
}

public class AiExerciseDto
{
    public string ExerciseType { get; set; } = "MeaningChoice"; // "MeaningChoice", "ParticleChoice", "GrammarChoice", "SentenceReorder"
    public string Question { get; set; } = string.Empty;
    public string PromptJapanese { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectOptionIndex { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

public class AiExerciseRequest
{
    public string Sentence { get; set; } = string.Empty;
    public string JlptLevel { get; set; } = "N3";
    public string? GrammarFocus { get; set; }
    public string NovelTitle { get; set; } = string.Empty;
}

public class AiSentenceExplanationResult
{
    public bool Success { get; set; }
    public string Sentence { get; set; } = string.Empty;
    public string TranslationVn { get; set; } = string.Empty;
    public string? FuriganaOrReading { get; set; }
    public string GrammarInsight { get; set; } = string.Empty;
    public string CulturalOrLiteraryNuance { get; set; } = string.Empty;
    public string? Uncertainties { get; set; }
    public bool IsAiGenerated { get; set; }
    public string Provider { get; set; } = "RuleBased-NLP";
    public string? ErrorMessage { get; set; }

    // Enhanced Structured Analysis
    public AiSentenceStructureDto? SentenceStructure { get; set; }
    public List<AiGrammarDetailDto> DetailedGrammar { get; set; } = new();
    public List<AiVocabDetailDto> DetailedVocabulary { get; set; } = new();
    public AiLearningNotesDto? LearningNotes { get; set; }
    public AiExerciseDto? Exercise { get; set; }

    // Backwards compatibility collections
    public List<AiTokenExplanation> Tokens { get; set; } = new();
    public List<KeyVocabItemDto> KeyVocabulary { get; set; } = new();
}

public class AiTokenExplanation
{
    public string Word { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? JlptLevel { get; set; }
}

public class KeyVocabItemDto
{
    public string Word { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string JlptLevel { get; set; } = "N3";
}
