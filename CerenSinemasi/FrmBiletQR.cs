using System;
using System.Windows.Forms;
using beste.Models;

namespace beste
{
    public partial class FrmBiletQR : Form
    {
        private readonly Bilet _bilet;

        public FrmBiletQR()
        {
            InitializeComponent();
        }

        public FrmBiletQR(Bilet bilet)
        {
            InitializeComponent();
            _bilet = bilet;
        }

        private void FrmBiletQR_Load(object sender, EventArgs e)
        {
            if (_bilet == null) return;

            EkraniDoldur();
            QRUret();
        }

        private void EkraniDoldur()
        {
            string kategori = !string.IsNullOrWhiteSpace(_bilet.KategoriAdi) ? _bilet.KategoriAdi : "Etkinlik";
            string etkinlik = !string.IsNullOrWhiteSpace(_bilet.EtkinlikAdi) ? _bilet.EtkinlikAdi : _bilet.OyunAdi;

            lblOyun.Text = $"{kategori}: {etkinlik}";
            lblTarih.Text = "Tarih: " + _bilet.BaslangicZamani.ToString("dd.MM.yyyy HH:mm");

            string salon = string.IsNullOrWhiteSpace(_bilet.Salon) ? "-" : _bilet.Salon;
            lblSalon.Text = "Salon: " + salon;

            lblKoltuk.Text = "Koltuk: " + (!string.IsNullOrWhiteSpace(_bilet.Koltuk) ? _bilet.Koltuk : "-");
            lblPNR.Text = "PNR: " + (!string.IsNullOrWhiteSpace(_bilet.BiletNo) ? _bilet.BiletNo : "-");
            lblFiyat.Text = "Fiyat: " + _bilet.Fiyat.ToString("0.00") + " TL";
        }

        private void QRUret()
        {
            if (picQR.Image != null)
            {
                picQR.Image.Dispose();
                picQR.Image = null;
            }

            picQR.Image = BiletQRService.CreateEventQRCode(_bilet);
        }
    }
}
