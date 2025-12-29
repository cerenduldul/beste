using System; //C# temel kütüphanesi
using System.Windows.Forms;// Windows Forms kütüphanesi

namespace beste //Proje adı
{
    internal static class Program //Dışarıdan erişime kapalı olan, uygulamanın başlangıç noktasıdır. İnternal: Dahili
    {
        [STAThread] //arayüzle ilgili tüm işlemlerin aynı sırayla ve aynı kontrol altında çalışması demektir.
        static void Main() //Program açıldığında çalışan ana metot
        {
            Application.EnableVisualStyles(); //Uygulamanın görsel stillerini etkinleştirir.
            Application.SetCompatibleTextRenderingDefault(false); //Metinlerin daha düzgün yazılmasını sağlar.

            Application.Run(new FrmEtkinlikSecim()); // Proje ayarlarında belirtilen başlangıç form. Açılan form: FrmEtkinlikSecim
        }
    }
}

