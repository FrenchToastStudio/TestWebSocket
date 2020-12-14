using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;

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

        public string getRandomPhrase() {
            Random rnd = new Random();
            int index = compterNbPhrases();
            string phrase = obtenirPhrase(rnd.Next(1, index));
            return phrase;
        }

        

        private string obtenirPhrase(int idPhrase)
        {
            string phrase = "";

            MySqlConnection connexion = new MySqlConnection();
            connexion.ConnectionString = "Server=127.0.0.1;Uid=journalisteBDArticle;Pwd=motDePasse;Database=phrases;";

            connexion.Open();
            

            MySqlCommand commande = connexion.CreateCommand();
            commande.Connection = connexion;
            commande.CommandText = "SELECT phrase FROM listePhrases WHERE id = @idPhrase;";
            commande.Parameters.AddWithValue("idPhrase", idPhrase);

            commande.CommandType = CommandType.Text;
           
            MySqlDataReader lecteur = commande.ExecuteReader();
            
            while (lecteur.Read())
            {
                phrase = lecteur.GetString(0);
            }

            connexion.Close();
            return phrase;
        }

        private int compterNbPhrases()
        {
            int id = -1;
            
            MySqlConnection connexion = new MySqlConnection();
            connexion.ConnectionString = "Server=127.0.0.1;Uid=journalisteBDArticle;Pwd=motDePasse;Database=phrases;";

            connexion.Open();


            MySqlCommand commande = connexion.CreateCommand();
            commande.Connection = connexion;
            commande.CommandText = "SELECT COUNT(id) AS number FROM listePhrases;";

            commande.CommandType = CommandType.Text;
            
            MySqlDataReader lecteur = commande.ExecuteReader();
            
            while (lecteur.Read())
            {
                id = lecteur.GetInt32(0);
            }

            connexion.Close();
            return id;
        }
    }
}