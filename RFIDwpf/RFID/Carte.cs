using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFIDwpf.RFID
{
    public class Carte
    {

        protected String identifiant;

        public string Identifiant
        {
            get { return identifiant; }
            set { identifiant = value; }
        }
        public Carte(string Identifiant)
        {
            this.identifiant = Identifiant;
        }
    }
}
