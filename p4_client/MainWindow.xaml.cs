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
using System.Windows.Media;
using System.Windows.Shapes;

namespace p4_client
{
    public partial class MainWindow : Window
    {
        private Socket _ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public string? player_uid;
        public Game? game;
        Thread? listening_thread;
        private int port = 10000;
        public CustomGrid? grid;
        public SolidColorBrush? color;

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
        private void launch_Click(object sender, RoutedEventArgs e)
        {
            this.grid = new CustomGrid(grille);
            launch.Visibility = Visibility.Collapsed;
            LoopConnect();

            listening_thread = new Thread(Receive);
            listening_thread.Start();

            player_uid = Guid.NewGuid().ToString();
            Send("search," + username.Text + "," + player_uid);
        }
        public void Send(string req)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(req);
            _ClientSocket.Send(buffer);
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
                game = new Game(actions[0], new Player(actions[1]), new Player(actions[2]));
                this.color = (this.player_uid == this.game!.player1.id) ? Brushes.Red : Brushes.Yellow;

                Dispatcher.Invoke(() => {
                    Utilitaires.PrintGameFound(this);
                });

                // Active les boutons pour le joueur 1
                if (player_uid == game.player1.id) {
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

            for (int row = 1; row <= 6; row++)
            {
                // Le joueur actuel à placer une pièce
                nextAvailable = Piece.NewPiece(row, column, nextAvailable, (player_uid == game.player1.id), grille);
                if (!nextAvailable) break;
                await Task.Delay(1000);
            }
        }
        private void NewPieceReceived(int column)
        {
            for (int row = 1; row <= 6; row++)
            {
                bool nextAvailable = false;
                Dispatcher.Invoke(() =>
                {
                   // Le joueur adversaire à placer une pièce
                    nextAvailable = Piece.NewPiece(row, column, nextAvailable, !(player_uid == game!.player1.id), grille);
                });
                if (!nextAvailable) break;
                Thread.Sleep(1000);
            }

            // vérifie si l'adversaire à gagné ou s'il est encore possible de jouer
            if (this.grid!.CheckEndGame(this)) LoseGame();
            else if (!ToggleEnableButtons()) DrawGame();
        }
        private void LoseGame()
        {
            Send("endGame," + game!.id + "," + player_uid + ",victory");

            Dispatcher.Invoke(() => { 
                info.Content = "Vous avez perdu!";
                info.FontSize = 30;
                NewGame.Visibility = Visibility.Visible;
                LeaveGame.Visibility = Visibility.Visible;
            });
        }
        private void VictoryGame()
        {
            Dispatcher.Invoke(() => {
                info.Content = "Vous avez gagné!";
                info.FontSize = 30;
                NewGame.Visibility = Visibility.Visible;
                LeaveGame.Visibility = Visibility.Visible;
            });
        }
        private void DrawGame()
        {
            Send("endGame," + game!.id + "," + player_uid + ",draw");

            Dispatcher.Invoke(() => {
                info.Content = "Egalité!";
                info.FontSize = 30;
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
    }
}