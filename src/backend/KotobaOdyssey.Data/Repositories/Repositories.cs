using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace KotobaOdyssey.Data.Repositories;

public class NovelRepository : INovelRepository
{
    private readonly KotobaDbContext _context;

    public NovelRepository(KotobaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Novel>> GetAllAsync()
    {
        return await _context.Novels
            .Include(n => n.Chapters)
            .OrderByDescending(n => n.LastReadAt ?? n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Novel?> GetByIdAsync(int id)
    {
        return await _context.Novels
            .Include(n => n.Chapters)
                .ThenInclude(c => c.Bookmarks)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Novel?> GetLatestReadAsync()
    {
        return await _context.Novels
            .Include(n => n.Chapters)
            .OrderByDescending(n => n.LastReadAt)
            .FirstOrDefaultAsync(n => n.LastReadAt != null)
            ?? await _context.Novels.Include(n => n.Chapters).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<Novel>> GetFavoritesAsync()
    {
        return await _context.Novels
            .Include(n => n.Chapters)
            .Where(n => n.IsFavorite)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsWithTitleAsync(string title)
    {
        var normalized = title.Trim().ToLower();
        return await _context.Novels.AnyAsync(n =>
            n.Title.ToLower() == normalized ||
            n.JapaneseTitle.ToLower() == normalized);
    }

    public async Task<Novel> AddAsync(Novel novel)
    {
        _context.Novels.Add(novel);
        await _context.SaveChangesAsync();
        return novel;
    }

    public async Task UpdateAsync(Novel novel)
    {
        _context.Novels.Update(novel);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var novel = await _context.Novels
            .Include(n => n.Chapters)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (novel != null)
        {
            _context.Novels.Remove(novel);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Chapter?> GetChapterByIdAsync(int chapterId)
    {
        return await _context.Chapters
            .Include(c => c.Bookmarks)
            .Include(c => c.Novel)
            .FirstOrDefaultAsync(c => c.Id == chapterId);
    }

    public async Task UpdateChapterProgressAsync(int chapterId, int lastReadPosition, bool isCompleted)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Novel)
            .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter != null)
        {
            var now = DateTime.UtcNow;
            chapter.LastReadPosition = Math.Max(0, lastReadPosition);
            chapter.IsCompleted = isCompleted || chapter.IsCompleted;
            chapter.LastReadAt = now;

            if (chapter.Novel != null)
            {
                chapter.Novel.LastReadAt = now;
            }

            await _context.SaveChangesAsync();
        }
    }
}

public class BookmarkRepository : IBookmarkRepository
{
    private readonly KotobaDbContext _context;

    public BookmarkRepository(KotobaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Bookmark>> GetByChapterIdAsync(int chapterId)
    {
        return await _context.Bookmarks
            .Where(b => b.ChapterId == chapterId)
            .OrderBy(b => b.CharacterPosition)
            .ToListAsync();
    }

    public async Task<Bookmark> AddAsync(Bookmark bookmark)
    {
        _context.Bookmarks.Add(bookmark);
        await _context.SaveChangesAsync();
        return bookmark;
    }

    public async Task DeleteAsync(int id)
    {
        var bookmark = await _context.Bookmarks.FindAsync(id);
        if (bookmark != null)
        {
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
        }
    }
}

public class VocabularyRepository : IVocabularyRepository
{
    private readonly KotobaDbContext _context;

    public VocabularyRepository(KotobaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VocabularyItem>> GetAllAsync()
    {
        return await _context.VocabularyItems
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<VocabularyItem>> GetDueReviewsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.VocabularyItems
            .Where(v => v.NextReviewDate <= now)
            .OrderBy(v => v.NextReviewDate)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.VocabularyItems.CountAsync();
    }

    public async Task<int> GetDueCountAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.VocabularyItems.CountAsync(v => v.NextReviewDate <= now);
    }

    public async Task<VocabularyItem?> GetByIdAsync(int id)
    {
        return await _context.VocabularyItems
            .Include(v => v.ReviewHistory)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<bool> ExistsAsync(string word)
    {
        var normalized = word.Trim().ToLower();
        return await _context.VocabularyItems.AnyAsync(v => v.Word.ToLower() == normalized);
    }

    public async Task<VocabularyItem> AddAsync(VocabularyItem item)
    {
        _context.VocabularyItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateAsync(VocabularyItem item)
    {
        _context.VocabularyItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.VocabularyItems.FindAsync(id);
        if (item != null)
        {
            _context.VocabularyItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ReviewLog> RecordReviewAsync(int vocabularyItemId, ReviewRating rating, SpacedRepetitionResult result)
    {
        var item = await _context.VocabularyItems.FindAsync(vocabularyItemId);
        if (item == null) throw new InvalidOperationException($"VocabularyItem {vocabularyItemId} not found.");

        int oldInterval = item.IntervalDays;
        item.IntervalDays = result.NewIntervalDays;
        item.EaseFactor = result.NewEaseFactor;
        item.Repetitions = result.NewRepetitions;
        item.NextReviewDate = result.NextReviewDate;

        var log = new ReviewLog
        {
            VocabularyItemId = vocabularyItemId,
            Rating = rating,
            ReviewedAt = DateTime.UtcNow,
            OldIntervalDays = oldInterval,
            NewIntervalDays = result.NewIntervalDays,
            EaseFactorAfter = result.NewEaseFactor
        };

        _context.ReviewLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }
}

public class UserProgressRepository : IUserProgressRepository
{
    private readonly KotobaDbContext _context;

    public UserProgressRepository(KotobaDbContext context)
    {
        _context = context;
    }

    public async Task<UserProgress> GetProgressAsync()
    {
        var progress = await _context.UserProgresses.FirstOrDefaultAsync();
        if (progress == null)
        {
            progress = new UserProgress();
            _context.UserProgresses.Add(progress);
            await _context.SaveChangesAsync();
        }
        return progress;
    }

    public async Task UpdateProgressAsync(UserProgress progress)
    {
        _context.UserProgresses.Update(progress);
        await _context.SaveChangesAsync();
    }

    public async Task AddXpAsync(int xp)
    {
        var progress = await GetProgressAsync();
        var (newLevel, newCurrentXp, newRequiredXp, _) = GamificationCalculator.AddXp(
            progress.Level, progress.CurrentXp, xp);

        progress.Level = newLevel;
        progress.CurrentXp = newCurrentXp;
        progress.RequiredXp = newRequiredXp;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateStreakAsync()
    {
        var progress = await GetProgressAsync();
        var (newStreak, _) = GamificationCalculator.CalculateStreak(
            progress.StreakDays, progress.LastStudyDate, DateTime.UtcNow);

        progress.StreakDays = newStreak;
        progress.LastStudyDate = DateTime.UtcNow.Date;
        await _context.SaveChangesAsync();
    }

    public async Task AddStudyMinutesAsync(int minutes)
    {
        if (minutes <= 0) return;
        var progress = await GetProgressAsync();
        progress.TodayStudyMinutes += minutes;
        progress.TotalStudyMinutes += minutes;
        await _context.SaveChangesAsync();
    }

    public async Task IncrementWordsLearnedAsync()
    {
        var progress = await GetProgressAsync();
        progress.TotalWordsLearned++;
        await _context.SaveChangesAsync();
    }
}

public class DailyQuestRepository : IDailyQuestRepository
{
    private readonly KotobaDbContext _context;
    private readonly IUserProgressRepository _userProgressRepository;

    public DailyQuestRepository(KotobaDbContext context, IUserProgressRepository userProgressRepository)
    {
        _context = context;
        _userProgressRepository = userProgressRepository;
    }

    public async Task<IReadOnlyList<DailyQuest>> GetTodayQuestsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.DailyQuests
            .Where(q => q.QuestDate == today)
            .OrderBy(q => q.Id)
            .ToListAsync();
    }

    public async Task UpdateQuestAsync(DailyQuest quest)
    {
        _context.DailyQuests.Update(quest);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ClaimRewardAsync(int questId)
    {
        var quest = await _context.DailyQuests.FindAsync(questId);
        if (quest == null || quest.IsClaimed || !quest.IsCompleted)
        {
            return false;
        }

        quest.IsClaimed = true;
        await _userProgressRepository.AddXpAsync(quest.RewardXp);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task IncrementQuestProgressAsync(QuestType type, int count = 1)
    {
        var today = DateTime.UtcNow.Date;
        var quests = await _context.DailyQuests
            .Where(q => q.QuestDate == today && q.Type == type && !q.IsClaimed)
            .ToListAsync();

        foreach (var q in quests)
        {
            q.CurrentCount = Math.Min(q.TargetCount, q.CurrentCount + count);
        }

        await _context.SaveChangesAsync();
    }
}
