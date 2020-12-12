using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetJeuWSWeb3.Models
{
    public class Joueur
    {
        public string Nom { get; set; }
        public bool estAudience { get; set; } = false;
        public int pointage { get; set; } = 0;

    }
}