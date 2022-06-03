using p4_client.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
        /// <summary>
        /// Remove all color on the grid
        /// </summary>
        /// <param name="grille">The grid to clear</param>
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
        /// <summary>
        /// Remove all panels on the grid
        /// </summary>
        /// <param name="app">The Main Window</param>
        public static void ClearWindowUI(MainWindow app)
        {
            app.StartPage.Visibility = Visibility.Collapsed;
            app.GamePage.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Print informations on the UI when a match has been found
        /// </summary>
        /// <param name="app">The Main Window</param>
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
        /// <summary>
        /// Print a message in the Message ListView on the UI. Can be a message from one player to the other, or a generated message.
        /// </summary>
        /// <param name="app">The Main Window</param>
        /// <param name="message">Message to transfer</param>
        /// <param name="isMessageFromPlayers">true if a player sent the message, false if it is a generated message</param>
        /// <param name="isMessageFromPlayer2">true if player2 sent the message, false if it was player1</param>
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

        public static void CreateFile(string filePath, Game game)
        {
            //Création de fichier si aucun créer
            if (!File.Exists(filePath))
            {
                using StreamWriter sw = File.CreateText(filePath);
                sw.WriteLine("Duel entre " + game.Player1.Name + " et " + game.Player2.Name + "\n");
            }
            
        }

        /// <summary>
        ///message == id du joueur : nom du joueur : colone jouée
        /// </summary>
        public static void WriteInFile(string filePath, string message, Game game)
        {
            if (!File.Exists(filePath)) CreateFile(filePath, game);
            //Écriture a la suite dans le fichier
            using (StreamWriter sw = File.AppendText(filePath))
            {
                //id:nom:colonne
                sw.WriteLine(message);
            }
        }

        public static string[] ReadFile(string filePath)
        {
            if (!File.Exists(filePath)) return new string[0];
            string[] lines = File.ReadAllLines(filePath);
            return lines;
        }

        public static void OpenFile(string filePath)
        {
            string fileName = filePath.Split('\\')[filePath.Count(f => f == '\\')];
            try
            {
                // The following call to Start succeeds if test.txt exists.
                Console.WriteLine("\nTrying to launch '" + filePath + "'...");
                Process.Start("notepad.exe", filePath);
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
