using System;

namespace beste.Models
{
    public class Bilet
    {
        // Mevcut alanlar
        public string BiletNo { get; set; }
        public string OyunAdi { get; set; }          // Etkinlik adı olarak kullanılıyor
        public string TiyatroAdi { get; set; }
        public string TiyatroAdresi { get; set; }

        public DateTime BaslangicZamani { get; set; }
        public DateTime BitisZamani { get; set; }

        public string Salon { get; set; }
        public string Koltuk { get; set; }
        public string MusteriAdi { get; set; }

        public decimal Fiyat { get; set; }

        // 🔴 EKLENEN ALANLAR (ZORUNLU)
        public string KategoriAdi { get; set; }      // "Oyun" / "Konser"
        public string EtkinlikAdi { get; set; }      // "Toz" / "Bir Büyülü Gece" / "Cimri"
    }
}

