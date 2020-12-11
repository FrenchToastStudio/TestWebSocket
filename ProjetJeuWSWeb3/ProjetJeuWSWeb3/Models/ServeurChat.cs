using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebSockets;

namespace ProjetJeuWSWeb3.Models
{
    public class ServeurChat
    {
        public static Dictionary<string, AspNetWebSocket> ListeConnectes = new Dictionary<string, AspNetWebSocket>();

        #region Connectés
        public static void AjouterConnecte(string nomConnecte, AspNetWebSocket socketConnecte)
        {
            ListeConnectes.Add(nomConnecte, socketConnecte);
        }
        public static void EnleverConnecte(string nomConnecte)
        {
            ListeConnectes.Remove(nomConnecte);
        }
        public static List<string> GetNomsConnectes()
        {
            return ListeConnectes.Keys.ToList();
        }
        #endregion
    }
}