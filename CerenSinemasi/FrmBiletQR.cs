using System;
using System.Windows.Forms;
using beste.Models;

namespace beste
{
    public partial class FrmBiletQR : Form //QR gösteren ekranın bir Windows Form olduğunu ifade eder.
    {
        private readonly Bilet _bilet; //QR üretilecek bilet bilgisini saklamak için kullanılan değişkendir.

        public FrmBiletQR() //Parametresiz olarak form açılırsa çalışan kurucu metottur.
        {
            InitializeComponent(); //Formun tasarımını ekrana yerleştirir.
        }

        public FrmBiletQR(Bilet bilet) //Form açılırken bilet bilgisinin dışarıdan gönderilmesini sağlar.
        {
            InitializeComponent(); //Formun içindeki kontrolleri oluşturur.
            _bilet = bilet;//Gönderilen bilet bilgisini form içinde saklar.
        }

        private void FrmBiletQR_Load(object sender, EventArgs e)
        {
            if (_bilet == null) return; //Bilet bilgisi yoksa devam etmez.

            EkraniDoldur();//Bilet bilgilerini ekrana yazan metodu çağırır.
            QRUret(); //Bilet için QR kodu oluşturan metodu çağırır.
        }

        private void EkraniDoldur() //Bilet bilgilerini label’lara yerleştiren metottur.
        {

            //Kategori boşsa “Etkinlik” yazar, doluysa gerçek kategori adını alır.
            //Etkinlik adı yoksa oyun adını kullanır.
            string kategori = !string.IsNullOrWhiteSpace(_bilet.KategoriAdi) ? _bilet.KategoriAdi : "Etkinlik";
            string etkinlik = !string.IsNullOrWhiteSpace(_bilet.EtkinlikAdi) ? _bilet.EtkinlikAdi : _bilet.OyunAdi;


            //Etkinlik/oyun adını ekranda gösterir.
            //Etkinlik tarih ve saatini yazdırır.
            lblOyun.Text = kategori + ": " + etkinlik;
            lblTarih.Text = "Tarih: " + _bilet.BaslangicZamani.ToString("dd.MM.yyyy HH:mm");


            //Salon bilgisi yoksa “-” gösterir.
            //Koltuk bilgisi yoksa “-” gösterir.
            string salon = string.IsNullOrWhiteSpace(_bilet.Salon) ? "-" : _bilet.Salon;
            lblSalon.Text = "Salon: " + salon;


            //PNR kodunu ekranda gösterir.
            //Bilet fiyatını para formatında gösterir.
            lblKoltuk.Text = "Koltuk: " + (!string.IsNullOrWhiteSpace(_bilet.Koltuk) ? _bilet.Koltuk : "-");
            lblPNR.Text = "PNR: " + (!string.IsNullOrWhiteSpace(_bilet.BiletNo) ? _bilet.BiletNo : "-");
            lblFiyat.Text = "Fiyat: " + _bilet.Fiyat.ToString("0.00") + " TL";
        }
        //QR kodu üretip ekrana koyan metottur.
        private void QRUret()
        {
            if (picQR.Image != null) //Daha önce bir QR varsa kontrol eder.
            {
                picQR.Image.Dispose(); //Eski QR resmini bellekten temizler
                picQR.Image = null;//PictureBox’ı sıfırlar.
            }

            picQR.Image = BiletQRService.CreateEventQRCode(_bilet); //Bilet bilgisine göre QR kodu üretir ve ekranda gösterir.
        }
    }
}
