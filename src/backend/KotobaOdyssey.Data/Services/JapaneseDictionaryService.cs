using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;

namespace KotobaOdyssey.Data.Services;

public class JapaneseDictionaryService : IJapaneseDictionaryService
{
    private readonly Dictionary<string, DictionaryEntry> _dictionary = new(StringComparer.OrdinalIgnoreCase);

    public JapaneseDictionaryService()
    {
        InitializeDictionary();
    }

    public Task<DictionaryEntry?> LookupAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Task.FromResult<DictionaryEntry?>(null);
        }

        string cleanTerm = term.Trim();

        // 1. Direct exact match
        if (_dictionary.TryGetValue(cleanTerm, out var entry))
        {
            return Task.FromResult<DictionaryEntry?>(entry);
        }

        // 2. Common verb/adjective de-inflection heuristic
        foreach (var (candidate, note) in GetDeinflectionCandidatesWithNotes(cleanTerm))
        {
            if (_dictionary.TryGetValue(candidate, out var deinflectedEntry))
            {
                var result = new DictionaryEntry
                {
                    Term = deinflectedEntry.Term,
                    Reading = deinflectedEntry.Reading,
                    Meaning = deinflectedEntry.Meaning,
                    PartOfSpeech = deinflectedEntry.PartOfSpeech,
                    JlptLevel = deinflectedEntry.JlptLevel,
                    ExampleSentence = deinflectedEntry.ExampleSentence,
                    ExampleMeaning = deinflectedEntry.ExampleMeaning,
                    IsKnown = deinflectedEntry.IsKnown,
                    OriginalSurface = cleanTerm,
                    InflectionNote = note ?? $"[Dạng biến thể của từ gốc '{deinflectedEntry.Term}']"
                };
                return Task.FromResult<DictionaryEntry?>(result);
            }
        }

        // 3. Not found -> return null (Do not make up fake definitions)
        return Task.FromResult<DictionaryEntry?>(null);
    }

    public Task<IReadOnlyList<DictionaryEntry>> SearchSuggestionsAsync(string prefix, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return Task.FromResult<IReadOnlyList<DictionaryEntry>>(Array.Empty<DictionaryEntry>());
        }

        string p = prefix.Trim();
        var results = _dictionary.Values
            .Where(e => e.Term.Contains(p, StringComparison.OrdinalIgnoreCase) ||
                        e.Reading.Contains(p, StringComparison.OrdinalIgnoreCase))
            .DistinctBy(e => e.Term)
            .Take(limit)
            .ToList();

        return Task.FromResult<IReadOnlyList<DictionaryEntry>>(results);
    }

    public async Task<IReadOnlyList<DictionaryEntry>> LookupBatchAsync(IEnumerable<string> terms)
    {
        var results = new List<DictionaryEntry>();
        foreach (var term in terms)
        {
            var entry = await LookupAsync(term);
            if (entry != null)
            {
                results.Add(entry);
            }
        }
        return results;
    }

    private IEnumerable<(string Candidate, string? Note)> GetDeinflectionCandidatesWithNotes(string surface)
    {
        var candidates = new List<(string, string?)>();

        // 1. Polite past: ~ました
        if (surface.EndsWith("ました") && surface.Length > 3)
        {
            string stem = surface.Substring(0, surface.Length - 3);
            candidates.Add((stem + "る", "Thể lịch sự quá khứ (~ました)"));
            if (stem.EndsWith("い"))
            {
                candidates.Add((stem.Substring(0, stem.Length - 1) + "る", "Thể lịch sự quá khứ"));
                candidates.Add((stem.Substring(0, stem.Length - 1) + "う", "Thể lịch sự quá khứ"));
            }
            if (stem.EndsWith("き")) candidates.Add((stem.Substring(0, stem.Length - 1) + "く", "Thể lịch sự quá khứ"));
            if (stem.EndsWith("ぎ")) candidates.Add((stem.Substring(0, stem.Length - 1) + "ぐ", "Thể lịch sự quá khứ"));
            if (stem.EndsWith("し")) candidates.Add((stem.Substring(0, stem.Length - 1) + "す", "Thể lịch sự quá khứ"));
            if (stem.EndsWith("ち")) candidates.Add((stem.Substring(0, stem.Length - 1) + "つ", "Thể lịch sự quá khứ"));
            if (stem.EndsWith("び")) candidates.Add((stem.Substring(0, stem.Length - 1) + "ぶ", "Thể lịch sự quá khứ"));
            if (stem.EndsWith("み")) candidates.Add((stem.Substring(0, stem.Length - 1) + "む", "Thể lịch sự quá khứ"));
            if (stem.EndsWith("り")) candidates.Add((stem.Substring(0, stem.Length - 1) + "る", "Thể lịch sự quá khứ"));
        }

        // 2. Polite non-past: ~ます
        if (surface.EndsWith("ます") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add((stem + "る", "Thể lịch sự (~ます)"));
            if (stem.EndsWith("い")) candidates.Add((stem.Substring(0, stem.Length - 1) + "う", "Thể lịch sự (~ます)"));
            if (stem.EndsWith("き")) candidates.Add((stem.Substring(0, stem.Length - 1) + "く", "Thể lịch sự (~ます)"));
            if (stem.EndsWith("し")) candidates.Add((stem.Substring(0, stem.Length - 1) + "す", "Thể lịch sự (~ます)"));
            if (stem.EndsWith("ち")) candidates.Add((stem.Substring(0, stem.Length - 1) + "つ", "Thể lịch sự (~ます)"));
            if (stem.EndsWith("み")) candidates.Add((stem.Substring(0, stem.Length - 1) + "む", "Thể lịch sự (~ます)"));
            if (stem.EndsWith("り")) candidates.Add((stem.Substring(0, stem.Length - 1) + "る", "Thể lịch sự (~ます)"));
        }

        // 3. Concurrent action: ~ながら
        if (surface.EndsWith("ながら") && surface.Length > 3)
        {
            string stem = surface.Substring(0, surface.Length - 3);
            candidates.Add((stem + "る", "Dạng làm đồng thời (~ながら)"));
            if (stem.EndsWith("い")) candidates.Add((stem.Substring(0, stem.Length - 1) + "う", "Dạng làm đồng thời (~ながら)"));
            if (stem.EndsWith("き")) candidates.Add((stem.Substring(0, stem.Length - 1) + "く", "Dạng làm đồng thời (~ながら)"));
            if (stem.EndsWith("し")) candidates.Add((stem.Substring(0, stem.Length - 1) + "す", "Dạng làm đồng thời (~ながら)"));
            if (stem.EndsWith("ち")) candidates.Add((stem.Substring(0, stem.Length - 1) + "つ", "Dạng làm đồng thời (~ながら)"));
            if (stem.EndsWith("り")) candidates.Add((stem.Substring(0, stem.Length - 1) + "る", "Dạng làm đồng thời (~ながら)"));
        }

        // 4. Past plain / Conditional: ~いた / ~った / ~んだ / ~した
        if (surface.EndsWith("いた") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "く", "Thể quá khứ (~いた)"));
        }
        if (surface.EndsWith("いだ") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "ぐ", "Thể quá khứ (~いだ)"));
        }
        if (surface.EndsWith("った") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add((stem + "う", "Thể quá khứ (~った)"));
            candidates.Add((stem + "つ", "Thể quá khứ (~った)"));
            candidates.Add((stem + "る", "Thể quá khứ (~った)"));
        }
        if (surface.EndsWith("んだ") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add((stem + "む", "Thể quá khứ (~んだ)"));
            candidates.Add((stem + "ぶ", "Thể quá khứ (~んだ)"));
            candidates.Add((stem + "ぬ", "Thể quá khứ (~んだ)"));
        }
        if (surface.EndsWith("した") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "す", "Thể quá khứ (~した)"));
            candidates.Add((surface.Substring(0, surface.Length - 2) + "る", "Thể quá khứ (~した)"));
        }
        if (surface.EndsWith("た") && surface.Length > 1)
        {
            candidates.Add((surface.Substring(0, surface.Length - 1) + "る", "Thể quá khứ (~た)"));
        }

        // 5. Te-form / Progressive: ~ている / ~ていた / ~てきて / ~て / ~で
        if (surface.EndsWith("ている") && surface.Length > 3)
        {
            string stem = surface.Substring(0, surface.Length - 3);
            if (stem.EndsWith("っ")) candidates.Add((stem.Substring(0, stem.Length - 1) + "る", "Thể tiếp diễn (~ている)"));
            candidates.Add((stem + "る", "Thể tiếp diễn (~ている)"));
        }
        if (surface.EndsWith("ていた") && surface.Length > 3)
        {
            string stem = surface.Substring(0, surface.Length - 3);
            if (stem.EndsWith("っ")) candidates.Add((stem.Substring(0, stem.Length - 1) + "つ", "Thể tiếp diễn quá khứ (~ていた)"));
            if (stem.EndsWith("っ")) candidates.Add((stem.Substring(0, stem.Length - 1) + "る", "Thể tiếp diễn quá khứ (~ていた)"));
            if (stem.EndsWith("っ")) candidates.Add((stem.Substring(0, stem.Length - 1) + "う", "Thể tiếp diễn quá khứ (~ていた)"));
            candidates.Add((stem + "る", "Thể tiếp diễn quá khứ (~ていた)"));
        }
        if (surface.EndsWith("てきて") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "る", "Dạng ~てくる"));
        }
        if (surface.EndsWith("いて") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "く", "Thể Te (~いて)"));
        }
        if (surface.EndsWith("いで") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "ぐ", "Thể Te (~いで)"));
        }
        if (surface.EndsWith("って") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add((stem + "う", "Thể Te (~って)"));
            candidates.Add((stem + "つ", "Thể Te (~って)"));
            candidates.Add((stem + "る", "Thể Te (~って)"));
        }
        if (surface.EndsWith("んで") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add((stem + "む", "Thể Te (~んで)"));
            candidates.Add((stem + "ぶ", "Thể Te (~んで)"));
        }
        if (surface == "して")
        {
            candidates.Add(("する", "Thể Te của する"));
        }
        else if (surface.EndsWith("して") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "す", "Thể Te (~して)"));
        }
        if (surface.EndsWith("て") && surface.Length > 1)
        {
            candidates.Add((surface.Substring(0, surface.Length - 1) + "る", "Thể Te (~て)"));
        }

        // 6. Passive / Potential / Causative: ~られる / ~れる / ~せる / ~させる
        if (surface.EndsWith("られる") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "る", "Thể bị động / khả năng (~られる)"));
        }
        if (surface.EndsWith("られた") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "る", "Thể bị động quá khứ (~られた)"));
        }
        if (surface.EndsWith("かれる") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "く", "Thể bị động (~かれる)"));
        }
        if (surface.EndsWith("がれる") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "ぐ", "Thể bị động (~がれる)"));
        }
        if (surface.EndsWith("まれる") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "む", "Thể bị động (~まれる)"));
        }
        if (surface.EndsWith("たれる") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "つ", "Thể bị động (~たれる)"));
        }
        if (surface.EndsWith("われる") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "う", "Thể bị động (~われる)"));
        }
        // Potential (Godan): ~える row + る
        if (surface.EndsWith("める") && surface.Length > 2) candidates.Add((surface.Substring(0, surface.Length - 2) + "む", "Thể khả năng (~める)"));
        if (surface.EndsWith("ける") && surface.Length > 2) candidates.Add((surface.Substring(0, surface.Length - 2) + "く", "Thể khả năng (~ける)"));
        if (surface.EndsWith("げる") && surface.Length > 2) candidates.Add((surface.Substring(0, surface.Length - 2) + "ぐ", "Thể khả năng (~げる)"));
        if (surface.EndsWith("べる") && surface.Length > 2) candidates.Add((surface.Substring(0, surface.Length - 2) + "ぶ", "Thể khả năng (~べる)"));
        if (surface.EndsWith("てる") && surface.Length > 2) candidates.Add((surface.Substring(0, surface.Length - 2) + "つ", "Thể khả năng (~てる)"));
        if (surface.EndsWith("せる") && surface.Length > 2) candidates.Add((surface.Substring(0, surface.Length - 2) + "す", "Thể khả năng / Sai khiến (~せる)"));
        if (surface.EndsWith("れる") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add((stem + "る", "Thể bị động / Khả năng (~れる)"));
            candidates.Add((stem + "す", "Thể bị động (~れる)"));
        }
        if (surface.EndsWith("させる") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "る", "Thể sai khiến (~させる)"));
        }
        if (surface.EndsWith("せる") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add((stem + "く", "Thể sai khiến (~せる)"));
            candidates.Add((stem + "す", "Thể sai khiến (~せる)"));
        }

        // 7. Negative: ~ない / ~なかった / ~ず / ~ぬ
        if (surface.EndsWith("なかった") && surface.Length > 4)
        {
            string stem = surface.Substring(0, surface.Length - 4);
            candidates.Add((stem + "る", "Thể phủ định quá khứ (~なかった)"));
            if (stem.EndsWith("わ")) candidates.Add((stem.Substring(0, stem.Length - 1) + "う", "Thể phủ định quá khứ"));
            if (stem.EndsWith("か")) candidates.Add((stem.Substring(0, stem.Length - 1) + "く", "Thể phủ định quá khứ"));
            if (stem.EndsWith("さ")) candidates.Add((stem.Substring(0, stem.Length - 1) + "す", "Thể phủ định quá khứ"));
            if (stem.EndsWith("た")) candidates.Add((stem.Substring(0, stem.Length - 1) + "つ", "Thể phủ định quá khứ"));
            if (stem.EndsWith("ま")) candidates.Add((stem.Substring(0, stem.Length - 1) + "む", "Thể phủ định quá khứ"));
            if (stem.EndsWith("ら")) candidates.Add((stem.Substring(0, stem.Length - 1) + "る", "Thể phủ định quá khứ"));
        }
        if (surface.EndsWith("ない") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add((stem + "る", "Thể phủ định (~ない)"));
            if (stem.EndsWith("わ")) candidates.Add((stem.Substring(0, stem.Length - 1) + "う", "Thể phủ định (~ない)"));
            if (stem.EndsWith("か")) candidates.Add((stem.Substring(0, stem.Length - 1) + "く", "Thể phủ định (~ない)"));
            if (stem.EndsWith("さ")) candidates.Add((stem.Substring(0, stem.Length - 1) + "す", "Thể phủ định (~ない)"));
            if (stem.EndsWith("た")) candidates.Add((stem.Substring(0, stem.Length - 1) + "つ", "Thể phủ định (~ない)"));
            if (stem.EndsWith("ま")) candidates.Add((stem.Substring(0, stem.Length - 1) + "む", "Thể phủ định (~ない)"));
            if (stem.EndsWith("ら")) candidates.Add((stem.Substring(0, stem.Length - 1) + "る", "Thể phủ định (~ない)"));
        }
        if (surface.EndsWith("ぬ") && surface.Length > 1)
        {
            string stem = surface.Substring(0, surface.Length - 1);
            if (stem.EndsWith("ら")) candidates.Add((stem.Substring(0, stem.Length - 1) + "る", "Thể phủ định cổ (~ぬ)"));
        }

        // 8. Volitional: ~よう / ~ろう / ~こう / ~そう / ~とう / ~ぼう
        if (surface.EndsWith("よう") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "る", "Thể ý chí / rủ rê (~よう)"));
        }
        if (surface.EndsWith("ろう") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "る", "Thể ý chí (~ろう)"));
        }
        if (surface.EndsWith("こう") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "く", "Thể ý chí (~こう)"));
        }

        // 9. Imperative & Conditional: ~たら / ~だら / ~ば / ~と
        if (surface.EndsWith("んだら") && surface.Length > 3)
        {
            string stem = surface.Substring(0, surface.Length - 3);
            candidates.Add((stem + "む", "Thể điều kiện たら (~んだら)"));
            candidates.Add((stem + "ぶ", "Thể điều kiện たら (~んだら)"));
        }
        if (surface.EndsWith("ったら") && surface.Length > 3)
        {
            string stem = surface.Substring(0, surface.Length - 3);
            candidates.Add((stem + "る", "Thể điều kiện たら (~ったら)"));
            candidates.Add((stem + "つ", "Thể điều kiện たら (~ったら)"));
            candidates.Add((stem + "う", "Thể điều kiện たら (~ったら)"));
        }
        if (surface.EndsWith("いたら") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "く", "Thể điều kiện たら (~いたら)"));
        }
        if (surface.EndsWith("たら") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "る", "Thể điều kiện たら (~たら)"));
        }
        if (surface.EndsWith("えば") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "う", "Thể điều kiện ば (~えば)"));
        }
        if (surface.EndsWith("けば") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "く", "Thể điều kiện ば (~けば)"));
        }
        if (surface.EndsWith("めば") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "む", "Thể điều kiện ば (~めば)"));
        }
        if (surface.EndsWith("れば") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "る", "Thể điều kiện ば (~れば)"));
        }
        if (surface.EndsWith("と") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 1), "Thể điều kiện と"));
        }

        // 10. Adjective adverbial / past / te-form: ~く / ~かった / ~くて
        if (surface.EndsWith("く") && surface.Length > 1)
        {
            candidates.Add((surface.Substring(0, surface.Length - 1) + "い", "Dạng phó từ của tính từ đuôi い"));
        }
        if (surface.EndsWith("かった") && surface.Length > 3)
        {
            candidates.Add((surface.Substring(0, surface.Length - 3) + "い", "Dạng quá khứ của tính từ đuôi い"));
        }
        if (surface.EndsWith("くて") && surface.Length > 2)
        {
            candidates.Add((surface.Substring(0, surface.Length - 2) + "い", "Dạng nối (Te) của tính từ đuôi い"));
        }

        return candidates;
    }

    private void Add(string term, string reading, string meaning, string pos, JlptLevel level, string exSentence = "", string exMeaning = "")
    {
        var entry = new DictionaryEntry
        {
            Term = term,
            Reading = reading,
            Meaning = meaning,
            PartOfSpeech = pos,
            JlptLevel = level,
            ExampleSentence = exSentence,
            ExampleMeaning = exMeaning,
            IsKnown = true
        };

        _dictionary[term] = entry;
        if (!_dictionary.ContainsKey(reading))
        {
            _dictionary[reading] = entry;
        }
    }

    private void InitializeDictionary()
    {
        // Core words from Literature novels & seeded classics
        // Essential core verbs and adjectives for learning loop
        Add("読む", "よむ", "Đọc (sách, báo, chữ)", "Động từ nhóm 1", JlptLevel.N5, "本を読む。", "Đọc sách.");
        Add("書く", "かく", "Viết, soạn thảo, ghi chép", "Động từ nhóm 1", JlptLevel.N5, "手紙を書く。", "Viết thư.");
        Add("食べる", "たべる", "Ăn, dùng bữa", "Động từ nhóm 2", JlptLevel.N5, "ご飯を食べる。", "Ăn cơm.");
        Add("寒い", "さむい", "Lạnh lẽo (thời tiết)", "Tính từ đuôi i", JlptLevel.N5, "今日はとても寒い。", "Hôm nay trời rất lạnh.");
        Add("青い", "あおい", "Xanh da trời, xanh biếc", "Tính từ đuôi i", JlptLevel.N5, "青い空を見上げる。", "Ngước nhìn bầu trời xanh.");


        // --- Ginga Tetsudou no Yoru (Đêm trên đường sắt Ngân Hà) ---
        Add("銀河", "ぎんが", "Ngân hà, dải thiên hà (Milky Way)", "Danh từ", JlptLevel.N3, "銀河の帯のようなところを指しながら。", "Vừa chỉ vào dải mờ ảo tựa như dải ngân hà.");
        Add("鉄道", "てつどう", "Đường sắt, tuyến đường xe lửa", "Danh từ", JlptLevel.N3, "銀河鉄道の夜。", "Đêm trên chuyến tàu ngân hà.");
        Add("夜", "よる", "Đêm, ban đêm", "Danh từ", JlptLevel.N5, "夜でも昼でも、ちかくの村へ出てきて。", "Dù đêm hay ngày, nó đều mò ra ngôi làng gần đó.");
        Add("午後", "ごご", "Buổi chiều (PM)", "Danh từ", JlptLevel.N5, "午後の授業がはじまりました。", "Giờ học buổi chiều đã bắt đầu.");
        Add("授業", "じゅぎょう", "Tiết học, giờ học ở trường", "Danh từ", JlptLevel.N4, "午後の授業がはじまりました。", "Giờ học buổi chiều đã bắt đầu.");
        Add("始まる", "はじまる", "Bắt đầu, khởi đầu", "Động từ nhóm 1", JlptLevel.N4, "授業がはじまりました。", "Giờ học đã bắt đầu.");
        Add("先生", "せんせい", "Thầy giáo, cô giáo, bác sĩ", "Danh từ", JlptLevel.N5, "先生は黒板に大きな図を描きました。", "Thầy giáo đã vẽ một bức hình lớn lên bảng đen.");
        Add("黒板", "こくばん", "Bảng đen", "Danh từ", JlptLevel.N4, "黒板に吊した大きな黒い星座の図。", "Bản đồ các chòm sao màu đen treo trên bảng đen.");
        Add("吊す", "つるす", "Treo lên, mắc lên", "Động từ nhóm 1", JlptLevel.N2, "黒板に吊した大きな星座の図。", "Bản đồ chòm sao treo trên bảng.");
        Add("指す", "さす", "Chỉ tay, hướng về", "Động từ nhóm 1", JlptLevel.N3, "銀河の帯のようなところを指しながら。", "Vừa chỉ vào dải mờ ảo như ngân hà.");
        Add("星座", "せいざ", "Chòm sao, tinh tú", "Danh từ", JlptLevel.N3, "黒い星座の図。", "Bản đồ các chòm sao.");
        Add("図", "ず", "Bản đồ, bức vẽ, sơ đồ", "Danh từ", JlptLevel.N4, "星座の図を指しました。", "Chỉ vào bức vẽ chòm sao.");
        Add("星", "ほし", "Ngôi sao, tinh cầu", "Danh từ", JlptLevel.N5, "たしかにあれがみんな星だと読んだ。", "Chắc chắn từng đọc rằng tất cả đều là những vì sao.");
        Add("帯", "おび", "Dải, thắt lưng, vành đai", "Danh từ", JlptLevel.N2, "銀河の帯のようなところ。", "Nơi tựa như một dải Ngân Hà.");
        Add("上", "うえ", "Phía trên, bên trên", "Danh từ", JlptLevel.N5, "上から下へ白くけむった。", "Mờ trắng xóa từ trên xuống dưới.");
        Add("下", "した", "Phía dưới, bên dưới", "Danh từ", JlptLevel.N5, "羅生門の下で雨やみを待っていた。", "Chờ tạnh mưa dưới cổng La Sinh Môn.");
        Add("活版所", "かっぱんじょ", "Xưởng in kiểu chữ chì", "Danh từ", JlptLevel.N2, "ジョバンニはある大きな活版所に入って行きました。", "Giovanni bước vào một xưởng in lớn.");
        Add("学校", "がっこう", "Trường học", "Danh từ", JlptLevel.N5, "ジョバンニは学校の門を出ました。", "Giovanni bước ra khỏi cổng trường.");
        Add("門", "もん", "Cổng, cửa lớn", "Danh từ", JlptLevel.N3, "学校の門を出ました。", "Bước ra khỏi cổng trường.");
        Add("家", "いえ", "Nhà, gia đình", "Danh từ", JlptLevel.N5, "ジョバンニは家へ帰ってきました。", "Giovanni đã trở về nhà.");
        Add("母", "はは", "Mẹ (của mình)", "Danh từ", JlptLevel.N5, "お母さんはまだ起きていました。", "Mẹ vẫn còn thức.");
        Add("牛乳", "ぎゅうにゅう", "Sữa bò tươi", "Danh từ", JlptLevel.N5, "牛乳をとってきました。", "Em đã đi lấy sữa về rồi ạ.");
        Add("祭り", "まつり", "Lễ hội, hội hè", "Danh từ", JlptLevel.N4, "今夜はケンタウル祭の夜でした。", "Đêm nay là đêm lễ hội Centaurus.");
        Add("天気", "てんき", "Thời tiết, trời", "Danh từ", JlptLevel.N5, "天気輪の柱が見えました。", "Nhìn thấy cột trụ thời tiết.");
        Add("柱", "はしら", "Cột trụ, cây cột", "Danh từ", JlptLevel.N2, "天気輪の柱のところへ行きました。", "Đi đến chỗ cây cột thời tiết.");
        Add("ステーション", "すてーしょん", "Ga tàu, trạm xe", "Danh từ (Katakana)", JlptLevel.N5, "銀河ステーションという声が聞こえました。", "Nghe thấy tiếng gọi Ga Ngân Hà.");
        Add("切符", "きっぷ", "Vé tàu, vé xe", "Danh từ", JlptLevel.N5, "切符を拝見いたします。", "Xin phép cho tôi xem vé.");
        Add("車掌", "しゃしょう", "Người soát vé trên tàu", "Danh từ", JlptLevel.N2, "車掌が切符を調べに来ました。", "Người soát vé đến kiểm tra vé.");
        Add("天の川", "あまのがわ", "Dải Ngân Hà", "Danh từ", JlptLevel.N3, "天の川の岸を列車が走っていました。", "Đoàn tàu chạy dọc theo bờ sông Ngân Hà.");
        Add("列車", "れっしゃ", "Đoàn tàu hỏa", "Danh từ", JlptLevel.N4, "列車は静かに走っていました。", "Đoàn tàu chạy êm đềm.");
        Add("汽車", "きしゃ", "Tàu hỏa chạy hơi nước", "Danh từ", JlptLevel.N4, "汽車は星空を進んで行きます。", "Tàu hỏa tiến vào bầu trời đầy sao.");
        Add("本当", "ほんとう", "Thật sự, chân thật", "Danh từ / Tính từ Na", JlptLevel.N4, "本当の幸いは何だろう。", "Hạnh phúc thật sự là gì nhỉ.");
        Add("幸い", "さいわい", "Hạnh phúc, điều may mắn", "Danh từ / Tính từ Na", JlptLevel.N3, "みんなの本当の幸いのために。", "Vì hạnh phúc chân thật của mọi người.");
        Add("旅", "たび", "Chuyến đi, hành trình", "Danh từ", JlptLevel.N3, "ぼくたちの旅はどこまで続くのだろう。", "Chuyến hành trình của chúng mình sẽ tiếp tục đến đâu nhỉ.");

        // --- Hashire Melos (Chạy đi Melos) ---
        Add("走る", "はしる", "Chạy, di chuyển nhanh", "Động từ nhóm 1", JlptLevel.N5, "走れ、メロス！", "Chạy đi, Melos!");
        Add("親友", "しんゆう", "Bạn thân, tri kỷ", "Danh từ", JlptLevel.N3, "私には親友がいる。", "Ta có một người bạn thân.");
        Add("友", "とも", "Bạn bè, bạn hữu", "Danh từ", JlptLevel.N3, "友のために命をかける。", "Vì bạn mà liều cả tính mạng.");
        Add("王", "おう", "Vua, đức vua", "Danh từ", JlptLevel.N3, "ディオニス王は人を信じない。", "Vua Dionys không tin bất kỳ ai.");
        Add("人質", "ひとじち", "Con tin", "Danh từ", JlptLevel.N1, "彼を人質に残そう。", "Hãy để chàng lại làm con tin.");
        Add("命", "いのち", "Sinh mệnh, tính mạng", "Danh từ", JlptLevel.N3, "命を救うために走る。", "Chạy để cứu lấy sinh mạng.");
        Add("信じる", "しんじる", "Tin tưởng, tín nhiệm", "Động từ nhóm 2", JlptLevel.N3, "人を信じる心。", "Tấm lòng tin tưởng con người.");
        Add("約束", "やくそく", "Lời hứa, hẹn ước", "Danh từ", JlptLevel.N4, "約束を果たさなければならない。", "Phải thực hiện bằng được lời hứa.");
        Add("処刑", "しょけい", "Xử tử, thi hành án tử hình", "Danh từ", JlptLevel.N1, "日暮れまでに帰らねば処刑される。", "Nếu trước lúc hoàng hôn không về sẽ bị xử tử.");
        Add("日暮れ", "ひぐれ", "Lúc hoàng hôn, chập tối", "Danh từ", JlptLevel.N3, "日暮れまでに間に合わなければならない。", "Phải kịp trước lúc hoàng hôn.");
        Add("太陽", "たいよう", "Mặt trời, thái dương", "Danh từ", JlptLevel.N4, "太陽が沈みかけていた。", "Mặt trời đang dần lặn xuống.");
        Add("沈む", "しずむ", "Chìm xuống, lặn (mặt trời)", "Động từ nhóm 1", JlptLevel.N3, "まだ日は沈まない。", "Mặt trời vẫn chưa lặn.");
        Add("走れ", "はしれ", "Hãy chạy! (Thể mệnh lệnh)", "Động từ nhóm 1 (Mệnh lệnh)", JlptLevel.N5, "走れメロス。", "Chạy đi Melos.");
        Add("激怒", "げきど", "Tức giận tột cùng, phẫn nộ", "Danh từ", JlptLevel.N1, "メロスは激怒した。", "Melos vô cùng tức giận.");

        // --- Gon-gitsune (Chú cáo Gon) ---
        Add("狐", "きつね", "Con cáo", "Danh từ", JlptLevel.N2, "これは小さな狐のごんのお話です。", "Đây là câu chuyện về chú cáo nhỏ Gon.");
        Add("小狐", "こぎつね", "Cáo con, cáo nhỏ", "Danh từ", JlptLevel.N2, "小狐のごんは一人ぼっちでした。", "Chú cáo con Gon chỉ có một mình cô đơn.");
        Add("栗", "くり", "Hạt dẻ", "Danh từ", JlptLevel.N2, "ごんは山で栗をひろいました。", "Gon nhặt những hạt dẻ trên núi.");
        Add("松茸", "まつたけ", "Nấm tùng nhung Matsutake", "Danh từ", JlptLevel.N1, "松茸を持って行きました。", "Mang theo nấm matsutake.");
        Add("うなぎ", "うなぎ", "Con lươn, cá chình", "Danh từ", JlptLevel.N3, "兵十のうなぎをいたずらしました。", "Nghịch ngợm lấy trộm con lươn của Hyoju.");
        Add("兵十", "ひょうじゅう", "Hyoju (Tên nhân vật)", "Danh từ riêng", JlptLevel.None, "兵十の家へくりを持って行きました。", "Mang hạt dẻ đến nhà Hyoju.");
        Add("火縄銃", "ひなわじゅう", "Súng hỏa mai", "Danh từ", JlptLevel.N1, "兵十は火縄銃をとりました。", "Hyoju đã cầm lấy cây súng hỏa mai.");
        Add("穴", "あな", "Cái hang, lỗ hổng", "Danh từ", JlptLevel.N3, "ごんは穴から出てきました。", "Gon chui ra khỏi hang.");
        Add("村", "むら", "Ngôi làng, làng quê", "Danh từ", JlptLevel.N4, "ちかくの村へ出てきて。", "Đi ra ngôi làng ở gần đó.");
        Add("いたずら", "いたずら", "Nghịch ngợm, trò đùa tai quái", "Danh từ", JlptLevel.N3, "いろんな悪戯ばかりしていました。", "Chuyên làm những trò nghịch ngợm.");

        // --- Yukiguni (Xứ tuyết) ---
        Add("雪", "ゆき", "Tuyết", "Danh từ", JlptLevel.N5, "国境の長いトンネルを抜けると雪国であった。", "Vượt qua đường hầm dài nơi biên giới là bước vào xứ tuyết.");
        Add("国境", "こっきょう", "Biên giới, ranh giới quốc gia/vùng", "Danh từ", JlptLevel.N3, "国境の長いトンネル。", "Đường hầm dài nơi biên giới.");
        Add("長い", "ながい", "Dài, lâu", "Tính từ đuôi i", JlptLevel.N5, "国境の長いトンネルを抜けると。", "Hễ qua khỏi đường hầm dài.");
        Add("トンネル", "とんねる", "Đường hầm", "Danh từ (Katakana)", JlptLevel.N3, "長いトンネルを抜けました。", "Đã vượt qua đường hầm dài.");
        Add("抜ける", "ぬける", "Vượt qua, thoát ra, tuột ra", "Động từ nhóm 2", JlptLevel.N3, "トンネルを抜けると雪国であった。", "Vừa qua khỏi đường hầm là xứ tuyết.");
        Add("底", "そこ", "Đáy, vùng sâu bên dưới", "Danh từ", JlptLevel.N3, "夜の底が白くなった。", "Đáy đêm đã chuyển sang màu trắng xóa.");
        Add("信号所", "しんごうじょ", "Trạm tín hiệu đường sắt", "Danh từ", JlptLevel.N2, "汽車が信号所に止まりました。", "Tàu hỏa dừng lại ở trạm tín hiệu.");
        Add("窓", "まど", "Cửa sổ", "Danh từ", JlptLevel.N5, "汽車は窓のガラスに映りました。", "Phản chiếu trên tấm kính cửa sổ tàu.");
        Add("鏡", "かがみ", "Tấm gương", "Danh từ", JlptLevel.N4, "窓ガラスが鏡のようになっていました。", "Kính cửa sổ trở nên tựa như một tấm gương.");

        // --- Botchan ---
        Add("親譲り", "おやゆずり", "Thừa hưởng từ cha mẹ", "Danh từ", JlptLevel.N1, "親譲りの無鉄砲で子供の時から損ばかりしている。", "Do thừa hưởng tính liều lĩnh bạt mạng từ cha mẹ nên từ thuở nhỏ toàn gặp thiệt thòi.");
        Add("無鉄砲", "むてっぽう", "Liều lĩnh, bạt mạng, không suy tính", "Tính từ Na / Danh từ", JlptLevel.N1, "親譲りの無鉄砲。", "Bản tính liều lĩnh thừa hưởng từ cha mẹ.");
        Add("子供", "こども", "Trẻ con, con nít", "Danh từ", JlptLevel.N5, "子供の時から損ばかりしている。", "Từ khi còn là một đứa trẻ đã toàn gặp thiệt thòi.");
        Add("時", "とき", "Lúc, khi, thời gian", "Danh từ", JlptLevel.N5, "子供の時。", "Lúc còn nhỏ.");
        Add("損", "そん", "Thiệt thòi, thua lỗ", "Danh từ", JlptLevel.N3, "損ばかりしている。", "Toàn gặp điều thiệt thòi.");

        // --- Rashomon (La Sinh Môn) ---
        Add("羅生門", "らしょうもん", "Cổng La Sinh Môn", "Danh từ riêng", JlptLevel.None, "ある日の暮れ方のことである。一人の下人が、羅生門の下で雨やみを待っていた。", "Chuyện xảy ra vào một buổi chiều tối nọ. Một gã người hầu đang đứng chờ tạnh mưa dưới cổng La Sinh Môn.");
        Add("下人", "げにん", "Người hầu, đầy tớ hạ đẳng", "Danh từ", JlptLevel.N1, "一人の下人が待っていた。", "Một gã người hầu đang đứng chờ.");
        Add("待つ", "まつ", "Chờ, đợi", "Động từ nhóm 1", JlptLevel.N5, "雨やみを待っていた。", "Đang đứng chờ tạnh mưa.");
        Add("雨", "あめ", "Cơn mưa, mưa", "Danh từ", JlptLevel.N5, "激しい雨が降っていました。", "Mưa lớn đang rơi xuống.");
        Add("降る", "ふる", "Rơi (mưa, tuyết)", "Động từ nhóm 1", JlptLevel.N5, "雨が降っていました。", "Mưa đang rơi.");
        Add("死骸", "しがい", "Thi thể, xác chết", "Danh từ", JlptLevel.N1, "門の上には多くの死骸が捨てられていた。", "Trên cổng có nhiều xác chết bị vứt bỏ.");
        Add("老婆", "ろうば", "Bà lão, bà già", "Danh từ", JlptLevel.N1, "一人の老婆が火を灯していた。", "Một bà lão đang thắp lửa.");
        Add("髪", "かみ", "Mái tóc, sợi tóc", "Danh từ", JlptLevel.N4, "老婆は死人の髪の毛を抜いていた。", "Bà lão đang nhổ tóc của người chết.");
        Add("抜く", "ぬく", "Nhổ ra, rút ra", "Động từ nhóm 1", JlptLevel.N3, "髪の毛を一本ずつ抜いていた。", "Nhổ từng sợi tóc một.");
        Add("生きる", "いきる", "Sống, sinh tồn", "Động từ nhóm 2", JlptLevel.N4, "生きるためには仕方がなかった。", "Để sống sót thì không còn cách nào khác.");
        Add("盗む", "ぬすむ", "Ăn trộm, ăn cắp", "Động từ nhóm 1", JlptLevel.N4, "着物を盗んで闇の中へ消えた。", "Cướp lấy bộ kimono rồi biến mất vào màn đêm.");

        // --- Core Grammar Words, Pronouns & Auxiliaries ---
        Add("私", "わたし", "Tôi (Đại từ nhân xưng trang trọng)", "Đại từ", JlptLevel.N5, "私は学生です。", "Tôi là học sinh.");
        Add("僕", "ぼく", "Tôi, tớ, mình (Dùng cho nam giới)", "Đại từ", JlptLevel.N5, "ぼくはジョバンニです。", "Tớ là Giovanni.");
        Add("彼", "かれ", "Anh ấy, cậu ấy", "Đại từ", JlptLevel.N4, "彼もまた走った。", "Cậu ấy cũng lại chạy tiếp.");
        Add("彼女", "かのじょ", "Cô ấy, bạn gái", "Đại từ", JlptLevel.N4, "彼女は優しく微笑んだ。", "Cô ấy mỉm cười dịu dàng.");
        Add("自分", "じぶん", "Bản thân mình, tự mình", "Đại từ", JlptLevel.N4, "自分で選んだ道。", "Con đường do chính mình lựa chọn.");
        Add("誰", "だれ", "Ai, người nào", "Đại từ", JlptLevel.N5, "誰かが歌っていた。", "Có ai đó đang hát.");
        Add("何", "なに", "Cái gì, điều gì", "Đại từ", JlptLevel.N5, "何が起きたのだろう。", "Điều gì đã xảy ra nhỉ.");
        Add("どこ", "どこ", "Ở đâu, nơi nào", "Đại từ", JlptLevel.N5, "どこへ行くの？", "Đi đâu thế?");
        Add("これ", "これ", "Cái này (gần người nói)", "Đại từ", JlptLevel.N5, "これは何ですか。", "Đây là cái gì?");
        Add("それ", "それ", "Cái đó (gần người nghe)", "Đại từ", JlptLevel.N5, "それを聞いて安心した。", "Nghe điều đó tôi mới yên tâm.");
        Add("あれ", "あれ", "Cái kia (xa cả hai)", "Đại từ", JlptLevel.N5, "あれが銀河ステーションだ。", "Kia là Ga Ngân Hà.");
        Add("ある", "ある", "Có, tồn tại (vật vô tri)", "Động từ nhóm 1", JlptLevel.N5, "そこに本がある。", "Ở đó có một cuốn sách.");
        Add("いる", "いる", "Có, ở (sinh vật sống)", "Động từ nhóm 2", JlptLevel.N5, "友達がそこにいる。", "Bạn tôi đang ở đó.");
        Add("見る", "みる", "Nhìn, xem, trông thấy", "Động từ nhóm 2", JlptLevel.N5, "星を見ました。", "Tôi đã nhìn những ngôi sao.");
        Add("言う", "いう", "Nói, bảo", "Động từ nhóm 1", JlptLevel.N5, "先生は言いました。", "Thầy giáo đã nói.");
        Add("聞く", "きく", "Nghe, hỏi", "Động từ nhóm 1", JlptLevel.N5, "美しい歌声を聞いた。", "Tôi đã nghe thấy tiếng hát tuyệt đẹp.");
        Add("行く", "いく", "Đi, tiến về", "Động từ nhóm 1", JlptLevel.N5, "学校へ行きました。", "Đã đi đến trường.");
        Add("来る", "くる", "Đến, tới", "Động từ bất quy tắc", JlptLevel.N5, "汽車がやって来ました。", "Đoàn tàu đã tiến đến.");
        Add("知る", "しる", "Biết, hiểu rõ", "Động từ nhóm 1", JlptLevel.N5, "本当のことを知っている。", "Tôi biết sự thật.");
        Add("思う", "おもう", "Nghĩ, cảm thấy", "Động từ nhóm 1", JlptLevel.N4, "そう思いました。", "Tôi đã nghĩ như vậy.");
        Add("心", "こころ", "Trái tim, tấm lòng, tâm hồn", "Danh từ", JlptLevel.N4, "美しい心を持っている。", "Mang một tâm hồn cao đẹp.");
        Add("時間", "じかん", "Thời gian, tiếng đồng hồ", "Danh từ", JlptLevel.N5, "時間が過ぎて行きました。", "Thời gian cứ thế trôi đi.");
        Add("人", "ひと", "Con người, người", "Danh từ", JlptLevel.N5, "多くの人が集まっていました。", "Rất nhiều người đã tập hợp lại.");
        Add("道", "みち", "Con đường, lối đi", "Danh từ", JlptLevel.N5, "暗い道を歩きました。", "Tôi bước đi trên con đường tối tăm.");
        Add("手", "て", "Bàn tay, tay", "Danh từ", JlptLevel.N5, "手を差し伸べました。", "Đưa bàn tay ra.");
        Add("目", "め", "Mắt, ánh mắt", "Danh từ", JlptLevel.N5, "目を閉じて考えた。", "Nhắm mắt lại và suy ngẫm.");
        Add("声", "こえ", "Giọng nói, tiếng gọi", "Danh từ", JlptLevel.N4, "遠くから声が聞こえた。", "Nghe thấy giọng nói từ phương xa.");
        Add("空", "そら", "Bầu trời", "Danh từ", JlptLevel.N5, "夜の空を見上げました。", "Ngước nhìn lên bầu trời đêm.");
        Add("川", "かわ", "Con sông, dòng sông", "Danh từ", JlptLevel.N5, "川の向こう岸。", "Bờ bên kia dòng sông.");
        Add("山", "やま", "Ngọn núi, đồi núi", "Danh từ", JlptLevel.N5, "山の上へ登りました。", "Leo lên đỉnh núi.");
        Add("町", "まち", "Thị trấn, khu phố, thành phố", "Danh từ", JlptLevel.N5, "町中が明るくなりました。", "Khắp thị trấn bừng sáng.");
        Add("光", "ひかり", "Ánh sáng, tia sáng", "Danh từ", JlptLevel.N3, "青い光が輝いていました。", "Ánh sáng xanh biếc đang tỏa rạng.");
        Add("輝く", "かがやく", "Tỏa sáng, lấp lánh", "Động từ nhóm 1", JlptLevel.N3, "星が美しく輝いていました。", "Những vì sao lấp lánh tuyệt đẹp.");
        Add("進む", "すすむ", "Tiến lên, tiến bộ, tiếp tục", "Động từ nhóm 1", JlptLevel.N4, "前へ進みました。", "Tiến về phía trước.");
        Add("止まる", "とまる", "Dừng lại, đỗ lại", "Động từ nhóm 1", JlptLevel.N4, "列車が止まりました。", "Đoàn tàu đã dừng lại.");
        Add("笑う", "わらう", "Cười, mỉm cười", "Động từ nhóm 1", JlptLevel.N4, "カムパネルラは笑いました。", "Campanella đã mỉm cười.");
        Add("泣く", "なく", "Khóc", "Động từ nhóm 1", JlptLevel.N4, "悲しくて泣きました。", "Buồn bã bật khóc.");
        Add("呼ぶ", "よぶ", "Gọi, kêu gọi", "Động từ nhóm 1", JlptLevel.N4, "名前を呼びました。", "Đã gọi tên.");
        Add("助ける", "たすける", "Cứu giúp, giúp đỡ", "Động từ nhóm 2", JlptLevel.N3, "友達を助けたい。", "Muốn cứu giúp người bạn.");
        Add("急ぐ", "いそぐ", "Vội vã, gấp rút", "Động từ nhóm 1", JlptLevel.N4, "急いで走りました。", "Chạy vội vã.");
        Add("立ち上がる", "たちあがる", "Đứng dậy, vùng dậy", "Động từ nhóm 1", JlptLevel.N5, "ジョバンニは勢いよく立ち上がりました。", "Giovanni đã bật đứng dậy thật hăng hái.");
        Add("白い", "しろい", "Màu trắng, trắng ngần", "Tính từ đuôi i", JlptLevel.N5, "ぼんやりと白いもの。", "Một vệt mờ ảo trắng xóa.");
        Add("黒い", "くろい", "Màu đen, đen thẫm", "Tính từ đuôi i", JlptLevel.N5, "大きな黒い星座の図。", "Bức vẽ các chòm sao lớn màu đen.");
        Add("青い", "あおい", "Màu xanh da trời, xanh biếc", "Tính từ đuôi i", JlptLevel.N5, "青いあかりを網にして。", "Lấy ánh sáng xanh đan thành lưới.");
        Add("赤い", "あかい", "Màu đỏ, đỏ thắm", "Tính từ đuôi i", JlptLevel.N5, "赤い火が見えました。", "Nhìn thấy ngọn lửa màu đỏ.");
        Add("大きい", "おおきい", "To, lớn, vĩ đại", "Tính từ đuôi i", JlptLevel.N5, "大きな活版所に入って行きました。", "Bước vào một xưởng in lớn.");
        Add("小さい", "ちいさい", "Nhỏ, bé", "Tính từ đuôi i", JlptLevel.N5, "小さな鳥が鳴いていた。", "Chú chim nhỏ đang hót.");
        Add("美しい", "うつくしい", "Đẹp đẽ, mỹ lệ, tuyệt đẹp", "Tính từ đuôi i", JlptLevel.N3, "美しい銀色の蜘蛛。", "Chú nhện màu bạc tuyệt đẹp.");
        Add("悲しい", "かなしい", "Đau buồn, bi thương", "Tính từ đuôi i", JlptLevel.N4, "悲しい物語。", "Một câu chuyện buồn.");
        Add("嬉しい", "うれしい", "Vui mừng, sung sướng", "Tính từ đuôi i", JlptLevel.N4, "とても嬉しかった。", "Tôi đã cảm thấy vô cùng vui sướng.");
        Add("優しい", "やさしい", "Dịu dàng, hiền từ, tốt bụng", "Tính từ đuôi i", JlptLevel.N4, "優しい声で話しかけた。", "Nói chuyện bằng một giọng nói dịu dàng.");
        Add("静か", "しずか", "Yên tĩnh, thanh bình", "Tính từ Na", JlptLevel.N5, "夜の町は静かでした。", "Thị trấn về đêm thật yên bình.");
        Add("白く", "しろく", "Trắng xóa, một cách trắng trẻo", "Phó từ", JlptLevel.N5, "上から下へ白くけむった。", "Mờ trắng xóa từ trên xuống dưới.");
        Add("けむる", "けむる", "Mờ ảo, bốc khói, nhạt nhòa", "Động từ nhóm 1", JlptLevel.N1, "上から下へ白くけむった。", "Mờ nhạt trắng xóa từ trên xuống.");
        Add("ちかく", "ちかく", "Gần, lân cận", "Danh từ / Phó từ", JlptLevel.N5, "ちかくの村へ出てきて。", "Mò ra ngôi làng gần đó.");
        Add("ふち", "ふち", "Bờ, mép, rìa", "Danh từ", JlptLevel.N2, "極楽の蓮池のふちを歩く。", "Dạo bước bên bờ ao sen cực lạc.");
        Add("雨やみ", "あまやみ", "Tạnh mưa, lúc dứt hạt mưa", "Danh từ", JlptLevel.N2, "羅生門の下で雨やみを待っていた。", "Chờ tạnh mưa dưới cổng La Sinh Môn.");
        Add("いらっしゃる", "いらっしゃる", "Kính ngữ của いる, 行く, 来る (ở, đi, đến)", "Động từ kính ngữ", JlptLevel.N3, "独りで歩いていらっしゃいました。", "Ngài đang một mình dạo bước.");
        Add("ところ", "ところ", "Nơi, chỗ, thời điểm", "Danh từ", JlptLevel.N4, "帯のようなところを指しながら。", "Vừa chỉ vào dải mờ ảo tựa như dải ngân hà.");
        Add("ような", "ような", "Tựa như, giống như", "Trợ từ / Cụm từ liên kết", JlptLevel.N3, "帯のようなところ。", "Nơi tựa như một dải.");
        Add("よう", "よう", "Hình như, dường như", "Danh từ phụ thuộc", JlptLevel.N3, "帯のようだ。", "Như một dải lụa.");
        Add("こと", "こと", "Điều, việc, sự việc", "Danh từ", JlptLevel.N5, "知ること。", "Việc thấu hiểu.");
        Add("もの", "もの", "Thứ, đồ vật, sự vật", "Danh từ", JlptLevel.N5, "白いもの。", "Một thứ màu trắng.");

        // --- Common Particles ---
        Add("は", "は", "Trợ từ chỉ chủ đề (Topic marker)", "Trợ từ", JlptLevel.N5, "私は学生です。", "Tôi là học sinh.");
        Add("が", "が", "Trợ từ chỉ chủ ngữ / tân ngữ của tính từ (Subject marker)", "Trợ từ", JlptLevel.N5, "雨が降る。", "Mưa rơi.");
        Add("を", "を", "Trợ từ chỉ tân ngữ trực tiếp (Object marker)", "Trợ từ", JlptLevel.N5, "本を読む。", "Đọc sách.");
        Add("に", "に", "Trợ từ chỉ phương hướng, thời gian, vị trí", "Trợ từ", JlptLevel.N5, "家に行く。", "Về nhà.");
        Add("へ", "へ", "Trợ từ chỉ hướng chuyển động", "Trợ từ", JlptLevel.N5, "学校へ行く。", "Đi về phía trường học.");
        Add("で", "で", "Trợ từ chỉ địa điểm hành động, phương tiện, nguyên nhân", "Trợ từ", JlptLevel.N5, "電車で行く。", "Đi bằng tàu điện.");
        Add("と", "と", "Trợ từ chỉ cùng với, và, trích dẫn", "Trợ từ", JlptLevel.N5, "友達と話す。", "Nói chuyện với bạn.");
        Add("から", "から", "Trợ từ chỉ xuất phát điểm (từ...), nguyên nhân (vì...)", "Trợ từ", JlptLevel.N5, "東京から京都まで。", "Từ Tokyo đến Kyoto.");
        Add("まで", "まで", "Trợ từ chỉ điểm đến (đến, cho tới...)", "Trợ từ", JlptLevel.N5, "夜まで待つ。", "Đợi cho tới đêm.");
        Add("より", "より", "Trợ từ so sánh (hơn là, từ)", "Trợ từ", JlptLevel.N5, "誰よりも速く。", "Nhanh hơn bất kỳ ai.");
        Add("も", "も", "Trợ từ cũng, đến mức", "Trợ từ", JlptLevel.N5, "私も行く。", "Tôi cũng đi.");
        Add("の", "の", "Trợ từ chỉ sở hữu, liên kết danh từ", "Trợ từ", JlptLevel.N5, "私の本。", "Sách của tôi.");
        Add("ね", "ね", "Trợ từ cuối câu xác nhận, đồng cảm (nhỉ, nhé)", "Trợ từ", JlptLevel.N5, "いい天気ですね。", "Thời tiết đẹp nhỉ.");
        Add("よ", "よ", "Trợ từ cuối câu nhấn mạnh, thông báo", "Trợ từ", JlptLevel.N5, "行くよ。", "Đi thôi đấy nhé.");
        Add("か", "か", "Trợ từ cuối câu nghi vấn, hoặc là", "Trợ từ", JlptLevel.N5, "本当ですか。", "Có thật không?");
        Add("だ", "だ", "Trợ động từ khẳng định suồng sã (là)", "Trợ động từ", JlptLevel.N5, "友達だ。", "Là bạn bè.");
        Add("です", "です", "Trợ động từ khẳng định lịch sự (là)", "Trợ động từ", JlptLevel.N5, "星です。", "Là những vì sao.");
    }
}
