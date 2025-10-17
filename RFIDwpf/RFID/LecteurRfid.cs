using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RFIDwpf.RFID
{
    public class LecteurRfid
    {
        // === DLLs du lecteur ===
        [DllImport("kernel32.dll")]
        static extern void Sleep(int dwMilliseconds);

        [DllImport("MasterRD.dll")]
        static extern int rf_init_com(int port, int baud);

        [DllImport("MasterRD.dll")]
        static extern int rf_ClosePort();

        [DllImport("MasterRD.dll")]
        static extern int rf_antenna_sta(short icdev, byte mode);

        [DllImport("MasterRD.dll")]
        static extern int rf_init_type(short icdev, byte type);

        [DllImport("MasterRD.dll")]
        static extern int rf_request(short icdev, byte mode, ref ushort pTagType);

        [DllImport("MasterRD.dll")]
        static extern int rf_anticoll(short icdev, byte bcnt, IntPtr pSnr, ref byte pRLength);

        [DllImport("MasterRD.dll")]
        static extern int rf_select(short icdev, IntPtr pSnr, byte srcLen, ref sbyte Size);

        // === Événement déclenché quand une carte est scannée ===
        public event EventHandler<string> CardScanned;

        // === Variables internes ===
        public bool bConnectedDevice;
        protected int port;
        protected int baud;
        protected string identifiant;
        private bool lectureEnCours = false;
        private CancellationTokenSource cts;

        public string Identifiant
        {
            get { return identifiant; }
            set { identifiant = value; }
        }

        public int Port
        {
            get { return port; }
            set { if (value > 0) port = value; }
        }

        public int Baud
        {
            get { return baud; }
            set { if (value > 0) baud = value; }
        }

        public LecteurRfid()
        {
            bConnectedDevice = false;
            baud = 19200;
            port = 5; // ✅ Port série du lecteur
        }

        // === Connexion au port série ===
        public int connectionRs()
        {
            int status = rf_init_com(port, baud);
            bConnectedDevice = (status == 0);

            if (bConnectedDevice)
                StartAutoRead(); // ✅ Démarre la lecture automatique

            return status;
        }

        public int fermetureRs()
        {
            if (bConnectedDevice)
            {
                bConnectedDevice = false;
                cts?.Cancel(); // Arrête la boucle de lecture
                return rf_ClosePort();
            }
            return -1;
        }

        // === Lecture RFID "low level" ===
        private int lireIdentifiantCarte()
        {
            short icdev = 0x0000;
            int status = -1;
            byte type = (byte)'A';
            byte mode = 0x52;
            ushort TagType = 0;
            byte bcnt = 0x04;
            IntPtr pSnr;
            byte len = 255;

            if (!bConnectedDevice)
                status = connectionRs();

            if (bConnectedDevice)
            {
                pSnr = Marshal.AllocHGlobal(1024);

                // 🔹 Éteint puis rallume l'antenne pour forcer nouvelle détection
                rf_antenna_sta(icdev, 0);
                Sleep(20);
                rf_init_type(icdev, type);
                Sleep(20);
                rf_antenna_sta(icdev, 1);
                Sleep(50);

                status = rf_request(icdev, mode, ref TagType);
                if (status == 0)
                {
                    status = rf_anticoll(icdev, bcnt, pSnr, ref len);
                    if (status == 0)
                    {
                        byte[] szBytes = new byte[len];
                        for (int j = 0; j < len; j++)
                            szBytes[j] = Marshal.ReadByte(pSnr, j);

                        string m_cardNo = string.Empty;
                        for (int q = 0; q < len; q++)
                            m_cardNo += byteHEX(szBytes[q]);

                        Identifiant = m_cardNo;
                    }
                }

                Marshal.FreeHGlobal(pSnr);
            }
            return status;
        }

        // === Lecture automatique en tâche de fond ===
        private void StartAutoRead()
        {
            if (lectureEnCours)
                return;

            lectureEnCours = true;
            cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                string lastCard = null;
                DateTime lastScanTime = DateTime.MinValue;

                while (!cts.Token.IsCancellationRequested)
                {
                    int status = lireIdentifiantCarte();

                    if (status == 0 && !string.IsNullOrEmpty(Identifiant))
                    {
                        // ✅ Autorise le même badge après 500ms
                        if (Identifiant != lastCard || (DateTime.Now - lastScanTime).TotalMilliseconds > 500)
                        {
                            lastCard = Identifiant;
                            lastScanTime = DateTime.Now;
                            CardScanned?.Invoke(this, Identifiant);
                        }
                    }

                    await Task.Delay(200); // lit toutes les 200ms pour plus de réactivité
                }

                lectureEnCours = false;
            }, cts.Token);
        }

        // === Conversion hexadécimale ===
        public static string byteHEX(byte ib)
        {
            try
            {
                char[] Digit = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
                char[] ob = new char[2];
                ob[0] = Digit[(ib >> 4) & 0X0F];
                ob[1] = Digit[ib & 0X0F];
                return new string(ob);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
