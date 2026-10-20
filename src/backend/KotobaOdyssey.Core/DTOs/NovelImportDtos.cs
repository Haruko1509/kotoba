using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Models;

namespace KotobaOdyssey.Core.DTOs;

public class NovelJsonDto
{
    public string Title { get; set; } = string.Empty;
    public string? JapaneseTitle { get; set; }
    public string? RomajiTitle { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public string? JlptLevel { get; set; }
    public List<ChapterJsonDto> Chapters { get; set; } = new();
}

public class ChapterJsonDto
{
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class NovelImportResult
{
    public bool IsSuccess { get; set; }
    public Novel? ImportedNovel { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();

    public static NovelImportResult Success(Novel novel) => new()
    {
        IsSuccess = true,
        ImportedNovel = novel
    };

    public static NovelImportResult Failure(string errorMessage, IEnumerable<string>? validationErrors = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        ValidationErrors = validationErrors?.ToList() ?? new List<string>()
    };
}

public class ReaderPreferences
{
    public double FontSize { get; set; } = 18.0;
    public double LineSpacing { get; set; } = 30.0;
    public string FontFamily { get; set; } = "Yu Gothic";
    public string ThemeKey { get; set; } = "Cosmic"; // Cosmic, DeepNight, WarmSepia, Amethyst
}
