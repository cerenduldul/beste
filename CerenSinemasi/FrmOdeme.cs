using System;
using System.Collections.Generic; //List, dictionary gibi koleksiyon yapılarını kullanmak için gerekli kütüphanedir.
using System.Linq;//Listeler üzerinde Select, Where gibi sorgulama işlemlerini yapmayı sağlar.
using System.Windows.Forms;
using MySqlConnector;
using beste.Models;

namespace beste //Formun beste adlı isim alanına ait olduğunu belirtir. İsim alanı=?
{
    public partial class FrmOdeme : Form //Ödeme ekranını temsil eden, Windows Form'da türeyen ve kodu tasatımla bölğnmüş bir sınıftır.
    {
        private readonly string cs; //Veritabanı bağlantı bilgisini tutan ve sonradan değiştirelemeyen bir değişkendir.
        private readonly int kullaniciId;//Ödeme yapan aktif kullanıcının ID bilgisini saklayan sabitlenmiş değişkendir.
        private readonly List<int> biletIdleri;//Ödeme yapılacak biletlerin veritabanı ID'lerini tutan  listedir.
        private readonly decimal toplamTutar;//Ödenecek toplam tutarı (para değeri olarak) saklayan değişkendir.

        private string bin4 = null; //Kart numarasının ilk 4 hanesini (BIN) tutmak için kullanılan değişkendir.
        private string bankaAdi = null; //Karttan tespit edilen banka adını saklamak için kullanılır.
        private string kartSchemesi = null;//Kartın Visa/ MasterCard gibi tür bilgisini saklama için kullanılır.
        private bool kartAktif = false;//Kart bilgilerinin geçerli olup olmadığını gösteren mantıksal değişkendir.

        public FrmOdeme(string cs, int kullaniciId, List<int> biletIdleri, decimal toplamTutar) //Ödeme formu açılırken veritabanı bağlantısı, kullanıcı ID'si, bilet ID'leri ve toplam tutar dışarıdan alan kurucu metottur.
        {
            InitializeComponent(); //Form tasarımında bulunan tüm kontrolleri ekrana yükler. (textbox,button vb.)

            this.cs = cs; //Parametre olarak gelen connection string'i sınıf içindeki değişkene atar.
            this.kullaniciId = kullaniciId;//Ödeme yapan aktif kullanıcının ID bilgisini sınıf içindee saklar.
            this.biletIdleri = biletIdleri ?? new List<int>();//Bilet ID listesi boş gelirse hata oluşmaması için yeni boş liste oluşturur.
            this.toplamTutar = toplamTutar;//Ödenecek toplam tutarı sınıf değişkenine atar.

         //Kredi kartı ile ödeme satırları.
            this.Text = "Kredi Kartı ile Ödeme"; //Formun üst başlığında kredi kartı ile ödeme yazmasını sağlayan komut satırıdır.

            //Kart bilgileri için maske tanımladığını belirtir. Kartın bilgilerinin gizlenmesi satırı nasıl yapılır?
            maskedTextBox1.Mask = "0000 0000 0000 0000"; // Kart No
            maskedTextBox2.Mask = "000";                 // CVC

            // Başlangıç metinleri
            label2.Text = "Banka: -"; //Banka bilgisi henüz yokken varsayılan metni gösterir.

      
            this.Load += FrmOdeme_Load;//Form açıldığında çalışacak Load event'ini bağlar.
            maskedTextBox1.TextChanged += maskedTextBox1_TextChanged;//Kart numarası değiştikçe kart bilgisi kontrolü yapılmasını sağlar.
            comboBox2.SelectedIndexChanged += (s, e) => AyDoldur();//Ay seçimi değiştiğinde ilgili ay bilgileri dolduran metotu çağırır.
            button1.Click += button1_Click;        // Öde butonuna basıldığında ödeme işlemini başlatan metodu çağırır.
            button2.Click += (s, e) => this.Close(); // İptal butonuna basıldığında ödeme formunu kapatır.

            //Bu kodlar, ödeme formunu gerekli verilerle başl  atır, kart girişlerini sınırlar ve kullanıcı etkileşimlerini (ödeme/iptal) hazırlar.
        }

        private void FrmOdeme_Load(object sender, EventArgs e) //Ödeme formu ilk kez ekrana yüklendiğinde otomatik çalışan olay metodudur.
        {
            //Bu kodlar yıl listesini doldurmayla ilgilidir.
            comboBox2.Items.Clear(); //Yıl seçimi yapılan ComboBox'ın önceki tüm elemanlarını temizler.
            int basYil = DateTime.Now.Year;//Geçerli sistem tarihine göre mevcut yılı alır.
            for (int i = 0; i <= 12; i++)//Mevcut yıldan bailayarak 12 yıl ileriye kadar dönen döngüdür.Yılı değiştirmeye çalış!!
                comboBox2.Items.Add((basYil + i).ToString());//ComboBox'a yıl değerlerini (2025,2026) ekler.

            comboBox2.SelectedIndex = 0; //İlk yılı varsayılan olarak seçili yapar. Varsayılandan kasıt 2025 yani bulunduğumuz yıl mı *

            
            AyDoldur(); //Seçilen yıla göre uygun ayları ComboBox'a dolduran metodu çağırır. 2025'te geçmiş ayrıları eklemeyen komut bu mu?

            // Kart doğrulanmadan ödeme kapalı, güvenlik amacıyla yazılan bir kod satırıdır.
            button1.Enabled = false; //Kart bilgilerini doğrulamadan Öde butonunu pasif hale getirir.
        }

        private void AyDoldur() //Seçilen yıla göre geçerli ay listesini oluşturan yardımcı metottur.
        {
            comboBox1.Items.Clear(); //Ay seçimi yapılan ComboBox'ın önceki tüm aylarını temizler.

            int secilenYil = int.Parse(comboBox2.SelectedItem.ToString()); //Yıl ComboBox'ında seçilen değeri sayısal yıl bilgisine çevirir.
            int baslangicAy = (secilenYil == DateTime.Now.Year) ? DateTime.Now.Month : 1; //Eğer seçilen yıl bu yıl ise başlangıç aynı mevcut aydan, değilse Ocak'tan başlatır.

            for (int ay = baslangicAy; ay <= 12; ay++)//Başlangıç ayından Aralık ayına kafar ayları sırayla döner.
                comboBox1.Items.Add(ay.ToString("00"));//Ayları iki haneli formatta (01,02 vb.) ComboBox'a ekler. Soru: tek hanelileri tek, çift hanelileri ift yazdıran kod satırı nasıl olur?

            comboBox1.SelectedIndex = 0; //İlk ayı varsayılan olarak seçili yapar. Yani o yıl bulunduğumuz ay mı?
            //Bu kodlar, kredi kartı son kullanma tarihi için geçerli yıl ve ay seçeneklerini otomatik olarak oluşturur ve güvenlik için ödeme butonunu başlangıçta kapalı tutar
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e) //Kredi kartı numarası alanındaki her değişiklite otomatik çalışan olay metodudur.
        {
            string digits = new string(maskedTextBox1.Text.Where(char.IsDigit).ToArray()); //Kart numarasından sadece rakamları ayıklayarak yeni bir metin oluşturur. Not: Harf girip dene!!

            if (digits.Length < 4)//Henüz ilk 4 hane girilip girilmediğini kontrol eder.
            {
                label2.Text = "Banka: -"; //Bnaka bilgisini varsayılan (bilinmiyor) duruma getirir.
                button1.Enabled = false; //Ödeme butonunu pasif hale getirir. Not: Banka bilgileri girilene kadar pasif bıraktıra*n kod satırı bu mu?
                kartAktif = false; //Kartın geçerli kabul edilmediğini işaretler. Uygulama kartın geçersiz olduğunu nasıl anlıyor?
                bankaAdi = null;//Banka adını temizler. Ne zaman temizler, uygulama ne zaman temizleyeceğini nasıl anlar?
                kartSchemesi = null;//Kart türü bilgisini temizler.
                bin4 = null;//Kartın ilk 4 hanesini temizler.
                return; //Yeterli bilgi olmadığı için metottan çıkar. Bu satır gerekli bilgiler yazılamdığında uyarı vermesi sonucu işlemi başa alamasını sağlayan komut satırıdır.
            }

            bin4 = digits.Substring(0, 4); //Kart numarasının ilk 4 hanesini alır.
            BinSorgula(bin4);//Bu 4 hanelik Bın bilgisini kullanarak banka ve kart türü sorgulamasını başlatır.
            //Bu kodlar, kart numarası yazıldıkça ilk 4 haneyi kontrol eder, banka bilgisi doğrulanmadan ödeme yapılmasını engeller ve yeterli bilgi girildiğinde banka sorgulamasını başlatır.
        }

        private void BinSorgula(string b) //Kart numarasının ilk hanesine göre banka ve kart bilgilerini sorgulayan yardımcı metottur.
            //Kart numarası ve bilgilerini uygulama nereden alır.
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
                        if (rd.Read()) //Sorgudan en az bir kayıt gelip gelmediğini kontrol eder.
                        {
                            bankaAdi = rd["banka_adi"].ToString(); //Veritabanından banka adını alır.
                            kartSchemesi = rd["kart_schemesi"].ToString(); //Kartın Visa/MasterCard gibi tür bilgisini alır.
                            kartAktif = Convert.ToBoolean(rd["aktif"]); //Kartın aktif mi pasif mi olduğunu belirler.

                            if (kartAktif) //Kartın aktif olup olmadığını kontrol eden sorgu başlangıcıdır ?
                            {
                                label2.Text = $"{bankaAdi} ({kartSchemesi})"; //Ekranda banka adı ve kart türünü gösterir.
                                button1.Enabled = true; //Kart geçerliyse öde butonunu aktif eder.
                            }
                            else //Kart pasifse çalışacak alternatif durumu belirtir.
                            {
                                label2.Text = $"{bankaAdi} ({kartSchemesi}) - PASİF"; //Kartın pasif olduğunu kullanıcıya açıkça gösterir. 
                                button1.Enabled = false; //Pasif kartta ödeme yapılmasını engeller.
                            }
                        }
                        else //BIN bilgisi veritabanında hiç bulunmazsa çalışır.
                        {
                            label2.Text = "Banka: Tanımsız"; //Kartın bankasının bilmediğini kullanıcıya bildirir.
                            kartAktif = false; //Kart geçersiz olarak işaretler.
                            button1.Enabled = false; //Ödeme yapılmasını engeller.

                            //Bu metot, kart numarasının ilk 4 hanesini kullanarak bankayı ve kart türünü veritabanından doğrular, kart aktifse ödemeye izin verir, değilse engeller.
                        }
                    }
                }
            }
            catch (Exception ex) //BIN sorgulama sırasında oluşan herhangi bir hatayı yakalar.
            {
                label2.Text = "BIN hata: " + ex.Message; //Oluşan hatayı kullanıcıya etiket üzerinde açıklama olarak gösterir.
                kartAktif = false; //Hata durumunda kartı geçersiz olarak değiştirir.
                button1.Enabled = false; //Hata varsa öde butonunu pasif hale getirir.
            }
        }
        //Öde butonuna basıldığında çalışan olay metodudur.
        private void button1_Click(object sender, EventArgs e) 
        {
            // Kart numarasına doğrulama yapıldığı komut satırıdır.
            string kartDigits = new string(maskedTextBox1.Text.Where(char.IsDigit).ToArray()); //Kart numarasından sadece rakamları ayıklar.
            if (kartDigits.Length != 16) //Kart numarasının 16 haneli olup olmadığını kontrol eder.
            {
                MessageBox.Show("Kart numarası 16 hane olmalı."); 
                return;
            }

            //Kart sahibi bilgisi kontrolünün yapıldığı komut satırıdır. Ad soyad kontrolüdür.
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Kart sahibinin ad soyadı boş olamaz."); 
                return; //Ad soyad eksik olduğu için ödeme işlemini durdurur.


                //Bu kodlar, ödeme yapılmadan önce kart numarasının 16 haneli olduğunu ve kart sahibinin ad-soyad bilgisinin girildiğini doğrulayarak hatalı ödemeleri engeller.
            }

            // CVC kontrol
            string cvcDigits = new string(maskedTextBox2.Text.Where(char.IsDigit).ToArray()); //CVC alanındaki sadece rakamları ayıklayarak yeni bir metin oluşturur.
            if (cvcDigits.Length < 3) //CVC'nin en az 3 haneli olup olmadığını kontrol eder.
            {
                MessageBox.Show("CVC en az 3 hane olmalı."); 
                return; //Geçersiz CVC nedeniyle ödeme işlemine devam edilmesini engeller.
            }

            // Kartın son kullanma tarihini kontrol ettiğini belirtir.
            int ay = int.Parse(comboBox1.SelectedItem.ToString()); //Seçilen son kullanma ayını sayısal değere çevirir.
            int yil = int.Parse(comboBox2.SelectedItem.ToString());//Seçilen son kullanma yılını sayısal değere çevirir.

            DateTime sonGun = new DateTime(yil, ay, DateTime.DaysInMonth(yil, ay));//Kartın son kullanma tarihini,seçilen ayın son günü olacak şekilde hesaplar. 30 vs 31 
            if (sonGun < DateTime.Today) //Kartın süresi dolmuş mu diye kontrol eder.
            {
                MessageBox.Show("Son kullanım tarihi geçmiş."); 
                return; //Süresi geçmiş kartla ödeme yapılmasını engeller.
            }

            if (!kartAktif) //Kartın sistemde aktif olup olmadığını kontrol eder.
            {
                MessageBox.Show("Kart pasif, ödeme alınamaz."); 
                return; //Kart pasif olduğu için işlemi durdurur.
            }

            if (biletIdleri == null || biletIdleri.Count == 0) //Ödenecek herhangi bir bilet olup olmadığını kontrol eder.
            {
                MessageBox.Show("Ödenecek bilet bulunamadı.");
                return; //Ödeme işlemini tamamen durdurur.
            }

            // Kartı DB'ye kaydetmiyoruz, maske kaydediyoruz. Güvenlik amacıyla yazıldığıı açıklar.
            string mask = kartDigits.Substring(0, 4) + " **** **** " + kartDigits.Substring(12, 4); //Kart numarasını ilk ve son 4 hane açık, ortası gizli olacak şekilde maskeler.

            // Tutarı bilet başına böl (kuruş kaymaması için)
            decimal parca = Math.Floor((toplamTutar / biletIdleri.Count) * 100m) / 100m; //Toplam tutarı bilet sayısına bölerek kuruş hassasiyetiyle parça tutarı hesaplar.
            decimal sonParca = toplamTutar - parca * (biletIdleri.Count - 1); //Yuvarlama farkını telafi etmek için son bilete düşecek kalan tutarı hesaplar.

            //

            try
            {
                using (var conn = new MySqlConnection(cs)) //MySQL veritabanına bağlantı oluşturur ve işlem bitince otomatik kapatır. Uygulama otomatik kapanacağını nereden biliyor?
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction()) //Birden fazla ödeme kaydını tek ve tutarlı bir işlem olarak yürütmek için başlatır.
                    {
                        for (int i = 0; i < biletIdleri.Count; i++) //Her bir bilet için ayrı ödeme kaydı oluışturmak üzere döngü başlatır.
                        {
                            int biletId = biletIdleri[i]; //Döngüdeki mevcut biletin veritabanı ID'sini alır.
                            decimal tutar = (i == biletIdleri.Count - 1) ? sonParca : parca; //Son bilete yuvarlama farkını ekleyerek, bilet başına düşen ödeme tutarını belirler. Soru= Yuvarlama farkı ne demek?

                           
                            var cmd = new MySqlCommand(@" INSERT INTO odeme (bilet_id, odeme_tutari, odeme_yontemi, durum, odeme_tarihi, kullanici_id,
                                                                             bin4, banka_adi, kart_schemesi, kart_mask, son_kullanim_ay, son_kullanim_yil,
                                                                             taksitli, taksit_sayisi) 

                                                      VALUES (@bilet, @tutar, 'Kredi kartı', 'Tamamlandı', NOW(), @kid,
                                                      @bin4, @banka, @schema, @mask, @ay, @yil,
                                                      0, NULL);", conn, tx);

                            cmd.Parameters.AddWithValue("@bilet", biletId); //Ödeme kayıdını ilgili bilete bağlar.
                            cmd.Parameters.AddWithValue("@tutar", tutar); //Bilet için hesaplanan ödeme tutarını kayda ekler.
                            cmd.Parameters.AddWithValue("@kid", kullaniciId); //Ödemeyi yapan kullanıcının ID'sini ekler.

                            cmd.Parameters.AddWithValue("@bin4", bin4); //Kartın ilk 4 hanesini (BIN) ödeme kaydına ekler.
                            cmd.Parameters.AddWithValue("@banka", (object)bankaAdi ?? DBNull.Value); //Banka adı varsa ekler yoksa NULL olarak kaydeder.
                            cmd.Parameters.AddWithValue("@schema", (object)kartSchemesi ?? DBNull.Value); //Kart türünü (Visa/MasterCard) varsa ekler yoksa NULL yazar.
                            cmd.Parameters.AddWithValue("@mask", mask); //Maskelenmiş kart numarasını güvenli biçimde kayıt eder.

                            cmd.Parameters.AddWithValue("@ay", ay); //Kartın son kullanma ayını kayıt eder.
                            cmd.Parameters.AddWithValue("@yil", yil);//Kartın son kullanma yılını kayıt eder.

                            cmd.ExecuteNonQuery(); //Hazırlanan ödeme kaydını veritabanına yazar.
                        }

                        tx.Commit(); //Tüm biletlerin ödemesi sorunsuzsa işlemleri kalıcı olarak onaylar. 
                    }
                }
                // PNR / BiletNo üret (12 haneli)
                string pnr = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

                // Şimdilik ilk bileti QR’da gösterelim (birden fazla bilet varsa)
                int ilkBiletId = biletIdleri[0];

                Bilet qrBilet = new Bilet
                {
                    BiletNo = pnr,
                    Koltuk = ilkBiletId.ToString(),      // İstersen DB'den koltuk çekince burayı güncelleriz
                    Salon = "-",                         // İstersen DB'den salon çekince burayı güncelleriz
                    MusteriAdi = textBox1.Text.Trim(),   // Kart sahibi ad-soyadını müşteri adı gibi gösteriyoruz
                    Fiyat = toplamTutar,                 // Toplam
                    OyunAdi = "Etkinlik",                // Şimdilik sabit (DB'den çekince gerçek olur)
                    TiyatroAdi = "Mekan",
                    TiyatroAdresi = "",
                    BaslangicZamani = DateTime.Now.AddHours(1),
                    BitisZamani = DateTime.Now.AddHours(3),
                };

                // QR ekranını aç
                new FrmBiletQR(qrBilet).ShowDialog();


                MessageBox.Show("Ödeme kaydedildi.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ödeme hatası:\n" + ex.Message);
            }
        }

        // --- Designer hata vermesin diye boş event'ler ---
        private void label2_Click(object sender, EventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
