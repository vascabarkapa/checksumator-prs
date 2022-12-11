using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

// grafikon iz WFA
using DataVisualization = System.Windows.Forms.DataVisualization;
using Color = System.Drawing.Color;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.IO;

namespace Checksumator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int brojJezgara;
        public string istorijaIzracunavanja = "";

        // liste potrebnih podataka za cuvanje tokom izvršavanja aplikacije u paralelnom režimu
        List<byte> listaChecksuma = new List<byte>();
        List<string> listaBinarnaPoruka = new List<string>();
        Loading prozorUcitavanja = new Loading();

        public MainWindow()
        {
            // prikaz prozora za učitavanje
            prozorUcitavanja.Show();
            InitializeComponent();

            // učitavanje broja jezgara računara
            brojJezgara = Environment.ProcessorCount;

            // izračunavanje granične vrijednosti
            Automatsko();

            // podešavanje linijskog grafikona, njegovih svojstava i načina iscrtavanja
            grafik.BackColor = Color.FromArgb(102, 255, 255, 255);
            grafik.ChartAreas[0].BackColor = Color.FromArgb(0, 255, 255, 255);

            grafik.ChartAreas[0].AxisX.Title = "Broj karaktera";
            grafik.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Near;
            grafik.ChartAreas[0].AxisX.TitleFont = new Font("Consolas", 7f);
            grafik.ChartAreas[0].AxisY.Title = "Vrijeme izvršavanja [ms]";
            grafik.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Near;
            grafik.ChartAreas[0].AxisY.TitleFont = new Font("Consolas", 7f);
            grafik.ChartAreas[0].AxisX.Minimum = 0;
            grafik.ChartAreas[0].AxisY.Minimum = 0;

            grafik.Legends[0].BackColor = Color.FromArgb(0, 255, 255, 255);
            grafik.Legends[0].Font = new Font("Consolas", 7f);

            grafik.Series[0].ChartType = DataVisualization.Charting.SeriesChartType.Line;
            grafik.Series[0].Color = Color.FromArgb(7, 231, 221);
            grafik.Series[0].BorderWidth = 5;

            grafik.Series[1].ChartType = DataVisualization.Charting.SeriesChartType.Line;
            grafik.Series[1].Color = Color.FromArgb(187, 2, 254);
            grafik.Series[1].BorderWidth = 5;

            // po inicijalizaciji programa, prozor za učitavanje se zatvara,
            // a otvara se glavni prozor
            prozorUcitavanja.Close();
        }

        // zatvaranje aplikacije na "X"
        private void LblZatvori_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        // prevlačenje prozora aplikacije preko naziva aplikacije "C h e c k s u m a t o r"
        private void LblNaslov_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void BtnIzracunaj_Click(object sender, RoutedEventArgs e)
        {
            // ispravan unos (unesena poruka i odabran način izračunavanja)
            if (txtUnesenaPoruka.Text.Length > 0 && (rbSekvencijalno.IsChecked == true || rbParalelno.IsChecked == true || rbAutomatski.IsChecked == true))
            {
                if (rbSekvencijalno.IsChecked == true)
                {
                    // logika blokiranja pojedinih funkcionalnosti
                    btnIzracunaj.IsEnabled = false;
                    rbSekvencijalno.IsEnabled = false;
                    rbAutomatski.IsEnabled = false;
                    rbParalelno.IsEnabled = false;
                    txtUnesenaPoruka.IsReadOnly = true;

                    // pozivanje metode sekvencijalnog izračunavanja
                    // prosljeđuje joj se konvertovana unesena poruka u bajtovima
                    Sekvencijalno(Encoding.ASCII.GetBytes(txtUnesenaPoruka.Text));
                }
                else if (rbParalelno.IsChecked == true)
                {
                    // logika blokiranja pojedinih funkcionalnosti
                    btnIzracunaj.IsEnabled = false;
                    rbSekvencijalno.IsEnabled = false;
                    rbAutomatski.IsEnabled = false;
                    rbParalelno.IsEnabled = false;
                    txtUnesenaPoruka.IsReadOnly = true;

                    // pozivanje metode paralelnog izračunavanja
                    // prosljeđuje joj se konvertovana unesena poruka u bajtovima
                    Paralelno(Encoding.ASCII.GetBytes(txtUnesenaPoruka.Text));
                }
                else if (rbAutomatski.IsChecked == true)
                {
                    // logika blokiranja pojedinih funkcionalnosti
                    btnIzracunaj.IsEnabled = false;
                    rbSekvencijalno.IsEnabled = false;
                    rbParalelno.IsEnabled = false;
                    rbAutomatski.IsEnabled = false;
                    txtUnesenaPoruka.IsReadOnly = true;

                    // pozivanje metode automatskog izračunavanja na osnovu prethodno izvršene računice
                    // prosljeđuje joj se konvertovana unesena poruka u bajtovima
                    // pronađeni (indeks + 1) predstavlja o kojem broju hiljada karaktera se radi kao granici
                    // gdje će se odabrati ili jedan ili drugi način izračunavanja
                    if (txtUnesenaPoruka.Text.Length <= (indeks + 1) * 1000)
                    {
                        // u slučaju da je dužina unesene poruke manja ili jednaka graničnoj vrijednosti
                        // bira se sekvencijalan način izračunavanja
                        Sekvencijalno(Encoding.ASCII.GetBytes(txtUnesenaPoruka.Text));
                    }
                    else
                    {
                        // u slučaju da je dužina unesene poruke veća od granične vrijednosti
                        // bira se paralelan način izračunavanja
                        Paralelno(Encoding.ASCII.GetBytes(txtUnesenaPoruka.Text));
                    }
                }
            }

            // neispravan unos (obavještenja korisniku)
            else if (txtUnesenaPoruka.Text.Length == 0 && (rbSekvencijalno.IsChecked == true || rbParalelno.IsChecked == true || rbAutomatski.IsChecked == true))
            {
                MessageBox.Show("Unesite poruku kako biste izračunali čeksumu!", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (txtUnesenaPoruka.Text.Length > 0 && (rbSekvencijalno.IsChecked == false && rbParalelno.IsChecked == false && rbAutomatski.IsChecked == false))
            {
                MessageBox.Show("Odaberite način izračunavanja čeksume za unesenu poruku!", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Unesite poruku i odaberite način izračunavanja čeksume za unesenu poruku!", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // funkcionalnosti dugmeta "Resetuj"
        // program resetuje sve funkcionalnosti i vraća se u prvobitno stanje, spreman za ispravan unos
        private void BtnResetuj_Click(object sender, RoutedEventArgs e)
        {
            txtBinarnaPoruka.Clear();
            txtUnesenaPoruka.Clear();
            listaChecksuma.Clear();
            listaBinarnaPoruka.Clear();

            btnIzracunaj.IsEnabled = true;
            rbSekvencijalno.IsEnabled = true;
            rbAutomatski.IsEnabled = true;
            rbParalelno.IsEnabled = true;
            rbSekvencijalno.IsChecked = false;
            rbParalelno.IsChecked = false;
            rbAutomatski.IsChecked = false;
            txtUnesenaPoruka.IsReadOnly = false;
        }

        // izračunavanje Checksume sekvencijalno
        private void Sekvencijalno(byte[] podaci)
        {
            // inicijalizacija štoperice za izračunavanje utrošenog vremena
            var vrijeme = new System.Diagnostics.Stopwatch();
            byte checksum = 0;
            string binarniString = string.Empty;

            // startovanje vremena izračunavanja
            vrijeme.Start();

            // glavna for petlja za izračunavanje checksume
            // prolazi se kroz svaki bajt unesene poruke
            for (int i = 0; i < podaci.Length; i++)
            {
                // konverzija bajta u string za potrebe ispisa binarne poruke
                string binarniBajt = Convert.ToString(podaci[i], 2);

                // osmobitni format bajta
                while (binarniBajt.Length < 8)
                {
                    binarniBajt = "0" + binarniBajt;
                }

                // nadovezivanje binarnih bajtova u kompletan string
                binarniString += binarniBajt;

                // sumiranje bajtova
                checksum += podaci[i];
            }

            // prvi komplement sume bajtova za dobijanje checksume
            string checksumString = Convert.ToString(((byte)~checksum), 2);

            // zaustavljanje vremena izvršavanja
            vrijeme.Stop();

            // ispis binarne poruke u textbox
            txtBinarnaPoruka.Text += binarniString;

            // osmobitni format checksume
            while (checksumString.Length < 8)
            {
                checksumString = "0" + checksumString;
            }

            // iscrtavanje rezultata na linijski grafikon
            grafik.Series[0].Points.AddXY(podaci.Length, vrijeme.ElapsedMilliseconds);

            // ispis izračunatih podataka u istoriji izračunavanja
            txtIstorijaIzracunavanja.Text += "Broj karaktera: " + podaci.Length + "\n";

            // u slučaju automatskog odabira - informacija o odabranom režimu
            if(rbAutomatski.IsChecked == true)
            {
                txtIstorijaIzracunavanja.Text += "Način rada: AUTOMATSKI - SEKVENCIJALNO\n";
            }
            else
            {
                txtIstorijaIzracunavanja.Text += "Način rada: SEKVENCIJALNO\n";
            }

            // u slučaju automatskog odabira - informacija o odabranom režimu
            txtIstorijaIzracunavanja.Text += "Checksum: " + checksumString + "\n";
            txtIstorijaIzracunavanja.Text += "Vrijeme izvršavanja: +" + vrijeme.ElapsedMilliseconds.ToString() + "ms\n";
            txtIstorijaIzracunavanja.Text += "----------------------------------------------------------------\n";

            // smještanje detaljne istorije izracunavanja zbog potencijalnog čuvanja istorije
            istorijaIzracunavanja += "Unesena poruka: " + txtUnesenaPoruka.Text + "\n";
            istorijaIzracunavanja += "Broj karaktera: " + podaci.Length + "\n";

            // u slučaju automatskog odabira - informacija o odabranom režimu
            if (rbAutomatski.IsChecked == true)
            {
                istorijaIzracunavanja += "Način rada: AUTOMATSKI - SEKVENCIJALNO\n";
            }
            else
            {
                istorijaIzracunavanja += "Način rada: SEKVENCIJALNO\n";
            }

            istorijaIzracunavanja += "Checksum: " + checksumString + "\n";
            istorijaIzracunavanja += "Vrijeme izvršavanja: +" + vrijeme.ElapsedMilliseconds.ToString() + "ms\n";
            istorijaIzracunavanja += "Binarna poruka: " + txtBinarnaPoruka.Text + "\n";
            istorijaIzracunavanja += "----------------------------------------------------------------\n";
        }

        // izračunavanje Checksume paralelno
        private void Paralelno(byte[] podaci)
        {
            // inicijalizacija liste niti
            List<Thread> listaNiti = new List<Thread>();
            var vrijeme = new System.Diagnostics.Stopwatch();

            // izračunavanje sekvence gdje će se unesena poruka podijeliti na niti sa podjednakom količinom bajtova
            // posljednja nit, u slučaju da veličina unesenih podataka nije djeljiva sa brojem jezgara,
            // dobiće još i ostatak bajtova

            // broj bajtova: 8; broj jezgara: 4;
            // sekvence [2, 2, 2, 2]

            // broj bajtova: 10; broj jezgara: 4;
            // sekvence [2, 2, 2, 4]
            int sekvenca = (int)(podaci.Length / brojJezgara);

            // ako je unesena poruka manja od broja jezgara, proces izračunavanja radiće se
            // sa manjim brojem jezgara radi poboljšanja performansi izračunavanja
            if (podaci.Length < brojJezgara)
            {
                brojJezgara = brojJezgara / 2;
            }

            // startovanje vremena izračunavanja
            vrijeme.Start();

            // ostatak u slučaju neravnomjernog broja podataka
            int ostatak = podaci.Length;

            // formiranje niti u jednakom broju kao i broj jezgara sistema
            for (int i = 0; i < brojJezgara; i++)
            {
                // prvi indeks niza bajtova
                int prvi = i * sekvenca;

                // posljednja nit
                if (i == brojJezgara - 1)
                {
                    // inicijalizacija niti sa funkcionalnošću izračunavanja
                    var nit = new Thread(() =>
                    {
                        byte checksum = 0;
                        string binarniString = string.Empty;

                        // sekvenca izračunavanja sa potencijalnim ostatkom
                        for (int j = prvi; j < prvi + ostatak; j++)
                        {
                            // logika kao i za sekvencijalno izračuvananje checksume
                            string binarniBajt = Convert.ToString(podaci[j], 2);

                            while (binarniBajt.Length < 8)
                            {
                                binarniBajt = "0" + binarniBajt;
                            }
                            binarniString += binarniBajt;
                            checksum += podaci[j];
                            
                        }

                        //dodavanje izračunate checksume za sekvencu i binarne poruke u odgovarajuće liste
                        listaChecksuma.Add(checksum);
                        listaBinarnaPoruka.Add(binarniString);
                    });

                    // dodavanje niti u listu niti za naknadno pokretanje
                    listaNiti.Add(nit);
                }
                // ostale niti
                else
                {
                    // izračunavanje potencijalnog ostatka dok se ne dođe do posljednje niti
                    // ako je ostatak jednak nuli, tada su uneseni bajtovi djeljivi sa brojem jezgara
                    // ako je ostatak različit od nule, tada će u sekvecni za posljednju nit biti dodat i ostatak
                    ostatak -= sekvenca;
                    var nit = new Thread(() =>
                    {
                        byte checksum = 0;
                        string binarniString = string.Empty;

                        // jednaka sekvenca izračunavanja
                        for (int j = prvi; j < prvi + sekvenca; j++)
                        {
                            // logika kao i za sekvencijalno izračuvananje checksume
                            string binarniBajt = Convert.ToString(podaci[j], 2);

                            while (binarniBajt.Length < 8)
                            {
                                binarniBajt = "0" + binarniBajt;
                            }
                            binarniString += binarniBajt;
                            checksum += podaci[j];
                            
                        }

                        //dodavanje izračunate checksume za sekvencu i binarne poruke u odgovarajuće liste
                        listaChecksuma.Add(checksum);
                        listaBinarnaPoruka.Add(binarniString);
                    });

                    // dodavanje niti u listu niti za naknadno pokretanje
                    listaNiti.Add(nit);
                }
            }

            // paralelno pozivanje niti za izračunavanje checksume
            Parallel.ForEach(listaNiti, nit =>
            {
                nit.Start();
                nit.Join();
            });

            // sumiranje pojedinačnih checksuma iz niti
            byte sum = 0;
            for (int i = 0; i < listaChecksuma.Count; i++)
            {
                sum += listaChecksuma[i];

            }

            // prvi komplement sume bajtova za dobijanje checksume
            string checksumString = Convert.ToString(((byte)~sum), 2);

            // zaustavljanje vremena izračunavanja
            vrijeme.Stop();

            // ispis binarne poruke u textbox
            for(int i =0; i<listaBinarnaPoruka.Count;i++)
            {
                txtBinarnaPoruka.Text += listaBinarnaPoruka[i];
            }

            // osmobitni format checksume
            while (checksumString.Length < 8)
            {
                checksumString = "0" + checksumString;
            }

            // iscrtavanje rezultata na linijski grafikon
            grafik.Series[1].Points.AddXY(podaci.Length, vrijeme.ElapsedMilliseconds);

            // ispis izračunatih podataka u istoriji izračunavanja
            txtIstorijaIzracunavanja.Text += "Broj karaktera: " + podaci.Length + "\n";

            // u slučaju automatskog odabira - informacija o odabranom režimu
            if (rbAutomatski.IsChecked == true)
            {
                txtIstorijaIzracunavanja.Text += "Način rada: AUTOMATSKI - PARALELNO\n";
            }
            else
            {
                txtIstorijaIzracunavanja.Text += "Način rada: PARALELNO\n";
            }
            txtIstorijaIzracunavanja.Text += "Checksum: " + checksumString + "\n";
            txtIstorijaIzracunavanja.Text += "Vrijeme izvršavanja: +" + vrijeme.ElapsedMilliseconds.ToString() + "ms\n";
            txtIstorijaIzracunavanja.Text += "----------------------------------------------------------------\n";

            // smještanje detaljne istorije izracunavanja zbog potencijalnog čuvanja istorije
            istorijaIzracunavanja += "Unesena poruka: " + txtUnesenaPoruka.Text + "\n";
            istorijaIzracunavanja += "Broj karaktera: " + podaci.Length + "\n";

            // u slučaju automatskog odabira - informacija o odabranom režimu
            if (rbAutomatski.IsChecked == true)
            {
                istorijaIzracunavanja += "Način rada: AUTOMATSKI - PARALELNO\n";
            }
            else
            {
                istorijaIzracunavanja += "Način rada: PARALELNO\n";
            }

            istorijaIzracunavanja += "Checksum: " + checksumString + "\n";
            istorijaIzracunavanja += "Vrijeme izvršavanja: +" + vrijeme.ElapsedMilliseconds.ToString() + "ms\n";
            istorijaIzracunavanja += "Binarna poruka: " + txtBinarnaPoruka.Text + "\n";
            istorijaIzracunavanja += "----------------------------------------------------------------\n";
        }
        
        // čuvanje slike linijskog grafika
        private void BtnSacuvajGrafik_Click(object sender, RoutedEventArgs e)
        {
            // inicijalizacija dijaloga za čuvanje
            SaveFileDialog cuvanjeGrafikaDialog = new SaveFileDialog();

            // slike se čuvaju u .png formatu
            cuvanjeGrafikaDialog.Filter = "PNG files (*.png)|*.png";

            // defaultna lokacija čuvanja grafika - desktop
            cuvanjeGrafikaDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            cuvanjeGrafikaDialog.Title = "Čuvanje linijskog grafika izračunavanja checksume";
            if (cuvanjeGrafikaDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                grafik.SaveImage(cuvanjeGrafikaDialog.FileName, DataVisualization.Charting.ChartImageFormat.Png);

                // obavještenje korisniku o uspješno sačuvanom grafikonu
                MessageBox.Show("Grafik izračunavanja checksume uspješno sačuvan!", "Čuvanje", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // čuvanje detaljne istorije izračunavanja
        private void BtnSacuvajIstorijuIzracunavanja_Click(object sender, RoutedEventArgs e)
        {
            // inicijalizacija dijaloga za čuvanje
            SaveFileDialog cuvanjeIstorijeIzracunavanjaDialog = new SaveFileDialog();

            // slike se čuvaju u .png formatu
            cuvanjeIstorijeIzracunavanjaDialog.Filter = "TXT files (*.txt)|*.txt";

            // defaultna lokacija čuvanja grafika - desktop
            cuvanjeIstorijeIzracunavanjaDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            cuvanjeIstorijeIzracunavanjaDialog.Title = "Čuvanje linijskog grafika izračunavanja checksume";
            if (cuvanjeIstorijeIzracunavanjaDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // ispisivanje teksta u novi fajl
                File.WriteAllText(cuvanjeIstorijeIzracunavanjaDialog.FileName, istorijaIzracunavanja);

                // obavještenje korisniku o uspješno sačuvanom grafikonu
                MessageBox.Show("Istorija izračunavanja uspješno sačuvana!", "Čuvanje", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // testni string (Lorem ipsum...) za testiranje optimalne granične vrijednosti kada program prelazi
        // sa sekvencijalnog na paralelni način izračunavanja (u dokumentaciji pokazano da se sa manjim brojem karaktera
        // više isplati raditi sekvencijalno, a sa više karaktera radi se paralelno)

        // testni tekst sadrži 1000 karaktera
        string test = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum.  ";

        // liste vremena izvršavanja izračunavanja za različite dužine karaktera
        // testiranje se obavlja za sljedeći broj karaktera na indeksima:
        //      1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000
        //      [0]   [1]   [2]   [3]   [4]   [5]   [6]   [7]   [8]   [9]
        List<long> listaTestnihSekvencijalnihIzracunavanja = new List<long>();
        List<long> listaTestnihParalelnihIzracunavanja = new List<long>();

        // pamćenje indeksa radi izračunavanja granične vrijednosti
        int indeks;
        
        // funkcija koja analizira optimalnu graničnu vrijednost za razliku između
        // sekvencijalnog i paralelnog načina izračunavanja čeksume
        private void Automatsko()
        {
            // granična vrijednost se bira tako što se pronađe minimalna razlika između vremena izvršavanja
            // za iste indekse, odnosno za iste brojeve karaktera
            long min;

            // početni tekst će služiti kao dodatak za nadovezivanje na prethodni kako bi se
            // postigao odgovarajući broj karaktera za testiranje
            string dodatak = test;

            // 10 izračunavanja na oba načina rada, kako bi se postigla granična vrijednost
            // između sekvencijalnog i paralelnog načina izračunavanja
            for (int i = 0; i < 9; i++)
            {
                listaTestnihSekvencijalnihIzracunavanja.Add(TestnoSekvencijalno(Encoding.ASCII.GetBytes(test)));
                listaTestnihParalelnihIzracunavanja.Add(TestnoParalelno(Encoding.ASCII.GetBytes(test)));
                //Thread.Sleep(500);
                test += dodatak;
            }

            // prvobitna pretpostavljena minimalna vrijednost je apsolutna razlika vremena izvršavanja za 1000 karaktera
            long minimalnaRazlika = listaTestnihSekvencijalnihIzracunavanja[0] - listaTestnihParalelnihIzracunavanja[0];

            // traženje minimalne vrijednosti razlike vremena izvršavanja između odgovarajućih brojeva karaktera
            for (int i = 1; i < listaTestnihSekvencijalnihIzracunavanja.Count; i++)
            {
                long razlika = listaTestnihSekvencijalnihIzracunavanja[i] - listaTestnihParalelnihIzracunavanja[i];

                if (Math.Abs(minimalnaRazlika) > Math.Abs(razlika))
                {
                    minimalnaRazlika = razlika;

                    // pamćenje indeksa gdje se nalazi najmanja razlika
                    indeks = i;
                }
            }

            //pronađena minimalna razlika
            min = minimalnaRazlika;
        }

        // testni sekvencijalni način rada - slična prethodna logika kao za sekvencijalno izračunavanje čeksume u programu
        // funkcija vraća vrijeme izvršavanja izračunavanja
        private long TestnoSekvencijalno(byte[] podaci)
        {
            var vrijeme = new System.Diagnostics.Stopwatch();
            byte checksum = 0;
            string binarniString = string.Empty;

            vrijeme.Start();

            for (int i = 0; i < podaci.Length; i++)
            {
                string binarniBajt = Convert.ToString(podaci[i], 2);

                while (binarniBajt.Length < 8)
                {
                    binarniBajt = "0" + binarniBajt;
                }

                binarniString += binarniBajt;
                checksum += podaci[i];
            }

            string checksumString = Convert.ToString(((byte)~checksum), 2);
            vrijeme.Stop();

            listaChecksuma.Clear();
            return vrijeme.ElapsedMilliseconds;
        }

        // testni paralelni način rada - slična prethodna logika kao za paralelno izračunavanje čeksume u programu
        // funkcija vraća vrijeme izvršavanja izračunavanja
        private long TestnoParalelno(byte[] podaci)
        {
            List<Thread> listaNiti = new List<Thread>();
            var vrijeme = new System.Diagnostics.Stopwatch();
            int sekvenca = (int)(podaci.Length / brojJezgara);

            if (podaci.Length < brojJezgara)
            {
                brojJezgara = brojJezgara / 2;
            }

            vrijeme.Start();
            int ostatak = podaci.Length;

            for (int i = 0; i < brojJezgara; i++)
            {
                int prvi = i * sekvenca;

                if (i == brojJezgara - 1)
                {
                    var nit = new Thread(() =>
                    {
                        byte checksum = 0;
                        string binarniString = string.Empty;

                        for (int j = prvi; j < prvi + ostatak; j++)
                        {
                            string binarniBajt = Convert.ToString(podaci[j], 2);

                            while (binarniBajt.Length < 8)
                            {
                                binarniBajt = "0" + binarniBajt;
                            }
                            binarniString += binarniBajt;
                            checksum += podaci[j];

                        }

                        listaChecksuma.Add(checksum);
                        listaBinarnaPoruka.Add(binarniString);
                    });

                    listaNiti.Add(nit);
                }
                else
                {
                    ostatak -= sekvenca;
                    var nit = new Thread(() =>
                    {
                        byte checksum = 0;
                        string binarniString = string.Empty;

                        for (int j = prvi; j < prvi + sekvenca; j++)
                        {
                            string binarniBajt = Convert.ToString(podaci[j], 2);

                            while (binarniBajt.Length < 8)
                            {
                                binarniBajt = "0" + binarniBajt;
                            }
                            binarniString += binarniBajt;
                            checksum += podaci[j];

                        }

                        listaChecksuma.Add(checksum);
                        listaBinarnaPoruka.Add(binarniString);
                    });

                    listaNiti.Add(nit);
                }
            }

            Parallel.ForEach(listaNiti, nit =>
            {
                nit.Start();
                nit.Join();
            });

            byte sum = 0;
            for (int i = 0; i < listaChecksuma.Count; i++)
            {
                sum += listaChecksuma[i];

            }

            string checksumString = Convert.ToString(((byte)~sum), 2);
            vrijeme.Stop();

            listaChecksuma.Clear();
            return vrijeme.ElapsedMilliseconds;
        }
    }
}
