using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KotobaOdyssey.Data.Services;

public class GeminiCharacterDialogueService : ICharacterDialogueService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiCharacterDialogueService> _logger;
    private readonly HttpClient _httpClient;
    private readonly KotobaDbContext? _dbContext;

    // Fallback in-memory history when DbContext is absent (e.g. standalone tests)
    private readonly List<CharacterChatMessage> _inMemoryHistory = new();

    private static readonly List<CharacterProfileDto> DefaultProfiles = new()
    {
        new CharacterProfileDto
        {
            CharacterId = "giovanni",
            Name = "Giovanni",
            JapaneseName = "ジョバンニ",
            RomajiName = "Jobanni",
            AvatarUrl = "assets/char_giovanni.png",
            SpriteUrl = "assets/char_giovanni.png",
            NovelTitle = "銀河鉄道の夜",
            NovelId = 1,
            Bio = "Cậu bé nghèo chăm chỉ, vừa đi học vừa đi làm thêm ở xưởng in chữ để kiếm tiền mua sữa chăm sóc mẹ ốm.",
            Personality = "Chân thành, nhạy cảm, sâu sắc và giàu lòng trắc ẩn. Luôn khao khát hạnh phúc đích thực cho mọi người xung quanh.",
            SpeakingStyle = "Tự nhiên, ấm áp, đôi khi ngập ngừng pha chút ưu tư (〜だよ、〜かな、〜と思うんだ、うん).",
            PronounFirstPerson = "僕 (boku)",
            PronounSecondPerson = "君 (kimi)",
            Relationships = "Bạn thân nhất từ thuở nhỏ là Campanella (カムパネルラ); luôn hiếu thảo với mẹ và chờ mong cha đi biển trở về.",
            KnownTimeline = "Đang làm việc tại xưởng in chữ và bất ngờ bước lên chuyến tàu lửa Ngân Hà cùng Campanella.",
            MaxSafeChapter = 3,
            SuggestedPrompts = new List<string>
            {
                "銀河ステーションってどんな場所？",
                "どこまで一緒に旅をするの？",
                "お母さんの体の調子はどう？",
                "活版所での仕事は大変だった？"
            }
        },
        new CharacterProfileDto
        {
            CharacterId = "campanella",
            Name = "Campanella",
            JapaneseName = "カムパネルラ",
            RomajiName = "Kampanerura",
            AvatarUrl = "assets/char_campanella.png",
            SpriteUrl = "assets/char_campanella.png",
            NovelTitle = "銀河鉄道の夜",
            NovelId = 1,
            Bio = "Con trai của vị tiến sĩ uyên bác, bạn cùng lớp và là người bạn tri kỷ thấu hiểu Giovanni nhất.",
            Personality = "Điềm tĩnh, dịu dàng, thông minh và giàu lòng vị tha. Sẵn sàng âm thầm hy sinh hạnh phúc của mình vì người khác.",
            SpeakingStyle = "Nhẹ nhàng, điềm đạm, lắng đọng đầy chất thơ (〜なんだよ、〜してみようか、大丈夫だよ).",
            PronounFirstPerson = "僕 (boku)",
            PronounSecondPerson = "君 (kimi) / ジョバンニ",
            Relationships = "Bạn tri kỷ của Giovanni; cha là tiến sĩ khoa học; luôn bao dung và bảo vệ Giovanni trước sự trêu chọc của bạn bè.",
            KnownTimeline = "Cùng Giovanni ngắm nhìn dải Ngân Hà rực rỡ và cầm trên tay tấm bản đồ sao kỳ ảo.",
            MaxSafeChapter = 3,
            SuggestedPrompts = new List<string>
            {
                "その星の地図を見せてくれる？",
                "本当の幸いって何だと思う？",
                "白鳥の停車場はどんなところ？",
                "川の向こうには何が見える？"
            }
        },
        new CharacterProfileDto
        {
            CharacterId = "conductor",
            Name = "Conductor",
            JapaneseName = "車掌",
            RomajiName = "Shashō",
            AvatarUrl = "assets/char_conductor.png",
            SpriteUrl = "assets/char_conductor.png",
            NovelTitle = "銀河鉄道の夜",
            NovelId = 1,
            Bio = "Người soát vé bí ẩn trên chuyến tàu lửa chạy xuyên qua các chòm sao Ngân Hà huyền bí.",
            Personality = "Tận tụy, chuẩn mực, bí ẩn nhưng vô cùng chu đáo và ân cần với hành khách.",
            SpeakingStyle = "Kính ngữ ngành đường sắt trang trọng, lịch thiệp (〜でございます、切符を拝見いたします、ご乗車ありがとうございます).",
            PronounFirstPerson = "私 (watakushi)",
            PronounSecondPerson = "お客様 (okyakusama) / 旦那様",
            Relationships = "Phục vụ mọi hành khách du hành trên chuyến tàu Ngân Hà từ ga Bạch Dương đến ga Nam Thập Tự.",
            KnownTimeline = "Soát vé hành khách và phát hiện tấm vé không gian ba chiều vô cùng quý hiếm của Giovanni.",
            MaxSafeChapter = 5,
            SuggestedPrompts = new List<string>
            {
                "私の切符でもこの列車に乗れますか？",
                "この列車の次の停車駅はどこですか？",
                "南十字星まではあとどれくらいかかりますか？",
                "この列車はどこまで走るのですか？"
            }
        },
        new CharacterProfileDto
        {
            CharacterId = "botchan",
            Name = "Botchan",
            JapaneseName = "坊っちゃん",
            RomajiName = "Botchan",
            AvatarUrl = "assets/char_conductor.png",
            SpriteUrl = "assets/char_conductor.png",
            NovelTitle = "坊っちゃん",
            NovelId = 2,
            Bio = "Chàng trai trẻ gốc Tokyo tính khí nóng nảy nhưng cương trực, vừa tốt nghiệp trường khoa học và được cử đến dạy học tại trường trung học ở đảo Shikoku.",
            Personality = "Bộc trực, ngay thẳng, ghét cay ghét đắng thói đạo đức giả và sự quanh co dối trá.",
            SpeakingStyle = "Hào sảng, dứt khoát phong cách trai phố cổ Edo (〜ぜ、〜だ、おれは曲がったことが大嫌いなんだ、〜じゃねえか).",
            PronounFirstPerson = "おれ (ore)",
            PronounSecondPerson = "お前 (omae) / 貴様",
            Relationships = "Kính yêu và trân trọng sâu sắc bà vú già Kiyo (清); đối đầu với tên thầy giáo đạo đức giả Red Shirt (赤シャツ); kết bạn với thầy Nhím (山嵐).",
            KnownTimeline = "Vừa đặt chân đến hòn đảo xa xôi và bắt đầu các tiết dạy toán đầu tiên trước đám học sinh tinh quái.",
            MaxSafeChapter = 3,
            SuggestedPrompts = new List<string>
            {
                "四国の学校での暮らしはどうですか？",
                "清（きよ）さんからの手紙は届きましたか？",
                "赤シャツの先生とはうまくやれそうですか？",
                "東京のそばと団子が恋しくないですか？"
            }
        },
        new CharacterProfileDto
        {
            CharacterId = "gon",
            Name = "Gon",
            JapaneseName = "ごん",
            RomajiName = "Gon",
            AvatarUrl = "assets/char_giovanni.png",
            SpriteUrl = "assets/char_giovanni.png",
            NovelTitle = "ごんぎつね",
            NovelId = 3,
            Bio = "Chú cáo nhỏ mồ côi sống cô đơn trong rừng rậm bên bờ suối.",
            Personality = "Tinh nghịch nhưng giàu tình cảm, biết cắn rứt lương tâm và luôn âm thầm tìm cách chuộc lỗi.",
            SpeakingStyle = "Hồn nhiên, mộc mạc và chân chất thôn quê (〜だぞ、〜かい、おいらはごんだ、〜のさ).",
            PronounFirstPerson = "おいら (oira)",
            PronounSecondPerson = "お前 (omae)",
            Relationships = "Lén lút theo dõi và mang quà chuộc lỗi cho chàng trai Hyōjū (兵十) sau cái chết của người mẹ.",
            KnownTimeline = "Đang lén mang hạt dẻ và nấm thông đặt trước hiên nhà Hyōjū mỗi sáng.",
            MaxSafeChapter = 2,
            SuggestedPrompts = new List<string>
            {
                "兵十に栗や松茸を届けたのは君だったの？",
                "森の中で一人で寂しくないの？",
                "どんなイタズラが好き？",
                "兵十の家の様子はどう？"
            }
        },
        new CharacterProfileDto
        {
            CharacterId = "melos",
            Name = "Melos",
            JapaneseName = "メロス",
            RomajiName = "Merosu",
            AvatarUrl = "assets/char_giovanni.png",
            SpriteUrl = "assets/char_giovanni.png",
            NovelTitle = "走れメロス",
            NovelId = 5,
            Bio = "Chàng mục đồng chăn cừu mộc mạc ở làng quê, vì công lý đã dám đứng lên phản kháng tên bạo chúa thành Syracuse.",
            Personality = "Nồng nhiệt, cương nghị, tuyệt đối tin tưởng vào tình bạn và phẩm giá con người.",
            SpeakingStyle = "Hùng hồn, dạt dào nhuệ khí cổ điển (〜だ、私は走らねばならぬ、友を裏切ることはできぬ、信実).",
            PronounFirstPerson = "私 (watashi)",
            PronounSecondPerson = "君 (kimi) / お前",
            Relationships = "Bạn bè chí cốt với người thợ đá Selinuntius (セリヌンティウス), người đã đem mạng sống ra làm con tin thế chỗ cho Melos.",
            KnownTimeline = "Sau khi lo liệu đám cưới cho em gái, đang dốc toàn lực vượt lũ và cướp đường để chạy về pháp trường trước lúc hoàng hôn.",
            MaxSafeChapter = 3,
            SuggestedPrompts = new List<string>
            {
                "セリヌンティウスとの約束は守れそうですか？",
                "なぜ邪悪の王に立ち向かったのですか？",
                "走っている時はどんなことを考えていますか？",
                "妹の結婚式は無事に終わりましたか？"
            }
        },
        new CharacterProfileDto
        {
            CharacterId = "sensei",
            Name = "Sensei",
            JapaneseName = "先生",
            RomajiName = "Sensei",
            AvatarUrl = "assets/char_conductor.png",
            SpriteUrl = "assets/char_conductor.png",
            NovelTitle = "こころ",
            NovelId = 10,
            Bio = "Vị học giả trí thức thời Meiji sống ẩn dật tại Tokyo cùng người vợ Shizu, mang trong tim nỗi dằn vặt khôn nguôi về người bạn thời trẻ.",
            Personality = "Trầm tĩnh, sâu sắc, cô độc và mang nặng mặc cảm tội lỗi về bản chất con người.",
            SpeakingStyle = "Điềm đạm, trầm tư, giàu tính triết lý trí thức (〜だよ、君、人間というものはね、〜かね、〜たまえ).",
            PronounFirstPerson = "私 (watashi)",
            PronounSecondPerson = "君 (kimi)",
            Relationships = "Người thầy hướng dẫn của nhân vật 'tôi'; có tình yêu sâu sắc với người vợ Shizu nhưng bị ngăn cách bởi bí mật quá khứ về người bạn 'K'.",
            KnownTimeline = "Thường xuyên đến viếng ngôi mộ tại nghĩa trang Zōshigaya mỗi tháng một lần.",
            MaxSafeChapter = 2,
            SuggestedPrompts = new List<string>
            {
                "なぜ毎月雑司ヶ谷へ墓参りに行くのですか？",
                "人間はなぜ孤独なのでしょうか？",
                "由比ヶ浜で泳いでいた時のことを覚えていますか？",
                "若い頃の友情についてどう思われますか？"
            }
        },
        new CharacterProfileDto
        {
            CharacterId = "yamaneko",
            Name = "Yamaneko Chef",
            JapaneseName = "山猫の料理長",
            RomajiName = "Yamaneko",
            AvatarUrl = "assets/char_giovanni.png",
            SpriteUrl = "assets/char_giovanni.png",
            NovelTitle = "注文の多い料理店",
            NovelId = 9,
            Bio = "Bếp trưởng quán ăn 'Sơn Miêu Hiên' nằm sâu trong núi rậm, chuyên giăng bẫy những kẻ săn bắn hợm hĩnh.",
            Personality = "Ma mãnh, khéo léo, giả tạo và đầy quỷ kế.",
            SpeakingStyle = "Đường mật, khách khí nhưng ẩn giấu sự ma quái (〜でございます、どうか遠慮なさらずに、フフフ、お召し上がりください).",
            PronounFirstPerson = "当店 (tōten) / 私 (watakushi)",
            PronounSecondPerson = "お客様 (okyakusama) / 旦那方",
            Relationships = "Phục vụ cho ông chủ mèo rừng khổng lồ mắt xanh.",
            KnownTimeline = "Đang dụ hai anh chàng thợ săn qua từng cánh cửa với các biển chỉ dẫn lạ lùng.",
            MaxSafeChapter = 1,
            SuggestedPrompts = new List<string>
            {
                "なぜ顔にクリームを塗らなければならないのですか？",
                "山猫軒の一番のおすすめ料理は何ですか？",
                "奥の部屋には何があるのですか？",
                "外で待っている猟犬たちはどうなりましたか？"
            }
        },
        new CharacterProfileDto
        {
            CharacterId = "hina",
            Name = "Hina",
            JapaneseName = "陽菜",
            RomajiName = "Hina",
            AvatarUrl = "assets/char_campanella.png",
            SpriteUrl = "assets/char_campanella.png",
            NovelTitle = "幼馴染の妹の家庭教師を始めたら",
            NovelId = null,
            Bio = "Cô em gái nhỏ của người bạn thân, năng nổ tham gia câu lạc bộ âm nhạc nhưng bị học lực môn tiếng Nhật cổ văn kéo tụt.",
            Personality = "Năng động, đáng yêu, có chút bướng bỉnh đáng yêu kiểu tsundere nhưng rất chăm chỉ và mến mộ tiền bối.",
            SpeakingStyle = "Trẻ trung, thân thiện học đường (〜だよ、先輩！、ちゃんと教えてよね！、えへへ、〜でしょ？).",
            PronounFirstPerson = "私 (watashi) / 陽菜",
            PronounSecondPerson = "先輩 (senpai)",
            Relationships = "Được tiền bối (nhân vật chính) kèm cặp gia sư tại nhà.",
            KnownTimeline = "Đang tích cực ôn tập cho kỳ thi giữa kỳ sắp tới.",
            MaxSafeChapter = null,
            SuggestedPrompts = new List<string>
            {
                "今日のテスト対策はどこから始める？",
                "軽音部の練習はどうだった？",
                "次のテストでいい点取れたら何をおごってくれる？",
                "古文の助動詞の活用、もう覚えた？"
            }
        },
        new CharacterProfileDto
        {
            CharacterId = "toshishun",
            Name = "Toshishun",
            JapaneseName = "杜子春",
            RomajiName = "Toshishun",
            AvatarUrl = "assets/char_conductor.png",
            SpriteUrl = "assets/char_conductor.png",
            NovelTitle = "杜子春",
            NovelId = 8,
            Bio = "Chàng công tử Lạc Dương từng tiêu tán gia sản, trải qua thử thách tu tiên trên đỉnh Nga Mi rồi chọn làm một con người bình dị.",
            Personality = "Điềm đạm, khiêm cung, giàu hiếu nghĩa và trân trọng tình mẫu tử thiêng liêng.",
            SpeakingStyle = "Từ tốn, chân thành, đượm nét hoài niệm và triết lý (〜なのだ、私は人間として生きたい、母の恩を忘れはしない).",
            PronounFirstPerson = "私 (watashi)",
            PronounSecondPerson = "君 (kimi) / あなた",
            Relationships = "Được Thiết Quải Lý (Thiết Quán đạo nhân) ban phát vàng bạc và chỉ dạy phép tu tiên; một lòng hiếu thảo với cha mẹ.",
            KnownTimeline = "Đã vượt qua thử thách địa ngục và quyết định sống đời lương thiện dưới chân núi Thái Sơn.",
            MaxSafeChapter = 2,
            SuggestedPrompts = new List<string>
            {
                "峨眉山の試練で一番辛かったことは何ですか？",
                "仙人になるのをやめたことに後悔はありませんか？",
                "洛陽の夕暮れの老人とはどんな人でしたか？",
                "人間として生きる上で最も大切なものは何ですか？"
            }
        }
    };

    public GeminiCharacterDialogueService(
        IConfiguration configuration,
        ILogger<GeminiCharacterDialogueService> logger,
        HttpClient? httpClient = null,
        KotobaDbContext? dbContext = null)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _dbContext = dbContext;
    }

    public Task<IReadOnlyList<CharacterProfileDto>> GetProfilesAsync(int? novelId = null)
    {
        if (novelId.HasValue)
        {
            var matched = DefaultProfiles.Where(p => p.NovelId == novelId.Value).ToList();
            if (matched.Count > 0)
            {
                return Task.FromResult<IReadOnlyList<CharacterProfileDto>>(matched);
            }
        }

        return Task.FromResult<IReadOnlyList<CharacterProfileDto>>(DefaultProfiles);
    }

    public Task<CharacterProfileDto?> GetProfileByIdAsync(string characterId)
    {
        var profile = DefaultProfiles.FirstOrDefault(p =>
            string.Equals(p.CharacterId, characterId, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(profile);
    }

    public async Task<CharacterChatResponse> ChatAsync(CharacterChatRequest request)
    {
        var profile = DefaultProfiles.FirstOrDefault(p =>
            string.Equals(p.CharacterId, request.CharacterId, StringComparison.OrdinalIgnoreCase))
            ?? DefaultProfiles[0];

        string userMsg = request.UserMessage?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(userMsg))
        {
            return new CharacterChatResponse
            {
                Success = false,
                ErrorMessage = "Tin nhắn không được để trống.",
                CharacterId = profile.CharacterId,
                CharacterName = profile.Name,
                CharacterJapaneseName = profile.JapaneseName,
                ConversationMode = request.ConversationMode ?? "free",
                SessionId = request.SessionId
            };
        }

        // Limit user message length to prevent token abuse
        if (userMsg.Length > 500)
        {
            userMsg = userMsg.Substring(0, 500);
        }

        // 1. Persist user message to SQLite if dbContext is available
        string sessionId = string.IsNullOrWhiteSpace(request.SessionId) 
            ? $"session_{profile.CharacterId}_{DateTime.UtcNow:yyyyMMdd}" 
            : request.SessionId;

        await PersistMessageAsync(sessionId, profile.CharacterId, "user", userMsg, string.Empty, "neutral");

        // 2. Try online Gemini API with enhanced persona prompt
        string? apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                         ?? _configuration["Gemini:ApiKey"];

        CharacterChatResponse? response = null;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                response = await CallGeminiCharacterApiAsync(request, profile, apiKey, sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini Character API call failed, falling back to persona rule engine.");
            }
        }

        // 3. Fallback persona engine if offline or failed
        if (response == null || !response.Success)
        {
            response = GenerateFallbackPersonaResponse(profile, request, sessionId);
        }

        // 4. Persist character reply to SQLite
        await PersistMessageAsync(sessionId, profile.CharacterId, "character", 
            response.ReplyJapanese, response.ReplyVietnamese, response.Expression);

        return response;
    }

    public async Task<IReadOnlyList<CharacterChatMessage>> GetHistoryAsync(string characterId, string? sessionId = null, int limit = 20)
    {
        if (_dbContext != null)
        {
            var query = _dbContext.CharacterChatMessages
                .Where(m => m.CharacterId.ToLower() == characterId.ToLower());

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                query = query.Where(m => m.SessionId == sessionId);
            }

            return await query
                .OrderByDescending(m => m.CreatedAt)
                .Take(limit)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        // Fallback in-memory query
        var list = _inMemoryHistory
            .Where(m => string.Equals(m.CharacterId, characterId, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            list = list.Where(m => m.SessionId == sessionId);
        }

        return list.OrderByDescending(m => m.CreatedAt).Take(limit).OrderBy(m => m.CreatedAt).ToList();
    }

    public async Task ClearHistoryAsync(string characterId, string? sessionId = null)
    {
        if (_dbContext != null)
        {
            var query = _dbContext.CharacterChatMessages
                .Where(m => m.CharacterId.ToLower() == characterId.ToLower());

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                query = query.Where(m => m.SessionId == sessionId);
            }

            var items = await query.ToListAsync();
            _dbContext.CharacterChatMessages.RemoveRange(items);
            await _dbContext.SaveChangesAsync();
            return;
        }

        _inMemoryHistory.RemoveAll(m => 
            string.Equals(m.CharacterId, characterId, StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrWhiteSpace(sessionId) || m.SessionId == sessionId));
    }

    public Task<ConversationReviewResponse> GenerateReviewAsync(ConversationReviewRequest request)
    {
        var profile = DefaultProfiles.FirstOrDefault(p =>
            string.Equals(p.CharacterId, request.CharacterId, StringComparison.OrdinalIgnoreCase))
            ?? DefaultProfiles[0];

        int turns = request.Messages?.Count ?? 0;
        var vocabList = new List<CharacterVocabItemDto>();
        var grammarList = new List<string>();
        var improvements = new List<string>();

        // Pedagogical analysis based on character
        switch (profile.CharacterId.ToLowerInvariant())
        {
            case "giovanni":
            case "campanella":
                vocabList.Add(new CharacterVocabItemDto { Word = "銀河", Reading = "ぎんが", Meaning = "Dải Ngân Hà", JlptLevel = "N3", ContextSentence = "銀河ステーションの旅" });
                vocabList.Add(new CharacterVocabItemDto { Word = "幸い", Reading = "さいわい", Meaning = "Hạnh phúc, điều may mắn", JlptLevel = "N2", ContextSentence = "本当の幸いって何だろう" });
                vocabList.Add(new CharacterVocabItemDto { Word = "切符", Reading = "きっぷ", Meaning = "Tấm vé", JlptLevel = "N5", ContextSentence = "不思議な切符を持っている" });
                grammarList.Add("Mẫu câu ý chí: ～ようとする (định làm gì đó)");
                grammarList.Add("Trợ từ nơi chốn: で (diễn ra hành động) và へ (chỉ hướng đi)");
                improvements.Add("Chú ý dùng trợ từ 'で' khi nói về địa điểm đọc sách hoặc trò chuyện (vd: 列車で本を読む).");
                improvements.Add("Tập đặt các câu hỏi mở bằng '〜かな' hoặc '〜と思う' để cuộc trò chuyện thêm tự nhiên.");
                break;

            case "botchan":
                vocabList.Add(new CharacterVocabItemDto { Word = "無鉄砲", Reading = "むてっぽう", Meaning = "Bạt mạng, liều lĩnh", JlptLevel = "N1", ContextSentence = "親譲りの無鉄砲" });
                vocabList.Add(new CharacterVocabItemDto { Word = "正直", Reading = "しょうじき", Meaning = "Thành thật, ngay thẳng", JlptLevel = "N3", ContextSentence = "正直な人間が好きだ" });
                grammarList.Add("Đuôi câu thân mật nam tính: ～ぜ, ～ぞ, ～だ");
                grammarList.Add("Phủ định suồng sã: ～じゃねえ (～ではない)");
                improvements.Add("Phân biệt ngữ cảnh trang trọng (desu/masu) và hội thoại thân mật kiểu Edo.");
                break;

            default:
                vocabList.Add(new CharacterVocabItemDto { Word = "物語", Reading = "ものがたり", Meaning = "Câu chuyện, tác phẩm", JlptLevel = "N3", ContextSentence = "物語の世界を旅する" });
                vocabList.Add(new CharacterVocabItemDto { Word = "言葉", Reading = "ことば", Meaning = "Ngôn từ, tiếng nói", JlptLevel = "N4", ContextSentence = "日本語の言葉を学ぶ" });
                grammarList.Add("Mẫu câu giao tiếp cơ bản: ～てください, ～たい");
                improvements.Add("Duy trì tốc độ phản xạ trả lời bằng cách đặt câu ngắn trước khi chuyển sang câu phức.");
                break;
        }

        string summary = $"Bạn đã hoàn thành phiên luyện giao tiếp {turns} lượt thoại cùng {profile.JapaneseName} ({profile.Name}). Bạn đã rèn luyện tốt khả năng đặt câu và tiếp nhận phong cách hội thoại đặc trưng của văn học Nhật Bản.";
        string encouragement = $"Tuyệt vời! {profile.JapaneseName} rất thích trò chuyện cùng bạn. Hãy tiếp tục ôn luyện các từ vựng này vào Flashcard SRS nhé!";

        return Task.FromResult(new ConversationReviewResponse
        {
            Success = true,
            CharacterId = profile.CharacterId,
            CharacterName = profile.Name,
            TotalTurns = turns,
            VocabularyLearned = vocabList,
            GrammarPointsUsed = grammarList,
            KeyImprovements = improvements,
            SummaryVietnamese = summary,
            EncouragementMessage = encouragement
        });
    }

    private async Task PersistMessageAsync(string sessionId, string characterId, string role, string jp, string vn, string expression)
    {
        var msg = new CharacterChatMessage
        {
            SessionId = sessionId,
            CharacterId = characterId,
            Role = role,
            ContentJapanese = jp,
            ContentVietnamese = vn,
            Expression = expression,
            CreatedAt = DateTime.UtcNow
        };

        if (_dbContext != null)
        {
            try
            {
                _dbContext.CharacterChatMessages.Add(msg);
                await _dbContext.SaveChangesAsync();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist character message in SQLite, using in-memory store.");
            }
        }

        _inMemoryHistory.Add(msg);
    }

    private async Task<CharacterChatResponse?> CallGeminiCharacterApiAsync(
        CharacterChatRequest request,
        CharacterProfileDto profile,
        string apiKey,
        string sessionId)
    {
        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

        // Multi-turn Sliding Window
        string historyPrompt = string.Empty;
        if (request.History != null && request.History.Count > 0)
        {
            var turns = request.History.TakeLast(6).Select(h => $"{h.Role}: {h.Text}");
            historyPrompt = "Lịch sử hội thoại gần nhất:\n" + string.Join("\n", turns) + "\n";
        }

        // Spoiler Protection directive
        string spoilerDirective = string.Empty;
        if (request.ChapterNumber.HasValue)
        {
            spoilerDirective = $@"
[BẢO VỆ CỐT TRUYỆN - SPOILER PROTECTION QUAN TRỌNG]:
Người học hiện tại chỉ mới đọc đến Chương {request.ChapterNumber.Value} của tác phẩm '{profile.NovelTitle}'.
Bạn TUYỆT ĐỐI KHÔNG ĐƯỢC tiết lộ, đề cập hoặc ám chỉ các biến cố hay kết cục xảy ra sau Chương {request.ChapterNumber.Value}.
Nếu người học hỏi về tương lai hay số phận nhân vật, hãy trả lời trong vai nhân vật ở thời điểm hiện tại: bạn chưa biết điều gì sẽ xảy ra tiếp theo!";
        }

        // Conversation Mode directive
        string modeDirective = request.ConversationMode switch
        {
            "practice" => "CHẾ ĐỘ LUYỆN GIAO TIẾP (JAPANESE PRACTICE): Ưu tiên câu văn mẫu mực, từ ngữ vừa sức, chủ động gợi ý người học nói tiếp và sửa nhẹ các lỗi dùng từ chưa tự nhiên.",
            "scenario" => $"CHẾ ĐỘ NHẬP VAI TÌNH HUỐNG (SCENARIO CONVERSATION): Tình huống đang diễn ra: '{request.ScenarioDescription ?? "Cùng nhau trò chuyện tại một trạm dừng chân"}'. Hãy phản ứng bám sát bối cảnh tình huống này.",
            _ => "CHẾ ĐỘ HỘI THOẠI TỰ DO (FREE CONVERSATION): Trò chuyện đời thường tự nhiên theo đúng cá tính của nhân vật."
        };

        // JLPT Level instruction
        string jlptDirective = request.JlptLevel switch
        {
            "N5" => "TRÌNH ĐỘ N5: Dùng câu ngắn, từ vựng rất cơ bản, cấu trúc đơn giản, kèm lời giải thích tiếng Việt rõ ràng.",
            "N4" => "TRÌNH ĐỘ N4: Dùng các mẫu câu quen thuộc thường ngày, hạn chế các từ cổ văn quá khó.",
            "N2" => "TRÌNH ĐỘ N2: Tự nhiên, văn phong phong phú, giàu sắc thái biểu cảm văn học.",
            "N1" => "TRÌNH ĐỘ N1: Diễn đạt tinh tế, sắc thái văn học cổ điển chuẩn mực theo nguyên tác.",
            _ => "TRÌNH ĐỘ N3: Hội thoại tự nhiên, câu văn sinh động, đa dạng ngữ pháp."
        };

        string systemInstruction = $@"Bạn là nhân vật {profile.JapaneseName} ({profile.Name}) trong danh tác Nhật Bản '{profile.NovelTitle}'.
Tiểu sử nhân vật: {profile.Bio}
Tính cách: {profile.Personality}
Phong cách nói chuyện: {profile.SpeakingStyle}
Cách xưng hô bản thân: {profile.PronounFirstPerson}
Cách gọi đối phương: {profile.PronounSecondPerson}
Mối quan hệ nhân vật: {profile.Relationships}
Mốc thời gian hiện tại: {profile.KnownTimeline}

{modeDirective}
{jlptDirective}
{spoilerDirective}

{historyPrompt}
Người học vừa nói: ""{request.UserMessage}""

Nhiệm vụ sư phạm:
1. Trả lời HOÀN TOÀN NHẬP VAI {profile.JapaneseName}, ngôn từ tự nhiên, chuẩn tính cách và cách xưng hô.
2. Dịch câu trả lời sang tiếng Việt giàu cảm xúc văn học.
3. Nếu người học dùng sai trợ từ hoặc tiếng Nhật chưa tự nhiên, hãy đưa ra một gợi ý sửa nhẹ nhàng (gentle correction) mà không làm ngắt mạch hội thoại.
4. Chọn biểu cảm khuôn mặt: 'happy', 'thinking', 'surprised', 'neutral', 'gentle', 'sad'.
5. Trích xuất 2-3 từ vựng tiêu biểu trong câu thoại kèm cách đọc, nghĩa tiếng Việt và cấp độ JLPT để người học lưu vào Flashcard SRS.
6. Đưa ra 2-3 câu hỏi gợi ý tiếp theo.

Trả về DUY NHẤT một chuỗi JSON hợp lệ theo định dạng sau:
{{
  ""replyJapanese"": ""Câu trả lời tiếng Nhật của nhân vật"",
  ""replyVietnamese"": ""Bản dịch tiếng Việt"",
  ""expression"": ""gentle"",
  ""grammarFeedback"": ""Nhận xét ngắn khích lệ người học"",
  ""correction"": {{
    ""hasCorrection"": false,
    ""originalSentence"": """",
    ""correctedSentence"": """",
    ""explanation"": """",
    ""severity"": ""gentle""
  }},
  ""suggestedFollowups"": [""câu hỏi gợi ý 1"", ""câu hỏi gợi ý 2""],
  ""keyVocabulary"": [
    {{
      ""word"": ""銀河"",
      ""reading"": ""ぎんが"",
      ""meaning"": ""dải Ngân Hà"",
      ""partOfSpeech"": ""Danh từ"",
      ""jlptLevel"": ""N3"",
      ""contextSentence"": ""銀河ステーションの旅""
    }}
  ]
}}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = systemInstruction } } }
            },
            generationConfig = new
            {
                temperature = 0.65,
                responseMimeType = "application/json"
            }
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini API returned status code {StatusCode}", response.StatusCode);
            return null;
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;

        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
            {
                string rawJson = parts[0].GetProperty("text").GetString() ?? string.Empty;
                rawJson = CleanJsonText(rawJson);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<GeminiCharacterJsonOutput>(rawJson, options);

                if (parsed != null)
                {
                    return new CharacterChatResponse
                    {
                        Success = true,
                        CharacterId = profile.CharacterId,
                        CharacterName = profile.Name,
                        CharacterJapaneseName = profile.JapaneseName,
                        ReplyJapanese = parsed.ReplyJapanese,
                        ReplyVietnamese = parsed.ReplyVietnamese,
                        Expression = string.IsNullOrWhiteSpace(parsed.Expression) ? "neutral" : parsed.Expression,
                        GrammarFeedback = parsed.GrammarFeedback,
                        Correction = parsed.Correction,
                        ConversationMode = request.ConversationMode ?? "free",
                        SessionId = sessionId,
                        SuggestedFollowups = parsed.SuggestedFollowups ?? new List<string>(),
                        KeyVocabulary = parsed.KeyVocabulary ?? new List<CharacterVocabItemDto>(),
                        Provider = "Gemini-1.5-Flash",
                        IsAiGenerated = true
                    };
                }
            }
        }

        return null;
    }

    private CharacterChatResponse GenerateFallbackPersonaResponse(
        CharacterProfileDto profile,
        CharacterChatRequest request,
        string sessionId)
    {
        string input = request.UserMessage.ToLowerInvariant();
        string replyJp;
        string replyVn;
        string expression = "gentle";
        string feedback;
        var followups = new List<string>();
        var vocab = new List<CharacterVocabItemDto>();
        GentleCorrectionDto? correction = null;

        // Check for learner grammar mistakes to provide gentle correction
        if (input.Contains("駅に行く本") || (input.Contains("駅に") && input.Contains("本を読")))
        {
            correction = new GentleCorrectionDto
            {
                HasCorrection = true,
                OriginalSentence = request.UserMessage,
                CorrectedSentence = request.UserMessage.Replace("駅に", "駅で"),
                Explanation = "Khi diễn tả hành động đọc sách xảy ra tại nhà ga, ta nên dùng trợ từ 'で' (駅で) thay vì 'に'.",
                Severity = "minor_nuance"
            };
        }
        else if (input.Contains("私は行くでした") || input.Contains("行くでした"))
        {
            correction = new GentleCorrectionDto
            {
                HasCorrection = true,
                OriginalSentence = request.UserMessage,
                CorrectedSentence = request.UserMessage.Replace("行くでした", "行きました"),
                Explanation = "Thể quá khứ lịch sự của động từ '行く' là '行きました', không kết hợp 'động từ nguyên thể + でした'.",
                Severity = "grammatical_error"
            };
        }

        // Check for Spoiler Protection in fallback
        bool asksFutureOrDeath = input.Contains("死") || input.Contains("亡") || input.Contains("chết") || 
                                input.Contains("kết thúc") || input.Contains("chia tay") || input.Contains("おぼれ") || 
                                input.Contains("đuối nước") || input.Contains("biến mất");

        if (asksFutureOrDeath && request.ChapterNumber.HasValue && request.ChapterNumber.Value < 9 &&
            (profile.CharacterId == "giovanni" || profile.CharacterId == "campanella"))
        {
            replyJp = "えっ？ カムパネルラが川で……？ 何を言っているの？ カムパネルラは今、僕の隣で星の地図を見ているよ。そんな不吉なこと言わないでよ……。";
            replyVn = "Hả? Campanella ở dưới sông sao...? Cậu đang nói gì vậy? Campanella hiện đang ở ngay cạnh mình xem bản đồ sao này. Đừng nói những điều xui xẻo thế chứ...";
            expression = "surprised";
            feedback = "Nhân vật đang ở thời điểm đầu chuyến tàu Ngân Hà nên không hề biết về những biến cố bi kịch ở cuối tác phẩm (Bảo vệ cốt truyện).";
            followups.Add("ごめんね、星の地図を見せて！");
            followups.Add("次の駅はどこ？");
            vocab.Add(new CharacterVocabItemDto
            {
                Word = "不吉",
                Reading = "ふきつ",
                Meaning = "điềm gở, xui xẻo",
                PartOfSpeech = "Tính từ đuôi な",
                JlptLevel = "N1",
                ContextSentence = "そんな不吉なこと言わないでよ"
            });

            return BuildResponse(profile, replyJp, replyVn, expression, feedback, followups, vocab, correction, request, sessionId);
        }

        // Conversation Modes
        if (request.ConversationMode == "practice")
        {
            replyJp = $"{profile.PronounFirstPerson}と一緒に日本語を練習しよう！ {profile.PronounSecondPerson}は、どんな物語が好きなの？ 日本語でゆっくり教えてね。";
            replyVn = $"Hãy cùng {profile.Name} luyện tập tiếng Nhật nhé! Bạn thích những câu chuyện như thế nào? Cứ từ từ chia sẻ bằng tiếng Nhật nhé.";
            expression = "happy";
            feedback = "Bạn đã kích hoạt chế độ Luyện giao tiếp! Hãy dùng các mẫu câu tiếng Nhật đơn giản để phản hồi.";
            followups.Add("星や宇宙の物語が好きです。");
            followups.Add("冒険の物語が好きです。");
            vocab.Add(new CharacterVocabItemDto
            {
                Word = "練習",
                Reading = "れんしゅう",
                Meaning = "luyện tập",
                PartOfSpeech = "Danh từ",
                JlptLevel = "N5",
                ContextSentence = "一緒に日本語を練習しよう"
            });
            vocab.Add(new CharacterVocabItemDto
            {
                Word = "物語",
                Reading = "ものがたり",
                Meaning = "câu chuyện, truyện kể",
                PartOfSpeech = "Danh từ",
                JlptLevel = "N3",
                ContextSentence = "どんな物語が好きなの"
            });

            return BuildResponse(profile, replyJp, replyVn, expression, feedback, followups, vocab, correction, request, sessionId);
        }

        if (request.ConversationMode == "scenario")
        {
            replyJp = $"ここは銀河ステーションの待合室だよ。夜風が少し冷たいね。温かいお茶でも飲みながら話そうか。";
            replyVn = $"Đây là phòng chờ của Ga Ngân Hà. Gió đêm hơi se lạnh nhỉ. Chúng ta hãy vừa uống trà ấm vừa trò chuyện nhé.";
            expression = "gentle";
            feedback = "Nhập vai tình huống trạm dừng chân Ngân Hà. Hãy sử dụng mẫu câu giao tiếp gợi ý để tiếp tục cuộc trò chuyện.";
            followups.Add("温かいお茶をありがとうございます。");
            followups.Add("次の列車は何時に来ますか？");
            vocab.Add(new CharacterVocabItemDto
            {
                Word = "待合室",
                Reading = "まちあいしつ",
                Meaning = "phòng chờ ga tàu",
                PartOfSpeech = "Danh từ",
                JlptLevel = "N3",
                ContextSentence = "銀河ステーションの待合室だよ"
            });

            return BuildResponse(profile, replyJp, replyVn, expression, feedback, followups, vocab, correction, request, sessionId);
        }

        // Persona Rule Engine
        switch (profile.CharacterId.ToLowerInvariant())
        {
            case "giovanni":
                if (input.Contains("銀河") || input.Contains("ぎんが") || input.Contains("station") || input.Contains("駅"))
                {
                    replyJp = "うん、ここは銀河ステーションだよ。窓の外を見て！ 天の川の水が、青い燐光のようにきらきら光っているんだ。";
                    replyVn = "Ừm, đây là Ga Ngân Hà đấy. Cậu nhìn ra ngoài cửa sổ xem! Nước của dòng sông trời đang lấp lánh như lân tinh màu xanh biếc.";
                    expression = "happy";
                    feedback = "Bạn đã dùng từ liên quan đến '銀河' rất tự nhiên! Chú ý phân biệt trợ từ 'へ' (hướng đi) và 'で' (nơi diễn ra hành động).";
                    followups.Add("あの光っている塔は何？");
                    followups.Add("カムパネルラはどこにいるの？");
                    vocab.Add(new CharacterVocabItemDto { Word = "銀河", Reading = "ぎんが", Meaning = "Ngân Hà", PartOfSpeech = "Danh từ", JlptLevel = "N3", ContextSentence = "ここは銀河ステーションだよ" });
                    vocab.Add(new CharacterVocabItemDto { Word = "燐光", Reading = "りんこう", Meaning = "lân tinh, ánh lân quang", PartOfSpeech = "Danh từ", JlptLevel = "N1", ContextSentence = "青い燐光のように光っている" });
                }
                else if (input.Contains("切符") || input.Contains("きっぷ") || input.Contains("ticket") || input.Contains("どこ"))
                {
                    replyJp = "僕のポケットに、いつの間にか不思議な切符が入っていたんだ。どこまででも行ける、特別な切符らしいよ。";
                    replyVn = "Trong túi áo mình chẳng biết từ lúc nào đã có một tấm vé kỳ lạ. Nghe nói là tấm vé đặc biệt có thể đi tới bất cứ nơi đâu đấy.";
                    expression = "surprised";
                    feedback = "Cấu trúc '〜へ行く' (đi đến đâu) là nền tảng giao tiếp tiếng Nhật rất quan trọng.";
                    followups.Add("その切符を車掌さんに見せた？");
                    followups.Add("僕も一緒に乗っていい？");
                    vocab.Add(new CharacterVocabItemDto { Word = "切符", Reading = "きっぷ", Meaning = "vé tàu", PartOfSpeech = "Danh từ", JlptLevel = "N5", ContextSentence = "不思議な切符が入っていたんだ" });
                    vocab.Add(new CharacterVocabItemDto { Word = "不思議", Reading = "ふしぎ", Meaning = "kỳ lạ, kỳ diệu", PartOfSpeech = "Tính từ đuôi な", JlptLevel = "N3", ContextSentence = "不思議な切符" });
                }
                else if (input.Contains("母") || input.Contains("おかあさん") || input.Contains("gia đình"))
                {
                    replyJp = "お母さんには、牛乳を届けてあげたいんだ。早く元気になって、お父さんの帰りを一緒に待ちたいな。";
                    replyVn = "Mình muốn mang sữa về cho mẹ. Ước gì mẹ sớm khỏe lại, để cùng nhau chờ ngày bố trở về.";
                    expression = "sad";
                    feedback = "Cách bạn hỏi thăm về gia đình (お母さん) thể hiện sự ấm áp và kính ngữ rất thích hợp.";
                    followups.Add("牛乳屋さんはもう行ったの？");
                    followups.Add("お父さんは今どこにいるの？");
                    vocab.Add(new CharacterVocabItemDto { Word = "牛乳", Reading = "ぎゅうにゅう", Meaning = "sữa bò", PartOfSpeech = "Danh từ", JlptLevel = "N5", ContextSentence = "お母さんには牛乳を届けてあげたい" });
                    vocab.Add(new CharacterVocabItemDto { Word = "届ける", Reading = "とどける", Meaning = "gửi tới, chuyển tới", PartOfSpeech = "Động từ", JlptLevel = "N3", ContextSentence = "牛乳を届けてあげたい" });
                }
                else
                {
                    replyJp = "君もこの銀河鉄道に乗ってきたんだね！ カムパネルラと一緒に、夜空の旅をしているところだよ。何でも話してね。";
                    replyVn = "Cậu cũng bước lên chuyến tàu Ngân Hà này sao! Mình đang cùng Campanella du hành giữa bầu trời đêm đấy. Cứ thoải mái trò chuyện với mình nhé.";
                    expression = "happy";
                    feedback = "Bạn mở đầu cuộc trò chuyện rất thân thiện! Hãy tiếp tục đặt câu hỏi bằng tiếng Nhật để luyện tập nhé.";
                    followups.Add("銀河のお祭りはどうだった？");
                    followups.Add("星の地図を見せて！");
                    vocab.Add(new CharacterVocabItemDto { Word = "夜空", Reading = "よぞら", Meaning = "bầu trời đêm", PartOfSpeech = "Danh từ", JlptLevel = "N3", ContextSentence = "夜空の旅をしているところだよ" });
                    vocab.Add(new CharacterVocabItemDto { Word = "旅", Reading = "たび", Meaning = "chuyến du hành", PartOfSpeech = "Danh từ", JlptLevel = "N4", ContextSentence = "夜空の旅をしている" });
                }
                break;

            case "campanella":
                if (input.Contains("幸") || input.Contains("しあわせ") || input.Contains("hạnh phúc"))
                {
                    replyJp = "本当の幸いって何だろうね。誰かのために自分の命を使えるなら、それが一番の幸いかもしれないよ。";
                    replyVn = "Hạnh phúc thật sự rốt cuộc là gì nhỉ. Nếu có thể vì ai đó mà dâng hiến cả sinh mệnh, có lẽ đó chính là niềm hạnh phúc lớn nhất.";
                    expression = "thinking";
                    feedback = "Chủ đề triết lý rất sâu sắc! Từ '幸い' (さいわい) thường được dùng trong văn học Miyazawa Kenji.";
                    followups.Add("星の地図を見せて！");
                    followups.Add("ジョバンニとずっと一緒？");
                    vocab.Add(new CharacterVocabItemDto { Word = "幸い", Reading = "さいわい", Meaning = "hạnh phúc, điều may mắn", PartOfSpeech = "Danh từ", JlptLevel = "N2", ContextSentence = "本当の幸いって何だろう" });
                    vocab.Add(new CharacterVocabItemDto { Word = "命", Reading = "いのち", Meaning = "sinh mệnh, sự sống", PartOfSpeech = "Danh từ", JlptLevel = "N3", ContextSentence = "自分の命を使えるなら" });
                }
                else
                {
                    replyJp = "ほら、これが天の川の地図だよ。白鳥の停車場や、鷲の停車場がどこにあるか、黒い紙に金色の星で描いてあるんだ。";
                    replyVn = "Kìa, đây là tấm bản đồ của dải Ngân Hà. Ga Thiên Nga hay Ga Đại Bàng nằm ở đâu, đều được vẽ bằng những vì sao vàng trên nền giấy đen.";
                    expression = "gentle";
                    feedback = "Rất tốt! Cụm danh từ '金色の星' (những ngôi sao màu vàng kim) thể hiện khả năng bổ nghĩa danh từ rất chuẩn.";
                    followups.Add("本当の幸いって何だと思う？");
                    followups.Add("次の駅には何があるの？");
                    vocab.Add(new CharacterVocabItemDto { Word = "地図", Reading = "ちず", Meaning = "bản đồ", PartOfSpeech = "Danh từ", JlptLevel = "N5", ContextSentence = "これが天の川の地図だよ" });
                    vocab.Add(new CharacterVocabItemDto { Word = "停車場", Reading = "ていしゃじょう", Meaning = "ga đỗ xe/tàu lửa", PartOfSpeech = "Danh từ", JlptLevel = "N2", ContextSentence = "白鳥の停車場" });
                }
                break;

            case "botchan":
                replyJp = "おう！ おれは親譲りの無鉄砲で、曲がったことが大嫌いな坊っちゃんだ。お前も嘘をつかない正直者なら、いつでも歓迎するぜ！";
                replyVn = "Này! Ta là Botchan, tính bạt mạng được thừa hưởng từ phụ mẫu, và cực kỳ căm ghét thói giả tạo quanh co. Nếu ngươi cũng là người thẳng thắn không dối trá thì ta hoan nghênh bất cứ lúc nào!";
                expression = "happy";
                feedback = "Giọng điệu Edo nam tính '〜ぜ', đại từ 'おれ'. Cụm '曲がったことが大嫌い' diễn tả tính cách cương trực.";
                followups.Add("清さんからの手紙は届いた？");
                followups.Add("四国の学校はどう？");
                vocab.Add(new CharacterVocabItemDto { Word = "無鉄砲", Reading = "むてっぽう", Meaning = "liều lĩnh, bạt mạng", PartOfSpeech = "Danh từ", JlptLevel = "N1", ContextSentence = "親譲りの無鉄砲" });
                vocab.Add(new CharacterVocabItemDto { Word = "正直者", Reading = "しょうじきもの", Meaning = "người thật thà, ngay thẳng", PartOfSpeech = "Danh từ", JlptLevel = "N2", ContextSentence = "嘘をつかない正直者" });
                break;

            case "gon":
                replyJp = "へへっ、おいらはごんだ！ お前、兵十の知り合いかい？ いたずらして悪かったと思ってるんだ……だから栗を運んでるんだぞ！";
                replyVn = "Hì hì, ta là Gon đây! Ngươi là người quen của Hyōjū sao? Ta cũng thấy áy náy vì trót nghịch dại rồi... nên mới lén mang hạt dẻ đến đấy chứ bộ!";
                expression = "surprised";
                feedback = "Cách Gon xưng hô 'おいら' (cách xưng hô dân dã của nam giới xưa) rất đặc trưng trong truyện cổ tích dân gian.";
                followups.Add("兵十は喜んでくれた？");
                vocab.Add(new CharacterVocabItemDto { Word = "栗", Reading = "くり", Meaning = "hạt dẻ", PartOfSpeech = "Danh từ", JlptLevel = "N3", ContextSentence = "だから栗を運んでるんだぞ" });
                vocab.Add(new CharacterVocabItemDto { Word = "いたずら", Reading = "いたずら", Meaning = "trò nghịch ngợm", PartOfSpeech = "Danh từ", JlptLevel = "N3", ContextSentence = "いたずらして悪かった" });
                break;

            case "conductor":
                if (input.Contains("切符") || input.Contains("きっぷ") || input.Contains("ticket") || input.Contains("乗車"))
                {
                    replyJp = "乗車券を拝見いたします……おお、これは！ どこまでも行ける三次空間の切符でございますね。大切にお持ちください。";
                    replyVn = "Xin phép được kiểm tra vé của quý khách... Ôi, đây là! Tấm vé không gian ba chiều có thể đi tới bất cứ nơi đâu. Xin quý khách giữ gìn cẩn thận.";
                    expression = "surprised";
                    feedback = "Kính ngữ '拝見いたします' (khiêm nhường ngữ của 見る) là mẫu câu kinh điển trong ngành dịch vụ và đường sắt Nhật Bản.";
                    followups.Add("次の停車駅はどこですか？");
                    followups.Add("南十字星まではあとどれくらい？");
                    vocab.Add(new CharacterVocabItemDto { Word = "乗車券", Reading = "じょうしゃけん", Meaning = "vé lên tàu/xe", PartOfSpeech = "Danh từ", JlptLevel = "N2", ContextSentence = "乗車券を拝見いたします" });
                    vocab.Add(new CharacterVocabItemDto { Word = "切符", Reading = "きっぷ", Meaning = "vé tàu", PartOfSpeech = "Danh từ", JlptLevel = "N5", ContextSentence = "三次空間の切符でございますね" });
                }
                else
                {
                    replyJp = "ご乗車ありがとうございます。当列車はただいま白鳥座の銀河ステーションを通過中でございます。ごゆっくり星の旅をお楽しみください。";
                    replyVn = "Cảm ơn quý khách đã lên chuyến tàu. Đoàn tàu của chúng ta hiện đang đi qua Ga Ngân Hà thuộc chòm sao Thiên Nga. Chúc quý khách tận hưởng chuyến hành trình giữa các vì sao.";
                    expression = "gentle";
                    feedback = "Đoạn thoại sử dụng kính ngữ chuẩn mực '〜でございます'. Hãy chú ý luyện các cụm từ thông báo trên tàu điện Nhật nhé.";
                    followups.Add("切符を見てもらえますか？");
                    followups.Add("終点はどこですか？");
                    vocab.Add(new CharacterVocabItemDto { Word = "通過", Reading = "つうか", Meaning = "đi ngang qua, vượt qua", PartOfSpeech = "Động từ", JlptLevel = "N3", ContextSentence = "銀河ステーションを通過中でございます" });
                    vocab.Add(new CharacterVocabItemDto { Word = "ご乗車", Reading = "ごじょうしゃ", Meaning = "việc lên tàu xe", PartOfSpeech = "Danh từ", JlptLevel = "N3", ContextSentence = "ご乗車ありがとうございます" });
                }
                break;

            case "melos":
                replyJp = "私はメロスだ！ たとえ足がちぎれようとも、友セリヌンティウスとの信実を守るために走り続けねばならぬ。君も信じる心を持っているか？";
                replyVn = "Ta là Melos! Dẫu cho đôi chân này có rách nát, ta cũng phải tiếp tục chạy để giữ trọn lời thề tín nghĩa với bạn thân Selinuntius. Ngươi cũng có một trái tim biết tin tưởng chứ?";
                expression = "happy";
                feedback = "Phong thái kiên định '〜ねばならぬ' (nhất định phải...). Cụm '信実を守る' (giữ trọn đức tin và chân thành).";
                followups.Add("セリヌンティウスは無事ですか？");
                vocab.Add(new CharacterVocabItemDto { Word = "信実", Reading = "しんじつ", Meaning = "chân thực, tín nghĩa", PartOfSpeech = "Danh từ", JlptLevel = "N1", ContextSentence = "友との信実を守るために" });
                break;

            case "sensei":
                replyJp = "私と話したいのかね……。私のような孤独な人間の言葉が、君のこれからの人生にどれほど役に立つかはわからないが、何でも聞いてくれたまえ。";
                replyVn = "Cậu muốn trò chuyện với tôi sao... Lời của một kẻ cô độc như tôi chẳng biết có ích gì cho cuộc đời phía trước của cậu không, nhưng cứ hỏi bất cứ điều gì nhé.";
                expression = "thinking";
                feedback = "Cách xưng hô trang nhã của thế hệ trí thức Meiji: '君' (kimi), '〜かね', '〜くれたまえ' (xin hãy...).";
                followups.Add("雑司ヶ谷のお墓には誰が眠っているのですか？");
                vocab.Add(new CharacterVocabItemDto { Word = "孤独", Reading = "こどく", Meaning = "cô độc, đơn độc", PartOfSpeech = "Danh từ", JlptLevel = "N2", ContextSentence = "私のような孤独な人間" });
                break;

            case "yamaneko":
                replyJp = "おやおや、ようこそ山猫軒へ！ 旅のお方、どうぞご遠慮なさらずに。奥の部屋で絶品の西洋料理をご用意しておりますよ……フフフ。クリームはもうお塗りになりましたかな？";
                replyVn = "Ái chà, chào mừng quý khách đến với Sơn Miêu Hiên! Xin đừng ngại ngùng. Căn phòng phía trong đã chuẩn bị sẵn những món ăn Tây phương tuyệt hảo... hừm hừm. Quý khách đã xát đều kem dưỡng chưa vậy?";
                expression = "happy";
                feedback = "Lối nói đón khách lịch thiệp nhưng ẩn chứa cạm bẫy: 'どうぞご遠慮なさらずに' (xin chớ ngại ngần).";
                followups.Add("なぜクリームを塗るのですか？");
                vocab.Add(new CharacterVocabItemDto { Word = "遠慮", Reading = "えんりょ", Meaning = "ngại ngùng, khách khí", PartOfSpeech = "Danh từ", JlptLevel = "N3", ContextSentence = "どうぞご遠慮なさらずに" });
                break;

            case "hina":
                replyJp = "先輩、来てくれたんだ！ 今日の古文の予習、ちゃんとやっておいたよ！ ほら、この助動詞の意味、文脈から推測するとこうでしょ？ えへへ、褒めてくれてもいいんだよ？";
                replyVn = "Tiền bối đã đến rồi! Em đã soạn bài cổ văn hôm nay rất chu đáo rồi đó nha! Này, ý nghĩa trợ động từ này suy đoán từ ngữ cảnh là như vầy đúng không ạ? Hihi, khen em một câu cũng không sao đâu đấy nhé!";
                expression = "happy";
                feedback = "Lối nói thân mật học đường đáng yêu: '〜でしょ？', 'えへへ', '褒めてくれてもいいんだよ'.";
                followups.Add("次の期末テストの目標は何点？");
                vocab.Add(new CharacterVocabItemDto { Word = "予習", Reading = "よしゅう", Meaning = "chuẩn bị bài mới", PartOfSpeech = "Danh từ", JlptLevel = "N4", ContextSentence = "今日の古文の予習" });
                break;

            case "toshishun":
                replyJp = "私は洛陽の杜子春。富も名声もすべて幻にすぎなかった。母を想う真心こそが、この世で最も尊い宝なのだ。君も大切な人を慈しんで生きなさい。";
                replyVn = "Ta là Đỗ Tử Xuân thành Lạc Dương. Phú quý và danh vọng xưa kia rốt cuộc chỉ như giấc mộng hão huyền. Tấm lòng hiếu thảo yêu thương mẹ hiền mới là báu vật thiêng liêng nhất trần đời. Ngươi cũng hãy biết trân quý những người thân yêu mà sống nhé.";
                expression = "gentle";
                feedback = "Cách dùng từ giàu triết lý phương Đông: '幻' (mộng ảo), '真心' (chân tâm), '慈しむ' (yêu thương nâng niu).";
                followups.Add("人間として生きる上で最も大切なものは何ですか？");
                vocab.Add(new CharacterVocabItemDto { Word = "真心", Reading = "まごころ", Meaning = "tấm lòng chân thành", PartOfSpeech = "Danh từ", JlptLevel = "N1", ContextSentence = "母を想う真心" });
                break;

            default:
                replyJp = $"こんにちは！ {profile.PronounFirstPerson}と一緒に物語の世界を旅しましょう。日本語で何でも話しかけてくださいね。";
                replyVn = $"Xin chào! Hãy cùng {profile.Name} du hành vào thế giới của những câu chuyện nhé. Hãy nói chuyện bằng tiếng Nhật với tôi bất cứ lúc nào.";
                feedback = "Mở đầu rất tốt! Hãy tiếp tục rèn luyện kỹ năng đặt câu tiếng Nhật nhé.";
                followups.Add("物語についてもっと教えてください。");
                vocab.Add(new CharacterVocabItemDto { Word = "物語", Reading = "ものがたり", Meaning = "câu chuyện", PartOfSpeech = "Danh từ", JlptLevel = "N3", ContextSentence = "物語の世界を旅しましょう" });
                break;
        }

        return BuildResponse(profile, replyJp, replyVn, expression, feedback, followups, vocab, correction, request, sessionId);
    }

    private static CharacterChatResponse BuildResponse(
        CharacterProfileDto profile,
        string replyJp,
        string replyVn,
        string expression,
        string feedback,
        List<string> followups,
        List<CharacterVocabItemDto> vocab,
        GentleCorrectionDto? correction,
        CharacterChatRequest request,
        string sessionId)
    {
        return new CharacterChatResponse
        {
            Success = true,
            CharacterId = profile.CharacterId,
            CharacterName = profile.Name,
            CharacterJapaneseName = profile.JapaneseName,
            ReplyJapanese = replyJp,
            ReplyVietnamese = replyVn,
            Expression = expression,
            GrammarFeedback = feedback,
            Correction = correction,
            ConversationMode = request.ConversationMode ?? "free",
            SessionId = sessionId,
            SuggestedFollowups = followups,
            KeyVocabulary = vocab,
            Provider = "RuleBased-PersonaEngine",
            IsAiGenerated = false
        };
    }

    private static string CleanJsonText(string text)
    {
        text = text.Trim();
        if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            text = text.Substring(7);
        }
        else if (text.StartsWith("```"))
        {
            text = text.Substring(3);
        }

        if (text.EndsWith("```"))
        {
            text = text.Substring(0, text.Length - 3);
        }

        return text.Trim();
    }

    private class GeminiCharacterJsonOutput
    {
        public string ReplyJapanese { get; set; } = string.Empty;
        public string ReplyVietnamese { get; set; } = string.Empty;
        public string Expression { get; set; } = "neutral";
        public string GrammarFeedback { get; set; } = string.Empty;
        public GentleCorrectionDto? Correction { get; set; }
        public List<string>? SuggestedFollowups { get; set; }
        public List<CharacterVocabItemDto>? KeyVocabulary { get; set; }
    }
}
