# BÁO CÁO NÂNG CẤP TRANG CHỦ (HOME PAGE ENHANCEMENT REPORT)
## Dự án: KOTOBA ODYSSEY
**Ngày thực hiện:** 03/11/2026  
**Định hướng cốt lõi:** *"Kotoba Odyssey là nền tảng đọc Light Novel Nhật Bản kết hợp học tiếng Nhật. Người dùng đến để đọc truyện và học tiếng Nhật tự nhiên thông qua nội dung truyện."*

---

## 1. TỔNG QUAN CẢI TIẾN ĐÃ THỰC HIỆN

Tất cả các thay đổi được thực hiện trên nguyên tắc **Bảo toàn giao diện & Tính ổn định hệ thống**:
- **Không thay đổi Backend, API, và Database SQLite.**
- **Bảo lưu 100% Dark Theme, màu sắc nhận diện thương hiệu, các font Mincho/Outfit và bố cục nền tảng.**
- **Reader, Explore Catalog, Book Detail và Hệ thống SRS/Từ vựng vẫn hoạt động trơn tru.**

---

## 2. CHI TIẾT CÁC HẠNG MỤC NÂNG CẤP

### 2.1. Sửa lỗi Header & Navigation (Xóa bỏ hoàn toàn thanh cuộn ngang)
* **Vấn đề trước đây:** Trên các màn hình desktop độ phân giải 1366px, 1536px (hoặc tỉ lệ scale Windows 125% - 150%), thanh navbar bị tràn pixel kích hoạt thanh scrollbar màu xám `◀ █ ▶` của Windows Chrome, làm mất thẩm mỹ.
* **Giải pháp đã thực hiện:**
  - Trong `web-preview/style.css`:
    - Thêm `scrollbar-width: none; -ms-overflow-style: none;` và `.ln-nav-links::-webkit-scrollbar { display: none; }` để loại bỏ vĩnh viễn thanh scrollbar native của OS.
    - Tối ưu khoảng cách và kích thước nút: `gap: 3px; padding: 6px 11px; font-size: 13px;`.
    - Thanh tìm kiếm toàn cục: `width: 180px;` (nở ra `230px` khi focus), co giãn linh hoạt.
  - Trong `web-preview/index.html`:
    - Tinh gọn navigation links: `Trang chủ`, `Khám phá truyện`, `Xếp hạng`, `Học tập & SRS`, `Visual Novel AI`.
    - Thêm class `.nav-label` cho responsive.
  - Breakpoints Responsive:
    - `@media (max-width: 1280px)`: Tự động thu gọn search bar và user streak chip.
    - `@media (max-width: 1080px)`: Tự động ẩn text label, chỉ giữ icon và badge, tránh vỡ layout.

---

### 2.2. Nâng cấp cấu trúc Home theo thứ tự chuẩn A ➔ B ➔ C ➔ D ➔ E ➔ F

#### A. Featured Novel (Hero Showcase)
* **Bố cục nâng cấp:**
  - Bìa truyện tỷ lệ chuẩn 2:3 với viền phát sáng và hiệu ứng nổi bật (depth shadow).
  - Tên tiếng Nhật Kanji (`銀河鉄道の夜`), Romaji & Tác giả (`Night on the Galactic Railroad • 宮沢賢治`).
  - Tóm tắt nội dung ngắn gọn mang phong cách văn học Light Novel.
  - Badges phân loại: Cấp độ JLPT (`JLPT N3`), Thể loại (`Cosmic Fantasy`), Trạng thái (`Hoàn thành`).
  - 3 nút kêu gọi hành động: `📖 Đọc ngay Chương 1 ➔`, `ℹ️ Chi tiết tác phẩm`, `🎭 Visual Novel AI`.
* **Bộ chuyển đổi tuyển tập nổi bật (Multi-Novel Switcher):**
  - Tích hợp switcher strip với 4 tác phẩm kinh điển:
    1. *銀河鉄道の夜* (Cosmic Fantasy • JLPT N3)
    2. *坊っちゃん* (Slice of Life / School • JLPT N4)
    3. *こころ* (Psychological Romance • JLPT N2)
    4. *走れメロス* (Action & Adventure • JLPT N3)
  - Khi người dùng click, Hero banner chuyển đổi mượt mà cả hình nền blur anime, bìa truyện, thông tin và liên kết đọc.

#### B. Continue Reading Shelf (Tiếp tục đọc dở dang)
* Bố trí các thẻ ngang (horizontal cards) tận dụng chiều rộng màn hình.
* Hiển thị bìa sách, tên tác phẩm, chương đang đọc dở, thanh tiến độ % (gradient Cyan - Blue), và nút `Đọc tiếp ➔`.
* Tự động lưu và cập nhật theo tiến độ đọc thực tế.

#### C. Recently Updated (Khu vực truyện mới cập nhật)
* **Thành phần mới bổ sung:**
  - Thẻ chương mới cập nhật cho 4 tác phẩm tiêu biểu.
  - Huy hiệu chương mới nhất (`Chương 2 (Mới)`, `Chương 1`).
  - Thời gian cập nhật (`Hôm nay`, `Hôm qua`, `2 ngày trước`).
  - Badge độ khó JLPT và nhãn thể loại hiện đại (`Isekai & Fantasy`, `Slice of Life`, `Psychological`).
  - Click vào thẻ dẫn ngay đến chương mới nhất.

#### D. Recommended Novels (Gợi ý tác phẩm theo JLPT & Thể loại)
* **Bộ lọc linh hoạt:** Phân loại theo 5 cấp độ JLPT từ N5 (Sơ khởi) đến N1 (Thành thạo).
* **Bookshelf Grid chuẩn tỉ lệ 2:3:**
  - Tỉ lệ bìa sách 2:3 chuẩn Dengeki Bunko / BookWalker.
  - Huy hiệu JLPT góc trên hiệu ứng kính mờ (frosted glass).
  - Hiệu ứng hover nâng card 6px với bóng đổ mềm và zoom ảnh nhẹ (1.05x).
  - Đảm bảo hiển thị đầy đủ ngay cả khi chạy offline thông qua `renderHomeBookshelfFallback()`.

#### E. Learning Through Reading (Đồng hành học tập cùng từng trang truyện)
* **Định vị đúng vai trò:** Khu vực hỗ trợ học tập đặt sau phần khám phá truyện, không lấn át trải nghiệm đọc Light Novel.
* **Bento Card đồng hành thông minh:**
  - **Thống kê tiến độ thực tế:** Số từ vựng đã lưu trong sổ (`32 từ`), số thẻ Flashcard đến hạn ôn (`8 thẻ`), chuỗi ngày đọc liên tục (`7 ngày`).
  - **Dải từ vựng vừa gặp trong truyện:** 4 thẻ từ vựng thực tế trích xuất từ các chương truyện đang đọc (`銀河`, `切符`, `無鉄砲`, `栗`).
  - **Tích hợp âm thanh giọng Nhật:** Nút loa `🔊` phát âm chuẩn giọng Nhật qua Web Speech Synthesis API.
  - **2 Nút CTA tiện ích:** `🧠 Ôn tập Flashcards (8 thẻ) ➔` và `📔 Sổ từ vựng cá nhân`.

#### F. Popular Novels (Bảng xếp hạng / Top Trending)
* **Tuyệt đối trung thực với dữ liệu CSDL:**
  - Đã loại bỏ hoàn toàn các con số giả như lượt đọc ảo (`12,450 lượt đọc`) và sao đánh giá ảo (`⭐ 4.9`).
  - Thay vào đó hiển thị thông tin thực tế từ cơ sở dữ liệu: Thứ hạng (#1 đến #5), Tên truyện Kanji + Romaji, Tác giả, Cấp độ JLPT, Số chương thực tế trong CSDL, và Thể loại.

---

## 3. DANH SÁCH FILE ĐÃ CHỈNH SỬA

1. **`web-preview/style.css`**:
   - Khắc phục triệt để thanh cuộn ngang của `.ln-nav-links` trên desktop.
   - Thêm responsive breakpoint rules tại 1280px và 1080px.
   - Bổ sung styles cho `.hero-switcher-strip`, `.recent-updates-grid`, `.recent-update-card`, `.learning-companion-card`, `.lc-stats-row`.
   - Chuẩn hóa tỷ lệ bìa sách 2:3 cho `.shelf-cover-wrap` và hiệu ứng hover.
2. **`web-preview/index.html`**:
   - Tối ưu thanh Navigation header.
   - Tổ chức lại toàn bộ cấu trúc Pane Home theo thứ tự chuẩn A ➔ B ➔ C ➔ D ➔ E ➔ F.
   - Loại bỏ số liệu ảo trong Bảng xếp hạng.
3. **`web-preview/js/navigation.js`**:
   - Bổ sung `featuredHeroData` và hàm `setFeaturedHero(key, btn)` cho Hero Switcher.
   - Bổ sung `renderHomeBookshelfFallback()` giúp hiển thị ngay dữ liệu sách khi trang load.
   - Bổ sung `startSrsReviewModal()`.
4. **`web-preview/js/api.js`**:
   - Đồng bộ số lượng thẻ SRS đến hạn với badge navbar (`#navSrsCount`) và trang chủ (`#homeDueSrsCount`).
   - Cập nhật số từ vựng trong CSDL với chỉ số trang chủ (`#homeSavedVocabCount`).
5. **`web-preview/app.js`**:
   - Gọi `renderHomeBookshelfFallback()` ngay trong hàm khởi tạo `initKotobaApp()`.

---

## 4. KẾT QUẢ KIỂM THỬ & XÁC MINH

- **Cú pháp JavaScript:** Kiểm tra bằng `node -c` trên toàn bộ các file JS (`api.js`, `navigation.js`, `reader.js`, `srs.js`, `app.js`) ➔ **100% hợp lệ, không có lỗi.**
- **Tính toàn vẹn CSS:** Kiểm tra cân bằng dấu ngoặc nhọn `{}` trong `style.css` ➔ **Open braces count: 0 (hoàn hảo).**
- **Hiển thị Desktop:** Navigation không còn thanh cuộn ngang, các section hiển thị theo đúng thứ tự thiết kế, Hero banner chuyển đổi mượt mà.

