using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebSockets;

namespace ProjetJeuWSWeb3.Models
{
    public class ServeurPhraseACompleter
    {
        public static Dictionary<string, AspNetWebSocket> ListeConnectes = new Dictionary<string, AspNetWebSocket>();
        private static List<JeuPhraseACompleter> PartiesEnCours = new List<JeuPhraseACompleter>();
        private static Dictionary<string, List<string>> Invitations = new Dictionary<string, List<string>>();

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
        #region Invitations
        public static bool AjouterInvitation(string inviteur, string invite)
        {
            List<string> invitationsEnvoyees;
            if (Invitations.TryGetValue(inviteur, out invitationsEnvoyees))
            {
                if (invitationsEnvoyees.Contains(invite))//l'invitation est déjà là.
                {
                    return false;
                }
                invitationsEnvoyees.Add(invite);
            }
            else
            {
                invitationsEnvoyees = new List<string>
                {
                    invite
                };
                Invitations.Add(inviteur, invitationsEnvoyees);
            }
            return true;
        }
        public static bool EnleverInvitation(string inviteur, string invite)
        {
            if (Invitations.TryGetValue(inviteur, out List<string> invitationsEnvoyees))
            {
                return invitationsEnvoyees.Remove(invite);
            }
            return false;
        }
        #endregion
        #region Parties
        public static bool AjouterPartie(JeuPhraseACompleter partie, AspNetWebSocket socket)
        {
            if (PartiesEnCours.Contains(partie))
            {
                return false;
            }
            PartiesEnCours.Add(partie);
            return true;
        }

        internal static JeuPhraseACompleter GetPartieDe(string nom)
        {
            List<JeuPhraseACompleter>.Enumerator it = PartiesEnCours.GetEnumerator();
            while (it.MoveNext())
            {
                if (it.Current.ParticipeRondeCourante(nom))
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
        #endregion
        #region Jeu
        #endregion
    }
}