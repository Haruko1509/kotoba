using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Services;

namespace KotobaOdyssey.Tests;

public class SpacedRepetitionCalculatorTests
{
    [Fact]
    public void CalculateNextReview_AgainRating_ResetsIntervalAndRepetitions()
    {
        var reviewDate = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        var result = SpacedRepetitionCalculator.CalculateNextReview(
            currentIntervalDays: 14,
            currentEaseFactor: 2.5,
            currentRepetitions: 3,
            rating: ReviewRating.Again,
            reviewDateUtc: reviewDate);

        Assert.Equal(1, result.NewIntervalDays);
        Assert.Equal(0, result.NewRepetitions);
        Assert.Equal(reviewDate.Date.AddDays(1), result.NextReviewDate);
        Assert.True(result.NewEaseFactor >= SpacedRepetitionCalculator.MinEaseFactor);
    }

    [Fact]
    public void CalculateNextReview_FirstGoodRating_SetsIntervalToOne()
    {
        var reviewDate = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        var result = SpacedRepetitionCalculator.CalculateNextReview(
            currentIntervalDays: 0,
            currentEaseFactor: 2.5,
            currentRepetitions: 0,
            rating: ReviewRating.Good,
            reviewDateUtc: reviewDate);

        Assert.Equal(1, result.NewIntervalDays);
        Assert.Equal(1, result.NewRepetitions);
        Assert.Equal(reviewDate.Date.AddDays(1), result.NextReviewDate);
    }

    [Fact]
    public void CalculateNextReview_SecondGoodRating_SetsIntervalToSix()
    {
        var reviewDate = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        var result = SpacedRepetitionCalculator.CalculateNextReview(
            currentIntervalDays: 1,
            currentEaseFactor: 2.5,
            currentRepetitions: 1,
            rating: ReviewRating.Good,
            reviewDateUtc: reviewDate);

        Assert.Equal(6, result.NewIntervalDays);
        Assert.Equal(2, result.NewRepetitions);
        Assert.Equal(reviewDate.Date.AddDays(6), result.NextReviewDate);
    }

    [Fact]
    public void CalculateNextReview_ThirdGoodRating_ScalesIntervalByEaseFactor()
    {
        var reviewDate = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        var result = SpacedRepetitionCalculator.CalculateNextReview(
            currentIntervalDays: 6,
            currentEaseFactor: 2.5,
            currentRepetitions: 2,
            rating: ReviewRating.Good,
            reviewDateUtc: reviewDate);

        // 6 * 2.5 = 15
        Assert.Equal(15, result.NewIntervalDays);
        Assert.Equal(3, result.NewRepetitions);
    }

    [Fact]
    public void CalculateNextReview_EaseFactorNeverDropsBelowMinimum()
    {
        var reviewDate = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        var result = SpacedRepetitionCalculator.CalculateNextReview(
            currentIntervalDays: 1,
            currentEaseFactor: 1.3,
            currentRepetitions: 0,
            rating: ReviewRating.Again,
            reviewDateUtc: reviewDate);

        Assert.True(result.NewEaseFactor >= SpacedRepetitionCalculator.MinEaseFactor);
    }
}
