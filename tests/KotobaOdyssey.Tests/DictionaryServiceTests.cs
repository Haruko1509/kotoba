using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Data.Services;

namespace KotobaOdyssey.Tests;

public class DictionaryServiceTests
{
    private readonly JapaneseDictionaryService _dictionaryService = new();

    [Fact]
    public async Task LookupAsync_KnownTerm_ReturnsAccurateEntry()
    {
        var entry = await _dictionaryService.LookupAsync("銀河");

        Assert.NotNull(entry);
        Assert.Equal("銀河", entry.Term);
        Assert.Equal("ぎんが", entry.Reading);
        Assert.Contains("Ngân hà", entry.Meaning);
        Assert.Equal(JlptLevel.N3, entry.JlptLevel);
        Assert.False(string.IsNullOrWhiteSpace(entry.ExampleSentence));
    }

    [Fact]
    public async Task LookupAsync_UnknownTerm_ReturnsNullWithoutHallucinating()
    {
        // An imaginary or non-existent word
        var entry = await _dictionaryService.LookupAsync("xyz_unknown_nonexistent");

        Assert.Null(entry);
    }

    [Fact]
    public async Task LookupAsync_InflectedVerb_SuccessfullyDeinflectsAndFindsBaseForm()
    {
        // "読みました" -> "読む"
        var entry = await _dictionaryService.LookupAsync("読みました");

        Assert.NotNull(entry);
        Assert.Equal("読む", entry.Term);
        Assert.Contains("Đọc", entry.Meaning);
    }

    [Fact]
    public async Task SearchSuggestionsAsync_ReturnsMatchingEntries()
    {
        var suggestions = await _dictionaryService.SearchSuggestionsAsync("星", limit: 5);

        Assert.NotEmpty(suggestions);
        Assert.All(suggestions, s => Assert.True(s.Term.Contains("星") || s.Reading.Contains("ほし")));
    }
}
