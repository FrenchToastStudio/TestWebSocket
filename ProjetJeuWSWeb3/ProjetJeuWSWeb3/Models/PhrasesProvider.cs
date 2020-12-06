using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetJeuWSWeb3.Models
{
    public class PhrasesProvider
    {
        private static List<string> listePhrase = new List<string>{ "The reason people hate ", "What is the deal with " };

        public static List<string> getListe() {
            return listePhrase;
        }
    }
}