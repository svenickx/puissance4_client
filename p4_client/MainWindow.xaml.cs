using p4_client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace p4_client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket _ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private string player_uid;
        Game game;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Send(string req)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(req);
            _ClientSocket.Send(buffer);
        }
        public void Receive()
        {
            Console.WriteLine("Now listening...");
            while (true)
            {
                byte[] receiveBuf = new byte[1024];
                int rec = _ClientSocket.Receive(receiveBuf);
                byte[] data = new byte[rec];
                Array.Copy(receiveBuf, data, rec);
                Console.WriteLine("Received: " + Encoding.ASCII.GetString(data));
                SelectActions(Encoding.ASCII.GetString(data));
            }
        }
        public void LoopConnect()
        {
            int attempts = 0;
            while (!_ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _ClientSocket.Connect(IPAddress.Loopback, 10000);
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

        private void launch_Click(object sender, RoutedEventArgs e) {
            launch.Visibility = Visibility.Collapsed;
            LoopConnect();

            Thread listening_thread = new Thread(Receive);
            listening_thread.Start();

            player_uid = Guid.NewGuid().ToString();
            string query = "search," + username.Text + "," + player_uid;
            Send(query);
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

                Dispatcher.Invoke(() => {
                    info.Content = "Partie trouvée";
                    username.Visibility = Visibility.Collapsed;

                    Thread.Sleep(2000);

                    playerOne.Visibility = Visibility.Visible;
                    playerOne.Content = (player_uid == game.player1.id) ? game.player1.name + "\n(vous)" : game.player1.name;
                    playerTwo.Visibility = Visibility.Visible;
                    playerTwo.Content = (player_uid == game.player2.id) ? game.player2.name + "\n(vous)" : game.player2.name;

                    grille.Visibility = Visibility.Visible;

                    info.Content = game.player1.name + " commence la partie.";
                });

                if (player_uid == game.player1.id) {
                    ToggleEnableButtons();
                }
            } 
            else 
            {
                Console.WriteLine(res);
            }
        }

        private void ToggleEnableButtons() {
            Dispatcher.Invoke(() => {
                btn_c0.IsEnabled = !btn_c0.IsEnabled;
                btn_c1.IsEnabled = !btn_c1.IsEnabled;
                btn_c2.IsEnabled = !btn_c2.IsEnabled;
                btn_c3.IsEnabled = !btn_c3.IsEnabled;
                btn_c4.IsEnabled = !btn_c4.IsEnabled;
                btn_c5.IsEnabled = !btn_c5.IsEnabled;
                btn_c6.IsEnabled = !btn_c6.IsEnabled;
            });
        }
    }
}