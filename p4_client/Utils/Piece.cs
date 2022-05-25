using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace p4_client.Utils
{
    class Piece
    {
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
