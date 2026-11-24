using System.IO;
using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Models;
using KotobaOdyssey.Core.Services;
using KotobaOdyssey.Data;
using KotobaOdyssey.Data.Repositories;
using KotobaOdyssey.Data.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace KotobaOdyssey.Tests;

public class CoreLearningLoopProofTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _dbFilePath;
    private readonly string _connectionString;

    public CoreLearningLoopProofTests(ITestOutputHelper output)
    {
        _output = output;
        _dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "proof_verification.db");
        if (File.Exists(_dbFilePath))
        {
            File.Delete(_dbFilePath);
        }
        _connectionString = $"Data Source={_dbFilePath}";
    }

    public void Dispose()
    {
        if (File.Exists(_dbFilePath))
        {
            try { File.Delete(_dbFilePath); } catch { }
        }
    }

    private KotobaDbContext CreateFreshDbContext()
    {
        var options = new DbContextOptionsBuilder<KotobaDbContext>()
            .UseSqlite(_connectionString)
            .Options;

        var context = new KotobaDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task Proof_EndToEndLearningLoop_VerifiesAllThreeRequirements()
    {
        _output.WriteLine("================================================================================");
        _output.WriteLine("CHỨNG MINH 1: ĐỌC TRUYỆN & TRA TỪ HOẠT ĐỘNG THỰC SỰ (KHÔNG PHẢI POPUP ẢO)");
        _output.WriteLine("================================================================================");

        var dictService = new JapaneseDictionaryService();
        var textAnalyzer = new JapaneseTextAnalyzer(dictService);

        // Đoạn trích thực tế từ tiểu thuyết "Yukiguni" (Xứ Tuyết - Kawabata Yasunari)
        string novelParagraph = "国境の長いトンネルを抜けると雪国であった。夜の底が白くなった。信号所に汽車が止まった。";
        _output.WriteLine($"[Đoạn văn tiểu thuyết]: {novelParagraph}");

        // Người đọc nhấp vào từ phái sinh / biến thể: "抜けると"
        string clickedWord = "抜けると";
        _output.WriteLine($"[Người dùng nhấp vào từ]: {clickedWord}");

        // 1.1 Kiểm tra bóc tách câu chính xác
        string extractedSentence = textAnalyzer.ExtractSentence(novelParagraph, clickedWord);
        _output.WriteLine($"[Câu ngữ cảnh bóc tách được]: \"{extractedSentence}\"");
        Assert.Equal("国境の長いトンネルを抜けると雪国であった。", extractedSentence);

        // 1.2 Kiểm tra De-inflection tra từ điển thực tế
        var dictEntry = await dictService.LookupAsync(clickedWord);
        Assert.NotNull(dictEntry);
        _output.WriteLine($"[Từ gốc (Base Form)]: {dictEntry.Term}");
        _output.WriteLine($"[Cách đọc (Hiragana)]: {dictEntry.Reading}");
        _output.WriteLine($"[Nghĩa tiếng Việt]: {dictEntry.Meaning}");
        _output.WriteLine($"[Từ loại]: {dictEntry.PartOfSpeech}");
        _output.WriteLine($"[Cấp độ JLPT]: {dictEntry.JlptLevel}");
        _output.WriteLine($"[Ghi chú dạng chia (InflectionNote)]: {dictEntry.InflectionNote}");

        Assert.Equal("抜ける", dictEntry.Term);
        Assert.Equal("ぬける", dictEntry.Reading);
        Assert.Equal("抜けると", dictEntry.OriginalSurface);
        Assert.Contains("Thể điều kiện", dictEntry.InflectionNote);

        // 1.3 Kiểm tra chống bịa từ: tra từ không tồn tại
        var fakeLookup = await dictService.LookupAsync("xyz_fake_word_12345");
        Assert.Null(fakeLookup);
        _output.WriteLine("[Tra từ giả 'xyz_fake_word_12345']: Kết quả = NULL (Không tự bịa nghĩa ảo)");


        _output.WriteLine("\n================================================================================");
        _output.WriteLine("CHỨNG MINH 2: TỪ ĐÃ LƯU XUẤT HIỆN Ở CẢ VOCABULARY VÀ FLASHCARD (CÙNG NGUỒN DỮ LIỆU)");
        _output.WriteLine("================================================================================");

        int savedVocabId;
        using (var context = CreateFreshDbContext())
        {
            var vocabRepo = new VocabularyRepository(context);

            var newVocab = new VocabularyItem
            {
                Word = dictEntry.Term,                    // "抜ける"
                Reading = dictEntry.Reading,              // "ぬける"
                Meaning = dictEntry.Meaning,
                JlptLevel = dictEntry.JlptLevel,
                ContextSentence = extractedSentence,     // Câu ngữ cảnh thực
                SourceNovelTitle = "雪国 (Xứ tuyết)",
                SourceChapterId = 1,
                CreatedAt = DateTime.UtcNow,
                NextReviewDate = DateTime.UtcNow,        // Sẵn sàng ôn tập ngay
                IntervalDays = 0,
                EaseFactor = 2.5,
                Repetitions = 0
            };

            var saved = await vocabRepo.AddAsync(newVocab);
            savedVocabId = saved.Id;
            _output.WriteLine($"[Đã lưu vào SQLite Database]: ID = {savedVocabId}, Từ = '{saved.Word}', Nghĩa = '{saved.Meaning}'");

            // Kiểm tra: Màn hình Danh sách từ vựng (Vocabulary List) đọc từ DB
            var allVocabItems = await vocabRepo.GetAllAsync();
            var foundInVocabList = allVocabItems.FirstOrDefault(v => v.Id == savedVocabId);
            Assert.NotNull(foundInVocabList);
            _output.WriteLine($"[Kiểm tra Vocabulary List]: Tìm thấy ID {foundInVocabList.Id} ('{foundInVocabList.Word}') trong tổng số {allVocabItems.Count} từ.");

            // Kiểm tra: Màn hình Flashcard SRS đọc từ DB (lấy các thẻ đến hạn hôm nay)
            var dueReviewCards = await vocabRepo.GetDueReviewsAsync();
            var foundInSrsQueue = dueReviewCards.FirstOrDefault(c => c.Id == savedVocabId);
            Assert.NotNull(foundInSrsQueue);
            _output.WriteLine($"[Kiểm tra Flashcard SRS Queue]: Tìm thấy ID {foundInSrsQueue.Id} ('{foundInSrsQueue.Word}') trong danh sách ôn tập đến hạn.");

            // Khẳng định 2 bên dùng CÙNG một bản ghi trong SQLite, không lưu riêng rẽ
            Assert.Equal(foundInVocabList.Word, foundInSrsQueue.Word);
            Assert.Equal(foundInVocabList.Id, foundInSrsQueue.Id);
            Assert.Equal(foundInVocabList.ContextSentence, foundInSrsQueue.ContextSentence);
        }


        _output.WriteLine("\n================================================================================");
        _output.WriteLine("CHỨNG MINH 3: ÔN TẬP XONG THÌ LỊCH SRS THAY ĐỔI & BỀN VỮNG KHI TẢI LẠI ỨNG DỤNG");
        _output.WriteLine("================================================================================");

        DateTime initialReviewTime = DateTime.UtcNow;

        // 3.1 Thực hiện ôn tập thẻ (Người dùng chấm điểm 'Good' = 4)
        using (var context = CreateFreshDbContext())
        {
            var vocabRepo = new VocabularyRepository(context);
            var itemToReview = await vocabRepo.GetByIdAsync(savedVocabId);
            Assert.NotNull(itemToReview);

            _output.WriteLine($"[Trước khi ôn tập]:");
            _output.WriteLine($" - IntervalDays: {itemToReview.IntervalDays}");
            _output.WriteLine($" - Repetitions:  {itemToReview.Repetitions}");
            _output.WriteLine($" - EaseFactor:   {itemToReview.EaseFactor}");
            _output.WriteLine($" - NextReview:   {itemToReview.NextReviewDate:yyyy-MM-dd HH:mm:ss} UTC");

            // Tính toán thuật toán SM-2
            var sm2Result = SpacedRepetitionCalculator.CalculateNextReview(
                currentIntervalDays: itemToReview.IntervalDays,
                currentEaseFactor: itemToReview.EaseFactor,
                currentRepetitions: itemToReview.Repetitions,
                rating: ReviewRating.Good,
                reviewDateUtc: initialReviewTime);

            // Ghi nhận lượt ôn tập vào SQLite
            var reviewLog = await vocabRepo.RecordReviewAsync(itemToReview.Id, ReviewRating.Good, sm2Result);

            _output.WriteLine($"[Đã chấm điểm 'Good' (SM-2)]: Kết quả cập nhật vào DB:");
            _output.WriteLine($" - New IntervalDays: {sm2Result.NewIntervalDays} ngày");
            _output.WriteLine($" - New Repetitions:  {sm2Result.NewRepetitions}");
            _output.WriteLine($" - New EaseFactor:   {sm2Result.NewEaseFactor}");
            _output.WriteLine($" - New NextReview:   {sm2Result.NextReviewDate:yyyy-MM-dd HH:mm:ss} UTC");
            _output.WriteLine($" - Đã tạo ReviewLog: ID = {reviewLog.Id}, Rating = {reviewLog.Rating}");

            Assert.Equal(1, sm2Result.NewIntervalDays);
            Assert.Equal(1, sm2Result.NewRepetitions);
        }

        // 3.2 TẢI LẠI ỨNG DỤNG (HOÀN TOÀN MỚI)
        // Mô phỏng việc tắt app, đóng tiến trình, khởi động lại và mở một kết nối SQLite hoàn toàn mới:
        _output.WriteLine("\n--> [MÔ PHỎNG TẮT VÀ KHỞI ĐỘNG LẠI ỨNG DỤNG] <--");

        using (var restartedContext = CreateFreshDbContext())
        {
            var freshVocabRepo = new VocabularyRepository(restartedContext);

            // Lấy lại từ vựng sau khi app đã restart
            var reloadedItem = await freshVocabRepo.GetByIdAsync(savedVocabId);
            Assert.NotNull(reloadedItem);

            _output.WriteLine($"[Sau khi khởi động lại ứng dụng đọc từ file SQLite trên đĩa]:");
            _output.WriteLine($" - ID:            {reloadedItem.Id}");
            _output.WriteLine($" - Word:          {reloadedItem.Word}");
            _output.WriteLine($" - IntervalDays:  {reloadedItem.IntervalDays} (Đã tăng từ 0 lên 1)");
            _output.WriteLine($" - Repetitions:   {reloadedItem.Repetitions} (Đã tăng từ 0 lên 1)");
            _output.WriteLine($" - NextReviewDate:{reloadedItem.NextReviewDate:yyyy-MM-dd HH:mm:ss} UTC (Đã dời vào tương lai)");

            // Khẳng định dữ liệu SRS đã thay đổi VÀ được lưu vĩnh viễn trên đĩa
            Assert.Equal(1, reloadedItem.IntervalDays);
            Assert.Equal(1, reloadedItem.Repetitions);
            Assert.True(reloadedItem.NextReviewDate > initialReviewTime);

            // Lịch sử ôn tập vẫn còn nguyên vẹn trong DB
            Assert.Single(reloadedItem.ReviewHistory);
            Assert.Equal(ReviewRating.Good, reloadedItem.ReviewHistory.First().Rating);
            _output.WriteLine($" - Lịch sử ôn tập (ReviewHistory): 1 bản ghi xác thực trong SQLite");

            // Thẻ đã được dời lịch sang ngày mai, nên nếu lấy danh sách thẻ đến hạn 'ngay bây giờ', thẻ này KHÔNG CÒN xuất hiện nữa!
            var dueCardsAfterReview = await freshVocabRepo.GetDueReviewsAsync();
            bool isStillDueNow = dueCardsAfterReview.Any(c => c.Id == savedVocabId);
            _output.WriteLine($" - Thẻ còn nằm trong danh sách cần ôn tập ngay bây giờ không?: {isStillDueNow} (ĐÚNG: đã được dời lịch)");
            Assert.False(isStillDueNow);
        }

        _output.WriteLine("\n================================================================================");
        _output.WriteLine("KẾT LUẬN: CẢ 3 YÊU CẦU ĐÃ ĐƯỢC CHỨNG MINH 100% HOẠT ĐỘNG THỰC TẾ TRÊN DATABASE THẬT!");
        _output.WriteLine("================================================================================");
    }
}
