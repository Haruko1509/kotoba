using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;

namespace KotobaOdyssey.Data.Services;

public class JapaneseTextAnalyzer : IJapaneseTextAnalyzer
{
    private readonly IJapaneseDictionaryService _dictionaryService;

    public JapaneseTextAnalyzer(IJapaneseDictionaryService dictionaryService)
    {
        _dictionaryService = dictionaryService;
    }

    public async Task<IReadOnlyList<TextToken>> AnalyzeTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<TextToken>();
        }

        var tokens = new List<TextToken>();
        int i = 0;
        int len = text.Length;

        while (i < len)
        {
            char c = text[i];

            // 1. Skip or record whitespace / punctuation
            if (char.IsWhiteSpace(c) || IsPunctuation(c))
            {
                int start = i;
                while (i < len && (char.IsWhiteSpace(text[i]) || IsPunctuation(text[i])))
                {
                    i++;
                }

                tokens.Add(new TextToken
                {
                    Surface = text.Substring(start, i - start),
                    PartOfSpeech = "Ký tự đặc biệt",
                    StartIndex = start,
                    Length = i - start,
                    HasDictionaryEntry = false
                });
                continue;
            }

            // 2. Try Longest Prefix Match against Dictionary (from max len 8 down to 1)
            bool matched = false;
            int maxSearchLen = Math.Min(8, len - i);

            for (int searchLen = maxSearchLen; searchLen >= 1; searchLen--)
            {
                string candidate = text.Substring(i, searchLen);
                var entry = await _dictionaryService.LookupAsync(candidate);

                if (entry != null)
                {
                    tokens.Add(new TextToken
                    {
                        Surface = candidate,
                        Reading = entry.Reading,
                        BaseForm = entry.Term,
                        PartOfSpeech = entry.PartOfSpeech,
                        JlptLevel = entry.JlptLevel,
                        StartIndex = i,
                        Length = searchLen,
                        HasDictionaryEntry = true
                    });

                    i += searchLen;
                    matched = true;
                    break;
                }
            }

            // 3. If no dictionary entry matched, consume continuous block of same script type
            if (!matched)
            {
                int start = i;
                var scriptType = GetCharScript(text[i]);

                while (i < len && GetCharScript(text[i]) == scriptType && !IsPunctuation(text[i]) && !char.IsWhiteSpace(text[i]))
                {
                    i++;
                }

                string unknownSurface = text.Substring(start, i - start);
                tokens.Add(new TextToken
                {
                    Surface = unknownSurface,
                    PartOfSpeech = scriptType switch
                    {
                        ScriptType.Kanji => "Hán tự (Chưa tra cứu)",
                        ScriptType.Katakana => "Từ mượn Katakana",
                        ScriptType.Hiragana => "Trợ từ / Đuôi từ",
                        _ => "Từ chưa rõ"
                    },
                    StartIndex = start,
                    Length = i - start,
                    HasDictionaryEntry = false
                });
            }
        }

        return tokens;
    }

    private static bool IsPunctuation(char c)
    {
        return char.IsPunctuation(c) ||
               c == '、' || c == '。' || c == '「' || c == '」' ||
               c == '『' || c == '』' || c == '・' || c == '…' ||
               c == '！' || c == '？' || c == 'ー';
    }

    private static ScriptType GetCharScript(char c)
    {
        if (c >= '\u4e00' && c <= '\u9faf') return ScriptType.Kanji;
        if (c >= '\u3040' && c <= '\u309f') return ScriptType.Hiragana;
        if (c >= '\u30a0' && c <= '\u30ff') return ScriptType.Katakana;
        if (char.IsAsciiLetterOrDigit(c)) return ScriptType.Ascii;
        return ScriptType.Other;
    }

    private enum ScriptType
    {
        Kanji,
        Hiragana,
        Katakana,
        Ascii,
        Other
    }
}
