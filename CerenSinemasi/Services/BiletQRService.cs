using System.Drawing;
using QRCoder;
using static QRCoder.PayloadGenerator;
using beste.Models;

namespace beste
{
    public static class BiletQRService
    {
        public static Bitmap CreateEventQRCode(Bilet bilet, int pixelsPerModule = 10)
        {
            var calendarEvent = new CalendarEvent(
                subject: $"🎭 {bilet.OyunAdi}",
                description: BuildDescription(bilet),
                location: $"{bilet.TiyatroAdi}, {bilet.TiyatroAdresi}",
                start: bilet.BaslangicZamani,
                end: bilet.BitisZamani,
                allDayEvent: false,
                encoding: CalendarEvent.EventEncoding.Universal
            );

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(calendarEvent.ToString(), QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrCodeData))
            {
                return qrCode.GetGraphic(pixelsPerModule, Color.Black, Color.White, true);
            }
        }

        private static string BuildDescription(Bilet bilet)
        {
            return
                $"Bilet No: {bilet.BiletNo}\n" +
                $"Koltuk: {bilet.Koltuk}\n" +
                $"Salon: {bilet.Salon}\n" +
                $"Müşteri: {bilet.MusteriAdi}\n" +
                $"Fiyat: {bilet.Fiyat:C2}";
        }
    }
}
