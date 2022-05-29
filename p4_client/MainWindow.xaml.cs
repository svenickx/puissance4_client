using p4_client.Model;
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

        private Thread? listening_thread;
        private readonly int delayFallPieces = 100;
        private readonly int port = 10000;

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
        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            this.grid = new CustomGrid(this);
            launch.Visibility = Visibility.Collapsed;
            LoopConnect();

            listening_thread = new Thread(Receive);
            listening_thread.Start();

            this.player_uid = Guid.NewGuid().ToString();
            Send("search," + username.Text + "," + this.player_uid);
        }
        public void Send(string req)
        {
            if (_ClientSocket.Connected)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(req);
                _ClientSocket.Send(buffer);
            }
        }
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
                this.game = new Game(actions[0], new Player(actions[1], Brushes.Red), new Player(actions[2], Brushes.Yellow));
                this.isPlayer1 = (this.player_uid == this.game!.player1.Id);

                Dispatcher.Invoke(() => {
                    Utilitaires.PrintGameFound(this);
                });

                // Active les boutons pour le joueur 1
                if (this.player_uid == game.player1.Id) {
                    ToggleEnableButtons();
                }
            } 
            else if (actions[0] == "move")
            {
                NewPieceReceived(Int32.Parse(actions[1]));
            }
            else if (actions[0] == "endGame")
            {
                if (actions[1] == "victory") VictoryGame();
                if (actions[1] == "draw") DrawGame();
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
        private void BtnNewPiece(object sender, RoutedEventArgs e)
        {
            int col = int.Parse((sender as Button)!.Tag.ToString()!);
            NewPiecePlayed(col);
        }
        private async void NewPiecePlayed(int column)
        {
            bool nextAvailable = false;
            ToggleEnableButtons();

            Send("move," + game!.id + "," + player_uid + "," + column);
            AddMessageToClient("Vous avez placé une pièce dans la colonne " + column.ToString());

            for (int row = 1; row <= 6; row++)
            {
                // Le joueur actuel à placer une pièce
                nextAvailable = Piece.NewPiece(row, column, nextAvailable, isPlayer1, grille);
                CurrentPlayer.Content = "A votre adversaire de jouer!";
                if (!nextAvailable) break;
                await Task.Delay(delayFallPieces);
            }
            // vérifie si l'adversaire à gagné ou s'il est encore possible de jouer
            bool isEndGame = (isPlayer1) ? grid!.CheckEndGame(game!.player2.Color) : grid!.CheckEndGame(game!.player1.Color);
            if (isEndGame) 
            {
                this.grid!.BlinkRectanglesWithoutDispatcher();
            }
        }
        private void NewPieceReceived(int column)
        {
            AddMessageToClient("Votre adversaire a placé une pièce dans la colonne " + column.ToString());
            for (int row = 1; row <= 6; row++)
            {
                bool nextAvailable = false;
                Dispatcher.Invoke(() =>
                {
                   // Le joueur adversaire à placer une pièce
                    nextAvailable = Piece.NewPiece(row, column, nextAvailable, !isPlayer1, grille);
                    CurrentPlayer.Content = "A vous de jouer!";
                });
                if (!nextAvailable) break;
                Thread.Sleep(delayFallPieces);
            }

            // vérifie si l'adversaire à gagné ou s'il est encore possible de jouer
            bool isEndGame = (isPlayer1) ? grid!.CheckEndGame(game!.player1.Color) : grid!.CheckEndGame(game!.player2.Color);
            if (isEndGame)
            {
                LoseGame();
                this.grid!.BlinkRectanglesWithDispatcher();
            }
            else if (!ToggleEnableButtons()) DrawGame();
        }
        private void LoseGame()
        {
            Send("endGame," + game!.id + "," + this.player_uid + ",victory");
            AddMessageToClient("Vous avez perdu la partie!");

            Dispatcher.Invoke(() => { 
                CurrentPlayer.Content = "Vous avez perdu!";
                info.FontSize = 30;
                NewGame.Visibility = Visibility.Visible;
                LeaveGame.Visibility = Visibility.Visible;
            });
        }
        private void VictoryGame()
        {
            Dispatcher.Invoke(() => {
            AddMessageToClient("Vous avez gagné la partie!");
                CurrentPlayer.Content = "Vous avez gagné!";
                info.FontSize = 30;
                NewGame.Visibility = Visibility.Visible;
                LeaveGame.Visibility = Visibility.Visible;
            });
        }
        private void DrawGame()
        {
            Send("endGame," + game!.id + "," + player_uid + ",draw");
            AddMessageToClient("La partie se termine sur une égalité!");

            Dispatcher.Invoke(() => {
                CurrentPlayer.Content = "Egalité!";
                NewGame.Visibility = Visibility.Visible;
                LeaveGame.Visibility = Visibility.Visible;
            });
        }
        private bool ToggleEnableButtons()
        {
            bool isGridStillPlayable = false;

            for (int col = 0; col <= 6; col++)
            {
                Dispatcher.Invoke(() => {
                    var btn = (Button?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == 0 && Grid.GetColumn(e) == col);
                    var rectangleBellow = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == 1 && Grid.GetColumn(e) == col);

                    if (rectangleBellow!.Fill.Equals(Brushes.White))
                    {
                        btn!.IsEnabled = !btn.IsEnabled;
                        isGridStillPlayable = true;
                    } 
                    else
                    {
                        btn!.IsEnabled = false;
                    }
                });
            }
            return isGridStillPlayable;
        }
        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            info.Content = "Recherche d'une partie...";
            info.FontSize = 22;

            NewGame.Visibility = Visibility.Collapsed;
            LeaveGame.Visibility = Visibility.Collapsed;

            Utilitaires.ClearGrid(grille);

            Send("newGame," + player_uid);
        }
        private void LeaveGame_Click(object sender, RoutedEventArgs e)
        {
            ExitApp();
        }
        protected override void OnClosed(EventArgs e)
        {
            ExitApp();
        }
        private void ExitApp()
        {
            if (_ClientSocket.Connected)
            {
                Send("quit," + game!.id + "," + player_uid);
                _ClientSocket.Shutdown(SocketShutdown.Both);
                _ClientSocket.Close();
                listening_thread!.Interrupt();
            }
            Application.Current.Shutdown();
        }
        private void AddMessageToClient(string message, bool isMessageFromPlayers = false, bool isMessageFromPlayer1 = false)
        {
            Dispatcher.Invoke(() => {
                Utilitaires.PrintMessageToClient(this, message, isMessageFromPlayers, isMessageFromPlayer1);
            });
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MessageClient.Text != "")
            {
                Send("message," + this.game!.id + "," + this.player_uid + "," + MessageClient.Text);
                AddMessageToClient("Vous: " + MessageClient.Text, true, true);
                MessageClient.Text = "";
            }
        }
        private void EnterEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && MessageClient.Text != "")
            {
                Send("message," + this.game!.id + "," + this.player_uid + "," + MessageClient.Text);
                AddMessageToClient("Vous: " + MessageClient.Text, true, true);
                MessageClient.Text = "";
            }
        }
    }
}