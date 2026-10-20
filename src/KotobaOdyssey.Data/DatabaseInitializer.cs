using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KotobaOdyssey.Data;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly KotobaDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(KotobaDbContext context, ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Ensuring SQLite database exists and schema is up to date...");
            await _context.Database.EnsureCreatedAsync();

            await SeedDataAsync();
            _logger.LogInformation("Database initialization and seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private async Task SeedDataAsync()
    {
        // 1. Seed User Progress if none exists
        if (!await _context.UserProgresses.AnyAsync())
        {
            var progress = new UserProgress
            {
                UserName = "旅人 (Traveler)",
                Title = "Starlight Apprentice",
                Level = 3,
                CurrentXp = 160,
                RequiredXp = 390,
                StreakDays = 5,
                LastStudyDate = DateTime.UtcNow.Date,
                TodayStudyMinutes = 25,
                TotalStudyMinutes = 410,
                TotalWordsLearned = 48,
                TotalChaptersRead = 6
            };
            _context.UserProgresses.Add(progress);
        }

        // 2. Seed Daily Quests for today
        var today = DateTime.UtcNow.Date;
        if (!await _context.DailyQuests.AnyAsync(q => q.QuestDate == today))
        {
            _context.DailyQuests.AddRange(
                new DailyQuest
                {
                    Title = "Starlight Reading",
                    Description = "Đọc truyện tiếng Nhật ít nhất 15 phút hôm nay.",
                    Type = QuestType.ReadMinutes,
                    TargetCount = 15,
                    CurrentCount = 15,
                    RewardXp = 50,
                    IsClaimed = false,
                    QuestDate = today
                },
                new DailyQuest
                {
                    Title = "Memory Echoes",
                    Description = "Ôn tập 10 thẻ từ vựng đến hạn.",
                    Type = QuestType.ReviewCards,
                    TargetCount = 10,
                    CurrentCount = 8,
                    RewardXp = 60,
                    IsClaimed = false,
                    QuestDate = today
                },
                new DailyQuest
                {
                    Title = "Lexicon Explorer",
                    Description = "Lưu 3 từ vựng mới vào kho từ cá nhân.",
                    Type = QuestType.AddVocabulary,
                    TargetCount = 3,
                    CurrentCount = 3,
                    RewardXp = 40,
                    IsClaimed = true,
                    QuestDate = today
                }
            );
        }

        // 3. Seed Achievements
        if (!await _context.Achievements.AnyAsync())
        {
            _context.Achievements.AddRange(
                new Achievement
                {
                    Title = "First Odyssey Step",
                    Description = "Đọc xong chương truyện đầu tiên.",
                    IconKey = "BookOpen",
                    IsUnlocked = true,
                    UnlockedAt = DateTime.UtcNow.AddDays(-4),
                    RewardXp = 50
                },
                new Achievement
                {
                    Title = "Constellation Scholar",
                    Description = "Đạt chuỗi học tập 5 ngày liên tiếp.",
                    IconKey = "Flame",
                    IsUnlocked = true,
                    UnlockedAt = DateTime.UtcNow.AddDays(-1),
                    RewardXp = 80
                },
                new Achievement
                {
                    Title = "Silver Lexicon",
                    Description = "Lưu trên 50 từ vựng vào bộ sưu tập.",
                    IconKey = "Star",
                    IsUnlocked = false,
                    RewardXp = 100
                },
                new Achievement
                {
                    Title = "Cosmic Storyteller",
                    Description = "Hoàn thành một tiểu thuyết dạng Visual Novel.",
                    IconKey = "Trophy",
                    IsUnlocked = false,
                    RewardXp = 150
                }
            );
        }

        // 4. Seed Novels & Chapters
        if (!await _context.Novels.AnyAsync())
        {
            var novelGinga = new Novel
            {
                Title = "銀河鉄道の夜",
                JapaneseTitle = "銀河鉄道の夜 (Ginga Tetsudou no Yoru)",
                RomajiTitle = "Night on the Galactic Railroad",
                Author = "宮沢賢治 (Miyazawa Kenji)",
                Genre = "Cosmic Fantasy",
                Description = "Tác phẩm kinh điển huyền ảo của Miyazawa Kenji. Cậu bé Giovanni bước lên chuyến tàu lửa ngân hà chạy xuyên qua các chòm sao kỳ vĩ trên bầu trời đêm cùng người bạn thân Campanella.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_ginga.png",
                JlptLevel = JlptLevel.N3,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                LastReadAt = DateTime.UtcNow.AddMinutes(-30)
            };

            novelGinga.Chapters.Add(new Chapter
            {
                ChapterNumber = 1,
                Title = "一、午後の授業 (Giờ học buổi chiều)",
                WordCount = 420,
                IsCompleted = true,
                LastReadPosition = 380,
                LastReadAt = DateTime.UtcNow.AddDays(-1),
                Content = @"「ではみなさんは、そういうふうに川だと云われたり、乳の流れたあとだと云われたりしていたこのぼんやりと白いものがほんとうは何かご承知ですか」
先生は、黒板に吊した大きな黒い星座の図の、上から下へ白くけむった銀河の帯のようなところを指しながら、みんなに問いをかけました。
カムパネルラが手をあげました。それから四五人手をあげました。ジョバンニも手をあげようとして、急いでそのままやめました。たしかにあれがみんな星だと、いつか雑誌で読んだのでしたが、このごろはジョバンニはまるで毎日教室でもねむく、本を読むひまも読む本もないので、なんだかそんなことはみんなうすれてしまったように思われたのでした。
ところが先生は早くもそれを見附けたのでした。
「ジョバンニさん。あなたはわかっているのでしょう」
ジョバンニは勢いよく立ちあがりましたが、立って見るともうはっきりとそれを答えることができませんでした。"
            });

            novelGinga.Chapters.Add(new Chapter
            {
                ChapterNumber = 2,
                Title = "二、活版所 (Xưởng in)",
                WordCount = 350,
                IsCompleted = false,
                LastReadPosition = 120,
                LastReadAt = DateTime.UtcNow.AddMinutes(-30),
                Content = @"ジョバンニは学校の門を出るとき、同じ組の七八人は家へ帰らずカムパネルラをまん中にして校庭の隅の桜の木のところに集まっていました。今夜の星祭に青いあかりを網にして川へ流す烏瓜の相談をしているのでした。
ジョバンニは手を大きく振って町の方へ歩きました。
町の家々はもうあかりがついて、店々では今夜の祭のために色とりどりの旗を吊したり、銀の紙のすずらんを飾ったりしていました。
ジョバンニはある大きな活版所に入って行きました。入口の計算台にすわっている白い上着を着た人が、ジョバンニを見ると紙切れを一枚渡しました。"
            });

            var novelBotchan = new Novel
            {
                Title = "坊っちゃん",
                JapaneseTitle = "坊っちゃん (Botchan)",
                RomajiTitle = "Master Darling",
                Author = "夏目漱石 (Natsume Soseki)",
                Genre = "School / Classic Drama",
                Description = "Tiểu thuyết châm biếm nổi tiếng của Natsume Soseki kể về một thầy giáo trẻ tính tình bộc trực, nóng nảy nhưng trọng chính nghĩa từ Tokyo đến dạy học tại trường trung học ở Shikoku.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_botchan.png",
                JlptLevel = JlptLevel.N4,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                LastReadAt = DateTime.UtcNow.AddDays(-2)
            };

            novelBotchan.Chapters.Add(new Chapter
            {
                ChapterNumber = 1,
                Title = "第一章 (Chương 1)",
                WordCount = 510,
                IsCompleted = true,
                LastReadPosition = 510,
                LastReadAt = DateTime.UtcNow.AddDays(-2),
                Content = @"親譲りの無鉄砲で小供の時から損ばかりしている。小学校に居る時分学校の二階から飛び降りて一週間ほど腰を抜かした事がある。なぜそんな無闇をしたと聞く人があるかも知れぬ。別段深い理由でもない。新築の二階から首を出していたら、同級生の一人が冗談に、いくら威張っても、そこから飛び降りる事は出来まい。弱虫やーい。と囃したからである。小使に負ぶさって帰って来た時、おやじが大きな眼をして二階ぐらいから飛び降りて腰を抜かす奴があるかと云ったから、この次は抜かさずに飛んで見せますと答えた。"
            });

            var novelGongitsune = new Novel
            {
                Title = "ごんぎつね",
                JapaneseTitle = "ごんぎつね (Gongitsune)",
                RomajiTitle = "Gon, the Little Fox",
                Author = "新美南吉 (Niimi Nankichi)",
                Genre = "Folktale / Touching",
                Description = "Câu chuyện cổ tích cảm động sâu sắc về chú cáo nhỏ tinh nghịch Gongitsune tìm cách chuộc lỗi với anh nông dân Hyoju bằng những món quà vô danh từ rừng sâu.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_gon.png",
                JlptLevel = JlptLevel.N5,
                IsFavorite = false,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                LastReadAt = null
            };

            novelGongitsune.Chapters.Add(new Chapter
            {
                ChapterNumber = 1,
                Title = "第一話 (Phần 1)",
                WordCount = 380,
                IsCompleted = false,
                LastReadPosition = 0,
                LastReadAt = null,
                Content = @"これは、わたしが小さいときに、村の茂平というおじいさんからきいたお話です。
むかしは、私たちの村のちかくの、中山というところに小さなお城があって、中山さまというおとのさまがおられたそうです。
その中山から、すこしはなれた山の中に、「ごんぎつね」という、いたずら好きの小ぎつねがいました。ごんは、ひとりぼっちのこぎつねで、しだのいっぱいしげった森の中に穴をほって住んでいました。そして、夜でも昼でも、ちかくの村へ出てきて、いたずらばかりしました。"
            });

            var novelYukiguni = new Novel
            {
                Title = "雪国",
                JapaneseTitle = "雪国 (Yukiguni)",
                RomajiTitle = "Snow Country",
                Author = "川端康成 (Kawabata Yasunari)",
                Genre = "Literature / Masterpiece",
                Description = "Kiệt tác đoạt giải Nobel của Kawabata Yasunari với văn phong trau chuốt, tinh tế, khắc họa mối tình buồn giữa Shimamura và nàng geisha Komako tại một thị trấn suối nước nóng phủ tuyết trắng xóa.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_yukiguni.png",
                JlptLevel = JlptLevel.N2,
                IsFavorite = false,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                LastReadAt = null
            };

            novelYukiguni.Chapters.Add(new Chapter
            {
                ChapterNumber = 1,
                Title = "第一章 (Chương 1)",
                WordCount = 490,
                IsCompleted = false,
                LastReadPosition = 0,
                LastReadAt = null,
                Content = @"国境の長いトンネルを抜けると雪国であった。夜の底が白くなった。信号所に汽車が止まった。
向側の座席から娘が立って来て、島村の前のガラス窓を落した。雪の冷気が流れこんだ。娘はうんと頸を乗り出して、遠くへ呼ぶように、
「駅長さん、駅長さん」
と叫んだ。
明りをさげて雪の中を足重にやって来た男は、襟巻に鼻をうずめ、耳に毛糸の帽子をかぶっていた。"
            });

            _context.Novels.AddRange(novelGinga, novelBotchan, novelGongitsune, novelYukiguni);
        }

        // 5. Seed Vocabulary items
        if (!await _context.VocabularyItems.AnyAsync())
        {
            var now = DateTime.UtcNow;
            _context.VocabularyItems.AddRange(
                new VocabularyItem
                {
                    Word = "銀河",
                    Reading = "ぎんが",
                    Meaning = "Ngân hà, dải thiên hà (Milky Way)",
                    JlptLevel = JlptLevel.N3,
                    ContextSentence = "上から下へ白くけむった銀河の帯のようなところを指しながら。",
                    SourceNovelTitle = "銀河鉄道の夜",
                    CreatedAt = now.AddDays(-5),
                    NextReviewDate = now.AddHours(-2), // Due for review!
                    IntervalDays = 1,
                    EaseFactor = 2.5,
                    Repetitions = 1
                },
                new VocabularyItem
                {
                    Word = "星座",
                    Reading = "せいざ",
                    Meaning = "Chòm sao, tinh tú",
                    JlptLevel = JlptLevel.N3,
                    ContextSentence = "黒板に吊した大きな黒い星座の図。",
                    SourceNovelTitle = "銀河鉄道の夜",
                    CreatedAt = now.AddDays(-4),
                    NextReviewDate = now.AddHours(-1), // Due for review!
                    IntervalDays = 1,
                    EaseFactor = 2.4,
                    Repetitions = 1
                },
                new VocabularyItem
                {
                    Word = "無鉄砲",
                    Reading = "むてっぽう",
                    Meaning = "Hấp tấp, liều lĩnh, không suy nghĩ tính toán",
                    JlptLevel = JlptLevel.N1,
                    ContextSentence = "親譲りの無鉄砲で小供の時から損ばかりしている。",
                    SourceNovelTitle = "坊っちゃん",
                    CreatedAt = now.AddDays(-6),
                    NextReviewDate = now.AddHours(-4), // Due for review!
                    IntervalDays = 3,
                    EaseFactor = 2.3,
                    Repetitions = 2
                },
                new VocabularyItem
                {
                    Word = "国境",
                    Reading = "こっきょう",
                    Meaning = "Biên giới quốc gia, ranh giới tỉnh",
                    JlptLevel = JlptLevel.N3,
                    ContextSentence = "国境の長いトンネルを抜けると雪国であった。",
                    SourceNovelTitle = "雪国",
                    CreatedAt = now.AddDays(-2),
                    NextReviewDate = now.AddDays(2),
                    IntervalDays = 4,
                    EaseFactor = 2.6,
                    Repetitions = 2
                },
                new VocabularyItem
                {
                    Word = "いたずら",
                    Reading = "いたずら",
                    Meaning = "Nghịch ngợm, trò đùa tai quái",
                    JlptLevel = JlptLevel.N4,
                    ContextSentence = "夜でも昼でも、ちかくの村へ出てきて、いたずらばかりしました。",
                    SourceNovelTitle = "ごんぎつね",
                    CreatedAt = now.AddDays(-3),
                    NextReviewDate = now.AddDays(1),
                    IntervalDays = 3,
                    EaseFactor = 2.5,
                    Repetitions = 2
                }
            );
        }

        await _context.SaveChangesAsync();
    }
}
