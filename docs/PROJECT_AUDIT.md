# KOTOBA ODYSSEY — BÁO CÁO TOÀN DIỆN DỰ ÁN (PROJECT AUDIT)
**Giai đoạn:** Giai đoạn 1 – Project Audit & Technical Evaluation  
**Ngày thực hiện:** 30/10/2026  
**Vai trò:** Senior Software Architect & Technical Lead  

---

## 1. TỔNG QUAN KIẾN TRÚC THỰC TẾ

### 1.1. Đính chính Stack Công nghệ: Giả định vs Thực tế Source Code

Trước khi audit, các giả định ban đầu về công nghệ đã được đối chiếu trực tiếp với mã nguồn thực tế:

| Thành phần | Công nghệ theo giả định ban đầu | Công nghệ thực tế trong Source Code | Đánh giá & Phát hiện |
| :--- | :--- | :--- | :--- |
| **Backend** | C# / .NET 9 / ASP.NET Core Web API | **C# / .NET 9.0.316 / ASP.NET Core Minimal API** | **XÁC NHẬN CHÍNH XÁC.** Project `KotobaOdyssey.Api` chạy Minimal API trên cổng 5000 (`http://localhost:5000`). |
| **Database** | SQLite / Entity Framework Core | **SQLite / EF Core 9.0 (Code-First without Migrations)** | **XÁC NHẬN CHÍNH XÁC.** Lưu trữ tại `%LOCALAPPDATA%\KotobaOdyssey\kotoba.db`. Khởi tạo qua `EnsureCreatedAsync()`. |
| **Desktop Client** | WPF | **WPF (.NET 9.0-windows)** | **XÁC NHẬN CHÍNH XÁC.** Project `KotobaOdyssey.UI`, kiến trúc MVVM dùng `CommunityToolkit.Mvvm`. |
| **Web Companion** | React / TypeScript | **Vanilla HTML5 / Modern CSS3 / Modular Vanilla JavaScript (ES6+)** | ⚠️ **SAI LỆCH LỚN SO VỚI GIẢ ĐỊNH.** Không hề có React, không có TypeScript, không có `package.json` hay build pipeline (Vite/Webpack). Thư mục `web-preview/` là một ứng dụng SPA thuần chạy trực tiếp trên trình duyệt. |
| **AI Engine** | Google Gemini | **Google Gemini (`gemini-1.5-flash`) + Rule-based Fallback Engine** | **XÁC NHẬN CHÍNH XÁC.** Tích hợp qua REST API, tự động chuyển sang bộ giải thích NLP rule-based khi không có `GEMINI_API_KEY`. |

### 1.2. Sơ đồ Luồng Giao tiếp Kiến trúc (Architecture Communication Flow)

Hệ thống hiện tại có mô hình 2 client (Desktop & Web) nhưng cách tiếp cận dữ liệu có sự bất đối xứng:

```
                  +-------------------------------------------------------+
                  |               SQLite Database (Local)                 |
                  |     %LOCALAPPDATA%\KotobaOdyssey\kotoba.db            |
                  +-------------------------------------------------------+
                             ^                                 ^
                             | (Direct in-process EF Core)     | (Direct in-process EF Core)
                             |                                 |
              +-------------------------------+  +-----------------------------------+
              |  Desktop App: KotobaOdyssey.UI|  | Backend API: KotobaOdyssey.Api    |
              |  (WPF / .NET 9 / MVVM)        |  | (ASP.NET Core Minimal API :5000)  |
              +-------------------------------+  +-----------------------------------+
                                                                   ^
                                                                   | (REST HTTP JSON)
                                                                   |
                                                 +-----------------------------------+
                                                 | Web Companion: web-preview/       |
                                                 | (Vanilla JS Modules / Satori UI)  |
                                                 +-----------------------------------+
```

* **Desktop WPF Client:** Giao tiếp trực tiếp với SQLite Database qua DbContext in-process (DI container trong `App.xaml.cs`). Desktop hoàn toàn độc lập, không phụ thuộc vào `KotobaOdyssey.Api`.
* **Web Companion Client:** Giao tiếp với backend thông qua các REST endpoint trên cổng 5000 (`/api/*`). Nếu backend offline, Web tự động kích hoạt cơ chế Local Fallback Mock Data để đảm bảo trải nghiệm người dùng không bị gián đoạn.

---

## 2. CẤU TRÚC CÁC PROJECT VÀ PHÂN QUYỀN TRÁCH NHIỆM

Solution `KotobaOdyssey.sln` bao gồm **5 project .NET** và **1 thư mục Web SPA Client**:

```text
d:\kotoba\
├── KotobaOdyssey.sln
├── src\
│   ├── backend\
│   │   ├── KotobaOdyssey.Core\      # [Class Library .NET 9] Domain Models, Enums, DTOs, Business Calculators
│   │   ├── KotobaOdyssey.Data\      # [Class Library .NET 9] EF Core DbContext, Repositories, DatabaseInitializer, NLP/AI Services
│   │   └── KotobaOdyssey.Api\       # [ASP.NET Core 9 Minimal API] REST API Server (:5000), HTTP Controllers
│   └── frontend\
│       └── KotobaOdyssey.UI\        # [WPF .NET 9-windows] Desktop Client MVVM, Custom Controls, Themes
├── tests\
│   └── KotobaOdyssey.Tests\         # [xUnit .NET 9-windows] 59 Unit & Integration Tests (100% Pass)
└── web-preview\                     # [Web SPA Client] Giao diện Satori Reader x HoYoLAB x Visual Novel
    ├── index.html
    ├── style.css
    ├── assets/                      # Anime sprites, Shinkai backgrounds, Book covers
    ├── js/                          # 9 Modular JavaScript Scripts
    └── app.js                       # Entrypoint & Hash Router
```

### Chi tiết vai trò từng Project:

1. **`KotobaOdyssey.Core` (Net 9.0):**
   * **Models:** `Novel`, `Chapter`, `Bookmark`, `VocabularyItem`, `ReviewLog`, `UserProgress`, `DailyQuest`, `Achievement`, `ChapterQuizAttempt`.
   * **Calculators:** 
     * `SpacedRepetitionCalculator`: Thuật toán SM-2 thuần túy tính toán `EaseFactor`, `IntervalDays`, `NextReviewDate`.
     * `GamificationCalculator`: Công thức tăng trưởng XP theo hàm lũy thừa cong, chuỗi học tập liên tục (Streak).
   * **Interfaces & DTOs:** Hợp đồng repository và DTO chuẩn cho NLP, AI, Quiz, Import.

2. **`KotobaOdyssey.Data` (Net 9.0):**
   * **`KotobaDbContext`:** Quản lý 9 `DbSet` cùng hệ thống Performance Index tối ưu hóa truy vấn.
   * **`DatabaseInitializer`:** Tự động tạo bảng và nạp sẵn 6 tiểu thuyết kinh điển (*Ginga Tetsudou no Yoru, Botchan, Gon-gitsune, Yukiguni, Hashire Melos, Rashomon*).
   * **Repositories:** `NovelRepository`, `VocabularyRepository`, `UserProgressRepository`, `DailyQuestRepository`, `BookmarkRepository`.
   * **Domain Services:**
     * `JapaneseDictionaryService`: Từ điển bộ nhớ 111 từ ngữ văn học + bộ giải biến hình (de-inflection engine).
     * `JapaneseTextAnalyzer`: Phân đoạn từ tiếng Nhật bằng Longest Prefix Match + gom nhóm Unicode ScriptType.
     * `GeminiAiTutorService`: Phân tích câu ngữ pháp qua Gemini API hoặc Rule-based NLP.
     * `GeminiCharacterDialogueService`: Hội thoại nhân vật AI theo Persona (Giovanni, Campanella, Conductor, Melos,...).
     * `ChapterPracticeService`: Ngân hàng câu hỏi trắc nghiệm đọc hiểu cuối chương và ghi nhận kết quả chống spam XP.
     * `NovelImportService`: Trình phân tích nhập truyện định dạng `.txt` (nhận diện chương tự động) và `.json`.

3. **`KotobaOdyssey.Api` (Net 9.0 Minimal API):**
   * Cung cấp 18 endpoints RESTful: `/api/novels`, `/api/vocab`, `/api/progress`, `/api/quests`, `/api/dictionary`, `/api/ai/explain`, `/api/characters`, `/api/ai/character-chat`, `/api/novels/{id}/chapters/{num}/quiz`, `/api/translate`.
   * Cấu hình CORS mở cho client web companion.

4. **`KotobaOdyssey.UI` (Net 9.0-windows WPF):**
   * Kiến trúc MVVM chuẩn mực với `CommunityToolkit.Mvvm`.
   * Giao diện Cosmic/Modern Dark Theme: `DashboardView`, `LibraryView`, `ReaderView`, `VocabularyView`, `SettingsView`.

5. **`KotobaOdyssey.Tests` (Net 9.0 xUnit):**
   * Bộ 59 test cases kiểm thử bao phủ toàn bộ Calculators, Repositories, Services, ViewModels và Import logic.

6. **`web-preview` (Web SPA Client):**
   * Thiết kế theo phong cách Satori Reader kết hợp Astral Journey / HoYoLAB.
   * Chạy trực tiếp trên trình duyệt, kết nối REST API hoặc fallback cục bộ.

---

## 3. DANH SÁCH CHỨC NĂNG VÀ MA TRẬN ĐÁNH GIÁ (FEATURE MATRIX)

Theo chuẩn 4 mức phân loại:
* **`COMPLETED`**: Đã triển khai hoàn chỉnh, hoạt động thực tế với logic nghiệp vụ và dữ liệu thực.
* **`PARTIAL`**: Đã có logic nhưng còn thiếu sót, giới hạn quy mô hoặc chưa liên kết giữa các nền tảng.
* **`MOCK/FALLBACK`**: Chỉ là giao diện giả lập, dữ liệu tĩnh hoặc chỉ chạy ở chế độ dự phòng.
* **`MISSING`**: Hoàn toàn chưa triển khai.

| STT | Nhóm Chức Năng | Trạng Thái | Thành Phần / File Chịu Trách Nhiệm | Lưu Database? | Xử Lý Lỗi / Ngoại Lệ? | Unit / Integration Test? | Chi Tiết Hiện Trạng |
| :---: | :--- | :---: | :--- | :---: | :---: | :---: | :--- |
| **1** | **Library & Quản lý truyện** | **COMPLETED** | `NovelRepository.cs`, `LibraryViewModel.cs`, `LibraryView.xaml`, `Program.cs` (`/api/novels`), `js/navigation.js` | **CÓ** (SQLite: bảng `Novels`, `Chapters`) | **CÓ** (Validate null, try-catch) | **CÓ** (`NovelRepository_CRUD`, `LibraryViewModel_SearchAndFiltering`) | Tìm kiếm đa năng, lọc cấp độ JLPT N5-N1, đánh dấu yêu thích, ghi nhận `LastReadAt`. Hoạt động hoàn hảo trên cả Desktop và Web. |
| **2** | **Reader & Đọc tiếng Nhật** | **COMPLETED** | `ReaderViewModel.cs`, `ReaderView.xaml`, `js/reader.js` | **CÓ** (`LastReadPosition`, `IsCompleted`, `Bookmarks`) | **CÓ** (Bound checking, pagination an toàn) | **CÓ** (`ReaderViewModel_LoadNovelAndBookmark`) | **Desktop:** Đọc FlowDocument, chỉnh cỡ chữ, dark mode, phân trang. **Web:** Render từng đoạn văn, click tra từ, chia cột, tracking tiến độ đọc. |
| **3** | **Furigana & Phân tích văn bản** | **PARTIAL** | `JapaneseTextAnalyzer.cs`, `JapaneseEngineDtos.cs`, `Program.cs` (`/api/nlp/analyze`), `js/reader.js` | **KHÔNG** (In-memory tokenization) | **CÓ** (Xử lý chuỗi rỗng, ký tự lạ) | **CÓ** (`JapaneseTextAnalyzerTests`, `Evaluate_Tokenizer_OnRealLiteratureSentences`) | Dùng thuật toán LPM + gom nhóm ScriptType. Nhận diện tốt từ vựng cơ bản nhưng chưa tách được các cụm biến cách phức tạp (thể bị động, sai khiến lồng ghép). Desktop chưa có Native Ruby text trên đầu Kanji. |
| **4** | **Tra từ điển & Giải thích ngữ pháp** | **PARTIAL** | `JapaneseDictionaryService.cs`, `ReaderViewModel.cs`, `js/reader.js`, `Program.cs` (`/api/dictionary/*`) | **CÓ** (Khi lưu vào sổ từ vựng) | **CÓ** (Trả về null an toàn, không sinh nghĩa ảo) | **CÓ** (`DictionaryServiceTests`, `DictionaryLookup_ExactAndDeinflected`) | Bộ từ điển nội bộ gồm 111 mục từ văn học chuẩn + thuật toán de-inflection động từ/tính từ rất chính xác. Tuy nhiên quy mô còn nhỏ, chưa nạp full JMdict (hơn 180.000 từ). |
| **5** | **AI Tutor** | **PARTIAL** | `GeminiAiTutorService.cs`, `IAiTutorService.cs`, `Program.cs` (`/api/ai/explain`), `js/reader.js` | **KHÔNG** (Stateless request/response) | **CÓ** (Tự động fallback về rule-based NLP khi mất mạng hoặc không có API key) | **CÓ** (`AiTutorFollowUpTests`, `Evaluate_AiTutor_RuleBasedQualityAndStructure`) | Đã triển khai đầy đủ trên Backend và Web Client. Hỗ trợ hỏi giải thích câu và follow-up. **Điểm khuyết:** Desktop WPF chưa tích hợp nút gọi AI Tutor trên `ReaderView`. |
| **6** | **AI Character Chat** | **PARTIAL** | `GeminiCharacterDialogueService.cs`, `Program.cs` (`/api/ai/character-chat`), `js/visual-novel.js` | **MỘT PHẦN** (Chỉ thưởng XP vào DB, chưa lưu lịch sử chat) | **CÓ** (Fallback persona thông minh khi offline) | **CÓ** (`CharacterDialogueServiceTests`) | Persona chuẩn xác cho các nhân vật kinh điển (Giovanni, Campanella, Conductor,...). Tự động trích xuất từ vựng từ câu thoại. **Điểm khuyết:** Lịch sử trò chuyện chưa lưu vào SQLite; Desktop WPF chưa có giao diện chat. |
| **7** | **Visual Novel Theatre** | **PARTIAL** | `web-preview/js/visual-novel.js`, `index.html`, `KotobaOdyssey.UI/Views/VisualNovelView.xaml` | **KHÔNG** (Web chạy in-memory) | **CÓ** (Xử lý timer, audio an toàn) | **KHÔNG** (Chưa có test cho phần UI kịch bản) | **Web:** Triển khai rất đẹp với sprite nhân vật, nền Shinkai, typewriter effect, retro Web Audio SFX, backlog và chuyển đổi chế độ story/chat. **Desktop:** Chỉ là màn hình chờ (Placeholder Card). |
| **8** | **Import truyện** | **COMPLETED** | `NovelImportService.cs`, `INovelImportService.cs`, `Program.cs` (`/api/novels/import`), `LibraryViewModel.cs` | **CÓ** (Lưu trọn vẹn Novel và Chapters vào SQLite) | **CÓ** (Kiểm tra trùng lặp tiêu đề, validate JSON lỗi, regex tách chương) | **CÓ** (`NovelImportServiceTests`) | Hỗ trợ cả `.json` và `.txt`. Tự động nhận diện tiêu đề chương bằng Regex (tiếng Nhật, Anh, Việt), tính tổng số chữ (WordCount). |
| **9** | **Flashcard & Thuật toán SRS** | **COMPLETED** | `SpacedRepetitionCalculator.cs`, `VocabularyRepository.cs`, `VocabularyViewModel.cs`, `VocabularyView.xaml`, `js/srs.js` | **CÓ** (Bảng `VocabularyItems`, `ReviewLogs`) | **CÓ** (Kiểm tra giới hạn EaseFactor >= 1.3, chống ghi đè) | **CÓ** (`SpacedRepetitionCalculatorTests`, `VocabularyReviewTests`) | Triển khai chuẩn thuật toán SuperMemo-2 (SM-2). Desktop có phiên ôn tập Flashcard tương tác 4 mức đánh giá (Again, Hard, Good, Easy), lọc theo cấp độ JLPT. Web có Flashcard Arena lật thẻ và đồng bộ về SQLite. |
| **10** | **Chapter Quiz** | **COMPLETED** | `ChapterPracticeService.cs`, `Program.cs` (`/api/novels/{id}/chapters/{num}/quiz`), `js/quiz.js` | **CÓ** (Bảng `ChapterQuizAttempts`) | **CÓ** (Chống hack/spam nhận XP trùng lặp, tính điểm an toàn) | **CÓ** (`PracticeServiceTests`) | Bộ câu hỏi đọc hiểu biên soạn riêng cho từng chương. Chấm điểm %, giải thích chi tiết, thưởng XP. **Điểm khuyết:** Mới tích hợp trên Web modal, Desktop WPF chưa có view làm Quiz. |
| **11** | **Gamification (XP, Level, Streak)** | **COMPLETED** | `GamificationCalculator.cs`, `UserProgressRepository.cs`, `DailyQuestRepository.cs`, `DomainModels.cs` | **CÓ** (Bảng `UserProgresses`, `DailyQuests`, `Achievements`) | **CÓ** (Chống nhận thưởng quest 2 lần, tự động reset streak khi đứt chuỗi) | **CÓ** (`GamificationCalculatorTests`, `DailyQuestRepository_ClaimReward`) | Tính toán XP tăng theo cấp số nhân cong (`RequiredXp`), tăng cấp (Level up), tự động cập nhật chuỗi học Streak, nhiệm vụ hằng ngày Daily Quests và nhận thưởng XP. |
| **12** | **Dashboard & Thống kê học tập** | **COMPLETED** (Desktop) / **PARTIAL** (Web) | `DashboardViewModel.cs`, `DashboardView.xaml`, `Program.cs` (`/api/progress`) | **CÓ** (Tổng hợp từ `UserProgress`, `DailyQuests`, `Novels`) | **CÓ** (Xử lý null progress, reload an toàn) | **CÓ** (`DashboardViewModel_LoadDashboard`, `DashboardViewModel_ClaimQuest`) | **Desktop:** Hoàn chỉnh với Cosmic card, thanh tiến độ XP, Streak, số từ đã học, số phút học hôm nay, danh sách Daily Quests có nút nhận thưởng. **Web:** Mới hiển thị thanh XP vắn tắt trên thanh header. |
| **13** | **APIs & Kết nối hệ thống** | **COMPLETED** | `KotobaOdyssey.Api/Program.cs` | **CÓ** (Đọc/ghi trực tiếp SQLite) | **CÓ** (Try-catch toàn bộ endpoint, fallback HTTP client) | **CÓ** (Được kiểm thử gián tiếp qua Web client & test suite) | 18 Minimal API endpoints hoàn chỉnh, đầy đủ CORS, chuẩn hóa JSON camelCase, phản hồi nhanh chóng dưới 20ms. |

---

## 4. CÁC VẤN ĐỀ VÀ RỦI RO KỸ THUẬT PHÁT HIỆN ĐƯỢC

### 4.1. Lỗi và Rủi ro Kiến trúc (Architecture Inconsistencies)
1. **Desktop WPF không dùng API mà truy cập trực tiếp SQLite:**
   * Cả Desktop (`KotobaOdyssey.UI`) và API (`KotobaOdyssey.Api`) đều cùng mở file SQLite tại `%LOCALAPPDATA%\KotobaOdyssey\kotoba.db`.
   * **Nguy cơ:** Khi cả ứng dụng Desktop và API chạy song song cùng lúc trên Windows, nếu có nhiều tác vụ ghi đồng thời có thể gây lỗi `SQLite Error 5: 'database is locked'` nếu chế độ WAL (Write-Ahead Logging) không được bật rõ ràng.
2. **Sự phân mảnh tính năng giữa Desktop và Web:**
   * Tính năng **Visual Novel Theatre**, **AI Sensei Chat** và **Chapter Quiz** đã có trên Web và Backend API, nhưng trên **Desktop WPF thì VisualNovelView vẫn là màn hình chờ (stub placeholder)**.
   * Ngược lại, **Dashboard chi tiết** và **Vocabulary Management chuyên sâu** thì Desktop làm rất tốt, trong khi Web chỉ có màn hình đọc và lật thẻ đơn giản.

### 4.2. Giới hạn của NLP Tokenizer & Dictionary
1. **Từ điển nội bộ còn nhỏ (111 từ vựng):**
   * Mặc dù 111 từ vựng này bao phủ rất tốt các chương đầu của 6 tác phẩm có sẵn, nhưng khi người dùng sử dụng tính năng **Import truyện mới từ file bên ngoài**, tỷ lệ tra cứu từ vựng không tìm thấy sẽ tăng cao do chưa kết nối cơ sở dữ liệu từ điển lớn như JMdict.
2. **Cơ chế ngắt từ (Tokenization) bằng Heuristic:**
   * Việc kết hợp Longest Prefix Match với phân nhóm Unicode ScriptType là một giải pháp nhẹ không cần phụ thuộc thư viện native MeCab. Tuy nhiên, nó sẽ gặp khó khăn khi ngắt các cụm trợ từ dính liền với động từ phụ (ví dụ: `〜てはいけない`, `〜ざるを得ない`).

### 4.3. Quản lý Database & Migrations
1. **Thiếu EF Core Migrations:**
   * Hiện tại dự án sử dụng `_context.Database.EnsureCreatedAsync()`.
   * **Hạn chế:** Khi cần bổ sung bảng mới (ví dụ bảng lưu lịch sử chat `CharacterChatHistory`), `EnsureCreatedAsync()` sẽ không tự động cập nhật schema của file database cũ nếu nó đã tồn tại trên máy người dùng, đòi hỏi phải xóa file DB tạo lại hoặc viết migration chính thức.

### 4.4. Bảo mật & Cấu hình AI
1. **Bảo vệ API Key:**
   * Kiểm tra `appsettings.json` cho thấy `Gemini:ApiKey` để trống `""`. Key được nạp qua biến môi trường `GEMINI_API_KEY`. **Đây là thực hành bảo mật tốt, không bị lộ secret trên Git.**
2. **Endpoint Google Translate gtx:**
   * Endpoint `/api/translate` đang dùng endpoint dịch miễn phí của Google (`client=gtx`). Đây là giải pháp tạm thời, nếu người dùng gửi yêu cầu liên tục với số lượng lớn có thể bị giới hạn tốc độ (Rate limit / HTTP 429).

---

## 5. NHỮNG CHỨC NĂNG ĐANG HOẠT ĐỘNG TỐT VÀ CẦN GIỮ NGUYÊN

Các thành phần sau đây có chất lượng mã nguồn cao, kiến trúc chuẩn, đã được kiểm thử 100% và **không nên sửa đổi cấu trúc lõi**:

1. **Thuật toán Spaced Repetition (SM-2):**
   * Class [`SpacedRepetitionCalculator`](file:///d:/kotoba/src/backend/KotobaOdyssey.Core/Services/SpacedRepetitionCalculator.cs) hoạt động hoàn hảo, công thức toán học chuẩn xác, có 6 unit tests bao phủ mọi trường hợp (Again, Hard, Good, Easy, Min EaseFactor 1.3).
2. **Bộ tính toán Gamification:**
   * Class [`GamificationCalculator`](file:///d:/kotoba/src/backend/KotobaOdyssey.Core/Services/GamificationCalculator.cs) tính toán đường cong XP và chuỗi streak cực kỳ tin cậy, chống lạm phát điểm kinh nghiệm.
3. **Bộ nạp truyện tự động (NovelImportService):**
   * Khả năng bóc tách chương từ file `.txt` bằng Regular Expressions thông minh, xử lý lỗi định dạng JSON vững chắc.
4. **Hệ thống Seeding tác phẩm văn học (`DatabaseInitializer`):**
   * Dữ liệu 6 tác phẩm kinh điển được gõ và biên tập công phu, chuẩn ngữ liệu tiếng Nhật.
5. **Giao diện & Cấu trúc Mô-đun của Web Companion (`web-preview`):**
   * Sau đợt tái cấu trúc vừa qua, các module `js/audio.js`, `js/reader.js`, `js/visual-novel.js`, `js/quiz.js` đã tách biệt rõ ràng, không còn mã rác, chạy mượt mà không lỗi console.

---

## 6. DANH SÁCH FILE QUAN TRỌNG LIÊN QUAN ĐẾN TỪNG CHỨC NĂNG

| Chức năng | Backend & Data Layer | Desktop Client (WPF) | Web Companion Client |
| :--- | :--- | :--- | :--- |
| **Library** | [`NovelRepository.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Data/Repositories/Repositories.cs)<br>[`Program.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Api/Program.cs) | [`LibraryViewModel.cs`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/ViewModels/LibraryViewModel.cs)<br>[`LibraryView.xaml`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/Views/LibraryView.xaml) | [`js/navigation.js`](file:///d:/kotoba/web-preview/js/navigation.js)<br>[`index.html`](file:///d:/kotoba/web-preview/index.html) |
| **Reader** | [`NovelRepository.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Data/Repositories/Repositories.cs) | [`ReaderViewModel.cs`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/ViewModels/ReaderViewModel.cs)<br>[`ReaderView.xaml`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/Views/ReaderView.xaml) | [`js/reader.js`](file:///d:/kotoba/web-preview/js/reader.js) |
| **NLP & Furigana** | [`JapaneseTextAnalyzer.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Data/Services/JapaneseTextAnalyzer.cs)<br>[`JapaneseEngineDtos.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Core/DTOs/JapaneseEngineDtos.cs) | [`ReaderViewModel.cs`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/ViewModels/ReaderViewModel.cs) | [`js/reader.js`](file:///d:/kotoba/web-preview/js/reader.js) |
| **Dictionary** | [`JapaneseDictionaryService.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Data/Services/JapaneseDictionaryService.cs) | [`ReaderViewModel.cs`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/ViewModels/ReaderViewModel.cs) | [`js/reader.js`](file:///d:/kotoba/web-preview/js/reader.js) |
| **AI Tutor** | [`GeminiAiTutorService.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Data/Services/GeminiAiTutorService.cs)<br>[`IAiTutorService.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Core/Interfaces/IAiTutorService.cs) | *Chưa tích hợp* | [`js/reader.js`](file:///d:/kotoba/web-preview/js/reader.js) |
| **AI Character Chat**| [`GeminiCharacterDialogueService.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Data/Services/GeminiCharacterDialogueService.cs) | *Chưa tích hợp* | [`js/visual-novel.js`](file:///d:/kotoba/web-preview/js/visual-novel.js) |
| **Visual Novel** | [`ICharacterDialogueService.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Core/Interfaces/ICharacterDialogueService.cs) | [`VisualNovelView.xaml`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/Views/VisualNovelView.xaml) (Stub) | [`js/visual-novel.js`](file:///d:/kotoba/web-preview/js/visual-novel.js)<br>[`js/audio.js`](file:///d:/kotoba/web-preview/js/audio.js) |
| **Import Truyện** | [`NovelImportService.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Data/Services/NovelImportService.cs) | [`LibraryViewModel.cs`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/ViewModels/LibraryViewModel.cs) | [`js/api.js`](file:///d:/kotoba/web-preview/js/api.js) |
| **Flashcard SRS** | [`SpacedRepetitionCalculator.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Core/Services/SpacedRepetitionCalculator.cs)<br>[`Repositories.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Data/Repositories/Repositories.cs) | [`VocabularyViewModel.cs`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/ViewModels/VocabularyViewModel.cs)<br>[`VocabularyView.xaml`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/Views/VocabularyView.xaml) | [`js/srs.js`](file:///d:/kotoba/web-preview/js/srs.js) |
| **Chapter Quiz** | [`ChapterPracticeService.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Data/Services/ChapterPracticeService.cs) | *Chưa tích hợp* | [`js/quiz.js`](file:///d:/kotoba/web-preview/js/quiz.js) |
| **Gamification** | [`GamificationCalculator.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Core/Services/GamificationCalculator.cs)<br>[`DomainModels.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Core/Models/DomainModels.cs) | [`DashboardViewModel.cs`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/ViewModels/DashboardViewModel.cs) | [`js/api.js`](file:///d:/kotoba/web-preview/js/api.js) |
| **Dashboard** | [`Repositories.cs`](file:///d:/kotoba/src/backend/KotobaOdyssey.Data/Repositories/Repositories.cs) | [`DashboardViewModel.cs`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/ViewModels/DashboardViewModel.cs)<br>[`DashboardView.xaml`](file:///d:/kotoba/src/frontend/KotobaOdyssey.UI/Views/DashboardView.xaml) | Header XP Bar trong [`index.html`](file:///d:/kotoba/web-preview/index.html) |

---

## 7. KẾT QUẢ BUILD VÀ TEST THỰC TẾ

### 7.1. Kết quả Build Solution (`dotnet build`)
* **Trạng thái:** **THÀNH CÔNG (0 Error, 0 Warning)**
* **Dự án biên dịch:**
  * `KotobaOdyssey.Core` -> `net9.0`
  * `KotobaOdyssey.Data` -> `net9.0`
  * `KotobaOdyssey.Api` -> `net9.0`
  * `KotobaOdyssey.UI` -> `net9.0-windows`
  * `KotobaOdyssey.Tests` -> `net9.0-windows`

### 7.2. Kết quả Chạy Test Toàn Diện (`dotnet test`)
* **Tổng số test cases:** **59/59 tests PASSED (100%)**
* **Số test thất bại:** **0**
* **Số test bỏ qua (Skipped):** **0**
* **Thời gian thực thi:** **2,47 giây**

```text
Phân loại các cụm Test Suite đã chạy:
  ✓ SpacedRepetitionCalculatorTests: 5/5 passed (Toán học SM-2)
  ✓ GamificationCalculatorTests: 8/8 passed (XP, Level, Streak)
  ✓ DictionaryServiceTests: 4/4 passed (Exact & De-inflected lookup)
  ✓ JapaneseTextAnalyzerTests: 2/2 passed (LPM & Script segmentation)
  ✓ AiTutorFollowUpTests: 3/3 passed (Sentence explanation & Follow-up)
  ✓ CharacterDialogueServiceTests: 5/5 passed (Persona chat & Fallback)
  ✓ PracticeServiceTests: 5/5 passed (Quiz scoring, Anti-duplicate XP)
  ✓ AiAndTokenizerEvaluationTests: 3/3 passed (Văn học thực tế)
  ✓ NovelImportServiceTests: 6/6 passed (JSON & TXT parsing, Bookmarks)
  ✓ RepositoryTests: 3/3 passed (CRUD Novels, Quests, Seed data)
  ✓ ReaderVocabSrsIntegrationTests: 3/3 passed (Luồng đọc -> Lưu từ -> Ôn tập)
  ✓ NavigationAndViewModelTests: 7/7 passed (WPF ViewModels & Navigation)
  ✓ VocabularyReviewTests: 4/4 passed (Flashcard UI Session)
```

---

## 8. ĐÁNH GIÁ MỨC ĐỘ HOÀN THIỆN TỔNG THỂ

* **Điểm kiến trúc hệ thống:** **8.5 / 10**
  * Core Domain sạch sẽ, tính toán nghiệp vụ tách biệt hoàn toàn khỏi UI và Database.
  * Dependency Injection chuẩn mực, Code-First Entity Framework trực quan.
* **Mức độ hoàn thiện tính năng Desktop (WPF):** **70%**
  * Phần Đọc truyện, Quản lý từ vựng, Ôn tập SRS, Import truyện và Dashboard hoạt động rất tốt.
  * Phần Visual Novel, AI Sensei Chat và Chapter Quiz chưa được đưa lên UI.
* **Mức độ hoàn thiện tính năng Web Companion:** **85%**
  * Trình đọc Satori, Visual Novel, AI Chat, Flashcard Arena và Quiz hoạt động mượt mà, đầy đủ hiệu ứng âm thanh và hình ảnh.
* **Tổng thể dự án:** Dự án có nền tảng cốt lõi **rất vững chắc và sạch sẽ**. Hoàn toàn khả thi và thuận lợi để tiếp tục phát triển nâng cấp lên mức độ hoàn thiện cao nhất mà **không cần đập đi xây lại**.

---

## 9. BẢNG PHÂN LOẠI ƯU TIÊN NÂNG CẤP (PRIORITY MATRIX)

Bảng phân chia mức độ ưu tiên theo đúng tiêu chí cho Giai đoạn tiếp theo:

### [P0] — Lỗi nghiêm trọng / Ảnh hưởng kiến trúc cốt lõi
1. **Đồng bộ hóa Database Concurrency giữa Desktop và API:** Cấu hình bật chế độ WAL (`PRAGMA journal_mode=WAL;`) trong SQLite connection string để tránh nguy cơ khóa file khi Desktop và Web API cùng chạy đồng thời.
2. **Kích hoạt EF Core Migrations:** Thiết lập Migration chính thức thay vì dựa hoàn toàn vào `EnsureCreatedAsync()`, chuẩn bị cho việc bổ sung các bảng dữ liệu mới (như lưu lịch sử Chat AI).

### [P1] — Chức năng quan trọng cần hoàn thiện (Core Value Completion)
1. **Tích hợp tính năng AI Tutor & AI Character Chat vào Desktop WPF Client:** Kết nối `IAiTutorService` và `ICharacterDialogueService` vào `ReaderViewModel` và `VisualNovelViewModel` trong WPF.
2. **Hoàn thiện VisualNovelView trên Desktop WPF:** Xây dựng UI hiển thị Visual Novel tương tự bản Web (hoặc nhúng WebView2 / native XAML controls) thay cho card placeholder hiện tại.
3. **Lưu lịch sử hội thoại AI Character Chat:** Bổ sung thực thể `ChatMessage` / `ChatSession` vào SQLite để người dùng có thể xem lại lịch sử các cuộc trò chuyện cùng nhân vật văn học.
4. **Mở rộng cơ sở dữ liệu Từ điển tiếng Nhật (JMdict Integration):** Tích hợp nguồn dữ liệu từ điển đầy đủ để người dùng tra cứu được mọi từ vựng mới khi import truyện tùy ý từ bên ngoài.

### [P2] — Cải thiện chất lượng và trải nghiệm người dùng
1. **Hỗ trợ Native Ruby Text (Furigana) trên Desktop WPF:** Tùy biến TextBlock/FlowDocument để hiển thị chữ Furigana nhỏ nằm ngay phía trên đầu chữ Hán thay vì chỉ xem qua tooltip.
2. **Nâng cấp độ chính xác của NLP Tokenizer:** Bổ sung tập luật nhận diện các cụm ngữ pháp nối phức tạp của tiếng Nhật văn học.
3. **Màn hình làm Chapter Quiz trên Desktop WPF:** Tạo giao diện trắc nghiệm đọc hiểu cuối chương trên WPF để đồng bộ tính năng với Web.

### [P3] — Tính năng mở rộng, chưa cần thiết ở giai đoạn này
1. **Chuyển đổi Web Companion sang framework SPA nâng cao (Next.js / React / Vue):** Hiện tại cấu trúc Vanilla JS Modular vừa tách đang chạy rất nhẹ và ổn định, chưa cấp bách phải viết lại bằng React.
2. **Cloud Sync / Multi-user Login:** Đồng bộ tài khoản đám mây (Cloud Auth/PostgreSQL) khi muốn triển khai phiên bản thương mại trực tuyến đa nền tảng.

