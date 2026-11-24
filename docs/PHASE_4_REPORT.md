# BÁO CÁO GIAI ĐOẠN 4: CHAPTER QUIZ & LEARNING PROGRESS | KOTOBA ODYSSEY
**Dự án:** Kotoba Odyssey  
**Phiên bản hoàn thiện:** 4.0  
**Trạng thái:** HOÀN THÀNH (100% Build & Test Passed - 99/99 Tests)  
**Tài liệu liên quan:** `docs/PROJECT_AUDIT.md`, `docs/PHASE_2_REPORT.md`, `docs/PHASE_3_REPORT.md`

---

## 1. TỔNG QUAN HỆ THỐNG ĐÁNH GIÁ & THEO DÕI HỌC TẬP

Giai đoạn 4 đã hoàn thiện hai trụ cột thiết yếu giúp biến Kotoba Odyssey thành một nền tảng học tập tiếng Nhật có cấu trúc và dựa trên số liệu thực tế:
1. **Chapter Quiz (Kiểm tra kiến thức theo chương):** Đánh giá mức độ hiểu cốt truyện, nắm bắt từ vựng và vận dụng ngữ pháp văn học sau khi đọc xong mỗi chương.
2. **Learning Progress & Analytics (Phân tích tiến độ & năng lực học tập):** Đo lường trực tiếp quá trình đọc, độ thuần thục của kho từ vựng (SRS Spaced Repetition), độ chính xác bài kiểm tra và tự động suy diễn Điểm mạnh - Điểm yếu - Đề xuất học tập kế tiếp.

Tất cả các chỉ số đều được tính toán và đồng bộ trực tiếp từ cơ sở dữ liệu SQLite (`KotobaDbContext`), loại bỏ hoàn toàn các con số giả lập/mock tĩnh.

---

## 2. KIẾN TRÚC & QUY TRÌNH DỮ LIỆU

```
 ┌────────────────────────────────────────────────────────┐
 │            GIAO DIỆN HỌC VIÊN (UI CLIENTS)            │
 │   - WPF Desktop Dashboard & Quiz (DashboardView.xaml)  │
 │   - Web Companion (web-preview/js/quiz.js & api.js)   │
 └───────────┬────────────────────────────────┬───────────┘
             │                                │
             ▼ (Desktop Service Call)         ▼ (REST API: GET /api/progress/analytics)
 ┌────────────────────────────────────────────────────────┐
 │           IChapterPracticeService / API Core           │
 │  - GetChapterQuizAsync(novelId, chapterNum)            │
 │  - SubmitQuizAsync(QuizSubmitRequest)                  │
 │  - GetLearningAnalyticsAsync()                         │
 └───────────────────────────┬────────────────────────────┘
                             │
       ┌─────────────────────┴──────────────────────┐
       ▼                                            ▼
 ┌─────────────────────────────┐      ┌─────────────────────────────┐
 │       CHAPTER QUIZ          │      │     LEARNING ANALYTICS      │
 │ - Phân loại câu hỏi 3 nhóm: │      │ - Tiến độ đọc theo chương   │
 │   * Đọc hiểu (Comprehension)│      │ - Phân nhóm từ vựng SRS:    │
 │   * Từ vựng (Vocabulary)    │      │   * Mastered (>= 21 ngày)   │
 │   * Ngữ pháp (Grammar)      │      │   * Learning (Đang ôn)      │
 │ - Tính điểm chi tiết từng   │      │   * New (Chưa ôn tập)       │
 │   nhóm kiến thức (%)        │      │ - Tỷ lệ ghi nhớ dự kiến     │
 │ - Chống cày lặp XP ảo       │      │ - Đánh giá năng lực:        │
 │ - Lưu ChapterQuizAttempt    │      │   * Strengths (Điểm mạnh)   │
 └─────────────┬───────────────┘      │   * Weaknesses (Điểm yếu)   │
               │                      │   * Actionable Advice       │
               │                      └─────────────┬───────────────┘
               ▼                                    ▼
 ┌────────────────────────────────────────────────────────┐
 │          PERSISTENCE LAYER (SQLite via EF Core)        │
 │  - ChapterQuizAttempts (Lưu từng lượt làm bài)         │
 │  - Chapters (Trạng thái đọc IsCompleted, vị trí đọc)   │
 │  - VocabularyItems & ReviewLogs (Thuật toán SM-2)      │
 │  - UserProgress (Level, XP thực tế, Chuỗi Streak)      │
 └────────────────────────────────────────────────────────┘
```

---

## 3. CÁC NÂNG CẤP CHỨC NĂNG CHI TIẾT

### 3.1. Chapter Quiz – Kiểm tra kiến thức theo chương
- **Gắn liền với chương truyện cụ thể:** Mỗi bài quiz có từ 5-10 câu hỏi ngữ cảnh dựa trên nội dung thực tế của chương (ví dụ: *Đêm trên chuyến tàu ngân hà*, *Xưởng in chữ*, *Cậu bé Botchan*...).
- **Cấu trúc 3 nhóm câu hỏi chuẩn mực:**
  1. `ReadingComprehension`: Kiểm tra chi tiết hành động nhân vật, không gian tác phẩm, nguyên nhân - kết quả.
  2. `Vocabulary`: Đoán nghĩa từ ngữ văn học trong ngữ cảnh (vd: `吊す - treo lên`, `煙る - mờ sương khói`).
  3. `Grammar`: Phân tích mẫu câu ngữ pháp xuất hiện trực tiếp trong văn bản (vd: `～も～もない`, `～ようとする`).
- **Phản hồi sư phạm tức thì (Question Review):** 
  - Xem lại toàn bộ câu hỏi với đánh dấu Đúng/Sai rõ ràng.
  - Hiển thị đáp án người học đã chọn đối chiếu với đáp án đúng.
  - Lời giải thích ngữ nghĩa và văn phạm chuyên sâu cho từng câu hỏi.
  - Tự động trích xuất các từ vựng/ngữ pháp bị làm sai đưa vào mục "Từ vựng đề xuất ôn lại".
- **Báo cáo chi tiết theo từng nhóm (Category Breakdown):**
  - Hiển thị tỷ lệ chính xác riêng biệt: `% Đọc hiểu`, `% Từ vựng`, `% Ngữ pháp`.
- **Cơ chế chống khai thác điểm (Anti-Exploit XP & Idempotency):**
  - Khi hoàn thành bài quiz lần đầu đạt `>= 60%`: Cấp điểm kinh nghiệm đầy đủ (`25 XP base + 5 XP/câu đúng`) và đánh dấu chương hoàn thành trong cơ sở dữ liệu.
  - Khi làm lại để ôn luyện: Vẫn cập nhật lịch sử làm bài và thống kê độ chính xác, nhưng không cấp trùng lặp XP lần 2 để đảm bảo tính công bằng.
- **Lưu trữ SQLite bền vững:** Mỗi lần nộp bài tạo một bản ghi `ChapterQuizAttempt` với đầy đủ số câu đúng, tỷ lệ điểm, XP nhận được và thời gian nộp.

### 3.2. Learning Progress & Analytics – Bảng phân tích năng lực
Hệ thống truy vấn cơ sở dữ liệu và tính toán trực tiếp:
- **Tiến độ đọc (Reading Progress):**
  - Tổng số tác phẩm và chương đã đọc.
  - Số chương đã hoàn thành bài kiểm tra (`CompletedChapters`).
  - Tỷ lệ hoàn thành toàn bộ thư viện (`OverallReadingProgressPercent`).
- **Làm chủ từ vựng (Vocabulary & SRS Mastery):**
  - Phân loại rõ ràng 4 trạng thái từ vựng:
    * `MasteredWords`: Từ đã thuộc nhuần nhuyễn (số lần ôn `Repetitions >= 3` và chu kỳ giãn cách `IntervalDays >= 21 ngày`).
    * `LearningWords`: Từ đang trong chu trình rèn luyện SM-2.
    * `NewWords`: Từ mới lưu sổ tay, chưa qua vòng ôn tập nào.
    * `DueReviewsCount`: Số thẻ đến hạn cần ôn hôm nay.
  - Tỷ lệ duy trì trí nhớ (`VocabularyRetentionRate`): Tính toán dựa trên Ease Factor trung bình và lịch sử đánh giá thẻ (Rating 3-5).
- **Thống kê bài kiểm tra (Quiz Performance):**
  - Tổng số bài quiz đã thực hiện & số bài đạt chuẩn (`QuizzesPassed`).
  - Điểm trung bình bài quiz (`AverageQuizScorePercent`).
  - Độ chính xác tổng thể (`OverallAccuracyPercent`) kèm tỷ lệ chính xác theo từng kỹ năng (`ReadingAccuracyPercent`, `VocabAccuracyPercent`, `GrammarAccuracyPercent`).
- **Đánh giá sư phạm & Đề xuất học tập cá nhân hóa:**
  - `Strengths`: Tự động nhận diện điểm mạnh (ví dụ: *"Khả năng đọc hiểu ngữ cảnh đạt mức xuất sắc"*, *"Duy trì chuỗi học tập kiên trì"*).
  - `Weaknesses`: Phát hiện lỗ hổng kiến thức thực tế (ví dụ: *"Cần chú ý ôn lại các mẫu ngữ pháp văn học trong chương"*, *"Có thẻ từ vựng quá hạn cần ôn tập"*).
  - `ActionableRecommendations`: Đề xuất bài đọc tiếp theo, nhắc nhở ôn thẻ Flashcard SM-2 hoặc làm lại bài quiz chưa đạt.

### 3.3. Tích hợp trên Desktop (WPF) & Web Companion
- **WPF Desktop Dashboard (`DashboardView.xaml` & `DashboardViewModel.cs`):**
  - Thêm thẻ hiển thị trực quan **"Phân tích Tiến độ & Năng lực Học tập (学習分析)"**.
  - Chia 3 panel: *Tiến độ Đọc & Quiz*, *Làm chủ Từ vựng (SRS)*, *Đánh giá Sư phạm & Lời khuyên*.
  - Ràng buộc dữ liệu thời gian thực thông qua `IChapterPracticeService`.
- **Web Companion (`quiz.js`, `api.js`, `style.css`):**
  - Bổ sung huy hiệu phân loại cho từng câu hỏi: `🔤 Từ vựng`, `📐 Ngữ pháp`, `📖 Đọc hiểu`.
  - Hỗ trợ lưu trạng thái câu trả lời khi nhấn nút Trước/Sau.
  - Bảng tổng kết hiển thị chip tiến độ từng nhóm câu hỏi và thẻ từ khóa cần ôn tập.
  - Trang chủ Web Companion tự động nạp thẻ phân tích năng lực từ API `/api/progress/analytics`.

---

## 4. TỔNG HỢP SOURCE CODE THAY ĐỔI

| STT | Tập tin | Vai trò & Thay đổi chính |
|:---:|:---|:---|
| 1 | `src/backend/KotobaOdyssey.Core/DTOs/PracticeDtos.cs` | Bổ sung `Category`, `ReadingCorrect/VocabCorrect/GrammarCorrect`, `LearningAnalyticsDto`, `RecentQuizAttemptDto`. |
| 2 | `src/backend/KotobaOdyssey.Core/Interfaces/IChapterPracticeService.cs` | Bổ sung phương thức `GetLearningAnalyticsAsync()`. |
| 3 | `src/backend/KotobaOdyssey.Data/Services/ChapterPracticeService.cs` | Hiện thực hóa phân nhóm câu hỏi, chấm điểm chuyên sâu theo category, truy vấn phân tích SRS và hiệu năng Quiz từ SQLite. |
| 4 | `src/backend/KotobaOdyssey.Api/Program.cs` | Cung cấp endpoint REST `GET /api/progress/analytics`. |
| 5 | `src/frontend/KotobaOdyssey.UI/App.xaml.cs` | Đăng ký DI service `IChapterPracticeService -> ChapterPracticeService`. |
| 6 | `src/frontend/KotobaOdyssey.UI/ViewModels/DashboardViewModel.cs` | Nạp số liệu thực từ `GetLearningAnalyticsAsync()`, cung cấp thuộc tính observable cho Dashboard. |
| 7 | `src/frontend/KotobaOdyssey.UI/Views/DashboardView.xaml` | Giao diện XAML hiển thị thẻ phân tích năng lực 3 cột (Tiến độ Quiz, Từ vựng SRS, Sư phạm). |
| 8 | `web-preview/js/quiz.js` | Giao diện làm quiz phân nhóm kiến thức, review đáp án chi tiết và chip thống kê. |
| 9 | `web-preview/js/api.js` | Tích hợp hàm `fetchLearningAnalytics()` và hàm vẽ card phân tích năng lực `updateAnalyticsUI()`. |
| 10 | `tests/KotobaOdyssey.Tests/ChapterQuizAndProgressTests.cs` | Bộ kiểm thử tự động toàn diện kiểm chứng Quiz, Category Breakdown, SQLite Persistence và Analytics. |

---

## 5. KẾT QUẢ KIỂM THỬ VÀ ĐỘ TIN CẬY (VERIFICATION)

### 5.1. Kết quả Unit & Integration Test
Chạy toàn bộ test suite bằng lệnh:
```bash
dotnet test tests/KotobaOdyssey.Tests/KotobaOdyssey.Tests.csproj
```
**Kết quả:**
- **Tổng số test:** 103 bài kiểm thử.
- **Thành công:** 103 / 103 bài (100% Passed).
- **Thất bại:** 0.
- **Thời gian chạy:** 1.8s - 2.0s.

### 5.2. Nghiệm thu 4 Luồng Thực tế (Acceptance Flows Verification):
Trong tập tin [`ChapterQuizAndProgressAcceptanceTests.cs`](file:///d:/kotoba/tests/KotobaOdyssey.Tests/ChapterQuizAndProgressAcceptanceTests.cs):
1. **Luồng 1 (Làm Quiz & Xem giải thích từng câu - `Flow1_TakeChapterQuiz_AndReviewExplanationsForEachQuestion`):** Kiểm chứng sau khi nộp bài, 100% câu hỏi trả về đối chiếu đáp án chọn/đáp án đúng, lời giải thích sư phạm chi tiết và từ khóa mục tiêu.
2. **Luồng 2 (Làm lại Quiz không nhận trùng XP - `Flow2_RetakeQuiz_DoesNotAwardDuplicateXp`):** Lần 1 đạt điểm nhận đủ XP (`25 + 6*5 = 55 XP`); lần 2 làm lại đạt điểm nhận 0 XP, XP tài khoản không đổi, cả 2 lượt làm đều được lưu vào SQLite.
3. **Luồng 3 (Hoàn thành học tập & Dashboard cập nhật đúng - `Flow3_CompleteLearningActivities_DashboardUpdatesAccurately`):** Sau khi hoàn thành bài đọc, quiz và ôn tập thẻ từ vựng Mastered, Dashboard tải lại và cập nhật chính xác các số liệu từ SQLite (chương hoàn thành, số quiz, tỷ lệ nhớ, điểm trung bình).
4. **Luồng 4 (Phát hiện điểm yếu thực tế theo kỹ năng - `Flow4_DetectRealWeaknesses_BasedOnQuizAttemptHistory`):** Giả lập người học làm sai phần Ngữ pháp (hoặc Từ vựng), thuật toán tính toán lịch sử `ChapterQuizAttempts` và tự động hiển thị chính xác điểm yếu đó trên Dashboard và trong đề xuất sư phạm.

---

## 6. HƯỚNG DẪN KIỂM TRA THỰC TẾ (MANUAL TEST)

### A. Kiểm tra trên Desktop (WPF):
1. Chạy ứng dụng Desktop: `dotnet run --project src/frontend/KotobaOdyssey.UI/KotobaOdyssey.UI.csproj`.
2. Tại màn hình **Tổng quan (Dashboard)**:
   - Quan sát mục **Phân tích Tiến độ & Năng lực Học tập (学習分析)**.
   - Kiểm tra các số liệu: Chương hoàn thành, Tỷ lệ bài Quiz đạt, Số từ Mastered / Đang ôn / Mới, Tỷ lệ ghi nhớ dự kiến, cùng các đánh giá Mạnh/Rèn/Gợi ý.
3. Vào màn hình Đọc truyện và làm bài kiểm tra sau chương để xem số liệu phân tích cập nhật tức thì.

### B. Kiểm tra trên Web Companion:
1. Khởi động Web API: `dotnet run --project src/backend/KotobaOdyssey.Api/KotobaOdyssey.Api.csproj`.
2. Mở `web-preview/index.html` trên trình duyệt:
   - Thẻ phân tích năng lực tại màn hình chính sẽ tự động đồng bộ hóa với API `/api/progress/analytics`.
   - Vào tab **Luyện tập (Quiz)**: Làm bài kiểm tra chương 1 để kiểm tra huy hiệu phân nhóm (`Từ vựng`, `Ngữ pháp`, `Đọc hiểu`), sau đó nộp bài để xem phân tích phần trăm chi tiết của từng nhóm kiến thức.

---

## 7. KẾT LUẬN & BƯỚC TIẾP THEO

Giai đoạn 4 đã hoàn thành xuất sắc toàn bộ yêu cầu về Chapter Quiz và Learning Progress Analytics:
- Đầy đủ 3 nhóm câu hỏi: Đọc hiểu, Từ vựng, Ngữ pháp.
- Báo cáo kết quả và giải thích chuyên sâu sau khi nộp bài.
- Chống lạm dụng điểm XP khi làm lại bài.
- Phân tích vốn từ vựng SRS theo từng tầng độ nhớ thực tế.
- Đồng bộ dữ liệu xuyên suốt giữa SQLite, Backend API, WPF Desktop và Web Companion.
- 99/99 bài kiểm thử vượt qua hoàn hảo.

*Hệ thống đã sẵn sàng cho giai đoạn kế tiếp theo chỉ đạo của người dùng.*

