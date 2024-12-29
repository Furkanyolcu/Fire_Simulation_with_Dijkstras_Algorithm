using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace OrmanSimulasyon
{
    public partial class Form1 : Form
    {
        private int[,] ormanGrafi;
        private int[] yanginSiddetleri;
        private const int yakitKapasitesi = 5000;
        private const int suKapasitesi = 20000;
        private const int yakitTuketimKm = 10;
        private const int suTuketimDerece = 1000;
        private int mevcutYakit;
        private int mevcutSu;
        private string ormanDosyaYolu;
        private Point[] bolgeKonumlari;
        private Graphics grafik;
        private const int ucakHizi = 50;
        private Bitmap pistGorsel;
        private Bitmap yanginGorsel;
        private Bitmap agacGorsel;
        private Bitmap ucakGorsel;
        private Bitmap arkaPlanBitmap;
        private bool[] yanginDurumlari;

        public Form1()
        {
            InitializeComponent();
            mevcutYakit = yakitKapasitesi;
            mevcutSu = suKapasitesi;
            ormanOku();

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            yanginGorsel = new Bitmap("C:\\Users\\Furkan\\Desktop\\Algoritmalar Final Lab proje\\yangin.png");
            pistGorsel = new Bitmap("C:\\Users\\Furkan\\Desktop\\Algoritmalar Final Lab proje\\pist.png");
            agacGorsel = new Bitmap("C:\\Users\\Furkan\\Desktop\\Algoritmalar Final Lab proje\\agac.png");
            ucakGorsel = new Bitmap("C:\\Users\\Furkan\\Desktop\\Algoritmalar Final Lab proje\\ucak.png");
        }
        private void ormanOku()
        {
            ormanDosyaYolu = "C:\\Users\\Furkan\\Desktop\\Algoritmalar Final Lab proje\\orman.txt";
            string[] satirlar = File.ReadAllLines(ormanDosyaYolu);
            int dugumSayisi = satirlar.Length;
            ormanGrafi = new int[dugumSayisi, dugumSayisi];
            yanginSiddetleri = new int[dugumSayisi];
            yanginDurumlari = new bool[dugumSayisi];

            for (int i = 0; i < dugumSayisi; i++)
            {
                string[] degerler = satirlar[i].Split(',');
                for (int j = 0; j < dugumSayisi; j++)
                {
                    int deger = int.Parse(degerler[j]);
                    ormanGrafi[i, j] = (deger == 0 && i != j) ? int.MaxValue : deger; 
                }
            }

            yanginSiddetleri[0] = 0; 
            yanginSiddetleri[1] = 1; 
            yanginSiddetleri[2] = 3; 
            yanginSiddetleri[3] = 2; 
            yanginSiddetleri[4] = 5; 
            yanginSiddetleri[5] = 4; 
            yanginSiddetleri[6] = 5; 
            yanginSiddetleri[7] = 2; 
            yanginSiddetleri[8] = 1; 
            yanginSiddetleri[9] = 4; 
            yanginSiddetleri[10] = 3;
            yanginSiddetleri[11] = 5; 
            yanginSiddetleri[12] = 2; 
            yanginSiddetleri[13] = 1; 

            for (int i = 0; i < dugumSayisi; i++)
            {
                yanginDurumlari[i] = yanginSiddetleri[i] > 0; 
            }

            hesaplaBolgeKonumlari();
        }


        private void hesaplaBolgeKonumlari()
        {
            int dugumSayisi = ormanGrafi.GetLength(0);
            bolgeKonumlari = new Point[dugumSayisi];
            double genislik = pictureBox1.Width;
            double yukseklik = pictureBox1.Height;

            double merkezX = genislik / 2;
            double merkezY = yukseklik / 2;
            double yaricap = Math.Min(genislik, yukseklik) / 3;
            double aciAraligi = 2 * Math.PI / dugumSayisi;

            for (int i = 0; i < dugumSayisi; i++)
            {
                double aci = i * aciAraligi;
                int x = (int)(merkezX + yaricap * Math.Cos(aci));
                int y = (int)(merkezY + yaricap * Math.Sin(aci));
                bolgeKonumlari[i] = new Point(x, y);
            }
        }

          private int[] dijkstra(int baslangic, out int[] mesafeler)
        {
            int dugumSayisi = ormanGrafi.GetLength(0);
            mesafeler = new int[dugumSayisi];
            bool[] ziyaretEdildi = new bool[dugumSayisi];
            int[] onceki = new int[dugumSayisi];

            for (int i = 0; i < dugumSayisi; i++)
            {
                mesafeler[i] = int.MaxValue;
                ziyaretEdildi[i] = false;
                onceki[i] = -1;
            }

            mesafeler[baslangic] = 0;

            for (int i = 0; i < dugumSayisi; i++)
            {
                int u = -1;

                for (int j = 0; j < dugumSayisi; j++)
                {
                    if (!ziyaretEdildi[j] && (u == -1 || mesafeler[j] < mesafeler[u]))
                    {
                        u = j;
                    }
                }

                if (u == -1) break;

                ziyaretEdildi[u] = true;

                for (int v = 0; v < dugumSayisi; v++)
                {
                    if (ormanGrafi[u, v] > 0 && ormanGrafi[u, v] != int.MaxValue) 
                    {
                        int alternatifMesafe = mesafeler[u] + ormanGrafi[u, v];
                        if (alternatifMesafe < mesafeler[v])
                        {
                            mesafeler[v] = alternatifMesafe;
                            onceki[v] = u;
                        }
                    }
                }
            }

            return onceki;
        }
        private void sondurmePlani()
        {
            int baslangic = 0; 
            int toplamMesafe = 0;
            int toplamYakit = 0;
            int toplamSu = 0;

            string sonuc = "Yangın Söndürme Rotası:\n";
            sonuc += "Başlangıç Noktası: B0\n";
            List<int> rota = new List<int> { baslangic };

            while (true)
            {
                int[] mesafeler;
                int[] oncekiDugumler = dijkstra(baslangic, out mesafeler);

                int sonrakiYangin = -1;
                int enKisaMesafe = int.MaxValue;

                for (int i = 1; i < yanginSiddetleri.Length; i++)
                {
                    if (yanginSiddetleri[i] > 0 && mesafeler[i] < enKisaMesafe && mesafeler[i] != int.MaxValue)
                    {
                        sonrakiYangin = i;
                        enKisaMesafe = mesafeler[i];
                    }
                }

                if (sonrakiYangin == -1) break; 

                int yakitIhtiyaci = yakitHesapla(enKisaMesafe);
                int suIhtiyaci = suHesapla(yanginSiddetleri[sonrakiYangin]);

                if (mevcutYakit < yakitIhtiyaci || mevcutSu < suIhtiyaci)
                {
                    string eksikKaynak = mevcutYakit < yakitIhtiyaci ? "Yakıt" : "Su";
                    animasyonluHareket(baslangic, 0);
                    mevcutYakit = yakitKapasitesi; 
                    mevcutSu = suKapasitesi; 
                    toplamMesafe += mesafeler[0];
                    rota.Add(0);
                    sonuc += $"B0'a dönüldü ve {eksikKaynak} dolduruldu.\n";
                    baslangic = 0;
                    continue;
                }

                mevcutYakit -= yakitIhtiyaci;
                mevcutSu -= suIhtiyaci;
                toplamYakit += yakitIhtiyaci;
                toplamSu += suIhtiyaci;
                toplamMesafe += enKisaMesafe;
                yanginSiddetleri[sonrakiYangin] = 0;

                yanginDurumlari[sonrakiYangin] = false; 
                cizHarita();

                List<int> tamRota = tamRotaCikar(sonrakiYangin, oncekiDugumler);
                sonuc += $"B{baslangic} -> ";
                for (int i = 0; i < tamRota.Count - 1; i++)
                {
                    animasyonluHareket(tamRota[i], tamRota[i + 1]);
                    sonuc += $"B{tamRota[i + 1]} ";
                    if (i < tamRota.Count - 2) sonuc += "-> ";
                }
                sonuc += $"\nB{sonrakiYangin} yangını söndürüldü. Mesafe: {enKisaMesafe} km\n";
                rota.Add(sonrakiYangin);

                baslangic = sonrakiYangin;
            }

            sonuc += "\nRota: " + string.Join(" -> ", rota.Select(r => $"B{r}")) + "\n";
            sonuc += $"Toplam Mesafe: {toplamMesafe} km\n";
            sonuc += $"Tüketilen Yakıt: {toplamYakit} lt\n";
            sonuc += $"Tüketilen Su: {toplamSu} lt\n";

            string sonucDosyaYolu = Path.Combine(Path.GetDirectoryName(ormanDosyaYolu), "sonuc.txt");
            File.WriteAllText(sonucDosyaYolu, sonuc);

            GuncelleDataGrid(sonuc);
            txtLog.Text = "Söndürme planı tamamlandı. Rota ve sonuçlar sonuc.txt dosyasına kaydedildi.";
        }


        private void GuncelleDataGrid(string sonuc)
        {
            string[] satirlar = sonuc.Split('\n');
            foreach (string satir in satirlar) 
            {
                if (!string.IsNullOrWhiteSpace(satir))
                {
                    dataGridView1.Rows.Add(satir);
                }
            }
        }
        private List<int> tamRotaCikar(int hedef, int[] onceki)
        {
            List<int> rota = new List<int>();

            while (hedef != -1)
            {
                rota.Add(hedef);
                hedef = onceki[hedef];
            }

            rota.Reverse(); 
            return rota;
        }

        private void GuncelleTablo()
{
    dataGridView1.Rows.Clear();
    dataGridView1.Columns.Clear();

    int dugumSayisi = ormanGrafi.GetLength(0);

    for (int i = 0; i < dugumSayisi; i++)
    {
        dataGridView1.Columns.Add($"B{i}", $"B{i}");
    }

    for (int i = 0; i < dugumSayisi; i++)
    {
        string[] row = new string[dugumSayisi];
        for (int j = 0; j < dugumSayisi; j++)
        {
            row[j] = ormanGrafi[i, j].ToString();
        }
        dataGridView1.Rows.Add(row);
        dataGridView1.Rows[i].HeaderCell.Value = $"B{i}";
    }
}
        private void animasyonluHareket(int baslangic, int hedef)
        {
            Point baslangicKonum = bolgeKonumlari[baslangic];
            Point hedefKonum = bolgeKonumlari[hedef];

            int adimSayisi = 10; 
            float xAdim = (hedefKonum.X - baslangicKonum.X) / (float)adimSayisi;
            float yAdim = (hedefKonum.Y - baslangicKonum.Y) / (float)adimSayisi;

            for (int i = 0; i <= adimSayisi; i++)
            {
                Thread.Sleep(80); 

                using (Graphics g = pictureBox1.CreateGraphics())
                {
                    g.DrawImage(arkaPlanBitmap, 0, 0);
                    Point yeniKonum = new Point(
                        (int)(baslangicKonum.X + i * xAdim),
                        (int)(baslangicKonum.Y + i * yAdim)
                    );
                    g.DrawImage(ucakGorsel, yeniKonum.X - 15, yeniKonum.Y - 15, 30, 30);
                }
            }

            if (baslangic != 0 && yanginDurumlari[baslangic]) 
            {
                yanginDurumlari[baslangic] = false; 
                cizHarita();
            }
        }

        private void cizHarita()
        {
            arkaPlanBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(arkaPlanBitmap))
            {
                g.Clear(Color.White);

                for (int i = 0; i < ormanGrafi.GetLength(0); i++)
                {
                    for (int j = i + 1; j < ormanGrafi.GetLength(1); j++)
                    {
                        if (ormanGrafi[i, j] > 0 && ormanGrafi[i, j] != int.MaxValue)
                        {
                            Point p1 = bolgeKonumlari[i];
                            Point p2 = bolgeKonumlari[j];
                            g.DrawLine(Pens.Gray, p1, p2);

                            Point ortaNokta = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                            g.DrawString(ormanGrafi[i, j].ToString(), DefaultFont, Brushes.Black, ortaNokta);
                        }
                    }
                }

                for (int i = 0; i < bolgeKonumlari.Length; i++)
                {
                    Point konum = bolgeKonumlari[i];
                    int gorselBoyut = 60;

                    if (i == 0) 
                    {
                        g.DrawImage(pistGorsel, konum.X - gorselBoyut / 2, konum.Y - gorselBoyut / 2, gorselBoyut, gorselBoyut*3/2);
                    }
                    else if (yanginDurumlari[i])
                    {
                        g.DrawImage(yanginGorsel, konum.X - gorselBoyut / 2, konum.Y - gorselBoyut / 2, gorselBoyut, gorselBoyut);
                    }
                    else
                    {
                        g.DrawImage(agacGorsel, konum.X - gorselBoyut / 2, konum.Y - gorselBoyut / 2, gorselBoyut, gorselBoyut);
                    }

                    g.DrawString($"B{i}", DefaultFont, Brushes.Black, konum.X + gorselBoyut / 2 + 5, konum.Y - gorselBoyut / 2);
                }
            }

            pictureBox1.Image = arkaPlanBitmap;
        }

        private int yakitHesapla(int mesafe)
        {
            return mesafe * yakitTuketimKm;
        }

        private int suHesapla(int siddet)
        {
            return siddet * suTuketimDerece;
        }
      

        private void btnSimulate_Click(object sender, EventArgs e)
        {
            grafik = pictureBox1.CreateGraphics();
            grafik.Clear(Color.White);
            cizHarita();
            sondurmePlani();
        }

    }
}
