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
        foreach (var candidate in GetDeinflectionCandidates(cleanTerm))
        {
            if (_dictionary.TryGetValue(candidate, out var deinflectedEntry))
            {
                return Task.FromResult<DictionaryEntry?>(deinflectedEntry);
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

    private IEnumerable<string> GetDeinflectionCandidates(string surface)
    {
        var candidates = new List<string>();

        // 1. Polite past form: ~ました
        if (surface.EndsWith("ました") && surface.Length > 3)
        {
            string stem = surface.Substring(0, surface.Length - 3);
            // Ichidan: stem + る (e.g. 見ました -> 見る, 食べました -> 食べる)
            candidates.Add(stem + "る");

            // Godan endings
            if (stem.EndsWith("い")) candidates.Add(stem.Substring(0, stem.Length - 1) + "う");
            if (stem.EndsWith("き")) candidates.Add(stem.Substring(0, stem.Length - 1) + "く");
            if (stem.EndsWith("ぎ")) candidates.Add(stem.Substring(0, stem.Length - 1) + "ぐ");
            if (stem.EndsWith("し")) candidates.Add(stem.Substring(0, stem.Length - 1) + "す");
            if (stem.EndsWith("ち")) candidates.Add(stem.Substring(0, stem.Length - 1) + "つ");
            if (stem.EndsWith("び")) candidates.Add(stem.Substring(0, stem.Length - 1) + "ぶ");
            if (stem.EndsWith("み")) candidates.Add(stem.Substring(0, stem.Length - 1) + "む");
            if (stem.EndsWith("り")) candidates.Add(stem.Substring(0, stem.Length - 1) + "る");
        }

        // 2. Polite non-past: ~ます
        if (surface.EndsWith("ます") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add(stem + "る");
            if (stem.EndsWith("い")) candidates.Add(stem.Substring(0, stem.Length - 1) + "う");
            if (stem.EndsWith("き")) candidates.Add(stem.Substring(0, stem.Length - 1) + "く");
            if (stem.EndsWith("し")) candidates.Add(stem.Substring(0, stem.Length - 1) + "す");
            if (stem.EndsWith("ち")) candidates.Add(stem.Substring(0, stem.Length - 1) + "つ");
            if (stem.EndsWith("み")) candidates.Add(stem.Substring(0, stem.Length - 1) + "む");
            if (stem.EndsWith("り")) candidates.Add(stem.Substring(0, stem.Length - 1) + "る");
        }

        // 3. Past plain: ~んだ / ~いた / ~った
        if (surface.EndsWith("んだ") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add(stem + "む");
            candidates.Add(stem + "ぶ");
        }
        if (surface.EndsWith("いた") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add(stem + "く");
        }
        if (surface.EndsWith("った") && surface.Length > 2)
        {
            string stem = surface.Substring(0, surface.Length - 2);
            candidates.Add(stem + "う");
            candidates.Add(stem + "つ");
            candidates.Add(stem + "る");
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
        // Core words from Literature novels
        Add("銀河", "ぎんが", "Ngân hà, dải thiên hà (Milky Way)", "Danh từ", JlptLevel.N3, "銀河の帯のようなところを指しながら。", "Vừa chỉ vào dải mờ ảo tựa như dải ngân hà.");
        Add("鉄道", "てつどう", "Đường sắt, tuyến đường xe lửa", "Danh từ", JlptLevel.N3, "銀河鉄道の夜。", "Đêm trên chuyến tàu ngân hà.");
        Add("夜", "よる", "Đêm, ban đêm", "Danh từ", JlptLevel.N5, "夜でも昼でも、ちかくの村へ出てきて。", "Dù đêm hay ngày, nó đều mò ra ngôi làng gần đó.");
        Add("授業", "じゅぎょう", "Tiết học, giờ học ở trường", "Danh từ", JlptLevel.N4, "午後の授業がはじまりました。", "Giờ học buổi chiều đã bắt đầu.");
        Add("先生", "せんせい", "Thầy giáo, cô giáo, bác sĩ", "Danh từ", JlptLevel.N5, "先生は黒板に大きな図を描きました。", "Thầy giáo đã vẽ một bức hình lớn lên bảng đen.");
        Add("黒板", "こくばん", "Bảng đen", "Danh từ", JlptLevel.N4, "黒板に吊した大きな黒い星座の図。", "Bản đồ các chòm sao màu đen treo trên bảng đen.");
        Add("星座", "せいざ", "Chòm sao, tinh tú", "Danh từ", JlptLevel.N3, "黒い星座の図。", "Bản đồ các chòm sao.");
        Add("星", "ほし", "Ngôi sao, tinh cầu", "Danh từ", JlptLevel.N5, "たしかにあれがみんな星だと読んだ。", "Chắc chắn từng đọc rằng tất cả đều là những vì sao.");
        Add("活版所", "かっぱんじょ", "Xưởng in, nhà in kiểu chữ chì", "Danh từ", JlptLevel.N2, "ジョバンニはある大きな活版所に入って行きました。", "Giovanni bước vào một xưởng in lớn.");
        Add("学校", "がっこう", "Trường học", "Danh từ", JlptLevel.N5, "ジョバンニは学校の門を出ました。", "Giovanni bước ra khỏi cổng trường.");
        Add("門", "もん", "Cổng, cửa lớn", "Danh từ", JlptLevel.N4, "学校の門を出るとき。", "Khi bước ra khỏi cổng trường học.");
        Add("桜", "さくら", "Hoa anh đào, cây hoa anh đào", "Danh từ", JlptLevel.N4, "校庭の隅の桜の木。", "Cây hoa anh đào nơi góc sân trường.");
        Add("町", "まち", "Thị trấn, phố xá", "Danh từ", JlptLevel.N5, "町の家々はもうあかりがついていました。", "Những ngôi nhà trong thị trấn đã thắp đèn sáng.");
        Add("旗", "はた", "Cờ, lá cờ", "Danh từ", JlptLevel.N3, "色とりどりの旗を吊したり。", "Treo những lá cờ đủ sắc màu.");
        Add("紙", "かみ", "Giấy tờ, tờ giấy", "Danh từ", JlptLevel.N5, "紙切れを一枚渡しました。", "Đã đưa cho một mẩu giấy.");

        Add("坊っちゃん", "ぼっちゃん", "Thiếu gia, cậu ấm (cách gọi cưng chiều)", "Danh từ", JlptLevel.N3, "坊っちゃんはおやじに似て無鉄砲だ。", "Cậu ấm giống tính cha nên rất hấp tấp liều lĩnh.");
        Add("無鉄砲", "むてっぽう", "Hấp tấp, liều lĩnh, liều mạng không tính toán", "Tính từ na / Danh từ", JlptLevel.N1, "親譲りの無鉄砲で小供の時から損ばかりしている。", "Thừa hưởng tính liều lĩnh từ cha nên từ bé chỉ toàn chịu thiệt thòi.");
        Add("小供", "ことも", "Trẻ con, đứa bé (cách viết cổ của 子供)", "Danh từ", JlptLevel.N5, "小供の時から損ばかりしている。", "Từ khi còn là đứa trẻ chỉ toàn gặp thiệt.");
        Add("損", "そん", "Sự thiệt hại, tổn thất, thua lỗ", "Danh từ", JlptLevel.N3, "損ばかりしている。", "Toàn chịu thiệt thòi.");
        Add("小学校", "しょうがっこう", "Trường tiểu học", "Danh từ", JlptLevel.N5, "小学校に居る時分。", "Hồi còn học ở trường tiểu học.");
        Add("二階", "にかい", "Tầng hai, gác hai", "Danh từ", JlptLevel.N5, "学校の二階から飛び降りた。", "Nhảy từ tầng hai của trường xuống.");
        Add("同級生", "どうきゅうせい", "Bạn cùng lớp, bạn đồng khóa", "Danh từ", JlptLevel.N3, "同級生の一人が冗談に囃した。", "Một người bạn cùng lớp đã hò reo trêu chọc.");
        Add("弱虫", "よわむし", "Kẻ yếu đuối, nhát gan", "Danh từ", JlptLevel.N2, "弱虫やーいと囃したからである。", "Bởi vì nó trêu chọc kêu đồ nhát gan.");

        Add("ごんぎつね", "ごんぎつね", "Cáo Gon (tên riêng tác phẩm cổ tích)", "Danh từ riêng", JlptLevel.N5, "「ごんぎつね」という、いたずら好きの小ぎつね。", "Một chú cáo nhỏ thích nghịch ngợm tên là Gongitsune.");
        Add("城", "しろ", "Lâu đài, thành trì", "Danh từ", JlptLevel.N3, "中山というところに小さなお城があって。", "Ở nơi tên là Nakayama có một tòa thành nhỏ.");
        Add("森", "もり", "Rừng rậm", "Danh từ", JlptLevel.N4, "森の中に穴をほって住んでいました。", "Đào hang trong rừng rậm để sinh sống.");
        Add("穴", "あな", "Hang, cái lỗ", "Danh từ", JlptLevel.N3, "森の中に穴をほって住んでいた。", "Đã đào một cái hang trong rừng để ở.");
        Add("村", "むら", "Làng mạc, thôn xóm", "Danh từ", JlptLevel.N4, "ちかくの村へ出てきて、いたずらばかりしました。", "Mò ra ngôi làng gần đó rồi toàn trêu chọc nghịch ngợm.");
        Add("いたずら", "いたずら", "Trò nghịch ngợm, trêu chọc, quậy phá", "Danh từ", JlptLevel.N4, "いたずらばかりしました。", "Toàn làm những trò nghịch ngợm.");

        Add("雪国", "ゆきぐに", "Xứ tuyết, miền tuyết trắng", "Danh từ", JlptLevel.N2, "国境の長いトンネルを抜けると雪国であった。", "Xuyên qua đường hầm dài nơi biên giới là tới xứ tuyết.");
        Add("国境", "こっきょう", "Biên giới, ranh giới tỉnh bang", "Danh từ", JlptLevel.N3, "国境の長いトンネル。", "Đường hầm dài nơi ranh giới tỉnh.");
        Add("トンネル", "とんねる", "Đường hầm (Tunnel)", "Danh từ", JlptLevel.N2, "長いトンネルを抜けると雪国であった。", "Vừa qua khỏi đường hầm dài đã là xứ tuyết.");
        Add("汽車", "きしゃ", "Xe lửa, tàu hỏa chạy bằng hơi nước", "Danh từ", JlptLevel.N4, "信号所に汽車が止まった。", "Đoàn tàu hỏa dừng lại ở trạm tín hiệu.");
        Add("駅長", "えきちょう", "Trưởng ga, trạm trưởng nhà ga", "Danh từ", JlptLevel.N3, "「駅長さん、駅長さん」と叫んだ。", "Cất tiếng gọi to: 'Bác trưởng ga ơi, bác trưởng ga!'");
        Add("窓", "まど", "Cửa sổ", "Danh từ", JlptLevel.N5, "窓を落した。雪の冷気が流れこんだ。", "Hạ cửa sổ xuống. Không khí lạnh buốt của tuyết ùa vào.");

        Add("蜘蛛の糸", "くものいと", "Sợi tơ nhện", "Danh từ", JlptLevel.N3, "美しい銀色の蜘蛛の糸を御手に取って。", "Cầm lấy sợi tơ nhện màu bạc lóng lánh.");
        Add("極楽", "ごくらく", "Cõi Cực Lạc, thiên đường an lạc", "Danh từ", JlptLevel.N1, "極楽の蓮池のふちを独りで歩いていらっしゃいました。", "Một mình dạo bước bên bờ ao sen cõi Cực Lạc.");
        Add("地獄", "じごく", "Địa ngục, chốn khổ ải", "Danh từ", JlptLevel.N2, "この蓮池の下は丁度地獄の底に当って居ります。", "Phía dưới ao sen này chính là đáy chốn địa ngục.");
        Add("蓮", "はす", "Hoa sen, búp sen", "Danh từ", JlptLevel.N2, "池の中に咲いている蓮の花。", "Những đóa sen nở rộ trong ao.");
        Add("羅生門", "らしょうもん", "Cổng Rashomon (cổng thành lịch sử Kyoto)", "Danh từ riêng", JlptLevel.N2, "一人の下人が、羅生門の下で雨やみを待っていた。", "Một người hạ nhân đứng dưới cổng Rashomon chờ tạnh mưa.");
        Add("下人", "げにん", "Người hầu, hạ nhân, gia nhân thời xưa", "Danh từ", JlptLevel.N1, "一人の下人が雨やみを待っていた。", "Một kẻ hạ nhân đang chờ trời tạnh mưa.");
        Add("雨", "あめ", "Cơn mưa, trời mưa", "Danh từ", JlptLevel.N5, "雨やみを待っていた。", "Đang chờ cho tạnh mưa.");
        Add("刀", "かたな", "Thanh kiếm, đao kiếm Nhật", "Danh từ", JlptLevel.N3, "下人は刀を抜いて老婆に躍りかかった。", "Hạ nhân rút đao xông vào mụ già.");

        // Essential verbs & adjectives
        Add("読む", "よむ", "Đọc sách, ngâm thơ", "Động từ nhóm 1", JlptLevel.N5, "本を読むひまもない。", "Không có cả thời gian rảnh rỗi để đọc sách.");
        Add("見る", "みる", "Nhìn, xem, trông thấy", "Động từ nhóm 2", JlptLevel.N5, "覗き眼鏡を見るように、はっきりと見える。", "Nhìn rõ mồn một như đang nhìn qua kính dòm.");
        Add("行く", "いく", "Đi, tiến tới", "Động từ nhóm 1", JlptLevel.N5, "活版所に入って行きました。", "Đã bước vào xưởng in.");
        Add("来る", "くる", "Đến, tới nơi", "Động từ nhóm 3", JlptLevel.N5, "雪の中をやって来た男。", "Người đàn ông đi tới từ trong làn tuyết trắng.");
        Add("歩く", "あるく", "Đi bộ, dạo bước", "Động từ nhóm 1", JlptLevel.N5, "町の方へ歩きました。", "Đã rảo bước đi về phía thị trấn.");
        Add("知る", "しる", "Biết, nhận biết", "Động từ nhóm 1", JlptLevel.N5, "ほんとうは何かご承知ですか。", "Các em có biết thực chất đó là thứ gì không?");
        Add("言う", "いう", "Nói, bảo rằng", "Động từ nhóm 1", JlptLevel.N5, "川だと云われたりしていた。", "Từng được bảo rằng đó là dòng sông.");
        Add("答える", "こたえる", "Trả lời, đáp lại", "Động từ nhóm 2", JlptLevel.N4, "はっきりと答えることができませんでした。", "Không thể nào trả lời một cách rõ ràng được.");
        Add("立つ", "たつ", "Đứng dậy, dựng lên", "Động từ nhóm 1", JlptLevel.N5, "ジョバンニは勢いよく立ちあがりました。", "Giovanni đã bật đứng dậy thật hăng hái.");
        Add("白い", "しろい", "Màu trắng, trắng ngần", "Tính từ đuôi i", JlptLevel.N5, "ぼんやりと白いもの。", "Một vệt mờ ảo trắng xóa.");
        Add("黒い", "くろい", "Màu đen, đen thẫm", "Tính từ đuôi i", JlptLevel.N5, "大きな黒い星座の図。", "Bức vẽ các chòm sao lớn màu đen.");
        Add("青い", "あおい", "Màu xanh da trời, xanh biếc", "Tính từ đuôi i", JlptLevel.N5, "青いあかりを網にして。", "Lấy ánh sáng xanh đan thành lưới.");
        Add("大きい", "おおきい", "To, lớn, vĩ đại", "Tính từ đuôi i", JlptLevel.N5, "大きな活版所に入って行きました。", "Bước vào một xưởng in lớn.");
        Add("美しい", "うつくしい", "Đẹp đẽ, mỹ lệ, tuyệt đẹp", "Tính từ đuôi i", JlptLevel.N3, "美しい銀色の蜘蛛。", "Chú nhện màu bạc tuyệt đẹp.");
    }
}
