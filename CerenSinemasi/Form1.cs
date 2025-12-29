// Form1.cs  (Designer ile UYUMLU - TAM DOSYA) // Bu dosya koltuk seçim + satın alma + iptal + ödeme akışını yönetir

using System; // Temel tipler (DateTime, Guid, Exception vb.)
using System.Collections.Generic; // List, Dictionary gibi koleksiyonlar
using System.Drawing; // Renk (Color) ve çizim sınıfları
using System.Linq; // LINQ (Select, Any, OrderBy vb.)
using System.Windows.Forms; // WinForms bileşenleri (Form, Button, MessageBox vb.)
using MySqlConnector; // MySQL bağlantı ve sorgu işlemleri

namespace beste
{
    public partial class Form1 : Form // Koltuk seçim ve satın alma ekranı
    {
        private readonly Form _oncekiForm; // Geri tuşuna basınca dönülecek önceki form

        private readonly List<Button> selectedSeats = new List<Button>(); // Satın alma için seçilen koltuk butonları listesi
        private Panel panelKoltuklar = null; // Koltuk butonlarının yer aldığı panel

        private bool iptalModu = false; // İptal modu açık mı?
        private Button iptalSecilenKoltuk = null; // İptal için seçilen koltuk butonu
        private CheckBox chkIptal = null; // İptal modu checkbox'ı
        private Button btnIptal = null; // “Bileti İptal Et” butonu

        private readonly Dictionary<string, string> _koltukIsimleri =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // KoltukNo -> AdSoyad eşlemesi (çoklu bilet sahibini tutar)

        private readonly string cs =
            "Server=localhost;Database=bilet_sistemi;User Id=root;Password=Ghezzal18."; // MySQL bağlantı cümlesi

        private readonly int _aktifEtkinlikId; // Seçilen etkinliğin ID'si (koltuk işlemleri bu ID ile yapılır)
        private readonly int _aktifKullaniciId; // Aktif kullanıcı ID (bilet işlemi kime ait)

        // Designer patlamasın diye parametresiz ctor // Tasarım açılırken hata vermesin diye boş constructor
        public Form1()
        {
            InitializeComponent(); // Designer kontrollerini oluşturur
            _aktifEtkinlikId = 0; // Varsayılan (geçersiz) etkinlik id
            _aktifKullaniciId = 0; // Varsayılan (geçersiz) kullanıcı id
            _oncekiForm = null; // Önceki form yok
        }

        public Form1(int etkinlikId, int kullaniciId, Form oncekiForm) // Gerçek kullanımda etkinlik ve kullanıcı parametreleri ile açılır
        {
            InitializeComponent(); // Kontrolleri oluşturur
            _aktifEtkinlikId = etkinlikId; // Aktif etkinliği sakla
            _aktifKullaniciId = kullaniciId; // Aktif kullanıcıyı sakla
            _oncekiForm = oncekiForm; // Geri dönüş için formu sakla
        }

        private void Form1_Load(object sender, EventArgs e) // Form ekrana yüklenince çalışan event
        {
            // Designer dosyanda btnGeri alanı var ama InitializeComponent içinde yok. // Designer'da field var ama kontrol eklenmemiş olabilir
            // Null ise burada oluşturup ekliyoruz (Designer'a dokunmadan fix). // Bu sayede Designer bozulmadan geri butonu gelir
            if (btnGeri == null) // btnGeri kontrolü oluşturulmamışsa
            {
                btnGeri = new Button // Yeni geri butonu oluştur
                {
                    Name = "btnGeri", // Kontrol adı
                    Text = "←", // Buton yazısı
                    Width = 40, // Genişlik
                    Height = 28, // Yükseklik
                    Left = 10, // Sol konum
                    Top = 10, // Üst konum
                    TabStop = false // Tab ile odaklanmasın
                };
                btnGeri.Click += BtnGeri_Click; // Geri butonuna tıklandığında geri fonksiyonu çalışsın
                Controls.Add(btnGeri); // Forma ekle
                btnGeri.BringToFront(); // En öne al
            }
            else // btnGeri zaten varsa event'i garanti şekilde bağla
            {
                btnGeri.Click -= BtnGeri_Click; // Çift bağlanmayı engelle
                btnGeri.Click += BtnGeri_Click; // Tekrar bağla
            }

            if (_aktifEtkinlikId <= 0) // Etkinlik seçilmediyse
            {
                MessageBox.Show("Etkinlik seçilmeden koltuk ekranı açılamaz."); // Uyarı ver
                Close(); // Formu kapat
                return; // Devam etme
            }

            // txtKoltukNo kullanıcı yazmasın // Koltuk no kullanıcı tarafından elle yazılmasın
            txtKoltukNo.ReadOnly = true; // Sadece okunabilir yap

            // comboBox1 bilet türü/özet için: kullanıcı değiştirmesin // ComboBox sadece bilgi/özet göstermek için
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; // Liste dışı yazmayı kapat
            comboBox1.Enabled = false; // Kullanıcı değiştiremesin

            // Koltuk panelini oluştur // Koltuk butonlarını dinamik üret
            KoltuklariOlustur();

            // DB'ye göre koltukları boyat // Dolu koltuklar kırmızı, boşlar beyaz
            KoltukRenkleriniDbyeGoreAyarla();

            // İptal kontrolleri // İptal modu checkbox ve iptal butonu kurulur
            IptalKontrolleriniKur();

            // İlk özet // İlk açılışta toplam/indirim yazısı güncellenir
            GuncelleBiletTuruBilgisi();
        }

        private void BtnGeri_Click(object sender, EventArgs e) // Geri butonuna basılınca çalışan event
        {
            if (_oncekiForm != null) _oncekiForm.Show(); // Önceki form varsa yeniden göster
            Close(); // Bu formu kapat
        }

        private void KoltuklariOlustur() // Koltuk butonlarını panel içinde üretir
        {
            int rows = 6; // A-F // Satır sayısı
            int cols = 8; // 1-8 // Sütun sayısı
            int size = 40; // Buton boyutu
            int gap = 5; // Butonlar arası boşluk

            if (panelKoltuklar != null && Controls.Contains(panelKoltuklar)) // Panel daha önce eklendiyse
                Controls.Remove(panelKoltuklar); // Paneli kaldır (yeniden oluşturacağız)

            // Paneli sağ tarafa taşımayalım; solda kalsın // Panel formun solunda duracak
            panelKoltuklar = new Panel // Yeni panel oluştur
            {
                Name = "panelKoltuklar", // Panel adı
                Left = 12, // Sol
                Top = 50, // Üst
                Width = cols * size + (cols - 1) * gap, // Panel genişliği hesap
                Height = rows * size + (rows - 1) * gap, // Panel yüksekliği hesap
                BorderStyle = BorderStyle.FixedSingle // Kenarlık
            };

            Controls.Add(panelKoltuklar); // Paneli forma ekle

            for (int i = 0; i < rows; i++) // Satırlar (A-F)
            {
                for (int j = 0; j < cols; j++) // Sütunlar (1-8)
                {
                    Button btn = new Button // Koltuk butonu oluştur
                    {
                        Width = size, // Buton genişliği
                        Height = size, // Buton yüksekliği
                        Left = j * (size + gap), // X konumu
                        Top = i * (size + gap), // Y konumu
                        Text = string.Concat((char)('A' + i), (j + 1).ToString()), // Koltuk kodu: A1, A2, ...
                        BackColor = Color.WhiteSmoke, // Boş koltuk rengi
                        Tag = "seat" // Bu kontrolün koltuk olduğunu belirtmek için etiket
                    };

                    btn.Click += Seat_Click; // Koltuğa tıklanınca Seat_Click çalışsın
                    panelKoltuklar.Controls.Add(btn); // Panel içine ekle
                }
            }
        }

        private void KoltukRenkleriniDbyeGoreAyarla() // DB’de dolu olan koltukları kırmızı yapar
        {
            if (panelKoltuklar == null) return; // Panel yoksa çık

            try // Hata olursa catch'e düş
            {
                using (MySqlConnection conn = new MySqlConnection(cs)) // DB bağlantısı
                {
                    conn.Open(); // Bağlantıyı aç

                    foreach (Control c in panelKoltuklar.Controls) // Paneldeki tüm kontrolleri gez
                    {
                        Button koltuk = c as Button; // Button mı?
                        if (koltuk == null) continue; // Değilse geç

                        int sayi; // Doluluk sayısı

                        using (MySqlCommand cmd = new MySqlCommand( // Bu koltuk dolu mu sorgusu
                            "SELECT COUNT(*) FROM bilet " +
                            "WHERE etkinlik_id=@e AND koltuk_no=@k " +
                            "AND durum IN ('Aktif','Rezerve')", conn))
                        {
                            cmd.Parameters.AddWithValue("@e", _aktifEtkinlikId); // Etkinlik parametresi
                            cmd.Parameters.AddWithValue("@k", koltuk.Text); // Koltuk parametresi (A1 vb.)
                            sayi = Convert.ToInt32(cmd.ExecuteScalar()); // COUNT(*) sonucu
                        }

                        koltuk.BackColor = (sayi > 0) ? Color.Red : Color.WhiteSmoke; // Doluysa kırmızı, boşsa beyaz
                    }
                }
            }
            catch (Exception ex) // DB bağlantı/sorgu hatası
            {
                MessageBox.Show("Koltuk durumu okunamadı:\n" + ex.Message); // Hata mesajı
            }
        }

        private void IptalKontrolleriniKur() // İptal modu checkbox ve iptal butonunu oluşturur
        {
            // Daha önce eklenmiş mi? // Aynı kontrolleri tekrar eklememek için kontrol
            if (Controls.Find("chkIptal", true).Length > 0) return; // Varsa çık

            chkIptal = new CheckBox // İptal modu checkbox
            {
                Name = "chkIptal", // Adı
                Text = "İptal Modu", // Görünen yazı
                AutoSize = true, // Otomatik boyut
                Left = btnSave.Left, // Satın Al butonunun hizasında
                Top = btnSave.Bottom + 8 // Satın Al butonunun altına
            };

            btnIptal = new Button // İptal butonu
            {
                Name = "btnIptal", // Adı
                Text = "Bileti İptal Et", // Görünen yazı
                Width = btnSave.Width, // Satın Al ile aynı genişlik
                Height = 30, // Yükseklik
                Left = btnSave.Left, // Hizalama
                Top = chkIptal.Bottom + 6, // Checkbox altına
                Visible = false // İlk başta görünmesin (iptal modu açılınca görünsün)
            };

            chkIptal.CheckedChanged += (s, a) => // Checkbox işaretlenince çalışan event
            {
                iptalModu = chkIptal.Checked; // İptal modu true/false
                btnIptal.Visible = iptalModu; // İptal butonunu göster/gizle
                btnSave.Enabled = !iptalModu; // İptal modundayken Satın Al kapalı olsun

                IptalSeciminiTemizle(); // İptal seçimini sıfırla
                SatisSeciminiTemizle(); // Satın alma seçimini sıfırla
            };

            btnIptal.Click += btnIptal_Click; // İptal butonu tıklanınca iptal fonksiyonu çalışsın

            Controls.Add(chkIptal); // Checkbox'ı forma ekle
            Controls.Add(btnIptal); // İptal butonunu forma ekle

            chkIptal.BringToFront(); // Öne al
            btnIptal.BringToFront(); // Öne al
        }

        private void Seat_Click(object sender, EventArgs e) // Koltuk butonuna tıklanınca çalışır
        {
            Button tiklanan = (Button)sender; // Tıklanan butonu al

            if (iptalModu) // İptal modu açıksa
            {
                if (tiklanan.BackColor != Color.Red && tiklanan.BackColor != Color.Gold) // Sadece dolu (kırmızı) koltuk iptal seçilebilir
                {
                    MessageBox.Show("İptal için satılmış (kırmızı) koltuk seçmelisiniz."); // Uyarı
                    return; // İşlem durur
                }

                if (iptalSecilenKoltuk != null && iptalSecilenKoltuk != tiklanan) // Daha önce başka koltuk seçildiyse
                    iptalSecilenKoltuk.BackColor = Color.Red; // Eski seçimi geri kırmızı yap

                if (iptalSecilenKoltuk == tiklanan && tiklanan.BackColor == Color.Gold) // Aynı koltuğa tekrar basılırsa seçim iptal edilsin
                {
                    tiklanan.BackColor = Color.Red; // Tekrar kırmızıya çevir
                    iptalSecilenKoltuk = null; // Seçimi temizle
                    txtKoltukNo.Text = ""; // Koltuk no textbox temizle
                    return; // Çık
                }

                iptalSecilenKoltuk = tiklanan; // İptal için seçilen koltuk
                iptalSecilenKoltuk.BackColor = Color.Gold; // Seçili iptal koltuğu altın rengi
                txtKoltukNo.Text = iptalSecilenKoltuk.Text; // Koltuk no yaz
                return; // İptal modunda satın alma akışına girme
            }

            if (tiklanan.BackColor == Color.Red) // Koltuk kırmızı ise dolu demektir
            {
                MessageBox.Show("Bu koltuk dolu."); // Uyarı
                return; // Seçme
            }

            if (selectedSeats.Contains(tiklanan)) // Koltuk zaten seçiliyse
            {
                selectedSeats.Remove(tiklanan); // Listeden çıkar
                tiklanan.BackColor = Color.WhiteSmoke; // Rengini boş rengine döndür

                if (_koltukIsimleri.ContainsKey(tiklanan.Text)) // Bu koltuk için isim kaydı varsa
                    _koltukIsimleri.Remove(tiklanan.Text); // İsim kaydını sil
            }
            else // Koltuk seçili değilse seç
            {
                selectedSeats.Add(tiklanan); // Listeye ekle
                tiklanan.BackColor = Color.LightGreen; // Seçili koltuk rengi yeşil
            }

            txtKoltukNo.Text = string.Join(", ", selectedSeats.Select(x => x.Text)); // Seçilen tüm koltukları textbox'a yaz

            if (selectedSeats.Count <= 1) // Tek koltuk seçiliyse
            {
                txtAdSoyad.Enabled = true; // Tek kişi adı girişi açık
            }
            else // Çoklu koltuk seçiliyse
            {
                txtAdSoyad.Text = ""; // Tekli alanı temizle
                txtAdSoyad.Enabled = false; // Tekli alanı kapat (isimler ayrı formdan alınacak)
            }

            GuncelleBiletTuruBilgisi(); // Toplam/indirim özetini güncelle
        }

        private decimal GetKoltukFiyati(char sira) // Satır harfine göre koltuk fiyatını döndürür
        {
            switch (char.ToUpper(sira)) // Harfi büyütüp karşılaştır
            {
                case 'A': return 500; // A sırası fiyatı
                case 'B': return 450; // B sırası fiyatı
                case 'C': return 400; // C sırası fiyatı
                case 'D': return 350; // D sırası fiyatı
                case 'E': return 300; // E sırası fiyatı
                case 'F': return 250; // F sırası fiyatı
                default: return 0; // Tanımsızsa 0
            }
        }

        private decimal GetKoltukIndirimi(char sira) // Satır harfine göre indirim tutarını döndürür
        {
            switch (char.ToUpper(sira)) // Harfi büyütüp karşılaştır
            {
                case 'A': return 50; // A sırası indirim
                case 'B': return 40; // B sırası indirim
                case 'C': return 30; // C sırası indirim
                case 'D': return 20; // D sırası indirim
                case 'E': return 10; // E sırası indirim
                case 'F': return 5;  // F sırası indirim
                default: return 0; // Tanımsızsa 0
            }
        }

        private void HesaplaToplamTutar(List<string> koltuklar, // Koltuk listesine göre toplam/indirim/ödenecek hesaplar
            out decimal toplamFiyat, // Toplam fiyat çıktısı
            out decimal toplamIndirim, // Toplam indirim çıktısı
            out decimal odenecekTutar) // Ödenecek tutar çıktısı
        {
            toplamFiyat = 0; // Başlangıç toplam fiyat
            toplamIndirim = 0; // Başlangıç toplam indirim

            if (koltuklar == null || koltuklar.Count == 0) // Liste boşsa
            {
                odenecekTutar = 0; // Ödenecek 0
                return; // Çık
            }

            foreach (string koltuk in koltuklar) // Her koltuğu gez
            {
                if (string.IsNullOrWhiteSpace(koltuk)) continue; // Boşsa geç

                char sira = char.ToUpper(koltuk[0]); // Koltuk kodunun ilk harfi sıra (A,B,C...)
                toplamFiyat += GetKoltukFiyati(sira); // Fiyat ekle
                toplamIndirim += GetKoltukIndirimi(sira); // İndirim ekle
            }

            if (koltuklar.Count < 2) toplamIndirim = 0; // 2’den az koltukta indirim yok
            odenecekTutar = toplamFiyat - toplamIndirim; // Ödenecek = fiyat - indirim
        }

        private void GuncelleBiletTuruBilgisi() // ComboBox'a toplam ve indirim özetini yazar
        {
            comboBox1.Items.Clear(); // Mevcut özetleri temizle

            if (selectedSeats.Count == 0) // Koltuk seçilmemişse
            {
                comboBox1.Items.Add("Koltuk seçiniz"); // Bilgi yaz
                comboBox1.SelectedIndex = 0; // Seçili yap
                return; // Çık
            }

            List<string> koltukKodlari = selectedSeats.Select(b => b.Text).ToList(); // Seçili koltuk kodlarını al

            decimal toplamFiyat, toplamIndirim, odenecekTutar; // Hesap değişkenleri
            HesaplaToplamTutar(koltukKodlari, out toplamFiyat, out toplamIndirim, out odenecekTutar); // Hesapla

            comboBox1.Items.Add("Toplam: " + odenecekTutar + " TL (İndirim: " + toplamIndirim + " TL)"); // Özet yaz
            comboBox1.SelectedIndex = 0; // Seçili yap
        }

        private void btnSave_Click(object sender, EventArgs e) // “Satın Al” butonu tıklandığında çalışır
        {
            if (selectedSeats.Count == 0) // Hiç koltuk seçilmediyse
            {
                MessageBox.Show("Lütfen koltuk seçin."); // Uyarı
                return; // Çık
            }

            List<string> koltukKodlari = selectedSeats.Select(b => b.Text).ToList(); // Koltuk kodları listesi

            if (selectedSeats.Count == 1) // Tek koltuk seçiliyse
            {
                if (string.IsNullOrWhiteSpace(txtAdSoyad.Text)) // Ad soyad boşsa
                {
                    MessageBox.Show("Lütfen Ad Soyad girin."); // Uyarı
                    return; // Çık
                }

                string seat = selectedSeats[0].Text; // Tek koltuk kodu
                _koltukIsimleri[seat] = txtAdSoyad.Text.Trim(); // Koltuk -> isim eşlemesini kaydet
            }
            else // Birden fazla koltuk seçiliyse
            {
                bool eksikVarMi = koltukKodlari.Any(k => // Her koltuk için isim var mı kontrol et
                    !_koltukIsimleri.ContainsKey(k) || string.IsNullOrWhiteSpace(_koltukIsimleri[k]));

                if (eksikVarMi) // Eksik isim varsa
                {
                    bool result = MultiNameInputForm.CollectNames(koltukKodlari, _koltukIsimleri); // Çoklu isim formunu aç
                    if (!result) // Vazgeçildiyse
                    {
                        MessageBox.Show("İsimler girilmeden satın alma tamamlanamaz."); // Uyarı
                        return; // Çık
                    }
                }
            }

            foreach (string k in koltukKodlari) // Son kontrol: her koltuk için isim var mı
            {
                if (!_koltukIsimleri.ContainsKey(k) || string.IsNullOrWhiteSpace(_koltukIsimleri[k]))
                {
                    MessageBox.Show("Her koltuk için Ad Soyad girilmelidir."); // Uyarı
                    return; // Çık
                }
            }

            decimal toplamFiyat, toplamIndirim, odenecekTutar; // Hesap değişkenleri
            HesaplaToplamTutar(koltukKodlari, out toplamFiyat, out toplamIndirim, out odenecekTutar); // Ödenecek hesapla

            List<int> yeniBiletIdleri = new List<int>(); // DB’de eklenen biletlerin ID listesi
            List<string> kisiAdlariSiraIle = koltukKodlari.Select(k => _koltukIsimleri[k].Trim()).ToList(); // Koltuk sırasına göre isim listesi

            try // DB işlemleri hata verebilir
            {
                using (MySqlConnection conn = new MySqlConnection(cs)) // Bağlantı oluştur
                {
                    conn.Open(); // Bağlantıyı aç

                    using (MySqlTransaction tx = conn.BeginTransaction()) // Transaction: ya hepsi olur ya hiçbiri
                    {
                        foreach (string koltukNo in koltukKodlari) // Her koltuk için bilet oluştur
                        {
                            string kisiAdi = _koltukIsimleri[koltukNo].Trim(); // Bu koltuk için bilet sahibi adı

                            int dolu = 0; // Koltuk dolu mu?
                            using (MySqlCommand kontrol = new MySqlCommand( // DB'de koltuk doluluğunu kontrol et
                                "SELECT COUNT(*) FROM bilet " +
                                "WHERE etkinlik_id=@e AND koltuk_no=@k " +
                                "AND durum IN ('Aktif','Rezerve')",
                                conn, tx))
                            {
                                kontrol.Parameters.AddWithValue("@e", _aktifEtkinlikId); // Etkinlik ID
                                kontrol.Parameters.AddWithValue("@k", koltukNo); // Koltuk no
                                dolu = Convert.ToInt32(kontrol.ExecuteScalar()); // Sonuç
                            }

                            if (dolu > 0) // Eğer koltuk artık doluysa
                            {
                                tx.Rollback(); // Tüm işlemleri geri al
                                MessageBox.Show("Seçtiğiniz koltuklardan biri artık dolu. Yenileyip tekrar deneyin."); // Uyar
                                KoltukRenkleriniDbyeGoreAyarla(); // Ekranı güncelle
                                return; // Çık
                            }

                            long lastId = 0; // Eklenen biletin id'si

                            try
                            {
                                using (MySqlCommand cmd = new MySqlCommand( // Bilet ekle
                                    "INSERT INTO bilet (kullanici_id, etkinlik_id, koltuk_no, satin_alma_tarihi, durum, isim) " +
                                    "VALUES (@k, @e, @koltuk, NOW(), 'Rezerve', @isim)",
                                    conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@k", _aktifKullaniciId); // Kullanıcı
                                    cmd.Parameters.AddWithValue("@e", _aktifEtkinlikId); // Etkinlik
                                    cmd.Parameters.AddWithValue("@koltuk", koltukNo); // Koltuk no
                                    cmd.Parameters.AddWithValue("@isim", kisiAdi); // İsim

                                    cmd.ExecuteNonQuery(); // INSERT çalıştır
                                    lastId = cmd.LastInsertedId; // Eklenen satırın ID'si
                                }
                            }
                            catch (MySqlException ex) when (ex.Number == 1062) // Unique hatası (aynı koltuk tekrar) yakalanırsa
                            {
                                using (MySqlCommand upd = new MySqlCommand( // Daha önce iptal edilmiş kaydı yeniden aktif etmek gibi update
                                    "UPDATE bilet SET kullanici_id=@k, satin_alma_tarihi=NOW(), durum='Rezerve', isim=@isim, iptal_tarihi=NULL " +
                                    "WHERE etkinlik_id=@e AND koltuk_no=@koltuk AND aktif_mi=0 " +
                                    "LIMIT 1",
                                    conn, tx))
                                {
                                    upd.Parameters.AddWithValue("@k", _aktifKullaniciId); // Kullanıcı
                                    upd.Parameters.AddWithValue("@e", _aktifEtkinlikId); // Etkinlik
                                    upd.Parameters.AddWithValue("@koltuk", koltukNo); // Koltuk
                                    upd.Parameters.AddWithValue("@isim", kisiAdi); // İsim

                                    int aff = upd.ExecuteNonQuery(); // Update çalıştır
                                    if (aff <= 0) throw; // Güncelleme olmadıysa hatayı tekrar fırlat
                                }

                                using (MySqlCommand getId = new MySqlCommand( // Güncellenen kaydın id'sini çek
                                    "SELECT bilet_id FROM bilet WHERE etkinlik_id=@e AND koltuk_no=@koltuk AND aktif_mi=0 LIMIT 1",
                                    conn, tx))
                                {
                                    getId.Parameters.AddWithValue("@e", _aktifEtkinlikId); // Etkinlik
                                    getId.Parameters.AddWithValue("@koltuk", koltukNo); // Koltuk
                                    object obj = getId.ExecuteScalar(); // ID'yi al
                                    if (obj == null || obj == DBNull.Value) throw; // Bulunamadıysa hata
                                    lastId = Convert.ToInt64(obj); // ID'yi long'a çevir
                                }
                            }

                            yeniBiletIdleri.Add((int)lastId); // Eklenen/güncellenen bilet ID listesini doldur
                        }

                        tx.Commit(); // Tüm biletler rezerve edildi, işlemi onayla
                    }

                    // Ödeme ekranı (senin FrmOdeme.cs ile uyumlu) // Ödeme formunu aç
                    using (FrmOdeme odeme = new FrmOdeme(cs, _aktifKullaniciId, yeniBiletIdleri, kisiAdlariSiraIle, odenecekTutar))
                    {
                        odeme.ShowDialog(); // Modal aç: ödeme bitmeden geri dönmez
                    }

                    // Ödeme gerçekten yazıldı mı? // odeme tablosunda kayıt var mı kontrolü
                    bool odemeBasarili = OdemeTamMi(conn, yeniBiletIdleri);

                    if (!odemeBasarili) // Ödeme tamamlanmadıysa
                    {
                        RezerveBiletleriSil(conn, yeniBiletIdleri); // Rezerve biletleri temizle
                        MessageBox.Show("Ödeme alınamadığı için bilet işlemi iptal edildi."); // Uyar

                        SatisSeciminiTemizle(); // Seçimleri sıfırla
                        KoltukRenkleriniDbyeGoreAyarla(); // Koltukları DB’ye göre boyat
                        return; // Çık
                    }

                    // Ödeme varsa aktif yap // Ödeme tamamlandıysa biletleri “Aktif” yap
                    BiletleriAktifYap(conn, yeniBiletIdleri);

                    foreach (Button seatBtn in selectedSeats) // Seçilen koltukları ekranda kırmızı yap
                        seatBtn.BackColor = Color.Red;

                    selectedSeats.Clear(); // Seçim listesini temizle
                    _koltukIsimleri.Clear(); // İsim eşlemelerini temizle
                    txtKoltukNo.Text = ""; // Koltuk textbox temizle
                    txtAdSoyad.Text = ""; // Ad soyad temizle
                    txtAdSoyad.Enabled = true; // Tekli isim alanını tekrar aç

                    GuncelleBiletTuruBilgisi(); // Özet bilgiyi güncelle
                }
            }
            catch (Exception ex) // Genel hata
            {
                MessageBox.Show("Satın alma hatası:\n" + ex.Message); // Hata mesajı
            }
        }

        private bool OdemeTamMi(MySqlConnection conn, List<int> biletIdleri) // Ödeme tablosunda her bilet için ödeme var mı kontrol eder
        {
            if (biletIdleri == null || biletIdleri.Count == 0) return false; // Liste boşsa ödeme tamam değildir

            List<string> inParams = new List<string>(); // IN(...) için parametre adları

            using (MySqlCommand cmd = new MySqlCommand()) // Tek komut nesnesi
            {
                cmd.Connection = conn; // Aynı bağlantıyı kullan

                for (int i = 0; i < biletIdleri.Count; i++) // Her bilet id için parametre üret
                {
                    string p = "@id" + i; // @id0, @id1...
                    inParams.Add(p); // Parametre adını listeye ekle
                    cmd.Parameters.AddWithValue(p, biletIdleri[i]); // Parametre değerini ekle
                }

                cmd.CommandText =
                    "SELECT COUNT(*) FROM odeme WHERE bilet_id IN (" + string.Join(",", inParams) + ") AND durum='Tamamlandı'"; // Ödeme sayısını say

                int odemeSayisi = Convert.ToInt32(cmd.ExecuteScalar()); // Kaç ödeme kaydı var
                return odemeSayisi == biletIdleri.Count; // Sayı bilet sayısına eşitse ödeme tamdır
            }
        }

        private void RezerveBiletleriSil(MySqlConnection conn, List<int> biletIdleri) // Ödeme alınamazsa rezerve edilen biletleri siler
        {
            if (biletIdleri == null || biletIdleri.Count == 0) return; // Liste boşsa çık

            using (MySqlTransaction tx = conn.BeginTransaction()) // Silme işlemi transaction içinde
            {
                List<string> inParams = new List<string>(); // IN parametreleri

                using (MySqlCommand cmd = new MySqlCommand()) // Komut
                {
                    cmd.Connection = conn; // Bağlantı
                    cmd.Transaction = tx; // Transaction

                    for (int i = 0; i < biletIdleri.Count; i++) // Parametreleri üret
                    {
                        string p = "@id" + i; // @id0...
                        inParams.Add(p); // listeye ekle
                        cmd.Parameters.AddWithValue(p, biletIdleri[i]); // değer ekle
                    }

                    cmd.CommandText =
                        "DELETE FROM bilet WHERE bilet_id IN (" + string.Join(",", inParams) + ") AND durum='Rezerve'"; // Sadece Rezerve olanları sil

                    cmd.ExecuteNonQuery(); // Sil
                }

                tx.Commit(); // Onayla
            }
        }

        private void BiletleriAktifYap(MySqlConnection conn, List<int> biletIdleri) // Ödeme başarılıysa biletleri Aktif yapar
        {
            if (biletIdleri == null || biletIdleri.Count == 0) return; // Liste boşsa çık

            using (MySqlTransaction tx = conn.BeginTransaction()) // Update işlemi transaction içinde
            {
                List<string> inParams = new List<string>(); // IN parametreleri

                using (MySqlCommand cmd = new MySqlCommand()) // Komut
                {
                    cmd.Connection = conn; // Bağlantı
                    cmd.Transaction = tx; // Transaction

                    for (int i = 0; i < biletIdleri.Count; i++) // Parametreleri üret
                    {
                        string p = "@id" + i; // @id0...
                        inParams.Add(p); // listeye ekle
                        cmd.Parameters.AddWithValue(p, biletIdleri[i]); // değer ekle
                    }

                    cmd.CommandText =
                        "UPDATE bilet SET durum='Aktif' WHERE bilet_id IN (" + string.Join(",", inParams) + ")"; // Durumu Aktif yap
                    cmd.ExecuteNonQuery(); // Update çalıştır
                }

                tx.Commit(); // Onayla
            }
        }

        private void btnIptal_Click(object sender, EventArgs e) // İptal butonuna basılınca çalışan event
        {
            if (!iptalModu) // İptal modu açık değilse
            {
                MessageBox.Show("Önce İptal Modu'nu açın."); // Uyar
                return; // Çık
            }

            if (iptalSecilenKoltuk == null) // İptal edilecek koltuk seçilmediyse
            {
                MessageBox.Show("İptal edilecek koltuğu seçin."); // Uyar
                return; // Çık
            }

            string koltukNo = iptalSecilenKoltuk.Text; // Seçilen koltuk no

            try
            {
                using (MySqlConnection conn = new MySqlConnection(cs)) // DB bağlantısı
                {
                    conn.Open(); // Aç

                    using (MySqlTransaction tx = conn.BeginTransaction()) // Transaction
                    {
                        int affected = 0; // Etkilenen satır sayısı

                        using (MySqlCommand iptal = new MySqlCommand( // Aktif bileti iptal et
                            "UPDATE bilet SET durum='Iptal', iptal_tarihi=NOW() " +
                            "WHERE etkinlik_id=@e AND koltuk_no=@k AND durum='Aktif' LIMIT 1",
                            conn, tx))
                        {
                            iptal.Parameters.AddWithValue("@e", _aktifEtkinlikId); // Etkinlik
                            iptal.Parameters.AddWithValue("@k", koltukNo); // Koltuk
                            affected = iptal.ExecuteNonQuery(); // Update sonucu
                        }

                        if (affected == 0) // Hiç kayıt güncellenmediyse
                        {
                            tx.Rollback(); // Geri al
                            MessageBox.Show("Aktif bilet bulunamadı."); // Uyar
                            return; // Çık
                        }

                        tx.Commit(); // Onayla
                    }
                }

                iptalSecilenKoltuk.BackColor = Color.WhiteSmoke; // Koltuk artık boş gibi gösterilir
                iptalSecilenKoltuk = null; // Seçimi temizle
                txtKoltukNo.Text = ""; // TextBox temizle
                MessageBox.Show("Bilet iptal edildi."); // Bilgilendir

                KoltukRenkleriniDbyeGoreAyarla(); // DB’ye göre renkleri yeniden ayarla
            }
            catch (Exception ex) // Hata yakala
            {
                MessageBox.Show("İptal sırasında hata:\n" + ex.Message); // Hata mesajı
            }
        }

        private void SatisSeciminiTemizle() // Satın alma için seçilen koltukları temizler
        {
            foreach (Button seat in selectedSeats) // Seçili koltukları dolaş
            {
                if (seat.BackColor == Color.LightGreen) // Yeşil (seçili) ise
                    seat.BackColor = Color.WhiteSmoke; // Boş renge döndür
            }

            selectedSeats.Clear(); // Listeyi temizle
            _koltukIsimleri.Clear(); // İsimleri temizle

            txtKoltukNo.Text = ""; // Koltuk no temizle
            txtAdSoyad.Text = ""; // Ad soyad temizle
            txtAdSoyad.Enabled = true; // Ad soyad alanını aktif et

            GuncelleBiletTuruBilgisi(); // Özet bilgiyi güncelle
        }

        private void IptalSeciminiTemizle() // İptal seçimini temizler
        {
            if (iptalSecilenKoltuk != null && iptalSecilenKoltuk.BackColor == Color.Gold) // İptal seçimi altın rengi ise
                iptalSecilenKoltuk.BackColor = Color.Red; // Tekrar kırmızıya çevir (satılmış görünümü)

            iptalSecilenKoltuk = null; // Seçimi sıfırla
            txtKoltukNo.Text = ""; // TextBox temizle
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) // ComboBox event'i
        {
            // Designer event'i var; bilerek boş bırakıldı // Kullanıcı değiştiremediği için burada iş yok
        }

        // ============================
        // Çoklu koltuk için isim toplama formu // Birden fazla koltukta her koltuk için ad soyad alınır
        // ============================
        private class MultiNameInputForm : Form // İç sınıf: koltuk başına isim girme formu
        {
            private readonly List<string> _seats; // İsim girilecek koltuk listesi
            private readonly Dictionary<string, string> _map; // Koltuk->isim eşlemesi
            private int _index = 0; // Şu an hangi koltuktayız?

            private Label _lblSeat; // Koltuk bilgisini gösteren label
            private TextBox _txtName; // Ad soyad girilen textbox
            private Button _btnNext; // İleri/Bitir butonu
            private Button _btnCancel; // Vazgeç butonu

            private MultiNameInputForm(List<string> seats, Dictionary<string, string> existingMap) // Form constructor
            {
                _seats = seats; // Koltuk listesini al
                _map = existingMap; // Mevcut eşlemeyi al

                Text = "Bilet Sahipleri"; // Form başlığı
                StartPosition = FormStartPosition.CenterParent; // Ortada aç
                FormBorderStyle = FormBorderStyle.FixedDialog; // Sabit dialog
                MaximizeBox = false; // Büyütme yok
                MinimizeBox = false; // Küçültme yok
                Width = 420; // Genişlik
                Height = 200; // Yükseklik

                _lblSeat = new Label { Left = 20, Top = 20, Width = 360, Text = "" }; // Koltuk bilgi label'ı
                Controls.Add(_lblSeat); // Forma ekle

                Label lbl = new Label { Left = 20, Top = 55, Width = 360, Text = "Bu koltuğun bilet sahibi (Ad Soyad):" }; // Açıklama label'ı
                Controls.Add(lbl); // Forma ekle

                _txtName = new TextBox { Left = 20, Top = 80, Width = 360 }; // İsim textbox'ı
                Controls.Add(_txtName); // Forma ekle

                _btnCancel = new Button { Left = 20, Top = 120, Width = 120, Text = "Vazgeç" }; // Vazgeç butonu
                _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); }; // Vazgeç: Cancel dön ve kapat
                Controls.Add(_btnCancel); // Forma ekle

                _btnNext = new Button { Left = 260, Top = 120, Width = 120, Text = "İleri" }; // İleri butonu
                _btnNext.Click += BtnNext_Click; // İleri butonu event'i
                Controls.Add(_btnNext); // Forma ekle

                LoadCurrent(); // İlk koltuğu yükle
            }

            private void LoadCurrent() // Şu anki koltuğu ekrana yükler
            {
                string seat = _seats[_index]; // Şu anki koltuk
                _lblSeat.Text = "Koltuk: " + seat + " (" + (_index + 1) + "/" + _seats.Count + ")"; // Bilgi: Koltuk + sıra

                string existing; // Daha önce girilmiş isim var mı?
                if (_map.TryGetValue(seat, out existing)) // Varsa
                    _txtName.Text = existing ?? ""; // Textbox'a yaz
                else
                    _txtName.Text = ""; // Yoksa boş bırak

                _btnNext.Text = (_index == _seats.Count - 1) ? "Bitir" : "İleri"; // Son koltukta Bitir yazar
                _txtName.Focus(); // İmleci textbox'a ver
                _txtName.SelectAll(); // İçeriği seç
            }

            private void BtnNext_Click(object sender, EventArgs e) // İleri/Bitir butonu
            {
                string seat = _seats[_index]; // Şu anki koltuk
                string name = (_txtName.Text ?? "").Trim(); // Girilen isim

                if (string.IsNullOrWhiteSpace(name)) // İsim boşsa
                {
                    MessageBox.Show("Bu koltuk için Ad Soyad zorunludur."); // Uyar
                    return; // Çık
                }

                _map[seat] = name; // Koltuk->isim kaydet

                if (_index < _seats.Count - 1) // Daha koltuklar bitmediyse
                {
                    _index++; // Sonraki koltuğa geç
                    LoadCurrent(); // Ekranı güncelle
                    return; // Çık
                }

                DialogResult = DialogResult.OK; // Tüm isimler girildi => OK
                Close(); // Formu kapat
            }

            public static bool CollectNames(List<string> seats, Dictionary<string, string> map) // Dışarıdan çağrılan isim toplama fonksiyonu
            {
                List<string> ordered = seats.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(); // Koltukları alfabetik sırala

                using (MultiNameInputForm f = new MultiNameInputForm(ordered, map)) // Formu aç
                {
                    return f.ShowDialog() == DialogResult.OK; // OK ile kapandıysa true, vazgeçildiyse false
                }
            }
        }
    }
}
