using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RFIDwpf.RFID
{
    public class LecteurRfid
    {

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
     
        #region byteHEX
        /// <summary>
        /// Transforme un octet en chaine contenant le caractère ASCII
        /// </summary>
        /// <param name="ib">valeur de l'octet</param>
        /// <returns>Chaine caractère</returns>
        ///

        public static String byteHEX(Byte ib)
        {
            String _str = String.Empty;
            try
            {
                char[] Digit = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A',
                'B', 'C', 'D', 'E', 'F' };
                char[] ob = new char[2];
                ob[0] = Digit[(ib >> 4) & 0X0F];
                ob[1] = Digit[ib & 0X0F];
                _str = new String(ob);
            }
            catch (Exception)
            {
                new Exception("对不起有错。");
            }
            return _str;

        }
        #endregion
        public bool bConnectedDevice;                      // permet de savoir si un lecteur est connecté
        protected int port;
        public int Port                                    // numéro du port série du lecteur
        {
            get { return port; }
            set { if (value > 0) port = value; }
        }

        protected int baud;
        public int Baud                                    // vitesse liaison série
        {
            get { return baud; }
            set { if (value > 0) baud = value; }
        }

        protected String identifiant;
        public string Identifiant
        {
            get { return identifiant; }
            set { identifiant = value; }
        }

        public LecteurRfid()
        {
       
            bConnectedDevice = false;
            baud = 19200;
            this.port = Int32.Parse("5"); // port RFID connection 
            /*this.port = 9;
            baud = 19200;
            this.carteLu = carteLu;*/
        }

        /* protected Carte carteLu;
         public Carte CarteLu                            // carte en liaison avec le lecteur
         {
             get { return carteLu; }
             set { carteLu = value; }
         }

         public LecteurRfid(Carte carteLu)
         {
             bConnectedDevice = false;
             this.port = 9;
             baud = 19200;
             this.carteLu = carteLu;
         }*/
        /*  public LecteurRfid(int port, int baud, Carte carteLu)
          {
              this.port = port;
              this.baud = baud;
              this.carteLu = carteLu;
              // connectionRs();
          }*/
        public int connectionRs()
        {
            int status;
            status = rf_init_com(port, baud);
            if (status == 0) bConnectedDevice = true;
            else bConnectedDevice = false;
            return status;
        }
        public int fermetureRs()
        {
            int status = -1; ;
            if (bConnectedDevice)
            {
                status = rf_ClosePort();
                if (status == 0) bConnectedDevice = false;
            }
            return status;
        }
        public int lireIdentifiantCarte()
        {
            int testlecture = 0;          // on essaye deux fois en cas de problème
            short icdev = 0x0000;       // Descripteur du lecteur
            int status = -1;
            byte type = (byte)'A';      //type mifare
            byte mode = 0x52;
            ushort TagType = 0;
            byte bcnt = 0x04;           //mifare 卡都用4
            IntPtr pSnr;
            byte len = 255;


            if (!bConnectedDevice)
            {
                status = connectionRs();                 // on essaye au cas où on est passé par le constructeur par défaut
            }

            if (bConnectedDevice)               // Là forcément la liaison série ne fonctionne pas
            {
                pSnr = Marshal.AllocHGlobal(1024);

                do
                {
                    status = rf_antenna_sta(icdev, 0);//on coupe l'antenne
                    if (status == 0)
                    {
                        Sleep(20);
                        status = rf_init_type(icdev, type);// on sélectionne le type de carte Mifare1k
                        if (status == 0)
                        {
                            Sleep(20);
                            status = rf_antenna_sta(icdev, 1);//On remet l'antenne
                            if (status == 0)
                            {
                                Sleep(50);
                                status = rf_request(icdev, mode, ref TagType);//On interroge pour voir s'il y a une carte
                                if (status == 0)
                                {
                                    status = rf_anticoll(icdev, bcnt, pSnr, ref len);//On valide une carte dont l'identifiant est recupéré avec pSnr                                       
                                    if (status == 0)
                                    {
                                        byte[] szBytes = new byte[len];
                                        for (int j = 0; j < len; j++) szBytes[j] = Marshal.ReadByte(pSnr, j);
                                        String m_cardNo = String.Empty;
                                        for (int q = 0; q < len; q++) m_cardNo += byteHEX(szBytes[q]);
                                        // carteLu.Identifiant = m_cardNo;
                                        Identifiant = m_cardNo;
                                        testlecture = 2;
                                    }
                                    else testlecture++;
                                }
                            }
                        }
                    }
                } while (testlecture < 2 && status == 0);

                Marshal.FreeHGlobal(pSnr);
            }
            return status;
        }
        // GET RFID ou error message
        public string GetCardID()
        {
            int status = lireIdentifiantCarte();
            string statusMsg = null;

            switch (status)
            {
                case 0:
                    statusMsg = Identifiant; // Retourne l'identifiant de la carte
                    break;

                case 1:
                    // Commenté pour éviter l'affichage d'erreurs
                    // MessageBox.Show("Erreur de vitesse du Port Série", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case 2:
                    // Commenté pour éviter l'affichage d'erreurs
                    // MessageBox.Show("Erreur de numéro du Port Série", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case 20:
                    // Commenté pour éviter l'affichage d'erreurs
                    // MessageBox.Show("Carte absente", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

            return statusMsg;
        }
    }
}
