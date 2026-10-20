using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Core.Services;

namespace KotobaOdyssey.Core.Interfaces;

public interface INovelRepository
{
    Task<IReadOnlyList<Novel>> GetAllAsync();
    Task<Novel?> GetByIdAsync(int id);
    Task<Novel?> GetLatestReadAsync();
    Task<IReadOnlyList<Novel>> GetFavoritesAsync();
    Task<bool> ExistsWithTitleAsync(string title);
    Task<Novel> AddAsync(Novel novel);
    Task UpdateAsync(Novel novel);
    Task DeleteAsync(int id);
    Task<Chapter?> GetChapterByIdAsync(int chapterId);
    Task UpdateChapterProgressAsync(int chapterId, int lastReadPosition, bool isCompleted);
}

public interface IBookmarkRepository
{
    Task<IReadOnlyList<Bookmark>> GetByChapterIdAsync(int chapterId);
    Task<Bookmark> AddAsync(Bookmark bookmark);
    Task DeleteAsync(int id);
}

public interface INovelImportService
{
    Task<NovelImportResult> ImportFromJsonAsync(string jsonContent);
    Task<NovelImportResult> ImportFromTxtAsync(string textContent, string title, string? author, JlptLevel jlptLevel, string? genre);
}

public interface IJapaneseDictionaryService
{
    Task<DictionaryEntry?> LookupAsync(string term);
    Task<IReadOnlyList<DictionaryEntry>> SearchSuggestionsAsync(string prefix, int limit = 10);
    Task<IReadOnlyList<DictionaryEntry>> LookupBatchAsync(IEnumerable<string> terms);
}

public interface IJapaneseTextAnalyzer
{
    Task<IReadOnlyList<TextToken>> AnalyzeTextAsync(string text);
}

public interface IVocabularyRepository
{
    Task<IReadOnlyList<VocabularyItem>> GetAllAsync();
    Task<IReadOnlyList<VocabularyItem>> GetDueReviewsAsync();
    Task<int> GetCountAsync();
    Task<int> GetDueCountAsync();
    Task<VocabularyItem?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(string word);
    Task<VocabularyItem> AddAsync(VocabularyItem item);
    Task UpdateAsync(VocabularyItem item);
    Task DeleteAsync(int id);
    Task<ReviewLog> RecordReviewAsync(int vocabularyItemId, ReviewRating rating, SpacedRepetitionResult result);
}

public interface IUserProgressRepository
{
    Task<UserProgress> GetProgressAsync();
    Task UpdateProgressAsync(UserProgress progress);
    Task AddXpAsync(int xp);
    Task UpdateStreakAsync();
    Task AddStudyMinutesAsync(int minutes);
    Task IncrementWordsLearnedAsync();
}

public interface IDailyQuestRepository
{
    Task<IReadOnlyList<DailyQuest>> GetTodayQuestsAsync();
    Task UpdateQuestAsync(DailyQuest quest);
    Task<bool> ClaimRewardAsync(int questId);
    Task IncrementQuestProgressAsync(QuestType type, int count = 1);
}

public interface IDatabaseInitializer
{
    Task InitializeAsync();
}

public interface INavigationService
{
    object? CurrentView { get; }
    string CurrentPageKey { get; }
    void NavigateTo<TViewModel>() where TViewModel : class;
    void NavigateTo(string pageKey, object? parameter = null);
    event Action? CurrentViewChanged;
}
