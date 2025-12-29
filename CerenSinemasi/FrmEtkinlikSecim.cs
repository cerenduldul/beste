using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySqlConnector;

namespace beste
{
    public partial class FrmEtkinlikSecim : Form // FrmEtkinlikSecim adlı ekranın bir Windows Form olduğunu söyler.
    {
        private readonly string cs =
            "Server=localhost;Database=bilet_sistemi;User Id=root;Password=Ghezzal18.";

        private const int AKTIF_KULLANICI_ID = 1; // Giriş (login) sistemi olmadığı için aktif kullanıcı şimdilik sabit (1) tutuluyor.

        private FlowLayoutPanel flow; // Etkinlik kartlarının listeleneceği paneli tutmak için değişken tanımlar.

        public FrmEtkinlikSecim()  
        {
            BuildUi(); // Formun içindeki kontrolleri (label, panel vb.) kodla oluşturan metodu çağırır.
            Load += FrmEtkinlikSecim_Load; // Form açılınca FrmEtkinlikSecim_Load metodunun çalışmasını sağlar.
        }

        private void FrmEtkinlikSecim_Load(object sender, EventArgs e) // Form ilk açıldığında çalışacak kodların bulunduğu metottur.
        {
            EtkinlikleriYukle(); // Veritabanından etkinlikleri çekip ekrana dizen metodu çağırır
        }

        private void BuildUi() // Formun tasarımını kodla kurmak için yazılmış metottur.
        {
            Text = "beste"; // Formun üst başlığında görünen yazıyı ayarlar.
            StartPosition = FormStartPosition.CenterScreen; // Formun ekranda ortada açılmasını sağlar.
            Width = 900;
            Height = 600;

            var lblBaslik = new Label // Ekranın üstüne koymak için bir başlık yazısı oluşturur.
            {
                Text = "beste", // Ekranın başlığı
                Font = new Font("Segoe UI", 16, FontStyle.Bold), // Başlık yazısını büyük ve kalın yapar.
                AutoSize = true,
                Left = 15,
                Top = 10
            };
            Controls.Add(lblBaslik); // Bu başlık yazısını forma ekler (ekranda görünür hale getirir).

            flow = new FlowLayoutPanel // Etkinlik kartlarını yan yana/alt alta dizmek için bir panel oluşturur.
            {
                Left = 15,
                Top = 55,
                Width = ClientSize.Width - 30,
                Height = ClientSize.Height - 80,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true // Etkinlikler çok olursa kaydırma çubuğu çıkmasını sağlar.
            };
            Controls.Add(flow); // Bu paneli forma ekler ve ekranda görünür hale getirir.
        }

        private void EtkinlikleriYukle() // Etkinlikleri veritabanından çekip ekrana yerleştiren metottur.
        {
            flow.Controls.Clear(); // Ekranda daha önce gösterilen etkinlikleri temizler.

            try
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open();

                    //  Artık sadece (5,6) değil, TÜM etkinlikler listelenir.
                    var cmd = new MySqlCommand(
                        "SELECT etkinlik_id, etkinlik_adi, tarih, konum, afis_yolu " +
                        "FROM etkinlik ORDER BY tarih",
                        conn);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int etkinlikId = r.GetInt32("etkinlik_id");
                            string ad = r.IsDBNull(r.GetOrdinal("etkinlik_adi")) ? "" : r.GetString("etkinlik_adi");
                            DateTime tarih = r.GetDateTime("tarih");
                            string konum = r.IsDBNull(r.GetOrdinal("konum")) ? "-" : r.GetString("konum");
                            string afis = r.IsDBNull(r.GetOrdinal("afis_yolu")) ? "" : r.GetString("afis_yolu"); // Afiş yolu boşsa hata olmaması için boş string atar, doluysa afiş yolunu alır.

                            flow.Controls.Add(CreateCard(etkinlikId, ad, tarih, konum, afis)); // Her etkinlik için bir “kart” oluşturur ve ekrana ekler.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Etkinlikler yüklenemedi:\n" + ex.Message);
            }
        }

        // Verilen etkinlik bilgilerine göre ekranda gösterilecek bir kart (panel) oluşturur ve geri döndürür.
        private Control CreateCard(int etkinlikId, string ad, DateTime tarih, string konum, string afis)
        {
            var card = new Panel // Etkinliğin tamamını tutacak ana kart panelini oluşturur.
            {
                Width = 400,
                Height = 220,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10)
            };

            var pic = new PictureBox // Etkinlik afişini göstermek için bir resim alanı oluşturur.
            {
                Width = 140,
                Height = 200,
                Left = 10,
                Top = 10,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Resim yüklerken dosya kilitlenmesin diye Image.FromFile yerine stream ile yüklenir.
            // Ayrıca afis boşsa ve dosya yoksa hata vermez.
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", afis); // fotoğraflar buradan çekiliyor.
                if (!string.IsNullOrWhiteSpace(afis) && File.Exists(path))
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        pic.Image = Image.FromStream(fs);
                    }
                }
            }
            catch
            {
                // Afiş yüklenemezse boş bırak (sunumda sorun çıkmasın)
            }

            var lblAd = new Label // Etkinlik adını göstermek için bir yazı alanı oluşturur.
            {
                Text = ad,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Left = 160,
                Top = 15,
                Width = 220
            };

            var lblTarih = new Label // Etkinlik tarihini göstermek için bir yazı alanı oluşturur.
            {
                Text = "Tarih: " + tarih.ToString("dd.MM.yyyy HH:mm"),
                Left = 160,
                Top = 60,
                Width = 220
            };

            var lblKonum = new Label // Etkinliğin yapılacağı yeri göstermek için yazı alanı oluşturur.
            {
                Text = "Konum: " + konum,
                Left = 160,
                Top = 85,
                Width = 220
            };

            var btnSec = new Button // Etkinliği seçmek için bir buton oluşturur.
            {
                Text = "Seç",
                Width = 120,
                Height = 35,
                Left = 160,
                Top = 150
            };

            // “Seç” butonuna basıldığında çalışacak kodları tanımlar.
            btnSec.Click += (s, e) =>
            {
                btnSec.Enabled = false;

                try
                {
                    this.Hide(); // Etkinlik seçim ekranını geçici olarak gizler.

                    // Form1'e "önceki form" olarak etkinlik ekranını gönderiyoruz
                    using (var frmKoltuk = new Form1(etkinlikId, AKTIF_KULLANICI_ID, this))
                    {
                        frmKoltuk.ShowDialog();
                    }
                }
                finally
                {
                    // Koltuk ekranı kapanınca tekrar etkinlik ekranını göster
                    this.Show();
                    btnSec.Enabled = true;
                }
            };

            card.Controls.Add(pic); // Etkinlik afişini karta ekler.
            card.Controls.Add(lblAd); // Etkinlik adını karta ekler.
            card.Controls.Add(lblTarih); // Etkinlik tarih bilgisini karta ekler.
            card.Controls.Add(lblKonum); // Etkinlik konum bilgisini karta ekler.
            card.Controls.Add(btnSec); // “Seç” butonunu karta ekler.

            return card; // Oluşturulan etkinlik kartını, ekrana eklenmesi için geri gönderir.
        }
    }
}
