using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KotobaOdyssey.Data.Services;

public class ChapterPracticeService : IChapterPracticeService
{
    private readonly INovelRepository _novelRepository;
    private readonly IVocabularyRepository _vocabRepository;
    private readonly IUserProgressRepository _progressRepository;
    private readonly KotobaDbContext? _dbContext;
    private readonly ILogger<ChapterPracticeService> _logger;
    private readonly List<ChapterQuizAttempt> _fallbackAttempts = new();

    public ChapterPracticeService(
        INovelRepository novelRepository,
        IVocabularyRepository vocabRepository,
        IUserProgressRepository progressRepository,
        ILogger<ChapterPracticeService> logger,
        KotobaDbContext? dbContext = null)
    {
        _novelRepository = novelRepository;
        _vocabRepository = vocabRepository;
        _progressRepository = progressRepository;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<ChapterQuizDto> GetChapterQuizAsync(int novelId, int chapterNumber)
    {
        var novel = await _novelRepository.GetByIdAsync(novelId);
        var chapter = novel?.Chapters?.FirstOrDefault(c => c.ChapterNumber == chapterNumber);

        var quiz = new ChapterQuizDto
        {
            NovelId = novelId,
            NovelTitle = novel?.Title ?? "Tiểu thuyết",
            ChapterNumber = chapterNumber,
            ChapterTitle = chapter?.Title ?? $"Chương {chapterNumber}",
            JlptLevel = novel?.JlptLevel.ToString() ?? "N3"
        };

        // Populate curated questions per novel title / chapter
        string title = novel?.Title ?? "";
        if (title.Contains("銀河鉄道") || title.Contains("Ginga"))
        {
            if (chapterNumber == 1)
            {
                quiz.Questions.AddRange(GetGingaChapter1Questions());
            }
            else
            {
                quiz.Questions.AddRange(GetGingaChapter2Questions());
            }
        }
        else if (title.Contains("坊っちゃん") || title.Contains("Botchan"))
        {
            quiz.Questions.AddRange(GetBotchanQuestions());
        }
        else if (title.Contains("ごんぎつね") || title.Contains("Gon"))
        {
            quiz.Questions.AddRange(GetGongitsuneQuestions());
        }
        else if (title.Contains("メロス") || title.Contains("Melos"))
        {
            quiz.Questions.AddRange(GetMelosQuestions());
        }
        else if (title.Contains("羅生門") || title.Contains("Rashomon"))
        {
            quiz.Questions.AddRange(GetRashomonQuestions());
        }
        else if (title.Contains("雪国") || title.Contains("Yukiguni"))
        {
            quiz.Questions.AddRange(GetYukiguniQuestions());
        }
        else if (title.Contains("蜘蛛の糸") || title.Contains("Spider"))
        {
            quiz.Questions.AddRange(GetSpiderQuestions());
        }
        else if (title.Contains("こころ") || title.Contains("Kokoro"))
        {
            quiz.Questions.AddRange(GetKokoroQuestions());
        }
        else if (title.Contains("料理店") || title.Contains("山猫"))
        {
            quiz.Questions.AddRange(GetChuumonQuestions());
        }
        else if (title.Contains("家庭教師"))
        {
            quiz.Questions.AddRange(GetKateikyoushiQuestions());
        }
        else if (title.Contains("杜子春"))
        {
            quiz.Questions.AddRange(GetToshishunQuestions());
        }
        else if (title.Contains("舞姫"))
        {
            quiz.Questions.AddRange(GetMaihimeQuestions());
        }
        else
        {
            quiz.Questions.AddRange(GetGenericLiteratureQuestions(chapter?.Title ?? "Chương đọc"));
        }

        return quiz;
    }

    public async Task<QuizSubmitResult> SubmitQuizAsync(QuizSubmitRequest request)
    {
        var quiz = await GetChapterQuizAsync(request.NovelId, request.ChapterNumber);
        int total = quiz.Questions.Count;
        int correct = 0;
        int readingTotal = 0, readingCorrect = 0;
        int vocabTotal = 0, vocabCorrect = 0;
        int grammarTotal = 0, grammarCorrect = 0;
        var reviews = new List<QuestionReviewDto>();
        var suggestedVocab = new List<CharacterVocabItemDto>();

        foreach (var q in quiz.Questions)
        {
            bool hasAnswer = request.Answers.TryGetValue(q.QuestionId, out int userChoice);
            bool isCorrect = hasAnswer && userChoice == q.CorrectIndex;
            if (isCorrect) correct++;

            string cat = q.Category ?? "ReadingComprehension";
            if (cat == "Vocabulary") { vocabTotal++; if (isCorrect) vocabCorrect++; }
            else if (cat == "Grammar") { grammarTotal++; if (isCorrect) grammarCorrect++; }
            else { readingTotal++; if (isCorrect) readingCorrect++; }

            reviews.Add(new QuestionReviewDto
            {
                QuestionId = q.QuestionId,
                QuestionJp = q.QuestionJp,
                Category = cat,
                IsCorrect = isCorrect,
                UserChoiceIndex = hasAnswer ? userChoice : -1,
                CorrectIndex = q.CorrectIndex,
                Explanation = q.Explanation,
                TargetKnowledge = q.TargetGrammarOrVocab
            });

            if (!isCorrect && !string.IsNullOrWhiteSpace(q.TargetGrammarOrVocab))
            {
                suggestedVocab.Add(new CharacterVocabItemDto
                {
                    Word = q.TargetGrammarOrVocab,
                    Reading = "",
                    Meaning = $"Từ khóa trong câu hỏi đọc hiểu #{q.QuestionId}",
                    JlptLevel = quiz.JlptLevel
                });
            }
        }

        int scorePercent = total > 0 ? (int)Math.Round((double)correct / total * 100) : 100;
        bool passed = scorePercent >= 60;

        // Check if user already passed this quiz previously to prevent duplicate XP exploitation
        bool alreadyPassed = false;
        if (_dbContext != null)
        {
            alreadyPassed = await _dbContext.ChapterQuizAttempts
                .AnyAsync(a => a.NovelId == request.NovelId && a.ChapterNumber == request.ChapterNumber && a.Passed);
        }
        else
        {
            alreadyPassed = _fallbackAttempts.Any(a => a.NovelId == request.NovelId && a.ChapterNumber == request.ChapterNumber && a.Passed);
        }

        int xpEarned = 0;
        string feedback = string.Empty;

        if (passed)
        {
            if (!alreadyPassed)
            {
                // First time passing: award full XP
                xpEarned = 25 + (correct * 5);
                await _progressRepository.AddXpAsync(xpEarned);

                // Mark chapter completed in database
                if (_dbContext != null)
                {
                    var ch = await _dbContext.Chapters
                        .FirstOrDefaultAsync(c => c.NovelId == request.NovelId && c.ChapterNumber == request.ChapterNumber);
                    if (ch != null)
                    {
                        ch.IsCompleted = true;
                        ch.LastReadAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    var novel = await _novelRepository.GetByIdAsync(request.NovelId);
                    var ch = novel?.Chapters?.FirstOrDefault(c => c.ChapterNumber == request.ChapterNumber);
                    if (ch != null)
                    {
                        await _novelRepository.UpdateChapterProgressAsync(ch.Id, ch.WordCount, true);
                    }
                }

                feedback = $"Xuất sắc! Bạn đã vượt qua bài kiểm tra đọc hiểu với {correct}/{total} câu đúng (+{xpEarned} XP). Chương truyện đã được đánh dấu hoàn thành!";
            }
            else
            {
                // Already passed previously: 0 duplicate XP
                xpEarned = 0;
                feedback = $"Bạn đạt {correct}/{total} câu đúng ({scorePercent}%). Bài đọc này đã hoàn thành và nhận XP trước đó.";
            }
        }
        else
        {
            // Failed: 0 XP
            xpEarned = 0;
            feedback = $"Bạn đạt {correct}/{total} câu đúng ({scorePercent}%). Cần đạt tối thiểu 60% để vượt qua bài đọc hiểu. Hãy đọc lại chương truyện và thử sức lại nhé!";
        }

        int readingScore = readingTotal > 0 ? (int)Math.Round((double)readingCorrect / readingTotal * 100) : 100;
        int vocabScore = vocabTotal > 0 ? (int)Math.Round((double)vocabCorrect / vocabTotal * 100) : 100;
        int grammarScore = grammarTotal > 0 ? (int)Math.Round((double)grammarCorrect / grammarTotal * 100) : 100;

        // Persist attempt history
        var attempt = new ChapterQuizAttempt
        {
            NovelId = request.NovelId,
            ChapterNumber = request.ChapterNumber,
            TotalQuestions = total,
            CorrectAnswers = correct,
            ScorePercentage = scorePercent,
            Passed = passed,
            XpAwarded = xpEarned,
            ReadingScorePercent = readingScore,
            VocabScorePercent = vocabScore,
            GrammarScorePercent = grammarScore,
            AttemptedAt = DateTime.UtcNow
        };

        if (_dbContext != null)
        {
            _dbContext.ChapterQuizAttempts.Add(attempt);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            _fallbackAttempts.Add(attempt);
        }

        return new QuizSubmitResult
        {
            Success = true,
            TotalQuestions = total,
            CorrectAnswers = correct,
            ScorePercentage = scorePercent,
            XpEarned = xpEarned,
            Passed = passed,
            FeedbackSummary = feedback,
            QuestionReviews = reviews,
            SuggestedReviewVocab = suggestedVocab,
            ReadingTotal = readingTotal,
            ReadingCorrect = readingCorrect,
            VocabTotal = vocabTotal,
            VocabCorrect = vocabCorrect,
            GrammarTotal = grammarTotal,
            GrammarCorrect = grammarCorrect
        };
    }

    public async Task<LearningAnalyticsDto> GetLearningAnalyticsAsync()
    {
        var novels = await _novelRepository.GetAllAsync();
        var allChapters = novels.SelectMany(n => n.Chapters ?? Enumerable.Empty<Chapter>()).ToList();
        int totalNovels = novels.Count;
        int completedChapters = allChapters.Count(c => c.IsCompleted);
        int inProgressNovels = novels.Count(n => n.Chapters != null && n.Chapters.Any(c => c.LastReadPosition > 0));

        var latestNovel = await _novelRepository.GetLatestReadAsync();
        var latestChapter = latestNovel?.Chapters?
            .OrderByDescending(c => c.LastReadAt ?? DateTime.MinValue)
            .FirstOrDefault();

        // 2. Vocabulary & SRS Mastery
        var vocabItems = await _vocabRepository.GetAllAsync();
        int totalSaved = vocabItems.Count;
        int newWords = vocabItems.Count(v => v.Repetitions == 0);
        int masteredWords = vocabItems.Count(v => v.Repetitions >= 3 && v.IntervalDays >= 21);
        int learningWords = vocabItems.Count(v => v.Repetitions > 0 && (v.Repetitions < 3 || v.IntervalDays < 21));
        var dueVocab = await _vocabRepository.GetDueReviewsAsync();
        int dueCount = dueVocab.Count;
        double retentionRate = totalSaved > 0 
            ? Math.Round((double)(masteredWords + learningWords) / totalSaved * 100, 1) 
            : 100.0;

        // 3. Quiz Attempts
        List<ChapterQuizAttempt> attempts = new();
        if (_dbContext != null)
        {
            attempts = await _dbContext.ChapterQuizAttempts
                .OrderByDescending(a => a.AttemptedAt)
                .ToListAsync();
        }
        else
        {
            attempts = _fallbackAttempts.OrderByDescending(a => a.AttemptedAt).ToList();
        }

        int totalAttempts = attempts.Count;
        int passedCount = attempts.Count(a => a.Passed);
        int avgScore = totalAttempts > 0 ? (int)Math.Round(attempts.Average(a => a.ScorePercentage)) : 0;
        int totalQuestions = attempts.Sum(a => a.TotalQuestions);
        int totalCorrect = attempts.Sum(a => a.CorrectAnswers);
        int overallAccuracy = totalQuestions > 0 ? (int)Math.Round((double)totalCorrect / totalQuestions * 100) : 0;

        // 4. User Progress
        var userProgress = await _progressRepository.GetProgressAsync();

        // 5. Strengths & Weaknesses
        var strengths = new List<string>();
        var weaknesses = new List<string>();
        var recommendations = new List<string>();

        if (masteredWords > 0)
        {
            strengths.Add($"Đã làm chủ vững chắc {masteredWords} từ vựng văn học trong trí nhớ dài hạn (SRS).");
        }
        else if (totalSaved > 0)
        {
            strengths.Add($"Đang tích cực tích lũy từ vựng với {totalSaved} từ lưu từ các chương sách.");
        }
        else
        {
            strengths.Add("Sẵn sàng khám phá kho từ vựng văn học Nhật Bản.");
        }

        if (avgScore >= 75)
        {
            strengths.Add($"Năng lực đọc hiểu cốt truyện tốt (Điểm trung bình quiz đạt {avgScore}%).");
        }
        else if (completedChapters > 0)
        {
            strengths.Add($"Đã hoàn thành xuất sắc {completedChapters} chương truyện kinh điển.");
        }

        if (userProgress.StreakDays >= 3)
        {
            strengths.Add($"Duy trì chuỗi học tập bền bỉ {userProgress.StreakDays} ngày liên tiếp.");
        }

        if (dueCount > 0)
        {
            weaknesses.Add($"Có {dueCount} thẻ từ vựng đến hạn ôn tập cần xử lý để không bị quên.");
            recommendations.Add($"1. Hoàn thành phiên ôn tập {dueCount} từ vựng Flashcard SRS hôm nay.");
        }
        else
        {
            recommendations.Add("1. Tất cả thẻ SRS đã ôn xong! Duy trì phong độ tuyệt vời.");
        }

        if (newWords >= 5)
        {
            weaknesses.Add($"Có {newWords} từ vựng mới lưu chưa trải qua lượt ôn tập củng cố đầu tiên.");
        }

        if (totalAttempts == 0)
        {
            weaknesses.Add("Chưa làm bài Chapter Quiz nào để kiểm tra khả năng đọc hiểu.");
            recommendations.Add($"2. Hãy thử sức làm bài Quiz đọc hiểu chương 1 của tác phẩm '{latestNovel?.Title ?? "銀河鉄道の夜"}'.");
        }
        else
        {
            int avgReadingAttempt = (int)Math.Round(attempts.Average(a => a.ReadingScorePercent));
            int avgVocabAttempt = (int)Math.Round(attempts.Average(a => a.VocabScorePercent));
            int avgGrammarAttempt = (int)Math.Round(attempts.Average(a => a.GrammarScorePercent));

            if (avgGrammarAttempt < 75 && avgGrammarAttempt <= avgReadingAttempt && avgGrammarAttempt <= avgVocabAttempt)
            {
                weaknesses.Add($"Kỹ năng Ngữ pháp còn hạn chế ({avgGrammarAttempt}%): bạn thường gặp khó khăn ở các cấu trúc ngữ pháp trong bài kiểm tra.");
                recommendations.Add("2. Ôn tập lại các cấu trúc ngữ pháp cốt lõi bằng cách dùng AI Tutor phân tích từng câu.");
            }
            else if (avgVocabAttempt < 75 && avgVocabAttempt <= avgReadingAttempt && avgVocabAttempt <= avgGrammarAttempt)
            {
                weaknesses.Add($"Vốn từ vựng văn cảnh còn thấp ({avgVocabAttempt}%): hay nhầm lẫn ý nghĩa từ vựng văn học.");
                recommendations.Add("2. Lưu thêm các từ vựng mới vào Flashcard và hoàn thành phiên ôn tập SRS hàng ngày.");
            }
            else if (avgReadingAttempt < 75)
            {
                weaknesses.Add($"Khả năng Đọc hiểu cốt truyện chưa vững ({avgReadingAttempt}%): cần chú ý hơn về diễn biến và hành động của nhân vật.");
                recommendations.Add("2. Đọc chậm lại, chú ý ngữ cảnh và tình huống truyện trước khi làm bài Quiz.");
            }
            else if (avgScore < 70)
            {
                weaknesses.Add($"Điểm trung bình các bài Quiz ({avgScore}%) cần cải thiện thêm về chi tiết cốt truyện.");
                recommendations.Add("2. Đọc kỹ lại ngữ cảnh câu chuyện trước khi làm bài kiểm tra tiếp theo.");
            }
            else
            {
                recommendations.Add($"2. Đọc tiếp chương mới của '{latestNovel?.Title ?? "銀河鉄道の夜"}' để mở rộng vốn đọc.");
            }

            if (avgReadingAttempt >= 80) strengths.Add($"Khả năng Đọc hiểu văn cảnh rất tốt ({avgReadingAttempt}% chính xác).");
            if (avgVocabAttempt >= 80) strengths.Add($"Vốn từ vựng văn học phong phú ({avgVocabAttempt}% chính xác).");
            if (avgGrammarAttempt >= 80) strengths.Add($"Nắm chắc ngữ pháp văn học Nhật Bản ({avgGrammarAttempt}% chính xác).");
        }

        recommendations.Add("3. Đặt mục tiêu đọc ít nhất 15 phút mỗi ngày để tăng tốc độ phản xạ đọc hiểu.");

        var recentHistory = attempts.Take(5).Select(a => {
            var n = novels.FirstOrDefault(x => x.Id == a.NovelId);
            return new RecentQuizAttemptDto
            {
                NovelId = a.NovelId,
                NovelTitle = n?.Title ?? $"Tiểu thuyết #{a.NovelId}",
                ChapterNumber = a.ChapterNumber,
                ScorePercentage = a.ScorePercentage,
                Passed = a.Passed,
                XpAwarded = a.XpAwarded,
                AttemptedAt = a.AttemptedAt
            };
        }).ToList();

        int avgReading = attempts.Any() ? (int)Math.Round(attempts.Average(a => a.ReadingScorePercent)) : 0;
        int avgVocab = attempts.Any() ? (int)Math.Round(attempts.Average(a => a.VocabScorePercent)) : 0;
        int avgGrammar = attempts.Any() ? (int)Math.Round(attempts.Average(a => a.GrammarScorePercent)) : 0;

        return new LearningAnalyticsDto
        {
            TotalNovels = totalNovels,
            CompletedChapters = completedChapters,
            InProgressNovels = inProgressNovels,
            OverallReadingProgressPercent = allChapters.Count > 0 ? Math.Round((double)completedChapters / allChapters.Count * 100, 1) : 0,
            LastReadNovelTitle = latestNovel?.Title ?? "銀河鉄道の夜",
            LastReadChapterTitle = latestChapter?.Title ?? "一、午後の授業",
            TotalSavedWords = totalSaved,
            NewWords = newWords,
            LearningWords = learningWords,
            MasteredWords = masteredWords,
            DueReviewsCount = dueCount,
            VocabularyRetentionRate = retentionRate,
            TotalQuizzesAttempted = totalAttempts,
            QuizzesPassed = passedCount,
            AverageQuizScorePercent = avgScore,
            TotalQuestionsAnswered = totalQuestions,
            OverallAccuracyPercent = overallAccuracy,
            ReadingAccuracyPercent = avgReading,
            VocabAccuracyPercent = avgVocab,
            GrammarAccuracyPercent = avgGrammar,
            StreakDays = userProgress.StreakDays,
            TotalStudyMinutes = userProgress.TotalStudyMinutes,
            TodayStudyMinutes = userProgress.TodayStudyMinutes,
            UserLevel = userProgress.Level,
            CurrentXp = userProgress.CurrentXp,
            Strengths = strengths,
            Weaknesses = weaknesses,
            ActionableRecommendations = recommendations,
            RecentQuizHistory = recentHistory
        };
    }

    public async Task<DailyPracticeRecommendationDto> GetDailyRecommendationAsync()
    {
        var dueVocab = await _vocabRepository.GetDueReviewsAsync();
        var novels = await _novelRepository.GetAllAsync();
        var uncompletedChapter = novels.SelectMany(n => n.Chapters ?? new List<Core.Models.Chapter>())
                                       .FirstOrDefault(c => !c.IsCompleted);

        return new DailyPracticeRecommendationDto
        {
            DueSrsCardsCount = dueVocab.Count,
            SuggestedNovelTitle = uncompletedChapter?.Novel?.Title ?? "銀河鉄道の夜",
            SuggestedNovelId = uncompletedChapter?.NovelId ?? 1,
            SuggestedChapterNum = uncompletedChapter?.ChapterNumber ?? 1,
            SuggestedChapterTitle = uncompletedChapter?.Title ?? "一、午後の授業",
            RecommendedAction = dueVocab.Count > 0 
                ? $"Bạn có {dueVocab.Count} thẻ từ vựng cần ôn tập SRS hôm nay trước khi tiếp tục đọc truyện."
                : "Tất cả thẻ từ vựng đã ôn xong! Hãy tiếp tục đọc chương truyện kế tiếp để rèn luyện đọc hiểu."
        };
    }

    private static List<QuizQuestionDto> GetGingaChapter1Questions()
    {
        return new List<QuizQuestionDto>
        {
            // 1. Reading Comprehension
            new QuizQuestionDto
            {
                QuestionId = 1,
                QuestionJp = "先生が黒板の図で指した、白くけむったものは何ですか？",
                QuestionVn = "Vệt mờ trắng mà thầy giáo chỉ tay vào trên bản đồ bảng đen là gì?",
                QuestionType = "multiple_choice",
                Category = "ReadingComprehension",
                ContextSentence = "上から下へ白くけむった銀河の帯のようなところを指しながら。",
                Options = new List<string> { "銀河（天の川）", "太陽の光", "月の影", "黒板のチョークの粉" },
                CorrectIndex = 0,
                Explanation = "Thầy giáo chỉ vào dải mờ trắng kéo từ trên xuống dưới, chính là dải Ngân Hà (銀河の帯) của bầu trời đêm.",
                TargetGrammarOrVocab = "銀河 (ぎんが)",
                Points = 10
            },
            // 2. Reading Comprehension
            new QuizQuestionDto
            {
                QuestionId = 2,
                QuestionJp = "ジョバンニが手をあげようとしてやめてしまった主な理由は何ですか？",
                QuestionVn = "Lý do chính khiến Giovanni định giơ tay rồi lại thôi là gì?",
                QuestionType = "multiple_choice",
                Category = "ReadingComprehension",
                ContextSentence = "このごろはジョバンニはまるで毎日教室でもねむく、本を読むひまも読む本もないので。",
                Options = new List<string> 
                { 
                    "Dạo này ngày nào cũng mệt mỏi buồn ngủ, không có thời gian đọc sách nên ký ức mờ nhạt dần", 
                    "Cậu không thích thầy giáo", 
                    "Campanella đã trả lời trước rồi", 
                    "Cậu muốn nhường cho các bạn khác giơ tay" 
                },
                CorrectIndex = 0,
                Explanation = "Giovanni phải đi làm thêm vất vả, ngày nào ở lớp cũng buồn ngủ và không có thời gian đọc sách nên cảm thấy kiến thức đã bị phai mờ.",
                TargetGrammarOrVocab = "ひま (暇 - thời gian rảnh)",
                Points = 10
            },
            // 3. Vocabulary
            new QuizQuestionDto
            {
                QuestionId = 3,
                QuestionJp = "【Từ vựng】文中の「黒板に吊した」の「吊す（つるす）」の意味として最も適切なものは？",
                QuestionVn = "Nghĩa đúng nhất của động từ '吊す (つるす)' trong ngữ cảnh câu văn là gì?",
                QuestionType = "multiple_choice",
                Category = "Vocabulary",
                ContextSentence = "黒板に吊した大きな黒い星座の図。",
                Options = new List<string> { "Treo lên, máng lên", "Gấp lại", "Vẽ lên", "Xé ra" },
                CorrectIndex = 0,
                Explanation = "Động từ '吊す' (つるす) có nghĩa là treo, máng một vật gì đó lên cao.",
                TargetGrammarOrVocab = "吊す (つるす)",
                Points = 10
            },
            // 4. Vocabulary
            new QuizQuestionDto
            {
                QuestionId = 4,
                QuestionJp = "【Từ vựng】「白くけむった」の「けむる（煙る）」はどのような様子を表していますか？",
                QuestionVn = "Động từ '煙る (けむる)' trong cụm '白くけむった' diễn tả trạng thái như thế nào?",
                QuestionType = "multiple_choice",
                Category = "Vocabulary",
                ContextSentence = "上から下へ白くけむった銀河の帯。",
                Options = new List<string> { "Mờ mịt như phủ khói hay sương", "Bốc cháy dữ dội", "Đen kịt tối tăm", "Lạnh buốt băng giá" },
                CorrectIndex = 0,
                Explanation = "『煙る (けむる)』trong văn cảnh miêu tả dải Ngân Hà mờ ảo, bàng bạc như phủ một làn khói trắng.",
                TargetGrammarOrVocab = "煙る (けむる)",
                Points = 10
            },
            // 5. Grammar
            new QuizQuestionDto
            {
                QuestionId = 5,
                QuestionJp = "【Ngữ pháp】「本を読むひまも読む本もない」の「～も～もない」の用法は？",
                QuestionVn = "Cấu trúc ngữ pháp『AもBもない』trong ngữ cảnh này diễn đạt điều gì?",
                QuestionType = "multiple_choice",
                Category = "Grammar",
                ContextSentence = "本を読むひまも読む本もないので、何だかどんなこともよくわからないという気持ちがするのでした。",
                Options = new List<string> { "Cả A lẫn B đều hoàn toàn không có (nhấn mạnh sự thiếu thốn)", "Có A nhưng không có B", "Không cần A cũng chẳng cần B", "Nếu có A thì sẽ có B" },
                CorrectIndex = 0,
                Explanation = "Cấu trúc 'AもBもない' dùng để liệt kê và nhấn mạnh sự phủ định hoàn toàn: 'cả thời gian đọc sách lẫn sách để đọc đều chẳng có'.",
                TargetGrammarOrVocab = "～も～もない",
                Points = 10
            },
            // 6. Grammar
            new QuizQuestionDto
            {
                QuestionId = 6,
                QuestionJp = "【Ngữ pháp】「手をあげようとして、急いでそれをやめました」の「～ようとする」の意味は？",
                QuestionVn = "Mẫu câu『Động từ thể ý chí + とする』mang ý nghĩa gì?",
                QuestionType = "multiple_choice",
                Category = "Grammar",
                ContextSentence = "ジョバンニは手をあげようとして、急いでそれをやめました。",
                Options = new List<string> { "Định làm gì đó, chuẩn bị thực hiện hành động", "Đã hoàn thành xong hành động", "Từ chối không làm", "Bắt người khác phải làm" },
                CorrectIndex = 0,
                Explanation = "V-ようとする diễn tả chủ ý chuẩn bị hành động tại một thời điểm ('định giơ tay thì vội dừng lại').",
                TargetGrammarOrVocab = "～ようとする",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetGingaChapter2Questions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 4,
                QuestionJp = "活版所（かっぱんじょ）に入ったジョバンニは何を受け取りましたか？",
                QuestionVn = "Khi bước vào xưởng in, Giovanni đã nhận được thứ gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "入口の計算台にすわっている白い上着を着た人が、ジョバンニを見ると紙切れを一枚渡しました。",
                Options = new List<string> { "紙切れを一枚 (Một mẩu giấy)", "銀河の切符 (Tấm vé ngân hà)", "本を一冊 (Một cuốn sách)", "お金の袋 (Túi tiền)" },
                CorrectIndex = 0,
                Explanation = "Người mặc áo trắng ở bàn tính đã đưa cho Giovanni một mẩu giấy (紙切れを一枚) ghi công việc sắp chữ cần làm.",
                TargetGrammarOrVocab = "紙切れ (かみきれ)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 5,
                QuestionJp = "今夜の祭のために、町の店先にはどんな飾りがされていましたか？",
                QuestionVn = "Để chuẩn bị cho lễ hội đêm nay, các cửa hàng trên phố đã trang trí những gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "色とりどりの旗を吊したり、銀の紙のすずらんを飾ったりしていました。",
                Options = new List<string> { "Cờ đủ sắc màu và hoa linh lan bằng giấy bạc", "Đèn lồng đỏ và pháo hoa", "Cây thông Noel", "Mặt nạ cáo" },
                CorrectIndex = 0,
                Explanation = "Đoạn văn nêu rõ: '色とりどりの旗を吊したり、銀の紙のすずらんを飾ったりしていました' (treo cờ đủ màu và trang trí hoa linh lan giấy bạc).",
                TargetGrammarOrVocab = "すずらん (hoa linh lan)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetBotchanQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 6,
                QuestionJp = "主人公の坊っちゃんは、子供の時からどんな性格だと述べていますか？",
                QuestionVn = "Nhân vật chính Botchan tự nhận mình có tính cách như thế nào từ thuở nhỏ?",
                QuestionType = "multiple_choice",
                ContextSentence = "親譲りの無鉄砲で小供の時から損ばかりしている。",
                Options = new List<string> { "親譲りの無鉄砲 (Bạt mạng, liều lĩnh thừa hưởng từ phụ mẫu)", "慎重で物静か (Thận trọng và trầm lặng)", "勉強熱心 (Chăm chỉ học hành)", "嘘つき (Hay nói dối)" },
                CorrectIndex = 0,
                Explanation = "Câu mở đầu bất hủ của tác phẩm: '親譲りの無鉄砲で小供の時から損ばかりしている' (Bản tính bạt mạng cha sinh mẹ đẻ khiến từ nhỏ tôi chỉ toàn chịu thiệt thòi).",
                TargetGrammarOrVocab = "無鉄砲 (むてっぽう)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 7,
                QuestionJp = "【Điền từ】二階から飛び降りて一週間ほど（　）を抜かした事がある。",
                QuestionVn = "Chọn từ đúng trong câu: 'Tôi nhảy từ tầng hai xuống và bị trẹo ... suốt cả tuần'.",
                QuestionType = "cloze",
                ContextSentence = "一週間ほど腰を抜かした事がある。",
                Options = new List<string> { "腰（こし - hông/eo）", "肩（かた - vai）", "首（くび - cổ）", "足（あし - chân）" },
                CorrectIndex = 0,
                Explanation = "Thành ngữ '腰を抜かす' (koshi o nukasu) vừa có nghĩa là ngã bệt trẹo hông, vừa có nghĩa bóng là sợ chết khiếp.",
                TargetGrammarOrVocab = "腰を抜かす (こしをぬかす)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetGongitsuneQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 8,
                QuestionJp = "ごんはどこに住んでいましたか？",
                QuestionVn = "Chú cáo Gon sống ở đâu?",
                QuestionType = "multiple_choice",
                ContextSentence = "しだのいっぱいしげった森の中に穴をほって住んでいました。",
                Options = new List<string> { "Trong hang đào giữa khu rừng đầy dương xỉ rậm rạp", "Trong ngôi đền cổ của làng", "Trên một cành cây cao", "Dưới gầm cầu" },
                CorrectIndex = 0,
                Explanation = "Nguyên văn: 'しだのいっぱいしげった森の中に穴をほって住んでいました' (đào hang sống trong khu rừng dương xỉ rậm rạp).",
                TargetGrammarOrVocab = "穴をほる (đào hang)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 9,
                QuestionJp = "ごんは普段、村へ出てきて何をしていましたか？",
                QuestionVn = "Gon thường ra làng làm gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "夜でも昼でも、ちかくの村へ出てきて、いたずらばかりしました。",
                Options = new List<string> { "Chuyên nghịch ngợm trêu chọc người làng", "Tìm thức ăn cho mẹ", "Giúp người dân cày ruộng", "Chơi với trẻ con" },
                CorrectIndex = 0,
                Explanation = "Nguyên văn: '夜でも昼でも、ちかくの村へ出てきて、いたずらばかりしました' (Dù ngày hay đêm cũng ra làng quậy phá nghịch ngợm).",
                TargetGrammarOrVocab = "いたずら (trò nghịch ngợm)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetMelosQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 10,
                QuestionJp = "メロスはどうして激怒しましたか？",
                QuestionVn = "Tại sao Melos lại vô cùng phẫn nộ?",
                QuestionType = "multiple_choice",
                ContextSentence = "メロスは激怒した。必ず、かの邪悪の王を除かなければならぬと決意した。",
                Options = new List<string> { "Quyết tâm phải diệt trừ tên bạo chúa tàn ác", "Bị bạn thân phản bội", "Bị mất hết gia tài", "Bị người dân cười nhạo" },
                CorrectIndex = 0,
                Explanation = "Mở đầu kiệt tác 'Chạy đi Melos': 'メロスは激怒した。必ず、かの邪悪の王を除かなければならぬと決意した' (Melos vô cùng phẫn nộ. Chàng quyết tâm nhất định phải trừ khử tên vua độc ác kia).",
                TargetGrammarOrVocab = "激怒 (げきど - phẫn nộ)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 11,
                QuestionJp = "メロスにとって何よりも大切な信条は何ですか？",
                QuestionVn = "Tín điều quan trọng nhất đối với Melos là gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "信実（しんじつ）を守ること、友との約束を果たすこと。",
                Options = new List<string> { "Lòng trung thực và giữ trọn lời hẹn ước với bạn", "Chiến thắng bằng mọi giá", "Sống sót an toàn", "Vàng bạc châu báu" },
                CorrectIndex = 0,
                Explanation = "Melos là biểu tượng của '信実' (lòng chân thật) và đức hy sinh vì tình bạn cao thượng.",
                TargetGrammarOrVocab = "信実 (しんじつ - trung thực, tín nghĩa)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetRashomonQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 12,
                QuestionJp = "下人はなぜ羅生門の下で雨やみを待っていましたか？",
                QuestionVn = "Tại sao kẻ hạ nhân lại đứng trú mưa dưới cổng La Sinh Môn?",
                QuestionType = "multiple_choice",
                ContextSentence = "数日前に仕えていた主人から暇を出されたばかりで、寝泊まりする所も今宵の糧もない。",
                Options = new List<string> { "Vừa bị chủ sa thải, không nơi ngủ trọ cũng chẳng có lương thực", "Đang chờ hẹn gặp một người bạn", "Đang ẩn nấp lính canh của triều đình", "Đợi trời tối để đi săn" },
                CorrectIndex = 0,
                Explanation = "Trong bối cảnh hoang tàn của Kyoto, người hạ nhân bị mất việc (暇を出された), bơ vơ giữa cơn mưa chiều và đứng trước lằn ranh đạo đức sinh tồn.",
                TargetGrammarOrVocab = "暇を出す (ひまをだす - sa thải, cho thôi việc)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 13,
                QuestionJp = "【Ngữ pháp】「盗人になるよりほかに道はない」の意味として最も適切なものは？",
                QuestionVn = "Ý nghĩa đúng nhất của cấu trúc '〜よりほかに道はない' trong câu là gì?",
                QuestionType = "cloze",
                ContextSentence = "盗人になるよりほかに道はない。",
                Options = new List<string> { "Chẳng còn con đường nào khác ngoài việc trở thành kẻ trộm", "Trở thành kẻ trộm là việc xấu nhất", "Có rất nhiều lối thoát tốt hơn làm trộm", "Nên suy nghĩ thêm các lựa chọn khác" },
                CorrectIndex = 0,
                Explanation = "Cấu trúc JLPT N2/N3: [V-rù + よりほかに〜ない] có nghĩa là 'không còn cách nào khác ngoài...', thể hiện sự dồn nén đến đường cùng.",
                TargetGrammarOrVocab = "〜よりほかに…ない (không còn cách nào khác ngoài)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetYukiguniQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 14,
                QuestionJp = "「国境の長いトンネルを抜けると雪国であった」の作者は誰ですか？",
                QuestionVn = "Câu văn mở đầu kinh điển 'Vượt qua đường hầm dài nơi biên giới là tới xứ tuyết' là của văn hào nào?",
                QuestionType = "multiple_choice",
                ContextSentence = "国境の長いトンネルを抜けると雪国であった。夜の底が白くなった。",
                Options = new List<string> { "川端康成 (Kawabata Yasunari - Giải Nobel Văn học 1968)", "太宰治 (Dazai Osamu)", "芥川龍之介 (Akutagawa Ryunosuke)", "夏目漱石 (Natsume Soseki)" },
                CorrectIndex = 0,
                Explanation = "Đây là câu mở đầu bất hủ trong tiểu thuyết 'Tuyết quốc' (雪国) của Kawabata Yasunari, nhà văn Nhật Bản đầu tiên đoạt giải Nobel Văn học.",
                TargetGrammarOrVocab = "雪国 (ゆきぐに - xứ tuyết)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 15,
                QuestionJp = "島村は何に心を奪われましたか？",
                QuestionVn = "Shimamura đã bị mê hoặc bởi điều gì trên chuyến tàu đêm?",
                QuestionType = "multiple_choice",
                ContextSentence = "島村は、鏡にうつる夕景の中で、その娘の瞳の美しさに心を奪われていた。",
                Options = new List<string> { "Ánh mắt tuyệt mỹ của thiếu nữ in trên mặt gương cửa sổ", "Tốc độ nhanh chóng của đoàn tàu", "Ánh đèn của nhà ga tuyết", "Tiếng còi tàu vang vọng" },
                CorrectIndex = 0,
                Explanation = "Kawabata sử dụng thủ pháp thẩm mỹ đa tầng: hình ảnh đôi mắt sáng của người con gái lồng ghép trong cảnh chiều phản chiếu trên tấm kính toa tàu.",
                TargetGrammarOrVocab = "心を奪われる (こころをうばわれる - bị mê hoặc, say đắm)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetSpiderQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 16,
                QuestionJp = "御釈迦様はなぜ大泥棒のカンダタを地獄から救おうとされましたか？",
                QuestionVn = "Tại sao Đức Phật lại muốn cứu đại đạo tặc Kandata khỏi đáy địa ngục?",
                QuestionType = "multiple_choice",
                ContextSentence = "生前、道端の小さな蜘蛛を踏み殺さずに助けてやったことがありました。",
                Options = new List<string> { "Vì sinh thời hắn từng tha mạng không dẫm chết một con nhện nhỏ", "Vì hắn biết tụng kinh sám hối", "Vì hắn là đệ tử thân cận", "Vì hắn hứa sẽ xây chùa" },
                CorrectIndex = 0,
                Explanation = "Dù là kẻ đại ác, Kandata trong tiền kiếp đã có một khoảnh khắc từ tâm: dừng chân lại không dẫm chết một chú nhện bên đường (一匹の蜘蛛を助けた善行).",
                TargetGrammarOrVocab = "善行 (ぜんこう - việc thiện, công đức)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 17,
                QuestionJp = "蜘蛛の糸はなぜカンダタのところでぷつりと切れてしまったのですか？",
                QuestionVn = "Tại sao sợi tơ nhện bạc lại đứt ngay nơi bàn tay Kandata đang nắm?",
                QuestionType = "multiple_choice",
                ContextSentence = "「この蜘蛛の糸はおれのものだ！ 降りろ、降りろ！」その途端でございます。糸はカンダタの握っているところからぷつりと切れました。",
                Options = new List<string> { "Vì tâm địa ích kỷ, chỉ muốn độc chiếm sợi tơ và xua đuổi người khác", "Vì sợi tơ quá cũ và mục rách", "Vì có quá nhiều người nặng cân", "Vì trời nổi gió bão" },
                CorrectIndex = 0,
                Explanation = "Chính tâm niệm vị kỷ (利己心) và lòng tham muốn độc chiếm ơn cứu rỗi ('おれのものだ!') đã khiến sợi tơ màu nhiệm từ cõi Cực Lạc lập tức đứt đoạn.",
                TargetGrammarOrVocab = "その途端 (そのとたん - đúng ngay khoảnh khắc đó)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetKokoroQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 18,
                QuestionJp = "主人公の「私」が先生と初めて出会った場所はどこですか？",
                QuestionVn = "Nhân vật 'Tôi' gặp gỡ Tiên sinh lần đầu tiên ở đâu?",
                QuestionType = "multiple_choice",
                ContextSentence = "私が先生と知り合ったのは鎌倉であった。由比ヶ浜の砂浜で、毎日海に入っていた。",
                Options = new List<string> { "Bờ biển Yuigahama tại Kamakura trong kỳ nghỉ hè", "Tại giảng đường đại học Tokyo", "Nơi một hiệu sách cổ ở Kanda", "Tại nhà ga xe lửa" },
                CorrectIndex = 0,
                Explanation = "Mở đầu phần 'Tiên sinh và tôi' (先生と私), nhân vật xưng 'Tôi' đã ấn tượng sâu sắc trước dáng vẻ cô độc, trầm mặc của Tiên sinh giữa bãi biển Kamakura đông đúc.",
                TargetGrammarOrVocab = "知り合う (しりあう - quen biết, diện kiến)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 19,
                QuestionJp = "【Triết lý】先生が語った「人間はいざという時には誰でも悪人になるものだよ」が暗示しているのは何ですか？",
                QuestionVn = "Câu nói nổi tiếng của Tiên sinh: 'Con người ta khi hữu sự, ai cũng có thể hóa thành kẻ ác' mang ngụ ý gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "美しい奥さんと二人で静かに暮らしている先生の胸の奥底には、深い孤独と後悔が横たわっていた。",
                Options = new List<string> { "Sự yếu đuối và khả năng tha hóa của bản ngã con người trước nghịch cảnh", "Khuyên mọi người nên làm việc thiện", "Chỉ trích xã hội phương Tây", "Khen ngợi bạn bè mình" },
                CorrectIndex = 0,
                Explanation = "Đây là chủ đề trung tâm của kiệt tác 'Kokoro': bi kịch giằng xé nội tâm, lòng ích kỷ tiềm ẩn trong mỗi con người và nỗi cô đơn hiện sinh không lối thoát.",
                TargetGrammarOrVocab = "いざという時 (khi có biến cố, khi hữu sự)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetChuumonQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 20,
                QuestionJp = "「当軒は注文の多い料理店ですからどうかそこはご承知ください」の本当の意味は何でしたか？",
                QuestionVn = "Ý nghĩa thật sự kinh hoàng đằng sau câu biển hiệu: 'Quán ăn nhiều yêu cầu nên xin quý khách hãy thông cảm' là gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "「注文が多いというのは客がたくさん来て繁盛しているという意味だろう」と二人は笑い合いました。",
                Options = new List<string> { "Quán ăn đặt ra nhiều 'yêu cầu tẩm ướp' đối với khách hàng để biến khách thành món ăn", "Quán có rất nhiều khách xếp hàng gọi món ăn", "Quán phục vụ rất chậm", "Đầu bếp nấu theo yêu cầu đặc biệt của khách" },
                CorrectIndex = 0,
                Explanation = "Thủ pháp chơi chữ trào phúng bậc thầy của Miyazawa Kenji: '注文' không phải là thực khách gọi món, mà là sơn quán yêu cầu thực khách bôi kem, xát muối để chuẩn bị... vào nồi.",
                TargetGrammarOrVocab = "注文 (ちゅうもん - yêu cầu / gọi món)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 21,
                QuestionJp = "【Điền từ】二人は（　）造りの西洋館を見つけました。",
                QuestionVn = "Chọn từ đúng: 'Hai chàng trai tìm thấy một dinh thự Tây phương xây bằng ... rất khang trang'.",
                QuestionType = "cloze",
                ContextSentence = "立派な白い煉瓦造りの西洋館が建っていました。",
                Options = new List<string> { "煉瓦（れんが - gạch nung）", "木（き - gỗ）", "鉄（てつ - sắt）", "氷（こおり - băng đá）" },
                CorrectIndex = 0,
                Explanation = "Từ vựng '煉瓦' (れんが - viên gạch). '煉瓦造り' (xây bằng gạch).",
                TargetGrammarOrVocab = "煉瓦造り (れんがづくり)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetKateikyoushiQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 22,
                QuestionJp = "亮太が主人公に妹の家庭教師を頼んだ理由は何ですか？",
                QuestionVn = "Lý do Ryota nhờ nhân vật chính làm gia sư cho em gái mình là gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "期末テストで赤点を取ると、大好きな軽音部の活動を禁止されてしまうらしい。",
                Options = new List<string> { "Nếu bị điểm liệt kỳ thi này, Hina sẽ bị cấm tham gia câu lạc bộ nhạc nhẹ", "Hina muốn thi đại học Tokyo", "Hina không chịu đi học", "Bố mẹ bắt buộc phải học" },
                CorrectIndex = 0,
                Explanation = "Hina rất đam mê câu lạc bộ nhạc nhẹ (軽音部 - Keionbu), nếu thi rớt điểm liệt (赤点 - akaten) sẽ bị đình chỉ hoạt động.",
                TargetGrammarOrVocab = "赤点を取る (あかてんをとる - bị điểm liệt/dưới trung bình)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 23,
                QuestionJp = "主人公がアドバイスした古典の勉強のコツは何ですか？",
                QuestionVn = "Bí quyết học văn học cổ điển mà người anh gia sư đã hướng dẫn là gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "古典の助動詞は丸暗記するんじゃなくて、文脈の意味から推測するんだ。",
                Options = new List<string> { "Không học vẹt máy móc mà suy đoán ý nghĩa trợ động từ dựa theo mạch văn", "Chép phạt mười lần mỗi bài", "Dùng từ điển tra từng chữ", "Chỉ học thuộc lòng các đề thi năm trước" },
                CorrectIndex = 0,
                Explanation = "Phương pháp học tiếng Nhật chuẩn xác: '丸暗記するんじゃなくて、文脈の意味から推測する' (không học vẹt, mà suy đoán nghĩa từ ngữ cảnh).",
                TargetGrammarOrVocab = "丸暗記 (まるあんき - học vẹt, học thuộc lòng không hiểu bản chất)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetToshishunQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 24,
                QuestionJp = "杜子春が仙人になるための修行で課された唯一の掟は何でしたか？",
                QuestionVn = "Quy tắc cấm kỵ duy nhất mà Tiên nhân thiết lập cho Đỗ Tử Xuân trong cuộc thử thách tu tiên là gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "何が起きても決して声を出してはならぬ。一言でも口を利けば、仙人にはなれんぞ。",
                Options = new List<string> { "Dù bất cứ chuyện gì xảy ra cũng tuyệt đối giữ im lặng, không được phát ra tiếng động", "Không được nhắm mắt", "Không được ăn uống trong 100 ngày", "Phải đánh bại ma thần" },
                CorrectIndex = 0,
                Explanation = "Thử thách giữ im lặng (沈黙): '決して声を出してはならぬ' (tuyệt đối không được lên tiếng dù bị tra tấn hay dọa nạt).",
                TargetGrammarOrVocab = "口を利く (くちをきく - mở miệng cất lời)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 25,
                QuestionJp = "杜子春はなぜ掟を破って「お母さん！」と叫んでしまったのですか？",
                QuestionVn = "Tại sao Đỗ Tử Xuân lại chấp nhận phá vỡ giới luật và thét lên 'Mẹ ơi!'?",
                QuestionType = "multiple_choice",
                ContextSentence = "「子春よ、私たちはどうなっても構わない。声を出してはならないよ」母の慈愛に満ちた優しい声を聞いた瞬間。",
                Options = new List<string> { "Vì lòng hiếu thảo và tình yêu thương vô bờ bến trước sự hy sinh của mẹ ruột", "Vì sợ hãi ma thần trừng phạt", "Vì muốn lấy lại vàng bạc", "Vì kiệt sức không chịu nổi" },
                CorrectIndex = 0,
                Explanation = "Chính tình mẫu tử thiêng liêng và lòng hiếu thảo (親を思う真心) đã khiến Tử Xuân chọn làm một con người chân chính hơn là một vị tiên nhân vô cảm.",
                TargetGrammarOrVocab = "真心 (まごころ - tấm lòng chân thành)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetMaihimeQuestions()
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 26,
                QuestionJp = "主人公の太田豊太郎がベルリンで出会ったエリスの職業は何ですか？",
                QuestionVn = "Nghề nghiệp của thiếu nữ Elise mà chàng du học sinh Toyotaro gặp gỡ tại Berlin là gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "少女の名はエリス。貧しい劇場の若き踊り子（舞姫）であった。",
                Options = new List<string> { "Vũ nữ múa ba lê tại một nhà hát nghèo", "Giáo viên tiếng Đức", "Y tá bệnh viện", "Tiểu thư quý tộc" },
                CorrectIndex = 0,
                Explanation = "Elise là một '踊り子' (vũ nữ / vũ công nghèo), cũng chính là hình tượng '舞姫' (Vũ nữ) trong kiệt tác của Mori Ogai.",
                TargetGrammarOrVocab = "踊り子 (おどりこ - vũ công, vũ nữ múa)",
                Points = 10
            },
            new QuizQuestionDto
            {
                QuestionId = 27,
                QuestionJp = "【Chủ đề bi kịch】『舞姫』が描いた近代日本の青年知識人の苦悩とは何ですか？",
                QuestionVn = "Bi kịch giằng xé nội tâm lớn nhất của người trí thức cận đại Nhật Bản trong 'Vũ nữ' là gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "愛と名誉の狭間で魂を引き裂かれた余は、深い悔恨を抱えたまま船の甲板に立つのであった。",
                Options = new List<string> { "Sự xung đột nghiệt ngã giữa tình yêu tự do cá nhân và nghĩa vụ danh vọng phụng sự quốc gia", "Khó khăn khi học tiếng nước ngoài", "Thời tiết mùa đông giá rét", "Bệnh tật nhiệt đới" },
                CorrectIndex = 0,
                Explanation = "Đây là chủ đề kinh điển của văn học Minh Trị: xung đột giữa 'bản ngã cá nhân và tình yêu' (エゴ・純愛) với 'trật tự gia tộc và danh vọng công quyền' (国家・名誉).",
                TargetGrammarOrVocab = "名誉と純愛の狭間 (giữa danh dự và ái tình thuần khiết)",
                Points = 10
            }
        };
    }

    private static List<QuizQuestionDto> GetGenericLiteratureQuestions(string chapterTitle)
    {
        return new List<QuizQuestionDto>
        {
            new QuizQuestionDto
            {
                QuestionId = 99,
                QuestionJp = $"この章（{chapterTitle}）の主なテーマは何ですか？",
                QuestionVn = $"Chủ đề chính được thể hiện trong phần {chapterTitle} là gì?",
                QuestionType = "multiple_choice",
                ContextSentence = "文脈を正確に読み取りましょう。",
                Options = new List<string> { "Khắc họa cảm xúc nhân vật và thế giới quan", "Bàn luận về chính trị hiện đại", "Phương pháp kinh doanh", "Hướng dẫn du lịch" },
                CorrectIndex = 0,
                Explanation = "Văn học Nhật Bản chú trọng khắc họa nội tâm nhân vật qua cảnh sắc thiên nhiên và lời thoại.",
                TargetGrammarOrVocab = "文脈 (ぶんみゃく - ngữ cảnh)",
                Points = 10
            }
        };
    }
}
