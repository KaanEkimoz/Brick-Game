# Brick Game: Retro Classic Game — Pazarlama Üretim Brief'i

> Bu dosya başka bir AI'a (ChatGPT, Gemini, Claude, Midjourney, DALL-E, Sora vb.) kopyala-yapıştır olarak verilebilir. Her bölüm bağımsız çalışır ya da hepsi birden brief olarak verilir. AI'a ne istediğini söylerken **"BÖLÜM X'i üret"** diye spesifik ol.

---

## 0) AI'A TALİMAT (en üste yapıştır)

Sen kıdemli bir mobil oyun pazarlama uzmanı + creative director'sın. Aşağıdaki oyun için pazarlama materyalleri üretmen istenecek. Önce **Bölüm 1**'i (Oyun Brief) tamamen oku ve içeri sindir, sonra hangi içeriği üretmen istendiyse o bölüme git.

İçerik üretirken:
- **Türkçe + İngilizce** iki versiyon üret (her ikisi de native, çeviri kokmasın)
- **Marka tonu**: nostaljik ama modern, retro ama tertemiz, samimi ama profesyonel — **"Tetris'in çocukluk hissi, 2026 zarafetiyle"**
- **Asla** klişe "Get ready to be amazed!" gibi cümleler kurma. Sıcak, gerçekçi, somut konuş.
- Görsel tariflerinde **boyut + kompozisyon + renk paleti + odak noktası + tipografi + duygu** ayrı ayrı belirt — direkt Midjourney/DALL-E promptu olarak kullanılabilsin.
- Türkçe metinlerde Türkiye'ye özgü hisleri kullan ("eski telefon oyunları", "harçlığını verirdik", "okul çıkışı arkadaşla").
- İngilizce metinlerde global retro-gaming nostalgia'sına hitap et ("the GameBoy era", "block-stacking ritual", "Eastern European arcade legacy").

---

## 1) OYUN BRIEF (Brick Game: Retro Classic Game)

### Temel kimlik
- **İsim:** Brick Game: Retro Classic Game
- **Geliştirici / Yayıncı:** Ekimoz Games (bağımsız stüdyo)
- **Platform:** Android (Google Play Store), şu an v1.3.x serisi yayında
- **Tür:** Casual puzzle / brick-stacking / klasik arcade — Tetris geleneğinde modern bir takipçi
- **Fiyat modeli:** Ücretsiz + reklam destekli + **opsiyonel "Reklamları Kaldır" IAP** ($1.99 global, ₺24.99 TR)
- **Hedef cihaz:** Mobil portrait (Android 7.1+, ARM64+ARMv7)

### Temel oynanış (Classic Mode)
- 7 standart tetromino parçası (I, O, T, S, Z, L, J) yukarıdan iner
- **7-bag dağıtım** — modern Tetris standardı, asla üst üste aynı parça gelmez
- Yatay satır doldurunca temizlenir, üst satırlar düşer
- Her 10 satır = +1 seviye, parça düşme hızı artar (Level 1: 0.75s → Level 15: 0.26s)
- Tap = döndür, swipe = hareket, hızlı aşağı swipe = hard drop, basılı tut = soft drop
- Hayalet parça (ghost piece) nereye düşeceğini gösterir

### YENİ ÖZELLİK — Extended Mode (USP)
Klasik mod sıkıcı geliyorsa **Extended Mode**:
- 4 satır temizledikçe **ability bar** dolar
- Dolduğunda sıradaki parça yerine **bomb** veya **laser** parçası gelir
- **Bomb**: 3×3 alan patlatır, etrafındaki bloklar yok olur, dramatik patlama efekti
- **Laser**: yatay veya dikey ışın atar, bir satır/sütun temizler, parça rengiyle renkli ışın efekti
- Strateji + güç hissi — klasik Tetris'in zen ritmine "şimşek anı" katar

### Diğer kilit özellikler
- **Tutorial**: animasyonlu el ikonlu kartlar, swipe-rotate-soft drop-hard drop-ability bar-bomb-laser adım adım öğretiyor. İlk açılışta gelir, sonra bir daha çıkmaz.
- **Hard drop streak efekti**: parça düştüğünde parça renginde (cyan I, kırmızı Z, vs.) yukarıdan aşağı doğru parlayan ışın çizgileri — fade out
- **Soft drop trail**: hafif basışta parça arkasından parlayan iz
- **Line clear flash + spark**: satır temizlendiğinde dramatik bir flash + particle patlaması
- **Hard drop shake**: ekran sertçe sallar, "ağırlık" hissi
- **Background music**: ambient retro synthwave + chill 8-bit (volume slider mevcut)
- **Pause + Continue save**: oyun kapansa bile bir sonraki açılışta kaldığın yerden devam (sadece classic, extended ayrı slot)
- **Settings**: ses, müzik, SFX toggle'ı, volume slider — temiz, tek sayfa
- **GameOver Restart + Main Menu** butonları — friction'sız tekrar oynama
- **DangerWarning**: board üstte dolmaya başlayınca ekran kenarı kırmızı titrer

### USP'ler (Unique Selling Points) — pazarlama bunlar üzerinden konuşmalı
1. **Tetris'in eski ruhu, modern arınmışlığı** — gereksiz özellikler yok, in-app store, daily mission, gacha yok. Sadece blok dizmek.
2. **Az reklam, opsiyonel kaldırma** — banner küçük + 90 saniye ad gap + interstitial sadece game over, ister isen $1.99'a hepsi kapanır
3. **Yeni Extended Mode** — Tetris klonlarında nadir görülen aksiyon: bomb + laser power-up'ları
4. **Görsel efekt detayı** — renkli streak'ler, particle flash, board shake — küçük ama hissedilir polish
5. **Türkçe** — yerel dil desteği, Türkiye pazarı için doğal
6. **Hafif, hızlı, offline** — internet olmadan çalışır, ~60MB AAB
7. **Bağımsız geliştirici samimiyeti** — büyük stüdyo gürültüsü yok, tek geliştirici tutkusu

### Hedef kitle
- **Birincil**: 25-45 yaş, Tetris'i çocukluğunda oynamış nostaljik mobil oyuncu (Türkiye + global EMEA)
- **İkincil**: 13-24 yaş Gen Z casual gamer, retro estetiğe sempati duyar (TikTok/Reels kanalından erişilir)
- **Davranış profili**: kısa molalar, otobüsteki 15 dk, akşam ekran açılınca 20 dk rahatlama — uzun grind oturma değil

### Karşı-pozisyonlama (ne DEĞİL'iz)
- **Hyper-casual değiliz** (15 saniyede sıkmıyor, derinlik var)
- **Pay-to-win değiliz** (Remove Ads sadece reklam, gameplay'i etkilemez)
- **Battle royale Tetris klonu değiliz** (multiplayer yok, focus single-player zen)
- **Reklam bombardımanı değiliz** (bunu marka mesajı olarak da kullan)

---

## 2) BÖLÜM A — PLAY STORE LISTING METİNLERİ

### A.1) App name (max 30 karakter)
**Üret:** TR + EN versiyon, hem SEO'yu (Tetris, brick, klasik) hem markayı koruyacak biçimde.

Mevcut: `Brick Game: Retro Classic Game`

Alternatifler önermen istenecek:
- "Brick Game" rekabet yüksek — yardımcı keyword ile differentiate
- Örnek: "BrickGame: Retro Neon Tetris" (klonu çağrıştırıyor — riskli, marka kontrolü yap)
- Türkçe için: "Brick Game: Retro Klasik Oyun" varyantı

### A.2) Short description (max 80 karakter)
Yüksek-impact, keyword-zengin, duygu uyandıran tek satır. Hem TR hem EN.

**Örnek tarz (referans, kopyalama):**
- EN: "Classic block puzzle with modern flair — abilities, neon FX, no spam ads."
- TR: "Klasik blok bulmacası, modern dokunuş — yetenekler, neon efektler, az reklam."

5 farklı TR + 5 farklı EN varyantı üret. Her birinin neye odaklandığını belirt (nostalji / yeni feature / az reklam / efekt / fiyat).

### A.3) Full description (max 4000 karakter)
Yapı:
1. **Hook** (ilk 200 karakter — kullanıcı "more" tıklamadan önce okuduğu kısım, çok kritik)
2. **Why this game** (oyunun ruh hâli, marka mesajı)
3. **Features list** (✦ veya • ile, ASO için keyword-zengin)
4. **What's new in v1.3.x** (Remove Ads, Extended Mode, abilities, polish)
5. **Why Ekimoz Games** (samimi bir cümle: "tek geliştirici, kahve eşliğinde 8 ayda yapıldı")
6. **Call to action** (ücretsiz indir, geri bildirim bekliyoruz)

TR + EN tam versiyonlar üret. Türkçesinde Türkiyeli oyuncuya hitap eden bir cümle olsun ("Eski telefon oyunlarının ruhunu bilenler hatırlar...").

### A.4) Keywords (her dil için 15-20 keyword)
Play Store'da ayrı keyword alanı yok ama description içine doğal yedirilecek. Hem TR hem EN üret. Örnek başlangıç:
- TR: tetris, brick, blok, klasik, retro, bulmaca, puzzle, türkçe, ücretsiz, offline, telefon oyunu, ekimoz, eski oyun, nostalji, neon
- EN: tetris, brick game, block puzzle, retro, arcade, classic, free, offline, no ads option, indie game, abilities, bomb, laser, neon, casual

---

## 3) BÖLÜM B — GÖRSEL ASSET TARİFLERİ

> Aşağıdaki her tarifi Midjourney / DALL-E / Stable Diffusion / Sora promptu olarak doğrudan kullanabilirsin. Her birinin **boyut + composition + color + lighting + reference + style** alanları var. AI'a verirken her görsel için ayrı prompt çalıştır.

### B.1) Feature graphic (1024×500 — Play Store banner)
**Concept**: Yatay banner, üst yarıda büyük "BRICK GAME" tipografisi (retro arcade font, gradient — cyan'dan magenta'ya), alt yarıda gameplay snapshot (parça düşüyor, line clear sparkles), sağ alt köşede "Available on Google Play" badge.

**Renk paleti**: 80'ler synthwave — neon cyan (#00E5FF), hot pink (#FF1F8F), deep purple (#3B0764) background, içinde grid pattern. Tetris parçaları orijinal renkleri (cyan I, sarı O, mor T, yeşil S, kırmızı Z, turuncu L, mavi J).

**Style references**: Synthwave/outrun album cover + retro game arcade flyer + minimalist 2026 mobile game marketing.

**Composition**: 1024 (genişlik) × 500 (yükseklik). Soldan sağa sol %40 tipografi + tagline, sağ %60 gameplay görsel. Asla overcrowded olmasın — Play Store'da küçük gösterilince okunabilir kalsın.

**Tagline option**: "Klasik Tetris ruhu, modern güç" / "Classic puzzle, modern power"

### B.2) Phone screenshots (1080×1920 portrait, 6 adet)

#### Screenshot 1: **Hero shot — Classic gameplay**
Board orta satırda, T parçası tam düşmek üzere, hayalet parça aşağıda, score "12,400", level 5. Üstte küçük bir tagline overlay: "Eski güzel günlerin oyunu, yeni nesil cilayla".

#### Screenshot 2: **Extended Mode — Ability bar full**
Board, ability bar tamamen dolu (neon mavi parlayan), next piece slotunda **bomb** parçası görünüyor. Overlay: "Yeni: Bomb + Laser yetenekleri"

#### Screenshot 3: **Bomb explosion mid-action**
Bomb parça boarda inmiş, 3×3 alan parlıyor, particle patlaması göz alıcı, etraf flash beyaz. Overlay: "Tetris klonlarında nadir: aksiyon"

#### Screenshot 4: **Line clear cascade**
4 satır aynı anda temizlenmek üzere, parça renginde streak'ler yukarı doğru fade, score sayacı animasyon halinde (+800). Overlay: "Saniyede 4 satırlık tatmin"

#### Screenshot 5: **Settings — Remove Ads CTA**
Settings paneli açık, "Reklamları Kaldır $1.99" butonu öne çıkıyor, volume slider yarıda, music toggle açık. Overlay: "Az reklam — istemezsen tek tıkla kapat"

#### Screenshot 6: **Tutorial card visible**
TutorialCanvas açık, "Tap to rotate" yazısı + animasyonlu el iconu, arkada başlangıç board'u. Overlay: "1 dakikalık öğrenme, ömür boyu eğlence"

**Stil notları (hepsi için ortak):**
- Çerçeve: ince neon glow (cyan veya magenta), 8px radius rounded corners
- Tagline overlay: alt %15 alana sabit, yarı saydam siyah blok + beyaz Inter/Poppins font 32-40pt
- Asla ekranın %70'inden fazlasını overlay metin kaplamasın — gameplay görünür kalsın
- Phone frame mockup KULLANMA (Play Store zaten ekranı telefon içinde gösterir, ekstra çerçeve gereksiz alan kaybı)

### B.3) Promo video (30 saniye, 1080×1920 portrait, MP4 H.264)
**Storyboard:**
- **0-3 sn**: Karanlık ekran, logo fade in (neon glow ile), background ambient synth note
- **3-8 sn**: Hızlı classic gameplay cut — 3-4 parça hızlı yerleştiriliyor, satır temizliyor, score atıyor
- **8-13 sn**: "YENİ" yazısı flash, Extended Mode geçişi, ability bar doluyor, bomb spawn, patlama
- **13-18 sn**: Laser parça, dramatik ışın efekti, ekrana açıyor, line clear cascade
- **18-23 sn**: Hard drop slow-motion + ekran shake + streak fade
- **23-27 sn**: Settings → "Reklamları Kaldır" butonuna tıklama, ekran "Ads removed" notification
- **27-30 sn**: Logo + "Brick Game: Retro Classic Game — Google Play'de" + Play Store badge

**Müzik**: Synthwave instrumental, 100-110 BPM, lo-fi feel, telif hakkı için Epidemic Sound veya Artlist'ten lisanslı kullan.

**Caption/subtitle**: TR + EN ayrı, alt %20 alanda küçük beyaz subtitle. Sessiz izleyiciye anlamlı olsun (TikTok/Reels otoplay sessizdir).

### B.4) Social media görselleri (her biri 1080×1080 square)
3 adet üret, hepsi Instagram + Twitter + Facebook uyumlu:
1. **Launch post**: Logo + "Now on Google Play" + üst kalitede 1 screenshot
2. **Feature announcement**: Extended Mode tanıtımı, bomb + laser görseli, "What's new in v1.3"
3. **Community CTA**: "Hangi parça en sevdiğin? 🟦🟥🟨" — engagement post, parçalar grid'de

### B.5) App icon refresh (opsiyonel — 512×512 PNG, alpha)
Mevcut iconu modernize etmek istersek:
- Square base, neon cyan→magenta gradient bg
- Merkezde 4 stilize tetromino parçası birleşmiş, küçük bir "spark" centerinde
- Köşelerde subtle glow halo
- Style: Pixel-perfect, 2026 mobil ikon trendi (Apple App Store / Material 3 friendly)

---

## 4) BÖLÜM C — SOSYAL MEDYA İÇERİĞİ

### C.1) TikTok / Reels / YouTube Shorts (5 farklı script, her biri 15-30 sn)

#### Script 1: "Tetris vs. Brick Game"
- 0-3sn: "Tetris'i severdin değil mi?" (gameplay sample)
- 3-8sn: "Şimdi düşün: Tetris + bomb + laser." (Extended Mode bomb explosion)
- 8-15sn: Full chaos gameplay clip
- 15-20sn: "Ücretsiz, az reklam, Türkçe. Brick Game."
- 20-23sn: Logo + Play Store CTA

#### Script 2: "Bir geliştiricinin hikayesi" (kişisel, samimi)
- "8 ay önce telefonumda Tetris oynarken düşündüm:" (geliştirici sesli)
- "Bu oyunu güncel haliyle nasıl yapardım?" (oyun screenshot'ları)
- "Çıkardığım şey bu" (final gameplay)
- "Ekimoz Games — tek kişi, kahve, ve çocukluk anısı"

#### Script 3: "Bomb + Laser combo" (sadece gameplay highlight)
- 30 sn boyunca yüksek tempolu gameplay, sıralı ability kullanımları, kart line clear satırları, dramatic shake
- Sessizce müzik + sound design + minimal text overlay ("YES" "WOW" gibi reactive)

#### Script 4: "Reklamlar bizi sevdirir mi?"
- 0-5sn: Diğer oyunlarda interstitial spam'i (jenerik clip)
- 5-10sn: "Bizim oyunda?" (gameplay 90 sn ad gap)
- 10-15sn: "Hiç istemiyorsan, 1.99 dolara hepsi gider." (Remove Ads CTA)
- 15-20sn: "Yapımcının size güveni: az reklamla satış." (Play Store yıldız animasyon)

#### Script 5: "Klasik vs. Extended yarış" (split-screen)
- Sol: Classic mode 1 dakika gameplay
- Sağ: Extended mode 1 dakika gameplay (ability spam)
- Alt yazı: "Hangisini seçerdin?"
- End: "Brick Game — ikisini de oynayabilirsin"

### C.2) Twitter / X postları (10 farklı tweet copy)
Karışık tone — bazı viral, bazı bilgilendirici, bazı topluluk. Hashtag stratejisi: #IndieGameDev #RetroGaming #Tetris #MobileGames #BrickGame #EkimozGames + tr için #İndieOyun #MobiloOyun

Üret 10 farklı tweet — her biri farklı: launch announcement, feature highlight, dev diary, meme/relatable, community question, review thank-you, behind-the-scenes screenshot, "what we learned", milestone celebration, contest/giveaway proposal.

### C.3) Instagram carousel post (5 slide tarif)
Slide 1: Hook — "Klasik Tetris hâlâ oynanır mı?"
Slide 2: Tarihçe — "1984'te Pajitnov... 2026'da biz"
Slide 3: Yeni özellikler — Extended Mode, bomb, laser görselleri
Slide 4: Mockups — gameplay screenshot'lar
Slide 5: CTA — "Play Store'da, ücretsiz"

---

## 5) BÖLÜM D — PR / TOPLULUK STRATEJİSİ

### D.1) Reddit post template
- **Subreddits**: r/AndroidGaming (350k), r/IndieGaming (450k), r/Tetris (40k niş ama hot), r/playmygame (90k), r/Turkey (geniş ama relevant for TR users)
- **Format**: Bir başlık + 2-3 paragraf + 1 GIF/video + Play Store link
- **Title tarz**: "Spent 8 months building a Tetris love-letter with bomb + laser abilities — would love your feedback [Free, Android]"
- **Bot avoidance**: Self-post (link post değil), comment'lerde aktif ol, Reddit kuralları için her sub'a 1 hafta arayla post

### D.2) Indie game press / blog outreach
Targetlanacak siteler:
- TouchArcade (mobil oyun blog, indie friendly)
- AndroidPolice (Android focus)
- 148Apps
- Pocket Gamer
- IndieDB (community based)
- Türkçe: ShiftDelete, Tamindir, Webrazzi, OYNANANGÜNCEL

**Press release şablonu** üret (450 kelime, hem TR hem EN, geliştirici quote dahil, screenshot link, contact email).

### D.3) Discord topluluk stratejisi
- Indie Game Dev Discord (350k+ üye, #showcase kanalı)
- Türk oyun toplulukları (Türk Oyun Geliştirme — Steam, Discord serverlar)
- Tetris Discord (resmi yok ama benzer puzzle communities)
- Lansman sırasında 1 hafta boyunca her gün showcase post

### D.4) YouTube outreach
Mobil oyun review yapan küçük-orta YouTuber'lar (5k-50k sub):
- TR: "Oyun İnceleme", "MobiloOyunlar", "TelefonGamer"
- EN: "Mobile Gamer", "ZachsMind", "App Highlights"
- Soğuk e-mail template + free promo code (yok ama IAP free unlock vermeyi düşünebilirsin) + screenshot kit

---

## 6) BÖLÜM E — PAID ADS (opsiyonel, küçük bütçe testi)

### E.1) Google Ads — UAC kampanya
- **Bütçe önerisi**: Günlük ₺50-100 (≈ $5-10), test için 7-14 gün
- **Target CPI**: ₺3-8 (Türkiye), $0.50-1.50 (global)
- **Assets**: Yukarıdaki 6 screenshot + 30sn promo video + headline'lar (15 farklı kısa headline TR + EN)
- **Locations**: Türkiye + ABD + Brezilya + Hindistan (test pazarları, düşük CPI)
- **Optimization**: Install volume → sonra in-app event (level 5 reach, ability use, IAP)

### E.2) TikTok Ads (Reels alternatifi)
- Daha düşük CPI, Gen Z erişimi
- TikTok Spark Ads ile organic post'u boost et (önce organic test, %5+ engagement gösterirse paid push)

### E.3) Meta (Instagram + Facebook) Ads
- Reels + carousel kombosu
- Türkiye için Facebook Stories interest targeting: "Retro gaming, Mobile games, Indie games"

---

## 7) BÖLÜM F — METRIK + KPI TAKİBİ

Üretilen pazarlama materyallerinin etkisini ölçmek için takip edilmesi gerekenler:
- **Play Store**: Install velocity (günlük), conversion rate (store visit → install), country breakdown
- **Retention**: D1, D7, D30 retention (yüksek = oyun iyi)
- **Monetization**: ARPDAU, IAP conversion rate, ad eCPM
- **Sosyal**: Engagement rate, profile visit, link click
- **Press**: Mention count, organik backlinks, brand search volume

**KPI eşik önerileri (ilk 30 gün için):**
- 5,000 install (modest)
- 10,000 install (good)
- 25,000 install (great)
- Retention D7 > %15 (industry avg)
- Remove Ads conversion > %1 (geliştirici geliri için anlamlı)

---

## 8) ÜRETİM SIRASI ÖNERİSİ (bütçesiz başlangıç)

Hafta 1:
- App icon refresh (opsiyonel)
- Feature graphic + 6 phone screenshot tasarımları
- Short + Full description TR + EN
- 30sn promo video kayıt + edit

Hafta 2:
- Reddit + Türk forumları organic post
- 5 TikTok video çek + yayınla
- Press release gönderim (10 mailing list)

Hafta 3:
- Engagement görsellerinden Instagram + Twitter campaign başlat
- 3 küçük YouTube creator'a outreach

Hafta 4:
- İlk metrikleri değerlendir
- En çok engage edileni paid campaign'e dönüştür (₺50/gün)

---

## 9) DOSYA SONU — AI'A SON HATIRLATMA

Bu brief'i okuyup bana ne **tek bir prompt** ile her şeyi üret değil. Yapın istediğin **belirli bölüm** ve hangi formatta. Örnekler:

> "Bölüm 2-A.3 için TR + EN full description üret, 4000 karakter sınırına uy."
> "Bölüm 3-B.2'deki 6 screenshot için Midjourney prompts üret, her birini ayrı yaz."
> "Bölüm 4-C.1'deki 5 TikTok script'in tam metnini Türkçe olarak yaz, voiceover + altyazı + onscreen action ayrı kolonlarda."
> "Bölüm 5-D.2 için press release şablonu (450 kelime, TR), Ekimoz Games adına imzalı."

Bu brief asla "her şeyi tek seferde üret" diye değil — **modüler, çağırdığında ürettir.** Sonuç kalitesi her bölümün ayrı çalıştırılmasıyla artar.

---

**Brief versiyonu:** 1.0 — 2026-06-05
**Hazırlayan:** Kaan Ekimoz (Ekimoz Games) + Claude
**Lisans:** Bu brief'i istediğin AI'a, freelancera, ajansa verebilirsin. Sadece sonuçları Ekimoz Games hesaplarından paylaş.
