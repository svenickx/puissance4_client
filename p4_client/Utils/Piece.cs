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
        public static async void NewPiecePlayed(MainWindow app, int column, string fileName)
        {
            bool nextAvailable = false;
            app.grid!.ToggleEnableButtons();

            app.Send("move," + app.game!.Id + "," + app.player_uid + "," + column);
            app.AddMessageToClient("Vous avez placé une pièce dans la colonne " + column.ToString());

            if (app.isPlayer1)
            {
                Utilitaires.WriteInFile(fileName, app.game!.Player1.Id + ":" + app.game.Player1.Name + ":" + column.ToString(), app.game);
            }
            else
            {
                Utilitaires.WriteInFile(fileName, app.game!.Player2.Id + ":" + app.game.Player2.Name + ":" + column.ToString(), app.game);

            }

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
                BotPiece(app, fileName);
        }

        /// <summary>
        /// Actions performed when the remote Client played.
        /// </summary>
        /// <param name="app">The Main Window</param>
        /// <param name="column">The column number where the piece is played</param>
        public static void NewPieceReceived(MainWindow app, int column, string fileName)
        {
            app.AddMessageToClient("Votre adversaire a placé une pièce dans la colonne " + column.ToString());
            if (app.isPlayer1)
            {
                Utilitaires.WriteInFile(fileName, app.game!.Player2.Id + ":" + app.game.Player2.Name + ":" + column.ToString(), app.game);
            }
            else
            {
                Utilitaires.WriteInFile(fileName, app.game!.Player1.Id + ":" + app.game.Player1.Name + ":" + column.ToString(), app.game);

            }

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
        public static async void BotPiece(MainWindow app, string fileName)
        {
            int colToPlay;
            colToPlay = CheckPlayPossibilities(app.grille);
            Console.WriteLine("Meilleur action: " + colToPlay + "\n");
            
            if (colToPlay == -1)
            {
                Random rnd = new();
                colToPlay = rnd.Next(0, 7);
            }
            
            app.AddMessageToClient("Votre adversaire a placé une pièce dans la colonne " + colToPlay.ToString());
            for (int row = 1; row <= 6; row++)
            {
                bool nextAvailable = false;
                // Le joueur adversaire à placer une pièce
                nextAvailable = NewPiece(row, colToPlay, nextAvailable, !app.isPlayer1, app.grille);
                app.CurrentPlayer.Content = "A vous de jouer!";
                if (!nextAvailable) break;
                await Task.Delay(app.delayFallPieces);
            }

            // vérifie si l'adversaire à gagné ou s'il est encore possible de jouer
            bool isEndGame = (app.isPlayer1) ? app.grid!.CheckEndGame(app.game!.Player1.Color) : app.grid!.CheckEndGame(app.game!.Player2.Color);
            if (isEndGame)
            {
                app.grid!.BlinkRectanglesWithoutDispatcher();
                app.game!.Lose();
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
            Console.WriteLine(column);
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


        private static int CheckPlayPossibilities(Grid grille)
        {
            Dictionary<int, int> allPriorities;
            Dictionary<int, int> priorities = new()
            {
                { 1, -1 },
                { 2, -1 },
                { 3, -1 },
                { 4, -1 },
                { 5, -1 }
            };

            for (int row = 6; row > 1; row--)
            {
                for (int col = 0; col < 7; col++)
                {
                    // verifier la ligne
                    if (col < 4)
                    {
                        allPriorities = CheckRows(grille, row, col);
                        if (allPriorities[1] != -1) return allPriorities[1]; // Une ligne gagnante
                        if (priorities[2] == -1) priorities[2] = allPriorities[2]; // Une ligne perdante
                        if (priorities[3] == -1) priorities[3] = allPriorities[3]; // ligne rouge presque complète
                        if (priorities[4] == -1) priorities[4] = allPriorities[4]; // ligne jaune presque complète
                    }

                    // verifier la colonne jusqu'a row == 4
                    if (row > 3)
                    {
                        allPriorities = CheckColumns(grille, row, col);
                        if (allPriorities[1] != -1) return allPriorities[1]; // Une colonne gagnante
                        if (priorities[2] == -1) priorities[2] = allPriorities[2]; // Une colonne perdante
                        if (priorities[3] == -1) priorities[3] = allPriorities[3]; // colonne rouge presque complète
                        if (priorities[4] == -1) priorities[4] = allPriorities[4]; // colonne jaune presque complète
                    }

                    // verifier la diagonale bottom left jusqu'a col == 3
                    if (col < 4 && row > 3)
                    {
                        allPriorities = CheckBottomLeftDiagonals(grille, row, col);
                        if (allPriorities[1] != -1) return allPriorities[1]; // Une diagonale gagnante
                        if (priorities[2] == -1) priorities[2] = allPriorities[2]; // Une diagonale perdante
                    }

                    // verifier la diagonale bottom right a partir de col == 3
                    if (col > 3 && row > 3)
                    {
                        allPriorities = CheckBottomRightDiagonals(grille, row, col);
                        if (allPriorities[1] != -1) return allPriorities[1]; // Une diagonale gagnante
                        if (priorities[2] == -1) priorities[2] = allPriorities[2]; // Une diagonale perdante
                    }
                }
            }


            if (priorities[2] != -1) return priorities[2];
            if (priorities[3] != -1) return priorities[3];
            if (priorities[4] != -1) return priorities[4];
            return -1;
        }

        private static Dictionary<int, int> CheckBottomRightDiagonals(Grid grille, int row, int col)
        {
            Dictionary<int, int> priorities = new();
            List<Rectangle> rects = new();

            // ajoute les rectangles à vérifier dans la liste
            for (int i = 0; i < 4; i++)
            {
                var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row - i && Grid.GetColumn(e) == col - i);
                rects.Add(element!);
            }

            priorities = CheckListOfRectangles(rects);
            if (priorities[1] != -1) priorities[1] = col;
            if (priorities[2] != -1) priorities[2] = col;



            return priorities;
        }

        private static Dictionary<int, int> CheckBottomLeftDiagonals(Grid grille, int row, int col)
        {
            Dictionary<int, int> priorities = new();
            List<Rectangle> rects = new();

            // ajoute les rectangles à vérifier dans la liste
            for (int i = 0; i < 4; i++)
            {
                var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row - i && Grid.GetColumn(e) == col + i);
                rects.Add(element!);
            }

            priorities = CheckListOfRectangles(rects);
            if (priorities[1] != -1) priorities[1] += col;
            if (priorities[2] != -1) priorities[2] += col;

            return priorities;
        }

        private static Dictionary<int, int> CheckColumns(Grid grille, int row, int col)
        {
            Dictionary<int, int> priorities = new();
            List<Rectangle> rects = new();

            // ajoute les rectangles à vérifier dans la liste
            for (int i = 0; i < 4; i++)
            {
                var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row - i && Grid.GetColumn(e) == col);
                rects.Add(element!);
            }

            priorities = CheckListOfRectangles(rects);
            if (priorities[1] != -1) priorities[1] = col;
            if (priorities[2] != -1) priorities[2] = col;
            if (priorities[3] != -1) priorities[3] = col;
            if (priorities[4] != -1) priorities[4] = col;

            return priorities;
        }
        private static Dictionary<int, int> CheckRows(Grid grille, int row, int col)
        {
            Dictionary<int, int> priorities = new();
            List<Rectangle> rects = new();

            // ajoute les rectangles à vérifier dans la liste
            for (int i = 0; i < 4; i++)
            {
                var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col + i);
                rects.Add(element!);
            }

            priorities = CheckListOfRectangles(rects);
            if (priorities[1] != -1) priorities[1] += col;
            if (priorities[2] != -1) priorities[2] += col;
            if (priorities[3] != -1) priorities[3] += col;
            if (priorities[4] != -1) priorities[4] += col;

            // Vérifie si la ligne gagnante possède une pièce en dessous du rectangle vide
            if (row < 6 && priorities[1] != -1)
            {
                var elementBellow = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row + 1 && Grid.GetColumn(e) == priorities[1]);
                if (elementBellow!.Fill == Brushes.White) priorities[1] = -1;
            }

            // Vérifie si la ligne perdante possède une pièce en dessous du rectangle vide
            if (row < 6 && priorities[2] != -1)
            {
                var elementBellow = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row + 1 && Grid.GetColumn(e) == priorities[2]);
                if (elementBellow!.Fill == Brushes.White) priorities[2] = -1;
            }

            return priorities;
        }
        private static Dictionary<int, int> CheckListOfRectangles(List<Rectangle> rects)
        {
            Dictionary<int, int> priorities = new() {
                { 1, -1 },
                { 2, -1 },
                { 3, -1 },
                { 4, -1 },
                { 5, -1 },
            };

            // code: 0 pas de couleur, 1 couleur rouge, 2 couleur jaune
            List<int> colorCode = new();

            foreach (Rectangle rectangle in rects)
            {
                if (rectangle.Fill == Brushes.White) colorCode.Add(0);
                if (rectangle.Fill == Brushes.Red) colorCode.Add(1);
                if (rectangle.Fill == Brushes.Yellow) colorCode.Add(2);
            }

            // Si la ligne est pleine => passe
            if (!colorCode.Contains(0)) return priorities;

            // Si la ligne est composé de plusieurs couleurs
            if (CountDuplicates(colorCode, 1) == 2 && CountDuplicates(colorCode, 2) > 0) return priorities;

            // Si la ligne == 3 rectangles rouges et un vide => priorité 1
            if (CountDuplicates(colorCode, 1) == 3 && CountDuplicates(colorCode, 0) == 1)
            {
                priorities[1] = colorCode.IndexOf(0);
            }

            // Si la ligne == 3 rectangles jaunes et un vide => priorité 2
            if (CountDuplicates(colorCode, 2) == 3 && CountDuplicates(colorCode, 0) == 1)
            {
                priorities[2] = colorCode.IndexOf(0);
            } 

            // Si la ligne == 2 rectangles rouges et 2 vide => priorité 3
            if (CountDuplicates(colorCode, 1) == 2 && CountDuplicates(colorCode, 0) == 2)
            {
                priorities[3] = colorCode.IndexOf(0);
            }

            // Si la ligne == 2 rectangles jaunes et 2 vide => priorité 4
            if (CountDuplicates(colorCode, 2) == 2 && CountDuplicates(colorCode, 0) == 2)
            {
                priorities[4] = colorCode.IndexOf(0, colorCode.IndexOf(0) + 1);
            }


            return priorities;
        }
        private static int CountDuplicates(List<int> listInt, int toFind)
        {
            int count = 0;
            for (int i = 0; i < listInt.Count; i++)
            {
                if (listInt[i] == toFind) count++;
            }
            return count;
        }
    }
}
