// FrmOdeme.Designer.cs
namespace beste //Projemin adı
{
    partial class FrmOdeme //Frmodeme butonunun birden fazla dosyaya bölündüğünü anlatır. (Desinger+Kod Dosyası)
    {
        private System.ComponentModel.IContainer components = null; //Form üzerinde bulunan tüm bileşenleri tutan kapsayıcı nesnedir.( button, label vb.)

        protected override void Dispose(bool disposing) //Kullanılmayan kaynakları temizlemek için kullanılır.
        {
            if (disposing && (components != null)) //Forn gerçekten kapanıyorsa ve bileşenler oluşturulmuşsa, bellekten temizlenmesi gerektiğini kontrol eder.
            {
                components.Dispose();//Form kapatıldığında ekranda kullanılan her şeyi kapatır ve bilgisayarı yormasın diye hafızadan siler.
            }
            base.Dispose(disposing); //Form kapanırken, Windows’un hazır form yapısının da düzgün kapanmasını sağlar
        }

        #region Windows Form Designer generated code //Windows Form Designer tarafından oluşturulan kodları içerir. Region: kodu gruplamak ve düzenli göstermek için bir işaretleme.
        private void InitializeComponent() //Form açılırken ekrandaki tüm butnları,yazıları ve kutuları oluşturan metottur.
        {
            this.label1 = new System.Windows.Forms.Label(); //Kart numarası başlığı
            this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();//Kart numarası giriş alanı (maskeli)
            this.label2 = new System.Windows.Forms.Label();//Banka başlığı
            this.label3 = new System.Windows.Forms.Label();//Kart üzerindeki ad soyad başlığı
            this.textBox1 = new System.Windows.Forms.TextBox();//Kart üzerindeki ad soyad giriş alanı
            this.label4 = new System.Windows.Forms.Label();//Son kullanım ay/yıl başlığı
            this.comboBox1 = new System.Windows.Forms.ComboBox();//Son kullanım ay seçimi açılır kutusu
            this.comboBox2 = new System.Windows.Forms.ComboBox();//Son kullanım yıl seçimi açılır kutusu
            this.label5 = new System.Windows.Forms.Label();//CVC başlığı
            this.maskedTextBox2 = new System.Windows.Forms.MaskedTextBox();//CVC giriş alanı (maskeli)
            this.button1 = new System.Windows.Forms.Button();//Öde butonu
            this.button2 = new System.Windows.Forms.Button();//İptal butonu
            this.SuspendLayout();//Foröm üzerindeki kontrollerin yerleşimini geçici olarak durdurur


            //Kart numarası yazısı
            this.label1.AutoSize = true;//Yazının otomatik boyutlanmasını sağlar.
            this.label1.Location = new System.Drawing.Point(25, 60);//Ekrandaki konumu 
            this.label1.Name = "label1";//Nesne adı
            this.label1.Size = new System.Drawing.Size(91, 16); //Boyutları
            this.label1.TabIndex = 0;//Sekme sırası
            this.label1.Text = "Kart Numarası";//Ekranda görünen yazı

            this.maskedTextBox1.Location = new System.Drawing.Point(28, 94); //Konumu
            this.maskedTextBox1.Name = "maskedTextBox1";//Mesne adı
            this.maskedTextBox1.Size = new System.Drawing.Size(229, 22);//Boyutları
            this.maskedTextBox1.TabIndex = 1;//Sekme sırası



            //Banka yazısı
            this.label2.AutoSize = true;//Yazının otomatik boyutlanmasını sağlar.
            this.label2.Location = new System.Drawing.Point(324, 60);//Ekrandaki konumu
            this.label2.Name = "label2";//Nesne adı
            this.label2.Size = new System.Drawing.Size(46, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Banka";//Ekranda görünen yazı
            this.label2.Click += new System.EventHandler(this.label2_Click);
            


            // Kart üzerindeki ad soyad yazısı
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 148);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(160, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "Kart Üzerindeki Ad Soyad";


            //Kart üzerindeki ad soyad giriş alanı,label3
            this.textBox1.Location = new System.Drawing.Point(28, 196);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(229, 22);
            this.textBox1.TabIndex = 4;

            //Son kullanım ay/yıl yazısı label4
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 259);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 16);
            this.label4.TabIndex = 5;
            this.label4.Text = "Son Kullanım Ay/Yıl";
           
            
            
            //Ay seçimi, comboBox1
             
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(28, 295);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(49, 24);
            this.comboBox1.TabIndex = 6;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);



            //Yıl seçimi, comboBox2
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(110, 295);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(80, 24);
            this.comboBox2.TabIndex = 7;


            //CVC yazısı,label5
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(203, 259);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 16);
            this.label5.TabIndex = 8;
            this.label5.Text = "CVC";


            //CVC giriş alanı, maskedTextBox2
            this.maskedTextBox2.Location = new System.Drawing.Point(206, 297);
            this.maskedTextBox2.Name = "maskedTextBox2";
            this.maskedTextBox2.Size = new System.Drawing.Size(100, 22);
            this.maskedTextBox2.TabIndex = 9;



            //Öde butonu, button1
            this.button1.Location = new System.Drawing.Point(231, 360);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "Öde";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);


            //iptal butonu, button2
            this.button2.Location = new System.Drawing.Point(113, 360);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = "İptal";
            this.button2.UseVisualStyleBackColor = true;


            //Form ayarları, FrmOdeme
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.maskedTextBox2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.maskedTextBox1);
            this.Controls.Add(this.label1);
            this.Name = "FrmOdeme"; //Formun kod adı
            this.Text = "FrmOdeme";//Formun başlıkta görünen adı

            //Form düzenleme işlemlerini tekrar başlatır.
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
        //Form üzerindeki kontrollerin tanımları
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MaskedTextBox maskedTextBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox maskedTextBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}
