using System.Drawing; // Bitmap gibi görsel/çizim sınıflarını kullanmak için
using QRCoder; // QR kod üretmek için kullanılan kütüphane
using static QRCoder.PayloadGenerator; // PayloadGenerator içindeki CalendarEvent gibi sınıfları doğrudan kullanmak için
using beste.Models; // Projedeki Bilet model sınıfını kullanmak için

namespace beste
{
    public static class BiletQRService // QR üretim işini yapan servis sınıfı (static: nesne oluşturmadan kullanılır)
    {
        public static Bitmap CreateEventQRCode(Bilet bilet, int pixelsPerModule = 10)
        // Verilen bilet bilgilerine göre QR kod resmi (Bitmap) üretir
        // pixelsPerModule: QR karelerinin büyüklüğü (artırırsan QR daha büyük/kaliteli görünür)
        {
            string etkinlikAd = !string.IsNullOrWhiteSpace(bilet.EtkinlikAdi)
                ? bilet.EtkinlikAdi
                : bilet.OyunAdi;
            // Etkinlik adı boş değilse EtkinlikAdi kullanılır
            // EtkinlikAdi boşsa yedek olarak OyunAdi kullanılır

            var calendarEvent = new CalendarEvent(
                subject: "Bilet - " + etkinlikAd, 
                // Takvim olayının başlığı: "Bilet - EtkinlikAdı"

                description: BuildDescription(bilet),
                // Takvim olayının açıklaması: BuildDescription metodu bilet bilgilerini metin olarak hazırlar

                location: (bilet.TiyatroAdi ?? "") + ", " + (bilet.TiyatroAdresi ?? ""),
                // Konum bilgisi: TiyatroAdi + TiyatroAdresi
                // null gelirse boş string kullanılır (?? "")

                start: bilet.BaslangicZamani,
                // Etkinliğin başlangıç zamanı (Bilet modelinden gelir)

                end: bilet.BitisZamani,
                // Etkinliğin bitiş zamanı (Bilet modelinden gelir)

                allDayEvent: false,
                // Tüm gün süren bir etkinlik değil (false)

                encoding: CalendarEvent.EventEncoding.Universal
            // Takvim olayının encoding (kodlama) türü: evrensel/uyumlu format
            );

            using (var gen = new QRCodeGenerator())
            // QR kod üretici nesnesi (using: iş bitince otomatik dispose edilir)
            using (var data = gen.CreateQrCode(calendarEvent.ToString(), QRCodeGenerator.ECCLevel.Q))
            // Takvim olayını string'e çevirip QR içeriği olarak veriyoruz
            // ECCLevel.Q: Hata düzeltme seviyesi (QR hasar görse bile okunabilirlik için)
            using (var qr = new QRCode(data))
            // QR kod nesnesi (data üzerinden görsel üretir)
            {
                return qr.GetGraphic(pixelsPerModule);
                // QR kodu Bitmap olarak üretip döndürür
            }
        }

        private static string BuildDescription(Bilet b)
        // QR içindeki açıklama metnini hazırlar (takvim event description alanı)
        {
            return
                "Etkinlik: " + b.EtkinlikAdi + "\n" +
                // Etkinlik adı satırı

                "Bilet Sahibi: " + b.MusteriAdi + "\n" +
                // Bilet sahibi (müşteri adı) satırı

                "PNR: " + b.BiletNo + "\n" +
                // PNR / bilet numarası satırı

                "Koltuk: " + b.Koltuk + "\n" +
                // Koltuk bilgisi satırı

                "Salon: " + b.Salon + "\n" +
                // Salon/konum bilgisi satırı

                "Fiyat: " + b.Fiyat.ToString("0.00") + " TL";
            // Fiyat satırı (0.00 formatı ile 2 ondalık)
        }
    }
}
