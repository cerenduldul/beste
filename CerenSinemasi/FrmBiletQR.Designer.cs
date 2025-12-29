namespace beste
{
    partial class FrmBiletQR // FrmBiletQR formunun Designer (tasarım) kısmı
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null; // Form üzerindeki bileşenleri (kontrolleri) tutan container

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) // Form kapanırken kaynak temizliği yapılır
        {
            if (disposing && (components != null)) // Yönetilen kaynaklar temizlenecekse ve components boş değilse
            {
                components.Dispose(); // Form üzerindeki tüm kontrollerin kaynaklarını temizle
            }

            // QR resmi dispose // PictureBox içinde yüklenen resim bellekten temizlensin diye
            if (disposing && picQR != null && picQR.Image != null) // picQR ve Image doluysa
            {
                picQR.Image.Dispose(); // Resmin bellek kaynağını serbest bırak
                picQR.Image = null; // Referansı sıfırla (tekrar dispose edilmesin)
            }

            base.Dispose(disposing); // Form'un temel Dispose işlemini çalıştır
        }

        #region Windows Form Designer generated code // Tasarımcı tarafından otomatik üretilen kod bölgesi

        private void InitializeComponent() // Form üzerindeki kontrolleri oluşturur ve özelliklerini ayarlar
        {
            this.picQR = new System.Windows.Forms.PictureBox(); // QR görüntüsünü gösterecek PictureBox
            this.lblOyun = new System.Windows.Forms.Label(); // Oyun/Etkinlik adını gösterecek Label
            this.lblTarih = new System.Windows.Forms.Label(); // Tarih bilgisini gösterecek Label
            this.lblSalon = new System.Windows.Forms.Label(); // Salon/Konum bilgisini gösterecek Label
            this.lblKoltuk = new System.Windows.Forms.Label(); // Koltuk bilgisini gösterecek Label
            this.lblPNR = new System.Windows.Forms.Label(); // PNR kodunu gösterecek Label
            this.lblFiyat = new System.Windows.Forms.Label(); // Fiyat bilgisini gösterecek Label
            ((System.ComponentModel.ISupportInitialize)(this.picQR)).BeginInit(); // PictureBox başlatma (designer standardı)
            this.SuspendLayout(); // Form layout güncellemelerini geçici durdurur (performans)

            // 
            // picQR
            // 
            this.picQR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle; // PictureBox kenarlığı
            this.picQR.Location = new System.Drawing.Point(20, 40); // Ekrandaki konumu (X=20, Y=40)
            this.picQR.Name = "picQR"; // Kontrolün adı (koddan erişim)
            this.picQR.Size = new System.Drawing.Size(250, 250); // Genişlik/Yükseklik
            this.picQR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom; // Resmi oranı bozmadan sığdır
            this.picQR.TabIndex = 0; // Sekme sırası
            this.picQR.TabStop = false; // Tab ile odaklanmasın

            // 
            // lblOyun
            // 
            this.lblOyun.AutoSize = true; // Yazıya göre otomatik boyut
            this.lblOyun.Location = new System.Drawing.Point(304, 40); // Konum
            this.lblOyun.Name = "lblOyun"; // Label adı
            this.lblOyun.Size = new System.Drawing.Size(41, 16); // Varsayılan boyut (designer üretir)
            this.lblOyun.TabIndex = 1; // Sekme sırası
            this.lblOyun.Text = "Oyun:"; // Başlangıç yazısı (sonradan kodla güncellenir)

            // 
            // lblTarih
            // 
            this.lblTarih.AutoSize = true; // Yazıya göre otomatik boyut
            this.lblTarih.Location = new System.Drawing.Point(307, 81); // Konum
            this.lblTarih.Name = "lblTarih"; // Label adı
            this.lblTarih.Size = new System.Drawing.Size(41, 16); // Varsayılan boyut
            this.lblTarih.TabIndex = 2; // Sekme sırası
            this.lblTarih.Text = "Tarih:"; // Başlangıç yazısı

            // 
            // lblSalon
            // 
            this.lblSalon.AutoSize = true; // Yazıya göre otomatik boyut
            this.lblSalon.Location = new System.Drawing.Point(307, 117); // Konum
            this.lblSalon.Name = "lblSalon"; // Label adı
            this.lblSalon.Size = new System.Drawing.Size(45, 16); // Varsayılan boyut
            this.lblSalon.TabIndex = 3; // Sekme sırası
            this.lblSalon.Text = "Salon:"; // Başlangıç yazısı

            // 
            // lblKoltuk
            // 
            this.lblKoltuk.AutoSize = true; // Yazıya göre otomatik boyut
            this.lblKoltuk.Location = new System.Drawing.Point(307, 152); // Konum
            this.lblKoltuk.Name = "lblKoltuk"; // Label adı
            this.lblKoltuk.Size = new System.Drawing.Size(46, 16); // Varsayılan boyut
            this.lblKoltuk.TabIndex = 4; // Sekme sırası
            this.lblKoltuk.Text = "Koltuk:"; // Başlangıç yazısı

            // 
            // lblPNR
            // 
            this.lblPNR.AutoSize = true; // Yazıya göre otomatik boyut
            this.lblPNR.Location = new System.Drawing.Point(307, 192); // Konum
            this.lblPNR.Name = "lblPNR"; // Label adı
            this.lblPNR.Size = new System.Drawing.Size(39, 16); // Varsayılan boyut
            this.lblPNR.TabIndex = 5; // Sekme sırası
            this.lblPNR.Text = "PNR:"; // Başlangıç yazısı

            // 
            // lblFiyat
            // 
            this.lblFiyat.AutoSize = true; // Yazıya göre otomatik boyut
            this.lblFiyat.Location = new System.Drawing.Point(310, 237); // Konum
            this.lblFiyat.Name = "lblFiyat"; // Label adı
            this.lblFiyat.Size = new System.Drawing.Size(39, 16); // Varsayılan boyut
            this.lblFiyat.TabIndex = 6; // Sekme sırası
            this.lblFiyat.Text = "Fiyat:"; // Başlangıç yazısı

            // 
            // FrmBiletQR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F); // Otomatik ölçekleme (designer standardı)
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font; // Font bazlı ölçekleme
            this.ClientSize = new System.Drawing.Size(800, 450); // Formun boyutu
            this.Controls.Add(this.lblFiyat); // lblFiyat form kontrol listesine eklenir
            this.Controls.Add(this.lblPNR); // lblPNR eklenir
            this.Controls.Add(this.lblKoltuk); // lblKoltuk eklenir
            this.Controls.Add(this.lblSalon); // lblSalon eklenir
            this.Controls.Add(this.lblTarih); // lblTarih eklenir
            this.Controls.Add(this.lblOyun); // lblOyun eklenir
            this.Controls.Add(this.picQR); // picQR eklenir
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle; // Form sabit boyutlu (kenardan büyütülemez)
            this.MaximizeBox = false; // Büyütme butonu kapalı
            this.Name = "FrmBiletQR"; // Form adı
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen; // Ekranın ortasında aç
            this.Text = "Bilet ve QR Bilgileri"; // Form başlığı
            this.Load += new System.EventHandler(this.FrmBiletQR_Load); // Form yüklenince FrmBiletQR_Load çalışır
            ((System.ComponentModel.ISupportInitialize)(this.picQR)).EndInit(); // PictureBox init bitişi
            this.ResumeLayout(false); // Layout devam ettir
            this.PerformLayout(); // Kontrollerin layout hesaplarını uygula
        }

        #endregion

        private System.Windows.Forms.PictureBox picQR; // QR görselinin gösterildiği PictureBox
        private System.Windows.Forms.Label lblOyun; // Oyun/Etkinlik adı label
        private System.Windows.Forms.Label lblTarih; // Tarih label
        private System.Windows.Forms.Label lblSalon; // Salon/Konum label
        private System.Windows.Forms.Label lblKoltuk; // Koltuk label
        private System.Windows.Forms.Label lblPNR; // PNR label
        private System.Windows.Forms.Label lblFiyat; // Fiyat label
    }
}
