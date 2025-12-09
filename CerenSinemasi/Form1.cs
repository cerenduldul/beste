using System;//c# dilinin temel kütüphanesi
using System.Collections.Generic; //List, dictionary gibi koleksiyonları kullanmak için kullanılan kütüphanelerdir.
using System.ComponentModel; //Form üzerindeki bileşenlerin (button,label) özelliklerini yönetmek için kullanılır.
using System.Data;//c# da veritabanı işlemlerini yapmak.
using System.Drawing;//Form tasarımı için renk, yazı tipi, koltuk butonları, rengi vb. grafik işlemleri için kullanılır.
using System.Linq; //Listeler üzerinde filtreleme, arama, sıralama yapmak için kullanılır.
using System.Text;//Metin işlemleri için kullanılır.
using System.Threading.Tasks;///Asekron işlemler için kullanılır.
using System.Windows.Forms;//WindowsForm uygulamasının temel kütüphanesidir. 
using MySqlConnector;//Projenin MySQL veritabanına bağlanmasını sağlayan temel kütüphanedir.

namespace beste
{
    public partial class Form1 : Form
    {
        private Button selectedSeat = null; // Seçilen koltuğu tutar

        private Panel panelKoltuklar = null; // Koltukların bulunduğu panel

        private readonly string cs = "Server=localhost;Database=bilet_sistemi;User Id=root;Password=Ghezzal18.";




        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try //hata riski olan kodları güvenli şekilde çalıştırmak için kullanılır.
            {
                using (var conn = new MySqlConnection(cs))
                {
                    conn.Open(); //veritabanı bağlantısını açar.
                    var cmd = new MySqlCommand("SELECT * FROM etkinlik", conn);//cmd etkinlik tablosundan veri çekmek için hazırlanan sql komutudur.
                    var list = new MySqlDataAdapter(cmd); //DataAdapter SQL den gelen verileri c# tarafına taşıyan ara köprüdür.
                    var dt = new DataTable(); //Data Table SQL'den çekilecek verilerin RAM'de tutulduğu geçici c# tablosudur.
                    list.Fill(dt); // DataAdapter, veri tabanındaki bilgileri data table içine doldurur.
                    dataGridView1.DataSource = dt; // DataTable içindeki verileri ekranda tablo şeklinde gösterir. 
                }
            }
            catch (Exception er)
            {
                MessageBox.Show("Hata Oluştu Detay: " + er.Message);
            }

            // Koltukları dinamik oluşturma kısmı
            try
            {
                panelKoltuklar = new Panel();
                panelKoltuklar.Size = new Size(320, 250);
                panelKoltuklar.Location = new Point(12, 30);
                //panelKoltuklar.BackColor = Color.LightGray;
                this.Controls.Add(panelKoltuklar);

                int rows = 5; //5sıra koltuk
                int cols = 8; //Her satırda 8 adet koltuk
                int buttonSize = 40; //koltuk buton genişliğini ifade eder.

                for (int i = 0; i < rows; i++) //satır, toplam 5 satır olucak.
                {
                    for (int j = 0; j < cols; j++) //sütun
                    {
                        Button btn = new Button // Dinamik olarak her koltuğun oluşmasını sağlayan temel satır.
                        {
                            Width = buttonSize, // Butonun genişliğini ifade ediyor.
                            Height = buttonSize, //Butonun yüksekliğini ifade ediyor.
                            Left = j * (buttonSize + 5), //Sütun sayısına göre göre butonun yatay konumu.
                            Top = i * (buttonSize + 5), //Satır sayısına göre butonun dikey konumu
                            Text = $"{(char)('A' + i)}{j + 1}", //Koltuk yazısının oluşturulması.

                           

                            //BackColor = Color.WhiteSmoke //Koltukların ilk rengi.
                        };



                        btn.Click += Seat_Click; // Koltuğa tıklayınca hangi fonksiyonun çalışacağını belirler ; koltuk seçme işlemi yapan metottur.
                        panelKoltuklar.Controls.Add(btn); //Oluşturduğum yeni butonu koltuk panelinin içine ekler.
                    }
                }
            }
            catch (Exception ex) //oluşan hatalar ex isimli değişkene atanır.
            {
                MessageBox.Show("Koltuk oluşturulurken bir hata oluştu.\n\n" + ex.Message, //hatanın nedenini yazar.
                                "Koltuk Oluşturma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            using (var conn = new MySqlConnection(cs))
            {
                conn.Open();

                foreach (Control kontrol in panelKoltuklar.Controls)
                {
                    if (kontrol is Button koltuk)
                    {
                        // Veritabanından durum kontrol komutu
                        var cmd = new MySqlCommand(
                            "SELECT COUNT(*) FROM bilet " +
                            "WHERE etkinlik_id=@etk AND koltuk_no=@koltuk " +
                            "AND durum IN ('Rezerve','Satildi')",
                            conn);

                        cmd.Parameters.AddWithValue("@etk", 2); // aktif etkinlik ID
                        cmd.Parameters.AddWithValue("@koltuk", koltuk.Text);

                        int sonuc = Convert.ToInt32(cmd.ExecuteScalar());

                        // --- DURUMA GÖRE RENKLENDİRME ---
                        if (sonuc > 0)
                        {
                            koltuk.BackColor = Color.Red;       // dolu koltuk
                            //koltuk.Enabled = false;             // tıklanamasın
                        }
                        else
                        {
                            koltuk.BackColor = Color.WhiteSmoke; // boş koltuk, başlangıç rengi
                        }
                    }
                }
            }

            }

        private void Seat_Click(object sender, EventArgs e)
        {
            Button clickedSeat = (Button)sender; //Tıklanan nesneyi butona dönüştürür.

            // Önceden seçili koltuk varsa rengini sıfırla
            if (selectedSeat != null)
                //selectedSeat.BackColor = Color.Green;

            // Yeni koltuğu işaretle, tıklanan koltuğun rengini yeşil yap.
            clickedSeat.BackColor = Color.LightGreen;
            selectedSeat = clickedSeat;

            // Koltuk numarasını textbox2’ye yaz
            textBox2.Text = clickedSeat.Text;

            try
            {


                using (var conn = new MySqlConnection(cs))// mevcut cs bağlantın kullanılıyor.
                {
                    conn.Open();

                    if (dataGridView1.CurrentRow == null)
                    {
                        MessageBox.Show("Lütfen önce listeden bir etkinlik seçiniz.",
                                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // BURADA 'etkinlik_id' YOK, sadece 0. sütun var
                    int etkinlikId = Convert.ToInt32(
                        dataGridView1.CurrentRow.Cells[0].Value);   // ilk kolon = etkinlik_id

                    // Koltuğun dolu olup olmadığını kontrol et 
                    var kontrol = new MySqlCommand(
                        "SELECT COUNT(*) FROM bilet " +
                        "WHERE etkinlik_id=@etk AND koltuk_no=@koltuk " +
                        "AND durum IN ('Rezerve','Satildi')",
                        conn);




                    kontrol.Parameters.AddWithValue("@etk", etkinlikId);
                    kontrol.Parameters.AddWithValue("@koltuk", clickedSeat.Text);

                    int varMi = Convert.ToInt32(kontrol.ExecuteScalar());

                    //eğer koltuk zaten doluysa uyarı ver ve kayıt ekleme
                    if (varMi > 0)
                    {
                        clickedSeat.BackColor = Color.Red;
                        MessageBox.Show("Bu koltuk zaten rezerve edilmiş veya satılmış.",
                            "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    //Koltuk boşsa (başarılı seçim) kayıt oluştur.
                    var ekle = new MySqlCommand(
                      "INSERT INTO bilet (kullanici_id,etkinlik_id, koltuk_no,durum) VALUES (1,@etk,@koltuk,'Rezerve')", conn);
                    ekle.Parameters.AddWithValue("@etk", etkinlikId);
                    ekle.Parameters.AddWithValue("@koltuk", clickedSeat.Text);
                    ekle.ExecuteNonQuery();

                    clickedSeat.BackColor = Color.Yellow;
                    MessageBox.Show($"Koltuk{clickedSeat.Text} Başarıyla rezerve edildi.",
                        "Bilgi", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı işlemlerinde hata oluştu:" + ex.Message);
            }


        }
        // Rezervasyon Butonu
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedSeat == null)
                {
                    MessageBox.Show("Lütfen bir koltuk seçiniz!",
                                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                MessageBox.Show($"Rezervasyon başarıyla yapıldı!\nKoltuk: {selectedSeat.Text}",
                                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rezervasyon sırasında hata oluştu.\n\n" + ex.Message,
                                "Rezervasyon Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ComboBox (Bilet türü seçme)
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Bilet türüne göre fiyat hesaplaması burada olacak
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
