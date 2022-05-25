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
        private async void NewPiecePlayed(int column)
        {
            bool nextAvailable = false;
            ToggleEnableButtons();

            string query = "move," + game.id + "," + player_uid + "," + column.ToString();
            Send(query);

            for (int row = 1; row <= 6; row++)
            {
                nextAvailable = NewPiece(row, column, nextAvailable, (player_uid == game.player1.id));
                if (!nextAvailable)
                {
                    break;
                }
                //await Task.Delay(1000);
            }
        }
        private void NewPieceReceived(int column)
        {
            int row;
            for (row = 1; row <= 6; row++)
            {
                bool nextAvailable = false;
                Dispatcher.Invoke(() =>
                {
                    nextAvailable = NewPiece(row, column, nextAvailable, !(player_uid == game.player1.id));
                });
                if (!nextAvailable)
                {
                    break;
                }
                //Thread.Sleep(1000);
            }
            if (CheckEndGame())
            {
                LoseGame();
                Dispatcher.Invoke(() => { 
                });
            }
            else
            {
                bool isGridStillPlayable = ToggleEnableButtons();
                if (!isGridStillPlayable)
                {
                    Send("endGame," + game.id + "," + player_uid + ",draw");
                    DrawGame();
                }
            }
        }
        private bool NewPiece(int row, int column, bool nextAvailable, bool isPlayer1)
        {
            var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == column);
            var color = (isPlayer1) ? Colors.Yellow: Colors.Red;

            if (row != 1)
            {
                var elementPrev = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row - 1 && Grid.GetColumn(e) == column);
                elementPrev!.Fill = new SolidColorBrush(Colors.Beige);
            }

            element!.Fill = new SolidColorBrush(color);

            if (row != 6)
            {
                var elementNext = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row + 1 && Grid.GetColumn(e) == column);
                nextAvailable = elementNext!.Fill.ToString().Equals(Colors.Beige.ToString());
            }
            else
            {
                var lastElement = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == 6 && Grid.GetColumn(e) == column);
                nextAvailable = lastElement!.Fill.ToString().Equals(Colors.Beige.ToString());
            }
            return nextAvailable;
        }
        private bool CheckEndGame()
        {
            return CheckRows() || CheckColumns() || CheckTopLeftDiagonals() || CheckBottomLeftDiagonals();
        }
        private bool CheckRows()
        {
            var color = (!(player_uid == game.player1.id)) ? Colors.Yellow : Colors.Red;
            
            for (int row = 1; row < 7; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    bool isSameColor = false;

                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col + i);
                            isSameColor = element.Fill.ToString().Equals(color.ToString());
                        });
                        if (!isSameColor)
                        {
                            break;
                        }
                    }

                    if (!isSameColor) continue;
                    return true;
                }
            }
            return false;
        }
        private bool CheckColumns()
        {
            var color = (!(player_uid == game.player1.id)) ? Colors.Yellow : Colors.Red;

            for (int col = 0; col < 7; col++)
            {
                for (int row = 1; row < 4; row++)
                {
                    bool isSameColor = false;

                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row + i && Grid.GetColumn(e) == col);
                            isSameColor = element.Fill.ToString().Equals(color.ToString());
                        });
                        if (!isSameColor)
                        {
                            break;
                        }
                    }

                    if (!isSameColor) continue;
                    return true;
                }
            }

            return false;
        }
        private bool CheckTopLeftDiagonals()
        {
            var color = (!(player_uid == game.player1.id)) ? Colors.Yellow : Colors.Red;

            for (int col = 0; col < 4; col++)
            {
                for (int row = 1; row < 4; row++)
                {
                    bool isSameColor = false;

                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row + i && Grid.GetColumn(e) == col + i);
                            isSameColor = element.Fill.ToString().Equals(color.ToString());
                        });
                        if (!isSameColor)
                        {
                            break;
                        }
                    }

                    if (!isSameColor) continue;
                    return true;
                }
            }

            return false;
        }
        private bool CheckBottomLeftDiagonals()
        {
            var color = (!(player_uid == game.player1.id)) ? Colors.Yellow : Colors.Red;

            for (int col = 0; col < 4; col++)
            {
                for (int row = 6; row > 3; row--)
                {
                    bool isSameColor = false;

                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row - i && Grid.GetColumn(e) == col + i);
                            isSameColor = element.Fill.ToString().Equals(color.ToString());
                        });
                        if (!isSameColor)
                        {
                            break;
                        }
                    }

                    if (!isSameColor) continue;
                    return true;
                }
            }

            return false;
        }
        private void LoseGame()
        {
            Send("endGame," + game.id + "," + player_uid + ",victory");

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

                    if (rectangleBellow!.Fill.ToString().Equals(Colors.Beige.ToString()))
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
        private void BtnNewPiece(object sender, RoutedEventArgs e)
        {
            int col = int.Parse((sender as Button).Tag.ToString());
            NewPiecePlayed(col);
        }
        private void NewGame_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LeaveGame_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}