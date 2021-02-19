using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Windows.Forms;



namespace CDirect
{
    public struct ComInfo
    {
        public String Name;
        public String PortName;
        public String DataBits;
        public String StopBit;
        public String Parity;
        public String BaudRate;
        public String HandShake; 
    }

    public struct Setting
    {
        public String SaveTime;
        public String TotalPacket;
        public String NowPacket;
        public String deltaSave;
    }

    public partial class Form1 : Form
    {

        public ComInfo srcPort;
        public ComInfo destPort;
        public Setting setting;

        public Form1()
        {
            InitializeComponent();
        }

        public void LoadSetting()
        {
            String _FileName = Application.StartupPath + "\\setting.txt";
            String _PacketFileName = Application.StartupPath + "\\packet.txt";
            if (!File.Exists(_FileName))
            {
                try
                {
                    File.Create(_FileName);
                }
                catch (IOException ioEx)
                {
                    MessageBox.Show("Pri pokusu o vytvoreni souboru s nastavenim se vyskytla chyba > " + ioEx.Message, "Chyba souboru s nastavenim");
                    Environment.Exit(0);
                }
            }
            UInt32 readerDelta = 0;
            String _Line;
            String[] _Setting = new String[50];
            StreamReader streamReader = new StreamReader(_FileName);
            while ( (_Line = streamReader.ReadLine()) != null )
            {
                _Setting[readerDelta] = _Line;
                readerDelta++;
                if (readerDelta >= 50)
                    break;
            }
            streamReader.Close();
            streamReader.Dispose();

            // ZDE PROVEDU DEFAULTNI NASTAVENI
            srcPort.Name = "!! Zarizeni-1";
            srcPort.PortName = "COM1";
            srcPort.BaudRate = "9600";
            srcPort.DataBits = "8";
            srcPort.Parity = "none";
            srcPort.StopBit = "1";
            srcPort.HandShake = "none";
            destPort.Name = "!! Zarizeni-2";
            destPort.PortName = "COM2";
            destPort.BaudRate = "9600";
            destPort.DataBits = "8";
            destPort.Parity = "0";
            destPort.StopBit = "1";
            destPort.HandShake = "0";
            setting.SaveTime = "1800";
            setting.TotalPacket = "0";
            setting.NowPacket = "0";
            setting.deltaSave = "0";

            // ZDE PROVEDU NASTAVENI ZE SOUBORU !
            srcPort.Name = _Setting[8];
            srcPort.PortName = _Setting[10];
            srcPort.BaudRate = _Setting[12];
            srcPort.DataBits = _Setting[14];
            srcPort.Parity = _Setting[16];
            srcPort.StopBit = _Setting[18];
            srcPort.HandShake = _Setting[20];
            destPort.Name = _Setting[28];
            destPort.PortName = _Setting[30];
            destPort.BaudRate = _Setting[32];
            destPort.DataBits = _Setting[34];
            destPort.Parity = _Setting[36];
            destPort.StopBit = _Setting[38];
            destPort.HandShake = _Setting[40];
            setting.SaveTime = _Setting[46];

            //ZDE UZ PRIMO NASTAVUJI PORTY
            try
            {
                serialPort1.ReadBufferSize = 4096;
                serialPort1.WriteTimeout = -1;
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

        }

        private void OnLoad(object sender, EventArgs e)
        {
            timer1.Interval = 25;
            timer1.Start();
            LoadSetting();

            textBox4.Text = srcPort.Name;
            textBox5.Text = serialPort1.PortName;
            textBox6.Text = Convert.ToString(serialPort1.BaudRate);
            textBox7.Text = Convert.ToString(serialPort1.Handshake);
            textBox8.Text = Convert.ToString(serialPort1.StopBits);
            textBox9.Text = Convert.ToString(serialPort1.DataBits);
            textBox10.Text = Convert.ToString(serialPort1.Parity);

            textBox17.Text = destPort.Name;
            textBox16.Text = serialPort2.PortName;
            textBox15.Text = Convert.ToString(serialPort2.BaudRate);
            textBox14.Text = Convert.ToString(serialPort2.Handshake);
            textBox11.Text = Convert.ToString(serialPort2.StopBits);
            textBox12.Text = Convert.ToString(serialPort2.DataBits);
            textBox13.Text = Convert.ToString(serialPort2.Parity);

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox1.Text = (DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()).ToString();
            textBox2.Text = Convert.ToString(setting.NowPacket);
            textBox3.Text = Convert.ToString(setting.TotalPacket);
        }
    }
}
