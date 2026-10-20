namespace KotobaOdyssey.Core.Enums;

public enum JlptLevel
{
    None = 0,
    N5 = 5,
    N4 = 4,
    N3 = 3,
    N2 = 2,
    N1 = 1
}

public enum ReviewRating
{
    Again = 1,
    Hard = 2,
    Good = 3,
    Easy = 4
}

public enum ReadingMode
{
    StandardReader = 0,
    VisualNovel = 1
}

public enum QuestType
{
    ReadMinutes,
    ReviewCards,
    AddVocabulary,
    CompleteChapter
}
