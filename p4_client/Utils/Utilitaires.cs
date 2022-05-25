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

        public static void PrintGameFound(MainWindow app)
        {
            app.info.Content = "Partie trouvée";
            app.username.Visibility = Visibility.Collapsed;

            app.playerOne.Visibility = Visibility.Visible;
            app.playerOne.Content = (app.player_uid == app.game!.player1.id) ? app.game!.player1.name + "\n(vous)" : app.game!.player1.name;
            app.playerTwo.Visibility = Visibility.Visible;
            app.playerTwo.Content = (app.player_uid == app.game!.player2.id) ? app.game!.player2.name + "\n(vous)" : app.game!.player2.name;

            app.grille.Visibility = Visibility.Visible;

            app.info.Content = app.game!.player1.name + " commence la partie.";
        }
    }
}
