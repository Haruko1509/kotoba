# DANH MỤC KIỂM TRA TRƯỚC GIỜ BẢO VỆ & PHÁT HÀNH
## Kotoba Odyssey — Pre-Defense & Release Checklist

Tài liệu này được lập ra để đảm bảo toàn bộ quá trình chuẩn bị máy tính thuyết trình, cấu hình môi trường và vận hành hệ thống Kotoba Odyssey diễn ra trơn tru, không gặp sự cố kỹ thuật trước Hội đồng đánh giá.

---

## 1. YÊU CẦU MÔI TRƯỜNG & PHẦN MỀM (PREREQUISITES)

- [x] **Hệ điều hành:** Windows 10 hoặc Windows 11 (khuyên dùng để chạy WPF Client).
- [x] **.NET SDK:** Phiên bản `.NET 9.0.x` (kiểm tra bằng lệnh `dotnet --version` trong PowerShell/CMD).
- [x] **Trình duyệt Web:** Google Chrome hoặc Microsoft Edge mới nhất (hỗ trợ đầy đủ CSS Grid, Flexbox, backdrop-filter và Web Speech API).
- [x] **Phông chữ tiếng Nhật (Tùy chọn nhưng khuyên dùng):** Cài đặt gói ngôn ngữ tiếng Nhật của Windows (`Japanese Language Pack`) hoặc phông chữ *Noto Sans JP*, *Yu Gothic*.
- [x] **Khóa API Google Gemini (Tùy chọn):** Nếu muốn demo tính năng gọi AI trực tuyến trực tiếp tới Google, chuẩn bị sẵn API Key. *(Lưu ý: Hệ thống đã có sẵn động cơ NLP và Persona Engine nội bộ chất lượng cao nên vẫn hoạt động hoàn hảo 100% khi không có mạng).*

---

## 2. KIỂM TRA DỮ LIỆU & CƠ SỞ DỮ LIỆU (DATABASE INTEGRITY)

- [x] **Đường dẫn cơ sở dữ liệu SQLite:**
  ```text
  %LOCALAPPDATA%\KotobaOdyssey\kotoba.db
  ```
- [x] **Kiểm tra nạp dữ liệu ban đầu (Seeding):**
  - Khi khởi động lần đầu, hệ thống tự động tạo cấu trúc bảng và nạp 6 tác phẩm văn học kinh điển:
    1. *Ginga Tetsudou no Yoru (Đêm trên đường sắt ngân hà - Miyazawa Kenji)*
    2. *Botchan (Cậu ấm - Natsume Soseki)*
    3. *Gon-gitsune (Chú cáo Gon - Niimi Nankichi)*
    4. *Yukiguni (Xứ tuyết - Kawabata Yasunari)*
    5. *Hashire Melos (Chạy đi Melos - Dazai Osamu)*
    6. *Rashomon (La Sinh Môn - Akutagawa Ryunosuke)*
  - Điểm khởi đầu người học: Cấp độ 3, 160 XP, Chuỗi 5 ngày (Streak).
  - Thẻ từ vựng SRS khởi tạo sẵn sàng cho phiên ôn tập.

---

## 3. CÁC BƯỚC KHỞI CHẠY HỆ THỐNG TRƯỚC BUỔI DEMO

### Bước 1: Khởi động Backend Web API (Server)
Mở cửa sổ Terminal/PowerShell thứ nhất:
```powershell
cd d:\kotoba
# Thiết lập API Key nếu có:
$env:GEMINI_API_KEY="AIzaSyYourGeminiApiKeyHere"

# Chạy Backend API (Cổng 5000):
dotnet run --project src/backend/KotobaOdyssey.Api/KotobaOdyssey.Api.csproj
```
*Xác nhận:* Mở trình duyệt truy cập `http://localhost:5000/api/health` → Nhận phản hồi JSON: `{"status":"ok","app":"Kotoba Odyssey Minimal API"}`.

### Bước 2: Khởi chạy Web Companion (Client Web)
Mở cửa sổ Terminal thứ hai hoặc mở trực tiếp:
```powershell
# Cách 1: Chạy máy chủ tĩnh cục bộ (khuyên dùng để tránh CORS file://)
cd d:\kotoba\web-preview
python -m http.server 8080
# Sau đó truy cập: http://localhost:8080
```
Hoặc:
```powershell
# Cách 2: Mở trực tiếp bằng trình duyệt Chrome/Edge
start chrome d:\kotoba\web-preview\index.html
```

### Bước 3: Khởi chạy Desktop Client (WPF Application)
Mở cửa sổ Terminal thứ ba (hoặc khởi chạy từ Visual Studio 2022):
```powershell
cd d:\kotoba
dotnet run --project src/frontend/KotobaOdyssey.UI/KotobaOdyssey.UI.csproj
```
*Xác nhận:* Cửa sổ Kotoba Odyssey xuất hiện với phong cách Cosmic Obsidian mượt mà, đầy đủ thanh điều hướng và dữ liệu từ vựng.

---

## 4. BẢNG KIỂM TRA CHỨC NĂNG TRƯỚC GIỜ G (SMOKE TEST MATRIX)

| STT | Hạng mục kiểm tra | Thao tác thực hiện | Kết quả mong đợi | Xác nhận |
| :---: | :--- | :--- | :--- | :---: |
| 1 | **Dashboard** | Mở trang chủ trên cả Web và Desktop | Thấy Avatar, Level 3, Thanh XP, Nhiệm vụ ngày | [x] |
| 2 | **Thư viện truyện** | Chọn tác phẩm "Ginga Tetsudou no Yoru" | Bìa sách hiển thị đẹp mắt, cấp độ JLPT N3 | [x] |
| 3 | **Trình đọc sách** | Nhấp nút 🌓 Nền để đổi sang theme Sepia / Dark | Màu nền đổi mượt mà, Furigana bật/tắt đúng | [x] |
| 4 | **Tra từ ngữ cảnh** | Nhấp vào từ `銀河` hoặc `切符` | Popover hiện ra cách đọc, từ loại, nghĩa đúng ngữ cảnh | [x] |
| 5 | **Lưu từ vào SRS** | Bấm nút "Lưu vào Flashcard" | Thông báo Toast xanh xuất hiện, số đếm SRS tăng | [x] |
| 6 | **Flashcard 3D** | Vào mục SRS, bấm phím Space hoặc nhấp chuột | Thẻ lật 3D sang mặt sau, hiện câu ví dụ | [x] |
| 7 | **Chấm điểm SM-2** | Bấm nút "Tốt (Good)" | Thẻ chuyển sang từ tiếp theo, lịch ôn cập nhật | [x] |
| 8 | **Chapter Quiz** | Làm bài kiểm tra cuối chương | Hiện kết quả trắc nghiệm và lời giải thích | [x] |
| 9 | **Visual Novel** | Chọn nhân vật Giovanni, gửi câu hỏi | Nhân vật trả lời đúng giọng điệu, đại từ `僕` | [x] |
| 10 | **Settings** | Thay đổi kích thước phông chữ | Cỡ chữ đọc truyện to/nhỏ trực quan | [x] |

---

## 5. KẾ HOẠCH DỰ PHÒNG KHI GẶP SỰ CỐ PHÒNG BẢO VỆ (CONTINGENCY PLAN)

1. **Nếu phòng bảo vệ không có mạng Internet / Wifi bị ngắt:**
   - Hoàn toàn yên tâm tiếp tục demo bình thường. Hệ thống tự động chuyển sang chế độ **Offline-first**, sử dụng từ điển 111 từ ngữ văn học có sẵn và động cơ Persona Engine nội bộ.
2. **Nếu cổng 5000 bị phần mềm khác chiếm dụng:**
   - Sửa dòng cuối cùng trong `src/backend/KotobaOdyssey.Api/Program.cs`: đổi từ `5000` sang `5050`.
   - Cập nhật biến `API_BASE` trong `web-preview/js/api.js` thành `http://localhost:5050/api`.
3. **Nếu hội đồng yêu cầu xem mã nguồn và kiểm thử:**
   - Mở sẵn PowerShell, gõ: `dotnet test` → Toàn bộ 117 bài kiểm thử sẽ chạy trong 2 giây và hiển thị `Passed! (117 Passed)`.

