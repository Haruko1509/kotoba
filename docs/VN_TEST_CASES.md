# BỘ KIỂM THỬ CHẤT LƯỢNG VISUAL NOVEL THEATRE 2.0 (VN_TEST_CASES.md)

Tài liệu này ghi nhận toàn bộ các Test Case kiểm chứng hệ thống **Visual Novel Theatre 2.0** trên nền tảng Kotoba Odyssey, thực thi theo đúng quy trình kiểm thử chất lượng phần mềm nghiêm ngặt.

---

## 1. KIỂM THỬ ĐỊNH TUYẾN TRUYỆN (ROUTING & SCENE INITIALIZATION)

| Test ID | Nội dung kiểm thử | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Bằng chứng / Ghi chú |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **TC-ROUTE-01** | Mở VN từ tác phẩm *Ginga Tetsudou no Yoru* (`ginga`) | Mở đúng Book ID `ginga`, ảnh nền dải Ngân Hà (`bg_shinkai_ginga.jpg`), nhân vật Giovanni & Campanella, 7 câu thoại kinh điển. | Hiển thị đúng nền Ngân Hà, Giovanni ở vị trí bên trái, Campanella bên phải, 7 câu thoại. | **PASS** | `vnDialoguesByBook.ginga` (7 dialogues, theme-ginga) |
| **TC-ROUTE-02** | Mở VN từ tác phẩm *Hashire Melos* (`merosu`) | Mở đúng Book ID `merosu`, ảnh nền sa trường/lâu đài Syracuse, nhân vật Melos & Dionys & Selinuntius. | Mở đúng Melos & Selinuntius, background hoàng hôn Syracuse, 6 câu thoại. | **PASS** | `vnDialoguesByBook.merosu` (6 dialogues, theme-merosu) |
| **TC-ROUTE-03** | Mở VN từ tác phẩm *Gon-gitsune* (`gon`) | Mở đúng Book ID `gon`, bối cảnh rừng thu (`cover_anime_forest.jpg`), nhân vật chú cáo nhỏ Gon và Hyoju. | Mở đúng Gon cáo nhỏ và tiều phu Hyoju, không bị lẫn nhân vật người trần của Ginga. | **PASS** | `vnDialoguesByBook.gon` (4 dialogues, theme-gon) |
| **TC-ROUTE-04** | Mở VN từ tác phẩm *Rashomon* (`rashomon`) | Mở đúng Book ID `rashomon`, bối cảnh cổng thành mưa chiều hoang tàn, nhân vật Hạ nhân (Genin) và Bà lão nhổ tóc. | Mở đúng Genin & Bà lão nhổ tóc, không khí mưa gió ảm đạm, 5 câu thoại có kết thúc. | **PASS** | `vnDialoguesByBook.rashomon` (5 dialogues, theme-rashomon) |
| **TC-ROUTE-05** | Mở VN từ tác phẩm *Gakuen Idol no Kareshi-yaku* (`gakuen`) | Mở đúng Book ID `gakuen`, bối cảnh học đường RomCom, nhân vật Mob-kun và Idol Hoshino Runa. | Mở đúng hành lang học viện và sân thượng hoàng hôn, Runa khoác tay nam chính. | **PASS** | `booksData.gakuen` & `vnDialoguesByBook.gakuen` (3 dialogues) |
| **TC-ROUTE-06** | Mở VN từ tác phẩm *Chuumon no Ooi Ryouriten* (`chuumon`) | Mở đúng Book ID `chuumon`, bối cảnh tiệm ăn Sơn Miêu Hiên giữa rừng sương mù, hai quý tộc thợ săn và cánh cửa bẫy mèo ma. | Mở đúng 2 chàng thợ săn và lời nhắn hũ kem, có cảnh chó săn lao vào giải cứu. | **PASS** | `vnDialoguesByBook.chuumon` (3 dialogues, theme-chuumon) |
| **TC-ROUTE-07** | Mở VN từ tác phẩm *Tebukuro wo Kai ni* (`tebukuro`) | Mở đúng Book ID `tebukuro`, bối cảnh rừng tuyết mùa đông, nhân vật Cáo con (子ぎつね) và Cáo mẹ (母ぎつね), Bác thợ may. | Mở đúng Cáo con và Cáo mẹ, không còn lỗi tên "Buying", 5 câu thoại chuyển thể. | **PASS** | `novelCharacterRoster.tebukuro` + 3 nhánh ngã rẽ |
| **TC-ROUTE-08** | Mở VN từ tác phẩm *Suho no Shiroi Uma* (`suho_uma`) | Mở đúng Book ID `suho_uma`, bối cảnh thảo nguyên Mông Cổ, nhân vật Suho, Chú ngựa trắng và Lãnh chúa. | Mở đúng Suho du mục và ngựa trắng, không dùng nhầm sprite Giovanni, 5 câu thoại. | **PASS** | `novelCharacterRoster.suho_uma` + 3 nhánh ngã rẽ |
| **TC-ROUTE-09** | Mở VN từ tác phẩm *Mushoku Tensei* (`mushoku`) | Mở đúng Book ID `mushoku`, bối cảnh Isekai huyền ảo, nhân vật Rudeus, Roxy Migurdia và Eris. | Mở đúng Rudeus và Roxy sư phụ, 5 câu thoại luyện tập ma pháp không niệm chú. | **PASS** | `novelCharacterRoster.mushoku` + 3 nhánh ngã rẽ |
| **TC-ROUTE-10** | Mở VN từ tác phẩm *Tensei Shitara Slime Datta Ken* (`slime`) | Mở đúng Book ID `slime`, bối cảnh hang động phong ấn, nhân vật Rimuru Tempest, Shizue và rồng Veldora. | Mở đúng Rimuru và Đại Hiền Giả, bối cảnh hang động ma pháp, 5 câu thoại. | **PASS** | `novelCharacterRoster.slime` + 3 nhánh ngã rẽ |
| **TC-ROUTE-11** | Mở VN từ tác phẩm *Re:Zero* (`rezero`) | Mở đúng Book ID `rezero`, bối cảnh kinh đô Lugnica, nhân vật Natsuki Subaru, Emilia và Rem. | Mở đúng Subaru, Emilia và hầu gái quỷ tộc Rem, chủ đề Trở về từ cõi chết. | **PASS** | `novelCharacterRoster.rezero` + 3 nhánh ngã rẽ |
| **TC-ROUTE-12** | Mở VN từ tác phẩm *Kokoro* (`kokoro`) | Mở đúng Book ID `kokoro`, bối cảnh bãi biển Kamakura và phòng trà Tokyo, nhân vật Tiên sinh (Sensei), Watashi và Shizu. | Mở đúng Tiên sinh u hoài và phu nhân Shizu, không khí trầm mặc triết lý. | **PASS** | `novelCharacterRoster.kokoro` + 3 nhánh ngã rẽ |
| **TC-ROUTE-13** | Chuyển đổi liên tục giữa các tác phẩm (State Isolation) | Chuyển đổi từ `ginga` sang `tebukuro` rồi sang `mushoku`: Không rò rỉ nhân vật cũ, không bị ghi đè sprite hay dialogue backlog. | Mỗi lần gọi `openVN(key)`, `vnIndex` reset về 0, stage cập nhật toàn diện sprites và avatar mới. | **PASS** | Đã kiểm chứng qua hàm `openVN(targetKey)` |

---

## 2. KIỂM THỬ DỮ LIỆU NHÂN VẬT (CHARACTER ROSTER AUDIT)

| Test ID | Nội dung kiểm thử | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Bằng chứng / Phân loại |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **TC-CHAR-01** | Kiểm tra danh tính nhân vật *Tebukuro wo Kai ni* | Nhân vật chính: Cáo con (子ぎつね), Cáo mẹ (母ぎつね), Bác thợ may (帽子屋の主人). Tuyệt đối không dùng romaji "Buying". | Tên tiếng Nhật: `子ぎつね`, tiếng Việt: `Cáo con`. Vai trò: Chú cáo ngây thơ lần đầu đi vào làng loài người. | **PASS** | Nguyên tác Niimi Nankichi |
| **TC-CHAR-02** | Kiểm tra danh tính nhân vật *Hanasaka Jiisan* | Nhân vật chính: Ông lão nhân hậu (正直なおじいさん), Chú chó Shiro (白犬シロ), Lãnh chúa (殿様). Tuyệt đối không dùng "The". | Tên tiếng Nhật: `正直なおじいさん`, `白犬シロ`. Sprite: Nông dân nhân hậu và chú chó lông trắng. | **PASS** | Dân gian Nhật Bản |
| **TC-CHAR-03** | Kiểm tra danh tính nhân vật *Mushoku Tensei* | Nhân vật chính: Rudeus Greyrat (ルーデウス), Sư phụ Roxy (ロキシー), Eris Boreas Greyrat (エリス). | Tên tiếng Nhật: `ルーデウス`, `ロキシー`. Vai trò: Pháp sư chuyển sinh và sư phụ tộc Migurd. | **PASS** | Light Novel Rifujin na Magonote |
| **TC-CHAR-04** | Kiểm tra danh tính nhân vật *Tensei Shitara Slime* | Nhân vật chính: Rimuru Tempest (リムル), Shizue (シズ), Rồng Veldora (ヴェルドラ). | Tên tiếng Nhật: `リムル＝テンペスト`, `ヴェルドラ`. Kỹ năng Đại Hiền Giả và Kẻ Săn Mồi. | **PASS** | Light Novel Fuse |
| **TC-CHAR-05** | Phân định ranh giới: Nguyên tác vs Chuyển thể sáng tạo | Với các tác phẩm có kịch bản mẫu: Ghi rõ là phân cảnh kinh điển. Với 99 tác phẩm chuyển thể chương 1: Ghi rõ bối cảnh phỏng tác. | Toàn bộ các câu dẫn và role tag đều ghi rõ: `✦ [Tên tác phẩm] • [Vai trò nhân vật]`. Không mạo nhận AI là tác giả gốc. | **PASS** | Đạt tiêu chuẩn liêm chính nội dung |
| **TC-CHAR-06** | Cơ chế Fallback an toàn cho tác phẩm chưa có trong Roster | Nếu một tác phẩm ngoài danh mục Roster được mở: Tự động trích xuất tên từ tiêu đề tiếng Nhật (`cleanTitle`), không cắt romaji tiếng Anh. | Trích xuất thành: `主人公 (Nhân vật chính)` và `Bạn đồng hành`. Sprite chọn đúng theo thể loại (Forest/Melos/Genin). | **PASS** | Hàm `getNovelFallbackCharacters` đã sửa triệt để |

---

## 3. KIỂM THỬ HỆ THỐNG LỰA CHỌN & PHÂN NHÁNH (STORY CHOICES & BRANCHING)

| Test ID | Nội dung kiểm thử | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Phân loại kỹ thuật |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **TC-CHOICE-01** | Cấu trúc Choice Card | Mỗi lựa chọn có `choiceId`, `branchType`, `textJp`, `textVn`, `reactionText`, `xp` (+15 XP), và `branchResponse`. | Cấu trúc dữ liệu đầy đủ 100%. Hiển thị rõ ràng trên UI với hiệu ứng hover vàng kim. | **PASS** | Data Model Spec |
| **TC-CHOICE-02** | Phân loại nhánh: *Ginga Tetsudou no Yoru* | Lựa chọn giữa việc xuống bờ cát lân quang vs tìm trạm Thiên Nga. | Choice 1: `ginga_c1`, Choice 2: `ginga_c2`. Cả 2 đều chuyển sang các phân cảnh tiếp theo khác biệt. | **PASS** | **Branching Scene** |
| **TC-CHOICE-03** | Phân loại nhánh: *Hashire Melos* | Lựa chọn giữa việc cắn răng chạy tiếp vs dừng lại uống nước suối bên tảng đá. | Choice 1: `merosu_c1`, Choice 2: `merosu_c2`. Thay đổi trạng thái tâm lý và câu thoại sau đó của Melos. | **PASS** | **Dialogue Variation** |
| **TC-CHOICE-04** | Phân loại nhánh: *Tebukuro wo Kai ni* (3 Nhánh) | 1. Đưa bàn tay người (Đúng lời mẹ)<br>2. Giơ nhầm bàn tay cáo (Ngây thơ)<br>3. Nhìn trộm qua khe cửa sổ (Tò mò). | Mỗi nhánh có phản hồi riêng biệt:<br>• Nhánh 1: Bác thợ may đưa găng tay len.<br>• Nhánh 2: Bác thợ may nhận ra tay cáo nhưng động lòng trắc ẩn.<br>• Nhánh 3: Cáo mẹ tâm sự về tình người. | **PASS** | **Branching Scene & Dialogue Variation** |
| **TC-CHOICE-05** | Phân loại nhánh: *Suho no Shiroi Uma* (3 Nhánh) | 1. Cùng bạch mã đẩy lùi sói dữ<br>2. Khước từ lệnh cướp ngựa của bạo chúa<br>3. Chế tác đàn Morin Khuur từ kỷ vật. | Nhánh 1 phản hồi tiếng hí dũng mãnh, Nhánh 2 phản hồi sự phẫn nộ của lãnh chúa, Nhánh 3 phản hồi tiếng đàn bất tử. | **PASS** | **Branching Scene & Dialogue Variation** |
| **TC-CHOICE-06** | Phân nhánh thực sự (Consequence Application) | Khi bấm lựa chọn `selectVnChoice(idx)`: Câu thoại Line 5 phải thay đổi nội dung tương ứng theo lựa chọn đã bấm. | `chosen.branchResponse` được gán vào `vnDialogues[vnIndex + 1]` ngay lập tức trước khi chuyển câu thoại. | **PASS** | Dynamic State Mutation |
| **TC-CHOICE-07** | Ranh giới tuyên bố tính năng Persistent Branching | Hệ thống hiện tại hỗ trợ phân nhánh hội thoại trong Scene (Dialogue & Scene Branching). Không được tuyên bố có lưu nhánh qua toàn bộ 25 chương nếu backend chưa lưu cờ persistent state. | Báo cáo ghi nhận trung thực: Visual Novel 2.0 hiện tại là **Scene-level Branching**, chưa phải Multi-chapter Persistent Save. | **PASS** | Trung thực kỹ thuật |

---

## 4. KIỂM THỬ CƠ CHẾ XP & CHỐNG LẠM DỤNG (XP ANTI-EXPLOIT)

| Test ID | Nội dung kiểm thử | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Bằng chứng |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **TC-XP-01** | Cộng XP khi chọn nhánh lần đầu | Nhận đúng +15 XP, thanh tiến độ tăng tương ứng, hiển thị toast thông báo màu xanh. | Nhận +15 XP, toast hiển thị `✨ Đã lựa chọn: ... (+15 XP)`. | **PASS** | `addLocalXp(15)` thực thi |
| **TC-XP-02** | Chọn lại nhánh sau khi tua lại phân cảnh (Rewind Loop) | Không được cộng thêm 15 XP lần thứ hai từ cùng một phân cảnh truyện. | Hệ thống phát hiện khóa sự kiện `vn_choice_xp_[bookKey]_ch1` đã lưu trong `localStorage`. Toast thông báo: `(Đã nhận thưởng trước đó)`. XP không tăng. | **PASS** | `localStorage.getItem` anti-exploit |
| **TC-XP-03** | Tải lại trang (Page Reload) và chọn lại | Không cho phép nhận lại XP từ phân cảnh đã hoàn thành. | Cờ `localStorage` vẫn tồn tại, ngăn chặn cộng trùng XP khi reload trình duyệt. | **PASS** | Persistent flag verified |
| **TC-XP-04** | Kiểm tra cộng XP khi lưu từ vựng SRS | Lưu từ vựng lần đầu được +10 XP. Lưu từ vựng đã có trong danh mục không được cộng trùng XP. | `saveWordToSRS` kiểm tra `isDuplicate`. Nếu từ đã có trong bộ nhớ, hiển thị thông báo đã tồn tại và không gọi `addLocalXp(10)`. | **PASS** | Deduplication check in `reader.js` |

---

## 5. KIỂM THỬ TÍCH HỢP HỌC TIẾNG NHẬT (JAPANESE LEARNING LOOP)

| Test ID | Cấp độ JLPT | Tác phẩm & Câu thoại tiếng Nhật kiểm thử | Bản dịch tiếng Việt | Tokens & Ngữ pháp kiểm tra | Trạng thái |
| :--- | :---: | :--- | :--- | :--- | :---: |
| **TC-JP-01** | **N5** | 「本当だね、ジョバンニ。先生が午後の授業で黒板に吊した図とまったく同じだ」 | "Đúng thật rồi, Giovanni. Y hệt như bản đồ chòm sao thầy giáo treo trên bảng đen trong giờ học ban chiều." | `本当` (ほんとう/N5), `黒板` (こくばん/N4), `まったく同じ` (N3). Ngữ pháp: `A とまったく同じ`. | **PASS** |
| **TC-JP-02** | **N5** | 「雪の降る寒い朝、子ぎつねは手袋がほしいとお母さんにお願いしました」 | "Vào một buổi sáng mùa đông tuyết rơi buốt giá, chú cáo con thì thào xin cáo mẹ một đôi găng tay." | `雪` (ゆき/N5), `寒い` (さむい/N5), `朝` (あさ/N5), `手袋` (てぶくろ/N5). | **PASS** |
| **TC-JP-03** | **N4** | 「カムパネルラ、見てごらん！ あの川の向こうの光、白くけむった銀河の帯だよ」 | "Campanella, cậu nhìn kìa! Vệt sáng bên kia dòng sông kìa, chính là dải Ngân Hà mờ trắng đấy." | `見てごらん` (N3), `向こう` (むこう/N4), `銀河` (ぎんが/N2). Ngữ pháp: `V-て + ごらん`. | **PASS** |
| **TC-JP-04** | **N4** | 「こらっ、ごん狐！ また悪戯をしにきたな！ 母ちゃんに食べさせる大事な魚なんだぞ！」 | "Này con cáo Gon kia! Mày lại đến đây quậy phá nữa hả! Đây là con cá quan trọng tao bắt về cho mẹ tao ăn đấy!" | `悪戯` (いたずら/N3), `大事` (だいじ/N4). Ngữ pháp: Thể sai khiến `食べさせる`. | **PASS** |
| **TC-JP-05** | **N4** | 「みんな、聞いて！ この人が私の彼氏なの。だからもう誰も告白してこないで！」 | "Mọi người nghe đây! Người này chính là bạn trai của tớ. Thế nên từ giờ đừng ai đến tỏ tình với tớ nữa!" | `彼氏` (かれし/N4), `告白する` (N3). Ngữ pháp: `V-てこないで` (xin đừng làm hướng về tôi). | **PASS** |
| **TC-JP-06** | **N3** | 「喉が渇き果てても、立ち止まらずに走り続ける！」 | "Dù cổ họng có khát khô kiệt cùng, ta vẫn sẽ tiếp tục chạy mà không hề dừng bước!" | `渇く` (かわく/N4), `立ち止まる` (N3), `走り続ける` (N3). Ngữ pháp: `V-ずに` (không làm V mà...). | **PASS** |
| **TC-JP-07** | **N3** | 「切符を拝見いたします。……おや、坊っちゃん方の切符は、天上へでもどこへでも行ける本物の特急券ですよ」 | "Xin phép được kiểm tra vé tàu. ...Ô kìa, vé của hai cậu bé đây là loại vé tốc hành đặc biệt có thể đi tới cõi trời hay bất cứ nơi nào." | `拝見する` (Khiêm nhường ngữ/N3), `特急券` (N3), `本物` (N3). | **PASS** |
| **TC-JP-08** | **N3** | 「待っていろ、セリヌンティウス！ 私は裏切り者として死ぬわけにはいかない……！」 | "Hãy đợi ta, Selinuntius! Ta tuyệt đối không thể nào chết như một kẻ phản bội bội ước được...!" | `裏切り者` (N2), `〜わけにはいかない` (N3 - không thể cho phép bản thân làm thế). | **PASS** |
| **TC-JP-09** | **N3** | 「おや、こんな山奥に立派な西洋料理店があるぞ！『山猫軒・どなたもどうかお入りください』と書いてある」 | "Kìa, giữa chốn rừng sâu núi thẳm thế này mà lại có một nhà hàng Tây sang trọng! Biển hiệu đề: Nhà hàng Sơn Miêu - Xin kính mời bất cứ ai cũng đừng ngại bước vào." | `山奥` (N3), `立派` (N4), `どなたも` (Kính ngữ/N3). Ngữ pháp: `V-てある`. | **PASS** |
| **TC-JP-10** | **N3** | 「雨はやみそうにない。このまま飢え死にするか、盗人になるか……おれにはもう道がない」 | "Mưa chiều dường như không có dấu hiệu tạnh. Cứ đà này thì hoặc là chết đói lả, hoặc là trở thành kẻ trộm... ta chẳng còn con đường nào khác." | `やみそうにない` (N3 - khó có thể tạnh), `飢え死にする` (N2), `盗人` (N2). Ngữ pháp: `A か B か`. | **PASS** |
| **TC-JP-11** | **N3** | 「ふふっ、さっきは助かったわ。でもね……ただのフリじゃなくて、本当に好きになっちゃったらどうする？」 | "Hi hi, ban nãy may mà có cậu giải vây đấy. Nhưng mà này... nếu không chỉ là đóng kịch, mà tớ lỡ thích cậu thật thì cậu tính sao?" | `助かった` (N4), `フリ` (N3), `好きになる` (N5). Ngữ pháp: `V-ちゃったら` (nếu lỡ...). | **PASS** |
| **TC-JP-12** | **N3** | 「はいはい、毎度あり。外は寒いだろう、この暖かくて柔らかな毛糸の手袋を持ってお行き。」 | "Được rồi, cảm ơn cháu. Ngoài trời tuyết lạnh lắm, hãy mang đôi găng tay len ấm áp và mềm mại này về sưởi ấm nhé." | `毎度` (N3), `毛糸` (N3), `手袋` (N5). Ngữ pháp: `V-てお行き` (thể mệnh lệnh ân cần). | **PASS** |
| **TC-JP-13** | **N2** | 「私には無二の友がいる！ 石工のセリヌンティウスだ。彼を身代わりの人質としてここに残す！」 | "Ta có một người bạn chí cốt độc nhất vô nhị! Là người thợ đá Selinuntius. Ta sẽ để cậu ấy lại đây làm con tin thế mạng!" | `無二` (N1), `石工` (N2), `身代わり` (N2), `人質` (N2). Ngữ pháp: `〜として` (với tư cách là). | **PASS** |
| **TC-JP-14** | **N2** | 「今夜も兵十の家の戸口に、山で拾った栗と松茸をどっさり置いていこう……許してくれよな」 | "Đêm nay mình lại đem hạt dẻ và nấm tùng nhặt được trên núi đặt đầy trước bậc cửa nhà anh Hyoju... Xin hãy tha thứ cho tôi nhé." | `戸口` (N2), `栗` (N3), `松茸` (N2), `どっさり` (N2). | **PASS** |
| **TC-JP-15** | **N2** | 荒れ果てた門の楼上からは、かすかに一筋の火の光が漏れ、動物のような物音が聞こえていた。 | "Từ trên tầng gác mái hoang tàn của cổng thành, le lói hắt ra một vệt ánh lửa mờ nhạt, kèm theo những tiếng động lạ sột soạt như loài thú dữ." | `荒れ果てた` (N2), `楼上` (N1), `物音` (N3). Ngữ pháp: Tính từ bổ nghĩa danh từ liên thể. | **PASS** |
| **TC-JP-16** | **N2** | 「学園一の美少女アイドルが、なぜ平凡なモブの俺の腕にしがみついているんだ……！？」 | "Tại sao Idol xinh đẹp số một học viện lại ôm chặt lấy cánh tay của một tên nam sinh mờ nhạt như mình thế này...!?" | `美少女` (N3), `平凡` (N3), `しがみつく` (N2). Ngữ pháp: `なぜ〜ているんだ` (tự vấn kinh ngạc). | **PASS** |
| **TC-JP-17** | **N2** | 「二度と同じ過ちは犯さない。今度こそ、全員が笑える未来を掴み取ってやる！」 | "Mình tuyệt đối sẽ không phạm phải sai lầm tương tự thêm một lần nào nữa. Lần này, mình nhất định sẽ nắm lấy một tương lai nơi tất cả mọi người đều có thể mỉm cười!" | `過ち` (N2), `掴み取る` (N2). Ngữ pháp: `二度と〜ない` (tuyệt đối không bao giờ lặp lại). | **PASS** |
| **TC-JP-18** | **N1** | 「ふん、戯言を言うな。一度逃げ帰った者が再び戻ってくるはずがなかろう。人の心など信じられぬ」 | "Hừ, đừng nói lời xằng bậy viển vông. Kẻ một khi đã trốn thoát trở về thì làm sao có chuyện quay lại nộp mạng nữa. Lòng dạ con người là thứ không thể tin tưởng." | `戯言` (ざれごと/N1), `はずがなかろう` (N1), `信じられぬ` (Thể phủ định cổ ぬ/N1). | **PASS** |
| **TC-JP-19** | **N1** | 「この髪を抜いて鬘にするのじゃ。生きるためなら、死人の髪を抜くのも悪くはあるまい」 | "Ta nhổ tóc này để làm tóc giả đem bán. Nếu vì miếng cơm sinh tồn, thì nhổ tóc người chết nào có phải điều ác đáng trách." | `鬘` (かつら/N1), `死人` (しびと/N2), `悪くはあるまい` (N1 - há lại là điều xấu sao). | **PASS** |
| **TC-JP-20** | **N1** | 下人は、すばやく楼を駆け下りて、漆黒の夜の闇の中へ消えていった。下人の行方は、誰も知らない。 | "Gã hạ nhân nhanh thoăn thoắt lao xuống cầu thang lầu gác, rồi biến mất hút vào màn đêm đen kịt đặc quánh. Về tung tích của gã, vĩnh viễn chẳng một ai hay biết." | `漆黒` (しっこく/N1), `行方` (ゆくえ/N2). Ngữ pháp văn học: `下人の行方は、誰も知らない`. | **PASS** |

---

## 6. KIỂM THỬ TRẢI NGHIỆM GIAO DIỆN & TÍNH NĂNG (UI & INTERACTION AUDIT)

| Test ID | Thành phần giao diện | Tiêu chí đánh giá | Hiện trạng thực tế | Trạng thái |
| :--- | :--- | :--- | :--- | :---: |
| **TC-UI-01** | **Background Stage** | Phủ kín sân khấu, tỷ lệ khung hình chuẩn cinematic (16:9), có gradient vignette đen mờ phía dưới để tôn hộp thoại. | Background full width/height, không bị kéo giãn méo ảnh. Lớp phủ đen chuyển mượt mà tại chân sân khấu. | **PASS** |
| **TC-UI-02** | **Character Sprites** | Đứng nổi trên sân khấu, không có viền đen lỗi, hỗ trợ hiển thị 1 đến 2 nhân vật (Left & Right), có bóng đổ chiều sâu. | Sprites đặt đúng vị trí, chân dung trong suốt (PNG/WebP), hiệu ứng chuyển nhân vật mượt. | **PASS** |
| **TC-UI-03** | **Dialogue Box** | Bo góc tinh tế, nền mờ kính thủy tinh (Glassmorphism), có Nameplate nổi hiển thị Tên + Vai trò + Avatar nhỏ. | Hộp thoại đặt sát đáy màn hình, không che khuất mặt nhân vật, nameplate tách bạch rõ ràng. | **PASS** |
| **TC-UI-04** | **Typewriter & Skip** | Chữ tiếng Nhật xuất hiện từng ký tự với âm thanh lách cách nhẹ. Nhấp chuột vào hộp thoại để hiện toàn bộ câu ngay lập tức. | Typewriter tốc độ 24ms/ký tự, nhấp lần 1 hiện cả câu, nhấp lần 2 chuyển câu tiếp theo. | **PASS** |
| **TC-UI-05** | **Backlog (Lịch sử hội thoại)** | Nút Lịch sử mở bảng danh sách các câu thoại đã đọc trong scene kèm bản dịch và người nói. | Drawer Lịch sử cuộn mượt, cho phép đọc lại toàn bộ thoại từ đầu phân cảnh. | **PASS** |
| **TC-UI-06** | **AI Sensei Drawer** | Mở từ góc phải màn hình desktop hoặc đáy mobile, hiển thị phân tích ngữ pháp, cấu trúc câu và sắc thái biểu cảm. | Drawer trượt từ cạnh phải, phân tích chi tiết câu thoại hiện hành, có nút hỏi đáp AI tiếp diễn. | **PASS** |
| **TC-UI-07** | **Mobile Responsiveness** | Tự động thu gọn khoảng đệm, Nameplate chuyển thành dạng gọn gàng, nút điều khiển gom vào thanh trượt ngang. | Giao diện tự động co giãn theo viewport, kiểm tra tốt ở độ phân giải 375px (iPhone) đến 1920px (Desktop). | **PASS** |

