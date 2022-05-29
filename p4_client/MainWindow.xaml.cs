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
        private void BtnNewPiece(object sender, RoutedEventArgs e)
        {
            int col = int.Parse((sender as Button)!.Tag.ToString()!);
            Piece.NewPiecePlayed(this, col);
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
        public void AddMessageToClient(string message, bool isMessageFromPlayers = false, bool isMessageFromPlayer1 = false)
        {
            Dispatcher.Invoke(() => {
                Utilitaires.PrintMessageToClient(this, message, isMessageFromPlayers, isMessageFromPlayer1);
            });
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MessageClient.Text != "")
            {
                Send("message," + this.game!.Id + "," + this.player_uid + "," + MessageClient.Text);
                AddMessageToClient("Vous: " + MessageClient.Text, true, true);
                MessageClient.Text = "";
            }
        }
        private void EnterEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && MessageClient.Text != "")
            {
                Send("message," + this.game!.Id + "," + this.player_uid + "," + MessageClient.Text);
                AddMessageToClient("Vous: " + MessageClient.Text, true, true);
                MessageClient.Text = "";
            }
        }
        private void Launch_Bot(object sender, RoutedEventArgs e)
        {
            LaunchGameAgainstBot();
        }
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
        private void CreateGame(string[] actions)
        {
            this.game = new Game(actions[0], new Player(actions[1], Brushes.Red), new Player(actions[2], Brushes.Yellow), this);
            this.isPlayer1 = (this.player_uid == this.game!.Player1.Id);

            Dispatcher.Invoke(() => { Utilitaires.PrintGameFound(this); });

            // Active les boutons pour le joueur 1
            if (this.player_uid == game.Player1.Id) this.grid!.ToggleEnableButtons();
        }
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
