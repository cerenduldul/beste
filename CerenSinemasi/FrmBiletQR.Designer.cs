namespace beste
{
    partial class FrmBiletQR
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            // QR resmi dispose
            if (disposing && picQR != null && picQR.Image != null)
            {
                picQR.Image.Dispose();
                picQR.Image = null;
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.picQR = new System.Windows.Forms.PictureBox();
            this.lblOyun = new System.Windows.Forms.Label();
            this.lblTarih = new System.Windows.Forms.Label();
            this.lblSalon = new System.Windows.Forms.Label();
            this.lblKoltuk = new System.Windows.Forms.Label();
            this.lblPNR = new System.Windows.Forms.Label();
            this.lblFiyat = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picQR)).BeginInit();
            this.SuspendLayout();
            // 
            // picQR
            // 
            this.picQR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picQR.Location = new System.Drawing.Point(20, 40);
            this.picQR.Name = "picQR";
            this.picQR.Size = new System.Drawing.Size(250, 250);
            this.picQR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picQR.TabIndex = 0;
            this.picQR.TabStop = false;
            // 
            // lblOyun
            // 
            this.lblOyun.AutoSize = true;
            this.lblOyun.Location = new System.Drawing.Point(304, 40);
            this.lblOyun.Name = "lblOyun";
            this.lblOyun.Size = new System.Drawing.Size(41, 16);
            this.lblOyun.TabIndex = 1;
            this.lblOyun.Text = "Oyun:";
            // 
            // lblTarih
            // 
            this.lblTarih.AutoSize = true;
            this.lblTarih.Location = new System.Drawing.Point(307, 81);
            this.lblTarih.Name = "lblTarih";
            this.lblTarih.Size = new System.Drawing.Size(41, 16);
            this.lblTarih.TabIndex = 2;
            this.lblTarih.Text = "Tarih:";
            // 
            // lblSalon
            // 
            this.lblSalon.AutoSize = true;
            this.lblSalon.Location = new System.Drawing.Point(307, 117);
            this.lblSalon.Name = "lblSalon";
            this.lblSalon.Size = new System.Drawing.Size(45, 16);
            this.lblSalon.TabIndex = 3;
            this.lblSalon.Text = "Salon:";
            // 
            // lblKoltuk
            // 
            this.lblKoltuk.AutoSize = true;
            this.lblKoltuk.Location = new System.Drawing.Point(307, 152);
            this.lblKoltuk.Name = "lblKoltuk";
            this.lblKoltuk.Size = new System.Drawing.Size(46, 16);
            this.lblKoltuk.TabIndex = 4;
            this.lblKoltuk.Text = "Koltuk:";
            // 
            // lblPNR
            // 
            this.lblPNR.AutoSize = true;
            this.lblPNR.Location = new System.Drawing.Point(307, 192);
            this.lblPNR.Name = "lblPNR";
            this.lblPNR.Size = new System.Drawing.Size(39, 16);
            this.lblPNR.TabIndex = 5;
            this.lblPNR.Text = "PNR:";
            // 
            // lblFiyat
            // 
            this.lblFiyat.AutoSize = true;
            this.lblFiyat.Location = new System.Drawing.Point(310, 237);
            this.lblFiyat.Name = "lblFiyat";
            this.lblFiyat.Size = new System.Drawing.Size(39, 16);
            this.lblFiyat.TabIndex = 6;
            this.lblFiyat.Text = "Fiyat:";
            // 
            // FrmBiletQR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lblFiyat);
            this.Controls.Add(this.lblPNR);
            this.Controls.Add(this.lblKoltuk);
            this.Controls.Add(this.lblSalon);
            this.Controls.Add(this.lblTarih);
            this.Controls.Add(this.lblOyun);
            this.Controls.Add(this.picQR);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmBiletQR";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bilet ve QR Bilgileri";
            this.Load += new System.EventHandler(this.FrmBiletQR_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picQR)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox picQR;
        private System.Windows.Forms.Label lblOyun;
        private System.Windows.Forms.Label lblTarih;
        private System.Windows.Forms.Label lblSalon;
        private System.Windows.Forms.Label lblKoltuk;
        private System.Windows.Forms.Label lblPNR;
        private System.Windows.Forms.Label lblFiyat;
    }
}
