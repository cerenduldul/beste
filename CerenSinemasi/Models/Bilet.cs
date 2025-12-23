using System;

namespace beste.Models
{
    public class Bilet
    {
        public string BiletNo { get; set; }
        public string OyunAdi { get; set; }
        public string TiyatroAdi { get; set; }
        public string TiyatroAdresi { get; set; }

        public DateTime BaslangicZamani { get; set; }
        public DateTime BitisZamani { get; set; }

        public string Salon { get; set; }
        public string Koltuk { get; set; }
        public string MusteriAdi { get; set; }

        public decimal Fiyat { get; set; }
    }
}
