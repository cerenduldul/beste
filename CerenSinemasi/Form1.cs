using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySqlConnector;

namespace beste
{
    public partial class Form1 : Form
    {
        // ✅ Bir önceki ekran (geri dönmek için)
        private readonly Form _oncekiForm;

        // ✅ Geri butonu (kodla eklenecek)
        private Button _btnGeri;

        // Kullanıcının seçtiği koltukları tutan liste.
        private readonly List<Button> selectedSeats = new List<Button>();

        // Koltuk butonlarının ekleneceği panel
        private Panel panelKoltuklar = null;

        // İptal Modu ile ilgili değişkenler
        private bool iptalModu = false;
        private Button iptalSecilenKoltuk = null;
        private CheckBox chkIptal = null;
        private Button btnIptal = null;

        // MySQL bağlantı
        private readonly string cs =
            "Server=localhost;Database=bilet_sistemi;User Id=root;Password=Ghezzal18.";

        private readonly int _aktifEtkinlikId;
        private readonly int _aktifKullaniciId;

        // Parametresiz constructor (opsiyonel)
        public Form1()
        {
            InitializeComponent();
            _aktifEtkinlikId = 2;
            _aktifKullaniciId = 1;
            _oncekiForm = null;
        }

        // ✅ En doğru kullanım: önceki formu da al
        public Form1(int etkinlikId, int kullaniciId, Form oncekiForm)
        {
            InitializeComponent();
            _aktifEtkinlikId = etkinlikId;
            _aktifKullaniciId = kullaniciId;
            _oncekiForm = oncekiForm;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GeriButonunuEkle();

            KoltuklariOlustur();
            KoltukRenkleriniDbyeGoreAyarla();

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Enabled = false;
            GuncelleBiletTuruBilgisi();

            IptalKontrolleriniKur();
        }

        private void GeriButonunuEkle()
        {
            // Formda aynı isimde geri butonu zaten eklendiyse tekrar ekleme
            if (Controls.Find("btnGeri", true).Length > 0) return;

            _btnGeri = new Button
            {
                Name = "btnGeri",
                Text = "←",
                Width = 40,
                Height = 30,
                Left = 10,
                Top = 10,
                TabStop = false
            };

            _btnGeri.Click += (s, e) =>
            {
                // Önceki ekran varsa göster
                _oncekiForm?.Show();
                // Bu ekranı kapat (program kapanmaz; önceki form açık olduğu için)
                this.Close();
            };

            Controls.Add(_btnGeri);
            _btnGeri.BringToFront();
        }

        private void KoltuklariOlustur()
        {
            int rows = 6;    // A-F
            int cols = 8;    // 1-8
            int size = 40;
            int gap = 5;

            panelKoltuklar = new Panel
            {
                Location = new Point(12, 30),
                Size = new Size(
                    cols * size + (cols - 1) * gap,
                    rows * size + (rows - 1) * gap)
            };

            Controls.Add(panelKoltuklar);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Button btn = new Button
                    {
                        Width = size,
                        Height = size,
                        Left = j * (size + gap),
                        Top = i * (size + gap),
                        Text = $"{(char)('A' + i)}{j + 1}",
                        BackColor = Color.WhiteSmoke
                    };

                    btn.Click += Seat_Click;
                    panelKoltuklar.Controls.Add(btn);
                }
            }
        }

        private void KoltukRenkleriniDbyeGoreAyarla()
        {
            try
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open();

                    foreach (Control c in panelKoltuklar.Controls)
                    {
                        if (c is Button koltuk)
                        {
                            var cmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM bilet " +
                                "WHERE etkinlik_id=@e AND koltuk_no=@k AND durum='Aktif'",
                                conn);

                            cmd.Parameters.AddWithValue("@e", _aktifEtkinlikId);
                            cmd.Parameters.AddWithValue("@k", koltuk.Text);

                            int sayi = Convert.ToInt32(cmd.ExecuteScalar());
                            koltuk.BackColor = (sayi > 0) ? Color.Red : Color.WhiteSmoke;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koltuk durumu okunamadı:\n" + ex.Message);
            }
        }

        private void IptalKontrolleriniKur()
        {
            chkIptal = new CheckBox
            {
                Name = "chkIptal",
                Text = "İptal Modu",
                AutoSize = true,
                Left = btnSave.Left,
                Top = btnSave.Bottom + 5
            };

            btnIptal = new Button
            {
                Name = "btnIptalDinamik",
                Text = "Bileti İptal Et",
                Width = btnSave.Width,
                Height = 35,
                Left = btnSave.Left,
                Top = chkIptal.Bottom + 5,
                Visible = false
            };

            chkIptal.CheckedChanged += (s, a) =>
            {
                iptalModu = chkIptal.Checked;
                btnIptal.Visible = iptalModu;
                btnSave.Enabled = !iptalModu;

                IptalSeciminiTemizle();
                SatisSeciminiTemizle();
            };

            btnIptal.Click += btnIptal_Click;

            Controls.Add(chkIptal);
            Controls.Add(btnIptal);
        }

        private void Seat_Click(object sender, EventArgs e)
        {
            Button tiklanan = (Button)sender;

            if (iptalModu)
            {
                if (tiklanan.BackColor != Color.Red && tiklanan.BackColor != Color.Gold)
                {
                    MessageBox.Show("İptal için satılmış (kırmızı) koltuk seçmelisiniz.");
                    return;
                }

                if (iptalSecilenKoltuk != null && iptalSecilenKoltuk != tiklanan)
                    iptalSecilenKoltuk.BackColor = Color.Red;

                if (iptalSecilenKoltuk == tiklanan && tiklanan.BackColor == Color.Gold)
                {
                    tiklanan.BackColor = Color.Red;
                    iptalSecilenKoltuk = null;
                    txtKoltukNo.Text = "";
                    return;
                }

                iptalSecilenKoltuk = tiklanan;
                iptalSecilenKoltuk.BackColor = Color.Gold;
                txtKoltukNo.Text = iptalSecilenKoltuk.Text;
                return;
            }

            if (tiklanan.BackColor == Color.Red)
            {
                MessageBox.Show("Bu koltuk dolu.");
                return;
            }

            if (selectedSeats.Contains(tiklanan))
            {
                selectedSeats.Remove(tiklanan);
                tiklanan.BackColor = Color.WhiteSmoke;
            }
            else
            {
                selectedSeats.Add(tiklanan);
                tiklanan.BackColor = Color.LightGreen;
            }

            txtKoltukNo.Text = string.Join(", ", selectedSeats.Select(x => x.Text));
            GuncelleBiletTuruBilgisi();
        }

        private decimal GetKoltukFiyati(char sira)
        {
            switch (char.ToUpper(sira))
            {
                case 'A': return 500;
                case 'B': return 450;
                case 'C': return 400;
                case 'D': return 350;
                case 'E': return 300;
                case 'F': return 250;
                default: return 0;
            }
        }

        private decimal GetKoltukIndirimi(char sira)
        {
            switch (char.ToUpper(sira))
            {
                case 'A': return 50;
                case 'B': return 40;
                case 'C': return 30;
                case 'D': return 20;
                case 'E': return 10;
                case 'F': return 5;
                default: return 0;
            }
        }

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
                if (string.IsNullOrWhiteSpace(koltuk)) continue;

                char sira = char.ToUpper(koltuk[0]);
                toplamFiyat += GetKoltukFiyati(sira);
                toplamIndirim += GetKoltukIndirimi(sira);
            }

            if (koltuklar.Count < 2) toplamIndirim = 0;

            odenecekTutar = toplamFiyat - toplamIndirim;
        }

        private void GuncelleBiletTuruBilgisi()
        {
            comboBox1.Items.Clear();

            if (selectedSeats.Count == 0)
            {
                comboBox1.Items.Add("Koltuk seçiniz");
                comboBox1.SelectedIndex = 0;
                return;
            }

            List<string> koltukKodlari = selectedSeats.Select(b => b.Text).ToList();

            decimal toplamFiyat, toplamIndirim, odenecekTutar;
            HesaplaToplamTutar(koltukKodlari, out toplamFiyat, out toplamIndirim, out odenecekTutar);

            comboBox1.Items.Add($"Toplam: {odenecekTutar} TL (İndirim: {toplamIndirim} TL)");
            comboBox1.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (selectedSeats.Count == 0)
            {
                MessageBox.Show("Lütfen koltuk seçin.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text))
            {
                MessageBox.Show("Lütfen Ad Soyad girin.");
                return;
            }

            List<string> koltukKodlari = selectedSeats.Select(b => b.Text).ToList();

            decimal toplamFiyat, toplamIndirim, odenecekTutar;
            HesaplaToplamTutar(koltukKodlari, out toplamFiyat, out toplamIndirim, out odenecekTutar);

            List<int> biletIdleri = new List<int>();

            try
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open();

                    foreach (string koltukNo in koltukKodlari)
                    {
                        var cmd = new MySqlCommand(
                            "INSERT INTO bilet (kullanici_id, etkinlik_id, koltuk_no, satin_alma_tarihi, durum, isim) " +
                            "VALUES (@k, @e, @koltuk, NOW(), 'Aktif', @isim)", conn);

                        cmd.Parameters.AddWithValue("@k", _aktifKullaniciId);
                        cmd.Parameters.AddWithValue("@e", _aktifEtkinlikId);
                        cmd.Parameters.AddWithValue("@koltuk", koltukNo);
                        cmd.Parameters.AddWithValue("@isim", txtAdSoyad.Text);

                        cmd.ExecuteNonQuery();
                        biletIdleri.Add((int)cmd.LastInsertedId);
                    }
                }

                new FrmOdeme(cs, _aktifKullaniciId, biletIdleri, odenecekTutar).ShowDialog();

                foreach (var seat in selectedSeats)
                    seat.BackColor = Color.Red;

                selectedSeats.Clear();
                txtKoltukNo.Text = "";
                txtAdSoyad.Text = "";
                GuncelleBiletTuruBilgisi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Satın alma hatası:\n" + ex.Message);
            }
        }

        private void btnIptal_Click(object sender, EventArgs e)
        {
            if (!iptalModu)
            {
                MessageBox.Show("Önce İptal Modu'nu açın.");
                return;
            }

            if (iptalSecilenKoltuk == null)
            {
                MessageBox.Show("İptal edilecek koltuğu seçin.");
                return;
            }

            string koltukNo = iptalSecilenKoltuk.Text;

            try
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        var sil = new MySqlCommand(
                            "DELETE FROM bilet WHERE etkinlik_id=@e AND koltuk_no=@k AND durum='Iptal'",
                            conn, tx);
                        sil.Parameters.AddWithValue("@e", _aktifEtkinlikId);
                        sil.Parameters.AddWithValue("@k", koltukNo);
                        sil.ExecuteNonQuery();

                        var iptal = new MySqlCommand(
                            "UPDATE bilet SET durum='Iptal', iptal_tarihi=NOW() " +
                            "WHERE etkinlik_id=@e AND koltuk_no=@k AND durum='Aktif' LIMIT 1",
                            conn, tx);
                        iptal.Parameters.AddWithValue("@e", _aktifEtkinlikId);
                        iptal.Parameters.AddWithValue("@k", koltukNo);

                        int affected = iptal.ExecuteNonQuery();
                        if (affected == 0)
                        {
                            tx.Rollback();
                            MessageBox.Show("Aktif bilet bulunamadı.");
                            return;
                        }

                        tx.Commit();
                    }
                }

                iptalSecilenKoltuk.BackColor = Color.WhiteSmoke;
                iptalSecilenKoltuk = null;
                txtKoltukNo.Text = "";
                MessageBox.Show("Bilet iptal edildi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("İptal sırasında hata:\n" + ex.Message);
            }
        }

        private void SatisSeciminiTemizle()
        {
            foreach (var seat in selectedSeats)
                if (seat.BackColor == Color.LightGreen)
                    seat.BackColor = Color.WhiteSmoke;

            selectedSeats.Clear();
            txtKoltukNo.Text = "";
            GuncelleBiletTuruBilgisi();
        }

        private void IptalSeciminiTemizle()
        {
            if (iptalSecilenKoltuk != null && iptalSecilenKoltuk.BackColor == Color.Gold)
                iptalSecilenKoltuk.BackColor = Color.Red;

            iptalSecilenKoltuk = null;
            txtKoltukNo.Text = "";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
