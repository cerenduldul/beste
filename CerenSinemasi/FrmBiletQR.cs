using System;
using System.Drawing;
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

            lblOyun.Text = "Oyun: " + _bilet.OyunAdi;
            lblTarih.Text = "Tarih: " + _bilet.BaslangicZamani.ToString("dd.MM.yyyy HH:mm");
            lblSalon.Text = "Salon: " + _bilet.Salon;
            lblKoltuk.Text = "Koltuk: " + _bilet.Koltuk;
            lblPNR.Text = "PNR: " + _bilet.BiletNo;
            lblFiyat.Text = "Fiyat: " + _bilet.Fiyat.ToString("0.00") + " TL";

            if (picQR.Image != null) { picQR.Image.Dispose(); picQR.Image = null; }
            picQR.Image = BiletQRService.CreateEventQRCode(_bilet);
        }
    }
}
