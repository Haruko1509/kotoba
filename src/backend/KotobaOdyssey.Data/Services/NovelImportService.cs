using System.Text.Json;
using System.Text.RegularExpressions;
using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using Microsoft.Extensions.Logging;

namespace KotobaOdyssey.Data.Services;

public class NovelImportService : INovelImportService
{
    private readonly INovelRepository _novelRepository;
    private readonly ILogger<NovelImportService> _logger;

    public NovelImportService(INovelRepository novelRepository, ILogger<NovelImportService> logger)
    {
        _novelRepository = novelRepository;
        _logger = logger;
    }

    public async Task<NovelImportResult> ImportFromJsonAsync(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return NovelImportResult.Failure("Nội dung file JSON trống hoặc không hợp lệ.");
        }

        NovelJsonDto? dto;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            dto = JsonSerializer.Deserialize<NovelJsonDto>(jsonContent, options);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse novel JSON.");
            return NovelImportResult.Failure($"Định dạng JSON không hợp lệ: {ex.Message}");
        }

        if (dto == null)
        {
            return NovelImportResult.Failure("Không thể giải mã dữ liệu tiểu thuyết từ JSON.");
        }

        // Validation
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            errors.Add("Tiêu đề truyện (Title) không được để trống.");
        }

        if (dto.Chapters == null || dto.Chapters.Count == 0)
        {
            errors.Add("Truyện phải có ít nhất một chương (Chapters).");
        }
        else
        {
            for (int i = 0; i < dto.Chapters.Count; i++)
            {
                var ch = dto.Chapters[i];
                if (string.IsNullOrWhiteSpace(ch.Content))
                {
                    errors.Add($"Chương {i + 1} ({ch.Title}) không có nội dung.");
                }
            }
        }

        if (errors.Count > 0)
        {
            return NovelImportResult.Failure("Dữ liệu tiểu thuyết không hợp lệ.", errors);
        }

        // Check duplicate
        if (await _novelRepository.ExistsWithTitleAsync(dto.Title))
        {
            return NovelImportResult.Failure($"Truyện có tiêu đề '{dto.Title}' đã tồn tại trong thư viện.");
        }

        // Parse JLPT Level
        JlptLevel level = JlptLevel.N5;
        if (!string.IsNullOrWhiteSpace(dto.JlptLevel) && Enum.TryParse<JlptLevel>(dto.JlptLevel, true, out var parsedLevel))
        {
            level = parsedLevel;
        }

        var novel = new Novel
        {
            Title = dto.Title.Trim(),
            JapaneseTitle = string.IsNullOrWhiteSpace(dto.JapaneseTitle) ? dto.Title.Trim() : dto.JapaneseTitle.Trim(),
            RomajiTitle = dto.RomajiTitle?.Trim() ?? string.Empty,
            Author = string.IsNullOrWhiteSpace(dto.Author) ? "Khuyết danh" : dto.Author.Trim(),
            Description = dto.Description?.Trim() ?? "Tiểu thuyết do người dùng nhập khẩu.",
            Genre = string.IsNullOrWhiteSpace(dto.Genre) ? "General" : dto.Genre.Trim(),
            JlptLevel = level,
            IsDemoData = false,
            IsFavorite = false,
            CreatedAt = DateTime.UtcNow
        };

        int chapterNum = 1;
        foreach (var chDto in dto.Chapters!)
        {
            var content = chDto.Content?.Trim() ?? string.Empty;
            novel.Chapters.Add(new Chapter
            {
                ChapterNumber = chDto.ChapterNumber > 0 ? chDto.ChapterNumber : chapterNum++,
                Title = string.IsNullOrWhiteSpace(chDto.Title) ? $"Chương {chapterNum}" : chDto.Title.Trim(),
                Content = content,
                WordCount = CountJapaneseCharacters(content),
                IsCompleted = false,
                LastReadPosition = 0
            });
        }

        var saved = await _novelRepository.AddAsync(novel);
        _logger.LogInformation("Successfully imported novel '{Title}' with {Count} chapters from JSON.", saved.Title, saved.Chapters.Count);
        return NovelImportResult.Success(saved);
    }

    public async Task<NovelImportResult> ImportFromTxtAsync(
        string textContent,
        string title,
        string? author,
        JlptLevel jlptLevel,
        string? genre)
    {
        if (string.IsNullOrWhiteSpace(textContent))
        {
            return NovelImportResult.Failure("Nội dung văn bản TXT không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return NovelImportResult.Failure("Vui lòng nhập tiêu đề cho tác phẩm.");
        }

        // Check duplicate
        if (await _novelRepository.ExistsWithTitleAsync(title.Trim()))
        {
            return NovelImportResult.Failure($"Truyện có tiêu đề '{title.Trim()}' đã tồn tại trong thư viện.");
        }

        var novel = new Novel
        {
            Title = title.Trim(),
            JapaneseTitle = title.Trim(),
            Author = string.IsNullOrWhiteSpace(author) ? "Khuyết danh" : author.Trim(),
            Description = "Tiểu thuyết văn bản thuần (TXT) do người dùng nhập khẩu.",
            Genre = string.IsNullOrWhiteSpace(genre) ? "General" : genre.Trim(),
            JlptLevel = jlptLevel,
            IsDemoData = false,
            IsFavorite = false,
            CreatedAt = DateTime.UtcNow
        };

        // Regex for detecting Japanese chapter headers (e.g. 第一章, 第一話, Chapter 1, 1., 一、)
        var chapterRegex = new Regex(@"(?m)^(第[一二三四五六七八九十0-9]+[章話節].*|Chapter\s+\d+.*|[一二三四五六七八九十\d]+[、.．]\s*.*)$", RegexOptions.Compiled);
        var matches = chapterRegex.Matches(textContent);

        if (matches.Count > 0)
        {
            // Split into chapters based on headers
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                int startPos = match.Index;
                int endPos = (i < matches.Count - 1) ? matches[i + 1].Index : textContent.Length;

                string chapterRaw = textContent.Substring(startPos, endPos - startPos).Trim();
                string header = match.Value.Trim();
                // Remove header line from content if header is first line
                string content = chapterRaw.StartsWith(header)
                    ? chapterRaw.Substring(header.Length).TrimStart('\r', '\n', ' ')
                    : chapterRaw;

                novel.Chapters.Add(new Chapter
                {
                    ChapterNumber = i + 1,
                    Title = header,
                    Content = string.IsNullOrWhiteSpace(content) ? header : content,
                    WordCount = CountJapaneseCharacters(content),
                    IsCompleted = false,
                    LastReadPosition = 0
                });
            }
        }
        else
        {
            // Single chapter
            novel.Chapters.Add(new Chapter
            {
                ChapterNumber = 1,
                Title = "第一章 (Chương 1)",
                Content = textContent.Trim(),
                WordCount = CountJapaneseCharacters(textContent.Trim()),
                IsCompleted = false,
                LastReadPosition = 0
            });
        }

        var saved = await _novelRepository.AddAsync(novel);
        _logger.LogInformation("Successfully imported novel '{Title}' from TXT with {Count} chapters.", saved.Title, saved.Chapters.Count);
        return NovelImportResult.Success(saved);
    }

    private static int CountJapaneseCharacters(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        // Count non-whitespace characters
        return text.Count(c => !char.IsWhiteSpace(c));
    }
}
