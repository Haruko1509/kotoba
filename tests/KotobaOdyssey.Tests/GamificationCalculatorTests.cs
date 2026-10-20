using KotobaOdyssey.Core.Services;

namespace KotobaOdyssey.Tests;

public class GamificationCalculatorTests
{
    [Theory]
    [InlineData(1, 250)]
    [InlineData(2, 313)] // 250 * 1.25 = 312.5 -> 313
    [InlineData(3, 391)] // 250 * 1.25^2 = 390.6 -> 391
    public void GetRequiredXpForLevel_ReturnsCorrectCurvedXp(int level, int expectedXp)
    {
        var requiredXp = GamificationCalculator.GetRequiredXpForLevel(level);
        Assert.Equal(expectedXp, requiredXp);
    }

    [Fact]
    public void AddXp_WhenNotEnoughToLevelUp_IncreasesCurrentXpOnly()
    {
        var (newLevel, newCurrentXp, newRequiredXp, didLevelUp) = GamificationCalculator.AddXp(
            currentLevel: 1,
            currentXp: 50,
            addedXp: 100);

        Assert.Equal(1, newLevel);
        Assert.Equal(150, newCurrentXp);
        Assert.Equal(250, newRequiredXp);
        Assert.False(didLevelUp);
    }

    [Fact]
    public void AddXp_WhenSurpassingRequired_LevelsUpCorrectly()
    {
        // Level 1 requires 250 XP. Starting at 200, add 100 -> total 300.
        // New Level: 2, Remaining XP: 300 - 250 = 50.
        var (newLevel, newCurrentXp, newRequiredXp, didLevelUp) = GamificationCalculator.AddXp(
            currentLevel: 1,
            currentXp: 200,
            addedXp: 100);

        Assert.Equal(2, newLevel);
        Assert.Equal(50, newCurrentXp);
        Assert.Equal(313, newRequiredXp);
        Assert.True(didLevelUp);
    }

    [Fact]
    public void AddXp_WhenMultiLevelSurge_LevelsUpMultipleTimes()
    {
        // Starting at Level 1, 0 XP. Add 600 XP.
        // Level 1 requires 250 (rem: 350) -> Level 2
        // Level 2 requires 313 (rem: 37) -> Level 3
        var (newLevel, newCurrentXp, _, didLevelUp) = GamificationCalculator.AddXp(
            currentLevel: 1,
            currentXp: 0,
            addedXp: 600);

        Assert.Equal(3, newLevel);
        Assert.Equal(37, newCurrentXp);
        Assert.True(didLevelUp);
    }

    [Fact]
    public void CalculateStreak_StudyingConsecutiveDay_IncrementsStreak()
    {
        var yesterday = new DateTime(2026, 10, 19, 15, 0, 0, DateTimeKind.Utc);
        var today = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        var (newStreak, streakIncreased) = GamificationCalculator.CalculateStreak(
            currentStreak: 4,
            lastStudyDateUtc: yesterday,
            currentStudyDateUtc: today);

        Assert.Equal(5, newStreak);
        Assert.True(streakIncreased);
    }

    [Fact]
    public void CalculateStreak_StudyingSameDay_KeepsStreakUnchanged()
    {
        var earlierToday = new DateTime(2026, 10, 20, 8, 0, 0, DateTimeKind.Utc);
        var laterToday = new DateTime(2026, 10, 20, 14, 0, 0, DateTimeKind.Utc);

        var (newStreak, streakIncreased) = GamificationCalculator.CalculateStreak(
            currentStreak: 5,
            lastStudyDateUtc: earlierToday,
            currentStudyDateUtc: laterToday);

        Assert.Equal(5, newStreak);
        Assert.False(streakIncreased);
    }

    [Fact]
    public void CalculateStreak_MissedDays_ResetsStreakToOne()
    {
        var threeDaysAgo = new DateTime(2026, 10, 17, 10, 0, 0, DateTimeKind.Utc);
        var today = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        var (newStreak, streakIncreased) = GamificationCalculator.CalculateStreak(
            currentStreak: 12,
            lastStudyDateUtc: threeDaysAgo,
            currentStudyDateUtc: today);

        Assert.Equal(1, newStreak);
        Assert.False(streakIncreased);
    }
}
