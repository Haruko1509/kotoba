# BÁO CÁO NGHIỆM THU ĐỘC LẬP VISUAL NOVEL THEATRE 2.0
**Dự án:** Kotoba Odyssey  
**Mã tài liệu:** `docs/VN_2_FINAL_VALIDATION.md`  
**Ngày thực hiện:** 09/11/2026  
**Trạng thái nghiệm thu:** **PARTIAL** (Đạt 100% Logic & Dữ liệu; Giao diện trình duyệt thực tế chưa thể kiểm chứng tự động do rào cản môi trường)

---

## 1. PHẠM VI KIỂM TRA

Đợt kiểm chứng này tập trung xác minh độc lập toàn bộ các thành phần đã triển khai của Visual Novel Theatre 2.0 trên 6 tác phẩm trọng điểm:
1. **Tebukuro wo Kai ni** (`tebukuro` - Cổ tích / Thiếu nhi)
2. **Ginga Tetsudou no Yoru** (`ginga` - Vũ trụ / Kinh điển)
3. **Mushoku Tensei** (`mushoku` - Isekai / Huyền ảo)
4. **Tensei Shitara Slime Datta Ken** (`slime` - Isekai / Huyền ảo)
5. **Kokoro** (`kokoro` - Tâm lý / Cận đại)
6. **Hashire Melos** (`merosu` - Chính kịch / Tình bạn)

### Các trục kiểm chứng chính:
- **Routing & State Isolation:** Đúng ID, đúng ảnh nền, chuyển cảnh dọn sạch dữ liệu cũ.
- **Character Roster & Sprites:** Định danh nhân vật tiếng Nhật/Việt chuẩn xác, triệt tiêu lỗi gán nhãn `Buying` hoặc lạm dụng sprite Giovanni.
- **Dialogue & Story Integrity:** Lời thoại mở đầu nguyên tác/chuyển thể, không chứa placeholder thô `${book.title}の第1章の本文`.
- **Choices & Branching Consequence:** Có 2–3 lựa chọn riêng biệt; mỗi lựa chọn có `choiceId`, phân loại nhánh (`Dialogue Variation` vs `Branching Scene`), phản hồi riêng; chọn nhánh làm thay đổi câu thoại tiếp theo của phân cảnh.
- **XP Anti-Exploit:** Nhận +15 XP lần đầu; tua lại (rewind), chọn lại hoặc tải lại trang (reload) nhận 0 XP.
- **SRS Flashcard Integration:** Lưu từ vựng lần đầu nhận +10 XP; lưu từ trùng lặp phát hiện `isDuplicate = true`, nhận 0 XP và không tạo bản ghi rác.
- **Môi trường Web Preview & Trình duyệt thực tế:** Kiểm tra cổng 8080/8099, HTTP server và khả năng chạy Headless Chrome/Edge.

---

## 2. MÔI TRƯỜNG VÀ CÔNG CỤ THỰC THI

- **Hệ điều hành:** Windows 10/11 Pro (x64)
- **Runtime:** Node.js v24.19.0 (V8 Engine)
- **Harness kiểm chứng Logic:** `scripts/verify_vn2_rigorous.js` (Thực thi mã nguồn thật qua Node VM & Assertions nghiêm ngặt).
- **Web Server:** Node.js HTTP Server (`scripts/test_server_http.js` trên cổng 8099 / `web-preview/serve.js` trên cổng 8080).
- **Trình duyệt kiểm thử:** Google Chrome (`C:\Program Files\Google\Chrome\Application\chrome.exe`), Microsoft Edge (`C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe`).

---

## 3. KẾT QUẢ TỪNG TEST CASE (CHẠY THỰC TẾ)

| Mã kiểm thử | Tác phẩm / Hạng mục | Tiêu chí kiểm tra | Kết quả thực tế quan sát được | Đánh giá |
| :--- | :--- | :--- | :--- | :---: |
| **TC-VAL-01** | *Tebukuro wo Kai ni* (`tebukuro`) | Roster Cáo con, Cáo mẹ, Bác thợ may. Tuyệt đối không chứa chữ "Buying" hay placeholder thô. 3 ngã rẽ. | 5 câu thoại hoàn chỉnh. Không xuất hiện "Buying". Lựa chọn: `tebukuro_c1`, `c2`, `c3`. Chọn nhánh 1 làm câu tiếp theo biến đổi thành lời thoại của Bác thợ may. XP: 100 $\to$ 115 (+15 XP). Tua lại chọn lại: +0 XP. | **PASS** |
| **TC-VAL-02** | *Ginga Tetsudou no Yoru* (`ginga`) | Roster Giovanni, Campanella. Nền Ngân Hà. 2 ngã rẽ có branchResponse riêng biệt. | 7 câu thoại. Lựa chọn: `ginga_c1`, `ginga_c2`. Chọn bãi cát lân quang làm câu tiếp theo đổi thành lời thề của Giovanni. XP: +15 XP lần đầu, +0 XP khi tua lại. | **PASS** |
| **TC-VAL-03** | *Mushoku Tensei* (`mushoku`) | Roster Rudeus, Roxy, Eris. Mở đầu chuyển sinh. 3 ngã rẽ luyện tập ma pháp. | 5 câu thoại. Mở đầu: tai nạn xe tải và quyết tâm làm lại cuộc đời. Lựa chọn: `mushoku_c1`, `c2`, `c3`. Chọn nhánh làm câu tiếp theo đổi thành phản hồi của sư phụ Roxy. XP: +15 XP lần đầu, +0 XP khi tua lại. | **PASS** |
| **TC-VAL-04** | *Tensei Shitara Slime* (`slime`) | Roster Rimuru, Shizue, Veldora. Mở đầu hang động ma pháp. 3 ngã rẽ. | 5 câu thoại. Lời thoại mở đầu: Mikami Satoru thức tỉnh thành Slime trong hang động. Lựa chọn: `slime_c1`, `c2`, `c3`. Chọn nhánh phản hồi Đại Hiền Giả. XP: +15 XP lần đầu, +0 XP khi tua lại. | **PASS** |
| **TC-VAL-05** | *Kokoro* (`kokoro`) | Roster Tiên sinh, Tôi, Phu nhân Shizu. Mở đầu Kamakura. 3 ngã rẽ tâm lý. | 5 câu thoại. Lời thoại mở đầu: cuộc gặp gỡ bên bờ biển Kamakura. Lựa chọn: `kokoro_c1`, `c2`, `c3`. Chọn nhánh phản hồi triết lý u uẩn của Tiên sinh. XP: +15 XP lần đầu, +0 XP khi tua lại. | **PASS** |
| **TC-VAL-06** | *Hashire Melos* (`merosu`) | Roster Melos, Selinuntius, Dionys. 2 ngã rẽ ý chí kiên định. | 6 câu thoại. Lựa chọn: `merosu_c1`, `c2`. Chọn chạy tiếp làm câu tiếp theo đổi thành tiếng thét kiên cường của Melos; chọn uống nước làm câu tiếp theo đổi thành cảnh trùng phùng xúc động với Selinuntius. XP: +15 XP lần đầu, +0 XP khi tua lại. | **PASS** |
| **TC-VAL-07** | **Cơ chế XP Anti-Exploit** | Nhận điểm phân cảnh 1 lần duy nhất; tua lại hoặc tải lại trang không nhận trùng. | Lần 1: +15 XP (100 $\to$ 115). Tua lại chọn lại: +0 XP (vẫn 115). Tải lại phân cảnh với localStorage cũ: +0 XP (vẫn 115). Cờ `vn_choice_xp_[key]_ch1` khóa thành công. | **PASS** |
| **TC-VAL-08** | **Tích hợp Từ vựng & SRS** | Lưu từ vựng lần đầu +10 XP; lưu từ đã có trong kho không cộng điểm đúp. | Lưu lần đầu: `isDuplicate = false`, XP: 115 $\to$ 125 (+10 XP). Lưu lần 2 từ trùng: `isDuplicate = true`, XP giữ nguyên 125 (+0 XP). | **PASS** |
| **TC-VAL-09** | **AI Sensei Drawer** | Mở phân tích ngữ pháp theo câu thoại hiện tại. | Phân tích cú pháp và sắc thái ngữ nghĩa được kết xuất đầy đủ theo đúng câu đang hiển thị. | **PASS** |
| **TC-VAL-10** | **Web Preview HTTP Server** | Khởi động server HTTP phục vụ preview trên cổng nội bộ. | `node scripts/test_server_http.js` khởi động trên cổng 8099: GET `/` trả về 200 OK (83,876 bytes), GET `/js/visual-novel.js` trả về 200 OK (118,279 bytes). | **PASS** |
| **TC-VAL-11** | **Automated Headless Browser Capture** | Mở Chrome/Edge không đầu (headless) để chụp ảnh màn hình phân cảnh trực tiếp. | Cả Chrome và Edge đều bị treo / crash tiến trình do xung đột phân quyền thư mục người dùng Windows (`Cannot create profile at path ...\User Data\Default`) và lỗi named pipe IPC của trình updater. | **BLOCKED** |

---

## 4. CÁC LỖI ĐÃ PHÁT HIỆN VÀ SỬA CHỮA TRONG ĐỢT KIỂM TRA NÀY

Trong quá trình thực hiện kiểm thử nghiêm ngặt bằng assertion thực tế, hệ thống đã phát hiện và xử lý triệt để 5 lỗi logic ngầm:
1. **Lỗi rò rỉ Romaji "Buying Mittens" vào câu thoại mở đầu của Tebukuro:**
   - *Nguyên nhân:* Hàm tạo câu thoại dùng `${book.romaji || book.title}`. Tác phẩm `tebukuro` có `romaji: "Buying Mittens"`, dẫn đến việc xuất hiện cụm từ tiếng Anh `"Buying Mittens" - Khởi đầu câu chuyện`.
   - *Khắc phục:* Sửa `web-preview/js/visual-novel.js` trỏ trực tiếp về `book.title` chuẩn tiếng Nhật.
2. **Thiếu `branchResponse` trong kịch bản mẫu của `ginga` và `merosu`:**
   - *Nguyên nhân:* Các thẻ lựa chọn `ginga_c1`, `ginga_c2`, `merosu_c1`, `merosu_c2` trong `data.js` chỉ có `reactionText` dạng thông báo, thiếu trường `branchResponse` để đột biến câu thoại kế tiếp.
   - *Khắc phục:* Đã bổ sung đầy đủ trường `branchResponse` (người nói, vai trò, bối cảnh, câu thoại tiếng Nhật, bản dịch tiếng Việt, ngữ pháp, từ vựng) cho cả 4 nhánh lựa chọn trong `web-preview/js/data.js`.
3. **Lỗi `currentActiveChoice` rỗng khi gọi chọn nhánh:**
   - *Nguyên nhân:* Hàm `selectVnChoice(idx)` kiểm tra `if (!currentActiveChoice || !currentActiveChoice[idx]) return;`. Nếu người chơi tương tác nhanh trước khi kết thúc hiệu ứng Typewriter hoặc khi chạy kiểm thử tự động, `currentActiveChoice` chưa được gán dẫn đến việc nuốt sự kiện, không cộng XP.
   - *Khắc phục:* Bổ sung fallback an toàn: `const choices = currentActiveChoice || (vnDialogues && vnDialogues[vnIndex] && vnDialogues[vnIndex].choices);`.
4. **Lỗi `ReferenceError: currentChapterNum is not defined` trong `reader.js`:**
   - *Nguyên nhân:* Hàm `saveWordToSRS` truy cập trực tiếp `currentChapterNum || 1`. Khi người dùng ở chế độ Visual Novel mà chưa từng mở Reader View, biến này chưa được khai báo trên phạm vi toàn cục.
   - *Khắc phục:* Bổ sung `var currentChapterNum = typeof currentChapterNum !== 'undefined' ? currentChapterNum : 1;` ở đầu file `web-preview/js/reader.js`.
5. **Lỗi `ReferenceError: updateCardUI is not defined` trong `api.js`:**
   - *Nguyên nhân:* Hàm `refreshSrsDeckFromLocal` trong `api.js` gọi vô điều kiện `updateCardUI(srsIndex)`. Khi ở màn hình Visual Novel, component Flashcard chưa khởi tạo gây văng lỗi runtime.
   - *Khắc phục:* Bổ sung kiểm tra an toàn: `if (srsDeck.length > 0 && typeof updateCardUI === 'function') { updateCardUI(srsIndex); }`.

---

## 5. CÁC LỖI VÀ GIỚI HẠN CÒN TỒN TẠI

1. **Giới hạn của cơ chế chống spam XP (Client-side localStorage):**
   - Rào chắn chống nhận thưởng trùng lặp hiện dựa trên cờ `localStorage.getItem('vn_choice_xp_[bookKey]_ch1')`.
   - Nếu người dùng chủ động xóa dữ liệu trình duyệt (`localStorage.clear()`) hoặc chuyển sang chế độ Ẩn danh (Incognito Mode), cờ này sẽ bị mất và người dùng có thể nhận lại +15 XP.
   - *Giải pháp lâu dài:* Cần ghi nhận lịch sử hoàn thành phân cảnh vào bảng `UserEventLogs` ở Backend SQLite khi người dùng đăng nhập tài khoản chính thức.
2. **Persistent Branching xuyên suốt 25 chương:**
   - Visual Novel Theatre 2.0 hiện chỉ hỗ trợ **Scene-level Branching** (phân nhánh ngữ cảnh và câu thoại tiếp theo trong cùng phân cảnh).
   - Lựa chọn tại Chương 1 chưa làm thay đổi cốt truyện của Chương 2 hay Chương 25 do cơ sở dữ liệu chưa có bảng lưu trạng thái phân nhánh theo tiến trình chương dài hạn.

---

## 6. NHỮNG NỘI DUNG CHƯA THỂ KIỂM CHỨNG TỰ ĐỘNG

- **Kiểm chứng mắt nhìn thực tế qua Headless Browser (Visual Rendering Verification):**
  - Trong môi trường sandbox Windows hiện tại, các tiến trình Chrome và Edge không thể tạo profile người dùng tự động (`Cannot create profile at path ...\User Data\Default`), gây timeout khi chụp screenshot tự động.
  - Do đó, báo cáo này **chưa có ảnh chụp giao diện tự động từ Chrome/Edge headless**.
  - Trải nghiệm giao diện thực tế (hiệu ứng chuyển động Typewriter, độ mờ nền Glassmorphism, tỷ lệ standee trên màn hình điện thoại) cần người dùng mở trực tiếp file `web-preview/index.html` hoặc truy cập `http://localhost:8080` trên trình duyệt thông thường để đánh giá bằng mắt.

---

## 7. ẢNH CHỤP GIAO DIỆN

- **Trạng thái:** *Chưa tạo được tự động qua lệnh do hạn chế sandbox profile trình duyệt Windows.*
- **Đường dẫn xem trực tiếp cho người dùng:** `http://localhost:8080/#vn/tebukuro` hoặc mở trực tiếp `d:\kotoba\web-preview\index.html#vn/tebukuro`.

---

## 8. KẾT LUẬN NGHIỆM THU

### Trạng thái: **PARTIAL (CHẤP THUẬN CÓ ĐIỀU KIỆN)**

- **Logic, Dữ liệu, Phân nhánh, XP & SRS:** **PASS 100% (6/6 tác phẩm đã được kiểm chứng bằng assertion)**. Không còn tình trạng nhân vật Giovanni xuất hiện ở truyện khác, không còn tên "Buying", không còn placeholder `${book.title}の第1章の本文`, cơ chế chống cày XP và chống lưu trùng SRS hoạt động chuẩn xác.
- **Môi trường Trình duyệt Tự động (Headless Automated Screenshot):** **BLOCKED** do rào cản phân quyền hệ điều hành Windows trên máy cục bộ.
- **Hành động tiếp theo:** Đóng giai đoạn kiểm thử Visual Novel Theatre 2.0 ở trạng thái PARTIAL với báo cáo kỹ thuật trung thực, giữ nguyên mã nguồn ổn định và dừng lại theo đúng yêu cầu nghiệm thu.

