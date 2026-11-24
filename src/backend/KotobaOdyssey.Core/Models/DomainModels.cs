using KotobaOdyssey.Core.Enums;

namespace KotobaOdyssey.Core.Models;

public class Novel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string JapaneseTitle { get; set; } = string.Empty;
    public string RomajiTitle { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverImagePath { get; set; } = string.Empty;
    public JlptLevel JlptLevel { get; set; } = JlptLevel.N5;
    public string Genre { get; set; } = "Fantasy";
    public int TotalChapters => Chapters?.Count ?? 0;
    public bool IsFavorite { get; set; }
    public bool IsDemoData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReadAt { get; set; }

    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}

public class Chapter
{
    public int Id { get; set; }
    public int NovelId { get; set; }
    public Novel? Novel { get; set; }
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public bool IsCompleted { get; set; }
    public int LastReadPosition { get; set; }
    public DateTime? LastReadAt { get; set; }

    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}

public class Bookmark
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public Chapter? Chapter { get; set; }
    public int CharacterPosition { get; set; }
    public string Snippet { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VocabularyItem
{
    public int Id { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public JlptLevel JlptLevel { get; set; } = JlptLevel.N5;
    public string ContextSentence { get; set; } = string.Empty;
    public string SourceNovelTitle { get; set; } = string.Empty;
    public int? SourceChapterId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Spaced repetition properties (SM-2 variant)
    public DateTime NextReviewDate { get; set; } = DateTime.UtcNow;
    public int IntervalDays { get; set; } = 0;
    public double EaseFactor { get; set; } = 2.5;
    public int Repetitions { get; set; } = 0;

    public ICollection<ReviewLog> ReviewHistory { get; set; } = new List<ReviewLog>();
}

public class ReviewLog
{
    public int Id { get; set; }
    public int VocabularyItemId { get; set; }
    public VocabularyItem? VocabularyItem { get; set; }
    public ReviewRating Rating { get; set; }
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
    public int OldIntervalDays { get; set; }
    public int NewIntervalDays { get; set; }
    public double EaseFactorAfter { get; set; }
}

public class UserProgress
{
    public int Id { get; set; }
    public string UserName { get; set; } = "Traveler";
    public string Title { get; set; } = "Star Reader";
    public int Level { get; set; } = 1;
    public int CurrentXp { get; set; } = 140;
    public int RequiredXp { get; set; } = 300;
    public int StreakDays { get; set; } = 5;
    public DateTime LastStudyDate { get; set; } = DateTime.UtcNow.Date;
    public int TodayStudyMinutes { get; set; } = 25;
    public int TotalStudyMinutes { get; set; } = 380;
    public int TotalWordsLearned { get; set; } = 64;
    public int TotalChaptersRead { get; set; } = 7;
}

public class DailyQuest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestType Type { get; set; }
    public int TargetCount { get; set; }
    public int CurrentCount { get; set; }
    public int RewardXp { get; set; }
    public bool IsClaimed { get; set; }
    public DateTime QuestDate { get; set; } = DateTime.UtcNow.Date;

    public bool IsCompleted => CurrentCount >= TargetCount;
}

public class Achievement
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconKey { get; set; } = "Star";
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public int RewardXp { get; set; } = 100;
}

public class ChapterQuizAttempt
{
    public int Id { get; set; }
    public int NovelId { get; set; }
    public int ChapterNumber { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int ScorePercentage { get; set; }
    public bool Passed { get; set; }
    public int XpAwarded { get; set; }
    public int ReadingScorePercent { get; set; } = 100;
    public int VocabScorePercent { get; set; } = 100;
    public int GrammarScorePercent { get; set; } = 100;
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}

public class CharacterChatMessage
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string CharacterId { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // "user" or "character"
    public string ContentJapanese { get; set; } = string.Empty;
    public string ContentVietnamese { get; set; } = string.Empty;
    public string Expression { get; set; } = "neutral";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
