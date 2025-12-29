// FrmOdeme.cs
using System; // Temel C# tipleri (DateTime, Guid, Exception vb.)
using System.Collections.Generic; // List<T> gibi koleksiyonlar
using System.Linq; // Where, ToArray gibi LINQ işlemleri
using System.Windows.Forms; // Windows Forms bileşenleri (Form, MessageBox vb.)
using MySqlConnector; // MySQL bağlantı ve komut sınıfları
using beste.Models; // Projedeki Bilet modeli

namespace beste
{
    public partial class FrmOdeme : Form // Ödeme ekranı form sınıfı
    {
        private readonly string cs; // Veritabanı bağlantı cümlesi (connection string)
        private readonly int kullaniciId; // İşlemi yapan aktif kullanıcı ID
        private readonly List<int> biletIdleri; // Ödeme yapılacak biletlerin ID listesi
        private readonly List<string> kisiAdlari; // Çoklu koltuk seçildiyse kişi adları listesi (opsiyonel)
        private readonly decimal toplamTutar; // Toplam ödenecek tutar

        private string bin4 = null; // Kartın ilk 4 hanesi (BIN) - bankayı bulmak için kullanılır
        private string bankaAdi = null; // BIN sorgusundan gelen banka adı
        private string kartSchemesi = null; // Kart şeması (Visa/Mastercard vb.)
        private bool kartAktif = false; // BIN tablosunda kart aktif mi (aktif=1) kontrolü

        // 4 parametreli ctor: kişi adları yoksa null geçer, alttaki ana ctor'a yönlendirir
        public FrmOdeme(string cs, int kullaniciId, List<int> biletIdleri, decimal toplamTutar)
            : this(cs, kullaniciId, biletIdleri, null, toplamTutar)
        {
        }

        // Ana ctor: ödeme formunun tüm gerekli verileri burada alınır
        public FrmOdeme(string cs, int kullaniciId, List<int> biletIdleri, List<string> kisiAdlari, decimal toplamTutar)
        {
            InitializeComponent(); // Designer tarafındaki kontrolleri oluşturur

            this.cs = cs; // connection string'i sakla
            this.kullaniciId = kullaniciId; // kullanıcı id'yi sakla
            this.biletIdleri = biletIdleri ?? new List<int>(); // null gelirse boş listeye çevir (hata önleme)
            this.kisiAdlari = kisiAdlari; // kişi adları listesi (null olabilir)
            this.toplamTutar = toplamTutar; // toplam tutarı sakla

            Text = "Kredi Kartı ile Ödeme"; // Form başlığı

            maskedTextBox1.Mask = "0000 0000 0000 0000"; // Kart no maskesi (16 hane)
            maskedTextBox2.Mask = "000"; // CVC maskesi (3 hane)

            label2.Text = "Banka: -"; // İlk açılışta banka alanı boş gösterilir

            // Event bağlantıları
            Load += FrmOdeme_Load; // Form açılışında yılları/ayları doldur
            maskedTextBox1.TextChanged += maskedTextBox1_TextChanged; // Kart numarası değiştikçe BIN sorgula
            comboBox2.SelectedIndexChanged += (s, e) => AyDoldur(); // Yıl değişince ay listesini yeniden doldur
            button1.Click += button1_Click; // Öde butonu
            button2.Click += (s, e) => Close(); // İptal/Kapat butonu
        }

        // Form ilk açıldığında çalışır: Yıl listesini doldurur, ayları hazırlar
        private void FrmOdeme_Load(object sender, EventArgs e)
        {
            comboBox2.Items.Clear(); // Yıl combobox'ını temizle
            int basYil = DateTime.Now.Year; // Bu yıl

            for (int i = 0; i <= 12; i++) // Bu yıldan başlayarak 12 yıl ileriye kadar ekle
                comboBox2.Items.Add((basYil + i).ToString());

            comboBox2.SelectedIndex = 0; // Varsayılan olarak ilk yılı seç
            AyDoldur(); // Seçilen yıla göre ay listesini doldur

            button1.Enabled = false; // Kart doğrulanana kadar "Öde" kapalı
        }

        // Yıl seçimine göre ay listesini doldurur (geçmiş ayları engeller)
        private void AyDoldur()
        {
            comboBox1.Items.Clear(); // Ay combobox'ını temizle

            int secilenYil = int.Parse(comboBox2.SelectedItem.ToString()); // Seçilen yılı al
            int baslangicAy = (secilenYil == DateTime.Now.Year) ? DateTime.Now.Month : 1; // Bu yılsa geçmiş ayları gösterme

            for (int ay = baslangicAy; ay <= 12; ay++) // Başlangıç ayından 12'ye kadar ekle
                comboBox1.Items.Add(ay.ToString("00"));

            comboBox1.SelectedIndex = 0; // Varsayılan ilk ay
        }

        // Kart numarası yazıldıkça tetiklenir, ilk 4 hane tamamlanınca BIN sorgular
        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            // MaskedTextBox içindeki sadece rakamları al (boşlukları temizler)
            string digits = new string(maskedTextBox1.Text.Where(char.IsDigit).ToArray());

            // 4 haneden azsa daha BIN çıkmadı: her şeyi sıfırla
            if (digits.Length < 4)
            {
                label2.Text = "Banka: -"; // Banka gösterme
                button1.Enabled = false; // Öde kapalı

                kartAktif = false; // Kart aktif değil varsay
                bankaAdi = null; // Banka adı sıfırla
                kartSchemesi = null; // Şema sıfırla
                bin4 = null; // BIN sıfırla
                return;
            }

            bin4 = digits.Substring(0, 4); // İlk 4 haneyi BIN olarak al
            BinSorgula(bin4); // BIN tablosundan banka/şema/aktif bilgisi çek
        }

        // BIN tablosunda kartın bankasını ve aktifliğini sorgular
        private void BinSorgula(string b)
        {
            try
            {
                using (var conn = new MySqlConnection(cs)) // DB bağlantısı oluştur
                {
                    conn.Open(); // Bağlantıyı aç

                    using (var cmd = new MySqlCommand(
                        "SELECT banka_adi, kart_schemesi, aktif FROM kart_bin WHERE bin4=@b LIMIT 1",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@b", b); // Parametre ile güvenli sorgu (SQL Injection önler)

                        using (var rd = cmd.ExecuteReader()) // Sorguyu oku
                        {
                            if (rd.Read()) // Eşleşen BIN bulundu
                            {
                                bankaAdi = rd["banka_adi"]?.ToString(); // Banka adı
                                kartSchemesi = rd["kart_schemesi"]?.ToString(); // Kart şeması
                                kartAktif = Convert.ToBoolean(rd["aktif"]); // Aktif mi?

                                if (kartAktif)
                                {
                                    label2.Text = $"{bankaAdi} ({kartSchemesi})"; // Banka ve şemayı göster
                                    button1.Enabled = true; // Kart aktifse ödeme aç
                                }
                                else
                                {
                                    label2.Text = $"{bankaAdi} ({kartSchemesi}) - PASİF"; // Pasif uyarısı
                                    button1.Enabled = false; // Pasifse ödeme kapalı
                                }
                            }
                            else // BIN bulunamadı
                            {
                                label2.Text = "Banka: Tanımsız"; // Tanımsız banka göster
                                kartAktif = false; // Aktif değil
                                button1.Enabled = false; // Ödeme kapalı
                            }
                        }
                    }
                }
            }
            catch (Exception ex) // DB hatası vb.
            {
                label2.Text = "BIN hata: " + ex.Message; // Hata mesajı label'a yaz
                kartAktif = false; // Güvenlik: ödeme kapat
                button1.Enabled = false;
            }
        }

        // "Öde" butonuna basıldığında çalışır: doğrulamalar, bakiye düşme, ödeme kaydı ve QR işlemi
        private void button1_Click(object sender, EventArgs e)
        {
            // Kart numarasını sadece rakamlar olarak al
            string kartDigits = new string(maskedTextBox1.Text.Where(char.IsDigit).ToArray());

            // Kart 16 hane olmalı
            if (kartDigits.Length != 16)
            {
                MessageBox.Show("Kart numarası 16 hane olmalı.");
                return; // işlem durur
            }

            // Kart sahibi ad soyad zorunlu
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Kart sahibinin ad soyadı boş olamaz.");
                return;
            }

            // CVC sadece rakamları al
            string cvcDigits = new string(maskedTextBox2.Text.Where(char.IsDigit).ToArray());

            // CVC en az 3 hane olmalı
            if (cvcDigits.Length < 3)
            {
                MessageBox.Show("CVC en az 3 hane olmalı.");
                return;
            }

            // Son kullanım ay/yıl al
            int ay = int.Parse(comboBox1.SelectedItem.ToString());
            int yil = int.Parse(comboBox2.SelectedItem.ToString());

            // Son kullanım tarihi kontrolü: ayın son günü bugünden küçükse kart geçmiş sayılır
            DateTime sonGun = new DateTime(yil, ay, DateTime.DaysInMonth(yil, ay));
            if (sonGun < DateTime.Today)
            {
                MessageBox.Show("Son kullanım tarihi geçmiş.");
                return;
            }

            // BIN bulunmamışsa veya kart pasifse ödeme alınmaz
            if (!kartAktif || string.IsNullOrWhiteSpace(bin4))
            {
                MessageBox.Show("Kart pasif veya tanımsız. Ödeme alınamaz.");
                return;
            }

            // Ödenecek bilet listesi boş olamaz
            if (biletIdleri == null || biletIdleri.Count == 0)
            {
                MessageBox.Show("Ödenecek bilet bulunamadı.");
                return;
            }

            // Kartı maskele (güvenlik): 1234 **** **** 5678 gibi
            string mask = kartDigits.Substring(0, 4) + " **** **** " + kartDigits.Substring(12, 4);

            // Toplam tutarı bilet sayısına bölerek parça tutar hesapla (kuruş yuvarlama kontrolü)
            decimal parca = Math.Floor((toplamTutar / biletIdleri.Count) * 100m) / 100m; // 2 ondalığa kırp
            decimal sonParca = toplamTutar - parca * (biletIdleri.Count - 1); // kalan kuruşu son bilete ekle

            bool odemeBasarili = false; // ödeme tamamlandı mı flag'i

            try
            {
                using (var conn = new MySqlConnection(cs)) // DB bağlantısı
                {
                    conn.Open(); // aç

                    using (var tx = conn.BeginTransaction()) // Transaction başlat: ya hepsi olur ya hiçbiri
                    {
                        // 1) Kart bakiyesini düş: aktif olmalı ve bakiye yeterli olmalı
                        using (var cmdBakiye = new MySqlCommand(
                            @"UPDATE kart_bin
                              SET bakiye = bakiye - @tutar
                              WHERE bin4 = @bin4 AND aktif = 1 AND bakiye >= @tutar;",
                            conn, tx))
                        {
                            cmdBakiye.Parameters.AddWithValue("@tutar", toplamTutar); // toplam tutar düşülecek
                            cmdBakiye.Parameters.AddWithValue("@bin4", bin4); // hangi kart (BIN)

                            int affected = cmdBakiye.ExecuteNonQuery(); // etkilenen satır sayısı
                            if (affected == 0) // 0 ise: ya bakiye yetmedi ya kart pasif
                            {
                                tx.Rollback(); // tüm işlemleri geri al
                                MessageBox.Show("Yetersiz bakiye veya kart pasif. Ödeme alınamadı.");
                                return;
                            }
                        }

                        // 2) Her bilet için odeme tablosuna kayıt at
                        for (int i = 0; i < biletIdleri.Count; i++)
                        {
                            int biletId = biletIdleri[i]; // o anki bilet id
                            decimal tutar = (i == biletIdleri.Count - 1) ? sonParca : parca; // son bilete kalan kuruşu ekle

                            using (var cmdOdeme = new MySqlCommand(@"
INSERT INTO odeme
(bilet_id, odeme_tutari, odeme_yontemi, durum, odeme_tarihi, kullanici_id,
 bin4, banka_adi, kart_schemesi, kart_mask, son_kullanim_ay, son_kullanim_yil,
 taksitli, taksit_sayisi)
VALUES
(@bilet, @tutar, 'Kredi kartı', 'Tamamlandı', NOW(), @kid,
 @bin4, @banka, @schema, @mask, @ay, @yil,
 0, NULL);", conn, tx))
                            {
                                cmdOdeme.Parameters.AddWithValue("@bilet", biletId); // bilet id
                                cmdOdeme.Parameters.AddWithValue("@tutar", tutar); // bilet tutarı
                                cmdOdeme.Parameters.AddWithValue("@kid", kullaniciId); // kullanıcı id

                                cmdOdeme.Parameters.AddWithValue("@bin4", bin4); // kart bin
                                cmdOdeme.Parameters.AddWithValue("@banka", (object)bankaAdi ?? DBNull.Value); // banka adı
                                cmdOdeme.Parameters.AddWithValue("@schema", (object)kartSchemesi ?? DBNull.Value); // şema
                                cmdOdeme.Parameters.AddWithValue("@mask", mask); // maskelenmiş kart no

                                cmdOdeme.Parameters.AddWithValue("@ay", ay); // son kullanım ay
                                cmdOdeme.Parameters.AddWithValue("@yil", yil); // son kullanım yıl

                                cmdOdeme.ExecuteNonQuery(); // ödeme kaydını DB'ye yaz
                            }
                        }

                        tx.Commit(); // bakiye düştü + tüm odeme kayıtları yazıldı => onayla
                        odemeBasarili = true; // ödeme başarılı
                    }

                    // Ödeme başarılıysa QR ekranı aç (her bilet için ayrı QR)
                    if (odemeBasarili)
                    {
                        try
                        {
                            for (int i = 0; i < biletIdleri.Count; i++)
                            {
                                int biletId = biletIdleri[i]; // bilet id
                                decimal biletTutar = (i == biletIdleri.Count - 1) ? sonParca : parca; // bu bilete düşen tutar

                                // Çoklu isim varsa i. ismi al; yoksa kart sahibinin adını kullan
                                string kisiAdi =
                                    (kisiAdlari != null && i < kisiAdlari.Count && !string.IsNullOrWhiteSpace(kisiAdlari[i]))
                                        ? kisiAdlari[i].Trim()
                                        : textBox1.Text.Trim();

                                // PNR üret (12 karakter): bilet numarası gibi kullanılacak
                                string pnr = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

                                // DB'den bilet+etkinlik bilgilerini çekip Bilet modeli oluştur
                                var qrBilet = DbdenBiletCek(conn, biletId, pnr, biletTutar, kisiAdi);

                                // QR formunu aç (modal)
                                new FrmBiletQR(qrBilet).ShowDialog();
                            }
                        }
                        catch (Exception qrEx)
                        {
                            // Ödeme tamamlanmış olsa bile QR üretimde hata olabilir
                            MessageBox.Show("Ödeme alındı ancak QR oluşturulurken hata oluştu:\n" + qrEx.Message);
                        }

                        MessageBox.Show("Ödeme kaydedildi."); // Kullanıcıya bilgi ver
                        Close(); // Ödeme formunu kapat
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ödeme hatası:\n" + ex.Message); // Genel hata yakalama
            }
        }

        // DB'den biletin bağlı olduğu etkinlik bilgilerini çekip Bilet modelini doldurur
        private Bilet DbdenBiletCek(MySqlConnection conn, int biletId, string pnr, decimal biletFiyati, string musteriAdSoyad)
        {
            string etkinlikAdi = "Etkinlik"; // Varsayılan değer (DB'den gelmezse)
            DateTime tarih = DateTime.Now; // Varsayılan tarih (DB'den gelmezse)
            string salon = "-"; // Varsayılan salon/konum
            string koltuk = "-"; // Varsayılan koltuk

            // bilet tablosundan koltuk_no ve etkinlik_id ile etkinlik tablosundan ad/tarih/konum çekilir
            using (var cmd = new MySqlCommand(@"
SELECT 
    b.koltuk_no, 
    e.etkinlik_adi,
    e.tarih,
    e.konum
FROM bilet b
JOIN etkinlik e ON e.etkinlik_id = b.etkinlik_id
WHERE b.bilet_id = @id
LIMIT 1;", conn))
            {
                cmd.Parameters.AddWithValue("@id", biletId); // istenen bilet

                using (var r = cmd.ExecuteReader()) // sorguyu çalıştır
                {
                    if (r.Read()) // kayıt bulunduysa
                    {
                        koltuk = r["koltuk_no"] != DBNull.Value ? r["koltuk_no"].ToString() : "-"; // koltuk bilgisi
                        etkinlikAdi = r["etkinlik_adi"] != DBNull.Value ? r["etkinlik_adi"].ToString() : "Etkinlik"; // etkinlik adı

                        if (r["tarih"] != DBNull.Value) // tarih boş değilse
                            tarih = Convert.ToDateTime(r["tarih"]); // etkinlik tarihini al

                        salon = r["konum"] != DBNull.Value ? r["konum"].ToString() : "-"; // salon/konum al
                        if (string.IsNullOrWhiteSpace(salon)) // boş gelirse
                            salon = "-"; // ekranda boş kalmasın
                    }
                }
            }

            // QR ekranında kullanılacak Bilet modeli
            return new Bilet
            {
                BiletNo = pnr, // Üretilen PNR kodu
                EtkinlikAdi = etkinlikAdi, // DB'den gelen etkinlik adı
                KategoriAdi = "Etkinlik", // Şimdilik sabit (kategori çekmiyorsun)
                OyunAdi = etkinlikAdi, // Oyun adı alanı varsa aynı değeri ver

                Koltuk = koltuk, // DB'den gelen koltuk no
                Salon = salon, // DB'den gelen konum

                MusteriAdi = musteriAdSoyad, // Bilet sahibi
                Fiyat = biletFiyati, // Bu bilete düşen fiyat

                BaslangicZamani = tarih, // Etkinlik başlangıç zamanı (DB'den)
                BitisZamani = tarih.AddHours(2), // Varsayılan olarak 2 saat eklenmiş bitiş zamanı

                TiyatroAdi = "Mekan", // Şimdilik sabit
                TiyatroAdresi = "" // Şimdilik boş
            };
        }

        // Designer'da var olduğu için boş bırakılmış event handler (kullanılmıyor)
        private void label2_Click(object sender, EventArgs e) { }

        // Designer'da var olduğu için boş bırakılmış event handler (kullanılmıyor)
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
