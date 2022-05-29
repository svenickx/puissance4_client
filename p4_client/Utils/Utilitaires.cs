using p4_client.Model;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace p4_client.Utils
{
    class Utilitaires
    {
        public static void ClearGrid(Grid grille)
        {
            for (int row = 1; row <= 6; row++)
            {
                for (int col = 0; col <= 6; col++)
                {
                    var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);
                    element!.Fill = Brushes.White;
                    element!.Stroke = Brushes.White;
                }
            }
        }
        public static void ClearWindowUI(MainWindow app)
        {
            app.StartPage.Visibility = Visibility.Collapsed;
            app.GamePage.Visibility = Visibility.Collapsed;
        }
        public static void PrintGameFound(MainWindow app)
        {
            ClearWindowUI(app);
            app.GamePage.Visibility = Visibility.Visible;
            app.MessageListView.Items.Clear();

            PrintMessageToClient(app, "Une partie a été trouvée!");
            PrintMessageToClient(app, app.game!.Player1.Name + " VS " + app.game!.Player2.Name);
            app.playerOne.Content = (app.player_uid == app.game!.Player1.Id) ? app.game!.Player1.Name + "\n(vous)" : app.game!.Player1.Name;
            app.playerTwo.Content = (app.player_uid == app.game!.Player2.Id) ? app.game!.Player2.Name + "\n(vous)" : app.game!.Player2.Name;
            app.CurrentPlayer.Content = app.game.Player1.Name + " commence la partie.";
        }
        public static void PrintMessageToClient(MainWindow app, string message, bool isMessageFromPlayers = false, bool isMessageFromPlayer2 = false)
        {
            app.MessageListView.SelectedIndex = app.MessageListView.Items.Count - 1;
            app.MessageListView.Items.Remove(app.MessageListView.SelectedIndex);

            ListViewItem listViewItem = new();
            Label messageLabel = new();
            messageLabel.Content = message;
            listViewItem.Content = messageLabel;
            if (isMessageFromPlayers)
            {
                listViewItem.Background = (isMessageFromPlayer2) ? app.game!.Player2.Color : app.game!.Player1.Color;
            }

            app.MessageListView.Items.Add(listViewItem);

            app.MessageListView.Items.Add(new ListViewItem());
            app.MessageListView.SelectedIndex = app.MessageListView.Items.Count - 1;
            app.MessageListView.ScrollIntoView(app.MessageListView.SelectedItem);
        }
    }
}
