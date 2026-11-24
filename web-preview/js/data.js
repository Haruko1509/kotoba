// ===================================================================
// KOTOBA ODYSSEY - MOCK & FALLBACK DATA REGISTRY
// ===================================================================

// State Data - Novels & Stories
const booksData = {
  "gon": {
    "id": "gon",
    "title": "ごんぎつね",
    "romaji": "Gon, the Little Fox",
    "author": "新美南吉",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/cover_gon.svg",
    "synopsis": "Cáo nhỏ Gon mang hạt dẻ và nấm chuộc lỗi với anh lính Hyoju.",
    "chapters": "25 Chương",
    "words": "17.550 Chữ Hán",
    "vocab": "356 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "momotaro": {
    "id": "momotaro",
    "title": "桃太郎",
    "romaji": "Momotaro",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_adventurer.jpg",
    "synopsis": "Cậu bé quả đào cùng chó, khỉ, chim trĩ lên Đảo Quỷ Oni trừ tà.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "urashima": {
    "id": "urashima",
    "title": "浦島太郎",
    "romaji": "Urashima Taro",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_sunset.jpg",
    "synopsis": "Chàng đánh cá cứu rùa biển và chuyến thăm Long Cung diệu kỳ.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kaguyahime": {
    "id": "kaguyahime",
    "title": "かぐや姫（竹取物語）",
    "romaji": "Princess Kaguya",
    "author": "日本古典",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_night_shrine.jpg",
    "synopsis": "Nàng công chúa sinh ra từ ống tre vàng rực rỡ trở về Cung Trăng.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "tsuru": {
    "id": "tsuru",
    "title": "鶴の恩返し",
    "romaji": "The Crane Wife",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/snow_country.png",
    "synopsis": "Con hạc trắng hóa thân thành cô gái ngày đêm dệt lụa đền ơn.",
    "chapters": "25 Chương",
    "words": "19.250 Chữ Hán",
    "vocab": "380 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kasajizou": {
    "id": "kasajizou",
    "title": "笠地蔵",
    "romaji": "Kasa Jizo",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_forest.jpg",
    "synopsis": "Đội nón lá cho tượng Địa Tạng và phép màu đêm giao thừa.",
    "chapters": "25 Chương",
    "words": "22.650 Chữ Hán",
    "vocab": "428 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "sarukani": {
    "id": "sarukani",
    "title": "さるかに合戦",
    "romaji": "The Crab and the Monkey",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_slow_life.jpg",
    "synopsis": "Cuộc chiến giành lại công lý giữa chú cua hiền lành và khỉ gian xảo.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "omusubi": {
    "id": "omusubi",
    "title": "おむすびころりん",
    "romaji": "The Rolling Rice Ball",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_isekai_gourmet.jpg",
    "synopsis": "Nắm cơm lăn vào hang chuột mở ra thế giới ca hát vui vẻ.",
    "chapters": "25 Chương",
    "words": "20.950 Chữ Hán",
    "vocab": "404 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "tebukuro": {
    "id": "tebukuro",
    "title": "手袋を買いに",
    "romaji": "Buying Mittens",
    "author": "新美南吉",
    "level": "N5",
    "genre": "Children",
    "cover": "assets/covers/cover_anime_cafe.jpg",
    "synopsis": "Cáo mẹ dạy cáo con mua đôi găng tay ấm áp giữa mùa đông tuyết rơi.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "hanasaka": {
    "id": "hanasaka",
    "title": "花さかじいさん",
    "romaji": "The Old Man Who Made Trees Bloom",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/whisper_of_sakura.jpg",
    "synopsis": "Nắm tro kỳ diệu gieo rắc mùa xuân khiến cây cối nở rộ hoa anh đào.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kachikachi": {
    "id": "kachikachi",
    "title": "かちかち山",
    "romaji": "Mount Kachi-Kachi",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_forest.jpg",
    "synopsis": "Chú thỏ mưu trí trừng trị con lửng ranh mãnh để báo ân ông lão.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "issun": {
    "id": "issun",
    "title": "一寸法師",
    "romaji": "The One-Inch Boy",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_adventurer.jpg",
    "synopsis": "Chàng tí hon cao một tấc dùng kim khâu làm kiếm trừ quỷ cứu công chúa.",
    "chapters": "25 Chương",
    "words": "19.250 Chữ Hán",
    "vocab": "380 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kintaro": {
    "id": "kintaro",
    "title": "金太郎",
    "romaji": "Kintaro",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_dungeon_adventure.jpg",
    "synopsis": "Cậu bé vàng mặc yếm đỏ kết bạn cùng muông thú núi Ashigara.",
    "chapters": "25 Chương",
    "words": "20.950 Chữ Hán",
    "vocab": "404 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kobutori": {
    "id": "kobutori",
    "title": "こぶとりじいさん",
    "romaji": "The Old Men with Wens",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_night_shrine.jpg",
    "synopsis": "Ông lão nhảy múa cùng bầy yêu quái và được lấy đi khối bướu trên má.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "yukionna": {
    "id": "yukionna",
    "title": "雪女",
    "romaji": "Yuki-Onna",
    "author": "小泉八雲",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_yukiguni.svg",
    "synopsis": "Truyền thuyết người đàn bà tuyết xinh đẹp tuyệt trần trong đêm bão tuyết.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "warashibe": {
    "id": "warashibe",
    "title": "わらしべ長者",
    "romaji": "The Straw Millionaire",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/saikyou_no_kanteishi_tte_dare_no_koto.jpg",
    "synopsis": "Từ một cọng rơm duy nhất, chàng trai nghèo vươn lên thành triệu phú.",
    "chapters": "25 Chương",
    "words": "22.650 Chữ Hán",
    "vocab": "428 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "nezumi": {
    "id": "nezumi",
    "title": "ネズミの嫁入り",
    "romaji": "The Mouse Wedding",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/oyomesan.png",
    "synopsis": "Gia đình chuột đi tìm chàng rể quyền năng nhất thế gian.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "sanmai": {
    "id": "sanmai",
    "title": "三枚のお札",
    "romaji": "The Three Talismans",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_night_shrine.jpg",
    "synopsis": "Chú tiểu dùng ba lá bùa thoát khỏi sự truy đuổi của mụ phù thủy.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "usagi_kame": {
    "id": "usagi_kame",
    "title": "うさぎとかめ",
    "romaji": "The Tortoise and the Hare",
    "author": "イソップ童話",
    "level": "N5",
    "genre": "Fable",
    "cover": "assets/covers/cover_slow_life.jpg",
    "synopsis": "Bài học kiên trì từng bước vững chắc của rùa chiến thắng thỏ kiêu ngạo.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "bunbuku": {
    "id": "bunbuku",
    "title": "ぶんぶく茶釜",
    "romaji": "The Magic Teakettle",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/daidokoro_escape.png",
    "synopsis": "Chú lửng biến thành ấm trà biết đi biểu diễn xiếc mang lại tiếng cười.",
    "chapters": "25 Chương",
    "words": "20.950 Chữ Hán",
    "vocab": "404 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "shitakiri": {
    "id": "shitakiri",
    "title": "舌切り雀",
    "romaji": "The Tongue-Cut Sparrow",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/bokurawayomiwomachigaeru.jpg",
    "synopsis": "Chú chim sẻ biết nói tiếng người đền ơn ông lão hiền lành tráp vàng tráp bạc.",
    "chapters": "25 Chương",
    "words": "22.650 Chữ Hán",
    "vocab": "428 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "chikaratarou": {
    "id": "chikaratarou",
    "title": "力太郎",
    "romaji": "Chikaratarou",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/bullbuster.jpg",
    "synopsis": "Cậu bé sinh ra từ bùn đất mang sức mạnh dời non lấp biển trừ yêu diệt quỷ.",
    "chapters": "25 Chương",
    "words": "25.200 Chữ Hán",
    "vocab": "464 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "yamata": {
    "id": "yamata",
    "title": "八岐大蛇退治",
    "romaji": "Yamata no Orochi",
    "author": "日本神話",
    "level": "N5",
    "genre": "Mythology",
    "cover": "assets/covers/crimson_academy.jpg",
    "synopsis": "Thần Susanoo dùng rượu quý phục kích và trảm xà vương tám đầu tám đuôi.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "inaba": {
    "id": "inaba",
    "title": "因幡の白兎",
    "romaji": "The White Hare of Inaba",
    "author": "日本神話",
    "level": "N5",
    "genre": "Mythology",
    "cover": "assets/covers/usagi_goutou_ni_wa_shinde_morau.jpg",
    "synopsis": "Chú thỏ trắng Inaba được thần Okuninushi chữa lành và se duyên công chúa.",
    "chapters": "25 Chương",
    "words": "19.250 Chữ Hán",
    "vocab": "380 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "tentou": {
    "id": "tentou",
    "title": "天道さん金の鎖",
    "romaji": "Heavenly Chain of Gold",
    "author": "日本昔話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/the_forgotten_mage.jpg",
    "synopsis": "Ba chị em trèo lên sợi xích vàng của thần Mặt Trời trốn thoát sói dữ.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "neko_danba": {
    "id": "neko_danba",
    "title": "猫の檀家",
    "romaji": "The Cat Monk",
    "author": "民話",
    "level": "N5",
    "genre": "Animals",
    "cover": "assets/covers/meshi_buff_shokudou.jpg",
    "synopsis": "Chú mèo tam thể thông minh cứu ngôi chùa làng khỏi cơn khánh kiệt.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "inu_hachiko": {
    "id": "inu_hachiko",
    "title": "忠犬ハチ公物語",
    "romaji": "The Story of Hachiko",
    "author": "実話文学",
    "level": "N5",
    "genre": "Touching Animal",
    "cover": "assets/covers/koinu_wo_tasuketara_class_de_ninki_no.jpg",
    "synopsis": "Câu chuyện chú chó Hachiko kiên nhẫn đợi chủ suốt mười năm ở ga Shibuya.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "daruma": {
    "id": "daruma",
    "title": "だるまさんの休日",
    "romaji": "Daruma’s Day Off",
    "author": "絵本物語",
    "level": "N5",
    "genre": "Cute Life",
    "cover": "assets/covers/cover_anime_cafe.jpg",
    "synopsis": "Một ngày dạo phố ngộ nghĩnh và ấm áp của búp bê Daruma may mắn.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "manjuu": {
    "id": "manjuu",
    "title": "まんじゅうこわい",
    "romaji": "Scared of Manju Buns",
    "author": "落語民話",
    "level": "N5",
    "genre": "Rakugo Comedy",
    "cover": "assets/covers/daidokoro_escape.png",
    "synopsis": "Truyện cười kịch Rakugo về chàng trai giả vờ sợ bánh bao ngọt.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "jugemu": {
    "id": "jugemu",
    "title": "じゅげむ",
    "romaji": "Jugemu",
    "author": "落語民話",
    "level": "N5",
    "genre": "Rakugo Comedy",
    "cover": "assets/covers/bokutachi_no_seishun_wa_chotto_dake_tokubetsu.jpg",
    "synopsis": "Cái tên dài vô tận chúc phúc cho em bé gây nên những tình huống oái oăm.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "mizukake": {
    "id": "mizukake",
    "title": "水かけ地蔵",
    "romaji": "Water Pouring Jizo",
    "author": "民話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_night_shrine.jpg",
    "synopsis": "Gáo nước trong lành tưới mát pho tượng Địa Tạng và niềm vui làng quê.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kitsune_yome": {
    "id": "kitsune_yome",
    "title": "狐の嫁入り",
    "romaji": "Fox Wedding in the Rain",
    "author": "民話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_sunset.jpg",
    "synopsis": "Cơn mưa rào giữa trời nắng rực rỡ và đoàn rước dâu bí ẩn của loài cáo.",
    "chapters": "25 Chương",
    "words": "25.200 Chữ Hán",
    "vocab": "464 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "tanuki_shoujin": {
    "id": "tanuki_shoujin",
    "title": "たぬきの糸車",
    "romaji": "The Tanuki and the Spinning Wheel",
    "author": "岸なみ",
    "level": "N5",
    "genre": "Children",
    "cover": "assets/covers/cover_slow_life.jpg",
    "synopsis": "Chú chồn nhỏ nhìn lén bà cụ kéo sợi và bắt chước quay tơ đáng yêu.",
    "chapters": "25 Chương",
    "words": "26.900 Chữ Hán",
    "vocab": "488 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kujira_kumo": {
    "id": "kujira_kumo",
    "title": "くじらぐも",
    "romaji": "The Whale Cloud",
    "author": "中川李枝子",
    "level": "N5",
    "genre": "School Children",
    "cover": "assets/covers/spring_after_graduation.jpg",
    "synopsis": "Đám mây hình cá voi trắng mời các bạn học sinh tiểu học cùng bay lên trời cao.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "suho_uma": {
    "id": "suho_uma",
    "title": "スーホの白い馬",
    "romaji": "Suho’s White Horse",
    "author": "モンゴル民話",
    "level": "N5",
    "genre": "Touching Legend",
    "cover": "assets/covers/our_last_summer.jpg",
    "synopsis": "Tình bạn cảm động giữa cậu bé du mục Suho và chú bạch mã sinh ra cây đàn Mã Đầu.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "chui_kintaro": {
    "id": "chui_kintaro",
    "title": "ちいちゃんのかげおくり",
    "romaji": "Chi-chan's Shadow Sending",
    "author": "あまんきみこ",
    "level": "N5",
    "genre": "Touching Peace",
    "cover": "assets/covers/whisper_of_sakura.jpg",
    "synopsis": "Trò chơi nhìn bóng dưới bầu trời xanh thẳm và ước vọng hòa bình trong sáng.",
    "chapters": "25 Chương",
    "words": "25.200 Chữ Hán",
    "vocab": "464 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "ookina_kabu": {
    "id": "ookina_kabu",
    "title": "大きなかぶ",
    "romaji": "The Enormous Turnip",
    "author": "再話文学",
    "level": "N5",
    "genre": "Children",
    "cover": "assets/covers/isekai_yururi_nouka_seikatsu.jpg",
    "synopsis": "Cả gia đình cùng chó, mèo, chuột tí hon đoàn kết nhổ củ cải khổng lồ.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "guri_gura": {
    "id": "guri_gura",
    "title": "ぐりとぐらの森の遠足",
    "romaji": "Forest Picnic of Guri and Gura",
    "author": "中川李枝子",
    "level": "N5",
    "genre": "Children Cooking",
    "cover": "assets/covers/meshi_buff_shokudou.jpg",
    "synopsis": "Hai chú chuột rừng sinh đôi nướng bánh xốp thơm lừng chiêu đãi muôn thú.",
    "chapters": "25 Chương",
    "words": "22.650 Chữ Hán",
    "vocab": "428 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "shiroi_boushi": {
    "id": "shiroi_boushi",
    "title": "白いぼうし",
    "romaji": "The White Hat",
    "author": "あまんきみこ",
    "level": "N5",
    "genre": "Fantasy Everyday",
    "cover": "assets/covers/the_bookstore_promise.jpg",
    "synopsis": "Bác tài xế Matsui nhặt được chiếc mũ trắng giấu chú bướm mùa hè kỳ diệu.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "dendenmushi": {
    "id": "dendenmushi",
    "title": "でんでんむしのかなしみ",
    "romaji": "The Sorrow of the Snail",
    "author": "新美南吉",
    "level": "N5",
    "genre": "Philosophy Fable",
    "cover": "assets/covers/cover_anime_forest.jpg",
    "synopsis": "Chú ốc sên nhận ra ai trong đời cũng mang trong mình một nỗi buồn riêng.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "ame_furase": {
    "id": "ame_furase",
    "title": "雨ふらしの龍",
    "romaji": "The Rain Dragon",
    "author": "民話",
    "level": "N5",
    "genre": "Folktale",
    "cover": "assets/covers/cover_anime_sunset.jpg",
    "synopsis": "Rồng con hy sinh giọt nước mắt cuối cùng để mang mưa về cho bản làng khô hạn.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "hoshi_hime": {
    "id": "hoshi_hime",
    "title": "星のお姫さま",
    "romaji": "Princess of the Stars",
    "author": "メルヘン",
    "level": "N5",
    "genre": "Fairy Tale",
    "cover": "assets/covers/cover_anime_night_shrine.jpg",
    "synopsis": "Nàng công chúa nhỏ thắp sáng những vì sao đêm dẫn đường cho lữ khách.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "morino_kajiya": {
    "id": "morino_kajiya",
    "title": "森の小さな鍛冶屋さん",
    "romaji": "Little Blacksmith of the Forest",
    "author": "メルヘン",
    "level": "N5",
    "genre": "Crafting",
    "cover": "assets/covers/isekai_craft_gurashi.jpg",
    "synopsis": "Thợ rèn tí hon rèn móng ngựa vàng mang lại may mắn cho muông thú.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "pan_ya_guma": {
    "id": "pan_ya_guma",
    "title": "パン屋のくまさん",
    "romaji": "The Baker Bear",
    "author": "メルヘン",
    "level": "N5",
    "genre": "Bakery Slice of Life",
    "cover": "assets/covers/cover_anime_cafe.jpg",
    "synopsis": "Mỗi sáng chú gấu nâu thức dậy sớm nướng những ổ bánh mì mật ong thơm phức.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "koinu_kororin": {
    "id": "koinu_kororin",
    "title": "子犬のポチの冒険",
    "romaji": "Puppy Pochi's Adventure",
    "author": "日常童話",
    "level": "N5",
    "genre": "Cute Pets",
    "cover": "assets/covers/koinu_wo_tasuketara_class_de_ninki_no.jpg",
    "synopsis": "Chuyến phiêu lưu khám phá khu vườn nhà của chú cún con tai cụp Pochi.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "botchan": {
    "id": "botchan",
    "title": "坊っちゃん",
    "romaji": "Botchan",
    "author": "夏目漱石",
    "level": "N4",
    "genre": "School Humor",
    "cover": "assets/cover_botchan.jpg",
    "synopsis": "Thầy giáo trẻ bộc trực từ Tokyo về dạy học ở vùng quê Shikoku.",
    "chapters": "25 Chương",
    "words": "20.950 Chữ Hán",
    "vocab": "404 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kumabear": {
    "id": "kumabear",
    "title": "くまクマ熊ベアー",
    "romaji": "Kuma Kuma Kuma Bear",
    "author": "くまなの",
    "level": "N4",
    "genre": "Isekai Slice of Life",
    "cover": "assets/covers/cover_anime_forest.jpg",
    "synopsis": "Yuna 15 tuổi trong bộ đồ gấu toàn năng tự do mở quán và phiêu lưu.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "katei": {
    "id": "katei",
    "title": "幼馴染の妹の家庭教師を始めたら",
    "romaji": "Tutoring My Childhood Friend's Sister",
    "author": "逢坂",
    "level": "N4",
    "genre": "School Romcom",
    "cover": "assets/covers/osananajimi_no_imouto_no_kateikyoushi_wo_hajimetara.jpg",
    "synopsis": "Làm gia sư ôn thi tiếng Nhật cho em gái nghịch ngợm của bạn thân.",
    "chapters": "25 Chương",
    "words": "19.250 Chữ Hán",
    "vocab": "380 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "isekai_nobu": {
    "id": "isekai_nobu",
    "title": "異世界居酒屋「のぶ」",
    "romaji": "Isekai Izakaya Nobu",
    "author": "蝉川夏哉",
    "level": "N4",
    "genre": "Gourmet",
    "cover": "assets/covers/isekai_izakaya_nobu.jpg",
    "synopsis": "Quán nhậu Nhật Bản mở cửa thông tới thành phố cổ phương Tây.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "tondemo_meshi": {
    "id": "tondemo_meshi",
    "title": "とんでもスキルで異世界放浪メシ",
    "romaji": "Campfire Cooking",
    "author": "江口連",
    "level": "N4",
    "genre": "Gourmet Adventure",
    "cover": "assets/covers/tondemo_skill_de_isekai_hourou_meshi.jpg",
    "synopsis": "Kỹ năng siêu thị nấu ăn thuần hóa thần thú ma sói Fenrir.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "nonbiri_nouka": {
    "id": "nonbiri_nouka",
    "title": "異世界のんびり農家",
    "romaji": "Farming Life in Another World",
    "author": "内藤騎之介",
    "level": "N4",
    "genre": "Farming Slow Life",
    "cover": "assets/covers/isekai_yururi_nouka_seikatsu.jpg",
    "synopsis": "Cuộc sống canh tác nông trang yên bình cùng vạn năng nông cụ.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "otonari_tenshi": {
    "id": "otonari_tenshi",
    "title": "お隣の天使様にいつの間にか駄目人間にされていた件",
    "romaji": "The Angel Next Door",
    "author": "佐伯さん",
    "level": "N4",
    "genre": "School Romance",
    "cover": "assets/covers/otonari.jpg",
    "synopsis": "Tình yêu ngọt ngào giữa nam sinh bừa bộn và nàng thiên sứ phòng bên.",
    "chapters": "25 Chương",
    "words": "26.900 Chữ Hán",
    "vocab": "488 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "mamahaha_tsurego": {
    "id": "mamahaha_tsurego",
    "title": "継母の連れ子が元カノだった",
    "romaji": "My Stepmom's Daughter Is My Ex",
    "author": "紙城境介",
    "level": "N4",
    "genre": "School Romcom",
    "cover": "assets/covers/mamahaha_no_tsurego_ga_motokano_datta.jpg",
    "synopsis": "Cặp đôi vừa chia tay bất ngờ trở thành chị em một nhà.",
    "chapters": "25 Chương",
    "words": "28.600 Chữ Hán",
    "vocab": "512 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kurasu_nibanme": {
    "id": "kurasu_nibanme",
    "title": "クラスで２番目に可愛い女の子と友達になった",
    "romaji": "Friends with the 2nd Cutest Girl",
    "author": "たかた",
    "level": "N4",
    "genre": "School Romance",
    "cover": "assets/covers/kurasu_de_nibanme_ni_kawaii.jpg",
    "synopsis": "Tình bạn bình yên sau giờ học mỗi thứ Sáu cùng cô bạn dễ thương.",
    "chapters": "25 Chương",
    "words": "26.900 Chữ Hán",
    "vocab": "488 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "death_march": {
    "id": "death_march",
    "title": "デスマーチからはじまる異世界狂想曲",
    "romaji": "Death March",
    "author": "愛七ひろ",
    "level": "N4",
    "genre": "Isekai Fantasy",
    "cover": "assets/covers/death_march_kara_hajimaru.jpg",
    "synopsis": "Lập trình viên nhận sức mạnh tối cao và bắt đầu du lịch ngắm cảnh.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "konosuba": {
    "id": "konosuba",
    "title": "この素晴らしい世界に祝福を！",
    "romaji": "KonoSuba",
    "author": "暁なつめ",
    "level": "N4",
    "genre": "Comedy Isekai",
    "cover": "assets/covers/konosuba.jpg",
    "synopsis": "Kazuma cùng bộ ba nữ thần vô dụng, pháp sư cuồng nổ và hiệp sĩ bựa.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "mofumofu_kaitaku": {
    "id": "mofumofu_kaitaku",
    "title": "もふもふと楽しむ無人島開拓ライフ",
    "romaji": "Island Life with Fluffy Friends",
    "author": "長尾隆生",
    "level": "N4",
    "genre": "Slow Life",
    "cover": "assets/covers/mofumofu_to_tanoshimu_mujintou_nonbiri_kaitaku_life.jpg",
    "synopsis": "Xây dựng hòn đảo thiên đường cùng bầy thú lông xù đáng yêu.",
    "chapters": "25 Chương",
    "words": "28.600 Chữ Hán",
    "vocab": "512 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "shokubutsu_mahou": {
    "id": "shokubutsu_mahou",
    "title": "植物魔法チートでのんびり領主生活始めます",
    "romaji": "Plant Magic Cheat Territory Life",
    "author": "りょふ",
    "level": "N4",
    "genre": "Territory Building",
    "cover": "assets/covers/shokubutsu_mahou_cheat_de_nonbiri_ryoushu_seikatsu_hajimemasu.jpg",
    "synopsis": "Phát triển lãnh địa khô cằn thành vườn hoa trái bốn mùa xum xuê.",
    "chapters": "25 Chương",
    "words": "28.600 Chữ Hán",
    "vocab": "512 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "houkago_pleiades": {
    "id": "houkago_pleiades",
    "title": "放課後の図書室で君と出会った",
    "romaji": "Meeting You in the Library",
    "author": "青木祐介",
    "level": "N4",
    "genre": "School Youth",
    "cover": "assets/covers/houkago.jpg",
    "synopsis": "Góc thư viện chiều tà chia sẻ niềm say mê từng trang sách.",
    "chapters": "25 Chương",
    "words": "28.600 Chữ Hán",
    "vocab": "512 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kawaii_otonari": {
    "id": "kawaii_otonari",
    "title": "可愛いお隣さんと節約してみませんか？",
    "romaji": "Saving Money with My Cute Neighbor",
    "author": "市川夕",
    "level": "N4",
    "genre": "Daily Romcom",
    "cover": "assets/covers/kawaii_otonari-san_to_setsuyaku_shite_mimasen_ka.jpg",
    "synopsis": "Bí kíp nấu ăn tiết kiệm sinh viên cùng cô bạn dễ thương cùng tầng.",
    "chapters": "25 Chương",
    "words": "26.900 Chữ Hán",
    "vocab": "488 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kurasu_gimai": {
    "id": "kurasu_gimai",
    "title": "クラスで一番可愛い子が、義妹になりまして",
    "romaji": "Cutest Girl in Class Became My Stepsister",
    "author": "三河ごーすと",
    "level": "N4",
    "genre": "School Romcom",
    "cover": "assets/covers/kurasu_de_ichiban_kawaii_ko_ga,_gimai_ni_narimashite.jpg",
    "synopsis": "Hoa khôi xinh đẹp nhất trường bỗng chuyển về ở cùng một mái nhà.",
    "chapters": "25 Chương",
    "words": "25.200 Chữ Hán",
    "vocab": "464 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "sekaiichi_osananajimi": {
    "id": "sekaiichi_osananajimi",
    "title": "世界一可愛い俺の幼馴染が今日も可愛い",
    "romaji": "My World Cutest Childhood Friend",
    "author": "あきさかあさひ",
    "level": "N4",
    "genre": "Sweet Romcom",
    "cover": "assets/covers/sekaiichi_kawaii_ore_no_osananajimi_ga_kyou_mo_kawaii.jpg",
    "synopsis": "Những mẩu chuyện ngọt ngào hàng ngày cùng cô bạn thanh mai trúc mã.",
    "chapters": "25 Chương",
    "words": "32.850 Chữ Hán",
    "vocab": "572 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "senpai_imouto": {
    "id": "senpai_imouto",
    "title": "先輩の妹が俺にだけ懐いている件",
    "romaji": "Senior's Sister Likes Only Me",
    "author": "葵せきな",
    "level": "N4",
    "genre": "School Romcom",
    "cover": "assets/covers/senpai_no_imouto.jpg",
    "synopsis": "Em gái nhỏ của đàn anh câu lạc bộ chỉ thích bám dính lấy mình.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kaji_daikou": {
    "id": "kaji_daikou",
    "title": "家事代行のアルバイトを始めたら学園一の美少女の家だった",
    "romaji": "Housekeeping at the School Idol House",
    "author": "葉月文",
    "level": "N4",
    "genre": "School Romance",
    "cover": "assets/covers/kaji_daikou_no_arubaitu_wo_hajimetara_gakuen_ichi_no_bishoujo.jpg",
    "synopsis": "Đi làm thêm dọn dẹp nhà cửa nhận ra chủ nhà là mỹ nữ số một trường.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "ore_no_ie_megami": {
    "id": "ore_no_ie_megami",
    "title": "俺の家に何故か学園の女神様が居候している件",
    "romaji": "Goddess of School Stays at My Home",
    "author": "水野歩",
    "level": "N4",
    "genre": "Sweet Romcom",
    "cover": "assets/covers/ore_no_ie_ni_naze_ka_gakuen_no_megami-sama.jpg",
    "synopsis": "Nữ thần học đường lạnh lùng chuyển đến ở trọ tại nhà nam sinh bình thường.",
    "chapters": "25 Chương",
    "words": "28.600 Chữ Hán",
    "vocab": "512 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "hizamakura": {
    "id": "hizamakura",
    "title": "膝枕してくれる後輩女子との甘々放課後",
    "romaji": "Sweet After School with My Junior",
    "author": "七菜なな",
    "level": "N4",
    "genre": "Relax Romance",
    "cover": "assets/covers/hizamakura_romcom.jpg",
    "synopsis": "Những buổi chiều yên ả nằm gối đầu lên đùi cô hậu bối dịu dàng.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "danjo_hi": {
    "id": "danjo_hi",
    "title": "男女比１対５の学園で始まる日常",
    "romaji": "School with 1 to 5 Gender Ratio",
    "author": "涼暮皐",
    "level": "N4",
    "genre": "School Comedy",
    "cover": "assets/covers/danjo-hi.jpg",
    "synopsis": "Ngôi trường nữ sinh vừa mở cửa nhận nam sinh và những bỡ ngỡ ban đầu.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "classmate_mamatomo": {
    "id": "classmate_mamatomo",
    "title": "同級生がママ友になった件",
    "romaji": "Classmate Became a Mom Friend",
    "author": "柑橘ゆすら",
    "level": "N4",
    "genre": "Daily Life",
    "cover": "assets/covers/classmate_ga_mama-tomo_ni_natta.jpg",
    "synopsis": "Gặp lại cô bạn cùng lớp năm xưa khi dắt cháu đi dạo công viên.",
    "chapters": "25 Chương",
    "words": "30.300 Chữ Hán",
    "vocab": "536 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "arudo_tensei": {
    "id": "arudo_tensei",
    "title": "アルドの異世界ほのぼの転生記",
    "romaji": "Aldo Isekai Warm Life",
    "author": "錬金王",
    "level": "N4",
    "genre": "Slow Life",
    "cover": "assets/covers/arudo_no_isekai_tensei.jpg",
    "synopsis": "Chuyển sinh thành cậu bé Aldo tận hưởng cuộc sống êm đềm ở làng quê dị giới.",
    "chapters": "25 Chương",
    "words": "25.200 Chữ Hán",
    "vocab": "464 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "guild_tsuihou": {
    "id": "guild_tsuihou",
    "title": "ギルドを追放された雑用係の下剋上",
    "romaji": "Banished Errand Boy Overcoming Fate",
    "author": "門司柿家",
    "level": "N4",
    "genre": "Adventure Growth",
    "cover": "assets/covers/guild_tsuihou_sareta_zatsuyougakari_no_gekokujou.jpg",
    "synopsis": "Chàng trai tạp vụ bị khai trừ tự lập tổ đội mới và tỏa sáng tài năng.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "tensei_kizoku": {
    "id": "tensei_kizoku",
    "title": "転生貴族の異世界冒険録",
    "romaji": "Chronicles of an Aristocrat in Another World",
    "author": "夜州",
    "level": "N4",
    "genre": "Fantasy Adventure",
    "cover": "assets/covers/tensei_kizoku_no_isekai_boukenroku.jpg",
    "synopsis": "Cậu bé quý tộc Cain được các vị thần ban tặng muôn vàn ân huệ.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "hassai_shito": {
    "id": "hassai_shito",
    "title": "八歳から始まる神々の使徒の転生生活",
    "romaji": "Apostle of the Gods from Age 8",
    "author": "えぬ",
    "level": "N4",
    "genre": "Fantasy Growth",
    "cover": "assets/covers/hassai_kara_hajimaru_kamigami_no_shito_no_tensei_seikatsu.jpg",
    "synopsis": "Sứ giả của thần thức tỉnh năm tám tuổi bảo vệ bình yên làng quê.",
    "chapters": "25 Chương",
    "words": "25.200 Chữ Hán",
    "vocab": "464 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "isekai_craft": {
    "id": "isekai_craft",
    "title": "異世界クラフト暮らし～魔王城の前で工務店始めます～",
    "romaji": "Isekai Crafting Shop",
    "author": "木ノ野まこと",
    "level": "N4",
    "genre": "Crafting Comedy",
    "cover": "assets/covers/isekai_craft_gurashi.jpg",
    "synopsis": "Mở tiệm mộc xây dựng công trình tiện nghi ngay trước cổng lâu đài ma vương.",
    "chapters": "25 Chương",
    "words": "25.200 Chữ Hán",
    "vocab": "464 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "meshi_buff": {
    "id": "meshi_buff",
    "title": "飯バフ食堂～まごころ込めた料理でみんなを強化～",
    "romaji": "Meal Buff Restaurant",
    "author": "七沢また",
    "level": "N4",
    "genre": "Gourmet Fantasy",
    "cover": "assets/covers/meshi_buff_shokudou.jpg",
    "synopsis": "Mỗi món ăn nấu ra đều tăng cường sức mạnh và chữa lành cho mạo hiểm giả.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "tenmou_kaikai": {
    "id": "tenmou_kaikai",
    "title": "天網恢恢アルケミー",
    "romaji": "Alchemist Everyday",
    "author": "風見鶏",
    "level": "N4",
    "genre": "Alchemy Healing",
    "cover": "assets/covers/tenmou_kaikai_alchemy.jpg",
    "synopsis": "Nữ dược sư pha chế độc dược và thuốc trị thương thần kỳ trong hẻm nhỏ.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "daidokoro_esc": {
    "id": "daidokoro_esc",
    "title": "台所エスケープ～今日もお腹いっぱい～",
    "romaji": "Kitchen Escape",
    "author": "谷崎泉",
    "level": "N4",
    "genre": "Cooking Daily",
    "cover": "assets/covers/daidokoro_escape.png",
    "synopsis": "Góc bếp ấm cúng thắp lửa sau những giờ học căng thẳng mệt nhoài.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kounai_radio": {
    "id": "kounai_radio",
    "title": "校内ラジオで恋愛相談始めました",
    "romaji": "Campus Radio Love Advice",
    "author": "春川三咲",
    "level": "N4",
    "genre": "School Daily",
    "cover": "assets/covers/kounai_radio_de_renai_soudan.jpg",
    "synopsis": "Chương trình phát thanh giờ nghỉ trưa gỡ rối tơ lòng cho học sinh.",
    "chapters": "25 Chương",
    "words": "25.200 Chữ Hán",
    "vocab": "464 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "mushoku": {
    "id": "mushoku",
    "title": "無職転生　- 異世界行ったら本気だす",
    "romaji": "Mushoku Tensei",
    "author": "理不尽な孫の手",
    "level": "N3",
    "genre": "Isekai & Fantasy",
    "cover": "assets/covers/mushoku_tensei.jpg",
    "synopsis": "Đỉnh cao chuyển sinh Isekai, Rudeus quyết tâm sống hết mình tại thế giới mới.",
    "chapters": "25 Chương",
    "words": "20.950 Chữ Hán",
    "vocab": "404 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "slime": {
    "id": "slime",
    "title": "転生したらスライムだった件",
    "romaji": "That Time I Got Reincarnated as a Slime",
    "author": "伏瀬",
    "level": "N3",
    "genre": "Isekai & Fantasy",
    "cover": "assets/covers/slime.jpg",
    "synopsis": "Rimuru Tempest từ một con Slime thành lập đại liên minh ma vật Jura.",
    "chapters": "25 Chương",
    "words": "19.250 Chữ Hán",
    "vocab": "380 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "honzuki": {
    "id": "honzuki",
    "title": "本好きの下剋上",
    "romaji": "Ascendance of a Bookworm",
    "author": "香月美夜",
    "level": "N3",
    "genre": "Slice of Life & Fantasy",
    "cover": "assets/covers/moonlit_library.png",
    "synopsis": "Cô bé Myne yếu ớt sáng chế giấy và in ấn để thỏa mãn tình yêu sách.",
    "chapters": "25 Chương",
    "words": "20.950 Chữ Hán",
    "vocab": "404 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "ginga": {
    "id": "ginga",
    "title": "銀河鉄道の夜",
    "romaji": "Night on the Galactic Railroad",
    "author": "宮沢賢治",
    "level": "N3",
    "genre": "Cosmic Fantasy",
    "cover": "assets/cover_ginga.jpg",
    "synopsis": "Chuyến tàu đêm ngân hà lấp lánh tìm kiếm hạnh phúc đích thực.",
    "chapters": "25 Chương",
    "words": "19.250 Chữ Hán",
    "vocab": "380 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "merosu": {
    "id": "merosu",
    "title": "走れメロス",
    "romaji": "Run, Melos!",
    "author": "太宰治",
    "level": "N3",
    "genre": "Action & Friendship",
    "cover": "assets/covers/run_melos.png",
    "synopsis": "Chạy đua với mặt trời lặn để cứu bạn thân Selinuntius khỏi máy chém.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "spider": {
    "id": "spider",
    "title": "蜘蛛の糸",
    "romaji": "The Spider's Thread",
    "author": "芥川龍之介",
    "level": "N3",
    "genre": "Buddhist Parable",
    "cover": "assets/covers/cover_anime_sunset.jpg",
    "synopsis": "Sợi tơ nhện bạc mong manh buông xuống từ cõi Cực Lạc giải thoát ác nhân.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "chuumon": {
    "id": "chuumon",
    "title": "注文の多い料理店",
    "romaji": "The Restaurant of Many Orders",
    "author": "宮沢賢治",
    "level": "N3",
    "genre": "Dark Fantasy",
    "cover": "assets/covers/many_orders.png",
    "synopsis": "Hai quý tộc lạc vào tiệm ăn kỳ lạ giữa rừng với hàng loạt yêu cầu oái oăm.",
    "chapters": "25 Chương",
    "words": "20.950 Chữ Hán",
    "vocab": "404 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "gakuen": {
    "id": "gakuen",
    "title": "学園アイドルの彼氏役（仮）",
    "romaji": "Gakuen Idol no Kareshi-yaku (Kari)",
    "author": "Kotoba Original",
    "level": "N3",
    "genre": "School RomCom",
    "cover": "assets/covers/cover_school_romcom.jpg",
    "synopsis": "Một nam sinh bình thường bất ngờ bị cô nàng thần tượng trường học lôi vào vai bạn trai hờ, mở ra chuỗi ngày dở khóc dở cười.",
    "chapters": "25 Chương",
    "words": "24.500 Chữ Hán",
    "vocab": "480 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "toshishun": {
    "id": "toshishun",
    "title": "杜子春",
    "romaji": "Toshishun",
    "author": "芥川龍之介",
    "level": "N3",
    "genre": "Eastern Mythology",
    "cover": "assets/covers/cover_anime_night_shrine.jpg",
    "synopsis": "Hành trình nếm trải bạc bẽo trần gian và tu tiên trên núi Nga Mi.",
    "chapters": "25 Chương",
    "words": "22.650 Chữ Hán",
    "vocab": "428 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "tate_yuusha": {
    "id": "tate_yuusha",
    "title": "盾の勇者の成り上がり",
    "romaji": "The Rising of the Shield Hero",
    "author": "アネコユサギ",
    "level": "N3",
    "genre": "Action Fantasy",
    "cover": "assets/covers/tate_no_yuusha_no_nariagari.jpg",
    "synopsis": "Khiên Hiệp Sĩ Naofumi vươn lên từ đáy vực phản bội cùng Raphtalia.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "sao": {
    "id": "sao",
    "title": "ソードアート・オンライン",
    "romaji": "Sword Art Online",
    "author": "川原礫",
    "level": "N3",
    "genre": "Sci-Fi VRMMO",
    "cover": "assets/covers/sao.jpg",
    "synopsis": "Kirito và Asuna chiến đấu vượt qua 100 tầng tử thần Aincrad.",
    "chapters": "25 Chương",
    "words": "17.550 Chữ Hán",
    "vocab": "356 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "rebuild_world": {
    "id": "rebuild_world",
    "title": "リビルドワールド",
    "romaji": "Rebuild World",
    "author": "ナフセ",
    "level": "N3",
    "genre": "Sci-Fi Cyberpunk",
    "cover": "assets/covers/rebuild_world.jpg",
    "synopsis": "Thợ săn phế tích Akira cùng ảo ảnh Alpha sinh tồn nơi thế giới hậu tận thế.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "hitori_bocchi": {
    "id": "hitori_bocchi",
    "title": "ひとりぼっちの異世界攻略",
    "romaji": "Loner Life in Another World",
    "author": "五示正司",
    "level": "N3",
    "genre": "Adventure Comedy",
    "cover": "assets/covers/hitori_bocchi_no_isekai_kouryakukai.jpg",
    "synopsis": "Cậu học sinh thích cô độc nhận các kỹ năng phế và tự mình phá đảo.",
    "chapters": "25 Chương",
    "words": "26.050 Chữ Hán",
    "vocab": "476 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "chiyu_mahou": {
    "id": "chiyu_mahou",
    "title": "治癒魔法の間違った使い方",
    "romaji": "The Wrong Way to Use Healing Magic",
    "author": "くろかた",
    "level": "N3",
    "genre": "Action Training",
    "cover": "assets/covers/chiyu_mahou_wa_tsukaenai_to_tsuihou_sareta_no_ni,_naze_ka_tayoraretemasu.jpg",
    "synopsis": "Khóa huấn luyện ma pháp trị liệu thành vũ khí cận chiến bạo lực.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kage_jitsuryoku": {
    "id": "kage_jitsuryoku",
    "title": "陰の実力者になりたくて！",
    "romaji": "The Eminence in Shadow",
    "author": "逢沢大介",
    "level": "N3",
    "genre": "Action Chuunibyou",
    "cover": "assets/covers/cover_anime_castle.jpg",
    "synopsis": "Cid Kagenou đóng giả nhân vật quần chúng nhưng ngầm chỉ huy Shadow Garden.",
    "chapters": "25 Chương",
    "words": "27.750 Chữ Hán",
    "vocab": "500 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kusuriya": {
    "id": "kusuriya",
    "title": "薬屋のひとりごと",
    "romaji": "The Apothecary Diaries",
    "author": "日向夏",
    "level": "N3",
    "genre": "Mystery Historical",
    "cover": "assets/covers/the_bookstore_promise.jpg",
    "synopsis": "Dược sư Maomao phá giải những vụ án độc dược chốn hậu cung thâm hiểm.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "dungeon_gourmet": {
    "id": "dungeon_gourmet",
    "title": "推しに捧げるダンジョン飯",
    "romaji": "Dungeon Gourmet",
    "author": "九井諒子風",
    "level": "N3",
    "genre": "Dungeon Cooking",
    "cover": "assets/covers/oshi_ni_sasageru_dungeon_gourmet.jpg",
    "synopsis": "Chế biến quái vật hầm ngục thành các món lẩu và nướng thượng hạng.",
    "chapters": "25 Chương",
    "words": "27.750 Chữ Hán",
    "vocab": "500 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "gokuaku_kizoku": {
    "id": "gokuaku_kizoku",
    "title": "極悪貴族に転生したけど善政敷きます",
    "romaji": "Reincarnated as a Villain Noble",
    "author": "三木なずな",
    "level": "N3",
    "genre": "Territory Building",
    "cover": "assets/covers/gokuaku_kizoku.jpg",
    "synopsis": "Tái sinh thành quý tộc độc ác quyết tâm sửa đổi vận mệnh mang lại ấm no.",
    "chapters": "25 Chương",
    "words": "26.900 Chữ Hán",
    "vocab": "488 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "ito_tsumugu": {
    "id": "ito_tsumugu",
    "title": "糸を紡ぐ転生者",
    "romaji": "The Thread Spinner",
    "author": "天野ハザマ",
    "level": "N3",
    "genre": "Fantasy Magic",
    "cover": "assets/covers/ito_wo_tsumugu_tenseisha.jpg",
    "synopsis": "Dùng ma pháp tơ chỉ kết nối vận mệnh và dệt nên kết giới bảo hộ quốc gia.",
    "chapters": "25 Chương",
    "words": "24.350 Chữ Hán",
    "vocab": "452 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "rezero": {
    "id": "rezero",
    "title": "Ｒｅ：ゼロから始める異世界生活",
    "romaji": "Re:Zero",
    "author": "長月達平",
    "level": "N2",
    "genre": "Dark Fantasy & Psychology",
    "cover": "assets/covers/rezero.jpg",
    "synopsis": "Subaru chịu ngàn cái chết đớn đau để cứu những người thân yêu.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kokoro": {
    "id": "kokoro",
    "title": "こころ",
    "romaji": "Kokoro (Heart)",
    "author": "夏目漱石",
    "level": "N2",
    "genre": "Psychological Masterpiece",
    "cover": "assets/cover_kokoro.jpg",
    "synopsis": "Kiệt tác tâm lý sâu sắc về nỗi cô đơn và bí mật tội lỗi của Tiên sinh.",
    "chapters": "25 Chương",
    "words": "20.100 Chữ Hán",
    "vocab": "392 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "yukiguni": {
    "id": "yukiguni",
    "title": "雪国",
    "romaji": "Snow Country",
    "author": "川端康成",
    "level": "N2",
    "genre": "Nobel Literature",
    "cover": "assets/covers/cover_anime_forest.jpg",
    "synopsis": "Văn hào Nobel 1968: \"Vượt qua đường hầm biên giới dài, đã là xứ tuyết.\"",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "rashomon": {
    "id": "rashomon",
    "title": "羅生門",
    "romaji": "Rashomon Gate",
    "author": "芥川龍之介",
    "level": "N2",
    "genre": "Dark Classic",
    "cover": "assets/covers/rashomon.png",
    "synopsis": "Dưới cổng thành hoang tàn trong mưa chiều, ranh giới giữa chết đói và làm cướp.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "overlord": {
    "id": "overlord",
    "title": "オーバーロード",
    "romaji": "Overlord",
    "author": "丸山くがね",
    "level": "N2",
    "genre": "Dark Fantasy",
    "cover": "assets/covers/overlord.jpg",
    "synopsis": "Ainz Ooal Gown cùng đại lăng tẩm Nazarick bắt đầu kế hoạch chinh phục dị giới.",
    "chapters": "25 Chương",
    "words": "21.800 Chữ Hán",
    "vocab": "416 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "maihime": {
    "id": "maihime",
    "title": "舞姫",
    "romaji": "The Dancing Girl",
    "author": "森鴎外",
    "level": "N2",
    "genre": "Meiji Tragedy",
    "cover": "assets/covers/cover_anime_sunset.jpg",
    "synopsis": "Bi kịch chàng du học sinh Berlin giữa nghĩa vụ danh vọng và ái tình.",
    "chapters": "25 Chương",
    "words": "20.950 Chữ Hán",
    "vocab": "404 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "bullbuster": {
    "id": "bullbuster",
    "title": "ブルバスター～中小企業のロボット開発～",
    "romaji": "Bullbuster",
    "author": "中尾浩之",
    "level": "N2",
    "genre": "Real Robot Drama",
    "cover": "assets/covers/bullbuster.jpg",
    "synopsis": "Doanh nghiệp vừa và nhỏ đối mặt bài toán tài chính khi chế tạo robot diệt quái.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "wagahai": {
    "id": "wagahai",
    "title": "吾輩は猫である",
    "romaji": "I Am a Cat",
    "author": "夏目漱石",
    "level": "N1",
    "genre": "Classic Satire",
    "cover": "assets/covers/the_bookstore_promise.jpg",
    "synopsis": "Góc nhìn trào phúng, thâm thúy của chú mèo vô danh quan sát giới trí thức.",
    "chapters": "25 Chương",
    "words": "20.950 Chữ Hán",
    "vocab": "404 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "sangetsuki": {
    "id": "sangetsuki",
    "title": "山月記",
    "romaji": "The Moon Over the Mountain",
    "author": "中島敦",
    "level": "N1",
    "genre": "Philosophical Allegory",
    "cover": "assets/covers/whisper_of_sakura.jpg",
    "synopsis": "Bi kịch của thi sĩ Lý Trưng vì kiêu ngạo tự ti mà hóa thành cọp gầm dưới trăng.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  },
  "kusamakura": {
    "id": "kusamakura",
    "title": "草枕",
    "romaji": "The Three-Cornered World",
    "author": "夏目漱石",
    "level": "N1",
    "genre": "Aesthetic Literature",
    "cover": "assets/covers/moonlit_library.png",
    "synopsis": "Bước lên con đường núi tìm kiếm cái đẹp phi nhân gian thoát khỏi trần tục xô bồ.",
    "chapters": "25 Chương",
    "words": "23.500 Chữ Hán",
    "vocab": "440 Từ JLPT",
    "progress": "Chưa đọc"
  }
};

// Initial SRS Flashcards SM-2 Deck
let srsDeck = [
  {
    kanji: '銀河',
    romaji: 'ginga',
    reading: 'ぎんが',
    meaning: 'Dải Ngân Hà (Milky Way Galaxy)',
    level: 'N2',
    sentence: '白くけむった銀河の帯のようなところを指しながら...',
    sentenceVn: 'Vừa chỉ vào dải mờ trắng như dải Ngân Hà...'
  },
  {
    kanji: '星座',
    romaji: 'seiza',
    reading: 'せいざ',
    meaning: 'Chòm sao (Constellation)',
    level: 'N2',
    sentence: '黒板に吊した大きな黒い星座の図...',
    sentenceVn: 'Bản đồ chòm sao lớn màu đen treo trên bảng...'
  },
  {
    kanji: '黒板',
    romaji: 'kokuban',
    reading: 'こくばん',
    meaning: 'Bảng đen (Blackboard)',
    level: 'N5',
    sentence: '先生は、黒板に吊した図を指しました。',
    sentenceVn: 'Thầy giáo chỉ vào hình vẽ treo trên bảng đen.'
  },
  {
    kanji: '承知',
    romaji: 'shouchi',
    reading: 'しょうち',
    meaning: 'Hiểu biết, nhận thức (Consent / Knowing)',
    level: 'N3',
    sentence: '本当は何かご承知ですか。',
    sentenceVn: 'Thực ra các em có biết nó là cái gì không?'
  }
];

// Visual Novel Registry: Story & Scene Definitions
const vnStoryRegistry = {
  "ginga": {
    "id": "ginga",
    "title": "銀河鉄道の夜",
    "vnTitle": "Chuyến tàu Ngân Hà",
    "author": "Miyazawa Kenji (宮沢賢治)",
    "theme": "theme-ginga",
    "bgAmbient": "rgba(30, 58, 138, 0.25)",
    "scenes": [
      {
        "id": "station",
        "name": "Cảnh 1: Bờ cát sông sao Thiên Nga",
        "startIdx": 0,
        "bg": "assets/bg_shinkai_ginga.jpg"
      },
      {
        "id": "carriage",
        "name": "Cảnh 2: Bên trong khoang tàu đêm",
        "startIdx": 4,
        "bg": "assets/bg_shinkai_carriage.jpg"
      }
    ]
  },
  "merosu": {
    "id": "merosu",
    "title": "走れメロス",
    "vnTitle": "Chạy đi Melos",
    "author": "Dazai Osamu (太宰治)",
    "theme": "theme-merosu",
    "bgAmbient": "rgba(180, 83, 9, 0.25)",
    "scenes": [
      {
        "id": "throne",
        "name": "Cảnh 1: Ngai vàng bạo chúa Syracuse",
        "startIdx": 0,
        "bg": "assets/covers/cover_anime_castle.jpg"
      },
      {
        "id": "road",
        "name": "Cảnh 2: Sa trường hoàng hôn rực lửa",
        "startIdx": 4,
        "bg": "assets/covers/cover_anime_sunset.jpg"
      }
    ]
  },
  "gon": {
    "id": "gon",
    "title": "ごんぎつね",
    "vnTitle": "Cáo nhỏ Gon",
    "author": "Niimi Nankichi (新美南吉)",
    "theme": "theme-gon",
    "bgAmbient": "rgba(16, 185, 129, 0.22)",
    "scenes": [
      {
        "id": "stream",
        "name": "Cảnh 1: Bờ suối làng quê giăng lưới",
        "startIdx": 0,
        "bg": "assets/covers/cover_anime_forest.jpg"
      },
      {
        "id": "cottage",
        "name": "Cảnh 2: Thềm nhà tranh mùa thu êm đềm",
        "startIdx": 3,
        "bg": "assets/covers/cover_anime_night_shrine.jpg"
      }
    ]
  },
  "rashomon": {
    "id": "rashomon",
    "title": "羅生門",
    "vnTitle": "La Sinh Môn",
    "author": "Akutagawa Ryunosuke (芥川龍之介)",
    "theme": "theme-rashomon",
    "bgAmbient": "rgba(71, 85, 105, 0.32)",
    "scenes": [
      {
        "id": "gate",
        "name": "Cảnh 1: Dưới cổng thành Kyoto chiều mưa",
        "startIdx": 0,
        "bg": "assets/bg_shinkai_rashomon.jpg"
      },
      {
        "id": "attic",
        "name": "Cảnh 2: Tầng gác mái le lói ánh đuốc",
        "startIdx": 3,
        "bg": "assets/rashomon.png"
      }
    ]
  },
  "gakuen": {
    "id": "gakuen",
    "title": "学園アイドルの彼氏役",
    "vnTitle": "Được Gakuen Idol Nhận Làm Bạn Trai",
    "author": "佐々木 (Sasaki)",
    "theme": "theme-gakuen",
    "bgAmbient": "rgba(236, 72, 153, 0.22)",
    "scenes": [
      {
        "id": "hallway",
        "name": "Cảnh 1: Hành lang trước tủ giày học viện",
        "startIdx": 0,
        "bg": "assets/covers/cover_school_romcom.jpg"
      },
      {
        "id": "rooftop",
        "name": "Cảnh 2: Góc sân thượng bí mật hoàng hôn",
        "startIdx": 3,
        "bg": "assets/covers/cover_anime_sunset.jpg"
      }
    ]
  },
  "chuumon": {
    "id": "chuumon",
    "title": "注文の多い料理店",
    "vnTitle": "Quán Ăn Nhiều Yêu Cầu",
    "author": "Miyazawa Kenji (宮沢賢治)",
    "theme": "theme-chuumon",
    "bgAmbient": "rgba(202, 138, 4, 0.25)",
    "scenes": [
      {
        "id": "forest",
        "name": "Cảnh 1: Cửa hiệu Sơn Miêu Hiên giữa rừng",
        "startIdx": 0,
        "bg": "assets/covers/cover_isekai_gourmet.jpg"
      },
      {
        "id": "dining",
        "name": "Cảnh 2: Căn phòng khách quý tộc sang trọng",
        "startIdx": 2,
        "bg": "assets/covers/cover_anime_cafe.jpg"
      }
    ]
  }
};

// Visual Novel Dialogues: Curated Multi-Scene Dialogues
const vnDialoguesByBook = {
  "ginga": [
    {
      "speaker": "ジョバンニ (Giovanni)",
      "role": "✦ 星めぐりの旅人 • Hành khách chuyến tàu Ngân Hà",
      "charId": "giovanni",
      "type": "dialogue",
      "avatar": "assets/char_giovanni.png",
      "leftSprite": "assets/char_giovanni.png",
      "centerSprite": null,
      "rightSprite": "assets/char_campanella.png",
      "speakerPos": "left",
      "bg": "assets/bg_shinkai_ginga.jpg",
      "location": "🌸 CHUYẾN TÀU NGÂN HÀ • GA SỐ 4: BỜ BÃI THIÊN NGA & DÒNG SÔNG SAO",
      "theme": "theme-ginga",
      "jp": "「カムパネルラ、見てごらん！ あの川の向こうの光、白くけむった銀河の帯だよ」",
      "vn": "\"Campanella, cậu nhìn kìa! Vệt sáng bên kia dòng sông kìa, chính là dải Ngân Hà mờ trắng đấy.\"",
      "grammarInsight": "Cấu trúc [Động từ thể て + ごらん]: Mẫu câu rủ rê, khuyên nhủ một cách ân cần, trìu mến giữa bạn bè đồng trang lứa hoặc người lớn với trẻ nhỏ (Xem kìa!).",
      "nuanceInsight": "Khắc họa sự ngỡ ngàng tột bậc xen lẫn niềm vui thuần khiết của cậu bé Giovanni khi lần đầu chạm mắt vào dòng sông sao vũ trụ.",
      "vocabTokens": [
        {
          "word": "見てごらん",
          "reading": "みてごらん",
          "meaning": "Hãy nhìn xem này (lời rủ rê trìu mến)",
          "level": "N3"
        },
        {
          "word": "向こう",
          "reading": "むこう",
          "meaning": "Phía bên kia, hướng đối diện",
          "level": "N4"
        },
        {
          "word": "銀河",
          "reading": "ぎんが",
          "meaning": "Dải Ngân Hà, sông sao trời",
          "level": "N2"
        },
        {
          "word": "帯",
          "reading": "おび",
          "meaning": "Dải, vệt dài trải ngang",
          "level": "N3"
        }
      ]
    },
    {
      "speaker": "カムパネルラ (Campanella)",
      "role": "✦ 親友 • Người bạn đồng hành định mệnh",
      "charId": "campanella",
      "type": "dialogue",
      "avatar": "assets/char_campanella.png",
      "leftSprite": "assets/char_giovanni.png",
      "centerSprite": null,
      "rightSprite": "assets/char_campanella.png",
      "speakerPos": "right",
      "bg": "assets/bg_shinkai_ginga.jpg",
      "location": "🌸 CHUYẾN TÀU NGÂN HÀ • GA SỐ 4: BỜ BÃI THIÊN NGA & DÒNG SÔNG SAO",
      "theme": "theme-ginga",
      "jp": "「本当だね、ジョバンニ。先生が午後の授業で黒板に吊した図とまったく同じだ」",
      "vn": "\"Đúng thật rồi, Giovanni. Y hệt như bản đồ chòm sao thầy giáo treo trên bảng đen trong giờ học ban chiều.\"",
      "grammarInsight": "Cấu trúc [A とまったく同じ]: Hoàn toàn y hệt, giống hệt như A. Phó từ まったく nhấn mạnh tính tuyệt đối không một chút sai lệch.",
      "nuanceInsight": "Campanella có phong thái điềm đạm, trí thức và ấm áp, kết nối trải nghiệm kỳ ảo trên trời sao với lớp học đời thực ở trần thế.",
      "vocabTokens": [
        {
          "word": "本当",
          "reading": "ほんとう",
          "meaning": "Thực sự, quả thật",
          "level": "N5"
        },
        {
          "word": "黒板",
          "reading": "こくばん",
          "meaning": "Bảng đen lớp học",
          "level": "N4"
        },
        {
          "word": "吊した",
          "reading": "つるした",
          "meaning": "Đã treo lơ lửng lên",
          "level": "N2"
        },
        {
          "word": "まったく同じ",
          "reading": "まったくおなじ",
          "meaning": "Hoàn toàn giống hệt",
          "level": "N3"
        }
      ]
    },
    {
      "speaker": "ジョバンニ (Giovanni)",
      "role": "💭 心の声 • Độc thoại nội tâm của Giovanni",
      "charId": "giovanni",
      "type": "monologue",
      "avatar": "assets/char_giovanni.png",
      "leftSprite": "assets/char_giovanni.png",
      "centerSprite": null,
      "rightSprite": "assets/char_campanella.png",
      "speakerPos": "left",
      "bg": "assets/bg_shinkai_ginga.jpg",
      "location": "🌌 DÒNG SÔNG THIÊN NGA • KÝ ỨC VÀ ÁNH SÁNG KỲ ẢO",
      "theme": "theme-ginga",
      "jp": "（僕たちはいつの間に、この銀河ステーションにやってきたのだろう。胸の奥が不思議な安らぎで満たされていく……）",
      "vn": "(Chúng mình đã bước lên ga tàu Ngân Hà này từ lúc nào vậy nhỉ. Trong sâu thẳm lồng ngực mình đang ngập tràn một nỗi an yên kỳ lạ...)",
      "grammarInsight": "Cấu trúc [いつの間に]: Tự bao giờ, từ lúc nào mà không hề hay biết. Biểu thị sự chuyển đổi không gian vô thức đặc trưng của văn phong hư ảo Miyazawa Kenji.",
      "nuanceInsight": "Độc thoại nội tâm thể hiện tâm trạng bàng hoàng thoát tục của Giovanni khi rời xa những vất vả thiếu thốn trần thế để bước vào thế giới sao.",
      "vocabTokens": [
        {
          "word": "いつの間に",
          "reading": "いつのまに",
          "meaning": "Từ lúc nào không hay",
          "level": "N3"
        },
        {
          "word": "胸の奥",
          "reading": "むねのおく",
          "meaning": "Tận đáy lòng, sâu trong tim",
          "level": "N3"
        },
        {
          "word": "安らぎ",
          "reading": "やすらぎ",
          "meaning": "Sự bình yên, an tĩnh nội tâm",
          "level": "N2"
        },
        {
          "word": "満たされる",
          "reading": "みたされる",
          "meaning": "Được lấp đầy, ngập tràn",
          "level": "N2"
        }
      ]
    },
    {
      "speaker": "ジョバンニ (Giovanni)",
      "role": "✦ Lựa chọn hành động của bạn",
      "charId": "giovanni",
      "type": "choice",
      "avatar": "assets/char_giovanni.png",
      "leftSprite": "assets/char_giovanni.png",
      "centerSprite": null,
      "rightSprite": "assets/char_campanella.png",
      "speakerPos": "left",
      "bg": "assets/bg_shinkai_ginga.jpg",
      "location": "🌸 NGÃ RẼ TRÊN BỜ SÔNG SAO • LỰA CHỌN CỦA GIOVANNI",
      "theme": "theme-ginga",
      "jp": "「ねえ、カムパネルラ。僕たち、これからどうしようか？」",
      "vn": "\"Này Campanella. Bọn mình giờ nên làm gì tiếp theo đây?\"",
      "choices": [
        {
          "choiceId": "ginga_c1",
          "branchType": "Branching Scene",
          "textJp": "「あの川原の青い燐光を確かめに行こう！」",
          "textVn": "Xuống bãi cát sông sao tìm hiểu luồng lân quang biếc (+15 XP)",
          "reactionText": "Giovanni và Campanella hào hứng chạy xuống bờ cát pha lê lấp lánh!",
          "xp": 15,
          "branchResponse": {
            "speaker": "ジョバンニ (Giovanni)",
            "role": "✦ Người bạn đường dũng cảm",
            "charId": "giovanni",
            "avatar": "assets/char_giovanni.png",
            "leftSprite": "assets/char_giovanni.png",
            "centerSprite": null,
            "rightSprite": "assets/char_campanella.png",
            "speakerPos": "left",
            "bg": "assets/bg_shinkai_ginga.jpg",
            "location": "🌌 BỜ SÔNG SAO • BÃI CÁT LÂN QUANG",
            "theme": "theme-ginga",
            "jp": "「ああ、どこまでも一緒に行こう！ 僕はみんなの本当の幸いのためなら、この体だって惜しくないよ」",
            "vn": "\"Phải rồi, chúng mình hãy cùng nhau đi tới muôn nơi nhé! Nếu vì hạnh phúc thực sự của muôn người, tớ chẳng tiếc thân mình đâu.\"",
            "grammarInsight": "Cấu trúc [〜のためなら]: Hễ là vì mục đích X thì sẵn sàng làm Y.",
            "nuanceInsight": "Ý niệm cao cả và đức hy sinh vô điều kiện vì hạnh phúc nhân gian của Giovanni.",
            "vocabTokens": [
              { "word": "幸い", "reading": "さいわい", "meaning": "Hạnh phúc đích thực", "level": "N3" },
              { "word": "惜しくない", "reading": "おしくない", "meaning": "Không hề tiếc nuối", "level": "N2" }
            ]
          }
        },
        {
          "choiceId": "ginga_c2",
          "branchType": "Branching Scene",
          "textJp": "「先生が話していた白鳥の停車場を探してみよう」",
          "textVn": "Đi tìm nhà ga Thiên Nga mà thầy giáo từng nhắc tới (+15 XP)",
          "reactionText": "Hai cậu bé lắng nghe tiếng chuông bạc ngân vang từ ngọn tháp thiên hà!",
          "xp": 15,
          "branchResponse": {
            "speaker": "カムパネルラ (Campanella)",
            "role": "✦ Tri kỷ ấm áp",
            "charId": "campanella",
            "avatar": "assets/char_campanella.png",
            "leftSprite": "assets/char_giovanni.png",
            "centerSprite": null,
            "rightSprite": "assets/char_campanella.png",
            "speakerPos": "right",
            "bg": "assets/bg_shinkai_ginga.jpg",
            "location": "🌌 TRẠM DỪNG BẠCH DƯƠNG • NGỌN THÁP THIÊN HÀ",
            "theme": "theme-ginga",
            "jp": "「見上げてごらん、ジョバンニ。あの白鳥の停車場の標識が、星の光に照らされて光っているよ」",
            "vn": "\"Cậu ngước nhìn lên kìa, Giovanni. Biển báo trạm dừng Thiên Nga đang tỏa sáng lấp lánh dưới ánh sao trời đấy.\"",
            "grammarInsight": "Cấu trúc [〜てごらん]: Lời rủ rê trìu mến giữa những người bạn bè thân thiết.",
            "nuanceInsight": "Sự hướng dẫn dịu dàng và ánh mắt sáng ngời của Campanella bên bờ sông Ngân Hà.",
            "vocabTokens": [
              { "word": "見上げる", "reading": "みあげる", "meaning": "Ngước nhìn lên cao", "level": "N3" },
              { "word": "標識", "reading": "ひょうしき", "meaning": "Biển chỉ dẫn, cột mốc", "level": "N2" }
            ]
          }
        }
      ],
      "grammarInsight": "Cấu trúc [〜ようか / どうしようか]: Đưa ra lời đề nghị hoặc tự hỏi xem bản thân nên làm thế nào trong ngữ cảnh thân mật.",
      "nuanceInsight": "Thời khắc tình bạn gắn kết sâu sắc trước khi chuyến tàu bắt đầu lao vút qua các chòm sao phía Nam.",
      "vocabTokens": [
        {
          "word": "川原",
          "reading": "かわら",
          "meaning": "Bãi cát ven sông",
          "level": "N3"
        },
        {
          "word": "燐光",
          "reading": "りんこう",
          "meaning": "Ánh lân tinh lấp lánh trong đêm",
          "level": "N1"
        },
        {
          "word": "確かめる",
          "reading": "たしかめる",
          "meaning": "Xác minh, làm cho rõ ràng",
          "level": "N3"
        }
      ]
    },
    {
      "speaker": "ジョバンニ (Giovanni)",
      "role": "✦ 星めぐりの旅人 • Lời nguyện ước chân thành",
      "charId": "giovanni",
      "type": "dialogue",
      "avatar": "assets/char_giovanni.png",
      "leftSprite": "assets/char_giovanni.png",
      "centerSprite": null,
      "rightSprite": "assets/char_campanella.png",
      "speakerPos": "left",
      "bg": "assets/bg_shinkai_carriage.jpg",
      "location": "🚂 BÊN TRONG KHOANG TÀU ĐÊM • ÁNH ĐÈN DẦU ẤM ÁP",
      "theme": "theme-ginga",
      "jp": "「ああ、どこまでも一緒に行こう！ 僕はみんなの本当の幸いのためなら、この体だって惜しくないよ」",
      "vn": "\"Phải rồi, chúng mình hãy cùng nhau đi tới muôn nơi nhé! Nếu là vì hạnh phúc đích thực của mọi người, tớ chẳng tiếc thân mình đâu.\"",
      "grammarInsight": "Cấu trúc [〜のためなら、〜だって惜しくない]: Nếu là vì mục đích cao cả X thì ngay cả bản thân Y cũng không mảy may tiếc nuối. [惜しい] = tiếc nuối.",
      "nuanceInsight": "Tuyên ngôn nhân văn cốt lõi của tác phẩm: \"Hạnh phúc đích thực (本当の幸い)\" chỉ có được khi con người biết hy sinh và chia sẻ cùng tha nhân.",
      "vocabTokens": [
        {
          "word": "どこまでも",
          "reading": "どこまでも",
          "meaning": "Tới bất cứ phương trời nào, muôn nơi",
          "level": "N3"
        },
        {
          "word": "幸い",
          "reading": "さいわい",
          "meaning": "Hạnh phúc, niềm phúc lành",
          "level": "N3"
        },
        {
          "word": "惜しくない",
          "reading": "おしくない",
          "meaning": "Không màng tiếc nuối",
          "level": "N2"
        }
      ]
    },
    {
      "speaker": "車掌 (Người soát vé)",
      "role": "🎫 銀河鉄道の車掌 • Người soát vé tàu đêm bí ẩn",
      "charId": "conductor",
      "type": "dialogue",
      "avatar": "assets/char_conductor.png",
      "leftSprite": "assets/char_giovanni.png",
      "centerSprite": "assets/char_conductor.png",
      "rightSprite": "assets/char_campanella.png",
      "speakerPos": "center",
      "bg": "assets/bg_shinkai_carriage.jpg",
      "location": "🚂 KHOANG TÀU ĐÊM • TIẾNG CHUÔNG SOÁT VÉ VANG LÊN",
      "theme": "theme-ginga",
      "jp": "「切符を拝見いたします。……おや、坊っちゃん方の切符は、天上へでもどこへでも行ける本物の特別な切符ですね」",
      "vn": "\"Xin phép kiểm tra vé tàu. ...Ô kìa, vé của hai cậu bé là tấm vé đặc biệt đích thực, có thể lên tận thiên giới hay tới bất cứ chân trời nào.\"",
      "grammarInsight": "Kính ngữ khiêm nhường [拝見いたします]: Khiêm nhường ngữ của 見る (tôi xin phép được xem vé). Đi liền sau là dạng phủ định lửng tôn kính [天上へでもどこへでも行ける].",
      "nuanceInsight": "Người soát vé đóng vai trò người dẫn đường tâm linh, hé lộ chân tướng huyền ảo của tấm vé không gian đa chiều mà Giovanni mang theo.",
      "vocabTokens": [
        {
          "word": "拝見する",
          "reading": "はいけんする",
          "meaning": "Xin phép xem (Khiêm nhường ngữ)",
          "level": "N3"
        },
        {
          "word": "坊っちゃん",
          "reading": "ぼっちゃん",
          "meaning": "Công tử, cậu bé nhà gia giáo",
          "level": "N3"
        },
        {
          "word": "天上",
          "reading": "てんじょう",
          "meaning": "Thiên giới, cõi trời cao",
          "level": "N2"
        },
        {
          "word": "特別",
          "reading": "とくべつ",
          "meaning": "Đặc biệt, phi thường",
          "level": "N4"
        }
      ]
    },
    {
      "speaker": "語り手 (Người dẫn chuyện)",
      "role": "📜 旅の終わりと始まり • Lời tự sự ngân hà",
      "charId": "narrator",
      "type": "narration",
      "avatar": "assets/char_campanella.png",
      "leftSprite": null,
      "centerSprite": null,
      "rightSprite": null,
      "speakerPos": "center",
      "bg": "assets/bg_shinkai_carriage.jpg",
      "location": "✨ VŨ TRỤ HUYỀN BÍ • KHÚC CA QUA CHÒM NAM THẬP TỰ",
      "theme": "theme-ginga",
      "jp": "銀河の夜はどこまでも深く、車窓の外には金剛石の砂利とリンドウの花が静かに流れていくのでした。",
      "vn": "Đêm Ngân Hà tĩnh lặng thâm sâu vô cùng, ngoài cửa sổ toa tàu, những hạt sỏi kim cương và đóa hoa thạch thảo tím biếc cứ thế êm đềm lướt trôi.",
      "grammarInsight": "Dạng kết câu trần thuật cổ điển [〜のでした]: Dùng trong văn học thiếu nhi và truyện cổ tích để gợi âm hưởng hồi tưởng êm dịu, lắng đọng.",
      "nuanceInsight": "Miyazawa Kenji sử dụng những hình ảnh biểu tượng như kim cương và hoa Gentian tím để đặc tả cái đẹp thanh khiết của thế giới vĩnh hằng.",
      "vocabTokens": [
        {
          "word": "車窓",
          "reading": "しゃそう",
          "meaning": "Cửa sổ toa xe tàu hỏa",
          "level": "N2"
        },
        {
          "word": "金剛石",
          "reading": "こんごうせき",
          "meaning": "Kim cương thiên nhiên",
          "level": "N1"
        },
        {
          "word": "砂利",
          "reading": "じゃり",
          "meaning": "Sỏi đá nhỏ",
          "level": "N2"
        }
      ]
    }
  ],
  "merosu": [
    {
      "speaker": "メロス (Melos)",
      "role": "🗡️ シラクスの熱血漢 • Người chăn cừu xả thân vì công lý",
      "charId": "melos",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_melos.png",
      "leftSprite": "assets/chars/char_melos.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_dionys.png",
      "speakerPos": "left",
      "bg": "assets/covers/cover_anime_castle.jpg",
      "location": "🏛️ ĐIỆN VÀNG SYRACUSE • ĐỐI MẶT BẠO CHÚA DIONYS",
      "theme": "theme-merosu",
      "jp": "「暴君ディオニスよ！ 私を処刑する前に、三日間の猶予をくれ。妹の結婚式を故郷で挙げてやりたいのだ」",
      "vn": "\"Bạo chúa Dionys kia! Trước khi xử tử ta, hãy ban cho ta ba ngày ân hạn. Ta muốn về quê nhà hoàn tất hôn lễ cho em gái ta.\"",
      "grammarInsight": "Cấu trúc [〜前に]: Trước khi thực hiện hành động X. [〜てやりたい]: Thể hiện mong muốn làm điều gì đó tốt đẹp cho người thân nhỏ tuổi hơn (ở đây là em gái).",
      "nuanceInsight": "Dazai Osamu khắc họa Melos ngay từ đầu là người bộc trực, không toan tính, dám đối mặt bạo quyền vì nghĩa vụ gia đình.",
      "vocabTokens": [
        {
          "word": "暴君",
          "reading": "ぼうくん",
          "meaning": "Bạo chúa, kẻ cai trị tàn độc",
          "level": "N1"
        },
        {
          "word": "処刑",
          "reading": "しょけい",
          "meaning": "Xử tử, thi hành án phạt",
          "level": "N2"
        },
        {
          "word": "猶予",
          "reading": "ゆうよ",
          "meaning": "Sự ân hạn, hoãn lại",
          "level": "N1"
        },
        {
          "word": "結婚式",
          "reading": "けっこんしき",
          "meaning": "Lễ cưới, hôn lễ",
          "level": "N4"
        }
      ]
    },
    {
      "speaker": "ディオニス王 (Vua Dionys)",
      "role": "👑 猜疑心の暴君 • Bạo chúa chìm trong hoài nghi thế gian",
      "charId": "dionys",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_dionys.png",
      "leftSprite": "assets/chars/char_melos.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_dionys.png",
      "speakerPos": "right",
      "bg": "assets/covers/cover_anime_castle.jpg",
      "location": "🏛️ ĐIỆN VÀNG SYRACUSE • TIẾNG CƯỜI KHẨY CỦA BẠO QUYỀN",
      "theme": "theme-merosu",
      "jp": "「ふん、戯言を言うな。一度逃げ帰った者が再び戻ってくるはずがなかろう。人の心など信じられぬ」",
      "vn": "\"Hừ, đừng nói lời xằng bậy. Kẻ một khi đã trốn thoát nào có lý do gì tự quay lại nộp mạng. Lòng dạ con người trên đời này làm sao tin được!\"",
      "grammarInsight": "Cấu trúc [〜はずがなかろう (はずがない)]: Tuyệt đối không thể nào có chuyện như vậy. Thể phủ định phỏng đoán trang trọng của [はずがない].",
      "nuanceInsight": "Bạo chúa Dionys tượng trưng cho sự hoài nghi cực độ của nhân loại thời bấy giờ, khi lòng tin giữa người với người đã hoàn toàn sụp đổ.",
      "vocabTokens": [
        {
          "word": "戯言",
          "reading": "ざれごと / たわごと",
          "meaning": "Lời nói nhảm, chuyện hão huyền",
          "level": "N1"
        },
        {
          "word": "再び",
          "reading": "ふたたび",
          "meaning": "Lại một lần nữa",
          "level": "N3"
        },
        {
          "word": "信じる",
          "reading": "しんじる",
          "meaning": "Tin tưởng, đặt niềm tin",
          "level": "N4"
        }
      ]
    },
    {
      "speaker": "メロス (Melos)",
      "role": "🗡️ Lời thề son sắt",
      "charId": "melos",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_melos.png",
      "leftSprite": "assets/chars/char_melos.png",
      "centerSprite": "assets/chars/char_selinuntius.png",
      "rightSprite": "assets/chars/char_dionys.png",
      "speakerPos": "left",
      "bg": "assets/covers/cover_anime_castle.jpg",
      "location": "🏛️ ĐIỆN VÀNG SYRACUSE • CON TIM LÀM CON TIN",
      "theme": "theme-merosu",
      "jp": "「私には無二の友がいる！ 石工のセリヌンティウスだ。彼を身代わりの人質としてここに残す！」",
      "vn": "\"Ta có một người bạn chí cốt độc nhất vô nhị! Là người thợ đá Selinuntius. Ta sẽ để cậu ấy lại đây làm con tin thế mạng!\"",
      "grammarInsight": "Cấu trúc [〜として]: Với tư cách là, đóng vai trò là. Cụm [無二の友] (người bạn độc nhất vô nhị) nhấn mạnh mối liên kết tri kỷ phi thường.",
      "nuanceInsight": "Hành động đặt cược sinh mệnh của người bạn thân nhất biểu thị niềm tin sắt đá tuyệt đối của tình bằng hữu chân chính.",
      "vocabTokens": [
        {
          "word": "無二",
          "reading": "むに",
          "meaning": "Có một không hai, vô song",
          "level": "N1"
        },
        {
          "word": "石工",
          "reading": "いしく",
          "meaning": "Thợ điêu khắc đá",
          "level": "N2"
        },
        {
          "word": "身代わり",
          "reading": "みがわり",
          "meaning": "Người thế mạng, gánh thay",
          "level": "N2"
        },
        {
          "word": "人質",
          "reading": "ひとじち",
          "meaning": "Con tin",
          "level": "N2"
        }
      ]
    },
    {
      "speaker": "メロス (Melos)",
      "role": "💭 Sa trường hoàng hôn rực lửa",
      "charId": "melos",
      "type": "choice",
      "avatar": "assets/chars/avatar_melos.png",
      "leftSprite": "assets/chars/char_melos.png",
      "centerSprite": null,
      "rightSprite": null,
      "speakerPos": "left",
      "bg": "assets/covers/cover_anime_sunset.jpg",
      "location": "🏃 CON ĐƯỜNG VỀ SYRACUSE • MẶT TRỜI ĐANG DẦN CHÌM XUỐNG BIỂN",
      "theme": "theme-merosu",
      "jp": "「待っていろ、セリヌンティウス！ 私は裏切り者として死ぬわけにはいかない……！」",
      "vn": "\"Hãy đợi ta, Selinuntius! Ta tuyệt đối không thể chết như một kẻ phản bội hèn nhát...!\"",
      "choices": [
        {
          "choiceId": "merosu_c1",
          "branchType": "Dialogue Variation",
          "textJp": "「喉が渇き果てても、立ち止まらずに走り続ける！」",
          "textVn": "Cắn răng chạy tiếp không ngừng dù cổ họng khô rát (+15 XP)",
          "reactionText": "Melos gạt đi giọt mồ hôi đẫm máu, lao mình qua con dốc như tên bắn!",
          "xp": 15,
          "branchResponse": {
            "speaker": "メロス (Melos)",
            "role": "✦ Tinh thần bất khuất",
            "charId": "melos",
            "avatar": "assets/chars/char_melos.png",
            "leftSprite": "assets/chars/char_melos.png",
            "centerSprite": null,
            "rightSprite": "assets/chars/char_selinuntius.png",
            "speakerPos": "left",
            "bg": "assets/covers/cover_anime_sunset.jpg",
            "location": "☀️ ĐƯỜNG VỀ SYRACUSE • Ý CHÍ SẮT ĐÁ",
            "theme": "theme-merosu",
            "jp": "「走れ、メロス！ 太陽が沈む前に、友の命を救い出すのだ！」",
            "vn": "\"Chạy đi, Melos! Trước khi mặt trời lặn xuống, ta nhất định phải cứu mạng bạn ta!\"",
            "grammarInsight": "Cấu trúc [〜前に]: Trước khi sự việc X xảy đến.",
            "nuanceInsight": "Tiếng thét xé lòng và sức mạnh tinh thần vượt lên trên mọi giới hạn thể xác của Melos.",
            "vocabTokens": [
              { "word": "沈む", "reading": "しずむ", "meaning": "Chìm xuống, lặn (mặt trời)", "level": "N3" },
              { "word": "救い出す", "reading": "すくいだす", "meaning": "Cứu thoát, giải cứu", "level": "N2" }
            ]
          }
        },
        {
          "choiceId": "merosu_c2",
          "branchType": "Dialogue Variation",
          "textJp": "「岩陰の湧き水を一口すくい、全身の力を取り戻す！」",
          "textVn": "Uống ngụm nước suối mát lành để đánh thức mọi giác quan (+15 XP)",
          "reactionText": "Dòng nước suối ngọt ngào cứu sống linh hồn Melos, thổi bùng lên ngọn lửa hy vọng!",
          "xp": 15,
          "branchResponse": {
            "speaker": "セリヌンティウス (Selinuntius)",
            "role": "✦ Người bạn kiên trung",
            "charId": "selinuntius",
            "avatar": "assets/chars/char_selinuntius.png",
            "leftSprite": "assets/chars/char_melos.png",
            "centerSprite": null,
            "rightSprite": "assets/chars/char_selinuntius.png",
            "speakerPos": "right",
            "bg": "assets/covers/cover_anime_sunset.jpg",
            "location": "☀️ ĐOÀN TỤ TẠI PHÁP TRƯỜNG • ĐỨC TIN BẤT DIỆT",
            "theme": "theme-merosu",
            "jp": "「メロスよ、お前が帰ってくることを少しも疑わなかった。さあ、私を殴れ！ 一瞬でも疑った私を！」",
            "vn": "\"Melos, ta chưa từng mảy may nghi ngờ việc cậu sẽ trở về. Nào, hãy đấm ta đi! Đấm kẻ đã có một thoáng ngờ vực này!\"",
            "grammarInsight": "Cấu trúc [少しも〜ない]: Hoàn toàn không hề, một chút mảy may cũng không.",
            "nuanceInsight": "Giọt nước mắt xúc động và cái ôm nghẹn ngào giữa hai người bạn trước toàn thể nhân dân Syracuse.",
            "vocabTokens": [
              { "word": "疑う", "reading": "うたがう", "meaning": "Nghi ngờ, ngờ vực", "level": "N3" },
              { "word": "一瞬", "reading": "いっしゅん", "meaning": "Một thoáng, một khoảnh khắc", "level": "N2" }
            ]
          }
        }
      ],
      "grammarInsight": "Cấu trúc [〜わけにはいかない]: Không thể nào làm chuyện X được (do đạo đức, lương tâm hoặc hoàn cảnh bắt buộc không cho phép).",
      "nuanceInsight": "Đỉnh điểm của ý chí kiên định: Thà kiệt sức ngã gục trên đường chạy còn hơn sống sót nhục nhã như một kẻ bội ước.",
      "vocabTokens": [
        {
          "word": "裏切り者",
          "reading": "うらぎりもの",
          "meaning": "Kẻ phản bội, kẻ nuốt lời thề",
          "level": "N2"
        },
        {
          "word": "わけにはいかない",
          "reading": "wake ni wa ikanai",
          "meaning": "Không thể nào cho phép bản thân làm thế",
          "level": "N3"
        },
        {
          "word": "渇く",
          "reading": "かわく",
          "meaning": "Khát khô họng",
          "level": "N4"
        }
      ]
    },
    {
      "speaker": "セリヌンティウス (Selinuntius)",
      "role": "🤝 親友 • Người tri kỷ trên đoạn đầu đài",
      "charId": "selinuntius",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_selinuntius.png",
      "leftSprite": "assets/chars/char_melos.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_selinuntius.png",
      "speakerPos": "right",
      "bg": "assets/covers/cover_anime_sunset.jpg",
      "location": "⚔️ QUẢNG TRƯỜNG THI HÀNH ÁN • ĐÚNG KHOẢNH KHẮC HOÀNG HÔN TẮT",
      "theme": "theme-merosu",
      "jp": "「メロスよ、お前が帰ってくることを少しも疑わなかった。さあ、私を殴れ！ 一瞬でも疑った私を！」",
      "vn": "\"Melos, ta chưa từng mảy may nghi ngờ việc cậu sẽ trở về. Nào, hãy đấm ta đi! Hãy trừng phạt kẻ từng có một thoáng dao động nghi ngờ này!\"",
      "grammarInsight": "Cấu trúc [少しも〜ない]: Hoàn toàn không một chút nào... Mẫu câu [〜を一瞬でも]: Dẫu chỉ trong một tích tắc thoáng qua.",
      "nuanceInsight": "Cái tát lẫn nhau giữa hai người bạn là hành động gột rửa sạch sẽ những yếu lòng nhân tính, nâng tầm tình bạn thành biểu tượng thiêng liêng.",
      "vocabTokens": [
        {
          "word": "疑う",
          "reading": "うたがう",
          "meaning": "Nghi ngờ, ngờ vực",
          "level": "N3"
        },
        {
          "word": "殴る",
          "reading": "なぐる",
          "meaning": "Đấm, giáng một cú đấm",
          "level": "N3"
        },
        {
          "word": "一瞬",
          "reading": "いっしゅん",
          "meaning": "Một tích tắc, chớp mắt",
          "level": "N2"
        }
      ]
    },
    {
      "speaker": "ディオニス王 (Vua Dionys)",
      "role": "👑 改心した王 • Nước mắt của vị vua thức tỉnh",
      "charId": "dionys",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_dionys.png",
      "leftSprite": "assets/chars/char_melos.png",
      "centerSprite": "assets/chars/char_dionys.png",
      "rightSprite": "assets/chars/char_selinuntius.png",
      "speakerPos": "center",
      "bg": "assets/covers/cover_anime_sunset.jpg",
      "location": "🌅 QUẢNG TRƯỜNG SYRACUSE • TIẾNG HOAN HÔ RỢP TRỜI",
      "theme": "theme-merosu",
      "jp": "「お前たちの勝利だ。真の信頼とは何か、今この目で確かに見た。どうか私をも仲間に入れてくれ」",
      "vn": "\"Các ngươi đã chiến thắng rồi. Lòng tin chân thực là gì, hôm nay chính mắt ta đã được chứng kiến. Xin hãy cho ta kết nạp làm một người bạn của các ngươi.\"",
      "grammarInsight": "Cấu trúc [〜てくれ / 入れてくれ]: Cầu xin một cách khẩn thiết, chân thành từ một bạo chúa kiêu ngạo nay hạ mình học bài học lòng tin.",
      "nuanceInsight": "Cái kết huy hoàng rực rỡ của văn học nhân văn lãng mạn: Ánh sáng của lòng trung thực và đức hy sinh đã làm tan chảy cả trái tim sắt đá nhất.",
      "vocabTokens": [
        {
          "word": "勝利",
          "reading": "しょうり",
          "meaning": "Chiến thắng vinh quang",
          "level": "N3"
        },
        {
          "word": "信頼",
          "reading": "しんらい",
          "meaning": "Lòng tin cậy, tín nhiệm",
          "level": "N3"
        },
        {
          "word": "仲間",
          "reading": "なかま",
          "meaning": "Bạn hữu, đồng đội cùng chí hướng",
          "level": "N4"
        }
      ]
    }
  ],
  "gon": [
    {
      "speaker": "ごん (Chú cáo nhỏ Gon)",
      "role": "🦊 いたずら好きの子狐 • Chú cáo tinh nghịch cô đơn",
      "charId": "gon",
      "type": "monologue",
      "avatar": "assets/chars/avatar_gon.png",
      "leftSprite": "assets/chars/char_gon.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_hyoju.png",
      "speakerPos": "left",
      "bg": "assets/covers/cover_anime_forest.jpg",
      "location": "🍂 BỜ SUỐI LÀNG QUÊ • NẮNG CHIỀU VÀ TẤM LƯỚI CÁ",
      "theme": "theme-gon",
      "jp": "（兵十が川でうなぎを捕っているぞ。ふふっ、ちょっといたずらして驚かせてやろうか）",
      "vn": "(Anh Hyoju đang bắt lươn dưới suối kìa. Hì hì, hay là mình nghịch ngợm một tí cho anh ấy giật mình chơi nhỉ?)",
      "grammarInsight": "Cấu trúc [〜て驚かせてやろうか]: Thể ý chí [〜ようか] kết hợp thể sai khiến [驚かせる] biểu thị ý định tinh nghịch trêu chọc.",
      "nuanceInsight": "Gon bản chất không hề có ác ý, sự nghịch ngợm của chú cáo chỉ là khao khát được kết nối và chú ý trong nỗi cô đơn lẻ loi.",
      "vocabTokens": [
        {
          "word": "うなぎ",
          "reading": "うなぎ",
          "meaning": "Con lươn suối",
          "level": "N3"
        },
        {
          "word": "いたずら",
          "reading": "いたずら",
          "meaning": "Trò nghịch ngợm, quậy phá",
          "level": "N3"
        },
        {
          "word": "驚かせる",
          "reading": "おどろかせる",
          "meaning": "Làm cho kinh ngạc, giật mình",
          "level": "N3"
        }
      ]
    },
    {
      "speaker": "ごん (Chú cáo nhỏ Gon)",
      "role": "🦊 Lựa chọn của Gon bên bờ suối",
      "charId": "gon",
      "type": "choice",
      "avatar": "assets/chars/avatar_gon.png",
      "leftSprite": "assets/chars/char_gon.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_hyoju.png",
      "speakerPos": "left",
      "bg": "assets/covers/cover_anime_forest.jpg",
      "location": "🍂 BỤI CỎ VEN BỜ SUỐI • THỜI KHẮC ĐỊNH MỆNH",
      "theme": "theme-gon",
      "jp": "「兵十のビクの中に、大きなうなぎが跳ねているぞ……」",
      "vn": "\"Trong giỏ cá của anh Hyoju, có con lươn béo tròn đang quẫy đạp...\"",
      "choices": [
        {
          "choiceId": "gon_c1",
          "branchType": "Branching Scene",
          "textJp": "「ビクからうなぎを引っ張り出して川へ逃がす！」",
          "textVn": "Kéo con lươn ra thả về dòng nước suối (+15 XP)",
          "reactionText": "Con lươn trườn tuột vào dòng nước, Hyoju tức giận vung chài đuổi bắt!",
          "xp": 15
        },
        {
          "choiceId": "gon_c2",
          "branchType": "Dialogue Variation",
          "textJp": "「いたずらをぐっと我慢して、草むらから見守る」",
          "textVn": "Kìm nén trò đùa, lặng lẽ quan sát từ bụi cỏ dày (+15 XP)",
          "reactionText": "Gon cảm nhận được nỗi nhọc nhằn của người tiều phu đánh cá mưu sinh...",
          "xp": 15
        }
      ],
      "grammarInsight": "Cấu trúc [〜が跳ねている]: Động từ miêu tả hành động đang diễn ra trước mắt (con lươn đang quẫy đạp).",
      "nuanceInsight": "Hành động tưởng chừng vô hại này chính là mấu chốt dẫn đến bi kịch khi mẹ Hyoju ốm nặng không kịp ăn bát cháo lươn.",
      "vocabTokens": [
        {
          "word": "引っ張り出す",
          "reading": "ひっぱりだす",
          "meaning": "Kéo tuột ra ngoài",
          "level": "N2"
        },
        {
          "word": "我慢する",
          "reading": "がまんする",
          "meaning": "Nhẫn nhịn, chịu đựng kiềm chế",
          "level": "N3"
        },
        {
          "word": "草むら",
          "reading": "くさむら",
          "meaning": "Bụi cỏ dại ven đường",
          "level": "N2"
        }
      ]
    },
    {
      "speaker": "兵十 (Anh tiều phu Hyoju)",
      "role": "🌾 村の若者 • Người tiều phu nghèo hiếu thảo",
      "charId": "hyoju",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_hyoju.png",
      "leftSprite": "assets/chars/char_gon.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_hyoju.png",
      "speakerPos": "right",
      "bg": "assets/covers/cover_anime_forest.jpg",
      "location": "🍂 BỜ SUỐI LÀNG QUÊ • TIẾNG LA MẮNG CỦA HYOJU",
      "theme": "theme-gon",
      "jp": "「こらっ、ごん狐！ また悪戯をしにきたな！ 母ちゃんに食べさせる大事な魚なんだぞ！」",
      "vn": "\"Này con cáo Gon kia! Mày lại đến đây quậy phá nữa hả! Đây là con cá quý để tao bồi bổ cho mẹ già đang ốm đấy!\"",
      "grammarInsight": "Cấu trúc [〜に食べさせる]: Thể sai khiến [食べさせる] mang ý nghĩa hiếu thảo \"cho ai đó ăn / phụng dưỡng\".",
      "nuanceInsight": "Chi tiết hé lộ động cơ thiêng liêng của Hyoju, làm dậy sóng lương tâm và sự hối hận khôn nguôi trong lòng Gon sau này.",
      "vocabTokens": [
        {
          "word": "大事",
          "reading": "だいじ",
          "meaning": "Quan trọng, quý giá",
          "level": "N4"
        },
        {
          "word": "食べさせる",
          "reading": "たべさせる",
          "meaning": "Cho ăn, dâng món ăn cho mẹ",
          "level": "N3"
        }
      ]
    },
    {
      "speaker": "ごん (Chú cáo nhỏ Gon)",
      "role": "🌰 栗と松茸の贈り物 • Âm thầm chuộc lại lỗi lầm",
      "charId": "gon",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_gon.png",
      "leftSprite": "assets/chars/char_gon.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_hyoju.png",
      "speakerPos": "left",
      "bg": "assets/covers/cover_anime_night_shrine.jpg",
      "location": "🏡 THỀM NHÀ TRANH MÙA THU • ÁNH TRĂNG CHIẾU XUỐNG RỔ HẠT DẺ",
      "theme": "theme-gon",
      "jp": "「今夜も兵十の家の戸口に、山で拾った栗と松茸をどっさり置いていこう……許してくれよな」",
      "vn": "\"Đêm nay mình lại đem hạt dẻ và nấm tùng nhặt được trên núi đặt trước hiên nhà anh Hyoju... Xin anh hãy tha lỗi cho tôi nhé.\"",
      "grammarInsight": "Phó từ [どっさり]: Đầy ắp, chất đống nặng trĩu. Cấu trúc [〜ていこう]: Thể hiện hành động kiên trì tiếp diễn trong tương lai.",
      "nuanceInsight": "Tình cảm chuộc tội thầm lặng, thánh thiện của Gon diễn ra dưới ánh trăng mùa thu tịch mịch tạo nên dư âm buồn man mác của văn học Niimi Nankichi.",
      "vocabTokens": [
        {
          "word": "戸口",
          "reading": "とぐち",
          "meaning": "Ngưỡng cửa, cửa ngõ ra vào",
          "level": "N2"
        },
        {
          "word": "どっさり",
          "reading": "dossari",
          "meaning": "Đầy ắp, ê hề nhiều",
          "level": "N2"
        },
        {
          "word": "松茸",
          "reading": "まつたけ",
          "meaning": "Nấm Tùng quý giá",
          "level": "N2"
        },
        {
          "word": "許す",
          "reading": "ゆるす",
          "meaning": "Tha thứ, bao dung lỗi lầm",
          "level": "N3"
        }
      ]
    }
  ],
  "rashomon": [
    {
      "speaker": "下人 (Người hạ nhân)",
      "role": "🌧️ 行き場を失った下人 • Đứng trước vực thẳm sinh tử",
      "charId": "genin",
      "type": "monologue",
      "avatar": "assets/chars/avatar_genin.png",
      "leftSprite": "assets/chars/char_genin.png",
      "centerSprite": null,
      "rightSprite": null,
      "speakerPos": "left",
      "bg": "assets/bg_shinkai_rashomon.jpg",
      "location": "⛩️ DƯỚI CỔNG THÀNH KYOTO • CHIỀU MƯA RẢ RÍCH HOANG TÀN",
      "theme": "theme-rashomon",
      "jp": "（雨はやみそうにない。このまま飢え死にするか、盗人になるか……おれにはもう道がない）",
      "vn": "(Mưa chiều không có dấu hiệu tạnh. Cứ đà này thì hoặc là chết đói, hoặc là trở thành kẻ trộm... ta chẳng còn con đường nào khác.)",
      "grammarInsight": "Cấu trúc [〜そうにない]: Dường như tuyệt đối không thể nào... Mẫu diễn đạt [A か B か]: Lựa chọn giằng xé giữa hai ngả đường cùng quẫn.",
      "nuanceInsight": "Vấn đề hiện sinh cốt lõi của Akutagawa Ryunosuke: Khi xã hội suy đồi đến đáy, ranh giới giữa cái thiện và cái ác trở nên mỏng manh như sợi tóc.",
      "vocabTokens": [
        {
          "word": "やみそうにない",
          "reading": "yamisou ni nai",
          "meaning": "Dường như không thể tạnh mưa",
          "level": "N3"
        },
        {
          "word": "飢え死にする",
          "reading": "うえじにする",
          "meaning": "Chết vì đói lả",
          "level": "N2"
        },
        {
          "word": "盗人",
          "reading": "ぬすびと",
          "meaning": "Kẻ trộm cắp, kẻ cướp",
          "level": "N2"
        }
      ]
    },
    {
      "speaker": "語り手 (Người dẫn chuyện)",
      "role": "📜 La Sinh Môn trong mưa chiều hoang tàn",
      "charId": "narrator",
      "type": "narration",
      "avatar": "assets/chars/avatar_genin.png",
      "leftSprite": null,
      "centerSprite": null,
      "rightSprite": null,
      "speakerPos": "center",
      "bg": "assets/bg_shinkai_rashomon.jpg",
      "location": "⛩️ CỔNG THÀNH LA SINH MÔN • ĐỐM LỬA TRÊN TẦNG GÁC",
      "theme": "theme-rashomon",
      "jp": "荒れ果てた門の楼上からは、かすかに一筋の火の光が漏れ、動物のような物音が聞こえていた。",
      "vn": "Từ trên tầng gác mái hoang tàn của cổng thành, le lói hắt ra một vệt ánh lửa mờ nhạt, kèm theo những tiếng động sột soạt như loài thú dữ.",
      "grammarInsight": "Cụm [荒れ果てた]: Tan hoang, đổ nát hoàn toàn. [一筋の火の光]: Một vệt sáng le lói đơn độc xuyên qua bóng tối.",
      "nuanceInsight": "Bút pháp miêu tả mang không khí u ám, lạnh lẽo đặc trưng của văn học cận đại Nhật Bản.",
      "vocabTokens": [
        {
          "word": "荒れ果てた",
          "reading": "あれはてた",
          "meaning": "Hoang tàn, trơ trọi đổ nát",
          "level": "N2"
        },
        {
          "word": "楼上",
          "reading": "ろうじょう",
          "meaning": "Tầng gác mái lầu cao",
          "level": "N1"
        },
        {
          "word": "物音",
          "reading": "ものおと",
          "meaning": "Âm thanh lạ phát ra",
          "level": "N3"
        }
      ]
    },
    {
      "speaker": "老婆 (Bà lão nhổ tóc)",
      "role": "🕯️ 楼上の老婆 • Sinh tồn giữa cõi thây ma",
      "charId": "oldwoman",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_oldwoman.png",
      "leftSprite": "assets/chars/char_genin.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_oldwoman.png",
      "speakerPos": "right",
      "bg": "assets/rashomon.png",
      "location": "🕯️ TẦNG GÁC MÁI LA SINH MÔN • ÁNH ĐUỐC SOI RỌI BỘ MẶT CỦA SỰ THẬT",
      "theme": "theme-rashomon",
      "jp": "「この髪を抜いて鬘にするのじゃ。生きるためなら、死人の髪を抜くのも悪くはあるまい」",
      "vn": "\"Ta nhổ tóc này để làm tóc giả đem bán. Nếu vì miếng cơm sinh tồn, thì nhổ tóc người chết nào có phải điều ác đáng trách.\"",
      "grammarInsight": "Cấu trúc [〜悪くはあるまい]: Há lại là điều xấu sao (phủ định biểu thị ý khẳng định ngầm thanh minh cho bản thân).",
      "nuanceInsight": "Triết lý sinh tồn tàn nhẫn của bà lão: Bất kỳ hành vi nào, dẫu vô đạo đức tới đâu, đều được hợp thức hóa khi khoác lên chiếc áo \"vì sinh tồn\".",
      "vocabTokens": [
        {
          "word": "鬘",
          "reading": "かつら",
          "meaning": "Tóc giả",
          "level": "N1"
        },
        {
          "word": "死人",
          "reading": "しびと",
          "meaning": "Người đã chết, tử thi",
          "level": "N2"
        },
        {
          "word": "悪くはあるまい",
          "reading": "waruku wa arumai",
          "meaning": "Há lại là điều sai trái sao",
          "level": "N1"
        }
      ]
    },
    {
      "speaker": "下人 (Người hạ nhân)",
      "role": "⚡ Lựa chọn lương tri hay thú tính",
      "charId": "genin",
      "type": "choice",
      "avatar": "assets/chars/avatar_genin.png",
      "leftSprite": "assets/chars/char_genin.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_oldwoman.png",
      "speakerPos": "left",
      "bg": "assets/rashomon.png",
      "location": "🕯️ LA SINH MÔN • RANH GIỚI BƯỚC VÀO ĐÊM TỐI",
      "theme": "theme-rashomon",
      "jp": "「そうか……生きるためなら許されるのだな。ならば！」",
      "vn": "\"Hóa ra là vậy... nếu vì sinh tồn thì việc gì cũng được dung thứ sao. Thế thì!\"",
      "choices": [
        {
          "choiceId": "rashomon_c1",
          "branchType": "Branching Scene",
          "textJp": "「『その言葉、忘れるな！』と老婆の着物を剥ぎ取る」",
          "textVn": "Lột phăng chiếc áo bào của mụ già để đem bán lấy tiền sống sót (+15 XP)",
          "reactionText": "Hạ nhân lột sạch áo của mụ già, đạp mụ ngã dúi dụi rồi lao mình vào màn đêm!",
          "xp": 15
        },
        {
          "choiceId": "rashomon_c2",
          "branchType": "Branching Scene",
          "textJp": "「良心の呵責を覚え、刀を収めて闇の中へ立ち去る」",
          "textVn": "Khựng lại trước lương tri, quay gót bước vào mưa chiều dằn vặt (+15 XP)",
          "reactionText": "Chàng hạ nhân cắn chặt môi, chọn giữ lại phần người còn sót lại trong tâm can...",
          "xp": 15
        }
      ],
      "grammarInsight": "Cấu trúc [〜ならば]: Nếu quả thật là như thế thì... Liên từ mang tính quyết đoán cao sau khi đã thông tỏ chân tướng.",
      "nuanceInsight": "Bước ngoặt tâm lý chớp nhoáng: Lòng căm thù cái ác ban đầu biến tướng thành hành động cướp bóc nhân danh cùng một lý lẽ.",
      "vocabTokens": [
        {
          "word": "剥ぎ取る",
          "reading": "はぎとる",
          "meaning": "Lột phăng, tước đoạt",
          "level": "N2"
        },
        {
          "word": "良心",
          "reading": "りょうしん",
          "meaning": "Lương tâm, thiên lương",
          "level": "N3"
        },
        {
          "word": "呵責",
          "reading": "かしゃく",
          "meaning": "Sự dằn vặt, cắn rứt",
          "level": "N1"
        }
      ]
    },
    {
      "speaker": "語り手 (Người dẫn chuyện)",
      "role": "📜 Màn đêm nuốt chửng cổng La Sinh Môn",
      "charId": "narrator",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_genin.png",
      "leftSprite": null,
      "centerSprite": null,
      "rightSprite": null,
      "speakerPos": "center",
      "bg": "assets/bg_shinkai_rashomon.jpg",
      "location": "⛩️ CỔNG THÀNH LA SINH MÔN • ĐÊM ĐEN BẤT TẬN",
      "theme": "theme-rashomon",
      "jp": "下人は、すばやく楼を駆け下りて、漆黒の夜の闇の中へ消えていった。下人の行方は、誰も知らない。",
      "vn": "\"Gã hạ nhân nhanh thoăn thoắt lao xuống cầu thang lầu gác, rồi biến mất hút vào màn đêm đen kịt đặc quánh. Về tung tích của gã, vĩnh viễn chẳng một ai hay biết.\"",
      "grammarInsight": "Cấu trúc [〜の中へ消えていく]: Dần biến mất vào sâu trong không gian X. Câu văn kết kinh điển: [下人の行方は、誰も知らない].",
      "nuanceInsight": "Dấu chấm hết đầy ám ảnh của kiệt tác Rashomon: Kẻ hạ nhân một khi đã bước qua lằn ranh tội ác thì bị nuốt chửng vào bóng tối của lịch sử.",
      "vocabTokens": [
        {
          "word": "漆黒",
          "reading": "しっこく",
          "meaning": "Đen kịt, tối như mực",
          "level": "N1"
        },
        {
          "word": "行方",
          "reading": "ゆくえ",
          "meaning": "Tung tích, chốn đi về",
          "level": "N2"
        }
      ]
    }
  ],
  "gakuen": [
    {
      "speaker": "星野 瑠奈 (Hoshino Runa)",
      "role": "⭐ 学園の絶対的アイドル • Gakuen Idol vạn người mê",
      "charId": "runa",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_runa.png",
      "leftSprite": "assets/chars/char_mob.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_runa.png",
      "speakerPos": "right",
      "bg": "assets/covers/cover_school_romcom.jpg",
      "location": "🏫 HÀNH LANG HỌC VIỆN • CÚ TỎ TÌNH BẤT NGỜ CỦA GAKUEN IDOL",
      "theme": "theme-gakuen",
      "jp": "「みんな、聞いて！ この人が私の彼氏なの。だからもう誰も告白してこないで！」",
      "vn": "\"Mọi người nghe đây! Người này chính là bạn trai của tớ. Thế nên từ giờ đừng ai đến tỏ tình với tớ nữa!\"",
      "grammarInsight": "Cấu trúc [〜てこないで]: Xin đừng tiếp tục làm hành động hướng về phía tôi nữa. Cụm [告白してくる] = tiến tới tỏ tình với ai đó.",
      "nuanceInsight": "Tình huống kinh điển của Light Novel RomCom học đường: Cô nàng Idol viện cớ kéo một nam sinh bình thường làm lá chắn tình ái.",
      "vocabTokens": [
        {
          "word": "彼氏",
          "reading": "かれし",
          "meaning": "Bạn trai, người yêu",
          "level": "N4"
        },
        {
          "word": "告白する",
          "reading": "こくはくする",
          "meaning": "Tỏ tình, bộc bạch lòng mình",
          "level": "N3"
        },
        {
          "word": "アイドル",
          "reading": "aidoru",
          "meaning": "Thần tượng học đường",
          "level": "N4"
        }
      ]
    },
    {
      "speaker": "モブ主人公 (Nam sinh bình phàm)",
      "role": "💭 心の声 • Tiếng thét hoang mang trong lồng ngực",
      "charId": "mob",
      "type": "choice",
      "avatar": "assets/chars/avatar_mob.png",
      "leftSprite": "assets/chars/char_mob.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_runa.png",
      "speakerPos": "left",
      "bg": "assets/covers/cover_school_romcom.jpg",
      "location": "🏫 HÀNH LANG HỌC VIỆN • TÂM ĐIỂM CỦA MỌI ÁNH NHÌN",
      "theme": "theme-gakuen",
      "jp": "（学園一の美少女アイドルが、なぜ平凡なモブの俺の腕にしがみついているんだ……！？）",
      "vn": "(Tại sao Idol xinh đẹp số một học viện lại ôm chặt lấy cánh tay của một tên nam sinh mờ nhạt như mình thế này...!?)",
      "choices": [
        {
          "choiceId": "gakuen_c1",
          "branchType": "Dialogue Variation",
          "textJp": "「『ええっ！？ 僕なんかで本当にいいの？』と戸惑う」",
          "textVn": "Bối rối hoang mang: \"Hả!? Người như tớ mà được sao?\" (+15 XP)",
          "reactionText": "Runa nháy mắt tinh nghịch rồi khẽ ghé sát tai bạn thì thầm lời cảm ơn!",
          "xp": 15
        },
        {
          "choiceId": "gakuen_c2",
          "branchType": "Dialogue Variation",
          "textJp": "「『よし、任せて！ 完璧に彼氏役を演じてみせるよ』と引き受ける」",
          "textVn": "Bản lĩnh nhận lời: \"Được thôi, cứ để tớ diễn tròn vai!\" (+15 XP)",
          "reactionText": "Gương mặt Runa thoáng ửng đỏ ngượng ngùng trước sự dứt khoát của bạn!",
          "xp": 15
        }
      ],
      "grammarInsight": "Cấu trúc [なぜ〜ているんだ]: Câu hỏi chất vấn bản thân với cảm xúc kinh ngạc tột độ.",
      "nuanceInsight": "Sự tương phản hài hước giữa một nam sinh tự nhận là Mob và cô nàng Idol nổi tiếng nhất trường.",
      "vocabTokens": [
        {
          "word": "美少女",
          "reading": "びしょうじょ",
          "meaning": "Mỹ thiếu nữ, cô gái xinh đẹp",
          "level": "N3"
        },
        {
          "word": "平凡",
          "reading": "へいぼん",
          "meaning": "Bình thường, mờ nhạt",
          "level": "N3"
        },
        {
          "word": "しがみつく",
          "reading": "shigamitsuku",
          "meaning": "Bám chặt lấy, ôm ghì lấy",
          "level": "N2"
        }
      ]
    },
    {
      "speaker": "星野 瑠奈 (Hoshino Runa)",
      "role": "🌇 Hoàng hôn trên sân thượng bí mật",
      "charId": "runa",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_runa.png",
      "leftSprite": "assets/chars/char_mob.png",
      "centerSprite": null,
      "rightSprite": "assets/chars/char_runa.png",
      "speakerPos": "right",
      "bg": "assets/covers/cover_anime_sunset.jpg",
      "location": "🌇 SÂN THƯỢNG HỌC VIỆN • GIÓ CHIỀU VÀ ÁNH MẮT THẸN THÙNG",
      "theme": "theme-gakuen",
      "jp": "「ふふっ、さっきは助かったわ。でもね……ただのフリじゃなくて、本当に好きになっちゃったらどうする？」",
      "vn": "\"Hi hi, ban nãy may mà có cậu giải vây đấy. Nhưng mà này... nếu không chỉ là đóng kịch, mà tớ lỡ thích cậu thật thì cậu tính sao?\"",
      "grammarInsight": "Cấu trúc [〜ちゃったら (てしまったら)]: Nếu lỡ như xảy ra điều gì ngoài dự tính. Cấu trúc [どうする]: Tính thế nào đây?",
      "nuanceInsight": "Cú twist ngọt ngào điển hình của RomCom: Khi ranh giới giữa thỏa thuận giả vờ và cảm xúc thật bắt đầu xóa nhòa.",
      "vocabTokens": [
        {
          "word": "助かった",
          "reading": "たすかった",
          "meaning": "Đã được cứu nguy, cảm ơn cậu",
          "level": "N4"
        },
        {
          "word": "フリ",
          "reading": "furi",
          "meaning": "Sự giả vờ, đóng kịch giả bộ",
          "level": "N3"
        },
        {
          "word": "好きになる",
          "reading": "すきになる",
          "meaning": "Phải lòng, đem lòng yêu thích",
          "level": "N5"
        }
      ]
    }
  ],
  "chuumon": [
    {
      "speaker": "紳士の猟師 (Hai chàng thợ săn)",
      "role": "🎩 東京から来た紳士 • Hai quý tộc thành thị lạc vào rừng sâu",
      "charId": "hunter",
      "type": "dialogue",
      "avatar": "assets/chars/avatar_mob.png",
      "leftSprite": "assets/chars/char_mob.png",
      "centerSprite": null,
      "rightSprite": null,
      "speakerPos": "left",
      "bg": "assets/covers/cover_isekai_gourmet.jpg",
      "location": "🌲 RỪNG SÂU MÙ SƯƠNG • CỬA HIỆU SƠN MIÊU HIÊN KỲ BÍ",
      "theme": "theme-chuumon",
      "jp": "「おや、こんな山奥に立派な西洋料理店があるぞ！『山猫軒・どなたもどうかお入りください』と書いてある」",
      "vn": "\"Kìa, giữa rừng sâu núi thẳm thế này mà lại có một nhà hàng Tây sang trọng! Biển hiệu đề: Nhà hàng Sơn Miêu - Xin mời bất cứ ai cũng đừng ngại bước vào.\"",
      "grammarInsight": "Cấu trúc [〜と書いてある]: Đang có ghi dòng chữ rằng... [どなたもどうかお入りください]: Lời mời kính ngữ trang trọng bậc nhất.",
      "nuanceInsight": "Vẻ ngoài hào nhoáng và hiếu khách của tiệm ăn phương Tây giữa chốn rừng sâu chính là cái bẫy tinh vi của loài mèo ma.",
      "vocabTokens": [
        {
          "word": "山奥",
          "reading": "やまおく",
          "meaning": "Sâu thẳm trong núi non",
          "level": "N3"
        },
        {
          "word": "立派",
          "reading": "りっぱ",
          "meaning": "Lộng lẫy, tráng lệ",
          "level": "N4"
        },
        {
          "word": "どなたも",
          "reading": "donatamo",
          "meaning": "Bất kỳ ai (Kính ngữ của だれも)",
          "level": "N3"
        }
      ]
    },
    {
      "speaker": "山猫軒の扉 (Lời nhắn trên cửa vàng)",
      "role": "🚪 怪しき扉の指示 • Lời yêu cầu quái dị thứ 6",
      "charId": "chef",
      "type": "choice",
      "avatar": "assets/char_conductor.png",
      "leftSprite": "assets/chars/char_mob.png",
      "centerSprite": "assets/char_conductor.png",
      "rightSprite": null,
      "speakerPos": "center",
      "bg": "assets/covers/cover_anime_cafe.jpg",
      "location": "🍽️ PHÒNG KHÁCH SƠN MIÊU HIÊN • CÁNH CỬA SỐ 6 VÀ HŨ KEM TRẮNG",
      "theme": "theme-chuumon",
      "jp": "「『壺の中のクリームを顔や手足にすっかり塗ってください』……なあ、これって本当に美味しい料理を食べるためなのか？」",
      "vn": "\"Hãy lấy kem trong hũ này bôi đều lên khắp mặt và tay chân... Này, việc này thực sự là để thưởng thức đồ ăn ngon sao?\"",
      "choices": [
        {
          "choiceId": "chuumon_c1",
          "branchType": "Branching Scene",
          "textJp": "「指示に従い、顔や手足にクリームを塗りたくる」",
          "textVn": "Ngây thơ làm theo: Bôi đẫm kem béo lên khắp mặt và chân tay (+15 XP)",
          "reactionText": "Mùi bơ thơm lừng lan tỏa, phía sau cánh cửa vang lên tiếng mài dao xoèn xoẹt!",
          "xp": 15
        },
        {
          "choiceId": "chuumon_c2",
          "branchType": "Branching Scene",
          "textJp": "「何かおかしいと気付き、出口へ向かって脱出を図る！」",
          "textVn": "Nhận ra cạm bẫy: Quay đầu tìm cách thoát thân trong gang tấc! (+15 XP)",
          "reactionText": "Hai thợ săn giật bắn mình, hai con mắt mèo xanh lè sau khe cửa trừng trừng nhìn!",
          "xp": 15
        }
      ],
      "grammarInsight": "Cấu trúc [〜にすっかり塗る]: Bôi trát toàn bộ không chừa chỗ nào. [壺]: Chiếc bình / hũ gốm đựng gia vị.",
      "nuanceInsight": "Nghệ thuật châm biếm sâu sắc của Miyazawa Kenji: Con người thành thị săn bắn thú rừng để tiêu khiển, nay lại chính là món mồi ngon được tẩm ướp gia vị.",
      "vocabTokens": [
        {
          "word": "壺",
          "reading": "つぼ",
          "meaning": "Hũ, bình gốm",
          "level": "N1"
        },
        {
          "word": "すっかり",
          "reading": "sukkari",
          "meaning": "Toàn bộ, hoàn toàn",
          "level": "N3"
        },
        {
          "word": "塗る",
          "reading": "ぬる",
          "meaning": "Bôi, thoa lên bề mặt",
          "level": "N3"
        }
      ]
    },
    {
      "speaker": "語り手 (Người dẫn chuyện)",
      "role": "🌲 Tiếng chó săn xé toạc màn sương",
      "charId": "narrator",
      "type": "dialogue",
      "avatar": "assets/char_conductor.png",
      "leftSprite": "assets/chars/char_mob.png",
      "centerSprite": null,
      "rightSprite": "assets/char_conductor.png",
      "speakerPos": "center",
      "bg": "assets/covers/cover_anime_forest.jpg",
      "location": "🌲 RỪNG SÂU MÙ SƯƠNG • TÀN TÍCH CỦA SƠN MIÊU HIÊN",
      "theme": "theme-chuumon",
      "jp": "「ワン、ワン！ ガタガタ！ 突如飛び込んできた二匹の白犬の咆哮と共に、不思議な西洋館は幻のように煙と消え去った。」",
      "vn": "\"Gâu! Gâu! Cánh cửa rung lên bần bật. Cùng với tiếng sủa vang dội của hai chú chó săn lao vào giải cứu, ngôi nhà Tây kỳ dị bỗng tan biến như một làn sương khói.\"",
      "grammarInsight": "Cấu trúc [〜と共に（とともに）]: Đồng thời với, cùng lúc diễn ra hành động X. [煙と消え去る]: Tan biến như làn khói.",
      "nuanceInsight": "Cái kết giàu tính ngụ ngôn của Miyazawa Kenji: Sự ảo ảnh của lòng tham và sự trở về với thực tại tự nhiên.",
      "vocabTokens": [
        {
          "word": "突如",
          "reading": "とつじょ",
          "meaning": "Đột ngột, bất thình lình",
          "level": "N1"
        },
        {
          "word": "消え去る",
          "reading": "きえさる",
          "meaning": "Biến mất, tiêu tan",
          "level": "N2"
        }
      ]
    }
  ]
};

// Satori Reader Sentence Analysis Sample Data
const sentencesData = {
  sent1: {
    id: 'sent1',
    level: 'N3',
    jp: '先生は、黒板に吊した大きな黒い星座の図の上から下へ白くけむった銀河の帯のようなところを指しながら、みんなに問いをかけました。',
    vn: 'Thầy giáo vừa chỉ tay từ trên xuống dưới vào vệt mờ trắng tựa dải Ngân Hà trên bản đồ các chòm sao màu đen treo trên bảng, vừa đặt câu hỏi cho cả lớp.',
    grammarNote: `
      <strong>💡 Điểm ngữ pháp cốt lõi:</strong><br>
      • <strong>V-stem + ながら:</strong> Diễn tả hai hành động diễn ra đồng thời bởi cùng một chủ thể (Ở đây: vừa chỉ tay [指しながら] vừa đặt câu hỏi [問いをかけました]).<br>
      • <strong>吊した (つるした):</strong> Động từ 吊す (treo) chia thể quá khứ liên thể bổ nghĩa cho cụm danh từ "bản đồ chòm sao lớn màu đen" (大きな黒い星座の図).<br>
      • <strong>ような:</strong> Trợ động từ tỷ dụ (比況) mang nghĩa "giống như, tựa như".
    `,
    tokens: [
      { word: '先生', furigana: 'せんせい', pos: 'Danh từ', role: 'Chủ ngữ câu', meaning: 'Thầy giáo, giáo viên' },
      { word: 'は', furigana: 'wa', pos: 'Trợ từ chủ đề', role: 'Đánh dấu chủ ngữ/đề tài', meaning: 'thì / là' },
      { word: '黒板', furigana: 'こくばん', pos: 'Danh từ', role: 'Địa điểm/mục tiêu', meaning: 'Bảng đen' },
      { word: 'に', furigana: 'ni', pos: 'Trợ từ cách', role: 'Chỉ nơi chốn đính kèm', meaning: 'ở trên, vào' },
      { word: '吊した', furigana: 'つるした', pos: 'Động từ ngũ đoạn', role: 'Thể Ta (Quá khứ) bổ nghĩa', meaning: 'Đã treo lên' },
      { word: '大きな', furigana: 'おおきな', pos: 'Liên thể từ', role: 'Bổ nghĩa trực tiếp cho danh từ', meaning: 'To lớn' },
      { word: '黒い', furigana: 'くろい', pos: 'Tính từ đuôi -i', role: 'Bổ nghĩa tính chất danh từ', meaning: 'Màu đen' },
      { word: '星座の図', furigana: 'せいざのず', pos: 'Cụm danh từ', role: 'Tân ngữ của hành động treo', meaning: 'Bản đồ chòm sao' },
      { word: '上から下へ', furigana: 'うえからしたへ', pos: 'Trạng từ phương hướng', role: 'Điểm khởi đầu (から) và đích đến (へ)', meaning: 'Từ trên xuống dưới' },
      { word: '白くけむった', furigana: 'しろくけむった', pos: 'Động từ/Tính từ', role: 'Mô tả khói sương mờ', meaning: 'Mờ ảo trắng xóa' },
      { word: '銀河の帯', furigana: 'ぎんがのおび', pos: 'Danh từ ghép', role: 'Đối tượng được ví von', meaning: 'Dải Ngân Hà' },
      { word: 'のような', furigana: 'の よう な', pos: 'Trợ động từ tỷ dụ', role: 'So sánh hình tượng', meaning: 'Tựa như, giống như' },
      { word: 'ところを', furigana: 'ところ を', pos: 'Danh từ + Trợ từ を', role: 'Tân ngữ trực tiếp của 指す', meaning: 'Nơi đó / chỗ đó' },
      { word: '指しながら', furigana: 'さしながら', pos: 'Động từ liên dụng + ながら', role: 'Hành động song song 1', meaning: 'Vừa chỉ tay...' },
      { word: 'みんなに', furigana: 'みんな に', pos: 'Đại từ + Trợ từ に', role: 'Đối tượng tiếp nhận', meaning: 'Tới mọi người / cả lớp' },
      { word: '問いをかけました', furigana: 'といをかけました', pos: 'Cụm động từ lịch sự', role: 'Vị ngữ chính của câu', meaning: 'Đã đặt câu hỏi' }
    ]
  },
  sent2: {
    id: 'sent2',
    level: 'N4',
    jp: '「ではみなさん、このぼんやりと白く見えるものは本当は何かご承知ですか」',
    vn: '"Vậy các em, vật trông lờ mờ trắng này thực ra là cái gì các em có biết không?"',
    grammarNote: `
      <strong>💡 Điểm ngữ pháp cốt lõi:</strong><br>
      • <strong>見える (みえる):</strong> Tha động từ 見る (nhìn) vs Tự động từ khả năng 見える (tự nó đập vào mắt, trông thấy).<br>
      • <strong>何か (なにか):</strong> Nghi vấn từ 何 kết hợp trợ từ か tạo thành đại từ phiếm chỉ "cái gì đó / điều gì".<br>
      • <strong>ご承知 (ごしょうち):</strong> Dạng tôn kính ngữ (Kenjougo/Sonkeigo) tiền tố ご + 承知, dùng trong văn cảnh thầy cô hỏi học sinh một cách lịch thiệp.
    `,
    tokens: [
      { word: 'では', furigana: 'dewa', pos: 'Liên từ', role: 'Chuyển ý / mở đầu lời nói', meaning: 'Vậy thì, thế thì' },
      { word: 'みなさん', furigana: 'minasan', pos: 'Đại từ nhân xưng', role: 'Hô ngữ gọi cả lớp', meaning: 'Các em, mọi người' },
      { word: 'この', furigana: 'kono', pos: 'Chỉ định từ', role: 'Bổ nghĩa vật ở gần', meaning: 'Này, cái này' },
      { word: 'ぼんやりと', furigana: 'bonyari to', pos: 'Phó từ tượng thái', role: 'Mô tả trạng thái mờ nhạt', meaning: 'Lờ mờ, mờ ảo' },
      { word: '白く', furigana: 'しろく', pos: 'Tính từ đuôi -i (thể Ku)', role: 'Bổ nghĩa cho động từ 見える', meaning: 'Màu trắng' },
      { word: '見えるもの', furigana: 'みえるもの', pos: 'Động từ khả năng + もの', role: 'Danh từ hóa đối tượng nhìn thấy', meaning: 'Vật trông thấy' },
      { word: '本当は', furigana: 'ほんとうは', pos: 'Phó từ / Danh từ', role: 'Nhấn mạnh thực chất', meaning: 'Thực ra thì' },
      { word: '何か', furigana: 'なにか', pos: 'Đại từ nghi vấn + か', role: 'Nội dung câu hỏi', meaning: 'Là cái gì' },
      { word: 'ご承知ですか', furigana: 'ごしょうちですか', pos: 'Kính ngữ + ですか', role: 'Câu hỏi lịch sự', meaning: 'Các em có biết không?' }
    ]
  },
  sent3: {
    id: 'sent3',
    level: 'N3',
    jp: 'ジョバンニは、あの川の向こうの光がみんな星だということを、いつか本で読んだのを覚えていたのです。',
    vn: 'Giovanni vẫn nhớ rõ ràng rằng mình từng đọc trong cuốn sách nào đó rằng tất cả ánh sáng bên kia dòng sông ấy đều là những vì sao.',
    grammarNote: `
      <strong>💡 Điểm ngữ pháp cốt lõi:</strong><br>
      • <strong>〜ということ:</strong> Danh từ hóa toàn bộ mệnh đề nội dung ("việc tất cả ánh sáng bên kia sông là sao").<br>
      • <strong>〜のを覚えている:</strong> Trợ từ の dùng để danh từ hóa hành động đọc trong quá khứ làm tân ngữ cho động từ 覚えている (vẫn còn nhớ).<br>
      • <strong>〜のです (のだ):</strong> Cấu trúc giải thích nguyên nhân, lý do ngầm cho hành động chần chừ giơ tay của Giovanni.
    `,
    tokens: [
      { word: 'ジョバンニは', furigana: 'Jobanni wa', pos: 'Danh từ riêng + は', role: 'Chủ ngữ câu', meaning: 'Giovanni thì' },
      { word: 'あの川', furigana: 'ano kawa', pos: 'Chỉ định từ + Danh từ', role: 'Đối tượng ở xa', meaning: 'Dòng sông kia' },
      { word: '向こうの光', furigana: 'むこうのひかり', pos: 'Cụm danh từ', role: 'Vị trí ánh sáng', meaning: 'Ánh sáng phía bên kia' },
      { word: 'みんな星だ', furigana: 'みんなほしだ', pos: 'Phó từ + Danh từ + だ', role: 'Mệnh đề khẳng định', meaning: 'Tất cả đều là vì sao' },
      { word: 'ということ', furigana: 'to iu koto', pos: 'Mệnh đề danh từ', role: 'Liên kết nội dung trích dẫn', meaning: 'Rằng là...' },
      { word: '本で読んだの', furigana: 'ほんでよんだの', pos: 'Động từ + の', role: 'Ký ức đọc sách', meaning: 'Việc từng đọc trong sách' },
      { word: '覚えていたのです', furigana: 'おぼえていたのです', pos: 'Động từ tiếp diễn + のです', role: 'Vị ngữ giải thích', meaning: 'Đã nhớ rõ như vậy' }
    ]
  }
};

// Visual Novel Word Dictionary
const vnWordDictionary = {
  'カムパネルラ': { reading: 'かむぱねるら', meaning: 'Campanella (Tên người bạn thân thiết của Giovanni trong hành trình Ngân Hà)', jlpt: 'N3', romaji: 'kamupanerura' },
  '見てごらん': { reading: 'みてごらん', meaning: 'Nhìn xem này (thể nhẹ nhàng, thân mật của 〜てごらんなさい)', jlpt: 'N4', romaji: 'mitegoran' },
  '川': { reading: 'かわ', meaning: 'Dòng sông, con sông (chỉ dải Ngân Hà tựa dòng sông ánh sáng)', jlpt: 'N5', romaji: 'kawa' },
  '向こう': { reading: 'むこう', meaning: 'Phía bên kia, đằng kia', jlpt: 'N4', romaji: 'mukou' },
  '光': { reading: 'ひかり', meaning: 'Ánh sáng, quầng sáng lung linh', jlpt: 'N4', romaji: 'hikari' },
  '白く': { reading: 'しろく', meaning: 'Màu trắng, trắng xóa', jlpt: 'N5', romaji: 'shiroku' },
  'けむった': { reading: 'けむった', meaning: 'Mờ ảo như phủ làn khói/sương lam', jlpt: 'N2', romaji: 'kemutta' },
  '銀河': { reading: 'ぎんが', meaning: 'Dải Ngân Hà (Milky Way Galaxy)', jlpt: 'N2', romaji: 'ginga' },
  '帯': { reading: 'おび', meaning: 'Dải, vệt, vành đai ánh sáng', jlpt: 'N2', romaji: 'obi' },
  '本当': { reading: 'ほんとう', meaning: 'Sự thật, thực tế, đúng thật', jlpt: 'N4', romaji: 'hontou' },
  'ジョバンニ': { reading: 'じょばんに', meaning: 'Giovanni (Nhân vật chính cậu bé xưởng in)', jlpt: 'N3', romaji: 'jobanni' },
  '先生': { reading: 'せんせい', meaning: 'Thầy giáo, giáo viên', jlpt: 'N5', romaji: 'sensei' },
  '午後': { reading: 'ごご', meaning: 'Buổi chiều (PM)', jlpt: 'N5', romaji: 'gogo' },
  '授業': { reading: 'じゅぎょう', meaning: 'Tiết học, giờ học trên lớp', jlpt: 'N4', romaji: 'jugyou' },
  '黒板': { reading: 'こくばん', meaning: 'Bảng đen', jlpt: 'N5', romaji: 'kokuban' },
  '吊した': { reading: 'つるした', meaning: 'Treo lên (quá khứ của động từ 吊す)', jlpt: 'N3', romaji: 'tsurushita' },
  '図': { reading: 'ず', meaning: 'Bản đồ, biểu đồ hình vẽ', jlpt: 'N3', romaji: 'zu' },
  'まったく': { reading: 'まったく', meaning: 'Hoàn toàn, y hệt không khác chút nào', jlpt: 'N4', romaji: 'mattaku' },
  '同じ': { reading: 'おなじ', meaning: 'Giống nhau, tương tự', jlpt: 'N5', romaji: 'onaji' },
  '一緒': { reading: 'いっしょ', meaning: 'Cùng nhau, chung bước hành trình', jlpt: 'N5', romaji: 'issho' },
  '幸い': { reading: 'さいわい', meaning: 'Hạnh phúc, điều tốt lành, phúc lợi', jlpt: 'N2', romaji: 'saiwai' },
  '身体': { reading: 'からだ', meaning: 'Thân xác, cơ thể con người', jlpt: 'N4', romaji: 'karada' },
  '切符': { reading: 'きっぷ', meaning: 'Vé tàu, vé xe', jlpt: 'N5', romaji: 'kippu' },
  '拝見': { reading: 'はいけん', meaning: 'Xin phép xem (kính ngữ khiêm nhường ngữ)', jlpt: 'N3', romaji: 'haiken' },
  '天上': { reading: 'てんじょう', meaning: 'Cõi trời, thiên giới, nơi thanh khiết cao vời', jlpt: 'N2', romaji: 'tenjou' },
  '本物': { reading: 'ほんもの', meaning: 'Đồ thật, đích thực, không phải giả', jlpt: 'N3', romaji: 'honmono' },
  '特別': { reading: 'とくべつ', meaning: 'Đặc biệt, khác biệt', jlpt: 'N4', romaji: 'tokubetsu' },
  '王': { reading: 'おう', meaning: 'Quốc vương, bệ hạ', jlpt: 'N3', romaji: 'ou' },
  '処刑': { reading: 'しょけい', meaning: 'Xử tử, thi hành án tử', jlpt: 'N2', romaji: 'shokei' },
  '猶予': { reading: 'ゆうよ', meaning: 'Ân hạn, trì hoãn thời hạn', jlpt: 'N1', romaji: 'yuuyo' },
  '結婚式': { reading: 'けっこんしき', meaning: 'Lễ cưới, hôn lễ', jlpt: 'N4', romaji: 'kekkonshiki' },
  '親友': { reading: 'しんゆう', meaning: 'Bạn thân chí cốt', jlpt: 'N3', romaji: 'shinyuu' }
};


// Visual Novel In-Scene Challenges
const vnSceneChallenges = {
  ginga: {
    title: 'Thử thách đọc hiểu: Ga số 4 & Tấm vé Ngân Hà',
    desc: 'Dựa vào cuộc gặp gỡ giữa hai cậu bé và Người soát vé trên chuyến tàu sao:',
    question: 'Theo lời người soát vé (車掌), tấm vé của hai cậu bé có đặc điểm gì phi thường?',
    options: [
      { text: 'Chỉ có thể đi đến một ga duy nhất ven dòng sông', correct: false },
      { text: 'Có thể đi tới tận thiên giới (天上) hay bất cứ nơi nào trong vũ trụ', correct: true },
      { text: 'Là vé giả bị rách một góc do nhặt được', correct: false },
      { text: 'Chỉ được dùng vào ban ngày khi có ánh mặt trời', correct: false }
    ],
    explanation: 'Người soát vé nhận xét: "坊っちゃん方の切符は、天上へでもどこへでも行ける本物の特別な切符ですね" (Vé của hai cậu bé là tấm vé thật đặc biệt, có thể đi tới tận thiên giới hay bất kỳ đâu).'
  },
  merosu: {
    title: 'Thử thách đọc hiểu: Lời thề bạn hữu của Melos',
    desc: 'Dựa vào cuộc đối chất giữa Melos và bạo chúa Dionys:',
    question: 'Melos đã đưa ra điều kiện gì để được hoãn thi hành án tử hình 3 ngày?',
    options: [
      { text: 'Nộp một khoản tiền chuộc lớn từ người giàu trong thành', correct: false },
      { text: 'Để người bạn thân nhất là thợ đá Selinuntius lại làm con tin', correct: true },
      { text: 'Hứa sẽ phục tùng vô điều kiện mệnh lệnh của nhà vua', correct: false },
      { text: 'Xây dựng một đền thờ mới cho Syracuse', correct: false }
    ],
    explanation: 'Melos tuyên bố: "私には親友がいる！ 石工のセリヌンティウスだ。彼を人質に残そう" (Ta có bạn thân là thợ đá Selinuntius, ta xin để chàng lại làm con tin).'
  }
};


// Real Authentic Japanese Chapter Texts Crawled from Syosetu
const crawledChapterProse = {
  "mushoku": {
    "title": "プロローグ",
    "content": "俺は34歳住所不定無職。\n\n人生を後悔している真っ最中の小太りブサメンのナイスガイだ。\n\nつい三時間ほど前までは住所不定ではない、\n\nただの引きこもりベテランニートだったのだが、\n\n気付いたら親が死んでおり、\n\n引きこもっていて親族会議に出席しなかった俺はいないものとして扱われ、\n\n兄弟たちの奸計にハマり、見事に家を追い出された。\n\n床ドンと壁ドンをマスターし、\n\n家で傍若無人に振舞っていた俺に味方はいなかった。\n\n葬式当日、ブリッヂオ○ニー中にいきなり喪服姿の兄弟姉妹たちに部屋に乱入され、絶縁状を突きつけられた。\n\n無視すると、弟が木製バットで命よりも大切なパソコンを破壊しやがった。\n\n半狂乱で暴れてみたものの、兄は空手の有段者で、逆にぼっこぼこにされた。\n\n無様に泣きじゃくって事無きをえようとしたら、着の身着のまま家から叩き出された。\n\nズキズキと痛む脇腹（多分肋骨が折れてる）を抑えながら、とぼとぼと町を歩く。\n\n家を後にした時の、兄弟たちの罵詈雑言が未だ耳に残っている。\n\n聞くに堪えない暴言だ。\n\n心は完璧に折れていた。\n\n俺が一体なにをしたっていうんだ。\n\n親の葬式をブッチして無修正ロリ画像（兄の娘を風呂に入れた時にデジカメで撮りました）でオ○ってただけじゃないか……。\n\nこれからどうしよう。\n\nいや、頭ではわかっている。\n\nバイトか何かを探して、住む場所を見つけて、食べ物を買うのだ。\n\nどうやって？\n\n仕事を探す方法がわからない。\n\nいや、なんとなくだが、ハロワにいけばいいということはわかる。\n\nが、伊達に十年以上引きこもっていたわけじゃない。\n\nハロワの場所なんかわかるわけもなし。\n\nそれに、ハロワにいっても仕事を紹介されるだけだと聞いたことがある。\n\n紹介された所に履歴書を持っていき、面接をうけるわけだ。\n\nこの、エッチな液体で袖とかカピカピなって、ところどころに血が付いた服で面接を？\n\n受かるわけがない。\n\n俺だったらこんなクレイジーな格好した奴は絶対に採用しない。\n\n共感は覚えるかもしれないが、絶対に採用はしない。\n\nそもそも履歴書の売っている店もわからない。\n\n文房具屋か？\n\nコンビニか？\n\nコンビニぐらいは歩いてればあるかもしれないが、金は持っていない。\n\nもし、それらがクリアできたとしよう。\n\n運よく金融機関か何かで金を借りることが出来て、服を新調して、履歴書と筆記用具を買ったとしよう。\n\n履歴書というものは住所が無いと書けない、と聞いたことがある。\n\n詰んだ。\n\nここにきて、俺は人生が完全に詰んだのを自覚した。\n\n「………はぁ」\n\n雨が降ってきた。\n\nもう夏も終わり、肌寒くなってくる時期だ。\n\n冷たい雨は何年も着古したスウェットに難なく染みこみ、容赦なく体温を奪った。\n\n「………やりなおせればな」\n\n思わずそんな言葉が溢れる。\n\n俺だって、生まれた時からクズ人間だったわけじゃないのだ。\n\nそこそこ裕福な家庭の三男として生まれた。兄兄姉弟。５人兄弟の４番目。\n\n小学生の頃は、この歳にしては頭がいいと褒められて育った。\n\n勉強は得意じゃなかったが、ゲームがうまくて、運動もできるお調子者。\n\nクラスの中心だった。\n\n中学時代にはパソコン部に入り、雑誌を参考に、お小遣いを貯めて自作PCを作成。\n\nパソコンのパの字も知らなかった家族からは、一目も二目も置かれていた。\n\n人生が狂ったのは高校……いや、中学３年からだ。\n\nパソコンにかまける余りに、勉強をおろそかにした。\n\n勉強なんか、将来に必要ないと思っていた。役に立たないと思っていた。\n\nその結果、県内でも最底辺と噂の超絶バカ高校に入学するハメになった。\n\nそこでも、���はイケる気でいた。\n\nやればできる俺は、他の馬鹿どもとは出来がちがうんだと思っていた。\n\nあの時の事は、今でも覚えている。\n\n購買で昼食を買おうとして並んでいた時、いきなり横入りしてきた奴がいた。\n\n俺は正義感ぶってそいつに文句を言った。\n\n当時、変な自尊心と、中二病心あふれる性格をしていたためにやってしまった暴挙だ。\n\n相手は先輩で、この学校でも一、二を争うほど危ない奴だった。\n\n放課後、俺は顔が腫れ上がるまで殴られ、全裸で校門に磔にされた。\n\n写真もいっぱい取られた。\n\nもし俺が美少女だったら、\n\nさんざんレイプされた挙句、写真を取られて脅されて性奴隷にでもされただろう。\n\n残念ながら、俺は小太りのキモオタだった。\n\nその時の写メは、いとも容易く学校中にバラまかれた。\n\n何の交渉もなく、ただ面白半分で。\n\nヒエラルキーは一瞬にして最下層に落ちて、ホーケーという仇名を付けられてからかわれた。\n\n一ヶ月も学校に通わないうちに不登校になって引きこもった。\n\n父や兄は、そんな俺を見て、\n\n勇気を出せだの、頑張れだのと無責任な言葉を投げつけた。\n\nどうしろと言うんだ。\n\nあんな状況で、誰が学校にいけると言うんだ。\n\n俺は引きこもった。\n\n断固として引きこもった。\n\n同年代の知り合いが、みんな俺の全裸磔と股間部のアップの写真を見て笑っていると思っていた。\n\nずっと引きこもってネトゲをやった。\n\nたまにP2Pソフトでエロゲーやエミュレータ、漫画を落としたりした。\n\nネットとパソコンがあれば、時間はいくらでも潰せた。\n\nネットで影響を受けて、色んな事に興味を持ち、色んな事をやった。\n\nプラモを作ったり、フィギュアを塗装してみたり、ブログをやってみたり。\n\n母はそんな俺を応援するがごとく、ねだればいくらでも金を出してくれた。\n\nが、どれも一年以内には飽きた。\n\n自分より上の人間を見て、やる気が失せたのだ。\n\n傍から見れば、ただ遊んでいるだけに見えただろう。\n\nけれど、一人だけ時間に取り残され、暗い殻に閉じこもってしまった俺には、他に出来る事がなかった。\n\nいいや、今にして思えば、そんなのは言い訳だ。\n\nただ遊んでいただけだ。\n\nまだしも、漫画家になると言い出してヘタクソなWEB漫画を開始したり、\n\nラノベ作家になると言い出して小説を投稿してみたほうがマシだったろう。\n\n俺と似たような境遇でそうしている奴はたくさんいた。\n\nそんな奴らを、俺は馬鹿にした。\n\n彼らの創作物を見て鼻で笑って、「クソ以下だな」と評論家気取りで批判していただけだ。\n\n自分は何もやっていないのに……。\n\n戻りたい。\n\n出来れば最高だった小学か、中学時代に。\n\nいや、一年でも二年でもいい。\n\nちょっとでも時間があれば、俺には何かができたはずなんだ。\n\nどれも中途半端でやめたから、どれも途中から始められる。\n\n本気を出せば、一番にはなれなくても、プロにはなれたかもしれない。\n\nいや、よそう。\n\n無駄だ。\n\n無駄無駄。\n\nこんなことを考えるのは無駄なのだ。\n\n「ん？」\n\n激しい雨の中、俺は誰かの言い争う声を聞いた。\n\n喧嘩だろうか。\n\n嫌だな、かかわり合いになりたくないな。\n\nそう思いつつも、足はまっすぐにそちらに向かっていた。\n\n「――だから、あんたが――」\n\n「おまえこそ――」\n\n見つけたのは、痴話喧嘩の真っ最中っぽい三人の高校生だ。\n\n男二人に女が一人。\n\nいまどき珍しいことに、詰襟とセーラー服。\n\nどうやら修羅場らしく、一際背の高い少年と少女が何かを言い争っていた。\n\nもう一人の少年が、二人を落ち着かせようと間に入っているが、\n\n喧嘩中の二人は聞く耳を持たない。\n\n（ああ、俺にもあったな、あんなの）\n\n中学時代には、そこそこ可愛い幼馴染がいた。\n\nそこそこ可愛いといっても、クラスで４番目か５番目ぐらい。\n\n陸上部だったので髪型はベリィショート。\n\n町を歩いて１０人とすれ違ったら、二人か三人ぐらいは振り返るかな、そんな容貌だ。\n\n当時の俺は完全に２次元にハマっていた。\n\n陸上部といえばポニテと言って憚らなかった。\n\nそんな俺にとって、彼女はブスもいい所だった。\n\nけど、家も近く、小中と同じクラスになる事も多かったので、\n\n会話をする機会は多かったし、口喧嘩をしたりしたものだ。\n\n中学になっても、何度か一緒に帰ったりもした。\n\n惜しいことをしたもんだ。\n\n今の俺なら、中学生・幼馴染・陸上部、それらの単語だけで３発はイケる。\n\nちなみに、その幼馴染は七年前に結婚したらしいと風の噂で聞いた。\n\n風の噂たって、リビングから聞こえてきた兄弟の会話だが。\n\n決して悪い関係じゃなかった。\n\nお互いを小さい頃から知っていたから、気兼ねなく話せていた。\n\n彼女が俺に惚れていたとかは無かったと思うが、\n\nもっと勉強してあの子と同じ高校に入っていれば、\n\nあるいは、同じ陸上部に入って推薦入学でもしていれば、\n\nフラグの一つも立ったかもしれない。\n\n本気で告白すれば、付き合う事ぐらいは出来たかもしれない。\n\nそして、放課後に誰もいない教室でエロいことをしたり、\n\n彼らのように、帰り道で喧嘩したりするのだ。\n\nまさにエロゲーの世界。\n\n（そう考えるとあいつらマジリア充だな。爆発し……ん？）\n\nと、俺はその瞬間に気付いた。\n\nトラックが一台、三人に向かって猛スピードで突っ込んできているのを。\n\n同時に、トラックの運転手がハンドルに突っ伏しているのを。\n\n居眠り運転。\n\n三人はまだ気づいていない。\n\n！！！！！\n\n「ぁ、ぁ、ぶ、危ねぇ、ぞぉ」\n\n叫んだつもりだったが、十年以上もロクに声を出していなかった俺の声帯は、\n\n肋骨の痛みと雨の冷たさでさらに縮こまり、\n\n情けなくも震えた声しか発せず、雨音にかき消された。\n\n助けなきゃ、と思った。\n\n俺が、なんで、とも思った。\n\nもし助けなければ、五分後にきっと後悔するんだろうと直感した。\n\n凄まじい速度で突っ込んでくるトラックにハネられ、\n\nぐちゃぐちゃに潰れる三人を見て、後悔するんだろうと直感した。\n\n助けておけばよかった、と。\n\nだから助けなきゃ、と思った。\n\n俺はもうすぐ、きっとどこかそのへんで野垂れ死ぬだろうけど、\n\nその瞬間ぐらいは、せめてささやかな満足感を得ていたいと思っていた。\n\n最後の瞬間まで後悔していたくないと思った。\n\n転げるように走った。\n\n十数年以上もロクに動いていなかった俺の足はいうことを聞かない。\n\nもっと運動をしておけばと、生まれて初めて思った。\n\n折れた肋骨が凄まじい痛みを発し、俺の足を止めようとする。\n\nもっとカルシウムを取っておけばと、生まれて初めて思った。\n\n痛い。\n\n痛くてうまく走れない。\n\nけれども走った。\n\n走った。\n\n走れた。\n\nトラックが目の前に迫っているのに気づいて、喧嘩していた少年が少女を抱き寄せた。\n\nもう一人の少年は、後ろを向いていたため、まだトラックに気づいていない。\n\n唐突にそんな行動にでた事に、きょとんとしている。\n\n俺は迷わず、まだ気づいていない少年の襟首を掴んで、渾身の力で後ろに引っ張った。\n\n少年は体重１００キロの俺に引っ張られ、トラックの進路の外へと転がった。\n\nよし。\n\nあと二人。\n\nそう思った瞬間、俺の目の前にトラックがいた。\n\n安全な所から、腕だけ伸ばして引っ張ろうと思ったのだが、\n\n人を引っ張れば、反作用で自分が前に出る。\n\n当然のことだ。\n\n俺の体重が１００キロを超えていようと関係ない。\n\n全力疾走でガクガクしていた足は、簡単に前に出てしまった。\n\nトラックに接触する瞬間、何かが後ろで光った気がした。\n\nあれが噂の走馬灯だろうか。一瞬すぎてわからなかった。\n\n早すぎる。\n\n中身の薄い人生だったという事か。\n\n俺は自分の五十倍以上の重量を持つトラックに跳ね飛ばされ、コンクリートの外壁に体を打ち付けた。\n\n「かッハ……！」\n\n肺の中の空気が一瞬で吐き出される。\n\n全力疾走で酸素を求める肺が痙攣する。\n\n声も出ない。\n\nだが、死んではいない。\n\nたっぷりと蓄えた脂肪のおかげで助かった……。\n\nと思ったが、トラックはまだ迫ってきていた。\n\n俺はトラックとコンクリートに挟まれて、トマトみたいに潰れて死んだ。"
  },
  "rezero": {
    "title": "プロローグ　『始まりの余熱』",
    "content": "――これは本気でヤバい。\n\n固い地べたの感触を顔面に味わい、彼は自分がうつ伏せに倒れたのだと気付いた。\n\n全身に力が入らず、手先の感覚はすでにない。\n\nただ、喉をかきむしりたくなるほどの熱が体の真ん中を支配している。\n\n――熱い、熱い、熱い、熱い、熱い、熱い、熱い。\n\n叫び声を上げようと口を開いた瞬間、こぼれ出たのは絶叫ではなく血塊だ。\n\nせき込み、喉からこみ上げる命の源を思うさまに吐き出す。ごぼごぼと、口の端を血泡が浮かぶほどの吐血。ぼんやりとした視界に、真っ赤に染まった地面が見える。\n\n――ああ、これ全部、俺の血かよ。\n\n倒れる体が浸るほどの出血。人間の血の量は全体の約８％、そのうちの三分の一が流れ出すと命に関わるという話だが――これはもう、全部出ているんじゃなかろうか。\n\n口からの吐血は打ち止めだが、体を焼き尽くすような『熱』の原因はいまだに活動中。かろうじて動いた手が腹部に向かい、そこにあり得ない感触を得て、納得がいく。\n\n――なんだ、腹が破けてんのかよ。\n\nどうりで熱いと感じるわけだ。『痛み』を『熱』と錯覚しているらしい。\n\n鋭い裂傷は胴体をほぼ真っ二つに通り抜けて、腰の皮一枚で繋がっている状態だ。\n\nつまるところ、どうやら人生の『詰み』というやつに直面したようだ。\n\n理解した瞬間に急速に意識が遠のいていく。\n\nさっきまでのた打ち回るのを強要していた『熱』すらどこかへ消え去り、不快な血の感触も内臓に触れる手の感覚も、全ては遠ざかる意識の付き添いとして連れていかれる。\n\n置いてけぼりにされるのは、『魂』の同行を拒否された肉体だけだ。\n\nその肉体を、消える意識からの最後の働きかけで少しだけ動かす。首を、上に向けて。\n\n眼前、鮮血の絨毯を敷き詰めた床を、黒い靴が波紋を生みながら踏みつける。\n\n誰かがいるのだ。そしてその誰かがおそらく、自分を殺したのだろう。\n\n不思議と、その相手の顔を拝んでやろうという気にはならなかった。\n\n自分を殺すような相手、そんな相手にすら傍観を決め込むほど日和見主義だった記憶はないのだが、心はその相手の素姓など欠片も興味を払っていない。\n\nただ願ったのは――彼女が無事でありますように、ということだけだった。\n\n「――バル？」\n\n鈴の音のような声が聞こえた気がする。\n\nどこが耳でどこが鼻かもわからない状態だから、空耳の可能性の方が高い。\n\nそれなのに、記憶を頼りに再現したのだとしても、その声はひどく心地よく感情を揺さぶる。\n\nだから――、\n\n「――っ！」\n\n短い悲鳴が上がって、血の絨毯が新たな参加者を歓迎する。\n\n倒れ込んだ体はすぐ傍らに、そしてそこにはだらしなく伸びた自分の腕があった。\n\n力なく落ちたその白い手と、血まみれの自分の手が絡む。\n\n全ては偶然だったのだろう。\n\nかすかに動いた指先が、自分の手を握り返したような気がした。\n\n「……っていろ」\n\n遠ざかる意識の首根っこを引っ掴み、無理やりに振り向かせて時間を稼ぐ。\n\n『痛み』も『熱』も全ては遠く、無駄な足掻きの負け犬の遠吠えだ。\n\nだが、それでも――、\n\n「俺が、必ず――」\n\n――お前を、救ってみせる。\n\n次の瞬間に彼――ナツキ・スバルは命を落とした。"
  },
  "slime": {
    "title": "死亡～そして転生～",
    "content": "何ということもない普通の人生。\n\n大学を出て一応大手と言われるゼネコンに入社し、現在一人暮らしの37歳。彼女はいない。\n\n年の離れた兄が両親を養っており、俺は気ままな独身貴族という訳だ。\n\n身長も低い訳ではなく、顔も悪い訳ではない。だけどモテない。彼女を作ろうと努力した事もあったが、３回告白してフラれた時点で心が折れた。まあ、この年になると彼女がどうのというのは正直面倒くさい。\n\n仕事が忙しいというのもあるが、別にいなくて困るというものでもないし。言い訳してる訳ではないよ？\n\nそんな事を何故考えていたかというと、\n\n「先輩。お待たせしました！」\n\n笑顔で俺に向かって歩いて来る、爽やかな青年。そして、その横に並ぶ美人さん。\n\n俺の後輩の田村と、会社のマドンナと名高い、受付の沢渡さんである。\n\nそう、今日はこいつらに、結婚するから相談に乗ってくれと頼まれたのだ。つい、何故自分はモテないのかなどと考えてしまった理由である。\n\n仕事帰りの待ち合わせ場所の交差点脇で、柱にもたれてつらつらともの思いにふけっていた訳だ。\n\n「おう。で、相談って何だ？」\n\n俺は沢渡さんに目礼しながら質問する。\n\n「どうも初めまして、沢渡美穂です。いつもお見かけしてますが、話すのは初めてですね。何だか緊張します。」\n\n緊張してるのは俺の方だっての！そもそも俺は女子と話すのが苦手なのだ。察しろっての！などと、内心でボヤく俺。\n\nそもそも、どう見ても恋愛に縁の無さそうな俺に持ってくる相談ではない。絶対に当てつけだろう、間違いない！\n\n「ども。三上悟です。緊張なんてしなくても大丈夫ですよ。\n\n沢渡さんは会社で有名だから、紹介されなくても知ってますよ。\n\n田村はたまたま同じ大学でして、会社の研修会で意気投合してね。それ以来の付き合いなんです。」\n\n「有名って何ですか！何か、変な噂でも流れてるんですか？」\n\n「ええ。○○部長と浮気してるとか、△△君とデートしてたとかね！」\n\nついからかい始めてしまった。軽いジョークのつもりだったのだが沢渡さん、顔真っ赤になって涙目になって可愛いわ。\n\n俺のジョークはデリカシーに欠けるしセンスないから、絶対やめとけとよく言われるんだが、つい言ってしまう。\n\nやはり、今回も失敗か。やっぱ俺、性格悪いな。\n\n田村が沢渡さんの肩を叩きながらとりなしてる。\n\nくそ、田村め！こういう状況はまさに、リア充爆発しろ！って叫ぶ場面だな。\n\n「先輩、それくらいにしてくださいよ！美穂もからかわれてるだけだって。」\n\n笑いながら取りなす田村。出来た後輩だ。\n\n嫌味がなく爽やかで、憎めないやつなのだ。\n\n田村はまだ28歳で、俺とはだいぶ年も離れてるのに、何故か馬があった。しょーがない、素直に祝福してやるか・・・。\n\n「すまんね、性格悪いもんでね。まあ、ここで話すのもなんだし、場所変えて飯でも食いながら話聞くわ。」\n\n妬んでても仕方ない。そう思って俺がそう言った時、\n\n「「「キャーーーーーーーーーー」」」\n\n悲鳴。混乱。\n\n何だ？何が起きてる？！\n\n「どけ！殺すぞ！！！」\n\nその声に振り向き、包丁と鞄を持った男が走ってくるのが見えた。\n\n悲鳴が聞こえる。男が向かってくる。手には包丁。包丁？その切っ先には・・・\n\n「田村ぁーーーーーー」\n\nドン！っと、俺は田村を突き飛ばし、\n\nドスッ！っと、俺の背中に焼けるような痛み。\n\n「邪魔すんなぁーーーー」\n\n叫びながら逃げていく男を眺めて、田村と沢渡さんの無事を確認する。\n\n田村が声にならない叫び声をあげながら駆け寄ってくる。\n\n沢渡さんは突然の事態に茫然自失になっているようだが、怪我はなさそうだ。良かった。\n\nそれにしても、背中が熱い。痛いとかそんな感覚通り越して、背中が熱い。\n\nなんだこれ？熱すぎる・・・勘弁して欲しい。\n\n《確認しました。対熱耐性獲得・・・成功しました》\n\nもしかして・・・刺されちゃった？\n\n刺されて死ぬとか、ないわぁ・・・\n\n《確認しました。刺突耐性獲得・・・成功しました。続けて、物理攻撃耐性獲得・・・成功しました》\n\n「先輩、血、血がでて・・・血が止まらないんですぅ」\n\nなんだ、うるさい奴だ。田村か。変な声が聞こえた気がしたが、田村ならしょーがない。\n\n血？そりゃ、出るよ。俺だって人間だ。刺されたら血くらい出るさ！\n\nしかし、痛いのはかなわんな・・・。\n\n《確認しました。痛覚無効獲得・・・成功しました》\n\nえっと・・・やばい、俺も痛みと焦りで意識が混乱しているようだ。\n\n「た、田村・・・ウルサイぞ。た、大した事ないだろ？心配すんな・・・」\n\n「先輩、血、血が・・・」\n\n真っ青な顔で泣きじゃくりそうな顔して、俺を抱えようとする田村。男前が台無しだな。\n\n沢渡さんの様子を見ようとしたが、視界が霞んでよく見えない。\n\n背中の熱さが感じられなくなり、かわりに猛烈な寒気が俺を襲った。\n\nやばいかもわからんね・・・。人は血液が足りないと死ぬんだっけか。\n\n《確認しました。血液が不要な身体を作成します・・・成功しました》\n\n（ちょ、お前、さっきから何言ってるんだ？よく聞き取れない・・・）\n\n声を出そうとして、出なかった。やばい。本当に俺、死ぬかも・・・\n\nてか、だんだん熱さも痛みも感じなくなってきた。\n\n寒いのだ。寒くてどうしようもない。何てことだ・・・寒さで凍えるとか、俺も忙しいな。\n\n《確認しました。対寒耐性獲得・・・成功しました。\n\n対熱耐寒耐性を獲得した事により、『熱変動耐性ex』にスキルが進化しました》\n\nその時、俺の死にかけの脳細胞が、閃のように重要な事柄を思い出す。\n\nそうだ！PCのハードディスクの中身！！！\n\n「田村ぁ！！！万が一、万が一だが、俺が死んだら・・・俺のPCを頼む。\n\n風呂に沈めて、電気流して、データを完全に消去してやってくれ・・・」\n\n俺は、最後の気力を振り絞って、最重要事項を伝えた。\n\n《確認しました。電流によるデータの消去・・・情報不足により実行不能。失敗しました。\n\n代行措置として、電流耐性獲得・・・成功しました。付属して、麻痺耐性獲得・・・成功しました》\n\n田村は一瞬何を言われたのかわからなかったのか、きょとんとした顔をした。\n\nしかし、言われた意味を理解すると、\n\n「ははっ、先輩らしいですね・・・」\n\nそう言って、苦笑を浮かべた。男の泣き顔なんてみたくないしな。苦笑いでも、泣き顔よかマシだ。\n\n「俺、本当は、沢渡の事、先輩に自慢したくて・・・」\n\nそうだろうと思ったよ・・・。まったく、この野郎は。\n\n「ちっ・・・、たく。全部許してやるから、彼女の事、幸せにしてやれよ。PC頼んだぞ・・・」\n\n最後の力で、それだけを伝えた。\n\nあっけなく、三上悟は死んだ。\n\nだがこの時、三上悟の&quot;魂&quot;は、異なる世界の同一時空に偶然発生した魔物とリンクしたのだ。\n\n目視も出来ない、小さな次元の亀裂。発生した魔素の塊に、リンクした魂。\n\n魔素の塊は、魔物を生み出す元となり、リンクした三上悟の意思に基づき、その身体を作成する。\n\n本来有り得ぬ天文学的確率で、三上悟は、異なる世界の魔物として転生する事となる。\n\n何ということもない普通の人生。\n\n大学を出て一応大手と言われるゼネコンに入社し、現在一人暮らしの37歳。彼女はいない。\n\n年の離れた兄が両親を養っており、俺は気ままな独身貴族だった。\n\nおかげで、童貞。\n\nまさか、未使用であの世に旅立つ事になるとは・・・俺の息子も泣いてるだろう。\n\nすまんな、お前を大人にしてやれなくて・・・\n\n次生まれ変わる事が出来たら、ガンガン攻めよう。声かけまくって、喰いまくるぞ・・・。ってそれは駄目か。\n\n《確認しました。ユニークスキル『捕食者』を獲得・・・成功しました》\n\nそして40歳目前の俺なんて、30歳童貞で魔法使いならもうすぐ賢者だったのに・・・大賢者も夢じゃないが、流石にそこまではどうかと思うけど。\n\n《確認しました。エクストラスキル『賢者』を獲得・・・成功しました。\n\n続けて、エクストラスキル『賢者』をユニークスキル『大賢者』に進化させます・・・成功しました》\n\n・・・って、さっきから何だ、何が、《ユニークスキル『大賢者』》だ。舐めてるのか？\n\n全然ユニークなんかじゃねーよ！\n\n笑えないよ、こっちわ！\n\n本当に失礼な・・・\n\nそんな事を考えながら、俺は眠りについた。\n\n（これが死ぬって事か・・・思ったほど、寂しくないな。）\n\nそれが、三上悟が、この世で思った最後の言葉だった。"
  },
  "honzuki": {
    "title": "プロローグ",
    "content": "本須　麗乃（もとすうらの）、22歳。\n\nわたしは本が好きだ。大好きだ。\n\n三度のご飯より愛してる。\n\n活字を通して誰かの知識に触れるのが好きだし、妄想に触れるのも好き。そこから筆者の思想をたどるのも心が踊って顔が思わずニヨニヨしてしまう。\n\n様々な知識が一冊にまとめられている本を読むと、とても得をした気分になれるし、自分がこの目で見たことがない世界を、本屋や図書館に並ぶ写真集を通して見るのも、世界が広がっていくようで陶酔できると思わない？\n\n外国の古い物語だって、違う時代の、違う国の風習が垣間見えて趣深いし、あらゆる分野において歴史があり、それを紐解いていけば、時間を忘れるなんていつものこと。\n\n心理学、宗教、歴史、地理、教育学、民俗学、数学、物理、地学、化学、生物学、芸術、体育、言語、物語……人類の積もり積もった知識がぎっちり詰め込まれた本を心の底から愛している。\n\nぶ厚く連なる百科事典も、\n\n一冊も欠けることなく揃った文学全集も、\n\n一見簡素に見える表紙でも内容は高度な専門誌も、\n\n写真を多用したカラフルな雑誌も、\n\n小難しい言い回しが多い小説も、\n\n内容は薄くてもよく売れているライトノベルも、\n\n子供向けの大きな絵本も、\n\n日本の誇る文化になっている漫画も、\n\n素人が作った同人誌さえ、\n\nパラリとページを開けば、わたしを酔わせる美酒となる。\n\nそれから、図書館の古い本が集められた書庫の古い本独特の少々黴臭い匂いや埃っぽい匂いが好き。ゆっくりと吸い込み、そこに年を経た本があるだけで絶頂を迎えられそうなほど身悶える。\n\nもちろん、新しい紙とインクの匂いもたまらない。そこに何が書かれているのか、新しい知識があるか、考えるだけで楽しくなれる。\n\n一生を本に囲まれて生きていきたい。\n\n本を傷めないように日光が当たらないように、しかし、風通しは良いように作られた書庫で、できることなら一生過ごしたい。\n\n本を読むことにできる限りの時間を費やすことで、肌が青白くて薄気味悪いと言われようと、運動不足で不健康と言われようと、食事を忘れて叱られようと本を手放すことなんてできない。\n\nどうせ死ぬなら、本に埋もれて死にたいと思う。畳の上で往生するより、ベッドの上で本に埋もれて死ぬ方がよほど幸せな死に方だと思う。\n\n――いえ、正確には、思っていた。\n\n実は、わたし！\n\nついさっき、大地震で本に埋もれて死にました！\n\nえぇ、まさに、希望通りですが、何か？\n\n本望だったけれど、神様ありがとうと素直に思えないのは、司書資格を取って、就職難のこの時代に大学図書館への就職が決まったばかりだったから。\n\n神様、お願いします。\n\nできれば、転生させてください。\n\nまだ読み足りない。\n\n次の生でも、本が読みたい。\n\nそれから、図書館の司書になりたい。\n\n一日中、本に囲まれて過ごしたい。\n\nもちろん、司書の仕事は本を読むことじゃない。仕事していたら、本なんて読めない。わかっている。\n\nでも、他の仕事より、本に囲まれている時間は圧倒的に長いじゃない。本に囲まれているだけで、幸せだもの。\n\n紙とインクの匂いを吸い込むだけでうっとりする気持ち、誰にだってわかるでしょ？\n\n知識を伝達するために文字を残すなんて、人間だけが行う知的な作業が昔からずっと行われてきたなんて、歴史をちょっと振り返ってみれば、絶対心ときめくでしょ？\n\n本が読めればそれでいい。\n\nさぁ、神様。わたしの願いがわかったら、とっとと転生させてください。\n\n生まれ変わって、また、本を読むんだから。"
  },
  "kumabear": {
    "title": "0　ユナのスキル　（ネタバレ注意）",
    "content": "※　書籍版を基本にするため、設定が変わる場合があります。ご了承ください。\n\nアイテム名　クマセット\n\n右手　黒クマの手袋（譲渡不可）\n\n左手　白クマの手袋（譲渡不可）\n\n右足　黒クマの靴（譲渡不可）\n\n左足　白クマの靴（譲渡不可）\n\n服　白黒クマの服　（リバーシ機能あり、表と裏で色が違う。見た目着ぐる身）（譲渡不可）\n\n黒クマのてぶくろ\n\n攻撃の手袋、使い手のレベルによって威力アップ。\n\n白クマのてぶくろ\n\n防御の手袋、使い手のレベルによって防御アップ。\n\n黒クマのくつ　白クマのくつ\n\n使い手のレベルによって速度アップ。\n\n使い手のレベルによって長時間歩いても疲れない。\n\n黒クマの服（表）\n\n使い手のレベルによって物理、魔法の耐性がアップ。\n\n耐熱、耐寒付き。\n\n白クマの服（裏）\n\n着ていると体力、魔力が自動回復する。\n\n回復量は使い手のレベルに依存する。\n\n耐熱、耐寒付き。\n\n異世界言語（2話）\n\n異世界の言葉が日本語で聞こえる。\n\n日本語で話すと異世界の言葉として相手に伝わる。\n\n異世界文字（2話）\n\n異世界の文字が読める。\n\n日本語で書いた文字が異世界の文字になる。\n\nクマの異次元ボックス（2話）\n\n白クマの口は無限に広がる空間。どんな物も食べる（入れる）ことが出来る。\n\n但し、生きているものは食べる（入れる）ことは出来ない。\n\n入れている間は時間が止まる。\n\nクマの観察眼（7話）\n\nフードのクマの目は道具や武器の効果を見ることが出来る。\n\nフードを被らないと効果は発動しない。\n\nクマの探知（11話）\n\nクマの野生の力によって危険を察知する。（魔物や盗賊などの位置を把握することができる）\n\nレベルが上がったことにより、魔物種別が分かるようになった。（人類も含む）（52話）\n\nクマの召喚獣（20話）\n\nクマの手袋より召喚されるクマ。\n\n黒いクマはくまゆる。\n\n白いクマはくまきゅう。\n\nクマの地図（15話）\n\nクマの目が見た場所を地図として作ることが出来る。\n\nゲームの自動マッピングと同じもの。\n\nクマの転移門（41話）\n\n門を設置することによってお互いの門を行き来出来るようになる。\n\n3つ以上の門を設置する場合は行き先をイメージすることによって転移することが出来る。\n\nこの門はクマの手袋ではないと開けることは出来ない。\n\nクマフォン（83話）\n\nクマの形をした通信機。何個でも作れる。\n\n魔力登録をするため、本人しか使えない。\n\n音「くぅーん、くぅーん、くぅーん、くぅーん、くぅーん」\n\n通話する方法、かけたい相手をイメージする。\n\n受け取る側、持ち主本人が魔力を流す。それがスイッチ代わりになっている。\n\n召喚獣こぐま化（83話）\n\n言葉通り、召喚獣のくまゆるとくまきゅうのこぐま化だ。\n\n主に、宿屋などに泊まるときにボディーガードとして使われる。\n\n※知っている人物（フィナ、ノア、シュリ、孤児院の子供、国王、王妃、エレローラ、フローラ）\n\nクマの水上歩行（212話）\n\n水の上を移動することが可能になる。\n\n召喚獣は水の上を移動することが可能になる。\n\nクマの念話（227話）\n\n離れている召喚獣に呼びかけることができる\n\nクマの水中遊泳（483話）　初使用（526話）\n\nクマの形をしている空気玉の中に入って、水中の中に入ることができる。\n\n時間は未知数。\n\nクマの魔法陣（魔力で魔法陣を生成できる）（753話）\n\nクマの魔法強化の魔法陣\n\nクマの契約魔法の魔法陣。\n\nクマの魔法強化の魔法陣（753話）\n\n魔法陣の中にいると魔法効果が強化される。（一文の下にクマの顔が描かれている）\n\n●魔法関係\n\nクマのライト（8話）\n\nクマの手袋に集まった魔力によって、クマの形をした光を生み出す。\n\nクマの身体強化（10話）\n\nクマの装備に魔力を通すことで身体強化を行うことができる。\n\nクマの火属性魔法（10話）\n\nクマの手袋に集まった魔力により、火属性の魔法を使うことが出来る。\n\n威力は魔力、イメージに比例する。\n\nクマをイメージするとさらに威力が上がる。\n\nクマの水属性魔法（10話）\n\nクマの手袋に集まった魔力により、水属性の魔法を使うことが出来る。\n\n威力は魔力、イメージに比例する。\n\nクマをイメージするとさらに威力が上がる。\n\nクマの風属性魔法（10話）\n\nクマの手袋に集まった魔力により、風属性の魔法を使うことが出来る。\n\n威力は魔力、イメージに比例する。\n\nクマをイメージするとさらに威力が上がる。\n\nクマの地属性魔法（10話）\n\nクマの手袋に集まった魔力により、地属性の魔法を使うことが出来る。\n\n威力は魔力、イメージに比例する。\n\nクマをイメージするとさらに威力が上がる。\n\nクマの回復魔法\n\nキュア：毒、病原菌を消すことが出来る。（31話）\n\nヒール：体力を回復させる。疲労を無くす（31話）\n\nかすり傷程度なら治すことが出来る。大怪我は治せない（30話）\n\nクマの電撃魔法（145話）\n\nクマの手袋に集まった魔力により、雷属性の魔法を使うことが出来る。\n\n威力は魔力、イメージに比例する。\n\nクマをイメージするとさらに威力が上がる。（228話）"
  }
};
