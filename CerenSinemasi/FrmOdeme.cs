using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MySqlConnector;
using beste.Models;

namespace beste
{
    public partial class FrmOdeme : Form
    {
        private readonly string cs;
        private readonly int kullaniciId;
        private readonly List<int> biletIdleri;
        private readonly decimal toplamTutar;

        private string bin4 = null;
        private string bankaAdi = null;
        private string kartSchemesi = null;
        private bool kartAktif = false;

        public FrmOdeme(string cs, int kullaniciId, List<int> biletIdleri, decimal toplamTutar)
        {
            InitializeComponent();

            this.cs = cs;
            this.kullaniciId = kullaniciId;
            this.biletIdleri = biletIdleri ?? new List<int>();
            this.toplamTutar = toplamTutar;

            this.Text = "Kredi Kartı ile Ödeme";

            maskedTextBox1.Mask = "0000 0000 0000 0000";
            maskedTextBox2.Mask = "000";

            label2.Text = "Banka: -";

            this.Load += FrmOdeme_Load;
            maskedTextBox1.TextChanged += maskedTextBox1_TextChanged;
            comboBox2.SelectedIndexChanged += (s, e) => AyDoldur();
            button1.Click += button1_Click;
            button2.Click += (s, e) => this.Close();
        }

        private void FrmOdeme_Load(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            int basYil = DateTime.Now.Year;
            for (int i = 0; i <= 12; i++)
                comboBox2.Items.Add((basYil + i).ToString());

            comboBox2.SelectedIndex = 0;
            AyDoldur();

            button1.Enabled = false;
        }

        private void AyDoldur()
        {
            comboBox1.Items.Clear();

            int secilenYil = int.Parse(comboBox2.SelectedItem.ToString());
            int baslangicAy = (secilenYil == DateTime.Now.Year) ? DateTime.Now.Month : 1;

            for (int ay = baslangicAy; ay <= 12; ay++)
                comboBox1.Items.Add(ay.ToString("00"));

            comboBox1.SelectedIndex = 0;
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            string digits = new string(maskedTextBox1.Text.Where(char.IsDigit).ToArray());

            if (digits.Length < 4)
            {
                label2.Text = "Banka: -";
                button1.Enabled = false;
                kartAktif = false;
                bankaAdi = null;
                kartSchemesi = null;
                bin4 = null;
                return;
            }

            bin4 = digits.Substring(0, 4);
            BinSorgula(bin4);
        }

        private void BinSorgula(string b)
        {
            try
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open();

                    var cmd = new MySqlCommand(
                        "SELECT banka_adi, kart_schemesi, aktif FROM kart_bin WHERE bin4=@b",
                        conn);

                    cmd.Parameters.AddWithValue("@b", b);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            bankaAdi = rd["banka_adi"].ToString();
                            kartSchemesi = rd["kart_schemesi"].ToString();
                            kartAktif = Convert.ToBoolean(rd["aktif"]);

                            if (kartAktif)
                            {
                                label2.Text = $"{bankaAdi} ({kartSchemesi})";
                                button1.Enabled = true;
                            }
                            else
                            {
                                label2.Text = $"{bankaAdi} ({kartSchemesi}) - PASİF";
                                button1.Enabled = false;
                            }
                        }
                        else
                        {
                            label2.Text = "Banka: Tanımsız";
                            kartAktif = false;
                            button1.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                label2.Text = "BIN hata: " + ex.Message;
                kartAktif = false;
                button1.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string kartDigits = new string(maskedTextBox1.Text.Where(char.IsDigit).ToArray());
            if (kartDigits.Length != 16)
            {
                MessageBox.Show("Kart numarası 16 hane olmalı.");
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Kart sahibinin ad soyadı boş olamaz.");
                return;
            }

            string cvcDigits = new string(maskedTextBox2.Text.Where(char.IsDigit).ToArray());
            if (cvcDigits.Length < 3)
            {
                MessageBox.Show("CVC en az 3 hane olmalı.");
                return;
            }

            int ay = int.Parse(comboBox1.SelectedItem.ToString());
            int yil = int.Parse(comboBox2.SelectedItem.ToString());

            DateTime sonGun = new DateTime(yil, ay, DateTime.DaysInMonth(yil, ay));
            if (sonGun < DateTime.Today)
            {
                MessageBox.Show("Son kullanım tarihi geçmiş.");
                return;
            }

            if (!kartAktif)
            {
                MessageBox.Show("Kart pasif, ödeme alınamaz.");
                return;
            }

            if (biletIdleri == null || biletIdleri.Count == 0)
            {
                MessageBox.Show("Ödenecek bilet bulunamadı.");
                return;
            }

            string mask = kartDigits.Substring(0, 4) + " **** **** " + kartDigits.Substring(12, 4);

            // Tutarı bilet başına böl (kuruş kaymaması için)
            decimal parca = Math.Floor((toplamTutar / biletIdleri.Count) * 100m) / 100m;
            decimal sonParca = toplamTutar - parca * (biletIdleri.Count - 1);

            try
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open();

                    using (var tx = conn.BeginTransaction())
                    {
                        // =========================
                        // ✅ BAKİYE KONTROLÜ (SENİN KODUN) - INSERT'lerden ÖNCE
                        // =========================
                        decimal bakiye = 0;

                        var bakiyeCmd = new MySqlCommand(
                            "SELECT bakiye FROM kart_bin WHERE bin4=@b",
                            conn, tx);

                        bakiyeCmd.Parameters.AddWithValue("@b", bin4);

                        object bakiyeObj = bakiyeCmd.ExecuteScalar();
                        if (bakiyeObj == null)
                        {
                            MessageBox.Show("Kart bakiyesi bulunamadı.");
                            tx.Rollback();
                            return;
                        }

                        bakiye = Convert.ToDecimal(bakiyeObj);

                        if (bakiye < toplamTutar)
                        {
                            MessageBox.Show("Yetersiz bakiye. Ödeme alınamadı.");
                            tx.Rollback();
                            return;
                        }
                        // =========================

                        // Ödeme kayıtları
                        for (int i = 0; i < biletIdleri.Count; i++)
                        {
                            int biletId = biletIdleri[i];
                            decimal tutar = (i == biletIdleri.Count - 1) ? sonParca : parca;

                            var cmd = new MySqlCommand(@"
INSERT INTO odeme (bilet_id, odeme_tutari, odeme_yontemi, durum, odeme_tarihi, kullanici_id,
                   bin4, banka_adi, kart_schemesi, kart_mask, son_kullanim_ay, son_kullanim_yil,
                   taksitli, taksit_sayisi)
VALUES (@bilet, @tutar, 'Kredi kartı', 'Tamamlandı', NOW(), @kid,
        @bin4, @banka, @schema, @mask, @ay, @yil,
        0, NULL);", conn, tx);

                            cmd.Parameters.AddWithValue("@bilet", biletId);
                            cmd.Parameters.AddWithValue("@tutar", tutar);
                            cmd.Parameters.AddWithValue("@kid", kullaniciId);

                            cmd.Parameters.AddWithValue("@bin4", bin4);
                            cmd.Parameters.AddWithValue("@banka", (object)bankaAdi ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@schema", (object)kartSchemesi ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@mask", mask);

                            cmd.Parameters.AddWithValue("@ay", ay);
                            cmd.Parameters.AddWithValue("@yil", yil);

                            cmd.ExecuteNonQuery();
                        }

                        // ✅ Ödeme başarılıysa bakiyeden düş (commit’ten önce)
                        var bakiyeDusCmd = new MySqlCommand(
                            "UPDATE kart_bin SET bakiye = bakiye - @tutar WHERE bin4=@b",
                            conn, tx);

                        bakiyeDusCmd.Parameters.AddWithValue("@tutar", toplamTutar);
                        bakiyeDusCmd.Parameters.AddWithValue("@b", bin4);

                        bakiyeDusCmd.ExecuteNonQuery();

                        tx.Commit();
                    }

                    // ✅ Commit sonrası QR (bakiye yetersizse bu kısma asla gelmez)
                    for (int i = 0; i < biletIdleri.Count; i++)
                    {
                        int biletId = biletIdleri[i];
                        decimal biletTutar = (i == biletIdleri.Count - 1) ? sonParca : parca;

                        string pnr = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
                        var qrBilet = DbdenBiletCek(conn, biletId, pnr, biletTutar, textBox1.Text.Trim());

                        new FrmBiletQR(qrBilet).ShowDialog();
                    }
                }

                MessageBox.Show("Ödeme kaydedildi.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ödeme hatası:\n" + ex.Message);
            }
        }

        // ✅ “Etkinlik / Salon / Koltuk” DB’den çekiliyor (koltuk A1 formatı b.koltuk_no’dan gelir)
        private Bilet DbdenBiletCek(MySqlConnection conn, int biletId, string pnr, decimal biletFiyati, string musteriAdSoyad)
        {
            string etkinlikAdi = "Etkinlik";
            DateTime tarih = DateTime.Now;
            string salon = "-";
            string koltuk = "-";

            var cmd = new MySqlCommand(@"
SELECT 
    b.koltuk_no,
    e.etkinlik_adi,
    e.tarih,
    e.konum
FROM bilet b
JOIN etkinlik e ON e.etkinlik_id = b.etkinlik_id
WHERE b.bilet_id = @id
LIMIT 1;", conn);

            cmd.Parameters.AddWithValue("@id", biletId);

            using (var r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    koltuk = r["koltuk_no"]?.ToString() ?? "-";
                    etkinlikAdi = r["etkinlik_adi"]?.ToString() ?? "Etkinlik";

                    if (r["tarih"] != DBNull.Value)
                        tarih = Convert.ToDateTime(r["tarih"]);

                    salon = r["konum"]?.ToString();
                    if (string.IsNullOrWhiteSpace(salon))
                        salon = "-";
                }
            }

            return new Bilet
            {
                BiletNo = pnr,
                EtkinlikAdi = etkinlikAdi,
                KategoriAdi = "Etkinlik",
                OyunAdi = etkinlikAdi,

                Koltuk = koltuk,
                Salon = salon,

                MusteriAdi = musteriAdSoyad,
                Fiyat = biletFiyati,

                BaslangicZamani = tarih,
                BitisZamani = tarih.AddHours(2),

                TiyatroAdi = "Mekan",
                TiyatroAdresi = ""
            };
        }

        // --- Designer hata vermesin diye boş event'ler ---
        private void label2_Click(object sender, EventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
