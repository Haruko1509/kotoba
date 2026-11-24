# DANH MỤC HẠN CHẾ ĐÃ BIẾT & HƯỚNG PHÁT TRIỂN
## Kotoba Odyssey — Known Limitations & Future Roadmap

Tài liệu này ghi nhận trung thực các hạn chế kỹ thuật hiện tại, phạm vi thiết kế có chủ đích và phương án giải trình thỏa đáng trước Hội đồng chấm khóa luận tốt nghiệp.

---

## 1. PHÂN LOẠI CÁC HẠN CHẾ ĐÃ BIẾT (KNOWN LIMITATIONS)

### 1. Phụ thuộc vào giọng đọc Text-to-Speech (TTS) của trình duyệt / hệ điều hành
- **Mô tả:** Chức năng phát âm tiếng Nhật trên Web Companion sử dụng chuẩn tiêu chuẩn `window.speechSynthesis`. Nếu máy tính chạy Windows chưa cài đặt gói giọng đọc tiếng Nhật (`Microsoft Haruka` hoặc `Google 日本語`), trình duyệt có thể không phát ra âm thanh hoặc dùng giọng mặc định.
- **Mức độ ảnh hưởng:** P2 (Trải nghiệm âm thanh bổ trợ).
- **Phương án giải trình trước Hội đồng:** Ứng dụng ưu tiên sử dụng API bản địa (Native Web Speech API) để tối ưu hiệu năng và không phụ thuộc vào dịch vụ trả phí bên thứ ba. Hướng phát triển tiếp theo là tích hợp sẵn thư viện âm thanh TTS ngoại tuyến (như Piper TTS hoặc voice audio clip đóng gói cục bộ).

### 2. Dung lượng kho từ điển nội bộ trong bộ nhớ (Embedded Dictionary)
- **Mô tả:** Để đảm bảo ứng dụng có thể khởi động ngay lập tức dưới 1 giây và chạy hoàn toàn ngoại tuyến mà không cần tải file từ điển lớn (như JMdict ~150MB), phiên bản hiện tại đóng gói sẵn 111 từ vựng và thành ngữ văn học xuất hiện trực tiếp trong 6 tiểu thuyết kinh điển.
- **Mức độ ảnh hưởng:** P2 (Độ bao phủ từ vựng ngoài sách giáo khoa).
- **Phương án giải trình trước Hội đồng:** Thiết kế của Kotoba Odyssey tập trung sâu vào phương pháp *Extensive Reading* (Đọc sâu có chủ đích theo tác phẩm). Các từ vựng ngoài sách sẽ được tự động tra cứu và phân tích ngữ pháp khi người học kết nối với Google Gemini AI hoặc qua các gói mở rộng từ điển SQLite tiếp theo.

### 3. Đồng bộ hóa giữa Web Companion và WPF Desktop qua Local Network
- **Mô tả:** WPF Desktop kết nối trực tiếp với file SQLite `%LOCALAPPDATA%\KotobaOdyssey\kotoba.db` (in-process). Web Companion kết nối với file này thông qua Web API (`KotobaOdyssey.Api`). Nếu người dùng mở Web Companion mà không bật Web API, Web Companion sẽ tự động kích hoạt cơ chế Local Storage Mock Sync để giữ trải nghiệm liền mạch nhưng dữ liệu sẽ chưa được đồng bộ vào file SQLite trung tâm cho đến khi Web API chạy.
- **Mức độ ảnh hưởng:** P2 (Trải nghiệm người dùng khi tắt server).
- **Phương án giải trình trước Hội đồng:** Đây là kiến trúc Hybrid Client-Server. Trong môi trường triển khai thực tế trên máy chủ đám mây, cả hai client sẽ cùng gọi qua API gateway an toàn, loại bỏ hoàn toàn sự phụ thuộc vào tiến trình API cục bộ.

### 4. Giới hạn tỷ lệ gọi API (Rate Limit / Quota) của Google Gemini Free Tier
- **Mô tả:** Bản miễn phí của Google Gemini có giới hạn 15 RPM (Request Per Minute). Nếu người dùng bấm tra cứu câu văn liên tục với tần suất rất cao trong thời gian ngắn, API của Google có thể trả về mã lỗi `429 Too Many Requests`.
- **Mức độ ảnh hưởng:** P2 (Ngoại cảnh).
- **Phương án giải trình trước Hội đồng:** Kotoba Odyssey đã được lập trình sẵn hệ thống tự phục hồi đa tầng (Multi-tier Graceful Degradation): Khi phát hiện lỗi 429 hoặc timeout từ Google, hệ thống tự động chuyển sang bộ giải thích ngữ pháp nội bộ (Rule-based NLP & Persona Engine) với độ trễ 0ms, đảm bảo người học không bao giờ gặp màn hình trắng hay lỗi gián đoạn.

---

## 2. HƯỚNG PHÁT TRIỂN SAU TỐT NGHIỆP (FUTURE ROADMAP)

1. **Giai đoạn tiếp theo (Kotoba Odyssey Cloud & Mobile App):**
   - Đóng gói ứng dụng di động đa nền tảng (Flutter hoặc .NET MAUI) kết nối cùng cơ sở dữ liệu đám mây.
   - Thêm tính năng đọc sách theo phương đứng (Vertical Text Flow - 縦書き) chuẩn phong cách truyện xuất bản Nhật Bản.
2. **Nâng cao năng lực AI:**
   - Hỗ trợ giọng đọc cảm xúc riêng cho từng nhân vật bằng công nghệ Voice Cloning (VITS / Coqui TTS).
   - Tự động nhận diện trình độ JLPT của người học thông qua lịch sử tra từ và bài kiểm tra để tự động điều chỉnh độ khó của văn bản.
3. **Cộng đồng học tập:**
   - Cho phép người dùng chia sẻ danh sách từ vựng (Vocabulary Decks) và bài Quiz tự tạo cho cộng đồng.

