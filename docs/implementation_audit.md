# KOTOBA ODYSSEY — BÁO CÁO AUDIT TOÀN DIỆN MÃ NGUỒN (PHASE 1)
**Ngày thực hiện:** 27/10/2026  
**Phiên bản audit:** v1.0.0  
**Tác giả:** Senior Software Engineer & AI Engineer  

---

## I. TỔNG QUAN HỆ THỐNG VÀ HIỆN TRẠNG REPOSITORY

### 1. Trạng thái Git & Workspace
* **Commit gần nhất:** `bcbadec chore: backup project state before Astral Journey complete redesign` (20/10/2026).
* **Trạng thái Working Tree:**
  * Các file bị sửa đổi chưa commit: `KotobaOdyssey.sln`, `DatabaseInitializer.cs`, `KotobaDbContext.cs`, `Repositories.cs`, `web-preview/app.js`, `web-preview/index.html`, `web-preview/style.css`.
  * Các file untracked mới:
    * `src/backend/KotobaOdyssey.Api/` (Minimal API REST server port 5000)
    * `src/backend/KotobaOdyssey.Core/DTOs/` (`AiDtos.cs`, `CharacterChatDtos.cs`, `PracticeDtos.cs`)
    * `src/backend/KotobaOdyssey.Core/Interfaces/` (`IAiTutorService.cs`, `IChapterPracticeService.cs`, `ICharacterDialogueService.cs`)
    * `src/backend/KotobaOdyssey.Data/Services/` (`ChapterPracticeService.cs`, `GeminiAiTutorService.cs`, `GeminiCharacterDialogueService.cs`)
    * `tests/KotobaOdyssey.Tests/` (`CharacterDialogueServiceTests.cs`, `PracticeServiceTests.cs`)
    * `web-preview/assets/` (Sprite anime nhân vật và background Shinkai)
* **Kết quả Unit Tests hiện tại:**
  * Tổng số: **46/46 tests passed (100%)**, thời gian thực thi ~3 giây. Không có test nào bị fail hoặc skip.

### 2. Kiến trúc giải pháp (Architecture Map)
* **KotobaOdyssey.Core**: Chứa Domain Models (`Novel`, `Chapter`, `VocabularyItem`, `Bookmark`, `ReviewLog`, `UserProgress`, `DailyQuest`, `Achievement`), Enums, Interfaces (`INovelRepository`, `IVocabularyRepository`, v.v.), và thuật toán lõi thuần túy (`SpacedRepetitionCalculator` SM-2, `GamificationCalculator`).
* **KotobaOdyssey.Data**: SQLite DbContext (`KotobaDbContext`), Repositories, Database Seeding (`DatabaseInitializer` với 6 tác phẩm kinh điển: Ginga Tetsudou no Yoru, Botchan, Gon-gitsune, Yukiguni, Hashire Melos, Rashomon), Services (`NovelImportService`, `JapaneseDictionaryService`, `JapaneseTextAnalyzer`, `GeminiAiTutorService`, `GeminiCharacterDialogueService`, `ChapterPracticeService`).
* **KotobaOdyssey.Api**: ASP.NET Core Minimal API trên port 5000 cung cấp các REST endpoints cho client web companion.
* **KotobaOdyssey.UI**: Desktop client WPF (.NET 9 Windows) theo mô hình MVVM (`CommunityToolkit.Mvvm`).
* **web-preview**: Web client SPA kết hợp phong cách Satori Reader × HoYoLAB.

---

## II. MA TRẬN ĐÁNH GIÁ TÍNH NĂNG (VERIFICATION MATRIX)

| Phân hệ / Tính năng | Trạng thái thực tế | Cơ chế hoạt động (Thực / Mock / Fallback) | Rủi ro / Vấn đề phát hiện |
| :--- | :---: | :--- | :--- |
| **Novel & Chapter Library** | ✅ Hoạt động tốt | Đọc trực tiếp từ SQLite DB / EF Core, hỗ trợ import `.txt` và `.json`. | Chưa có pagination khi thư viện truyện mở rộng rất lớn. |
| **Dictionary Engine** | ⚠️ Hoạt động một phần | In-memory `Dictionary<string, DictionaryEntry>` với thuật toán de-inflection cơ bản. | Quy mô từ vựng mẫu còn nhỏ (~30-50 từ), chưa tích hợp JMdict. Mismatch tên trường DTO giữa API và Web. |
| **NLP Tokenizer** | ⚠️ Hoạt động một phần | Longest Prefix Match + gom nhóm theo Unicode ScriptType (Kanji/Hiragana/Katakana). | Chưa xử lý được các thể chia động từ phức tạp (bị động, sai khiến, thể て kép). |
| **SRS SM-2 Engine** | ✅ Hoạt động tốt | Tính toán chuẩn xác các thông số `IntervalDays`, `EaseFactor`, `Repetitions`, `NextReviewDate`. 100% tests pass. | Web chưa xử lý đồng bộ mượt mà khi backend offline. |
| **Gamification (XP/Streak)** | ✅ Hoạt động tốt | Công thức tính XP, thăng cấp, chuỗi học theo ngày chuẩn xác. | Quiz Submit đang cộng XP không kiểm soát, thiếu chống trùng lặp. |
| **AI Tutor (Gemini)** | ⚠️ Hoạt động một phần | Gọi API `gemini-1.5-flash` nếu có `GEMINI_API_KEY`. Nếu không, dùng fallback rule-based. | Fallback trả về chuỗi tĩnh *"Bản dịch theo ngữ cảnh văn học của tiểu thuyết"*, chưa hỗ trợ hỏi tiếp (follow-up). |
| **AI Character Chat** | ⚠️ Hoạt động một phần | Prompt nhập vai persona theo nhân vật kinh điển qua Gemini API. | Chưa giới hạn chặt chẽ theo từng chương truyện, chưa lưu lịch sử chat vào SQLite. |
| **Chapter Quiz** | ⚠️ Rủi ro logic | Bộ câu hỏi biên soạn thủ công theo từng chương. | **Lỗi nghiêm trọng:** Không lưu lịch sử kết quả thi; gửi lại request sẽ bị cộng dồn XP vô hạn; không cập nhật `IsCompleted` của chương. |
| **Web Companion UI** | ❌ Nhiều lỗi ngầm | Giao diện HTML/CSS phong phú nhưng logic JS có nhiều lỗ hổng nghiêm trọng. | Nút lưu từ ảo, gọi hàm không tồn tại (`fetchSrsDeck`, `fetchUserProgress`), không đồng bộ offline. |

---

## III. CHI TIẾT CÁC LỖI VÀ RỦI RO PHÁT HIỆN ĐƯỢC

### 1. Lỗi nghiêm trọng tại Web Companion (`web-preview/app.js`)
* **Lỗi 1 (Mismatch thuộc tính tra từ điển - Silent Failure):**
  * Tại `app.js` (dòng 1105): `if (dictRes && dictRes.word) { ... }`
  * Trong khi DTO `DictionaryEntry` của backend trả về thuộc tính `term` (chuyển sang camelCase là `term`), KHÔNG CÓ thuộc tính `word`.
  * **Hậu quả:** `dictRes.word` luôn là `undefined`, khối lệnh gán dữ liệu không bao giờ chạy, Web luôn rơi vào nhánh fallback hiển thị nghĩa mặc định *"Đang tra cứu từ điển..."*.
* **Lỗi 2 (Nút lưu từ vựng ảo - Fake Save Button):**
  * Tại `app.js` (dòng 2753):
    ```javascript
    function saveTokenToSrs(word, furigana, meaning, level, btn) {
      playSuccessChime();
      if (btn) {
        btn.textContent = '✓ Đã thêm SRS!';
        btn.style.color = '#10B981';
        btn.disabled = true;
      }
    }
    ```
  * **Hậu quả:** Khi người dùng click nút "+ Thêm thẻ SRS" từ phân tích cú pháp câu, ứng dụng chỉ phát âm thanh và đổi text nút, hoàn toàn KHÔNG gọi API `/api/vocab`, KHÔNG lưu vào SQLite hay bộ nhớ.
* **Lỗi 3 (Gọi hàm chưa được định nghĩa - Runtime ReferenceError):**
  * Tại `app.js` (các dòng 1872, 2320, 2321, 2382, 3293): Gọi `fetchSrsDeck()` và `fetchUserProgress()`.
  * Tuy nhiên, cả hai hàm này **hoàn toàn không được khai báo** trong toàn bộ file `app.js`.
  * **Hậu quả:** Gây ra lỗi `Uncaught ReferenceError: fetchSrsDeck is not defined` và `fetchUserProgress is not defined`, làm gián đoạn luồng xử lý sau khi làm bài Quiz hoặc chat với nhân vật.
* **Lỗi 4 (Không xử lý lỗi HTTP 409 Conflict & Offline):**
  * Tại `app.js` (dòng 1343 & 2305): Khi lưu từ đã tồn tại, backend trả về HTTP 409 Conflict. Hàm `fetchFromBackend` bắt lỗi và trả về `null`.
  * Nhưng UI vẫn mặc định cập nhật: `btn.textContent = '✓ Đã lưu SRS!'` và hiện toast `Đã lưu từ vựng! (+10 XP)`. Người dùng bị đánh lừa là đã lưu thành công.
* **Lỗi 5 (Thiếu cơ chế Local Storage Fallback):**
  * `web-preview` không sử dụng `localStorage` để lưu trữ ngoại tuyến khi backend chưa chạy, dẫn đến toàn bộ từ vựng mới hoặc tiến độ đọc bị mất sạch khi F5 trình duyệt.

### 2. Lỗi và thiếu sót tại Backend (`KotobaOdyssey.Api` & `KotobaOdyssey.Data`)
* **Lỗi 6 (Cộng dồn XP vô hạn và không lưu kết quả bài Quiz):**
  * Trong `ChapterPracticeService.cs` (dòng 147): Mỗi lần gọi `SubmitQuizAsync`, service trực tiếp gọi `_progressRepository.AddXpAsync(xpEarned)`.
  * Không có bảng dữ liệu `QuizResult` hoặc `QuizAttempt` trong `KotobaDbContext`.
  * Người dùng có thể spam nút nộp bài để cày XP vô hạn.
  * Chưa gắn kết việc hoàn thành Quiz với việc đánh dấu `Chapter.IsCompleted = true`.
* **Lỗi 7 (AI Tutor fallback trả về text giả và thiếu ngữ cảnh / follow-up):**
  * Trong `GeminiAiTutorService.cs` (dòng 218): Khi không có `GEMINI_API_KEY`, trường `TranslationVn` trả về cứng: `"Bản dịch theo ngữ cảnh văn học của tiểu thuyết."`.
  * `AiSentenceExplanationRequest` chỉ nhận 3 tham số (`Sentence`, `Context`, `JlptLevel`), thiếu: `NovelTitle`, `ChapterTitle`, `RequestType` (dịch/ngữ pháp/hỏi tiếp), và lịch sử câu hỏi follow-up.
  * Backend chưa có file `appsettings.json` trong `KotobaOdyssey.Api` để cấu hình key an toàn.
* **Lỗi 8 (WPF ReaderViewModel kiểm tra từ đã lưu bị lệch thể):**
  * Trong `ReaderViewModel.cs` (dòng 285): `IsWordAlreadySaved = await _vocabularyRepository.ExistsAsync(LookupTerm);`.
  * Nếu người dùng tra từ chia thể (ví dụ: `見ました`), `LookupTerm` là `見ました`, nhưng từ lưu vào DB lại là thể nguyên mẫu `見る` (`LookupResult.Term`). Dẫn đến việc `ExistsAsync` trả về `false` dù từ gốc đã có trong sổ tay.

---

## IV. DANH SÁCH FILE CẦN CHỈNH SỬA & BỔ SUNG

### 1. Backend (`src/backend/`)
1. `KotobaOdyssey.Core/DTOs/AiDtos.cs`:
   * Bổ sung các trường vào `AiSentenceExplanationRequest`: `NovelTitle`, `ChapterNumber`, `RequestType`, `FollowUpQuestion`, `PreviousExplanation`.
   * Bổ sung vào `AiSentenceExplanationResult`: `KeyVocabulary`, `Uncertainties`, `IsAiGenerated`.
2. `KotobaOdyssey.Core/DTOs/PracticeDtos.cs`:
   * Bổ sung `UserId`, `AttemptId`, `IsFirstCompletion` để kiểm soát cấp phát XP.
3. `KotobaOdyssey.Core/Models/DomainModels.cs` & `KotobaDbContext.cs`:
   * Bổ sung Entity `ChapterQuizAttempt` (NovelId, ChapterNumber, Score, Passed, XpAwarded, AttemptedAt) để lưu lịch sử và ngăn chặn spam XP.
4. `KotobaOdyssey.Data/Services/GeminiAiTutorService.cs`:
   * Nâng cấp prompt Gemini có ngữ cảnh chương và phân biệt rõ request type (dịch, ngữ pháp, follow-up).
   * Thay thế fallback tĩnh bằng dynamic rule-based translation/analysis trung thực, ghi rõ `IsAiGenerated = false`.
5. `KotobaOdyssey.Data/Services/ChapterPracticeService.cs`:
   * Kiểm tra lịch sử làm bài trước khi cấp XP (chỉ cấp XP một lần duy nhất khi pass lần đầu).
   * Cập nhật `Chapter.IsCompleted = true` khi đạt yêu cầu đọc hiểu.
6. `KotobaOdyssey.Api/Program.cs`:
   * Bổ sung cấu hình `appsettings.json` / User Secrets cho Gemini.
   * Cập nhật endpoint `/api/vocab` để trả về thông báo lỗi rõ ràng (Conflict, BadRequest).
   * Bổ sung endpoint follow-up cho AI Tutor.

### 2. Frontend WPF (`src/frontend/KotobaOdyssey.UI/`)
1. `ViewModels/ReaderViewModel.cs`:
   * Sửa lỗi kiểm tra trùng từ dựa trên `LookupResult.Term` thay vì `LookupTerm`.
   * Bổ sung xử lý hiển thị trạng thái từ đã lưu và ngữ cảnh.

### 3. Web Companion (`web-preview/`)
1. `web-preview/app.js`:
   * Sửa lỗi `dictRes.term` thay vì `dictRes.word`.
   * Triển khai hàm `saveTokenToSrs` thực sự gọi API `/api/vocab` kèm câu ngữ cảnh.
   * Xử lý chính xác mã lỗi HTTP (409 Conflict, 400 Bad Request, Network Error) với thông báo phù hợp.
   * Khai báo và hiện thực đầy đủ `fetchSrsDeck()` và `fetchUserProgress()`.
   * Bổ sung tầng lưu trữ dự phòng `localStorage` khi offline.
   * Cập nhật modal AI Tutor hỗ trợ chọn chế độ (Dịch / Ngữ pháp / Hỏi tiếp) kèm trạng thái Loading, Error, Retry.
2. `web-preview/index.html` & `style.css`:
   * Bổ sung UI hộp thoại hỏi tiếp (Follow-up chat) trong Panel AI Tutor.
   * Bổ sung badge hiển thị nguồn dữ liệu: `🤖 Gemini AI` hoặc `⚡ Local Engine`.

### 4. Tests (`tests/KotobaOdyssey.Tests/`)
1. Thêm Integration Tests cho 3 luồng học tập:
   * `ReaderVocabSrsIntegrationTests.cs` (Luồng A)
   * `AiTutorFollowUpTests.cs` (Luồng B)
   * `QuizProgressIntegrationTests.cs` (Luồng C)

---

## V. KẾ HOẠCH THỰC HIỆN TỐI THIỂU & THỨ TỰ TRIỂN KHAI

Tuân thủ nguyên tắc không refactor ồ ạt, chia nhỏ thành các bước kiểm chứng tuần tự:

```
[Giai đoạn 1: Audit & Lập kế hoạch] ── (HOÀN TẤT)
                 │
                 ▼
[Giai đoạn 2.A: Hoàn thiện Luồng Reader → Dictionary → Vocabulary/SRS]
  1. Fix API & DTO Dictionary mapping (term vs word)
  2. Triển khai saveTokenToSrs thực sự gọi API & SQLite
  3. Xử lý triệt để chống trùng (409 Conflict), từ đã tồn tại, fallback localStorage
  4. Fix WPF ReaderViewModel kiểm tra từ gốc
  5. Viết Unit & Integration Tests cho Luồng A ──► CHẠY KIỂM THỬ ĐẠT 100%
                 │
                 ▼
[Giai đoạn 2.B: Hoàn thiện Luồng Reader → AI Tutor → Follow-up]
  1. Mở rộng AiDtos với ngữ cảnh tác phẩm, chương, request type, follow-up
  2. Cập nhật GeminiAiTutorService: prompt chặt chẽ, xử lý timeout/error, loại bỏ text giả
  3. Triển khai giao diện AI Panel với Loading, Error, Retry và ô hỏi tiếp
  4. Viết Unit & Integration Tests cho Luồng B ──► CHẠY KIỂM THỬ ĐẠT 100%
                 │
                 ▼
[Giai đoạn 2.C: Hoàn thiện Luồng Chapter → Quiz → Progress/XP]
  1. Thêm lưu vết kết quả QuizAttempt vào DB
  2. Chống lặp XP khi nộp lại quiz, cập nhật hoàn thành chương
  3. Sửa hàm fetchSrsDeck và fetchUserProgress trong app.js
  4. Viết Unit & Integration Tests cho Luồng C ──► CHẠY KIỂM THỬ ĐẠT 100%
                 │
                 ▼
[Giai đoạn 3: Nâng chất lượng NLP & Đánh giá AI (docs/ai_evaluation.md)]
                 │
                 ▼
[Giai đoạn 4: Hoàn thiện AI Character Conversation]
```

---

## VI. PHƯƠNG PHÁP KIỂM CHỨNG KẾT QUẢ

1. **Kiểm chứng tự động (Automated Testing):**
   * Chạy lệnh `dotnet test` đảm bảo toàn bộ 46 tests cũ tiếp tục pass 100%.
   * Viết thêm tối thiểu 10 integration tests mới cho các trường hợp: lưu trùng từ vựng, nộp quiz lần 2 không cộng XP, hỏi tiếp AI tutor với lịch sử câu hỏi.
2. **Kiểm chứng thủ công luồng A (Reader → SRS):**
   * Mở Web Companion, click vào một từ trong tiểu thuyết (ví dụ `銀河`).
   * Xác minh pop-up từ điển hiện đúng thông tin từ backend API.
   * Bấm "+ Lưu vào SRS" -> kiểm tra DB SQLite có record mới trong bảng `VocabularyItems`.
   * Bấm lại lần nữa -> kiểm tra hệ thống báo từ đã có, không tạo record trùng.
   * Mở màn hình SRS Flashcard -> thẻ từ vựng mới xuất hiện và cho phép đánh giá 1-4.
   * F5 tải lại trang -> từ vựng và lịch ôn tập vẫn tồn tại đầy đủ.

---
*Báo cáo được hoàn thành trong Giai đoạn 1. Sẵn sàng chuyển sang Giai đoạn 2.A sau khi người dùng xác nhận.*
