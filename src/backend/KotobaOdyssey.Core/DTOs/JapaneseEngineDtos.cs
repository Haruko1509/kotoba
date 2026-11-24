using KotobaOdyssey.Core.Enums;

namespace KotobaOdyssey.Core.DTOs;

public class DictionaryEntry
{
    public string Term { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
    public JlptLevel JlptLevel { get; set; } = JlptLevel.None;
    public string ExampleSentence { get; set; } = string.Empty;
    public string ExampleMeaning { get; set; } = string.Empty;
    public bool IsKnown { get; set; } = true;
    public string? OriginalSurface { get; set; }
    public string? InflectionNote { get; set; }
}

public class TextToken
{
    public string Surface { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string BaseForm { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
    public JlptLevel JlptLevel { get; set; } = JlptLevel.None;
    public int StartIndex { get; set; }
    public int Length { get; set; }
    public bool HasDictionaryEntry { get; set; }
}
