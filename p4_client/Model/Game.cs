using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace p4_client.Model {
    public class Game : EventArgs {
        public string Id { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        private readonly MainWindow MainWindow;

        public Game(string id, Player p1, Player p2, MainWindow mainWindow) {
            this.Id = id.Split(":")[1];
            this.Player1 = p1;
            this.Player2 = p2;
            this.MainWindow = mainWindow;
        }

        /// <summary>
        /// Send to the server that the game finished as draw, print rematch and leave buttons on the UI
        /// </summary>
        public void Draw()
        {
            MainWindow.Send("endGame," + this.Id + "," + MainWindow.player_uid + ",draw");
            MainWindow.AddMessageToClient("La partie se termine sur une égalité!");

            MainWindow.Dispatcher.Invoke(() => {
                MainWindow.CurrentPlayer.Content = "Egalité!";
                MainWindow.LeaveGame.Visibility = Visibility.Visible;

                if (MainWindow.isPlayingAgainstBot)
                    MainWindow.NewGameAgainstBot.Visibility = Visibility.Visible;
                else
                    MainWindow.NewGame.Visibility = Visibility.Visible;
            });
        }

        /// <summary>
        /// Send to the server that the game finished as a lose, print rematch and leave buttons on the UI
        /// </summary>
        public void Lose()
        {
            MainWindow.Send("endGame," + this.Id + "," + MainWindow.player_uid + ",victory");
            MainWindow.AddMessageToClient("Vous avez perdu la partie!");

            MainWindow.Dispatcher.Invoke(() => {
                MainWindow.CurrentPlayer.Content = "Vous avez perdu!";
                MainWindow.info.FontSize = 30;
                MainWindow.LeaveGame.Visibility = Visibility.Visible;

                if (MainWindow.isPlayingAgainstBot)
                    MainWindow.NewGameAgainstBot.Visibility = Visibility.Visible;
                else
                    MainWindow.NewGame.Visibility = Visibility.Visible;

            });
        }

        /// <summary>
        /// Send to the server that the game finished as a victory, print rematch and leave buttons on the UI
        /// </summary>
        public void Victory()
        {
            MainWindow.Dispatcher.Invoke(() => {
                MainWindow.AddMessageToClient("Vous avez gagné la partie!");
                MainWindow.CurrentPlayer.Content = "Vous avez gagné!";
                MainWindow.info.FontSize = 30;
                MainWindow.LeaveGame.Visibility = Visibility.Visible;

                if (MainWindow.isPlayingAgainstBot)
                    MainWindow.NewGameAgainstBot.Visibility = Visibility.Visible;
                else
                    MainWindow.NewGame.Visibility = Visibility.Visible;
            });
        }
    }
}
