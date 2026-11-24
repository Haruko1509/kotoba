using KotobaOdyssey.Core.DTOs;

namespace KotobaOdyssey.Core.Interfaces;

public interface IAiTutorService
{
    Task<AiSentenceExplanationResult> ExplainSentenceAsync(AiSentenceExplanationRequest request);
    Task<AiExerciseDto> GenerateExerciseAsync(AiExerciseRequest request);
}
