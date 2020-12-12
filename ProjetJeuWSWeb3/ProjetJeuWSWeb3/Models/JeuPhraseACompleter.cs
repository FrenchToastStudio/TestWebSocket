using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace ProjetJeuWSWeb3.Models
{
    public class JeuPhraseACompleter
    {
        public string host { get; set; }

        public string phraseACompleter { get; set; }

        //liste des joeueurs
        public List<Joueur> lesJoueurs = new List<Joueur>();
        public List<Joueur> lesSpectateur = new List<Joueur>();

        //Liste des reponse des joeueur
        public Dictionary<string, string> listeRéponses = new Dictionary<string, string>();
        public Dictionary<string, string> listeVotes = new Dictionary<string, string>();

        private int tour=0;
        private bool PartieDébuter = false;

        //régle du jeu
        private int nbTourMax = 10;
        private int pointParVote = 100;
        private int nbJoueurMax = 10;


        public bool EstDansPartie(string nom) 
        {
            for (int i =0;i<lesJoueurs.Count;i++) 
            {
                if (lesJoueurs[i].Nom.Equals(nom)) 
                {
                    return true;
                }
            }
            return false;
        }

        public bool VérifierPartieFini()
        {
            return nbTourMax == tour;
        }

        public bool VréifierPartieDébuter() 
        {
            return PartieDébuter;
        }

        public void EnvoyerRéponse(string joueur, string restantPhrase)
        {
            if (!listeRéponses.ContainsKey(joueur)) {
                Réponse réponse = new Réponse();
                listeRéponses.Add(joueur, restantPhrase);
            }
        }

        public Dictionary<string, int> ObtenirLeaderBoard()
        {
            Dictionary<string, int> leaderboard= new Dictionary<string, int>();
            for (int i = 0; i < lesJoueurs.Count - 1; i++) {
                for (int j = 0; j > lesJoueurs.Count - 1; j++) 
                {
                    if (lesJoueurs[j].pointage > lesJoueurs[j - 1].pointage) {
                        Joueur temporaire = lesJoueurs[j - 1];
                       lesJoueurs[j - 1] = lesJoueurs[j];
                        lesJoueurs[j] = temporaire;
                    }
                }
            }
            return leaderboard;
        }

        public void changerPointJoueur(int id,int point) 
        {
            lesJoueurs[id].pointage = point;
        }

        public List<Joueur> formatterLeaderBoard() 
        {
            return lesJoueurs;
        }

        internal bool vérifierSijoueurMaxAtteint()
        {
            return this.lesJoueurs.Count >= this.nbJoueurMax;
        }

        public void ProchainTour()
        {
            formatterLeaderBoard();
            PartieDébuter = true;
            phraseACompleter = PhrasesProvider.getRandomPhrase();
            listeRéponses = new Dictionary<string, string>();
            listeVotes = new Dictionary<string, string>();
            tour += 1;

        }

        public bool VérifierToutePhrasesJoueur()
        {
            return listeRéponses.Count == lesJoueurs.Count;
        }

        public void AjouterJoueur(string v)
        {
            Joueur unJoueur = new Joueur();
            unJoueur.Nom = v;
            if (lesJoueurs.Count >= nbJoueurMax && !PartieDébuter)
            {
                unJoueur.estAudience = true;
                lesSpectateur.Add(unJoueur);
            }
            else
            {
                unJoueur.estAudience = false;
                lesJoueurs.Add(unJoueur);
            }
        }

        public bool VérifierToutVote()
        {
            int nbVotes = lesJoueurs.Count + lesSpectateur.Count;
            return listeVotes.Count == nbVotes;
        }

        public void voter(string alias, string idJoeurVoterPour)
        {
            foreach (Joueur joueur in lesJoueurs) {
                if (joueur.Nom == idJoeurVoterPour)
                    joueur.pointage += pointParVote * tour;
            }

            listeVotes.Add(alias, idJoeurVoterPour);
        }

        internal Dictionary<string, int> ObtenirVote()
        {

            Dictionary<string, int> votes = new Dictionary<string, int>();

            foreach (KeyValuePair<string, string> phrase in listeVotes)
            {
                if (!votes.ContainsKey(phrase.Value))
                {
                    votes.Add(phrase.Value, 1);
                }
                else {
                    votes[phrase.Value] += 1;
                }
            }


            foreach (string réponse in listeRéponses.Values)
            {
                if (!votes.ContainsKey(réponse))
                {
                    votes.Add(réponse, 0);
                }
            }
             
            return classéDictionnaireVote(votes);
            
        }

        private Dictionary<string, int> classéDictionnaireVote(Dictionary<string, int> votes)
        {
            Dictionary<string, int> voteClassé = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> vote in votes.OrderBy(key => key.Value)) {
                voteClassé.Add(vote.Key, vote.Value);
            }

            return voteClassé;
        }
    }
}