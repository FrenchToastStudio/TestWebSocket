using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetJeuWSWeb3.Models
{
    public class MessageChat
    {
        public string Expediteur { get; set; }
        public string Contenu { get; set; }
        public DateTime dateMessage { get; set; }

        public string toString() {
            string msg = dateMessage.ToString("hh:mm tt") + " " + Expediteur + " : " + Contenu;
            return msg;
        }
    }
}