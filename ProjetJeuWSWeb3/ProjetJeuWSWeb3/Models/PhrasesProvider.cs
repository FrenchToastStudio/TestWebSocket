using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetJeuWSWeb3.Models
{
    public class PhrasesProvider
    {
        private static List<string> listePhrase = new List<string>
        { "The reason people hate ", 
            "What is the deal with ", 
            "This is your captain speaking. Fasten your seatbelts and prepare for",
            "What did I bring back from Mexico?",
            "When I am a billionaire, I shall erect a 50-foot statue to commemorate",
            "Before I run for president, I must destroy all evidence of my involvement with",
            "Getting really high.",
            "Riding off into the sunset.",
            "A moment of silence.",
            "A crazy little thing called love."
        };

        public static List<string> getListe() {
            return listePhrase;
        }

        public static string getRandomPhrase() {
            Random rnd = new Random();
            int index = rnd.Next(listePhrase.Count);
            return listePhrase[index];
        }
    }
}