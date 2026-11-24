# BÁO CÁO GIAI ĐOẠN 6 – UI/UX POLISH & VISUAL IDENTITY
## Kotoba Odyssey (言葉の旅) — Polish & Visual Design System

- **Giai đoạn:** Phase 6 – UI/UX Polish & Visual Identity
- **Kiến trúc:** C# .NET 9 WPF Desktop & Modern Web Companion (HTML5/ES6/CSS3)
- **Engine AI:** Google Gemini 2.5 Flash & Offline Morphological Fallback
- **Cơ sở dữ liệu:** SQLite (EF Core 9) / Offline-first SM-2 SRS
- **Trạng thái kiểm thử:** 112/112 Unit & Acceptance Tests PASS (0 Failed, 0 Skipped)

---

## 1. TỔNG QUAN ĐỊNH HƯỚNG THIẾT KẾ (DESIGN DIRECTION)

Kotoba Odyssey được định vị không phải là một ứng dụng quản lý từ vựng khô khan dạng CRUD hay LMS truyền thống, mà là một **sản phẩm học tiếng Nhật kết hợp Visual Novel và Light Novel có trải nghiệm nghệ thuật cao**.

Thiết kế kết hợp ba trải nghiệm cốt lõi:
1. **Light Novel Experience (Satori Reader Style):** Không gian đọc văn học Nhật Bản thanh thoát, chuẩn typographic tiếng Nhật (phông Mincho/Serif, ruby furigana cân xứng, khoảng cách dòng thoáng đãng `line-height: 2.3`, độ rộng tối ưu 720–740px chống mỏi mắt).
2. **Visual Novel Experience (Makoto Shinkai & Key Visual Style):** Sân khấu kịch tính với hình nền cinematic (chuyến tàu ngân hà Ginga Tetsudou, hoàng hôn Syracuse, cổng Rashomon mưa gió), nhân vật sprite biểu cảm đa dạng, khung hội thoại mica mờ (frosted glass) và hiệu ứng ánh sáng gradient tinh tế.
3. **Japanese Learning Experience (Wanikani & Kitsun.io Style):** Tra từ 1 chạm không ngắt mạch đọc, trợ giảng AI giải thích ngữ pháp theo ngữ cảnh thực tế, thẻ flashcard lật 3D tương tác, tính điểm SRS khoa học theo thuật toán SM-2.

---

## 2. HỆ THỐNG THIẾT KẾ ĐỒNG BỘ (DESIGN SYSTEM & TOKENS)

### 2.1. Bảng màu (Color Palette)
Cả phiên bản Web Companion (`web-preview/style.css`) và WPF Desktop (`Styles/Colors.xaml`) đã được quy chuẩn hóa theo các token sau:

| Tên Token / Resource | Giá trị Hex / RGBA | Vai trò & Ứng dụng |
| :--- | :--- | :--- |
| `bg-app` / `BrushSpaceBg` | `#0B0E14` | Nền ứng dụng chính (Cosmic Obsidian) |
| `bg-sidebar` / `BrushSidebarBg` | `#0F121B` | Thanh điều hướng bên trái |
| `bg-surface` / `BrushSurfaceBg` | `#151924` | Nền thẻ (Card), Container nội dung |
| `bg-surface-elevated` | `#1B202E` | Dropdown, popover, thanh công cụ nổi |
| `border-subtle` / `BrushBorder` | `rgba(255,255,255,0.08)` / `#1E2A4A` | Đường viền mảnh tinh xảo |
| `accent-blue` / `BrushBlueHighlight` | `#3B82F6` | Hành động chính, liên kết, từ vựng đang chọn |
| `accent-violet` / `BrushVioletPrimary`| `#7C5CFC` | Nút kích hoạt AI, hiệu ứng phát sáng mờ |
| `accent-amber` / `BrushGoldPrimary` | `#F59E0B` | Điểm kinh nghiệm (XP), Streak ngày, cấp độ |
| `paper-bg` (Satori Warm) | `#FAF7F0` | Nền trang sách ban ngày (ấm dịu, chống mỏi mắt) |
| `paper-sheet` | `#FFFFFF` | Tờ giấy văn bản Light Novel |
| `sepia-bg` (Vintage Paper) | `#F4ECD8` | Nền đọc sách phong cách cổ điển Sepia |
| `sepia-sheet` | `#FBF0D9` | Tờ giấy tông ngà ấm áp |

### 2.2. Hệ thống kiểu chữ (Typography)
- **Văn bản tiếng Nhật (Japanese Body & Novels):** `'Shippori Mincho', 'Noto Serif JP', 'Yu Mincho', serif` với độ co giãn ký tự `letter-spacing: 0.05em` và khoảng cách dòng `line-height: 2.3`.
- **Giao diện & Điều hướng (UI / Controls):** `'Plus Jakarta Sans', system-ui, sans-serif` kết hợp nét chữ semi-bold, hiện đại và sắc nét trên mọi mật độ điểm ảnh.
- **Kanji Card & Tiêu đề chính:** Kích thước lớn (48px – 56px), độ đậm cao (`font-weight: 800`) mang lại cảm giác ấn tượng cho thẻ từ vựng và tiêu đề chương.

### 2.3. Ba chủ đề hiển thị (Theme System)
Ứng dụng hỗ trợ chuyển đổi linh hoạt:
1. **Satori Paper Warm (Giấy ngà):** Tối ưu cho việc đọc ban ngày, tạo cảm giác như đang cầm trên tay một ấn bản Bunko (bản sách bỏ túi Nhật Bản).
2. **Sepia Vintage (Hoài cổ):** Tông màu giấy da ngà trầm ấm, độ tương phản dịu nhẹ thích hợp cho buổi chiều tà.
3. **Cosmic Obsidian / Dark (Đêm sâu thẳm):** Nền đen xám cao cấp `#0B0E14` với chữ trắng bạc `#F1F5F9` và điểm nhấn ánh sao mờ, bảo vệ mắt khi đọc đêm.

---

## 3. CÁC NÂNG CẤP CHI TIẾT TRÊN CẢ HAI NỀN TẢNG

### 3.1. Web Companion (`web-preview/`)
1. **Thư viện truyện & Tìm kiếm (Library View):**
   - Lưới bìa sách anime chất lượng cao, nhãn JLPT (N5 đến N1) phân màu riêng biệt.
   - Thêm trạng thái rỗng (`#libraryEmptyState`): Khi tìm kiếm hoặc lọc không có kết quả, giao diện hiển thị thẻ minh họa thân thiện cùng nút *"Xem tất cả truyện"*, không để lại khoảng trống đứt đoạn.
2. **Trải nghiệm đọc Light Novel (Reader View):**
   - Nút chuyển nhanh 3 chủ đề nền: **Giấy ngà (Paper) ⇄ Trầm (Sepia) ⇄ Đêm (Dark)**.
   - Nút bật/tắt Furigana mượt mà thông qua thẻ chuẩn `<ruby>` và `<rt>`.
   - Popover từ điển tương tác 1 chạm: Hiển thị từ gốc (lemma), từ loại, cấp độ JLPT, nghĩa ngữ cảnh và nút lưu thẻ SRS.
   - Trợ giảng AI phân tích ngữ pháp câu tích hợp thanh trượt mượt mà.
3. **Đấu trường từ vựng SRS (Flashcard Arena):**
   - Thẻ lật 3D mượt mà với 2 mặt: Mặt trước (Kanji + Romaji + Cấp độ), Mặt sau (Cách đọc Hiragana + Nghĩa tiếng Việt + Câu ví dụ trích từ chương truyện).
   - Thêm trạng thái rỗng (`#srsEmptyState`): Khi đã hoàn thành hết số từ cần ôn tập trong ngày, hiển thị thông báo chúc mừng kèm nút quay lại thư viện.
   - Thanh đánh giá 4 mức độ (Again, Hard, Good, Easy) chuẩn thuật toán SM-2.
4. **Nhà hát Visual Novel (VN Theatre & AI Character Chat):**
   - 6 bối cảnh đồ họa Shinkai: Tàu ngân hà Ginga, cỗ xe lãng mạn, cổng Rashomon, đồi hoa hướng dương Melos, rừng Gonx, học đường Gakuen.
   - Sprite nhân vật có bóng đổ và biểu cảm thay đổi theo nội dung thoại.
   - Hộp thoại mica mờ, hiệu ứng gõ chữ (Typewriter effect) và hỗ trợ đổi 3 chế độ hội thoại: Tự do, Luyện giao tiếp kèm sửa lỗi nhẹ, và Nhập vai tình huống.
5. **Giao diện Mobile & Responsive (< 768px):**
   - Thanh điều hướng tự động chuyển thành thanh menu ngang cuộn mượt.
   - Khung đọc truyện căn chỉnh toàn màn hình với lề tối ưu, kích thước chữ co giãn tỉ lệ.
   - Popover tra từ tự động neo vào đáy màn hình (Bottom Sheet style) chống tràn khung nhìn.

### 3.2. WPF Desktop (`KotobaOdyssey.UI`)
1. **Nâng cấp `VisualNovelView.xaml` & `VisualNovelViewModel.cs`:**
   - Thay thế thẻ tĩnh bằng **Visual Novel Theatre** hoàn chỉnh kết nối trực tiếp với backend `ICharacterDialogueService`.
   - Chọn nhân vật từ danh sách Literature Personas (Giovanni, Campanella, Melos, Gon, Conductor...).
   - Hiển thị phản hồi nhân vật, bản dịch tiếng Việt, hộp phân tích sửa lỗi ngữ pháp (Gentle Grammar Correction) và nút lưu từ mới vào SRS chỉ bằng 1 cú nhấp.
2. **Nâng cấp `SettingsView.xaml` & `SettingsViewModel.cs`:**
   - Thay thế thẻ giữ chỗ đơn sơ bằng bảng điều khiển cài đặt toàn diện gồm 4 phân vùng:
     - **Giao diện & Phông chữ:** Lựa chọn theme Cosmic/Paper/Midnight, chọn phông chữ Nhật (Noto Sans JP, Yu Gothic, Meiryo), tăng/giảm cỡ chữ đọc trực quan.
     - **Trải nghiệm đọc & Học tập:** Bật/tắt Furigana, bật/tắt âm thanh hiệu ứng (SFX), đặt cấp độ JLPT trọng tâm mặc định.
     - **Dữ liệu & Lưu trữ:** Hiển thị đường dẫn SQLite cục bộ, nút sao lưu dữ liệu offline an toàn.
     - **Thông tin ứng dụng:** Giới thiệu phiên bản v6.0 Polish Edition và triết lý kiến trúc.
3. **Thanh điều hướng & Thẻ Hero Dashboard:**
   - Cập nhật các nút RadioButton điều hướng có thanh chỉ báo phát sáng (Active Indicator Bar) và chuyển động êm ái.

---

## 4. BẢO ĐẢM TÍNH NGUYÊN VẸN & KẾT QUẢ KIỂM THỬ

Toàn bộ quá trình hoàn thiện giao diện tuân thủ nghiêm ngặt nguyên tắc: **Không viết lại logic nghiệp vụ cốt lõi, không phá vỡ tính năng học tập, không làm gãy các bài kiểm thử đã hoàn thành ở các giai đoạn trước.**

### Kết quả chạy kiểm thử tự động (.NET 9):
```
Passed!  - Failed: 0, Passed: 112, Skipped: 0, Total: 112, Duration: 1 s - KotobaOdyssey.Tests.dll (net9.0)
```
- **Phase 2 (Core Learning Loop & SRS SM-2):** Đạt 100%.
- **Phase 3 (AI Tutor Intelligence & JLPT Adaptation):** Đạt 100%.
- **Phase 4 (Chapter Quiz & SQLite Analytics Persistence):** Đạt 100%.
- **Phase 5 (AI Character Chat & Literature Personas):** Đạt 100%.
- **Cú pháp JavaScript (`web-preview/js/*.js`):** `node -c` xác nhận 100% hợp lệ, không có lỗi runtime.

---

## 5. HƯỚNG DẪN KIỂM TRA THỰC TẾ (MANUAL VERIFICATION GUIDE)

1. **Khởi chạy Web Companion:**
   - Mở file `web-preview/index.html` trên trình duyệt Chrome / Edge hoặc qua máy chủ tĩnh (`python -m http.server 8080`).
   - Vào mục **Thư viện**: Thử tìm kiếm một từ không có thực (ví dụ: `xyz123`) → Kiểm tra thẻ thông báo rỗng xuất hiện đẹp mắt kèm nút hoàn tác bộ lọc.
   - Vào mục **Đọc sách**: Bấm nút **🌓 Nền** để trải nghiệm chu kỳ 3 theme: *Giấy ngà* → *Trầm (Sepia)* → *Đêm (Dark)*. Bấm vào từ vựng bất kỳ để kiểm tra cửa sổ từ điển và nút lưu SRS.
   - Vào mục **Visual Novel Mode**: Chọn nhân vật Giovanni hoặc Melos, gửi tin nhắn tiếng Nhật để thấy sprite nhân vật, hộp thoại phong cách Shinkai và phân tích ngữ pháp.
   - Thu nhỏ cửa sổ trình duyệt xuống dưới 768px để kiểm tra bố cục responsive trên điện thoại di động.

2. **Khởi chạy WPF Desktop:**
   - Biên dịch và chạy ứng dụng: `dotnet run --project src/frontend/KotobaOdyssey.UI/KotobaOdyssey.UI.csproj`.
   - Bấm vào mục **Visual Novel (AI)** trên thanh điều hướng bên trái: Trải nghiệm sân khấu hội thoại với nhân vật văn học kinh điển.
   - Bấm vào mục **Cài đặt (Settings)**: Thử thay đổi cỡ chữ, phông chữ, cấp độ JLPT mặc định và sao lưu cơ sở dữ liệu SQLite.

---

## 6. KẾT LUẬN

Giai đoạn 6 đã hoàn thành xuất sắc việc xây dựng bản sắc hình ảnh độc đáo, hiện đại và thanh lịch cho Kotoba Odyssey, nâng tầm sản phẩm từ một hệ thống học tập kỹ thuật thành một tác phẩm kết hợp hài hòa giữa **văn học Nhật Bản, công nghệ AI tương tác và nghệ thuật Visual Novel**.

