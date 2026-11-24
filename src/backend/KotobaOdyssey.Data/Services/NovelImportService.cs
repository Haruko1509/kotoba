using System.Net;
using System.Net.Http;
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
    private readonly HttpClient? _httpClient;

    public NovelImportService(INovelRepository novelRepository, ILogger<NovelImportService> logger, HttpClient? httpClient = null)
    {
        _novelRepository = novelRepository;
        _logger = logger;
        _httpClient = httpClient;
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

    public async Task<NovelImportResult> ImportFromUrlAsync(string url, JlptLevel jlptLevel, string? genre, string? customTitle = null)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
        {
            return NovelImportResult.Failure("Đường dẫn URL không hợp lệ.");
        }

        try
        {
            using var client = _httpClient != null ? new HttpClient() : new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 KotobaOdyssey/2.0");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "ja,en-US;q=0.9,en;q=0.8");

            var html = await client.GetStringAsync(uri);
            if (string.IsNullOrWhiteSpace(html))
            {
                return NovelImportResult.Failure("Không nhận được nội dung phản hồi từ liên kết web.");
            }

            string title = customTitle?.Trim() ?? string.Empty;
            string author = "Tác giả mạng";
            string chapterTitle = "第一章";
            string bodyContent = string.Empty;

            var host = uri.Host.ToLowerInvariant();

            // 1. Syosetu (Shousetsuka ni Narou)
            if (host.Contains("syosetu.com"))
            {
                // Title from meta or title tag
                var titleTagMatch = Regex.Match(html, @"<title>(.*?)</title>", RegexOptions.IgnoreCase);
                if (titleTagMatch.Success)
                {
                    var rawTitle = StripHtml(titleTagMatch.Groups[1].Value);
                    // Syosetu format: "Novel Title - Episode Title"
                    var titleParts = rawTitle.Split(new[] { " - ", " – ", " — " }, StringSplitOptions.RemoveEmptyEntries);
                    if (titleParts.Length > 1)
                    {
                        if (string.IsNullOrWhiteSpace(title)) title = titleParts[0].Trim();
                        chapterTitle = titleParts[^1].Trim();
                    }
                    else if (string.IsNullOrWhiteSpace(title))
                    {
                        title = rawTitle.Trim();
                    }
                }

                var titleMatch = Regex.Match(html, @"<p\s+class=""novel_title""[^>]*>(.*?)</p>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (titleMatch.Success)
                {
                    title = StripHtml(titleMatch.Groups[1].Value);
                }

                var authorMatch = Regex.Match(html, @"<(?:div|span)\s+class=""[^""]*(?:novel_writername|p-novel__author)[^""]*""[^>]*>(?:作者：)?(.*?)<\/(?:div|span)>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (authorMatch.Success)
                {
                    author = StripHtml(authorMatch.Groups[1].Value);
                }

                var subTitleMatch = Regex.Match(html, @"<p\s+class=""(?:novel_subtitle|p-novel__title)""[^>]*>(.*?)</p>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (subTitleMatch.Success)
                {
                    chapterTitle = StripHtml(subTitleMatch.Groups[1].Value);
                }

                // Check modern Syosetu (.p-novel__body, .js-novel-text) & legacy (#novel_honbun)
                var honbunMatch = Regex.Match(html, @"<div\s+[^>]*class=""[^""]*(?:p-novel__body|js-novel-text|p-novel__text)[^""]*""[^>]*>(.*?)<\/div>\s*<\/div>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (!honbunMatch.Success)
                {
                    honbunMatch = Regex.Match(html, @"<div\s+id=""novel_honbun""[^>]*>(.*?)<\/div>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                }

                if (honbunMatch.Success)
                {
                    bodyContent = CleanNovelHtml(honbunMatch.Groups[1].Value);
                }
                else
                {
                    // Fallback to extracting all <p id="L..."> lines
                    var lineMatches = Regex.Matches(html, @"<p\s+id=""L\d+""[^>]*>(.*?)<\/p>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (lineMatches.Count > 0)
                    {
                        var lines = lineMatches.Select(m => CleanNovelHtml(m.Groups[1].Value)).Where(l => !string.IsNullOrWhiteSpace(l));
                        bodyContent = string.Join("\n", lines);
                    }
                }
            }
            // 2. Kakuyomu
            else if (host.Contains("kakuyomu.jp"))
            {
                var titleMatch = Regex.Match(html, @"<h1\s+id=""workTitle""[^>]*>(.*?)</h1>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (titleMatch.Success && string.IsNullOrWhiteSpace(title))
                {
                    title = StripHtml(titleMatch.Groups[1].Value);
                }

                var authorMatch = Regex.Match(html, @"<span\s+id=""workAuthor-activityName""[^>]*>(.*?)</span>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (authorMatch.Success)
                {
                    author = StripHtml(authorMatch.Groups[1].Value);
                }

                var epTitleMatch = Regex.Match(html, @"<p\s+class=""widget-episodeTitle[^""]*""[^>]*>(.*?)</p>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (epTitleMatch.Success)
                {
                    chapterTitle = StripHtml(epTitleMatch.Groups[1].Value);
                }

                var bodyMatch = Regex.Match(html, @"<div\s+class=""widget-episodeBody[^""]*""[^>]*>(.*?)</div>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (bodyMatch.Success)
                {
                    bodyContent = CleanNovelHtml(bodyMatch.Groups[1].Value);
                }
            }
            // 3. Generic Web Page
            else
            {
                var titleMatch = Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (titleMatch.Success && string.IsNullOrWhiteSpace(title))
                {
                    title = StripHtml(titleMatch.Groups[1].Value);
                }

                // Extract Japanese <p> paragraphs
                var paraMatches = Regex.Matches(html, @"<p[^>]*>(.*?)</p>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var jParas = new List<string>();
                foreach (Match p in paraMatches)
                {
                    var text = StripHtml(p.Groups[1].Value);
                    if (Regex.IsMatch(text, @"[\u3040-\u30ff\u4e00-\u9faf]") && text.Length > 8)
                    {
                        jParas.Add(text);
                    }
                }
                bodyContent = string.Join("\n", jParas);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = $"Web Novel ({uri.Host})";
            }

            if (string.IsNullOrWhiteSpace(bodyContent))
            {
                return NovelImportResult.Failure("Không tìm thấy nội dung văn bản tiếng Nhật từ URL được cung cấp. Vui lòng kiểm tra lại liên kết hoặc dán văn bản trực tiếp.");
            }

            // Assign smart cover based on genre
            string coverPath = PickCoverForGenre(genre);

            var novel = new Novel
            {
                Title = title.Trim(),
                JapaneseTitle = title.Trim(),
                RomajiTitle = title.Trim(),
                Author = string.IsNullOrWhiteSpace(author) ? "Tác giả mạng" : author.Trim(),
                Description = $"Tác phẩm cào trực tiếp từ: {uri.AbsoluteUri}",
                Genre = string.IsNullOrWhiteSpace(genre) ? "General" : genre.Trim(),
                JlptLevel = jlptLevel,
                CoverImagePath = coverPath,
                IsDemoData = false,
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };

            novel.Chapters.Add(new Chapter
            {
                ChapterNumber = 1,
                Title = string.IsNullOrWhiteSpace(chapterTitle) ? "第一章" : chapterTitle.Trim(),
                Content = bodyContent.Trim(),
                WordCount = CountJapaneseCharacters(bodyContent),
                IsCompleted = false,
                LastReadPosition = 0
            });

            var saved = await _novelRepository.AddAsync(novel);
            var firstChapterWords = saved.Chapters?.FirstOrDefault()?.WordCount ?? 0;
            _logger.LogInformation("Successfully imported novel '{Title}' from URL '{Url}' with 1 chapter ({Words} chars).", saved.Title, url, firstChapterWords);
            return NovelImportResult.Success(saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import novel from URL '{Url}'", url);
            return NovelImportResult.Failure($"Lỗi khi cào dữ liệu từ URL: {ex.Message}");
        }
    }

    private static string CleanNovelHtml(string rawHtml)
    {
        if (string.IsNullOrWhiteSpace(rawHtml)) return string.Empty;

        // Clean ruby: <ruby>漢<rt>かん</rt></ruby> -> 漢
        var clean = Regex.Replace(rawHtml, @"<ruby>(.*?)<rp>.*?</rp><rt>.*?</rt><rp>.*?</rp></ruby>", "$1", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        clean = Regex.Replace(clean, @"<ruby>(.*?)<rt>.*?</rt></ruby>", "$1", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Convert <br> and </p> to newline
        clean = Regex.Replace(clean, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        clean = Regex.Replace(clean, @"</p>", "\n", RegexOptions.IgnoreCase);

        // Strip tags
        clean = Regex.Replace(clean, @"<[^>]+>", string.Empty);

        // WebUtility decode
        clean = WebUtility.HtmlDecode(clean);

        // Normalize multiple empty lines
        clean = Regex.Replace(clean, @"\n{3,}", "\n\n");
        return clean.Trim();
    }

    private static string StripHtml(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var s = Regex.Replace(text, @"<[^>]+>", string.Empty);
        return WebUtility.HtmlDecode(s).Trim();
    }

    private static string PickCoverForGenre(string? genre)
    {
        var g = genre?.ToLowerInvariant() ?? string.Empty;
        if (g.Contains("isekai") || g.Contains("chuyển sinh") || g.Contains("fantasy") || g.Contains("dị giới"))
            return "assets/covers/cover_anime_forest.jpg";
        if (g.Contains("romcom") || g.Contains("school") || g.Contains("học đường") || g.Contains("lãng mạn"))
            return "assets/covers/cover_school_romcom.jpg";
        if (g.Contains("reijou") || g.Contains("phản diện") || g.Contains("hoàng gia"))
            return "assets/covers/yamiochi_last_boss_reijou_no_osananajimi_ni_tensei_shita.jpg";
        if (g.Contains("action") || g.Contains("dungeon") || g.Contains("hầm ngục") || g.Contains("hành động"))
            return "assets/covers/cover_dungeon_adventure.jpg";
        if (g.Contains("sci-fi") || g.Contains("viễn tưởng"))
            return "assets/covers/cover_anime_stars.jpg";
        if (g.Contains("slice of life") || g.Contains("đời thường") || g.Contains("chữa lành"))
            return "assets/covers/cover_slow_life.jpg";
        return "assets/covers/cover_anime_cafe.jpg";
    }

    private static int CountJapaneseCharacters(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        // Count non-whitespace characters
        return text.Count(c => !char.IsWhiteSpace(c));
    }
}
