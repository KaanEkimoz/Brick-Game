# Brick Game — iOS App Store Yayın Yol Haritası

Bu doküman Brick Game'in iOS'a taşınmasının tam adım listesi. Android (Google Play) yayını referans alınarak hazırlandı — kod tarafında %95'i hazır (UnityIAP 5.3.1 cross-platform, LevelPlay iOS desteği var, oyun zaten portrait mobil).

---

## TL;DR — Maliyet ve süre

| Kalem | Detay |
|---|---|
| **Apple Developer hesabı** | **$99/yıl** (Android'in $25 one-time'ına karşı) |
| **Donanım** | Mac (mevcut ✓), Xcode (ücretsiz indir) |
| **Yazılım** | Unity iOS Build Support modülü (Hub'dan kur) |
| **Süre — ilk yayın** | ~1 hafta (hesap 1-2 gün + setup 1-2 gün + review 1-2 gün) |
| **Apple komisyonu** | %15 (Small Business Program ile, <$1M/yıl gelir için) |

---

## FAZ 1 — Apple Developer hesabı (1-2 gün)

### 1.1 Tip seçimi: Individual vs Organization

| Tip | Avantaj | Dezavantaj | Tavsiye |
|---|---|---|---|
| **Individual** | Hızlı (1-2 gün), TC kimlik yeter | App'in adı "Kaan Ekimoz" olarak görünür Apple altında, geliştirici ismi değiştirilemez sonradan | İlk başlamak için **bu** |
| **Organization** | Geliştirici adı "Ekimoz Games" görünür | D-U-N-S numarası gerek (TR'de Dun & Bradstreet, ~2-4 hafta), şirket gerekir | İleride şirket olunca |

**Önerim**: Individual ile başla, gelir geldikçe Organization'a geçiş düşünülür.

### 1.2 Kayıt adımları
1. **developer.apple.com** → "Account" → Apple ID ile giriş yap (yoksa yarat — iCloud hesabınla aynı olabilir)
2. **Enroll in the Apple Developer Program** linki
3. **Individual** seç
4. Kişisel bilgiler + TC kimlik + adres
5. Ödeme: **$99 USD/yıl** (kredi kartı, otomatik yenileme)
6. **24-48 saat** Apple email doğrulaması bekle
7. Onaylanınca App Store Connect'e erişim açılır

### 1.3 Vergi + Banka kurulumu
App Store Connect → **Agreements, Tax, and Banking**
- **Bank Account**: TR banka hesabı bağla (IBAN + SWIFT)
- **Tax Forms**:
  - **W-8BEN** (US tax) — aynı Google'a verdiğin gibi: Treaty Turkey, business income, %0 stopaj
  - **Türkiye tax info**: VKN veya TC Kimlik
  - Apple kalkış noktası farklı olabilir, AppStore Connect Tax Wizard'ı izle

---

## FAZ 2 — Unity iOS hazırlığı (1 gün)

### 2.1 Unity Hub — iOS Build Support modülünü yükle
- Unity Hub → Installs → Unity 6000.3.12f1 → ⋮ → Add Modules
- ☑ **iOS Build Support** (yaklaşık 3 GB indirme)
- Bittiğinde Unity'i yeniden başlat

### 2.2 Build Settings — Platform switch
- File → Build Settings → iOS seç → **Switch Platform** (bekleyici, ~5-10 dk)

### 2.3 Player Settings (Project Settings → Player → iOS tab)
- **Identification:**
  - Bundle Identifier: `com.ekimozgames.brickgame` (Android ile aynı kalabilir, IAP product ID paylaşımı için iyi)
  - Version: `1.3.2` (Android ile senkron)
  - Build: `1` (iOS'ta ayrı, başlangıç)
- **Configuration:**
  - Scripting Backend: IL2CPP (zorunlu)
  - Api Compatibility: .NET Standard 2.1
  - Target Architecture: ARM64
  - Target Device: iPhone + iPad (universal) — istemiyorsan sadece iPhone
  - Target iOS Version: **13.0** (geniş coverage, ATT zorunlu yaş)
- **Other Settings → Camera Usage Description / Microphone Usage Description**: Brick Game kullanmıyor, boş bırakılabilir
- **Other Settings → User Tracking Usage Description**: **DOLDUR** (ATT prompt metni). Önerilen:
  > "This identifier allows us to show more relevant ads and support the free version of Brick Game."
  > TR: "Bu kimlik daha alakalı reklamlar göstermemizi ve Brick Game'in ücretsiz sürümünü desteklememizi sağlar."
- **Resolution and Presentation:**
  - Default Orientation: Portrait
  - Auto Rotation: sadece Portrait
- **Splash Image**: Unity Personal lisans varsa "Made with Unity" zorunlu (Pro'da kaldırılabilir)

### 2.4 iOS-spesifik manifest gereksinimleri (Editor script)
Apple **Privacy Manifest** (2024'ten beri zorunlu) — `Assets/Plugins/iOS/PrivacyInfo.xcprivacy` dosyası eklenmeli. LevelPlay SDK kendi privacy manifest'ini içerir, sen sadece üst-seviye uygulama privacy manifest'ini eklersin.

Şablon: oyun gerçek-zamanlı veri toplamıyor (sadece anonymous IDFA reklam için).

### 2.5 Build
- File → Build Settings → **Build** (Build And Run **YAPMA** — Xcode'da elle imzalayacağız)
- Output klasörü: `Builds/iOS/`
- Sonuç: bir **Xcode projesi** (.xcodeproj klasörü), AAB değil

---

## FAZ 3 — Xcode + App Store Connect (1-2 gün)

### 3.1 Apple Developer Portal — App ID register
- developer.apple.com → Certificates, Identifiers & Profiles
- **Identifiers** → + → App IDs → App
- Bundle ID: `com.ekimozgames.brickgame` (Unity ile birebir aynı)
- Capabilities:
  - ☑ In-App Purchase (IAP için zorunlu)
  - ☑ App Groups (gerekmez ama açabilirsin)

### 3.2 App Store Connect — Yeni uygulama oluştur
- appstoreconnect.apple.com → My Apps → + → New App
- Platform: iOS
- Name: **Brick Game: Retro Classic Game** (Android ile aynı)
- Primary Language: English (US) — Türkçe localization sonra eklenir
- Bundle ID: yukarıdaki ID
- SKU: `BRICK001` (sadece senin için, görünmez)
- User Access: Full Access

### 3.3 Store listing (App Store Connect → App Information)
Android Play Console'daki içeriğin **iOS versiyonu**:
- **Subtitle** (max 30 char) — Android'de yok, iOS'a özel
- **Promotional Text** (170 char) — versiyon yayınlamadan değiştirilebilir
- **Description** (4000 char) — Android'in full description'unu uyarla, "Google Play" yerine "App Store"
- **Keywords** (100 char total, virgülle ayrı, en kritik ASO field'ı)
- **Support URL** (zorunlu)
- **Marketing URL** (opsiyonel)
- **Privacy Policy URL** (zorunlu)
- **Category**: Games > Puzzle (Primary) + Games > Arcade (Secondary)
- **App Privacy Details** — uzun form: hangi veriler toplanır, tracking için kullanılır mı?
  - Identifier (IDFA) — reklam için
  - User content — yok
  - Diagnostic data — yok
  - Bunu doğru doldur, yanlışsa app review reddedilir

### 3.4 Xcode — Imza + Archive
1. **Xcode'u aç** → Unity'nin oluşturduğu `Builds/iOS/Unity-iPhone.xcodeproj` dosyasını aç
2. **Top-level project** (mavi simge) → Signing & Capabilities
   - Team: Apple Developer hesabın
   - "Automatically manage signing" ☑
   - Bundle Identifier doğru görünmeli
3. **Build target**: Generic iOS Device (sol üst, simulator değil)
4. **Product → Archive** (10-15 dk)
5. Organizer açılır → seçili archive → **Distribute App** → App Store Connect → Upload
6. 5-10 dakika upload, sonra App Store Connect'te "Processing" durumu

### 3.5 TestFlight (iç test)
- App Store Connect → TestFlight tab → builds liste
- Yeni build'i seç → Internal Testing
- Test grubu: sadece kendin
- TestFlight app'i iPhone'una yükle → Brick Game iOS sürümünü test et

---

## FAZ 4 — IAP setup (App Store Connect)

### 4.1 Ürünü oluştur
- App Store Connect → uygulaman → In-App Purchases → + → **Non-Consumable**
- Reference Name: `Remove Ads`
- **Product ID**: `remove_ads` ← **Android ile BİREBİR AYNI** (UnityIAP cross-platform çalışsın)
- Pricing: **Tier 2** ($1.99 USD, TR yaklaşık ₺24-30)
- Apple custom pricing izin verir — Türkiye için manuel ₺24.99 set edebilirsin
- **Localizations**:
  - English: "Remove Ads" + "Remove all ads from Brick Game forever."
  - Turkish: "Reklamları Kaldır" + "Brick Game'deki tüm reklamları kalıcı olarak kaldırır."
- **Review screenshot** (1024×1024) — Apple review'ı için Remove Ads butonunu gösteren screenshot
- **Review notes**: "Single-tap purchase from settings screen. Removes all banner + interstitial ads permanently. No subscription, no consumable."
- **Submit for Review** — App ile birlikte yayınlanır

### 4.2 Sandbox testing
- App Store Connect → Users and Access → **Sandbox Testers**
- Test Apple ID yarat (gerçek hesaptan farklı, örn. `kaan+sandbox@gmail.com`)
- TestFlight build'inde bu sandbox Apple ID ile giriş yap → IAP test et (gerçek para çekilmez)

### 4.3 Apple Small Business Program (%30 → %15 indirim)
- Yıllık geliri **$1M altında** olan geliştiriciler için Apple %15 komisyon alır (%30 yerine)
- Otomatik değil — başvurmak gerek: developer.apple.com → App Store Small Business Program
- 5 dakikalık form, sonraki ay aktif olur. **Mutlaka yap.**

---

## FAZ 5 — App Review + Public Release

### 5.1 App Review submission
- App Store Connect → uygulama → Distribution → App Review → Add for Review
- Bütün metadata ve build seçili olmalı
- Submit
- **Review süresi**: ortalama 24-48 saat (ilk submission genelde daha uzun, 2-5 gün)

### 5.2 Yaygın reddi sebepleri (önceden hazır ol)
1. **"Tetris" kelimesi description'da** → Tetris Holding LLC trademark sahibi. App rejected. **"Tetris" yazma**, "block puzzle", "brick game", "classic arcade puzzle" kullan.
2. **Missing ATT prompt** → LevelPlay reklamları IDFA istiyor, ATT açıklaması zorunlu. Player Settings'te User Tracking Usage Description boşsa reddedilir.
3. **App icon'da Apple logosu / başka marka** → telif ihlali sayılır
4. **Privacy nutrition labels eksik veya yanlış** → "Data Used to Track You" doğru beyan edilmeli (IDFA reklam için)
5. **IAP without restore button** → "Restore Purchases" butonu zorunlu (Settings ekranında olmalı). Android'de opsiyonel, iOS'ta ZORUNLU.

### 5.3 Public release
- Onaylanırsa "Pending Developer Release" durumuna gelir
- Manuel "Release" tıklat → app store'da görünür (ortalama 1-2 saat içinde)
- Veya "Auto-release after approval" seçeneği — daha hızlı

---

## FAZ 6 — LevelPlay iOS entegrasyonu

iOS için ayrı bir LevelPlay App Key gerek.

### 6.1 LevelPlay dashboard
- platform.ironsrc.com → Monetization → Apps → + Add App
- Platform: **iOS**
- App Store URL (yayınlandıktan sonra)
- iOS App Key alacaksın — bu **Android App Key'den farklı**

### 6.2 Unity tarafı
`Assets/Scripts/Ads/AdConfig.cs` ya da `LevelPlayProvider`'da iOS App Key için ayrı field:
```csharp
#if UNITY_ANDROID
private const string AppKey = "2682db2c5"; // Android
#elif UNITY_IOS
private const string AppKey = "YENI_IOS_APP_KEY";
#endif
```

### 6.3 SKAdNetwork + IDFA + ATT
- SKAN ID listesi LevelPlay otomatik ekler (Info.plist düzenler)
- ATT prompt için LevelPlay `IronSource.Agent.setConsent(true)` cağırılır
- Player ATT'yi reddederse reklamlar **göstermeye devam eder** ama IDFA olmadan (eCPM düşer)

---

## FAZ 7 — Sonraki adımlar (yayın sonrası)

1. **Localization** — Türkçe + İspanyolca + Almanca + Japonca ekle (App Store Connect'te dil bazlı metinler)
2. **App Store Optimization (ASO)** — Keywords field'ı kritik (Apple'da Google Play'den daha güçlü etki)
3. **Featured banner** için Apple editorial team'ine outreach
4. **App Store Analytics** — install, conversion, IAP performance
5. **Cross-promotion** — Android kullanıcılarını iOS'a yönlendiren campaign (sosyal medya)

---

## Karar tablosu — Kaan'a sorular

| Soru | A seçeneği | B seçeneği |
|---|---|---|
| Hesap tipi | Individual ($99) hemen | Organization (D-U-N-S 2-4 hafta) |
| Bundle ID | `com.ekimozgames.brickgame` (Android ile aynı) | `com.ekimozgames.brickgameios` (ayrı) |
| iOS min version | iOS 13 (geniş) | iOS 15 (daha modern API, biraz daha az cihaz) |
| Localization | Sadece EN ilk başta | EN + TR baştan |
| App icon | Mevcut Android icon yeniden kullan | iOS için ayrı tasarla (Apple stili) |

---

## Toplam efor tahmini

| Faz | Süre |
|---|---|
| Apple hesap açma | 1-2 gün (Apple onayı) |
| Unity iOS setup + build | 1 gün |
| Xcode + App Store Connect kurulum | 1 gün |
| IAP setup + sandbox test | yarım gün |
| App Review submission + onay | 2-5 gün |
| **Toplam (paralel iş yok)** | **1-2 hafta** |

Yıllık maliyetin **$99 + Apple komisyonu**, gerisi mevcut altyapıyla aynı.

---

**Versiyon:** 1.0 — 2026-06-05
**Hazırlayan:** Kaan Ekimoz + Claude
