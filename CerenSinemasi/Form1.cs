using System;//C#'ın temel kütüphanesi
using System.Collections.Generic;// Liste,dictionary gibi koleksiyonlar için
using System.Data;// Veri işlemleri için
using System.Drawing;// Grafik ve renk işlemleri için
using System.Linq;// Liste ve koleksiyonlarla ilgili sorgulamalar bu kütüphane üzerinden yapılır.
using System.Windows.Forms;// Windows Forms uygulamasında olduğumuzu ifade eder
using MySqlConnector;//MySQL veritabanı bağlantısı için

namespace beste //Projenin isim alanını temsil eder.
{ // Namespace bloğunun başlangıcını temsil eder.
    public partial class Form1 : Form //Form1 adında Windows Formdan oluşan sınıfı belirtir.
    {
        // Kullanıcının seçtiği koltukları tutan liste.
        private readonly List<Button> selectedSeats = new List<Button>();

        // Koltuk butonlarının ekleneceği paneli temsil eder. Başlangıçta bol olan değişkendir.
        private Panel panelKoltuklar = null;

        // İptal Modu ile ilgili değişkenler burada bulunur.
        private bool iptalModu = false;// İptal modunun aktif olup olmadığını temsil eder.
        private Button iptalSecilenKoltuk = null;//İptal edilmek için seçilen koltuğu temsil eden butonu tutar.
        private CheckBox chkIptal = null;//İptal modunu açıp kapatmak için kullanılan CheckBox bileşenini temsil eder.
        private Button btnIptal = null;//İptal işlemini başlatan butonu temsil eder.

        // MySQL bağlantı
        private readonly string cs =
            "Server=localhost;Database=bilet_sistemi;User Id=root;Password=Ghezzal18.";

        // Bu değişkenler sabittir ve değiştirelemez. Çünkü bu değerlerin sabit kalması güvenilirlik için önemlidir.
        private const int AKTIF_ETKINLIK_ID = 2;//Uygulamada aktif olarak kullanılan etkinlik_Id değerini sabit olarak tutulmasını sağlayan komuttur.
        private const int AKTIF_KULLANICI_ID = 1;//Uygulamada aktif olarak kullanılan kullanici_Id değerini sabit olarak tutulmasını sağlayan komut satırıdır.

        public Form1() // Form1 satırının kurucu metodudur. Form oluşurken otomatik olarak çalışır.
        {
            InitializeComponent(); // Formun tasarım dosyasında oluşturulan tüm kontrolleri ekrana yükler -button, label gibi-
        }

        private void Form1_Load(object sender, EventArgs e)// Form ekrana ilk yüklendiği anda çalışan olay metodudur.
        {
            // Koltukların dinamik olarak oluşturulmasını sağlayan komuttur.
            KoltuklariOlustur();

            // DB'ye göre koltuk renklerinin ayarlandığı komuttur. Gürkan Hocanın sorduğu komut satırı.
            KoltukRenkleriniDbyeGoreAyarla();

            // Fiyat bilgisinin gösterimiyle ilgili komut satırlarıdır. Kullanıcı tarafından yazılamaz sadece seçilebilir olmasını sağlıyor.
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Enabled = false; // ComboBox'ı pasif hale getirerek sadece gerekli bilginin gözükmesini sağlar.
            GuncelleBiletTuruBilgisi();//Bilet türü ve fiyat bilgisinin güncellenmesini sağlar.

            // İptal modu kontrolleri için gerekli komut satırıdır.
            IptalKontrolleriniKur();
        }

        private void KoltuklariOlustur() //Koltuk butonlarının dinamik oluşturulmasını için tanımlanan özel bir metottur.
        {
            int rows = 6;    // A-F 6 satır.
            int cols = 8;   // 1-8 Bir satırda 8 koltuk.
            int size = 40; // Her koltuğun genişlik ve yükseklik değerini belirler.
            int gap = 5;  // Koltuklar arasındaki boşluk miktarını belirler. piksel cinsinden ayarlanır : en küçük ölçü biriminden hesaplanır.

            panelKoltuklar = new Panel//Koltukların ekleneceği yeni bir panel nesnesi oluşturulur.
            { //Panel özelliklerinin ayarlandığı bloğun başlangıcını temsil eder.
                Location = new Point(12, 30), //Panelşn form üzerindeki bailangıç konumunu belirler.
                Size = new Size( //Panelin koltuk sayısına göre otomatik hesaplanan genişlik ve yüksekliğini belirler.
                    cols * size + (cols - 1) * gap, // Panelin genişliğini hesaplar. gap= koltuk butonlarının arasında bırakılan boşluk mesafesini ifade eder. Koltukların bitişik görünmesini engeller.
                    rows * size + (rows - 1) * gap) //Panelin yüksekliğini hesaplar. 
            };

            Controls.Add(panelKoltuklar); //Oluşturulan paneli formun ana kontrol listesine ekler ?

            for (int i = 0; i < rows; i++) // Her satırdaki koltukları oluşturmak için sütunlar arasında dönen iç döngüdür.
            {
                for (int j = 0; j < cols; j++) // Her sütundaki koltukları oluşturmak için satırlar arasında dönen dış döngüdür.
                {
                    Button btn = new Button //Yeni bir buton nesnesi oluşturur.
                    {
                        Width = size, //Butonun genişlik değerini belirler.
                        Height = size, //Butonun yükseklik değerini belirler.
                        Left = j * (size + gap), //Butonun panel içindeki yatay konumunu belirler.
                        Top = i * (size + gap), //Butonun panel içindeki dikey konumunu belirler.
                        Text = $"{(char)('A' + i)}{j + 1}", //Butonların üzerindeki A1.B2 gibi koltuk numaralarının yazılmasını sağlayan komut satırıdır.
                        BackColor = Color.WhiteSmoke //Koltukların başlangıç rengini belirler.
                    };

                    btn.Click += Seat_Click;//Koltuk butonuna tıklandığında çalışacak Click olayını bağlar. Click : Kullanıcının fareyle tıklaması sonucu tetiklenen olay anlamına gelir.
                    panelKoltuklar.Controls.Add(btn); //Oluşturulan koltuk butonunu panelin içine ekler.
                }
            }
        }

        private void KoltukRenkleriniDbyeGoreAyarla() //Koltuk renklerinin veritabanındaki dolu/boş bilgisine göre ayarlamak için tanımlanmış metottur.
        {
            try //Veritabanı işlemleri sırasında oluşabilecek hataları yakalamak için hata yönetim bloğu başlatır.
            {
                using (var conn = new MySqlConnection(cs)) //MySQL veritabanı bağlantısı açar ve işlem bitince otomatik olarak kapatılmasını sağlar.
                {
                    conn.Open(); //Veritabanı bağlantısını aktif hale getirir.

                    foreach (Control c in panelKoltuklar.Controls) //Panel içindeki tüm kontrolleri tek tek dolaşır.
                    {
                        if (c is Button koltuk) //Paneldeki kontrolün bit buton (koltuk) olup olmadığını kontrol eder. 
                                                

                        {
                            var cmd = new MySqlCommand( 
                                "SELECT COUNT(*) FROM bilet " + 
                                "WHERE etkinlik_id=@e AND koltuk_no=@k AND durum='Aktif'", 
                                conn);
                            

                            cmd.Parameters.AddWithValue("@e", AKTIF_ETKINLIK_ID); 
                            cmd.Parameters.AddWithValue("@k", koltuk.Text); 

                            int sayi = Convert.ToInt32(cmd.ExecuteScalar()); 
                            koltuk.BackColor = (sayi > 0) ? Color.Red : Color.WhiteSmoke; 
                        }
                    }
                }
            }
            catch (Exception ex) // try bloğunda oluşan herhangi bir hatayı yakalamak için kullanılan hata yakalama bloğudur.
            {// catch bloğunun başladığını gösterir.
                MessageBox.Show("Koltuk durumu okunamadı:\n" + ex.Message); //Hata oluştuğunda kullanıcıya, hata açıklamasıyla birlikte bilgilendirici bir mesaj gösterir.
            }
        }

        private void IptalKontrolleriniKur() //İptal işlemleri için gerekli arayüz kontrollerini oluşturmak ve ayarlamak amacıyla tanımlanmış metottur.
        {
            chkIptal = new CheckBox //İptal modunu açıp kapatmak için yeni bir ChecBox nesnesi oluşturur.
            {
                Name = "chkIptal", //ChecBox'a kod içinde erişmek için kullanılacak benzersiz adını belirler.
                Text = "İptal Modu", //ChecBox'ın ekranda kullanıcıya gösterilecek metini belirler.
                AutoSize = true, //CheckBox'ın boyutunun içeriğine göre otomatik ayarlanmasını sağlar.
                Left = btnSave.Left, //CheckBox'ın yatay konumunu btnSave butonuyla hizalar.
                Top = btnSave.Bottom + 5 //CheckBox'ın btnSave butonunun biraz altına yerleştirir.
            };

            btnIptal = new Button //İptal işlemini başlatmak için yeni bir Button nesnesi oluşturur.
            { //Buton özelliklerinin ayarlandığı bloğun başladıpını gösterir.
                Name = "btnIptalDinamik", //Butona kod içinde erişmek için kullanılacak benzersiz adını belirler.
                Text = "Bileti İptal Et",//Butonun kullanıcıya ekranda gösterilecek metini blirler.
                Width = btnSave.Width, //Butonun genişliğini btnSave butonuyla aynı yapar.
                Height = 35, //Butonun yüksekliğini piksel cinsinden belirler.
                Left = btnSave.Left, //Butonun yatay konumunu btnSave butonuyla hizalar.
                Top = chkIptal.Bottom + 5, //Butonu, iptal modu CheckBox'ının biraz altına yerleştirir.
                Visible = false //Butonu başlangıçta gizler, sadece iptal modu aktif olduğunda görünür.
            };

            chkIptal.CheckedChanged += (s, a) => //CheckBox'ın işaretlenme durumunu değiştiğinde çalışacak olayı tanımlar.
            {
                iptalModu = chkIptal.Checked; //CheckBox işaretliyse iptal modunu aktif, değilse pasif yapar.
                btnIptal.Visible = iptalModu; //İptal modu açıksa iptal butonunu görünür, kapalıysa gizli yapar.
                btnSave.Enabled = !iptalModu; //İptal modu aktifken kaydet/satın al butonunu devre dışı bırakır. Enabled=? 

                IptalSeciminiTemizle();//Daha önce seçilmiş iptal koltuğu varsa seçimi sıfırlar. 
                SatisSeciminiTemizle();//Satın alma için seçilmiş koltukları sıfırlar.
            };

            btnIptal.Click += btnIptal_Click; //İptal butonuna basıldığında çalışacak Click olayını bağlar.

            Controls.Add(chkIptal); //İptal modu CheckBox'ını forma ekler. 
            Controls.Add(btnIptal);// İptal butonun forma ekler.
        }

        private void Seat_Click(object sender, EventArgs e) //Herhangi bir koltuk butonuna tıklandığında çalışan ortak Click olay metodudur.
        {
            Button tiklanan = (Button)sender; //Tıklanan kontrolü Button türüne çevirerek hangi koltuğa basıldığını belirler.

            // İptal durumuyla ilgili işlemler burada yer alır.
            if (iptalModu) //Uygulamanın iptal modunda olup olmadığını kontrol eder.
            {
                if (tiklanan.BackColor != Color.Red && tiklanan.BackColor != Color.Gold) //Tıklanan koltuğun satılmış ya da iptal için seçili olup olmadığını kontrol eder.
                {
                    MessageBox.Show("İptal için satılmış (kırmızı) koltuk seçmelisiniz."); // Yanlış koltuk seçilirse kullanıcıyı uyarı mesajı ile bilgilendirir.
                    return; // Hatalı seçim durumunda metottan hemen çıkar.
                }
                //Daha önce seçilmiş başka bir iptal koltuğu varsa onu kontrol eder.
                if (iptalSecilenKoltuk != null && iptalSecilenKoltuk != tiklanan)
                    iptalSecilenKoltuk.BackColor = Color.Red; 

                //Aynı koltuğa tekrar tıklarsa iptal seçimini kaldırır.
                if (iptalSecilenKoltuk == tiklanan && tiklanan.BackColor == Color.Gold)
                {
                    tiklanan.BackColor = Color.Red; //İptal seçimi kaldırıldığında koltuğu tekrar satılmış rengine çevirir.
                    iptalSecilenKoltuk = null; //İptal için seçilmiş koltuk bilgisini temizler.
                    txtKoltukNo.Text = "";//Koltuk numarasını boşaltır.
                    return;//Bu işlem tamamlandığında metottan çıkar.
                }

                iptalSecilenKoltuk = tiklanan;// Tıklanan koltuğun iptal edilecek koltuk olarak kaydeder.
                iptalSecilenKoltuk.BackColor = Color.Gold; // İptal için seçilen koltuğu altın renkle vurgular.
                txtKoltukNo.Text = iptalSecilenKoltuk.Text; //Seçilen koltuğun numarasını ekranda gösterir.
                return; //İptal modu işlemleri tamalandığı için metottan çıkar.
            }

            // Normal bilet satın alma moduna ait olduğunu belirtir.
            if (tiklanan.BackColor == Color.Red) //Tıklanan koltuğun satılmış olup olmadığını kontrol eder.
            {
                MessageBox.Show("Bu koltuk dolu."); //Koltuk doluysa kullanıcıyı uyarır.
                return;// Koltuk dolu olduğu için işlemi durdurur ve metottan çıkar.
            }

            if (selectedSeats.Contains(tiklanan)) //Tıklanan koltuğun daha önce seçilmiş olup olmadığını kontrol eder.
            {
                selectedSeats.Remove(tiklanan); //Koltuk zaten seçiliyse seçim listesinden çıkarır.
                tiklanan.BackColor = Color.WhiteSmoke; //Seçimi kaldırılan koltuğu boş koltuk rengine dönüştürür.
            }
            else //Koltuk seçili değilse seçim ekleme kısmına geçer.
            { //Seçim ekleme bloğunun başladığını gösterir.
                selectedSeats.Add(tiklanan); //Yeni seçilen koltuğu seçili koltuklar listesine ekler.
                tiklanan.BackColor = Color.LightGreen; //Seçilen koltuğu yeşil renkle vurgular.
            }

            txtKoltukNo.Text = string.Join(", ", selectedSeats.Select(x => x.Text)); //Seçilen tüm koltuk numaralarını virgülle ayrılmış şekilde ekranda gösterir.
            GuncelleBiletTuruBilgisi(); // Seçilen koltuklara göre bilet türü ve fiyat bilgisini günceller.
        } //Bu kodlar, normal satış modunda dolu koltukların seçilmesini engeller, boş koltukları seçip/çıkarmayı yönetir ve seçime göre fiyat bilgisini günceller.

        // Fiyat hesaplamayla ilgili metotlar burada yer alır.
        private decimal GetKoltukFiyati(char sira) //Verilen koltuk sırasına göre biletin temel fiyatını döndüren metottur.
        {
            switch (char.ToUpper(sira)) //Koltuk büyük harfe çevirir ve hangi sıraya ait olduğunu kontrol eder. Switch = Birden fazla olasılık arasından uygun olan kod bloğunu çalıştırmayı sağlayan karar yapısıdır.
            {
                case 'A': return 500; //case = 
                case 'B': return 450;
                case 'C': return 400;
                case 'D': return 350;
                case 'E': return 300;
                case 'F': return 250;
                default: return 0; //Tanımsız bir sıra gelirse 0 değerini döndürür.
                                   //default: return 0; bu satır olmazsa; switch ifadesine uymayan bir değer geldiğinde metot bir değer döndürmeden biter ve C# derleme hatası verir.
            }
        }

        private decimal GetKoltukIndirimi(char sira) //Koltuk sırasına göre uygulanacak indirim tutarını hesaplayan metottur.
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
                                        out decimal odenecekTutar) //Seçilen koltuklara göre toplam fiyatı, toplam indirimi ve ödenecek tutarı hesaplayan metottur.
        {
            toplamFiyat = 0; //Toplam fiyat değişkenini başlangıçta sıfırlar.
            toplamIndirim = 0;//Toplam indirim değişkenini başlangıçta sıfırlar.

            if (koltuklar == null || koltuklar.Count == 0) //Koltuk listesi boş ve yoksa kontrol eder.
            {
                odenecekTutar = 0; //Seçili koltuk yoksa ödenecek tutarı sıfırlar.
                return; //Hesaplama yapmadan metottan çıkar.
            }

            foreach (var koltuk in koltuklar) //Seçilen her koltuğu tek tek dolaşır.
            {
                if (string.IsNullOrWhiteSpace(koltuk)) continue; //Geçersiz(boş) koltuk bilgisi varsa o koltuğu atlar.

                char sira = char.ToUpper(koltuk[0]); //Koltuk numarasının ilk harfini (A-F) sıra bilgisi olarak alır.
                toplamFiyat += GetKoltukFiyati(sira);//Koltuk sırasına göre fiyatı toplama ekler. 
                toplamIndirim += GetKoltukIndirimi(sira);//Koltuk sırasına göre indirimi toplama ekler.
            }

            // Birden fazla koltuk alımlarında indirim yapılıyor.
            if (koltuklar.Count < 2) toplamIndirim = 0; //Tek koltuk seçildiyse indirimi iptal eder.

            odenecekTutar = toplamFiyat - toplamIndirim; //Toplam fiyattan indirimi çıkararak ödenecek net tutarı hesaplar.
        }

        private void GuncelleBiletTuruBilgisi() //Seçilen koltuklara göre ekrandaki fiyat bilgisini güncelleyen metottur.
        {
            comboBox1.Items.Clear(); //ComboBox içindeki önceki tüm bilgileri temizler.

            if (selectedSeats.Count == 0) //Henüz hiç koltuk seçilmemişse kontrol eder.
            {
                comboBox1.Items.Add("Koltuk seçiniz"); //Kullanıcıya bilgilendirici mesaj ekler.
                comboBox1.SelectedIndex = 0; //ComboBox'ta bu mesajın seçili görünmesini sağlar.
                return;//Koltuk seçimi yoksa metottan çıkar.
            }

            List<string> koltukKodlari = selectedSeats.Select(b => b.Text).ToList(); //Seçili koltuk butonlarının üzerindeki yazıları (A1,B2 vb.) listeye çevirir.

            decimal toplamFiyat, toplamIndirim, odenecekTutar; //Hesaplama sonuçlarını tutacak değişkenleri tanımlar.
            HesaplaToplamTutar(koltukKodlari, out toplamFiyat, out toplamIndirim, out odenecekTutar); //Seçili koltuklara göre toplam tutar hesaplamasını yaptırır.

            comboBox1.Items.Add($"Toplam: {odenecekTutar} TL (İndirim: {toplamIndirim} TL)"); //Hesaplanan toplam ve indirim bilgisini ComboBox'ta kullanıcıya gösterir.
            comboBox1.SelectedIndex = 0; //Gösterilen fiyat bilgisini seçili hale getirir.
        }

        private void btnSave_Click(object sender, EventArgs e) //Kaydet/Satın Al butonuna basıldığında çalışan olay metodudur.
        {
            if (selectedSeats.Count == 0) //Hiç koltuk seçilmediğini ifade ediyor.
            {
                MessageBox.Show("Lütfen koltuk seçin."); //Koltuk seçilmemişse kullanıcıyı uyarı mesajıyla bilgilendirir.
                return;//Koltuk seçilmediği için işlemi durdurur.
            }

            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text)) //Ad Soyad bloğunun boş olup olmadığını kontrol eder. Zorunlu kılan satır hangisi=?

            {
                MessageBox.Show("Lütfen Ad Soyad girin."); //Blok eğer boşsa kullanıcıya uyarı mesajıyla bilgilendirir.
                return;//İsim girilmediği için işlemi durdurur.
            }

            List<string> koltukKodlari = selectedSeats.Select(b => b.Text).ToList(); //Seçilen koltkların koflarını (A1,B2 vb) listeye çevirir.

            decimal toplamFiyat, toplamIndirim, odenecekTutar; // Tutulacak değişkenleri tanımlar.
            HesaplaToplamTutar(koltukKodlari, out toplamFiyat, out toplamIndirim, out odenecekTutar); //Seçilen koltuklara göre fiyat ve indirim hesaplamasını yapar.

            List<int> biletIdleri = new List<int>(); //Veritabanına eklenen biletlerin ID'lerini tutacak listeyi oluşturur.

            try
            {
                using (var conn = new MySqlConnection(cs)) //Veritabanı bağlantısını açar ve işlem bitince otomatik kapanmasını sağlar. Neden birden fazla kez veritabanı bağlantısı açtık?
                {
                    conn.Open(); 

                    foreach (string koltukNo in koltukKodlari) //Seçilen her koltuk için ayrı ayrı bilet kaydı oluşturur.
                    {
                        var cmd = new MySqlCommand( 

                            "INSERT INTO bilet (kullanici_id, etkinlik_id, koltuk_no, satin_alma_tarihi, durum, isim) " +
                            "VALUES (@k, @e, @koltuk, NOW(), 'Aktif', @isim)", conn); 

                        cmd.Parameters.AddWithValue("@k", AKTIF_KULLANICI_ID); 
                        cmd.Parameters.AddWithValue("@e", AKTIF_ETKINLIK_ID);
                        cmd.Parameters.AddWithValue("@koltuk", koltukNo);
                        cmd.Parameters.AddWithValue("@isim", txtAdSoyad.Text); 

                        cmd.ExecuteNonQuery();
                        biletIdleri.Add((int)cmd.LastInsertedId);
                    }
                }

                // Ödeme formunu, gerekli bilgilerle modal (kilitleyici) şekilde açar.BU NE DEMEK?
                new FrmOdeme(cs, AKTIF_KULLANICI_ID, biletIdleri, odenecekTutar).ShowDialog();

                // Satılan koltukların dolu göstereceğini belirtir.Satılan tüm koltukları tek tek dolaşır. NEDEN ?
                foreach (var seat in selectedSeats)
                    seat.BackColor = Color.Red;//Satılan koltukları kırmızı renge çevirir.

                selectedSeats.Clear(); //Seçili koltuk listesini temizler.
                txtKoltukNo.Text = "";//Koltuk numarası alanını temizler.
                txtAdSoyad.Text = "";//Ad Soyad alanını temizler.
                GuncelleBiletTuruBilgisi();//Ekrandaki fiyat bilgisini yeniden günceller
            }
            catch (Exception ex) //Satın alma hatalarını yakalamak için kullanılır.
            {
                MessageBox.Show("Satın alma hatası:\n" + ex.Message); //Oluşan hatayı detay messajıyla birlikte gösterir.
            }
        }
        //btnIptal_Click Metodu
        private void btnIptal_Click(object sender, EventArgs e) //İptal butonuna basıldığında çalışan olay metodudur.
        {
            if (!iptalModu) //İptal modunun açık olup olmadığını kontrol eder.
            {
                MessageBox.Show("Önce İptal Modu'nu açın."); //İptal modu kapalıysa kullanıcıyı uyarı mesajıyla bilgilendirir.
                return;//İptal modu kapalı olduğu için işlemi durdurur.
            }

            if (iptalSecilenKoltuk == null) //İptal edilecek bir koltuk seçilip seçilmediğini kontrol eder.
            {
                MessageBox.Show("İptal edilecek koltuğu seçin."); //Koltuk seçilmediyse kullanıcıya uyarı mesajıyla bilgilendirir
                return; //Koltuk seçilmediği için işlemi durdurur.
            }

            string koltukNo = iptalSecilenKoltuk.Text; //İptal edilecek koltuğun numarasını alır.

            try
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction()) //Yapılacak iptal işlemi tek parça halinde yönetir. BU NE DEMEK?
                    {
                        //Aynı koltuğun eski iptal kayıtlarını temizler.
                        var sil = new MySqlCommand(
                            "DELETE FROM bilet WHERE etkinlik_id=@e AND koltuk_no=@k AND durum='Iptal'",
                            conn, tx);
                        sil.Parameters.AddWithValue("@e", AKTIF_ETKINLIK_ID); //Silme sorgusuna aktif etkinlik ID'sini ekler.
                        sil.Parameters.AddWithValue("@k", koltukNo); //Silme sorgusuna iptal edilecek koltuk numarasını ekler.
                        sil.ExecuteNonQuery();//Silme SQL komutunu çalıştırır.

                        //Aktif bileti iptal etmek için UPDATE SQL komutunu oluşturur. 
                        //İlgili komutun aktif biletini iptal durumuna çevirir.
                        var iptal = new MySqlCommand(
                            "UPDATE bilet SET durum='Iptal', iptal_tarihi=NOW() " +
                            "WHERE etkinlik_id=@e AND koltuk_no=@k AND durum='Aktif' LIMIT 1",
                            conn, tx);
                        iptal.Parameters.AddWithValue("@e", AKTIF_ETKINLIK_ID);//Güncelleme sorgusuna aktif etkinlik ID'sini ekler.
                        iptal.Parameters.AddWithValue("@k", koltukNo);//Güncelleme sorgusuna koltuk numarasını ekler.

                        int affected = iptal.ExecuteNonQuery(); //Kaç satırın eklendiğini kontrol etmek için sonucu alır.
                        if (affected == 0) //Aktif biletin bulunup bulunmadığını kontrol eder.
                        {
                            tx.Rollback(); //İşlemleri geri alır (iptal eder).
                            MessageBox.Show("Aktif bilet bulunamadı.");
                            return; //İşlem başarısız olduğu için metottan çıkar.
                        }

                        tx.Commit(); //Tüm iptal işlemlerini kalıcı olarak veritabanına kaydeder.
                    }
                }

                iptalSecilenKoltuk.BackColor = Color.WhiteSmoke; //İptali tamamlanan koltuğun rengini boş koltuk rengine çevirir.
                iptalSecilenKoltuk = null;//İptal için seçilmiş koltuk bilgisini temizler.
                txtKoltukNo.Text = ""; //Koltuk numarası gösterilen metin alanını boşaltır.
                MessageBox.Show("Bilet iptal edildi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("İptal sırasında hata:\n" + ex.Message);
            }
        }
        //Satış temizleme işlemleriyle ilgili metotlar burada bulunur.
        private void SatisSeciminiTemizle() //Satın alma için seçilmiş koltukları tamamen temizleyen yardımcı metottur.
        {
            foreach (var seat in selectedSeats) //Seçili tüm koltukları tek tek dolaşır.
                if (seat.BackColor == Color.LightGreen) //Sadece satıl alma için seçilmiş koltukları kontrol eder.
                    seat.BackColor = Color.WhiteSmoke;//Yeşil koltukları boş koltuk rengine geri döndürür.

            selectedSeats.Clear(); //Seçili koltuklar listesini tamamen boşaltır.
            txtKoltukNo.Text = ""; //Koltuk numarası alanını temizler.
            GuncelleBiletTuruBilgisi(); //Ekrandaki fiyat/bilet bilgisi alanını günceller.
        }

        private void IptalSeciminiTemizle() //İptal için seçilmiş koltuğu sıfırlayan yardımcı metottur.
        {
            if (iptalSecilenKoltuk != null && iptalSecilenKoltuk.BackColor == Color.Gold) //İptal için seçilmiş ve altın renkte olan koltuğu kontrol eder.
                iptalSecilenKoltuk.BackColor = Color.Red; //İptal seçimini kaldırıp koltuğu tekrar satılmış durumuna getirir.

            iptalSecilenKoltuk = null; //İptal seçimi bilgisini tamamen temizler.
            txtKoltukNo.Text = ""; //Koltuk numarası alanını boşaltır.
        }

        // Designer bağlıysa hata vermesin diye kalsın.Tasarımcı tarafından otomatik bağlandığı için boş bırakılır.
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
