using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace ProjetJeuWSWeb3.Models
{
    public class JeuPhraseACompleter
    {
        public string Host { get; set; }

        public string PhraseACompleter { get; set; }

        //liste des joeueurs
        public List<Joueur> lesJoueurs = new List<Joueur>();
        public List<Joueur> lesSpectateur = new List<Joueur>();

        //Liste des reponse des joeueur
        public Dictionary<string, string> listeRéponses = new Dictionary<string, string>();
        public Dictionary<string, string> listeVotes = new Dictionary<string, string>();

        private int tour=0;
        private bool PartieDébuter = false;

        //régle du jeu
        private int nbTourMax = 3;
        private int pointParVote = 100;
        private int nbJoueurMax = 10;
        public int NbJoueurMinimum = 3;

        /**
         * permet de véfieir si le joueur est dans la partie
         * return bool
         */
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

        /**
         * permet de vérifier si la partie est fini
         * Return bool
         */
        public bool VérifierPartieFini()
        {
            return nbTourMax < tour;
        }

        /**
         * permet de vérifier si la aprtie est débuter
         * return bool
         */
        public bool VréifierPartieDébuter() 
        {
            return PartieDébuter;
        }
        /**
         * permet a un joueur de completer un phrase
         */
        public void EnvoyerRéponse(string joueur, string restantPhrase)
        {
            if (!listeRéponses.ContainsKey(joueur)) {
                Réponse réponse = new Réponse();
                listeRéponses.Add(joueur, restantPhrase);
            }
        }

        /**
         * Retourne une liste de joueur classé par le plus haut pointage
         */
        public List<Joueur> ObtenirLeaderBoard()
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
            return lesJoueurs;
        }

        /**
         * ajoute le pointage d'un joueur
         */
        public void changerPointJoueur(int id,int point) 
        {
            lesJoueurs[id].pointage = point;
        }

        /**
         * Vérifie si tout les joueur sont dans la partie
         * Return bool
         */
        internal bool vérifierSijoueurMaxAtteint()
        {
            return this.lesJoueurs.Count >= this.nbJoueurMax;
        }

        /**
         * passe au prochain tour
         */
        public void ProchainTour()
        {
            PartieDébuter = true;
            PhraseACompleter = new PhrasesProvider().getRandomPhrase();
            listeRéponses = new Dictionary<string, string>();
            listeVotes = new Dictionary<string, string>();
            tour += 1;

        }

        /**
         * Permet de vérifier si tout les joeueur on envoyer leur phrase
         * Retur bool
         */
        public bool VérifierToutePhrasesJoueur()
        {
            return listeRéponses.Count == lesJoueurs.Count;
        }

        /**
         * Premet d'ajouter un joueur a la liste de joueur
         */
        public void AjouterJoueur(string v)
        {
            Joueur unJoueur = new Joueur();
            unJoueur.Nom = v;
            lesJoueurs.Add(unJoueur);
        }

        public void AjouterAudience(string v) {
            Joueur unJoueur = new Joueur();
            unJoueur.Nom = v;
            lesSpectateur.Add(unJoueur);
        }

        /**
         * Permet de vérifier si tout le monde a voter
         */
        public bool VérifierToutVote()
        {
            int nbVotes = lesJoueurs.Count + lesSpectateur.Count;
            return listeVotes.Count == nbVotes;
        }

        /**
         * Permet a un utilisateur de voter pour la phrase d'un autre utilsiateur
         */
        public void voter(string alias, string idJoeurVoterPour)
        {
            foreach (Joueur joueur in lesJoueurs) {
                if (joueur.Nom == idJoeurVoterPour)
                    joueur.pointage += pointParVote * tour;
            }

            listeVotes.Add(alias, idJoeurVoterPour);
        }

        /**
         * Récupère les votes des utilisateur et les compile
         * return Dictionary<string, int> 
         */
        internal Dictionary<string, int> ObtenirVote()
        {

            Dictionary<string, int> votes = new Dictionary<string, int>();

            foreach (KeyValuePair<string, string> vote in listeVotes)
            {

                if (!votes.ContainsKey(listeRéponses[vote.Value]))
                {
                    votes.Add(listeRéponses[vote.Value], 1);
                }
                else {
                    votes[listeRéponses[vote.Value]] += 1;
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

        /**
         * Classe les les phrase par les phrase voté le plus
         * Return Dictionary<string, int>
         */
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