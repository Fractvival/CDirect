// Czech Post COM Director
// verze: 1.1
// Autor: PROGMaxi software
// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Windows.Forms;


// Nazev hlavniho prostredi
namespace CDirect
{
    // Slouzi pro uchovani nastaveni serialPort (COM porty) nactene z nastaveni
    public struct ComInfo
    {
        public String Name; // Libovolne pojmenovani portu, treba SKENER
        public String PortName; // Nazev COM portu : napr COM1
        public String DataBits; 
        public String StopBit;
        public String Parity;
        public String BaudRate;
        public String HandShake; 
    }

    // Slouzi pro uchovani nastaveni jak nactenych tak aktualnich v prubehu programu
    public struct Setting
    {
        public UInt32 SaveTime; // Cas, v sekundach, pro automaticke ukladani poctu vytisknutych kodu, nesmi byt 0!
        public UInt32 TotalPacket; // Celkovy pocet vytisknutych kodu od pocatku souboru packet.txt
        public UInt32 NowPacket; // Pocet vytisknutych kodu od spusteni programu
        public UInt32 AutoRestart;  // Cas, v sekundach, pro automaticky restart aplikace, 0=vypnuto
                                    // Pokud je autorestart zapnut, deaktivuje savetime!
        public UInt32 MinLenghtBarcode; // Minimalni delka barcode
        public UInt32 MaxLenghtBarcode; // Maximalni delka barcode
        public String PrefixBarcode; // Povel tiskarne ktery se prida pred barcode (prefix+BARCODE)
        public String SuffixBarcode; // Povel tiskarne ktery se pripoji za prefix+barcode (prefix+BARCODE+suffix)
        // BARCODE je ziskan ze zdrojoveho serialPortu, cili ze skeneru
    }

    // Hlavni formular ZACATEK
    /// <summary>
    /// //////////////////////////////////////////////////
    /// </summary>
    public partial class Form1 : Form
    {
        //Vytvoreni instanci ComInfo pro serialPorty
        public ComInfo srcPort; //Zdrojovy COM port
        public ComInfo destPort; //Cilovy COM port
        public Setting setting; //Informace nactene z ulozeneho nastaveni + aktualni pomocne promenne
        public static UInt32 deltaTick = 0;
        public static UInt32 deltaSaveTime = 0;
        public static UInt32 deltaAutoRestart = 0;

        // Inicializace hlavniho formulare
        public Form1()
        {
            InitializeComponent();
        }

        // Tato fce slouzi pro nacteni vsech nastaveni ulozenych v souboru a to vcetne poctu vsech
        // zasilek ktere timto programem dostaly novy carovy kod
        public void LoadSetting()
        {
            // Pomocna promenna pro ulozeni kazdeho radku souboru setting.txt
            // Zde plati, ze soubor setting.txt ma pevne dany pocet radku a nesmi se zmenit a to
            // ani tak, ze by se nejaka hodnota prehodila na misto jine a to ani komentare !
            // !! Jakakoliv zmena v souboru bez predchoziho osetreni primo zde povede k fatalnim nasledkum
            String[] _Setting = new String[61]; //maximalni pocet radku v souboru setting.txt

            // ZDE do teto pomocne promenne dosadim defaultni hodnoty a texty
            // jak lze videt, je to vlastne cely soubor setting.txt
            _Setting[0] = "///////////////////////////////////////////////////////////////////////////////";
            _Setting[1] = "// SOUBOR S NASTAVENIM CDIRECT v1.1";
            _Setting[2] = "// NEPOUZIVAT NA JINE VERZE! NEMAZAT TYTO KOMENTARE A LOMITKA !!";
            _Setting[3] = "// KROME NAZVU ZARIZENI A NAZVU COM PORTU PSAT JEN CISLA, VSE VZDY POD PRISLUSNY KOMENTAR !!";
            _Setting[4] = "//";
            _Setting[5] = "//";
            _Setting[6] = "//";
            _Setting[7] = "// NAZEV PRVNIHO ZARIZENI (libovolny nazev- jde o zdrojove zarizeni, ze ktereho se bude cist)";
            _Setting[8] = "SKENER";
            _Setting[9] = "// NAZEV COM PORTU (napr.: COM1)";
            _Setting[10] = "COM7";
            _Setting[11] = "// RYCHLOST PRENOSU DAT (BaudRate: 2400,4800,9600,19200,23040,28800,38400,57600,115200 - kde 9600 je defaultni)";
            _Setting[12] = "9600";
            _Setting[13] = "// DATABITS (Rozsah hodnot je 5-8, kde 8 je defaultni)";
            _Setting[14] = "8";
            _Setting[15] = "// PARITA (Rozsah hodnot je 0-4, kde 0 je defaultni)";
            _Setting[16] = "0";
            _Setting[17] = "// STOPBIT (Rozsah hodnot je 0-3, kde 1 je defaultni)";
            _Setting[18] = "1";
            _Setting[19] = "// HANDSHAKE (Rozsah hodnot je 0-3, kde 0 je defaultni)";
            _Setting[20] = "0";
            _Setting[21] = "//";
            _Setting[22] = "//";
            _Setting[23] = "//";
            _Setting[24] = "//";
            _Setting[25] = "//";
            _Setting[26] = "//";
            _Setting[27] = "// NAZEV DRUHEHO ZARIZENI (libovolny nazev- jde o cilove zarizeni, na ktere se budou posilat data)";
            _Setting[28] = "TISKARNA";
            _Setting[29] = "// NAZEV COM PORTU (napr.: COM2)";
            _Setting[30] = "COM8";
            _Setting[31] = "// RYCHLOST PRENOSU DAT (BaudRate: 2400,4800,9600,19200,23040,28800,38400,57600,115200 - kde 9600 je defaultni)";
            _Setting[32] = "9600";
            _Setting[33] = "// DATABITS (Rozsah hodnot je 5-8, kde 8 je defaultni)";
            _Setting[34] = "8";
            _Setting[35] = "// PARITA (Rozsah hodnot je 0-4, kde 0 je defaultni)";
            _Setting[36] = "0";
            _Setting[37] = "// STOPBIT (Rozsah hodnot je 0-3, kde 1 je defaultni)";
            _Setting[38] = "1";
            _Setting[39] = "// HANDSHAKE (Rozsah hodnot je 0-3, kde 0 je defaultni)";
            _Setting[40] = "0";
            _Setting[41] = "//";
            _Setting[42] = "//";
            _Setting[43] = "//";
            _Setting[44] = "// CAS, V SEKUNDACH, PRO AUTOMATICKE UKLADANI POCTU ZASILEK";
            _Setting[45] = "// SOUBOR S POCTEM ZASILEK SE JMENUJE PACKET.TXT A TEN OBSAHUJE POUZE ONEN CELKOVY POCET ZASILEK!";
            _Setting[46] = "1800";
            _Setting[47] = "// AUTOMATICKY RESTART APLIKACE V SEKUNDACH (hodnota 0=vypnuto)";
            _Setting[48] = "// JAKAKOLIV HODNOTA VETSI NEZ 0 ZNACI ZAPNUTO A DEAKTIVUJE AUTOMATICKE UKLADANI POCTU ZASILEK";
            _Setting[49] = "// POCTY ZASILEK SE BUDOU UKLADAT PRED AUTOMATICKYM RESTARTEM APLIKACE !";
            _Setting[50] = "0";
            _Setting[51] = "// MINIMALNI DELKA BARCODE (ostatni vyrazuje a nezahrnuje do poctu + taktez nevytiskne barcode)";
            _Setting[52] = "13";
            _Setting[53] = "// MAXIMALNI DELKA BARCODE (ostatni vyrazuje a nezahrnuje do poctu + taktez nevytiskne barcode)";
            _Setting[54] = "13";
            _Setting[55] = "// PREFIX BARCODE (tj. prikaz pro tiskarnu dodany tesne pred barcode)";
            _Setting[56] = "^XA^FO20,100^BY3^BCN,100,Y,N,N^FD";
            _Setting[57] = "// SUFFIX BARCODE (tj. prikaz pro tiskarnu dodany tesne za barcode)";
            _Setting[58] = "^XZ";
            _Setting[59] = "//";
            _Setting[60] = "///////////////////////////////////////////////////////////////////////////////";

            // Zde si definujeme soubor s nastavenim
            String _FileName = Application.StartupPath + "\\setting.txt";
            // Zde si definujeme soubor s celkovym poctem vsech zasilek ktere dostaly barcode
            String _PacketFileName = Application.StartupPath + "\\packet.txt";
            // Zde otestujeme existenci vyse zminenych souboru a paklize neexistuji, vytvorime je.
            // V pripade souboru s nastavenim definujeme i jeho strukturu s defaultnim nastavenim a hned ulozime.
            // V pripade neexistence souboru s pocty vytvorime soubor a zapiseme do nej nulu (0).
            // !!! NUTNO PODOTKNOUT ze oba soubory musi byt v jedne slozce s EXE souborem
            if (!File.Exists(_FileName)) // Jeslize soubor setting.txt neexistuje..
            {
                // ..zkus
                try
                {
                    // Vytvorime novy soubor setting.txt
                    FileStream fs = File.Create(_FileName); //..vytvor soubor setting.txt
                    fs.Close(); // a ihned jej zavreme
                    // NYNI, tento soubor otevreme coby zapisovac
                    StreamWriter sw = new StreamWriter(_FileName);
                    // ..a radek po radku budeme zapisovat defaultni hodnoty
                    for ( int i = 0; i < _Setting.Length; i++ )
                    {
                        sw.WriteLine(_Setting.GetValue(i)); //getvalue zjisti co se v danem poli nachazi za text
                    }
                    // po skonceni zapisu soubor zavreme
                    sw.Close();
                    // a uvolnime drzavy
                    sw.Dispose();
                }
                // ..pokud predchozi blok ma problem, zobraz jakej..
                catch (IOException ioEx) //..pokud nastane chyba s IO operacemi, vyhod hlasku a skonci program
                {
                    MessageBox.Show("Pri pokusu o vytvoreni souboru s nastavenim se vyskytla chyba > " + ioEx.Message, "Chyba souboru s nastavenim (setting.txt)");
                    Environment.Exit(0); // ukonci cely program !
                }
            }

            // NYNI, provedeme v podstate to same jako v predchozim bode, ovsem se souborem packet.txt

            if (!File.Exists(_PacketFileName)) // Jeslize soubor packet.txt neexistuje..
            {
                // ..zkus na chybu
                try
                {
                    // Vytvorime novy soubor packet.txt
                    FileStream fs = File.Create(_PacketFileName); //..vytvor soubor packet.txt
                    fs.Close(); // a ihned jej zavreme
                    // NYNI, tento soubor otevreme coby zapisovac
                    StreamWriter sw = new StreamWriter(_PacketFileName);
                    sw.WriteLine("0"); // jelikoz je novy, celkovy pocet vytisknutych kodu je 0
                    // po skonceni zapisu soubor zavreme
                    sw.Close();
                    // a uvolnime drzavy
                    sw.Dispose();
                }
                // ..pokud predchozi blok ma problem, zobraz jakej..
                catch (IOException ioEx) //..pokud nastane chyba s IO operacemi, vyhod hlasku a skonci program
                {
                    MessageBox.Show("Pri pokusu o vytvoreni souboru s celkovym poctem vytisknutych kodu se vyskytla chyba > " + ioEx.Message, "Chyba souboru s pocty (packet.txt)");
                    Environment.Exit(0); // ukonci cely program !
                }
            }



            // pomocna promenna pro pocitani radku v souboru setting.txt
            UInt32 readerDelta = 0;
            // pomocna promenna pro ulozeni dat z aktualne nacteneho radku ze souboru setting.txt
            String _Line;
            // instance citace ze souboru s nastavenim a pocty
            StreamReader streamReader;
            // Vytvorime instanci citace streamu pro soubor setting.txt
            try
            {
                streamReader = new StreamReader(_FileName);
                // ..a cteme z nej dokud neni posledni radek nebo neprekrocime maximalni pocet radku
                while ((_Line = streamReader.ReadLine()) != null) //..hezky cteme radek po radku
                {
                    _Setting[readerDelta] = _Line; //..kazdy radek ulozime do pole _Setting[cisloRadkuOdNuly]
                    readerDelta++; //..zvysime pocet radku o jedna
                    if (readerDelta >= Convert.ToUInt32(_Setting.Length)) //..pokud jsme dosahli/prekrocili maximum radku, skoncime smycku
                        break; //ukonceni smycky
                }
                // Zavreme soubor setting.txt
                streamReader.Close();
                // Uvolnime citac z pameti
                streamReader.Dispose();
            }
            catch(IOException ioEx)
            {
                MessageBox.Show("Pri cteni hodnot ze souboru nastala chyba > " + ioEx.Message, "Problem se souborem setting.txt");
            }

            // ****
            // Do instance citace pouziteho v predchozim bode otevreme soubor packet.txt
            try
            {
                streamReader = new StreamReader(_PacketFileName);
                // ..a nacteme z nej radek s poctem vsech vytisknutych zasilek
                _Line = streamReader.ReadLine();
                setting.TotalPacket = Convert.ToUInt32(_Line);
                // Zavreme soubor setting.txt
                streamReader.Close();
                // Uvolnime citac z pameti
                streamReader.Dispose();
            }
            catch(IOException ioEx)
            {
                MessageBox.Show("Pri cteni hodnoty ze souboru nastala chyba > "+ioEx.Message, "Problem se souborem packet.txt");
            }

            // NYNI mame v poli _Setting ulozene vsechny radky ze souboru setting.txt pro pozdejsi zpracovani
            // v tomto poli je ulozena i hodnota celkoveho poctu ze souboru packet.txt

            // ZDE PROVEDU NASTAVENI promennych hodnotami ZE SOUBORU !
            // serialPort1
            srcPort.Name = _Setting[8];
            srcPort.PortName = _Setting[10];
            srcPort.BaudRate = _Setting[12];
            srcPort.DataBits = _Setting[14];
            srcPort.Parity = _Setting[16];
            srcPort.StopBit = _Setting[18];
            srcPort.HandShake = _Setting[20];
            // serialPort2
            destPort.Name = _Setting[28];
            destPort.PortName = _Setting[30];
            destPort.BaudRate = _Setting[32];
            destPort.DataBits = _Setting[34];
            destPort.Parity = _Setting[36];
            destPort.StopBit = _Setting[38];
            destPort.HandShake = _Setting[40];
            // dalsi nastaveni aplikace
            setting.SaveTime = Convert.ToUInt32(_Setting[46]);
            setting.NowPacket = 0;
            setting.AutoRestart = Convert.ToUInt32(_Setting[50]);
            setting.MinLenghtBarcode = Convert.ToUInt32(_Setting[52]);
            setting.MaxLenghtBarcode = Convert.ToUInt32(_Setting[54]);
            setting.PrefixBarcode = _Setting[56];
            setting.SuffixBarcode = _Setting[58];


            //ZDE UZ PRIMO NASTAVUJI PORTY
            //..tedy zkus na chybu
            try
            {
                // Nastav velikost bufferu portu
                serialPort1.ReadBufferSize = 4096;
                // jak dlouho cekat na provedeni operace v milisekundach
                serialPort1.WriteTimeout = 10000;
                serialPort1.PortName = Convert.ToString(srcPort.PortName);
                serialPort1.BaudRate = Convert.ToInt32(srcPort.BaudRate);
                serialPort1.DataBits = Convert.ToInt32(srcPort.DataBits);
                switch (Convert.ToInt32(srcPort.Parity))
                {
                    case 0:
                        serialPort1.Parity = Parity.None;
                        break;
                    case 1:
                        serialPort1.Parity = Parity.Odd;
                        break;
                    case 2:
                        serialPort1.Parity = Parity.Even;
                        break;
                    case 3:
                        serialPort1.Parity = Parity.Mark;
                        break;
                    case 4:
                        serialPort1.Parity = Parity.Space;
                        break;
                    default:
                        serialPort1.Parity = Parity.None;
                        break;
                }
                switch (Convert.ToInt32(srcPort.StopBit))
                {
                    case 0:
                        serialPort1.StopBits = StopBits.None;
                        break;
                    case 1:
                        serialPort1.StopBits = StopBits.One;
                        break;
                    case 2:
                        serialPort1.StopBits = StopBits.Two;
                        break;
                    case 3:
                        serialPort1.StopBits = StopBits.OnePointFive;
                        break;
                    default:
                        serialPort1.StopBits = StopBits.None;
                        break;
                }
                switch (Convert.ToInt32(srcPort.HandShake))
                {
                    case 0:
                        serialPort1.Handshake = Handshake.None;
                        break;
                    case 1:
                        serialPort1.Handshake = Handshake.XOnXOff;
                        break;
                    case 2:
                        serialPort1.Handshake = Handshake.RequestToSend;
                        break;
                    case 3:
                        serialPort1.Handshake = Handshake.RequestToSendXOnXOff;
                        break;
                    default:
                        serialPort1.Handshake = Handshake.None;
                        break;
                }
            }
            catch(IOException ioEx)
            {
                MessageBox.Show("Zdrojovy port (serialPort1) ma neplatne nastaveni! > " + ioEx.Message, "Neplatne nastaveni portu");
            }

            try
            {
                serialPort2.ReadBufferSize = 4096;
                serialPort2.WriteTimeout = -1;
                serialPort2.PortName = Convert.ToString(destPort.PortName);
                serialPort2.BaudRate = Convert.ToInt32(destPort.BaudRate);
                serialPort2.DataBits = Convert.ToInt32(destPort.DataBits);
                switch (Convert.ToInt32(destPort.Parity))
                {
                    case 0:
                        serialPort2.Parity = Parity.None;
                        break;
                    case 1:
                        serialPort2.Parity = Parity.Odd;
                        break;
                    case 2:
                        serialPort2.Parity = Parity.Even;
                        break;
                    case 3:
                        serialPort2.Parity = Parity.Mark;
                        break;
                    case 4:
                        serialPort2.Parity = Parity.Space;
                        break;
                    default:
                        serialPort2.Parity = Parity.None;
                        break;
                }
                switch (Convert.ToInt32(destPort.StopBit))
                {
                    case 0:
                        serialPort2.StopBits = StopBits.None;
                        break;
                    case 1:
                        serialPort2.StopBits = StopBits.One;
                        break;
                    case 2:
                        serialPort2.StopBits = StopBits.Two;
                        break;
                    case 3:
                        serialPort2.StopBits = StopBits.OnePointFive;
                        break;
                    default:
                        serialPort2.StopBits = StopBits.None;
                        break;
                }
                switch (Convert.ToInt32(destPort.HandShake))
                {
                    case 0:
                        serialPort2.Handshake = Handshake.None;
                        break;
                    case 1:
                        serialPort2.Handshake = Handshake.XOnXOff;
                        break;
                    case 2:
                        serialPort2.Handshake = Handshake.RequestToSend;
                        break;
                    case 3:
                        serialPort2.Handshake = Handshake.RequestToSendXOnXOff;
                        break;
                    default:
                        serialPort2.Handshake = Handshake.None;
                        break;
                }
            }
            catch (IOException ioEx)
            {
                MessageBox.Show("Cilovy port (serialPort2) ma neplatne nastaveni! > " + ioEx.Message, "Neplatne nastaveni portu");
            }

            // Pokusime se otevrit zdrojovy port pro nasi appku
            // Ten by mel jit otevrit, jinak nema mnoho smyslu appku spoustet
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    serialPort1.Open();
                }
                else
                {
                    serialPort1.Open();
                }
            }
            catch(UnauthorizedAccessException uaEx)
            {
                MessageBox.Show("Pristup ke zdrojovemu portu byl zamitnut > " + uaEx.Message, "Pristup zamitnut");
            }
            catch(ArgumentOutOfRangeException arEx)
            {
                MessageBox.Show("Zdrojovy port ma neplatne nektere nastaveni > " + arEx.Message, "Neplatne nastaveni");
            }
            catch(ArgumentException aEx)
            {
                MessageBox.Show("Neplatny nazev zdrojoveho portu > " + aEx.Message, "Neplatne nastaveni");
            }
            catch(IOException ioEx)
            {
                MessageBox.Show("Nastala jina vyjimka pri pristupu ke zdrojovemu portu > " + ioEx.Message, "Neplatne nastaveni nebo nespecificka chyba");
            }

            // zrovna tak je potreba otestovat dostupnost ciloveho portu a otevrit jej pro nasi appku
            try
            {
                if (serialPort2.IsOpen)
                {
                    serialPort2.Close();
                    serialPort2.Open();
                }
                else
                {
                    serialPort2.Open();
                }
            }
            catch (UnauthorizedAccessException uaEx)
            {
                MessageBox.Show("Pristup k cilovemu portu byl zamitnut > " + uaEx.Message, "Pristup zamitnut");
            }
            catch (ArgumentOutOfRangeException arEx)
            {
                MessageBox.Show("Cilovy port ma neplatne nektere nastaveni > " + arEx.Message, "Neplatne nastaveni");
            }
            catch (ArgumentException aEx)
            {
                MessageBox.Show("Neplatny nazev ciloveho portu > " + aEx.Message, "Neplatne nastaveni");
            }
            catch (IOException ioEx)
            {
                MessageBox.Show("Nastala jina vyjimka pri pristupu k cilovemu portu > " + ioEx.Message, "Neplatne nastaveni nebo nespecificka chyba");
            }


        }// Konec LoadSetting

        // Spusti se pri spousteni Form1, tedy jeste pred jeho zobrazenim
        private void OnLoad(object sender, EventArgs e)
        {
            // Nastaveni hlavniho casovace, tick je kazdych 25 milisekund
            timer1.Interval = 25;
            // zapnout casovac ted!
            timer1.Start();
            // Nacteni veskereho nastaveni vcetne celkoveho poctu vytisknutych kodu
            LoadSetting();

            // Nastaveni informaci o zdrojovem portu do editBoxu formulare 
            textBox4.Text = srcPort.Name;
            textBox5.Text = serialPort1.PortName;
            textBox6.Text = Convert.ToString(serialPort1.BaudRate);
            textBox7.Text = Convert.ToString(serialPort1.Handshake);
            textBox8.Text = Convert.ToString(serialPort1.StopBits);
            textBox9.Text = Convert.ToString(serialPort1.DataBits);
            textBox10.Text = Convert.ToString(serialPort1.Parity);

            // Nastaveni informaci o cilovem portu do editBoxu formulare 
            textBox17.Text = destPort.Name;
            textBox16.Text = serialPort2.PortName;
            textBox15.Text = Convert.ToString(serialPort2.BaudRate);
            textBox14.Text = Convert.ToString(serialPort2.Handshake);
            textBox11.Text = Convert.ToString(serialPort2.StopBits);
            textBox12.Text = Convert.ToString(serialPort2.DataBits);
            textBox13.Text = Convert.ToString(serialPort2.Parity);

        }

        // Ulozi celkovy pocet vytisknutych kodu
        public void SaveTotalBarcode(String PacketFileName)
        {
            // Pokud existuje soubor packet.txt
            if (File.Exists(PacketFileName))
            {
                try
                {
                    //..smaz ho
                    File.Delete(PacketFileName);
                }
                catch (IOException ioEx)
                {
                    // v pripade nejakych chyb pri mazani souboru vypis pouze debug, program neukoncuj!
                    Debug.WriteLine("SaveTotalBarcode -> TRY delete Packet.txt");
                    Debug.WriteLine("SaveTotalBarcode ->" + ioEx.Message + "\n");
                    Console.Beep(1000, 500);
                    Console.Beep(300, 500);
                    Console.Beep(1000, 500);
                    return;
                }
            }
            try
            {
                // Vytvorime novy soubor packet.txt
                FileStream fs = File.Create(PacketFileName); //..vytvor soubor packet.txt
                fs.Close(); // a ihned jej zavreme
                            // NYNI, tento soubor otevreme coby zapisovac
                StreamWriter sw = new StreamWriter(PacketFileName);
                // Zapis celkovy pocet
                sw.WriteLine(setting.TotalPacket.ToString()); 
                                   // po skonceni zapisu soubor zavreme
                sw.Close();
                // a uvolnime drzavy
                sw.Dispose();
            }
            // ..pokud predchozi blok ma problem, zobraz jakej..
            catch (IOException ioEx) //..pokud nastane chyba s IO operacemi, vyhod hlasku a skonci program
            {
                Debug.WriteLine("SaveTotalBarcode -> TRY create writeline Packet.txt");
                Debug.WriteLine("SaveTotalBarcode ->" + ioEx.Message + "\n");
                Console.Beep(1000, 500);
                Console.Beep(300, 500);
                Console.Beep(1000, 500);
                return;
            }
        }


        // Tick casovace, kazdych 25ms
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Zde do tohoto textboxu se bude vypisovat celkovy cas od psuteni programu
            textBox1.Text = (DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()).ToString();
            // Zde vypisujeme pocet vytisknutych kodu od spusteni programu
            textBox2.Text = Convert.ToString(setting.NowPacket);
            // Zde celkovy pocet ukladany/nacitany ze souboru packet.txt
            textBox3.Text = Convert.ToString(setting.TotalPacket);
            //// Zvedneme deltu o 25 ms (coz je interval tiku casovace)
            deltaTick += 25;
            
            // pokud ubehla sekunda tak..
            if ( deltaTick >= 1000 )
            {
                // ..vynuluj deltu a
                deltaTick = 0;
                // ..pokud je povolen autorestart aplikace tak..
                if ( setting.AutoRestart > 0 )
                {
                    // zvysime deltu Autorestart o 1 a pokud prekona nastaveny casovy limit tak spusti akci
                    deltaAutoRestart += 1;
                    if ( deltaAutoRestart >= setting.AutoRestart )
                    {
                        ///////////////////////
                        //* TOTO POTOM SMAZ !! ->> 
                        deltaAutoRestart = 0;
                        ///
                        // ULOZIME CELKOVY POCET VYTISKNUTYCH KODU - setting.TotalPacket
                        // POTE ZDE SPUSTIME NOVOU INSTANCI PROGRAMU
                        // A UKONCIME TU SOUCASNOU !
                    }
                }
                // .. v opacnem pripade, tedy kdyz neni povolen autorestart, tak..
                else
                {
                    // zvysime deltu ukladani poctu o 1 a pokud prekona nastaveny casovy limit tak spusti akci
                    deltaSaveTime += 1;
                    if ( deltaSaveTime >= setting.SaveTime )
                    {
                        // zresetujeme deltu ukladani poctu
                        deltaSaveTime = 0;
                        // ULOZIME celkovy pocet vytisknutych zasilek - setting.TotalPacket
                        SaveTotalBarcode("packet.txt");
                    }
                }
            }

        } // KONEC fce timer_tick

        // Mensi prevodni fce, potrebujeme citelna data, cista data z portu jsou totiz v HEXa
        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }


        // Pokud prijdou data ze zdrojoveho portu, provedeme akci
        private void onReceivedDataSourceCom(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort spData = (SerialPort)sender;
            byte[] buf = new byte[spData.BytesToRead];
            spData.Read(buf, 0, buf.Length);
            buf = FromHex(BitConverter.ToString(buf));
            String Barcode = Encoding.ASCII.GetString(buf);
            
            int len = Barcode.Length;
            String ch = Barcode.Substring((len-1), 1);
            
            if ( ch = "\\n")
            {

            }
            
            if (Barcode.Length >= setting.MinLenghtBarcode && Barcode.Length <= setting.MaxLenghtBarcode)
            {
                try
                {
                    if (!(serialPort2.IsOpen))
                        serialPort2.Open();
                    serialPort2.Write(setting.PrefixBarcode + Barcode + setting.SuffixBarcode);
                    serialPort2.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("onReceiveDataSourceCom -> TRY serialPort2 open write");
                    Debug.WriteLine("SaveTotalBarcode ->" + ex.Message + "\n");
                    Console.Beep(1000, 500);
                    Console.Beep(300, 500);
                    Console.Beep(1000, 500);
                    return;
                }
                setting.NowPacket++;
                setting.TotalPacket++;
            }
        }

    } //konec Form1

}
// Konec namespace
