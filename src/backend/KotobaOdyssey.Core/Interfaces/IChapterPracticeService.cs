using KotobaOdyssey.Core.DTOs;

namespace KotobaOdyssey.Core.Interfaces;

public interface IChapterPracticeService
{
    Task<ChapterQuizDto> GetChapterQuizAsync(int novelId, int chapterNumber);
    Task<QuizSubmitResult> SubmitQuizAsync(QuizSubmitRequest request);
    Task<DailyPracticeRecommendationDto> GetDailyRecommendationAsync();
    Task<LearningAnalyticsDto> GetLearningAnalyticsAsync();
}
