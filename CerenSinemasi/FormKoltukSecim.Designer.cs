namespace beste
{
    partial class FormKoltukSecim // Koltuk seçim ekranının Designer (tasarım) kısmı
    {
        private System.ComponentModel.IContainer components = null;
        // Form üzerindeki tüm bileşenleri (kontrolleri) tutan container

        protected override void Dispose(bool disposing)
        // Form kapatılırken kullanılan kaynakları temizler
        {
            if (disposing && (components != null))
            // Eğer yönetilen kaynaklar temizlenecekse ve components boş değilse
            {
                components.Dispose();
                // Form üzerindeki tüm kontrollerin bellek kaynaklarını serbest bırak
            }
            base.Dispose(disposing);
            // Form sınıfının temel Dispose metodunu çağır
        }

        #region Windows Form Designer generated code 
        // Visual Studio Designer tarafından otomatik oluşturulan kod bloğu

        private void InitializeComponent()
        // Form üzerindeki kontrolleri ve temel ayarları başlatır
        {
            this.SuspendLayout();
            // Layout işlemlerini geçici olarak durdurur (performans için)

            // 
            // FormKoltukSecim
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            // Formun otomatik ölçekleme referans boyutları
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            // Ölçeklemenin font bazlı yapılacağını belirtir
            this.ClientSize = new System.Drawing.Size(900, 600);
            // Formun başlangıç genişliği ve yüksekliği
            this.Name = "FormKoltukSecim";
            // Formun kod tarafındaki adı
            this.Text = "Koltuk Seçim";
            // Formun başlık çubuğunda görünen yazı
            this.Load += new System.EventHandler(this.FormKoltukSecim_Load);
            // Form yüklendiğinde FormKoltukSecim_Load metodu çalışır
            this.ResumeLayout(false);
            // Layout işlemlerini tekrar devam ettir
        }

        #endregion
    }
}
