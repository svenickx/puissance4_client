using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace p4_client.Utils
{
    class Piece
    {

        /// <summary>
        /// Actions performed when the current Client played.
        /// </summary>
        /// <param name="app">The Main Window</param>
        /// <param name="column">The column number where the piece is played</param>
        public static async void NewPiecePlayed(MainWindow app, int column)
        {
            bool nextAvailable = false;
            app.grid!.ToggleEnableButtons();

            app.Send("move," + app.game!.Id + "," + app.player_uid + "," + column);
            app.AddMessageToClient("Vous avez placé une pièce dans la colonne " + column.ToString());
            app.CurrentPlayer.Content = "A votre adversaire de jouer!";

            for (int row = 1; row <= 6; row++)
            {
                // Le joueur actuel à placer une pièce
                nextAvailable = NewPiece(row, column, nextAvailable, app.isPlayer1, app.grille);
                if (!nextAvailable) break;
                await Task.Delay(app.delayFallPieces);
            }

            // vérifie si l'adversaire à gagné ou s'il est encore possible de jouer
            bool isEndGame = (app.isPlayer1) ? app.grid!.CheckEndGame(app.game!.Player2.Color) : app.grid!.CheckEndGame(app.game!.Player1.Color);
            if (isEndGame && app.isPlayingAgainstBot)
                app.game!.Victory();
            if (isEndGame)
                app.grid!.BlinkRectanglesWithoutDispatcher();
            else if (app.isPlayingAgainstBot)
                BotPiece(app);
        }

        /// <summary>
        /// Actions performed when the remote Client played.
        /// </summary>
        /// <param name="app">The Main Window</param>
        /// <param name="column">The column number where the piece is played</param>
        public static void NewPieceReceived(MainWindow app, int column)
        {
            app.AddMessageToClient("Votre adversaire a placé une pièce dans la colonne " + column.ToString());
            for (int row = 1; row <= 6; row++)
            {
                bool nextAvailable = false;
                app.Dispatcher.Invoke(() =>
                {
                    // Le joueur adversaire à placer une pièce
                    nextAvailable = Piece.NewPiece(row, column, nextAvailable, !app.isPlayer1, app.grille);
                    app.CurrentPlayer.Content = "A vous de jouer!";
                });
                if (!nextAvailable) break;
                Thread.Sleep(app.delayFallPieces);
            }

            // vérifie si l'adversaire à gagné ou s'il est encore possible de jouer
            bool isEndGame = (app.isPlayer1) ? app.grid!.CheckEndGame(app.game!.Player1.Color) : app.grid!.CheckEndGame(app.game!.Player2.Color);
            if (isEndGame)
            {
                app.game!.Lose();
                app.grid!.BlinkRectanglesWithDispatcher();
            }
            else if (!app.grid!.ToggleEnableButtons()) app.game!.Draw();
        }

        /// <summary>
        /// Actions performed when the bot played.
        /// </summary>
        /// <param name="app">The Main Window</param>
        public static async void BotPiece(MainWindow app)
        {
            Random rnd = new();
            int column = rnd.Next(0, 7);
            app.AddMessageToClient("Votre adversaire a placé une pièce dans la colonne " + column.ToString());
            for (int row = 1; row <= 6; row++)
            {
                bool nextAvailable = false;
                // Le joueur adversaire à placer une pièce
                nextAvailable = NewPiece(row, column, nextAvailable, !app.isPlayer1, app.grille);
                app.CurrentPlayer.Content = "A vous de jouer!";
                if (!nextAvailable) break;
                await Task.Delay(app.delayFallPieces);
            }

            // vérifie si l'adversaire à gagné ou s'il est encore possible de jouer
            bool isEndGame = (app.isPlayer1) ? app.grid!.CheckEndGame(app.game!.Player1.Color) : app.grid!.CheckEndGame(app.game!.Player2.Color);
            if (isEndGame)
            {
                app.game!.Lose();
                app.grid!.BlinkRectanglesWithoutDispatcher();
            }
            else if (!app.grid!.ToggleEnableButtons()) app.game!.Draw();
        }

        /// <summary>
        /// Place a piece on a designed Rectangle to create an animation
        /// </summary>
        /// <param name="row">The row where the new Rectangle must be printed</param>
        /// <param name="column">The column where the new Rectangle must be printed</param>
        /// <param name="nextAvailable">Is the bottom Rectangle free</param>
        /// <param name="isPlayer1">Is the action performed by player1</param>
        /// <param name="grille">The grid to modify</param>
        /// <returns>true if the bottom piece is free, false if not</returns>
        public static bool NewPiece(int row, int column, bool nextAvailable, bool isPlayer1, Grid grille)
        {
            var element = (Rectangle?)grille
                .Children.Cast<UIElement>()
                .FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == column);
            var color = (isPlayer1) ? Brushes.Yellow : Brushes.Red;

            if (row != 1)
            {
                var elementPrev = (Rectangle?)grille.
                    Children.Cast<UIElement>()
                    .FirstOrDefault(e => Grid.GetRow(e) == row - 1 && Grid.GetColumn(e) == column);
                elementPrev!.Fill = Brushes.White;
                elementPrev!.Stroke = Brushes.White;
            }

            element!.Fill = color;
            element!.Stroke = Brushes.Black;

            if (row != 6)
            {
                var elementNext = (Rectangle?)grille.
                    Children.Cast<UIElement>()
                    .FirstOrDefault(e => Grid.GetRow(e) == row + 1 && Grid.GetColumn(e) == column);
                nextAvailable = elementNext!.Fill.Equals(Brushes.White);
            }
            else
            {
                var lastElement = (Rectangle?)grille
                    .Children.Cast<UIElement>()
                    .FirstOrDefault(e => Grid.GetRow(e) == 6 && Grid.GetColumn(e) == column);
                nextAvailable = lastElement!.Fill.Equals(Brushes.White);
            }
            return nextAvailable;
        }
    }
}
