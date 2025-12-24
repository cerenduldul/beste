using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySqlConnector;

namespace beste
{
    public partial class FrmEtkinlikSecim : Form
    {
        private readonly string cs =
            "Server=localhost;Database=bilet_sistemi;User Id=root;Password=Ghezzal18.";

        private const int AKTIF_KULLANICI_ID = 1;

        private FlowLayoutPanel flow;

        public FrmEtkinlikSecim()
        {
            BuildUi();
            Load += FrmEtkinlikSecim_Load;
        }

        private void FrmEtkinlikSecim_Load(object sender, EventArgs e)
        {
            EtkinlikleriYukle();
        }

        private void BuildUi()
        {
            Text = "beste";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 900;
            Height = 600;

            var lblBaslik = new Label
            {
                Text = "beste",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Left = 15,
                Top = 10
            };
            Controls.Add(lblBaslik);

            flow = new FlowLayoutPanel
            {
                Left = 15,
                Top = 55,
                Width = ClientSize.Width - 30,
                Height = ClientSize.Height - 80,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true
            };
            Controls.Add(flow);
        }

        private void EtkinlikleriYukle()
        {
            flow.Controls.Clear();

            using (var conn = new MySqlConnection(cs))
            {
                conn.Open();

                // ✅ fiyat bilgisi çekilmiyor (kartta fiyat gösterilmeyecek)
                var cmd = new MySqlCommand(
                    "SELECT etkinlik_id, etkinlik_adi, tarih, konum, afis_yolu " +
                    "FROM etkinlik WHERE etkinlik_id IN (5,6) ORDER BY tarih",
                    conn);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        int etkinlikId = r.GetInt32("etkinlik_id");
                        string ad = r.GetString("etkinlik_adi");
                        DateTime tarih = r.GetDateTime("tarih");
                        string konum = r.GetString("konum");
                        string afis = r.IsDBNull(r.GetOrdinal("afis_yolu")) ? "" : r.GetString("afis_yolu");

                        flow.Controls.Add(CreateCard(etkinlikId, ad, tarih, konum, afis));
                    }
                }
            }
        }

        private Control CreateCard(int etkinlikId, string ad, DateTime tarih,
                                   string konum, string afis)
        {
            var card = new Panel
            {
                Width = 400,
                Height = 220,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10)
            };

            var pic = new PictureBox
            {
                Width = 140,
                Height = 200,
                Left = 10,
                Top = 10,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", afis);
            if (!string.IsNullOrWhiteSpace(afis) && File.Exists(path))
            {
                pic.Image = Image.FromFile(path);
            }

            var lblAd = new Label
            {
                Text = ad,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Left = 160,
                Top = 15,
                Width = 220
            };

            var lblTarih = new Label
            {
                Text = "Tarih: " + tarih.ToString("dd.MM.yyyy HH:mm"),
                Left = 160,
                Top = 60,
                Width = 220
            };

            var lblKonum = new Label
            {
                Text = "Konum: " + konum,
                Left = 160,
                Top = 85,
                Width = 220
            };

            var btnSec = new Button
            {
                Text = "Seç",
                Width = 120,
                Height = 35,
                Left = 160,
                Top = 150
            };

            // ✅ DOĞRU AKIŞ
            btnSec.Click += (s, e) =>
            {
                this.Hide();

                // Form1'e "önceki form" olarak etkinlik ekranını gönderiyoruz
                using (var frmKoltuk = new Form1(etkinlikId, AKTIF_KULLANICI_ID, this))
                {
                    frmKoltuk.ShowDialog();
                }

                // Koltuk ekranı kapanınca tekrar etkinlik ekranını göster
                this.Show();
            };

            card.Controls.Add(pic);
            card.Controls.Add(lblAd);
            card.Controls.Add(lblTarih);
            card.Controls.Add(lblKonum);
            card.Controls.Add(btnSec);

            return card;
        }
    }
}
