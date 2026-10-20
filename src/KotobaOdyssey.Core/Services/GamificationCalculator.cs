namespace KotobaOdyssey.Core.Services;

public class GamificationCalculator
{
    public const int BaseXpPerLevel = 250;
    public const double GrowthFactor = 1.25;

    /// <summary>
    /// Calculates the required total XP to reach the next level from a given level.
    /// Level 1 -> 250
    /// Level 2 -> 312
    /// Level 3 -> 390 ...
    /// </summary>
    public static int GetRequiredXpForLevel(int level)
    {
        if (level < 1) level = 1;
        return (int)Math.Round(BaseXpPerLevel * Math.Pow(GrowthFactor, level - 1), MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Updates user XP and calculates level-ups.
    /// Handles multiple level-ups if gained XP exceeds required XP.
    /// </summary>
    public static (int newLevel, int newCurrentXp, int newRequiredXp, bool didLevelUp) AddXp(int currentLevel, int currentXp, int addedXp)
    {
        if (addedXp <= 0)
        {
            var req = GetRequiredXpForLevel(currentLevel);
            return (currentLevel, currentXp, req, false);
        }

        int level = currentLevel;
        int xp = currentXp + addedXp;
        int required = GetRequiredXpForLevel(level);
        bool leveledUp = false;

        while (xp >= required)
        {
            xp -= required;
            level++;
            required = GetRequiredXpForLevel(level);
            leveledUp = true;
        }

        return (level, xp, required, leveledUp);
    }

    /// <summary>
    /// Calculates updated streak based on previous study date and current study date.
    /// </summary>
    public static (int newStreak, bool streakIncreased) CalculateStreak(int currentStreak, DateTime lastStudyDateUtc, DateTime currentStudyDateUtc)
    {
        var lastDate = lastStudyDateUtc.Date;
        var today = currentStudyDateUtc.Date;

        if (today == lastDate)
        {
            // Already studied today; streak stays the same
            return (currentStreak, false);
        }
        else if (today == lastDate.AddDays(1))
        {
            // Studied yesterday -> streak increments!
            return (currentStreak + 1, true);
        }
        else
        {
            // Missed one or more days -> streak resets to 1
            return (1, false);
        }
    }
}
