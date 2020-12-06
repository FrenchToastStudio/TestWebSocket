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
    public class GestionnaireWebSockets : WebSocketHandler
    {
        private Message message = new Message();
        private MessageChat messageChat = new MessageChat();
        private readonly string Alias;

        public GestionnaireWebSockets(string username) : base(2048)
        {
            this.Alias = username;
            this.messageChat.Expediteur = username;
        }

        private async Task EnvoyerATous(Object objet)
        {
            string message = JsonConvert.SerializeObject(objet);

            byte[] buffer2 = Encoding.UTF8.GetBytes(message);
            ArraySegment<byte> envoi = new ArraySegment<byte>(buffer2);

            IEnumerator<string> it = ServeurPhraseACompleter.ListeConnectes.Keys.GetEnumerator();
            while (it.MoveNext())
            {
                await ServeurPhraseACompleter.ListeConnectes[it.Current].SendAsync(envoi, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        private async Task EnvoyerATousDeLaPartDe(Object objet, AspNetWebSocket socket)
        {
            string message = JsonConvert.SerializeObject(objet);

            byte[] buffer2 = Encoding.UTF8.GetBytes(message);
            ArraySegment<byte> envoi = new ArraySegment<byte>(buffer2);

            IEnumerator<string> it = ServeurPhraseACompleter.ListeConnectes.Keys.GetEnumerator();
            while (it.MoveNext())
            {
                if (ServeurPhraseACompleter.ListeConnectes[it.Current] != socket)
                {
                    await ServeurPhraseACompleter.ListeConnectes[it.Current].SendAsync(envoi, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
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
            if (ServeurPhraseACompleter.ListeConnectes.TryGetValue(nom, out AspNetWebSocket socket))
            {
                await EnvoyerA(objet, socket);
            }
        }
        public async Task Receiver(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket as AspNetWebSocket;
            ServeurPhraseACompleter.AjouterConnecte(this.Alias, socket);
            await EnvoyerA(new Message { Categorie = "CONNECTE", Type = "LIST", Data = ServeurPhraseACompleter.GetNomsConnectes() }, socket);
            await EnvoyerATousDeLaPartDe(new Message { Categorie = "CONNECTE", Type = "ADD", Data = this.Alias }, socket);

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
                    case "CONNECTE":
                        switch (message.Type)
                        {
                            case "QUIT":
                                await EnvoyerATousDeLaPartDe(new Message { Categorie = "CONNECTE", Type = "DEL", Data = this.Alias }, socket);
                                //TODO:
                                //  - Supprimer les parties et les invitations de this.Alias.
                                break;
                        }
                        break;
                    case "INVITATION":
                        switch (message.Type)
                        {
                            case "INVITE":
                                ServeurPhraseACompleter.AjouterInvitation(this.Alias, message.Data.ToString());
                                await EnvoyerA(new Message { Categorie = "INVITATION", Type = "RECEIVED", Data = this.Alias }, message.Data.ToString());
                                break;
                            case "ACCEPT":
                                string joueur1 = this.Alias,
                                       joueur2 = message.Data.ToString();
                                JeuPhraseACompleter partie = new JeuPhraseACompleter
                                {
                                    Joueur1 = new Joueur { Nom = joueur1, pointage = 0 },
                                    Joueur2 = new Joueur { Nom = joueur2, pointage = 0 }
                                };
                                List<string> listePhrases = PhrasesProvider.getListe();
                                partie.InitRonde(listePhrases[0],listePhrases[1]);
                                ServeurPhraseACompleter.AjouterPartie(partie);
                                await EnvoyerA(new Message { Categorie = "JEU", Type = "START", Data = partie }, joueur1);
                                await EnvoyerA(new Message { Categorie = "JEU", Type = "START", Data = partie }, joueur2);
                                break;
                            case "REFUSE":
                                //TODO
                                break;
                            case "CANCEL":
                                //TODO
                                break;
                        }
                        break;
                    case "MESSAGE":
                        this.messageChat.dateMessage = DateTime.Now;
                        this.messageChat.Contenu = message.Data.ToString();
                        string messageString = messageChat.toString();
                        await EnvoyerATous(new Message {Categorie = "MESSAGE",Type="CHAT",Data= messageString });
                        break;
                    case "JEU":
                        switch (message.Type)
                        {
                            case "SUBMIT":
                                JeuPhraseACompleter partieEnCours = ServeurPhraseACompleter.GetPartieDe(this.Alias);
                                if (partieEnCours != null)
                                {
                                    partieEnCours.CompleterPhrase(this.Alias, message.Data.ToString());
                                }
                                else
                                {
                                    string autreJoueur = (partieEnCours.Joueur1.Nom == this.Alias) ? partieEnCours.Joueur2.Nom : partieEnCours.Joueur1.Nom;
                                }
                                break;
                            case "ABANDON":
                                //TODO
                                break;
                            case "GAME":
                                JeuPhraseACompleter partie = ServeurPhraseACompleter.GetPartieDe(this.Alias);
                                if (partie != null)
                                {
                                    string phrasePartie = message.Data.ToString();
                                    partie.CompleterPhrase(this.Alias, phrasePartie);
                                    if (partie.IsRondeFini())
                                    {
                                        int gagnant = partie.GetIDGagnant();
                                        string nomGagnant = "";
                                        if (gagnant == 1)
                                        {
                                            nomGagnant = partie.Joueur1.Nom;
                                        }
                                        else if (gagnant == 2)
                                        {
                                            nomGagnant = partie.Joueur2.Nom;
                                        }
                                        await EnvoyerA(new Message { Categorie = "JEU", Type = "END", Data = nomGagnant }, partie.Joueur1.Nom);
                                        await EnvoyerA(new Message { Categorie = "JEU", Type = "END", Data = nomGagnant }, partie.Joueur2.Nom);
                                        ServeurPhraseACompleter.EnleverPartie(partie);
                                    }
                                    else
                                    {
                                        string autreJoueur = (partie.Joueur1.Nom == this.Alias) ? partie.Joueur2.Nom : partie.Joueur1.Nom;
                                        await EnvoyerA(new Message { Categorie = "JEU", Type = "GAME", Data = partie }, autreJoueur);
                                        await EnvoyerA(new Message { Categorie = "JEU", Type = "GAME", Data = partie }, this.Alias);
                                    }
                                }
                                break;
                        }
                        break;
                }
            }
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            await EnvoyerATousDeLaPartDe(new Message { Categorie = "CONNECTE", Type = "DEL", Data = this.Alias }, socket);
            ServeurPhraseACompleter.ListeConnectes.Remove(this.Alias);
        }
    }
}