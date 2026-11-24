using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Models;

namespace KotobaOdyssey.Core.Interfaces;

public interface ICharacterDialogueService
{
    Task<IReadOnlyList<CharacterProfileDto>> GetProfilesAsync(int? novelId = null);
    Task<CharacterProfileDto?> GetProfileByIdAsync(string characterId);
    Task<CharacterChatResponse> ChatAsync(CharacterChatRequest request);
    Task<IReadOnlyList<CharacterChatMessage>> GetHistoryAsync(string characterId, string? sessionId = null, int limit = 20);
    Task ClearHistoryAsync(string characterId, string? sessionId = null);
    Task<ConversationReviewResponse> GenerateReviewAsync(ConversationReviewRequest request);
}
