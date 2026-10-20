using KotobaOdyssey.Data.Services;

namespace KotobaOdyssey.Tests;

public class JapaneseTextAnalyzerTests
{
    private readonly JapaneseTextAnalyzer _analyzer;

    public JapaneseTextAnalyzerTests()
    {
        var dict = new JapaneseDictionaryService();
        _analyzer = new JapaneseTextAnalyzer(dict);
    }

    [Fact]
    public async Task AnalyzeTextAsync_SentenceWithoutSpaces_CorrectlySegmentsTokens()
    {
        string sentence = "銀河鉄道の夜。先生は黒板に星座を描いた。";

        var tokens = await _analyzer.AnalyzeTextAsync(sentence);

        Assert.NotEmpty(tokens);

        // Verify key tokens are detected
        Assert.Contains(tokens, t => t.Surface == "銀河" && t.HasDictionaryEntry);
        Assert.Contains(tokens, t => t.Surface == "鉄道" && t.HasDictionaryEntry);
        Assert.Contains(tokens, t => t.Surface == "夜" && t.HasDictionaryEntry);
        Assert.Contains(tokens, t => t.Surface == "先生" && t.HasDictionaryEntry);
        Assert.Contains(tokens, t => t.Surface == "黒板" && t.HasDictionaryEntry);
        Assert.Contains(tokens, t => t.Surface == "星座" && t.HasDictionaryEntry);
    }

    [Fact]
    public async Task AnalyzeTextAsync_EmptyOrWhitespace_ReturnsEmptyTokens()
    {
        var empty = await _analyzer.AnalyzeTextAsync("");
        var whitespace = await _analyzer.AnalyzeTextAsync("    ");

        Assert.Empty(empty);
    }
}
