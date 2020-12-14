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
        private string idPartieActuel;

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

            IEnumerator<string> it = ServeurChat.ListeConnectes.Keys.GetEnumerator();
            while (it.MoveNext())
            {
                await ServeurChat.ListeConnectes[it.Current].SendAsync(envoi, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        private async Task EnvoyerATousDeLaPartDe(Object objet, AspNetWebSocket socket)
        {
            string message = JsonConvert.SerializeObject(objet);

            byte[] buffer2 = Encoding.UTF8.GetBytes(message);
            ArraySegment<byte> envoi = new ArraySegment<byte>(buffer2);

            IEnumerator<string> it = ServeurChat.ListeConnectes.Keys.GetEnumerator();
            while (it.MoveNext())
            {
                if (ServeurChat.ListeConnectes[it.Current] != socket)
                {
                    await ServeurChat.ListeConnectes[it.Current].SendAsync(envoi, WebSocketMessageType.Text, true, CancellationToken.None);
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
            if (ServeurChat.ListeConnectes.TryGetValue(nom, out AspNetWebSocket socket))
            {
                await EnvoyerA(objet, socket);
            }
        }
        private async Task EnvoyerLobby(Object objet, string idPartie) {
            if (ServeurLobbyJeu.ListeLobby.TryGetValue(idPartie, out AspNetWebSocket socket))
            {
                await EnvoyerA(objet, socket);
            }
        }
        public async Task Receiver(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket as AspNetWebSocket;
            JeuPhraseACompleter partieEnCours;
            ServeurChat.AjouterConnecte(this.Alias, socket);
            await EnvoyerA(new Message { Categorie = "CONNECTE", Type = "LIST", Data = ServeurChat.GetNomsConnectes() }, socket);
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
                    /**
                     * Se connecte au web socket de chat 
                     */
                    case "CONNECTE":
                        switch (message.Type)
                        {
                            /**
                             * Lorsque qu'un joueur se déconnecte du WS de chat
                             */
                            case "QUIT":
                                await EnvoyerATousDeLaPartDe(new Message { Categorie = "CONNECTE", Type = "DEL", Data = this.Alias }, socket);
                                break;
                                /**
                                 * Lorsque le joueur se connecte à un Lobby de jeu
                                 */
                            case "PARTIE":
                                idPartieActuel = message.Data.ToString();
                                await EnvoyerLobby(new Message { Categorie = "CONNECTER", Type = "ADD", Data = this.Alias }, message.Data.ToString());
                                break;
                        }
                        break;
                        /**
                         * La réception d'un message de chat
                         * Tout le monde peut voir les messages avec temps 
                         */
                    case "MESSAGE":
                        this.messageChat.dateMessage = DateTime.Now;
                        this.messageChat.Contenu = message.Data.ToString();
                        string messageString = messageChat.toString();
                        await EnvoyerATous(new Message {Categorie = "MESSAGE",Type="CHAT",Data= messageString });
                        break;
                        /**
                         * Le déroulement d'un jeu et les cas selon l'état 
                         */
                    case "JEU":
                        switch (message.Type)
                        {
                            /**
                             * Lorsque le joueur envoie un vote pour une phrase
                             */
                            case "VOTE":
                                partieEnCours = ServeurLobbyJeu.GetPartieDe(this.idPartieActuel);
                                if (partieEnCours != null)
                                {
                                    partieEnCours.voter(this.Alias, message.Data.ToString());
                                }
                                await EnvoyerLobby(new Message { Categorie = "JEU", Type = "VOTER", Data = this.Alias }, idPartieActuel);
                                break;
                                /**
                                 * Lorsque le joueur reçois un message de démarrer la partie
                                 */
                            case "DEMARRER":
                                await EnvoyerLobby(new Message { Categorie = "JEU", Type = "DEBUTER", Data = this.Alias }, idPartieActuel);
                                break;
                                /**
                                 * Lorsque le joueur à fini sa phrase de jeu
                                 */
                            case "GAME":
                                JeuPhraseACompleter partie = ServeurLobbyJeu.GetPartieDe(this.idPartieActuel);
                                
                                if (partie != null)
                                {
                                    partie.EnvoyerRéponse(this.Alias, message.Data.ToString());
                                    await EnvoyerLobby(new Message { Categorie = "JEU", Type = "PHRASECOMPLETER", Data = message.Data.ToString() }, this.idPartieActuel);
                                }
                                break;
                                /**
                                 * Lorsque la partie fini
                                 */
                            case "FIN":
                                partieEnCours = ServeurLobbyJeu.GetPartieDe(this.idPartieActuel);

                                if (partieEnCours != null)
                                {
                                    //lapartie.EnvoyerRéponse(this.Alias, message.Data.ToString());
                                    await EnvoyerLobby(new Message { Categorie = "JEU", Type = "END", Data = partieEnCours.ObtenirLeaderBoard() }, this.idPartieActuel);
                                }
                                break;
                        }
                        break;
                }
            }
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            await EnvoyerATousDeLaPartDe(new Message { Categorie = "CONNECTE", Type = "DEL", Data = this.Alias }, socket);
            ServeurChat.ListeConnectes.Remove(this.Alias);
        }
    }
}