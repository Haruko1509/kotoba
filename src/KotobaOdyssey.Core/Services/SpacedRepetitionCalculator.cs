using KotobaOdyssey.Core.Enums;

namespace KotobaOdyssey.Core.Services;

public record SpacedRepetitionResult(
    int NewIntervalDays,
    double NewEaseFactor,
    int NewRepetitions,
    DateTime NextReviewDate
);

public class SpacedRepetitionCalculator
{
    public const double MinEaseFactor = 1.3;
    public const double DefaultEaseFactor = 2.5;

    /// <summary>
    /// Calculates next interval and ease factor based on SuperMemo-2 algorithm.
    /// </summary>
    public static SpacedRepetitionResult CalculateNextReview(
        int currentIntervalDays,
        double currentEaseFactor,
        int currentRepetitions,
        ReviewRating rating,
        DateTime reviewDateUtc)
    {
        double ef = currentEaseFactor <= 0 ? DefaultEaseFactor : currentEaseFactor;
        int reps = currentRepetitions;
        int interval;

        // Convert ReviewRating to SM-2 numeric grade (0..5)
        // Again -> 1, Hard -> 2, Good -> 4, Easy -> 5
        int grade = rating switch
        {
            ReviewRating.Again => 1,
            ReviewRating.Hard => 2,
            ReviewRating.Good => 4,
            ReviewRating.Easy => 5,
            _ => 3
        };

        if (grade < 3)
        {
            // Failed recall
            reps = 0;
            interval = 1;
        }
        else
        {
            // Successful recall
            if (reps == 0)
            {
                interval = 1;
            }
            else if (reps == 1)
            {
                interval = 6;
            }
            else
            {
                interval = (int)Math.Round(currentIntervalDays * ef);
                if (interval <= currentIntervalDays)
                {
                    interval = currentIntervalDays + 1;
                }
            }

            reps++;
        }

        // SM-2 EF formula: EF' = EF + (0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02))
        double efDelta = 0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02);
        ef = Math.Round(Math.Max(MinEaseFactor, ef + efDelta), 2);

        DateTime nextReviewDate = reviewDateUtc.Date.AddDays(interval);

        return new SpacedRepetitionResult(interval, ef, reps, nextReviewDate);
    }
}
