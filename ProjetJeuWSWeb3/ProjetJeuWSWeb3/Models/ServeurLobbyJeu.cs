using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebSockets;

namespace ProjetJeuWSWeb3.Models
{
    public class ServeurLobbyJeu
    {
        public static Dictionary<string, AspNetWebSocket> ListeLobby= new Dictionary<string, AspNetWebSocket>();
        private static List<JeuPhraseACompleter> PartiesEnCours = new List<JeuPhraseACompleter>();

        #region Lobby
        public static string AjouterLobby(AspNetWebSocket socketConnectee)
        {

            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var rand = new Random();
            bool nomUnique = true;
            string idPartie = "";
            while (nomUnique)
            {

                idPartie = "";
                for (var i = 0; i < 4; i++)
                {
                    idPartie += alphabet[rand.Next(0, alphabet.Length)];
                }

                if (!ListeLobby.ContainsKey(idPartie))
                {
                    nomUnique = false;
                }
            }
            ListeLobby.Add(idPartie, socketConnectee);
            return idPartie;
        }

        public static bool AjouterLobby(string nom, AspNetWebSocket socketConnectee)
        {
            ListeLobby.Remove(nom);
            return true;
        }

        internal static AspNetWebSocket ObtenirLobby(string host)
        {
            Dictionary<string, AspNetWebSocket>.Enumerator it = ListeLobby.GetEnumerator();
            while (it.MoveNext())
            {
                if (it.Current.Key == host)
                {
                    return it.Current.Value;
                }
            }
            return null;
        }
        #endregion

        #region Parties
        public static bool AjouterPartie(JeuPhraseACompleter partie)
        {
            PartiesEnCours.Add(partie);
            return true;
        }

        internal static JeuPhraseACompleter GetPartieDe(string host)
        {
            List<JeuPhraseACompleter>.Enumerator it = PartiesEnCours.GetEnumerator();
            while (it.MoveNext())
            {
                if (it.Current.Host == host)
                {
                    return it.Current;
                }
            }
            return null;
        }

        internal static void EnleverPartie(JeuPhraseACompleter partie)
        {
            PartiesEnCours.Remove(partie);
        }

        internal static void CompleterPhraseJoueur()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}