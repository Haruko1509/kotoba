namespace KotobaOdyssey.Core.DTOs;

public class ChapterQuizDto
{
    public int NovelId { get; set; }
    public string NovelTitle { get; set; } = string.Empty;
    public int ChapterNumber { get; set; }
    public string ChapterTitle { get; set; } = string.Empty;
    public string JlptLevel { get; set; } = "N3";
    public List<QuizQuestionDto> Questions { get; set; } = new();
}

public class QuizQuestionDto
{
    public int QuestionId { get; set; }
    public string QuestionJp { get; set; } = string.Empty;
    public string QuestionVn { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "multiple_choice"; // "multiple_choice", "cloze", "true_false"
    public string Category { get; set; } = "ReadingComprehension"; // "ReadingComprehension", "Vocabulary", "Grammar"
    public string ContextSentence { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string TargetGrammarOrVocab { get; set; } = string.Empty;
    public int Points { get; set; } = 10;
}

public class QuizSubmitRequest
{
    public int NovelId { get; set; }
    public int ChapterNumber { get; set; }
    public Dictionary<int, int> Answers { get; set; } = new(); // QuestionId -> SelectedOptionIndex
}

public class QuizSubmitResult
{
    public bool Success { get; set; } = true;
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int ScorePercentage { get; set; }
    public int XpEarned { get; set; }
    public bool Passed { get; set; }
    public string FeedbackSummary { get; set; } = string.Empty;
    public List<QuestionReviewDto> QuestionReviews { get; set; } = new();
    public List<CharacterVocabItemDto> SuggestedReviewVocab { get; set; } = new();

    // Category Breakdown
    public int ReadingCorrect { get; set; }
    public int ReadingTotal { get; set; }
    public int VocabCorrect { get; set; }
    public int VocabTotal { get; set; }
    public int GrammarCorrect { get; set; }
    public int GrammarTotal { get; set; }
    public int ReadingScorePercent => ReadingTotal > 0 ? (int)Math.Round((double)ReadingCorrect / ReadingTotal * 100) : 100;
    public int VocabScorePercent => VocabTotal > 0 ? (int)Math.Round((double)VocabCorrect / VocabTotal * 100) : 100;
    public int GrammarScorePercent => GrammarTotal > 0 ? (int)Math.Round((double)GrammarCorrect / GrammarTotal * 100) : 100;
}

public class QuestionReviewDto
{
    public int QuestionId { get; set; }
    public string QuestionJp { get; set; } = string.Empty;
    public string Category { get; set; } = "ReadingComprehension";
    public bool IsCorrect { get; set; }
    public int UserChoiceIndex { get; set; }
    public int CorrectIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string TargetKnowledge { get; set; } = string.Empty;
}

public class DailyPracticeRecommendationDto
{
    public int DueSrsCardsCount { get; set; }
    public string SuggestedNovelTitle { get; set; } = string.Empty;
    public int SuggestedNovelId { get; set; }
    public int SuggestedChapterNum { get; set; }
    public string SuggestedChapterTitle { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
}

public class LearningAnalyticsDto
{
    // Reading Progress
    public int TotalNovels { get; set; }
    public int CompletedChapters { get; set; }
    public int InProgressNovels { get; set; }
    public double OverallReadingProgressPercent { get; set; }
    public string LastReadNovelTitle { get; set; } = string.Empty;
    public string LastReadChapterTitle { get; set; } = string.Empty;

    // Vocabulary & SRS Mastery
    public int TotalSavedWords { get; set; }
    public int NewWords { get; set; }
    public int LearningWords { get; set; }
    public int MasteredWords { get; set; }
    public int DueReviewsCount { get; set; }
    public double VocabularyRetentionRate { get; set; }

    // Quiz Performance
    public int TotalQuizzesAttempted { get; set; }
    public int QuizzesPassed { get; set; }
    public int AverageQuizScorePercent { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int OverallAccuracyPercent { get; set; }
    public int ReadingAccuracyPercent { get; set; }
    public int VocabAccuracyPercent { get; set; }
    public int GrammarAccuracyPercent { get; set; }

    // Learning Activity
    public int StreakDays { get; set; }
    public int TotalStudyMinutes { get; set; }
    public int TodayStudyMinutes { get; set; }
    public int UserLevel { get; set; }
    public int CurrentXp { get; set; }

    // AI & Pedagogical Insights
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> ActionableRecommendations { get; set; } = new();

    // History
    public List<RecentQuizAttemptDto> RecentQuizHistory { get; set; } = new();
}

public class RecentQuizAttemptDto
{
    public int NovelId { get; set; }
    public string NovelTitle { get; set; } = string.Empty;
    public int ChapterNumber { get; set; }
    public int ScorePercentage { get; set; }
    public bool Passed { get; set; }
    public int XpAwarded { get; set; }
    public DateTime AttemptedAt { get; set; }
}
