using KotobaOdyssey.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KotobaOdyssey.Data;

public class KotobaDbContext : DbContext
{
    public DbSet<Novel> Novels => Set<Novel>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<VocabularyItem> VocabularyItems => Set<VocabularyItem>();
    public DbSet<ReviewLog> ReviewLogs => Set<ReviewLog>();
    public DbSet<UserProgress> UserProgresses => Set<UserProgress>();
    public DbSet<DailyQuest> DailyQuests => Set<DailyQuest>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<ChapterQuizAttempt> ChapterQuizAttempts => Set<ChapterQuizAttempt>();
    public DbSet<CharacterChatMessage> CharacterChatMessages => Set<CharacterChatMessage>();

    public KotobaDbContext(DbContextOptions<KotobaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Novel configuration
        modelBuilder.Entity<Novel>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(255);
            entity.Property(n => n.JapaneseTitle).HasMaxLength(255);
            entity.Property(n => n.Author).HasMaxLength(150);
            entity.Property(n => n.Genre).HasMaxLength(100);

            entity.HasMany(n => n.Chapters)
                .WithOne(c => c.Novel)
                .HasForeignKey(c => c.NovelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Performance Indexes for fast filtering and ordering
            entity.HasIndex(n => n.LastReadAt);
            entity.HasIndex(n => n.JlptLevel);
            entity.HasIndex(n => n.IsFavorite);
            entity.HasIndex(n => n.Title);
        });

        // Chapter configuration
        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).IsRequired().HasMaxLength(255);

            entity.HasMany(c => c.Bookmarks)
                .WithOne(b => b.Chapter)
                .HasForeignKey(b => b.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => new { c.NovelId, c.ChapterNumber });
        });

        // VocabularyItem configuration
        modelBuilder.Entity<VocabularyItem>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Word).IsRequired().HasMaxLength(100);
            entity.Property(v => v.Reading).HasMaxLength(100);
            entity.Property(v => v.Meaning).IsRequired().HasMaxLength(500);

            entity.HasMany(v => v.ReviewHistory)
                .WithOne(r => r.VocabularyItem)
                .HasForeignKey(r => r.VocabularyItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Performance Indexes for SRS SM-2 due calculation & lookups
            entity.HasIndex(v => v.NextReviewDate);
            entity.HasIndex(v => v.SourceChapterId);
            entity.HasIndex(v => v.Word);
            entity.HasIndex(v => v.JlptLevel);
        });

        // DailyQuest configuration
        modelBuilder.Entity<DailyQuest>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Title).IsRequired().HasMaxLength(200);
        });

        // UserProgress configuration
        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.UserName).IsRequired().HasMaxLength(100);
        });

        // Achievement configuration
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Title).IsRequired().HasMaxLength(150);
        });
    }
}
