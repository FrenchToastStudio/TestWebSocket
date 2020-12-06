using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetJeuWSWeb3.Models
{
    public class Message
    {
        public string Categorie { get; set; }
        public string Type { get; set; }
        public object Data { get; set; }
    }
}