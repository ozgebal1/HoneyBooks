# Honey Books

Honey Books, Visual Studio ile çalıştırılabilen ASP.NET Core kitapçı projesidir. Arayüz `wwwroot` içindedir; kitap katalogu, üye hesapları, sepet, sipariş kaydı ve iletişim mesajları ASP.NET Core API üzerinden SQL Server'a kaydedilir.

## Projeyi açma

Visual Studio'da `HoneyBooks.sln` dosyasını açıp çalıştır. SQL Server Express'in kurulu ve çalışan olması gerekir. Bağlantı adresi `appsettings.json` içinde `HoneyBooks` adıyla ayarlanır (`localhost\SQLEXPRESS`). Uygulama ilk çalıştığında tabloları oluşturur ve kitap/kategori kataloğunu ekler. SQL Server'a erişilemiyorsa API çalışmaz; `/api/health` sonucu `unavailable` olur.

## İçerik

- `wwwroot/index.html` — ana sayfa, kitap kataloğu, kategori ve arama
- `wwwroot/korku.html`, `wwwroot/kitap.html` — kategori ve kitap ayrıntısı
- `wwwroot/sepet.html` — giriş yapan kullanıcı için SQL Server'da saklanan sepet ve sipariş kaydı
- `wwwroot/kayit.html`, `wwwroot/giris.html` — e-posta biçimi doğrulanan hesap oluşturma ve giriş
- `wwwroot/iletisim.html` — mesajları SQL Server'a kaydeden form
- `Data/` — SQL Server veri modeli ve başlangıç kataloğu
- `wwwroot/images/` — projeyle birlikte GitHub'a eklenecek yerel kapak görselleri

Kapak görsellerinin bazıları Open Library Covers API'den indirilmiştir: https://openlibrary.org/dev/docs/api/covers . Honey Books seçkisi için oluşturulmuş kitapların ve Open Library'de kapak bulunmayan başlıkların kapakları proje içindeki SVG dosyalarıdır.

## Katalog fiyatları

Basılı kitap fiyatları 20 Kasım 2026 tarihinde kontrol edilen Türkiye satış listelemelerinden alınmıştır; satıcı indirimi ve stok değiştikçe fiyatlar değişebilir. Başlıca fiyat kaynakları: [Matmazel Noraliya'nın Koltuğu](https://www.akakce.com/turk-edebiyati-kitaplari/en-ucuz-matmazel-noraliya-nin-koltugu-peyami-safa-fiyati%2C398111717.html), [Yaprak Dökümü](https://www.akakce.com/turk-klasikleri/en-ucuz-yaprak-dokumu-fiyati%2C1446840990.html), [Küçük Prens](https://www.dr.com.tr/kitap/kucuk-prens/cocuk-ve-genclik/okul-cagi-6-10-yas/cocuk-klasik/urunno%3D0000000628979), [Üç Büyük Usta](https://www.dr.com.tr/kitap/uc-buyuk-usta/edebiyat/edebiyatcilar/urunno%3D0000000168888), [Doğu Ekspresinde Cinayet](https://www.kitapsec.com/Products/Dogu-Ekspresinde-Cinayet-Altin-Kitaplar-439585.html), [Şu Çılgın Türkler](https://www.dr.com.tr/kitap/su-cilgin-turkler/edebiyat/roman/turkiye-roman/urunno%3D0000000177540), [Nutuk](https://www.dr.com.tr/kitap/nutuk/mustafa-kemal-ataturk/arastirma-tarih/tarih/ataturk/urunno%3D0001946867001), [Aynı Yıldızın Altında](https://www.dr.com.tr/kitap/ayni-yildizin-altinda/john-green/edebiyat/roman/romantik/urunno%3D0000000448031), [Kilitli Kapı](https://www.dr.com.tr/kitap/kilitli-kapi/frelda-mcfadden/edebiyat/roman/korku-gerilim/urunno%3D0002166396001) ve [Korku](https://www.dr.com.tr/kitap/korku-modern-klasikler-57/stefan-zweig/edebiyat/roman/dunya-roman/urunno%3D0000000652495). Dexter Karanlıkta'nın listelenen fiyatı stokta görünmeyen ürün kaydından alınmıştır; [Seyir Kitap](https://www.seyirkitap.com/dexter-karanlikta). Honey Books için yazılmış özgün seçki başlıklarının piyasa fiyatı olmadığı için proje fiyatları korunmuştur.

## Önemli çalışma sınırları

Üyelikte e-posta adresi kontrol edilip kullanıcı kaydı veritabanında tutulur; e-posta sahipliği doğrulama veya dışarı e-posta gönderme servisi henüz bağlı değildir. Sipariş, veritabanına “Ödeme bekleniyor” durumuyla kaydedilir; kart tahsilatı yapılmaz. Canlı ödeme veya e-posta gönderimi için ilgili servis sağlayıcısı ve gizli anahtarları ayrıca yapılandırılmalıdır.

## GitHub

`.gitignore`, Visual Studio/derleme çıktılarının yüklenmesini engeller; `wwwroot/images` klasörünü dışarıda bırakmaz. GitHub'a `HoneyBooks.sln`, `HoneyBooks.csproj`, kaynak kodlar, `README.md` ve `wwwroot` klasörünü yükle. `bin`, `obj` ve `.vs` klasörlerini yükleme.
