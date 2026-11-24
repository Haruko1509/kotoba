# KỊCH BẢN THUYẾT TRÌNH & DEMO TỐT NGHIỆP
## Dự án: KOTOBA ODYSSEY (言葉の旅)
### Thời lượng dự kiến: 15 – 20 Phút | Đối tượng: Hội đồng chấm Khóa luận / Đồ án tốt nghiệp

---

## TỔNG QUAN PHÂN BỔ THỜI LƯỢNG

| Phần | Nội dung trình bày | Nền tảng minh họa | Thời gian |
| :---: | :--- | :--- | :---: |
| **Phần 1** | Giới thiệu bài toán & Mục tiêu sản phẩm | Slide / Khái niệm | 2 phút |
| **Phần 2** | Kiến trúc tổng thể & Sơ đồ công nghệ | Slide / Sơ đồ kiến trúc | 3 phút |
| **Phần 3** | Demo Luồng học tập cốt lõi (Core Learning Loop) | Web Companion / Satori Reader | 4 phút |
| **Phần 4** | Demo AI Tutor & Phân tích ngữ cảnh | Web Companion / Drawer | 3 phút |
| **Phần 5** | Demo Visual Novel Theatre & AI Character Chat | WPF Desktop & Web | 4 phút |
| **Phần 6** | Demo Ôn tập Flashcard SRS SM-2 & Chapter Quiz | Web Companion | 2 phút |
| **Phần 7** | Minh chứng kiểm thử tự động & Kết luận | Terminal (`dotnet test`) & Q&A | 2 phút |

---

## KỊCH BẢN THAO TÁC CHI TIẾT TỪNG BƯỚC

### PHẦN 1: ĐẶT VẤN ĐỀ & TRIẾT LÝ SẢN PHẨM (00:00 - 02:00)
- **Lời dẫn thuyết trình:**
  > *"Kính thưa quý Thầy Cô trong Hội đồng. Học tiếng Nhật qua sách giáo khoa truyền thống thường gặp hai rào cản lớn: từ vựng thiếu ngữ cảnh thực tế và dễ gây cảm giác nhàm chán. Kotoba Odyssey ra đời với sứ mệnh biến hành trình học tiếng Nhật thành một chuyến phiêu lưu văn học. Người học không học từ đơn lẻ, mà được đắm chìm trong các tác phẩm Light Novel kinh điển của Nhật Bản, kết hợp sức mạnh phân tích ngữ cảnh của AI và hình thức nhập vai tương tác Visual Novel."*

---

### PHẦN 2: KIẾN TRÚC HỆ THỐNG & ĐỒNG BỘ DỮ LIỆU (02:00 - 05:00)
- **Lời dẫn thuyết trình:**
  > *"Hệ thống được thiết kế theo kiến trúc Hybrid hiện đại, phân lớp chặt chẽ:
  > - **Backend:** ASP.NET Core 9 Minimal API hiệu năng cao kết hợp Entity Framework Core 9.
  > - **Cơ sở dữ liệu:** SQLite cục bộ chế độ Write-Ahead Logging (WAL) đảm bảo tính toàn vẹn dữ liệu ngoại tuyến (Offline-first).
  > - **Hai Client độc lập:** Desktop WPF phong cách game sang trọng và Web Companion phong cách Satori Reader tối giản.
  > - **Trí tuệ nhân tạo:** Tích hợp mô hình Google Gemini 2.5 Flash đa ngữ cảnh, đồng thời trang bị sẵn động cơ phân tích hình thái học tiếng Nhật (Rule-based NLP) tự động chạy mượt mà ngay cả khi không có mạng."*

---

### PHẦN 3: DEMO LUỒNG ĐỌC SÁCH & TRA TỪ NGỮ CẢNH (05:00 - 09:00)
- **Màn hình mở:** Web Companion (`http://localhost:8080`).
- **Thao tác 1 (Thư viện sách):**
  - Mở tab **Thư viện & Đọc sách**.
  - Giới thiệu các bộ lọc cấp độ JLPT (N5 đến N1) và các tác phẩm kinh điển (*Ginga Tetsudou no Yoru, Botchan, Rashomon*).
  - Thử gõ tìm kiếm một từ bất kỳ → Chỉ ra khả năng lọc tức thì và hiển thị thẻ rỗng (Empty State) thân thiện nếu không có kết quả.
- **Thao tác 2 (Trải nghiệm đọc Light Novel phong cách Satori Reader):**
  - Chọn tác phẩm *Ginga Tetsudou no Yoru* (Chương 1).
  - Bấm nút **🌓 Nền** để trình diễn 3 chủ đề đọc: **Giấy ngà (Paper Warm) ⇄ Trầm (Sepia) ⇄ Đêm (Dark)**.
  - Bấm nút bật/tắt **Furigana** trên thanh công cụ để chứng minh sự hỗ trợ linh hoạt cho người mới học.
- **Thao tác 3 (Tra cứu 1 chạm & Lưu từ vào SRS):**
  - Nhấp chuột vào từ `銀河` hoặc `切符`.
  - Popover từ điển lập tức xuất hiện: Hiển thị cách đọc Hiragana (`ぎんが`), cấp độ JLPT, từ loại và nghĩa tiếng Việt chuẩn ngữ cảnh.
  - Nhấp nút **Lưu vào sổ từ vựng**: Chỉ ra thông báo Toast xác nhận từ đã được lưu kèm câu ví dụ nguyên bản trong truyện.

---

### PHẦN 4: DEMO TRỢ GIẢNG AI (AI TUTOR INTELLIGENCE) (09:00 - 12:00)
- **Thao tác 1 (Phân tích câu văn phức tạp):**
  - Trên trang đọc, bấm nút **AI Tutor** ở câu:
    > 「ジョバンニは静かに立ち上がって、銀河鉄道の切符をポケットに入れた。」
  - Ngăn kéo trợ giảng mở ra mượt mà, trình diễn:
    1. Bản dịch tiếng Việt tự nhiên giàu sắc thái văn học.
    2. Cấu trúc câu: Chủ ngữ (`ジョバンニは`), Vị ngữ (`入れた`), Tân ngữ (`切符を`), Trạng thái (`静かに立ち上がって`).
    3. Giải thích biến thể ngữ pháp: Phân tích `立ち上がって` là thể -te nối tiếp của động từ ghép `立ち上がる`.
    4. Lời khuyên trọng tâm theo cấp độ JLPT N3.
- **Thao tác 2 (Hỏi đáp tiếp nối - Multi-turn Follow-up):**
  - Gõ câu hỏi: *"Từ '切符' ở đây có khác gì với 'チケット' không?"*
  - AI duy trì ngữ cảnh câu văn và giải thích sự khác biệt giữa từ gốc Hán (Kanji) truyền thống và từ mượn ngoại lai Katakana.

---

### PHẦN 5: DEMO NHÀ HÁT VISUAL NOVEL & HỘI THOẠI NHÂN VẬT (12:00 - 16:00)
- **Màn hình chuyển sang:** Ứng dụng Desktop **Kotoba Odyssey WPF** (hoặc tab Visual Novel trên Web).
- **Thao tác 1 (Sân khấu Visual Novel Shinkai):**
  - Chọn nhân vật **Giovanni** (Đêm trên đường sắt ngân hà).
  - Trình diễn hình nền không gian tàu sao lộng lẫy, sprite nhân vật có bóng đổ và hộp thoại mica mờ (glassmorphism).
- **Thao tác 2 (Hội thoại nhập vai có tính cách nhất quán):**
  - Người học gửi tin nhắn bằng tiếng Nhật: *"今夜はどこへ行くの？"* (Đêm nay cậu đi đâu thế?).
  - Giovanni trả lời tự nhiên, xưng hô bằng đại từ `僕 (boku)` đúng chuẩn nguyên tác văn học của Miyazawa Kenji.
- **Thao tác 3 (Sửa lỗi tiếng Nhật nhẹ nhàng - Gentle Grammar Correction):**
  - Người học thử gửi một câu sai trợ từ: *"私は駅に本を読みます。"* (Sai: phải là `駅で`).
  - Hệ thống nhân vật vẫn tiếp tục trò chuyện thân thiện, đồng thời hiển thị hộp thoại sửa lỗi sư phạm khích lệ người học: *"Gợi ý tự nhiên: Hãy dùng '駅で' khi chỉ hành động diễn ra tại địa điểm nhé!"*
  - Bấm nút lưu từ mới trực tiếp từ câu thoại vào hệ thống SRS.

---

### PHẦN 6: DEMO ÔN TẬP THẺ SRS SM-2 & CHAPTER QUIZ (16:00 - 18:00)
- **Thao tác 1 (Flashcard lật 3D):**
  - Mở mục **Ôn tập từ vựng (SRS)**.
  - Mặt trước thẻ hiển thị Kanji lớn ấn tượng: `切符`.
  - Nhấp chuột hoặc bấm phím Space: Thẻ lật 3D sang mặt sau, hiển thị cách đọc `きっぷ`, nghĩa tiếng Việt và câu ví dụ trong tác phẩm vừa đọc.
  - Nhấp chọn mức đánh giá **Tốt (Good)**: Hệ thống tính toán theo thuật toán SM-2, nâng khoảng cách ngày ôn tập và cộng điểm thưởng XP.
- **Thao tác 2 (Chapter Quiz & Chống gian lận điểm):**
  - Mở bài kiểm tra đọc hiểu cuối chương.
  - Làm bài và nộp bài: Xem phân tích điểm mạnh/yếu (Đọc hiểu, Từ vựng, Ngữ pháp).
  - Làm lại bài lần 2: Điểm đạt được ghi nhận nhưng hệ thống không cộng dồn XP trùng lặp, bảo đảm tính công bằng sư phạm.

---

### PHẦN 7: MINH CHỨNG KIỂM THỬ TỰ ĐỘNG & KẾT LUẬN (18:00 - 20:00)
- **Thao tác trên Terminal:**
  - Mở cửa sổ Terminal, chạy lệnh:
    ```powershell
    dotnet test
    ```
  - Chỉ cho Hội đồng thấy kết quả xuất ra màn hình:
    ```text
    Passed! - Failed: 0, Passed: 117, Skipped: 0, Total: 117 (net9.0)
    ```
- **Lời kết luận bảo vệ:**
  > *"Thưa quý Thầy Cô, 117 bài kiểm thử vượt qua tuyệt đối chính là minh chứng rõ ràng nhất cho sự ổn định, tính chặt chẽ về mặt thuật toán và độ tin cậy của Kotoba Odyssey. Em xin chân thành cảm ơn quý Thầy Cô đã lắng nghe và rất mong nhận được những câu hỏi, đóng góp quý báu từ Hội đồng."*

