namespace beste
{
    partial class Form1 // Form1 adlı Windows Form'un Designer (tasarım) kısmı
    {
        /// <summary>
        /// Gerekli tasarımcı değişkeni.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        // Form üzerindeki tüm kontrolleri ve bileşenleri yöneten container

        /// <summary>
        /// Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        /// <param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
        protected override void Dispose(bool disposing)
        // Form kapatılırken bellek ve kaynak temizliği yapılmasını sağlar
        {
            if (disposing && (components != null))
            // Eğer yönetilen kaynaklar temizlenecekse ve components boş değilse
            {
                components.Dispose();
                // Form üzerindeki tüm kontrollerin kaynaklarını serbest bırakır
            }
            base.Dispose(disposing);
            // Form sınıfının temel Dispose metodunu çağırır
        }

        #region Windows Form Designer üretilen kod
        // Visual Studio Designer tarafından otomatik olarak oluşturulan kod bölgesi

        /// <summary>
        /// Tasarımcı desteği için gerekli metot - bu metodun 
        /// içeriğini kod düzenleyici ile değiştirmeyin.
        /// </summary>
        private void InitializeComponent()
        // Form üzerindeki tüm kontrollerin oluşturulduğu ve ayarlandığı metot
        {
            this.btnSave = new System.Windows.Forms.Button();
            // "Satın Al" işlemini başlatan buton

            this.txtAdSoyad = new System.Windows.Forms.TextBox();
            // Kullanıcının ad soyad bilgisini girdiği TextBox

            this.comboBox1 = new System.Windows.Forms.ComboBox();
            // Bilet türünün seçildiği ComboBox

            this.label2 = new System.Windows.Forms.Label();
            // "Koltuk No" yazısını gösteren etiket

            this.label3 = new System.Windows.Forms.Label();
            // "Bilet Türü" yazısını gösteren etiket

            this.txtKoltukNo = new System.Windows.Forms.TextBox();
            // Seçilen koltuk numarasının yazıldığı TextBox

            this.label4 = new System.Windows.Forms.Label();
            // "Ad Soyad" yazısını gösteren etiket

            this.label5 = new System.Windows.Forms.Label();
            // Şu an içerik kullanılmayan (boş) etiket

            this.SuspendLayout();
            // Form üzerindeki yerleşim (layout) işlemlerini geçici olarak durdurur

            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(527, 210);
            // Butonun form üzerindeki konumu

            this.btnSave.Name = "btnSave";
            // Butonun kod tarafındaki adı

            this.btnSave.Size = new System.Drawing.Size(261, 23);
            // Butonun genişlik ve yüksekliği

            this.btnSave.TabIndex = 0;
            // Sekme (Tab) sırası

            this.btnSave.Text = "Satın Al";
            // Buton üzerinde görünen yazı

            this.btnSave.UseVisualStyleBackColor = true;
            // Varsayılan Windows stilini kullanır

            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // Butona tıklandığında btnSave_Click metodu çalışır

            // 
            // txtAdSoyad
            // 
            this.txtAdSoyad.Location = new System.Drawing.Point(613, 92);
            // Ad Soyad TextBox konumu

            this.txtAdSoyad.Name = "txtAdSoyad";
            // TextBox adı

            this.txtAdSoyad.Size = new System.Drawing.Size(175, 22);
            // TextBox boyutu

            this.txtAdSoyad.TabIndex = 3;
            // Sekme sırası

            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            // ComboBox içinde listeleme aktif

            this.comboBox1.Location = new System.Drawing.Point(613, 41);
            // ComboBox konumu

            this.comboBox1.Name = "comboBox1";
            // ComboBox adı

            this.comboBox1.Size = new System.Drawing.Size(175, 24);
            // ComboBox boyutu

            this.comboBox1.TabIndex = 4;
            // Sekme sırası

            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // Seçilen bilet türü değiştiğinde çalışan event

            // 
            // label2
            // 
            this.label2.AutoSize = true;
            // Yazıya göre otomatik boyutlanır

            this.label2.Location = new System.Drawing.Point(524, 152);
            // Label konumu

            this.label2.Name = "label2";
            // Label adı

            this.label2.Size = new System.Drawing.Size(64, 16);
            // Label boyutu

            this.label2.TabIndex = 5;
            // Sekme sırası

            this.label2.Text = "Koltuk No";
            // Label üzerinde görünen yazı

            // 
            // label3
            // 
            this.label3.AutoSize = true;
            // Yazıya göre otomatik boyut

            this.label3.Location = new System.Drawing.Point(524, 41);
            // Label konumu

            this.label3.Name = "label3";
            // Label adı

            this.label3.Size = new System.Drawing.Size(63, 16);
            // Label boyutu

            this.label3.TabIndex = 6;
            // Sekme sırası

            this.label3.Text = "Bilet Türü";
            // Label üzerinde görünen yazı

            // 
            // txtKoltukNo
            // 
            this.txtKoltukNo.Location = new System.Drawing.Point(613, 152);
            // Koltuk No TextBox konumu

            this.txtKoltukNo.Name = "txtKoltukNo";
            // TextBox adı

            this.txtKoltukNo.Size = new System.Drawing.Size(175, 22);
            // TextBox boyutu

            this.txtKoltukNo.TabIndex = 8;
            // Sekme sırası

            // 
            // label4
            // 
            this.label4.AutoSize = true;
            // Yazıya göre otomatik boyut

            this.label4.Location = new System.Drawing.Point(524, 92);
            // Label konumu

            this.label4.Name = "label4";
            // Label adı

            this.label4.Size = new System.Drawing.Size(67, 16);
            // Label boyutu

            this.label4.TabIndex = 9;
            // Sekme sırası

            this.label4.Text = "Ad Soyad";
            // Label üzerinde görünen yazı

            // 
            // label5
            // 
            this.label5.AutoSize = true;
            // Yazıya göre otomatik boyut

            this.label5.Location = new System.Drawing.Point(0, 0);
            // Label konumu (şu an kullanılmıyor)

            this.label5.Name = "label5";
            // Label adı

            this.label5.Size = new System.Drawing.Size(0, 16);
            // İçeriği boş olduğu için boyutu 0

            this.label5.TabIndex = 10;
            // Sekme sırası

            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            // Formun otomatik ölçekleme boyutları

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            // Ölçekleme font bazlı yapılır

            this.ClientSize = new System.Drawing.Size(800, 450);
            // Formun başlangıç boyutu

            this.Controls.Add(this.label5);
            // label5 forma eklenir

            this.Controls.Add(this.label4);
            // label4 forma eklenir

            this.Controls.Add(this.txtKoltukNo);
            // txtKoltukNo forma eklenir

            this.Controls.Add(this.label3);
            // label3 forma eklenir

            this.Controls.Add(this.label2);
            // label2 forma eklenir

            this.Controls.Add(this.comboBox1);
            // comboBox1 forma eklenir

            this.Controls.Add(this.txtAdSoyad);
            // txtAdSoyad forma eklenir

            this.Controls.Add(this.btnSave);
            // btnSave forma eklenir

            this.Name = "Form1";
            // Formun kod tarafındaki adı

            this.Text = "Form1";
            // Form başlık çubuğunda görünen yazı

            this.Load += new System.EventHandler(this.Form1_Load);
            // Form yüklendiğinde Form1_Load metodu çalışır

            this.ResumeLayout(false);
            // Layout işlemlerini devam ettir

            this.PerformLayout();
            // Kontrollerin yerleşimini uygula
        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        // Satın Al işlemini başlatan buton

        private System.Windows.Forms.TextBox txtAdSoyad;
        // Ad Soyad girişi yapılan TextBox

        private System.Windows.Forms.ComboBox comboBox1;
        // Bilet türü seçimi yapılan ComboBox

        private System.Windows.Forms.Label label2;
        // Koltuk No etiketi

        private System.Windows.Forms.Label label3;
        // Bilet Türü etiketi

        private System.Windows.Forms.TextBox txtKoltukNo;
        // Koltuk numarasının yazıldığı TextBox

        private System.Windows.Forms.Label label4;
        // Ad Soyad etiketi

        private System.Windows.Forms.Label label5;
        // Şu an kullanılmayan boş etiket

        private System.Windows.Forms.Button btnGeri;
        // Geri butonu (Designer'da tanımlı, kodda henüz ayarlanmayabilir)
    }
}
