using Microsoft.AspNet.SignalR.WebSockets;
using Newtonsoft.Json;
using ProjetJeuWSWeb3.Models;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebSockets;
using System.Diagnostics;

namespace ProjetJeuWSWeb3.WS
{
    public class GestionnaireWebSocketsLobby : WebSocketHandler
    {
        private Message message = new Message();
        string idPartie;
        string aliasHotePartie;
        JeuPhraseACompleter jeu;

        public GestionnaireWebSocketsLobby() : base(2048)
        {

        }

        private async Task EnvoyerA(Object objet, AspNetWebSocket socket)
        {
            string message = JsonConvert.SerializeObject(objet);

            byte[] buffer2 = Encoding.UTF8.GetBytes(message);
            ArraySegment<byte> envoi = new ArraySegment<byte>(buffer2);
            await socket.SendAsync(envoi, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task EnvoyerA(Object objet, string nom)
        {
            if (ServeurChat.ListeConnectes.TryGetValue(nom, out AspNetWebSocket socket))
            {
                await EnvoyerA(objet, socket);
            }
        }

        public async Task Receiver(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket as AspNetWebSocket;

            idPartie = ServeurLobbyJeu.AjouterLobby(socket);
            JeuPhraseACompleter partie = new JeuPhraseACompleter();
            partie.host = idPartie;
            ServeurLobbyJeu.AjouterPartie(partie);
            await EnvoyerA(new Message { Categorie = "LOBBY", Type = "DEBUTER", Data = idPartie }, socket);

            ArraySegment<byte> bufferDeReception = new ArraySegment<byte>(new byte[2048]);
            while (socket.State == WebSocketState.Open)
            {
                var resultat = await socket.ReceiveAsync(bufferDeReception, CancellationToken.None);
                if (socket.State != WebSocketState.Open)
                {
                    break;
                }

                string str = Encoding.UTF8.GetString(bufferDeReception.Array, 0, resultat.Count);
                message = JsonConvert.DeserializeObject<Message>(str);
                switch (message.Categorie)
                {
                    case "CONNECTER":
                        switch (message.Type)
                        {
                            case "ADDJOUEUR":
                                jeu = ServeurLobbyJeu.GetPartieDe(this.idPartie);

                                if (jeu != null)
                                {
                                    jeu.AjouterJoueur(message.Data.ToString());
                                    if (jeu.vérifierSijoueurMaxAtteint() && jeu.VréifierPartieDébuter())
                                    {
                                        await EnvoyerA(new Message { Categorie = "CONNECTER", Type = "NOUVEAUSPECTATEUR", Data = message.Data.ToString() }, socket);
                                    }
                                    else if (jeu.lesJoueurs.Count == 1) 
                                    {
                                        aliasHotePartie = message.Data.ToString();
                                        await EnvoyerA(new Message { Categorie = "CONNECTER", Type = "NOUVEAUJOUEUR", Data = message.Data.ToString() }, socket);
                                        await EnvoyerA(new Message { Categorie = "CONNECTE", Type = "PREMIERJOUEUR", Data = message.Data.ToString() }, message.Data.ToString());
                                    }
                                    else
                                    {
                                        await EnvoyerA(new Message { Categorie = "CONNECTER", Type = "NOUVEAUJOUEUR", Data = message.Data.ToString() }, socket);
                                    }
                                }
                                break;
                        }
                        break;
                    case "JEU":
                        switch (message.Type)
                        {
                            case "START":
                                jeu = ServeurLobbyJeu.GetPartieDe(this.idPartie);
                                if (jeu != null)
                                {
                                    jeu.ProchainTour();
                                    if (jeu.VérifierPartieFini()) {
                                        await EnvoyerA(new Message { Categorie = "JEU", Type = "PARTIETERMINER", Data = jeu.ObtenirVote() }, socket);
                                        foreach (Joueur joueur in jeu.lesJoueurs)
                                        {
                                            await EnvoyerA(new Message { Categorie = "JEU", Type = "PARTIETERMINER", Data = jeu.ObtenirVote() }, joueur.Nom);
                                        }
                                    } else {
                                        foreach (Joueur joueur in jeu.lesJoueurs)
                                        {
                                            await EnvoyerA(new Message { Categorie = "JEU", Type = "START", Data = jeu }, joueur.Nom);

                                        }
                                        foreach (Joueur foule in jeu.lesSpectateur)
                                        {
                                            await EnvoyerA(new Message { Categorie = "PUBLIC", Type = "START", Data = jeu }, foule.Nom);
                                        }
                                        await EnvoyerA(new Message { Categorie = "JEU", Type = "NEXTROUND", Data = jeu }, socket);
                                    }
                                }
                                break;
                                /**
                                 * Véérifie si tout les joueur on completer leur phrases et 
                                 */
                            case "PHRASECOMPLETER":
                                jeu = ServeurLobbyJeu.GetPartieDe(this.idPartie);
                                if (jeu != null)
                                {
                                    if (jeu.VérifierToutePhrasesJoueur())
                                    {
                                        await EnvoyerA(new Message { Categorie = "JEU", Type = "PHASEVOTE", Data = jeu.listeRéponses }, socket);
                                        foreach (Joueur joueur in jeu.lesJoueurs)
                                        {
                                            await EnvoyerA(new Message { Categorie = "JEU", Type = "PHASEVOTE", Data = jeu.listeRéponses }, joueur.Nom);
                                        }
                                        foreach(Joueur foule in jeu.lesSpectateur)
                                        {
                                            await EnvoyerA(new Message { Categorie = "JEU", Type = "PHASEVOTE", Data = jeu.listeRéponses }, foule.Nom);
                                        }
                                    }
                                }
                                break;
                                /**
                                 * Vérifie a chaque fois qu'un joueur complete son vote  
                                 */
                            case "VOTECOMPLETER":
                                jeu = ServeurLobbyJeu.GetPartieDe(this.idPartie);
                                if (jeu != null)
                                {
                                    
                                    if (jeu.VérifierToutVote())
                                    {
                                        await EnvoyerA(new Message { Categorie = "JEU", Type = "AFFICHERVOTE", Data = jeu.ObtenirVote() }, socket);
                                        foreach (Joueur joueur in jeu.lesJoueurs)
                                        {
                                            await EnvoyerA(new Message { Categorie = "JEU", Type = "AFFICHERVOTE", Data = jeu.ObtenirVote() }, joueur.Nom);
                                        }
                                    }
                                }
                                break;
                                /**
                                 * Dans le cas ou les joueur manque de temps pour finir leur phrase. complete leur prhrase et lance la fin de tour
                                 */
                            case "TIMERUNOUT":
                                 jeu = ServeurLobbyJeu.GetPartieDe(this.idPartie);
                                if (jeu != null)
                                {
                                    ServeurLobbyJeu.CompleterPhraseJoueur();
                                    await EnvoyerA(new Message { Categorie = "JEU", Type = "TOURTERMINER", Data = jeu }, socket);
                                }
                                break;
                                /**
                                 *  Lance la fin du tour et envoie les leaderboard
                                 */
                            case "TOURTERMINER":
                                jeu = ServeurLobbyJeu.GetPartieDe(this.idPartie);
                                if (jeu != null)
                                {
                                    await EnvoyerA(new Message { Categorie = "JEU", Type = "LEADERBOARD", Data = jeu.ObtenirLeaderBoard() }, socket);
                                }
                                break;
                                /**
                                 * Termine la partie dans le cas ou elle est teminer
                                 */
                            case "JEUXTERMINER":
                                jeu = ServeurLobbyJeu.GetPartieDe(this.idPartie);
                                if (jeu != null) {
                                    await EnvoyerA(new Message { Categorie = "JEU", Type = "JEUXTERMINER", Data = jeu.ObtenirLeaderBoard() }, socket);
                                    foreach (Joueur joueur in jeu.lesJoueurs)
                                    {
                                        await EnvoyerA(new Message { Categorie = "JEU", Type = "JEUXTERMINER", Data = jeu.ObtenirLeaderBoard() }, joueur.Nom);
                                    }
                                }
                                    break;


                        }
                        break;
                    
                }
            }

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

        }
    }
}