using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;

namespace beste
{
    public partial class Form1 : Form
    {
        // === ALANLAR ===

        // Birden fazla seçili koltuğu tutar
        private List<Button> selectedSeats = new List<Button>();

        // Koltukların bulunduğu panel
        private Panel panelKoltuklar = null;

        // MySQL bağlantı cümlesi
        private readonly string cs =
            "Server=localhost;Database=bilet_sistemi;User Id=root;Password=Ghezzal18.";

        // Sıra harfine göre temel bilet fiyatları (kampanya öncesi)
        private readonly Dictionary<string, decimal> rowPrices = new Dictionary<string, decimal>
        {
            {"A", 500 },
            {"B", 450 },
            {"C", 400 },
            {"D", 350 },
            {"E", 300 }
        };

        public Form1()
        {
            InitializeComponent();
        }

        // === FORM LOAD ===
        private void Form1_Load(object sender, EventArgs e)
        {
            // 1) Etkinlikleri veritabanından çek (şimdilik sadece bağlantıyı test ediyor)
            try
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open();

                    var cmd = new MySqlCommand("SELECT * FROM etkinlik", conn);
                    var list = new MySqlDataAdapter(cmd);
                    var dt = new DataTable();

                    list.Fill(dt);
                    // dataGridView kullanmadığımız için bağlamıyoruz
                    // dataGridView1.DataSource = dt;
                }
            }
            catch (Exception er)
            {
                MessageBox.Show("Hata Oluştu Detay: " + er.Message);
            }

            // 2) Koltukları dinamik oluştur
            try
            {
               panelKoltuklar = new Panel();
               panelKoltuklar.Size = new Size(320, 250);
               panelKoltuklar.Location = new Point(12, 30);
               this.Controls.Add(panelKoltuklar);

                int rows = 5;      // 5 sıra (A–E)
                int cols = 8;      // her satırda 8 koltuk
                int buttonSize = 40;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Button btn = new Button
                        {
                            Width = buttonSize,
                            Height = buttonSize,
                            Left = j * (buttonSize + 5),
                            Top = i * (buttonSize + 5),
                            Text = $"{(char)('A' + i)}{j + 1}",
                            BackColor = Color.WhiteSmoke
                        };

                        btn.Click += Seat_Click;
                        panelKoltuklar.Controls.Add(btn);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koltuk oluşturulurken bir hata oluştu.\n\n" + ex.Message,
                                "Koltuk Oluşturma Hatası",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 3) Veritabanına göre koltuk renklerini ayarla (Rezerve / Satildi ise kırmızı)
            try
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open();

                    foreach (Control kontrol in panelKoltuklar.Controls)
                    {
                        if (kontrol is Button koltuk)
                        {
                            var cmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM bilet " +
                                "WHERE etkinlik_id=@etk AND koltuk_no=@koltuk " +
                                "AND durum IN ('Rezerve','Satildi')",
                                conn);

                            cmd.Parameters.AddWithValue("@etk", 2);          // aktif etkinlik_id
                            cmd.Parameters.AddWithValue("@koltuk", koltuk.Text);

                            int sonuc = Convert.ToInt32(cmd.ExecuteScalar());

                            if (sonuc > 0)
                            {
                                koltuk.BackColor = Color.Red;   // dolu
                            }
                            else
                            {
                                koltuk.BackColor = Color.WhiteSmoke; // boş
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koltuk durumu kontrol edilirken hata oluştu.\n\n" + ex.Message);
            }

            // 4) Bilet Türü combobox'ını ilk haline getir
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Enabled = false; // sadece bilgi göstermek için
            GuncelleBiletTuruBilgisi(); // başlangıçta varsayılan liste
        }

        // === KOLTUK TIKLAMA ===
        // Çoklu seçim: tıklayınca seçiliyse kaldır, değilse ekle
        private void Seat_Click(object sender, EventArgs e)
        {
            Button tiklanan = (Button)sender;

            // Dolu koltuk seçilmesin
            if (tiklanan.BackColor == Color.Red)
            {
                MessageBox.Show("Bu koltuk dolu, seçemezsiniz.");
                return;
            }

            // Eğer zaten seçiliyse listeden çıkar ve rengini eski haline döndür
            if (selectedSeats.Contains(tiklanan))
            {
                selectedSeats.Remove(tiklanan);
                tiklanan.BackColor = Color.WhiteSmoke;
            }
            else
            {
                // Yeni seçim → listeye ekle ve yeşil yap
                selectedSeats.Add(tiklanan);
                tiklanan.BackColor = Color.LightGreen;
            }

            // Seçili koltukları virgülle textbox'a yaz
            txtKoltukNo.Text = string.Join(", ", selectedSeats.Select(b => b.Text));

            // Bilet türü alanındaki fiyat bilgisini güncelle
            GuncelleBiletTuruBilgisi();
        }

        // === FİYAT & İNDİRİM HESAPLAMA METOTLARI ===

        // Koltuk sıra harfine göre normal bilet fiyatı
        private decimal GetKoltukFiyati(char sira)
        {
            switch (char.ToUpper(sira))
            {
                case 'A': return 500;
                case 'B': return 450;
                case 'C': return 400;
                case 'D': return 350;
                case 'E': return 300;
                default: return 0;   // geçersiz ise
            }
        }

        // Koltuk sıra harfine göre indirim tutarı
        // A:50, B:40, C:30, D:20, E:10
        private decimal GetKoltukIndirimi(char sira)
        {
            switch (char.ToUpper(sira))
            {
                case 'A': return 50;
                case 'B': return 40;
                case 'C': return 30;
                case 'D': return 20;
                case 'E': return 10;
                default: return 0;
            }
        }

        // Çoklu alımlar için toplam fiyat ve indirim hesaplar
        private void HesaplaToplamTutar(List<string> koltuklar,
                                        out decimal toplamFiyat,
                                        out decimal toplamIndirim,
                                        out decimal odenecekTutar)
        {
            toplamFiyat = 0;
            toplamIndirim = 0;

            if (koltuklar == null || koltuklar.Count == 0)
            {
                odenecekTutar = 0;
                return;
            }

            foreach (var koltuk in koltuklar)
            {
                if (string.IsNullOrWhiteSpace(koltuk))
                    continue;

                // Örn: "A10" → 'A'
                char sira = char.ToUpper(koltuk[0]);

                decimal fiyat = GetKoltukFiyati(sira);
                decimal indirim = GetKoltukIndirimi(sira);

                toplamFiyat += fiyat;
                toplamIndirim += indirim;
            }

            // İndirim sadece 2 ve üzeri bilette geçerli olsun
            if (koltuklar.Count < 2)
            {
                toplamIndirim = 0;
            }

            odenecekTutar = toplamFiyat - toplamIndirim;
        }

        // === BİLET TÜRÜ ALANINDA ÖDENECEK TUTARI GÖSTEREN METOT ===
        private void GuncelleBiletTuruBilgisi()
        {
            // Seçili koltuk yoksa: standart sıra–fiyat listesini göster
            if (selectedSeats.Count == 0)
            {
                comboBox1.Items.Clear();

                foreach (var kvp in rowPrices)
                {
                    comboBox1.Items.Add($"{kvp.Key} Sırası - {kvp.Value} TL");
                }

                comboBox1.SelectedIndex = -1;
                return;
            }

            // Seçili koltuklar varsa: toplamı hesapla
            List<string> koltukKodlari = selectedSeats
                .Select(b => b.Text)
                .ToList();

            decimal toplamFiyat;
            decimal toplamIndirim;
            decimal odenecekTutar;

            HesaplaToplamTutar(koltukKodlari,
                               out toplamFiyat,
                               out toplamIndirim,
                               out odenecekTutar);

            comboBox1.Items.Clear();

            if (koltukKodlari.Count == 1)
            {
                // Tek koltukta: sıranın harfi + ödenecek tutar
                char row = koltukKodlari[0][0];   // "A4" -> 'A'
                comboBox1.Items.Add($"{row} - {odenecekTutar} TL");
            }
            else
            {
                // Çoklu alımda: kaç koltuk + ödenecek toplam tutar
                comboBox1.Items.Add($"{koltukKodlari.Count} koltuk - {odenecekTutar} TL");
            }

            comboBox1.SelectedIndex = 0;
        }

        // === KAYDET BUTONU ===
        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1) En az bir koltuk seçilmiş mi?
            if (selectedSeats.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir koltuk seçin.");
                return;
            }

            // 2) Ad Soyad kontrolü
            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text))
            {
                MessageBox.Show("Lütfen Ad Soyad bilgisini girin.");
                return;
            }

            // 3) Seçili koltukların kodlarını string listesine çevir
            List<string> koltukKodlari = selectedSeats
                                            .Select(b => b.Text)
                                            .ToList();

            // 4) Toplam fiyat ve indirim hesapla (bilgi için)
            decimal toplamFiyat;
            decimal toplamIndirim;
            decimal odenecekTutar;

            HesaplaToplamTutar(koltukKodlari,
                               out toplamFiyat,
                               out toplamIndirim,
                               out odenecekTutar);

            // Kullanıcıya bilgi ver
            MessageBox.Show(
                $"Toplam Fiyat (indirimsiz): {toplamFiyat} TL\n" +
                $"Toplam İndirim: {toplamIndirim} TL\n" +
                $"Ödenecek Tutar: {odenecekTutar} TL",
                "Kampanyalı Bilet",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // 5) Veritabanına kayıt
            try
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open();

                    foreach (string koltukNo in koltukKodlari)
                    {
                        var cmd = new MySqlCommand(
                            "INSERT INTO bilet (kullanici_id, etkinlik_id, koltuk_no, satin_alma_tarihi, durum, isim) " +
                            "VALUES (@kullanici, @etkinlik, @koltuk, NOW(), @durum, @isim)",
                            conn);

                        cmd.Parameters.AddWithValue("@kullanici", 1);   // şimdilik sabit
                        cmd.Parameters.AddWithValue("@etkinlik", 2);    // aktif etkinlik
                        cmd.Parameters.AddWithValue("@koltuk", koltukNo);
                        cmd.Parameters.AddWithValue("@durum", "Rezerve");   // veya "Satildi"
                        cmd.Parameters.AddWithValue("@isim", txtAdSoyad.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Rezervasyon(lar) başarıyla kaydedildi.");

                // 6) Seçili koltukları kırmızı yap ve listeleri temizle
                foreach (var seat in selectedSeats)
                {
                    seat.BackColor = Color.Red;
                    seat.Enabled = false;
                }

                selectedSeats.Clear();
                txtKoltukNo.Text = string.Empty;
                txtAdSoyad.Text = string.Empty;

                // Bilet türü bilgisini sıfırla
                GuncelleBiletTuruBilgisi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt sırasında bir hata oluştu:\n" + ex.Message);
            }
        }

        // === BOŞ EVENT'LER (Designer için) ===

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Kullanıcı combobox'ı değiştiremiyor (Enabled=false),
            // istersen ileride burada ekstra işlem yapabilirsin.
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Şimdilik boş. İstersen farklı bir işlev için kullanabilirsin.
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
