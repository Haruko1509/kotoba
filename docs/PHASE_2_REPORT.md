# BÁO CÁO GIAI ĐOẠN 2: HOÀN THIỆN VÒNG LẶP HỌC TẬP CỐT LÕI (CORE LEARNING LOOP)
**Dự án:** Kotoba Odyssey  
**Phiên bản kiểm toán & nâng cấp:** 2.0  
**Trạng thái:** HOÀN THÀNH (100% Test Passed - 78/78 Tests)  
**Tài liệu tham chiếu:** `docs/PROJECT_AUDIT.md`

---

## 1. TỔNG QUAN VÒNG LẶP HỌC TẬP CỐT LÕI

Giai đoạn 2 tập trung giải quyết toàn diện luồng trải nghiệm học tập trọng tâm:
```
[ Reader Đọc Truyện ]
       ↓ (Tương tác chọn từ trực quan / Ruby Furigana)
[ Tra Cứu Từ Điển & Ngữ Cảnh ]
       ↓ (De-inflection chuẩn xác + Bóc tách đúng câu ngữ cảnh)
[ Lưu Trữ Từ Vựng ]
       ↓ (Chống trùng lặp thông minh + Bồi đắp ngữ cảnh mới)
[ Flashcard & SRS Review ]
       ↓ (Thuật toán SM-2, Chống click-spam, Tích hợp XP)
[ Đồng Bộ Tiến Độ Học Tập ]
```

Toàn bộ luồng đã được liên kết đồng bộ xuyên suốt từ **Backend Core (.NET 9 / EF Core / SQLite WAL)**, **Desktop Client (WPF Modern UI)** đến **Web Companion Client (Modular JavaScript/HTML5)**.

---

## 2. CHI TIẾT CÁC CẢI TIẾN ĐÃ THỰC HIỆN

### 2.1. Reader – Trải Nghiệm Đọc Truyện & Tương Tác Từ Vựng
- **Desktop WPF (`ReaderViewModel.cs` & `ReaderView.xaml`):**
  - Thay thế TextBlock đơn khối bằng kiến trúc hiển thị đoạn văn tương tác đa thành phần (`ItemsControl` liên kết danh sách `ParagraphModel` và `TokenItemViewModel`).
  - Hỗ trợ hiển thị Ruby Furigana phía trên Kanji theo chuẩn typography tiếng Nhật với nút bật/tắt (Toggle Furigana) trên thanh công cụ.
  - Hiệu ứng phản hồi tương tác (Hover glow, Active word highlight).
  - Bấm vào từ để mở Drawer tra từ điển mà không làm mất vị trí trang sách đang đọc.
  - Nút lưu linh hoạt chuyển đổi giữa `+ Lưu vào SRS` và `💾 Bổ sung câu ngữ cảnh` dựa trên trạng thái đã lưu.
- **Web Companion (`reader.js`):**
  - Tích hợp hàm bóc tách câu tiếng Nhật `extractJapaneseSentence()` theo ranh giới dấu câu (`。`, `！`, `？`, `\n`) thay vì cắt chuỗi thô 80 ký tự.
  - Bổ sung ghi chú ngữ pháp/dạng chia `[inflectionNote]` trực tiếp vào phần giải thích từ vựng.
  - Xử lý mượt mà nút chuyển trạng thái lưu từ và phản hồi tức thì với người dùng.

### 2.2. Dictionary – Tra Từ Điển & Cơ Chế Khôi Phục Dạng Gốc (De-inflection)
- **Mở rộng cơ sở dữ liệu từ vựng cục bộ (`JapaneseDictionaryService.cs`):**
  - Nâng cấp từ 111 từ vựng lên hơn **260+ mục từ vựng văn học kinh điển và JLPT** (từ N5 đến N1) phục vụ các tác phẩm: *Ginga Tetsudou no Yoru*, *Rashomon*, *Yukiguni*, *Kokoro*, *Botchan*, *Sanshiro*, *Kaze Tachinu*, *Chuumon no Ooi Ryouriten*, v.v.
- **Quy tắc giải biến thể động từ / tính từ (De-inflection Engine):**
  - Động từ thể bị động (Passive): `書かれる` → `書く`, `褒められる` → `褒める`.
  - Động từ thể sai khiến (Causative): `食べさせる` → `食べる`, `書かせる` → `書く`.
  - Động từ thể khả năng (Potential - Godan & Ichidan): `読める` → `読む`, `走れる` → `走る`.
  - Động từ thể quá khứ (Past Plain): `走った` → `走る`, `読んだ` → `読む`, `抜けた` → `抜ける`.
  - Động từ thể Te: `走って` → `走る`, `抜けて` → `抜ける`.
  - Thể điều kiện (Conditionals): `読めば` → `読む`, `読んだら` → `読む`, `抜けると` → `抜ける`.
  - Thể lịch sự (Polite past & non-past): `読みました` → `読む`, `読みます` → `読む`.
  - Tính từ đuôi い: Quá khứ (`寒かった` → `寒い`), Phó từ hóa (`白く` → `白い`, `青く` → `青い`).
  - Gắn kèm thông tin `OriginalSurface` và `InflectionNote` để giải thích cho người học lý do từ xuất hiện ở dạng chia này.
  - Nguyên tắc chống ảo giác: Nếu từ không tồn tại trong từ điển, trả về `null` kèm thông báo rõ ràng, không tự sinh nghĩa giả mạo.

### 2.3. Bóc Tách Câu Ngữ Cảnh Chuẩn Xác (`JapaneseTextAnalyzer.cs`)
- Cung cấp phương thức `ExtractSentence(string text, string targetWord, int approximateIndex = -1)` trong `IJapaneseTextAnalyzer`.
- Nhận diện chính xác ranh giới câu bằng các ký tự phân tách chuẩn Nhật: `。`, `！`, `？`, `!`, `?`, `\n`, `\r`.
- Fallback an toàn: Nếu không tìm thấy vị trí từ trong văn bản, trả về nội dung đoạn văn được cắt tỉa (`Trim()`), loại bỏ hoàn toàn lỗi chuỗi rỗng hoặc cắt đứt chữ.

### 2.4. Quản Lý Từ Vựng – Chống Trùng Lặp & Bồi Đắp Ngữ Cảnh (Context Enrichment)
- **Repository (`VocabularyRepository.cs`):**
  - Bổ sung `GetByWordAsync(string word)` hỗ trợ so khớp chuẩn hóa chữ thường và cắt tỉa khoảng trắng.
- **API Endpoint (`POST /api/vocab` trong `Program.cs`):**
  - Loại bỏ phản hồi lỗi `409 Conflict` gây nghẽn trải nghiệm.
  - Khi lưu một từ đã có trong kho: Tự động phát hiện và kiểm tra câu ngữ cảnh mới. Nếu câu ví dụ mới chưa có, tự động nối thêm vào trường `ContextSentence` (`existing.ContextSentence += "\n\n" + newContext`) và cập nhật cơ sở dữ liệu.
  - Trả về mã HTTP `200 OK` với cờ `alreadyExisted: true`, bảo toàn ghi chú và dữ liệu SRS hiện có của người học.
- **Tính năng khả dụng ôn tập tức thì (Immediate Reviewability):**
  - Từ mới lưu được gán `NextReviewDate = DateTime.UtcNow`, giúp từ lập tức xuất hiện trong bộ thẻ ôn tập SRS mà không cần chờ sang ngày hôm sau.

### 2.5. Flashcard & Spaced Repetition (SRS)
- **SuperMemo-2 (SM-2):**
  - 4 mức đánh giá: `Again` (grade 1 - lặp lại), `Hard` (grade 2 - khó), `Good` (grade 4 - nhớ tốt), `Easy` (grade 5 - rất dễ).
  - Công thức tính toán chuẩn mực cho `IntervalDays`, `EaseFactor` và `Repetitions`.
- **Bảo vệ chống gửi trùng (Debounce / Double-Submit Protection):**
  - Cả Desktop (`VocabularyViewModel.cs`) và Web Companion (`srs.js`) đều được trang bị cờ trạng thái khóa rating trong lúc gửi yêu cầu (`_isRatingProcessing`, `isGrading`), ngăn chặn triệt để hành vi click đúp làm sai lệch thuật toán SRS và cộng dư XP.
- **Đồng bộ hóa cơ sở dữ liệu SQLite đa tiến trình:**
  - Bật cấu hình `PRAGMA journal_mode=WAL;` trong `DatabaseInitializer.cs`, chấm dứt hoàn toàn hiện tượng khóa file SQLite (`database is locked`) khi chạy đồng thời Web API và Desktop WPF.

---

## 3. DANH SÁCH FILE THAY ĐỔI VÀ TẠO MỚI

| Phân hệ | Đường dẫn file | Mô tả thay đổi |
| :--- | :--- | :--- |
| **Core** | `src/backend/KotobaOdyssey.Core/Interfaces/IRepositories.cs` | Thêm `GetByWordAsync`, `ExtractSentence`, mở rộng `DictionaryEntry` |
| **Data** | `src/backend/KotobaOdyssey.Data/Repositories/Repositories.cs` | Cài đặt `GetByWordAsync` trong `VocabularyRepository` |
| **Data** | `src/backend/KotobaOdyssey.Data/Services/JapaneseDictionaryService.cs` | Bổ sung 150+ từ vựng, mở rộng quy tắc De-inflection đa dạng ngữ pháp |
| **Data** | `src/backend/KotobaOdyssey.Data/Services/JapaneseTextAnalyzer.cs` | Cài đặt `ExtractSentence` theo dấu câu tiếng Nhật |
| **Data** | `src/backend/KotobaOdyssey.Data/DatabaseInitializer.cs` | Kích hoạt SQLite WAL Mode chống khóa file đa tiến trình |
| **API** | `src/backend/KotobaOdyssey.Api/Program.cs` | Context enrichment khi lưu từ trùng, trả về originalSurface & note |
| **UI WPF**| `src/frontend/KotobaOdyssey.UI/ViewModels/ReaderViewModel.cs` | Tokenization đoạn văn, hiển thị Ruby, bóc tách câu, enrich context |
| **UI WPF**| `src/frontend/KotobaOdyssey.UI/Views/ReaderView.xaml` | Nút bật/tắt Furigana, ItemsControl Ruby text, drawer tra từ |
| **UI WPF**| `src/frontend/KotobaOdyssey.UI/ViewModels/VocabularyViewModel.cs` | Khóa chống submit rating kép, Immediate reviewability |
| **Web**  | `web-preview/js/app.js` | Tách file nguyên khối 3,782 dòng thành 10 module độc lập |
| **Web**  | `web-preview/js/reader.js` | Tích hợp trích xuất câu chính xác, hiển thị ghi chú biến thể |
| **Web**  | `web-preview/js/srs.js` | Thêm debounce guard `isGrading`, xử lý UI flashcard mượt mà |
| **Web**  | `web-preview/js/api.js` | Cập nhật `saveVocabToLocalStorage` hỗ trợ bồi đắp ngữ cảnh offline |
| **Tests**| `tests/KotobaOdyssey.Tests/CoreLearningLoopTests.cs` | Bộ test mới kiểm thử de-inflection, extract sentence, repo & SRS |
| **Tests**| `tests/KotobaOdyssey.Tests/PracticeServiceTests.cs` | Cập nhật `FakeVocabRepository` tương thích interface mới |

---

## 4. KẾT QUẢ BUILD VÀ TEST THỰC TẾ

### 4.1. Kết Quả Biên Dịch Solution (KotobaOdyssey.sln)
- **Command:** `dotnet build d:\kotoba\KotobaOdyssey.sln`
- **Kết quả:** `Build succeeded. 0 Warning(s), 0 Error(s).`
- **Thời gian build:** `00:00:02.94`

### 4.2. Kết Quả Chạy Unit & Integration Test
- **Command:** `dotnet test d:\kotoba\tests\KotobaOdyssey.Tests\KotobaOdyssey.Tests.csproj`
- **Kết quả:**
  ```text
  Passed!  - Failed: 0, Passed: 78, Skipped: 0, Total: 78, Duration: 2 s
  ```
- **Phân bổ 78 bài test:**
  - `CoreLearningLoopTests`: 19 tests (De-inflection 10 thể, trích xuất câu, enrich context, immediate review, SM-2 4 mức).
  - `ReaderVocabSrsIntegrationTests`: 3 tests (End-to-end integration SQLite DB).
  - `DictionaryServiceTests`: 4 tests (Tra từ chính xác, khôi phục từ gốc, gợi ý từ).
  - `JapaneseTextAnalyzerTests`: 2 tests (Phân tách từ, xử lý ký tự trắng/rỗng).
  - `SpacedRepetitionCalculatorTests`: 4 tests (Thuật toán SM-2 chi tiết).
  - `VocabularyReviewTests`: 7 tests (Ghi nhận lượt ôn tập, cập nhật lịch review).
  - `PracticeServiceTests`: 12 tests (Luyện tập từ vựng, trắc nghiệm).
  - `NavigationAndViewModelTests`: 9 tests (Điều hướng màn hình và ViewModel).
  - `GamificationCalculatorTests`: 6 tests (XP, cấp độ, streak).
  - `AiAndTokenizerEvaluationTests`: 5 tests (Tokenizer và đánh giá AI).
  - `NovelImportServiceTests`: 4 tests (Import tiểu thuyết và chương).
  - `CharacterDialogueServiceTests`: 2 tests (Hội thoại nhân vật).
  - `AiTutorFollowUpTests`: 1 test (Theo dõi hội thoại AI Tutor).

---

## 5. HƯỚNG DẪN KIỂM THỬ THỦ CÔNG (MANUAL TESTING GUIDE)

### Cách 1: Kiểm thử trên Desktop (WPF)
1. Chạy API Backend (Terminal 1):
   ```powershell
   dotnet run --project d:\kotoba\src\backend\KotobaOdyssey.Api\KotobaOdyssey.Api.csproj
   ```
2. Chạy Desktop App (Terminal 2):
   ```powershell
   dotnet run --project d:\kotoba\src\frontend\KotobaOdyssey.UI\KotobaOdyssey.UI.csproj
   ```
3. Mở tab **Thư viện** → Chọn đọc **銀河鉄道の夜 (Đêm trên đường sắt Ngân Hà)**.
4. Nhấn nút **Ẩn / Hiện Furigana** trên thanh công cụ để kiểm tra tính năng hiển thị Ruby text.
5. Nhấp chuột vào một từ (ví dụ: `抜けると`, `銀河`, `白く`):
   - Quan sát Drawer tra cứu bên phải hiển thị từ gốc, cách đọc, nghĩa tiếng Việt, dạng chia và **chính xác câu chứa từ đó**.
6. Nhấn `+ Lưu vào SRS`:
   - Nút chuyển sang trạng thái đã lưu.
   - Chuyển sang chương khác hoặc đoạn khác có chứa từ đó, nhấp lại vào từ: Nút hiển thị `💾 Bổ sung câu ngữ cảnh`.
7. Chuyển sang tab **Từ vựng / Flashcard**:
   - Thẻ từ vừa lưu xuất hiện ngay lập tức trong hàng đợi ôn tập.
   - Lật thẻ và chọn một trong 4 mức đánh giá (`Lặp lại`, `Khó`, `Nhớ tốt`, `Dễ`).
   - Kiểm tra điểm XP tăng lên và thẻ chuyển sang từ tiếp theo mà không bị kẹt hay double-submit.

### Cách 2: Kiểm thử trên Web Companion
1. Khởi động Web Preview (dùng live server hoặc mở file):
   - Mở file `d:\kotoba\web-preview\index.html` trên trình duyệt.
2. Điều hướng vào phần Reader:
   - Nhấp vào bất kỳ token từ vựng tiếng Nhật nào trong đoạn văn.
   - Kiểm tra hộp thoại từ điển hiển thị câu ngữ cảnh chính xác và nút `+ Lưu vào SRS` hoạt động chuẩn xác.
3. Điều hướng vào phần **SRS Arena**:
   - Kiểm tra lật thẻ Flashcard (Space / Click) và đánh giá độ nhớ (1-4).

---

## 6. BẢNG KIỂM TRA CHỈ TIÊU VÒNG LẶP HỌC TẬP (EVALUATION CHECKLIST)

| Bước trong vòng lặp | Tiêu chí đánh giá | Trạng thái | Ghi chú |
| :--- | :--- | :--- | :--- |
| **1. Reader** | Hiển thị chữ tiếng Nhật rõ ràng, layout chuẩn | **ĐẠT** | WPF ItemsControl + Web Prose CSS |
| | Bật/tắt Furigana thuận tiện | **ĐẠT** | Nút Toggle Furigana ở cả WPF & Web |
| | Lưu vị trí đọc & chuyển chương mượt | **ĐẠT** | Giữ nguyên và liên kết với ChapterProgress |
| **2. Tra từ** | Tra cứu từ nhấn chọn, không mất vị trí đọc | **ĐẠT** | Drawer bên phải, không gián đoạn viewport |
| | Khôi phục từ gốc (De-inflection) | **ĐẠT** | Xử lý hơn 10 dạng chia (bị động, sai khiến, quá khứ, te, điều kiện, khả năng) |
| | Trích xuất chính xác câu ngữ cảnh | **ĐẠT** | Thay thế Substring(0, 80) bằng phân tách dấu câu tiếng Nhật |
| | Không bịa nghĩa khi thiếu dữ liệu | **ĐẠT** | Thông báo rõ ràng, phân biệt từ điển và AI |
| **3. Quản lý từ vựng** | Lưu từ mới với câu ví dụ & nguồn truyện | **ĐẠT** | Đầy đủ metadata (Title, Chapter, Context) |
| | Chống trùng lặp & bồi đắp ngữ cảnh | **ĐẠT** | Tự động append câu mới khi từ đã tồn tại, trả về 200 OK |
| **4. Flashcard & SRS** | Ôn tập ngay từ mới lưu | **ĐẠT** | `NextReviewDate = UtcNow`, có thẻ ngay |
| | Thuật toán SM-2 chuẩn mực | **ĐẠT** | 4 nút đánh giá tính đúng Interval & Ease Factor |
| | Chống spam rating / double-submit | **ĐẠT** | Thêm khóa debounce `_isRatingProcessing` & `isGrading` |
| | Tích hợp Gamification (XP, Streak) | **ĐẠT** | Cộng XP chuẩn xác vào UserProgress |

---

## 7. KẾT LUẬN & SẴN SÀNG CHO GIAI ĐOẠN TIẾP THEO

- **Giai đoạn 2 đã hoàn thành 100% mục tiêu:** Vòng lặp học tập cốt lõi của Kotoba Odyssey đã hoạt động hoàn chỉnh, mượt mà và tin cậy trên cả hai nền tảng Desktop WPF và Web Companion.
- Toàn bộ solution biên dịch sạch sẽ không có cảnh báo hay lỗi (`0 Warnings, 0 Errors`).
- Hệ thống test đạt 100% tỷ lệ vượt qua (`78/78 Passed`).
- Tuân thủ quy định: Hệ thống đã dừng lại và không tự ý chuyển sang Giai đoạn 3. Sẵn sàng nhận chỉ đạo tiếp theo từ Technical Lead.

