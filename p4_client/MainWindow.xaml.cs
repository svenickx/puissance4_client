﻿using p4_client.Model;
using p4_client.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace p4_client
{
    public partial class MainWindow : Window
    {
        private readonly Socket _ClientSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public string player_uid = "";
        public Game? game;
        public CustomGrid? grid;
        public bool isPlayer1 = false;
        public bool isPlayingAgainstBot = false;

        public Thread? listening_thread;
        public readonly int delayFallPieces = 100;
        public readonly int port = 10000;

        public MainWindow()
        {
            InitializeComponent();
        }
        public void LoopConnect()
        {
            int attempts = 0;
            while (!_ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _ClientSocket.Connect(IPAddress.Loopback, this.port);
                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connection attempts: " + attempts.ToString());
                }
            }
            Console.Clear();
            Console.WriteLine("Connected");
        }

        /// <summary>Create a game with a remote player</summary>
        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            this.grid = new CustomGrid(this);
            this.username.IsEnabled = false;
            SearchButtons.Visibility = Visibility.Collapsed;
            LoopConnect();

            listening_thread = new Thread(Receive);
            listening_thread.Start();

            this.player_uid = Guid.NewGuid().ToString();
            Send("search," + username.Text + "," + this.player_uid);
        }
        
        /// <summary>Send data to the server</summary>
        /// <param name="req">The data to send</param>
        public void Send(string req)
        {
            if (_ClientSocket.Connected)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(req);
                _ClientSocket.Send(buffer);
            }
        }
        
        /// <summary>Listen the data that comes from the server</summary>
        public void Receive()
        {
            while (_ClientSocket.Connected)
            {
                try 
                {
                    byte[] receiveBuf = new byte[1024];
                    int rec = _ClientSocket.Receive(receiveBuf);
                    byte[] data = new byte[rec];
                    Array.Copy(receiveBuf, data, rec);
                    Console.WriteLine("Received: " + Encoding.ASCII.GetString(data));
                    SelectActions(Encoding.ASCII.GetString(data));
                } 
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }   
        }

        /// <summary>Performs an action according to the server's data</summary>
        /// <param name="res">The data received from the server</param>
        private void SelectActions(string res) {
            string[] actions = res.Split(',');

            if (actions[0] == "waiting") 
            {
                Dispatcher.Invoke(() => {
                    info.Content = "Recherche d'une partie...";
                });
            } 
            else if (actions[0].Split(":")[0] == "matchFound") 
            {
                CreateGame(actions);
            } 
            else if (actions[0] == "move")
            {
                Piece.NewPieceReceived(this, Int32.Parse(actions[1]));
            }
            else if (actions[0] == "endGame")
            {
                if (actions[1] == "victory") game!.Victory();
                if (actions[1] == "draw") game!.Draw();
            }
            else if (actions[0] == "message")
            {
                AddMessageToClient(actions[1], true);
            }
            else 
            {
                Console.WriteLine(res);
            }
        }
        
        /// <summary>The current player clicked to place a new Piece on the grid</summary>
        private void BtnNewPiece(object sender, RoutedEventArgs e)
        {
            int col = int.Parse((sender as Button)!.Tag.ToString()!);
            Piece.NewPiecePlayed(this, col);
        }
        
        /// <summary>Create a new game with a remote player</summary>
        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            info.Content = "Recherche d'une partie...";
            info.FontSize = 22;

            NewGame.Visibility = Visibility.Collapsed;
            LeaveGame.Visibility = Visibility.Collapsed;

            Utilitaires.ClearGrid(grille);

            Send("newGame," + player_uid);
        }

        /// <summary>Shutdown the application by clicking the leave button</summary>
        private void LeaveGame_Click(object sender, RoutedEventArgs e)
        {
            ExitApp();
        }
        
        /// <summary>Shutdown the application by clicking on the cross</summary>
        protected override void OnClosed(EventArgs e)
        {
            ExitApp();
        }
        
        /// <summary>Send information to the server to close the socket properly</summary>
        private void ExitApp()
        {
            if (_ClientSocket.Connected)
            {
                if (game != null) 
                    Send("quit," + game!.Id + "," + player_uid);
                else
                    Send("quit, ," + player_uid);
                _ClientSocket.Shutdown(SocketShutdown.Both);
                _ClientSocket.Close();
                listening_thread!.Interrupt();
            }
            Application.Current.Shutdown();
        }
        
        /// <summary>Print the message to the Message ListView</summary>
        /// <param name="message">The message to print</param>
        /// <param name="isMessageFromPlayers">true if it is the message from a player, false if it is auto-generated</param>
        /// <param name="isMessageFromPlayer1">true if the message is from player 1, false if not</param>
        public void AddMessageToClient(string message, bool isMessageFromPlayers = false, bool isMessageFromPlayer1 = false)
        {
            Dispatcher.Invoke(() => {
                Utilitaires.PrintMessageToClient(this, message, isMessageFromPlayers, isMessageFromPlayer1);
            });
        }
        
        /// <summary>Send the message by clicking the send button</summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MessageClient.Text != "")
            {
                Send("message," + this.game!.Id + "," + this.player_uid + "," + MessageClient.Text);
                AddMessageToClient("Vous: " + MessageClient.Text, true, true);
                MessageClient.Text = "";
            }
        }
        
        /// <summary>Send the message by clicking enter</summary>
        private void EnterEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && MessageClient.Text != "")
            {
                Send("message," + this.game!.Id + "," + this.player_uid + "," + MessageClient.Text);
                AddMessageToClient("Vous: " + MessageClient.Text, true, true);
                MessageClient.Text = "";
            }
        }
        
        /// <summary>Create a game with a bot</summary>
        private void Launch_Bot(object sender, RoutedEventArgs e)
        {
            LaunchGameAgainstBot();
        }
        
        /// <summary>Print informations when the player wants to play against a bot</summary>
        private void LaunchGameAgainstBot()
        {
            this.grid = new CustomGrid(this);
            launch.Visibility = Visibility.Collapsed;
            this.player_uid = Guid.NewGuid().ToString();

            string game_id = "matchFound:" + Guid.NewGuid().ToString();
            string player1 = username.Text + ":" + this.player_uid;
            string bot = "Bot:" + Guid.NewGuid().ToString();
            this.isPlayingAgainstBot = true;

            CreateGame(new string[] { game_id, player1, bot });
        }
        
        /// <summary>Create a model with received informations</summary>
        /// <param name="actions">All informations to create the game</param>
        private void CreateGame(string[] actions)
        {
            this.game = new Game(actions[0], new Player(actions[1], Brushes.Red), new Player(actions[2], Brushes.Yellow), this);
            this.isPlayer1 = (this.player_uid == this.game!.Player1.Id);

            Dispatcher.Invoke(() => { Utilitaires.PrintGameFound(this); });

            // Active les boutons pour le joueur 1
            if (this.player_uid == game.Player1.Id) this.grid!.ToggleEnableButtons();
        }
        
        /// <summary>Create a new game with a bot</summary>
        private void NewGameBot_Click(object sender, RoutedEventArgs e)
        {
            NewGame.Visibility = Visibility.Collapsed;
            LeaveGame.Visibility = Visibility.Collapsed;

            Utilitaires.ClearGrid(grille);
            MessageListView.Items.Clear();

            LaunchGameAgainstBot();
        }

    }
}
