using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetJeuWSWeb3.Models
{
    public class JeuPhraseACompleter
    {
        public Joueur Joueur1 { get; set; }
        public Joueur Joueur2 { get; set; }
        public List<Joueur> lesJoueurs = new List<Joueur>();
        private int idGagnant, tour;
        private bool PartieFini = false;

        public Phrase phrase1 { get; set; }
        public Phrase phrase2 { get; set; }


        public bool ParticipeRondeCourante(string nom) {
            return nom.Equals(Joueur1.Nom) || nom.Equals(Joueur2.Nom);
        }
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
        public int getTour() { return tour; }
        public int GetIDGagnant() { return idGagnant; }
        public bool IsRondeFini() { return PartieFini; }
        public void FinirRonde(bool boolean) { this.PartieFini = boolean; }
        public void CompleterPhrase(string joueur,string restantPhrase) 
        {
            if (joueur == Joueur1.Nom)
            {
                this.phrase1.phraseFinale = this.phrase1.phraseInitiale + restantPhrase;
            } else if (joueur == Joueur2.Nom) 
            {
                this.phrase2.phraseFinale = this.phrase2.phraseInitiale + restantPhrase;
            }
        }
        public void InitRonde(string phrase1, string phrase2) 
        {
            this.phrase1 = new Phrase { 
                phraseInitiale = phrase1,
                pointage = 0
            };
            this.phrase2 = new Phrase
            {
                phraseInitiale = phrase2,
                pointage = 0
            };

            //TODO AJOUTER RANDOM POUR CHOISIR LES DEUX JOUEURS DE LA LISTE
            for (int i = 0; i > lesJoueurs.Count; i++) { 

            }
        }
    }
}