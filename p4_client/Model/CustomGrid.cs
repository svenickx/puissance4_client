using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace p4_client.Model
{
    public class CustomGrid: Grid
    {
        Grid grille;

        public CustomGrid(Grid grille)
        {
            this.grille = grille;
        }

        /// <summary>Call different funtions to check wether there is a line of the 4 same pieces on the grid.</summary>
        /// <returns>true if there is a line of 4 in the grid, false if not.</returns>
        public bool CheckEndGame(MainWindow app)
        {
            return CheckRows(app) || CheckColumns(app) || CheckTopLeftDiagonals(app) || CheckBottomLeftDiagonals(app);
        }

        /// <summary>Check if there is 4 same pieces on at least one row of the grid.</summary>
        /// <returns>true if there is a line of 4 in the grid, false if not.</returns>
        private bool CheckRows(MainWindow app)
        {
            for (int row = 1; row < 7; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    bool isSameColor = false;

                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col + i);
                            isSameColor = element!.Fill.Equals(app.color);
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

        /// <summary>Check if there is 4 same pieces on at least one column of the grid.</summary>
        /// <returns>true if there is a line of 4 in the grid, false if not.</returns>
        private bool CheckColumns(MainWindow app)
        {
            for (int col = 0; col < 7; col++)
            {
                for (int row = 1; row < 4; row++)
                {
                    bool isSameColor = false;

                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row + i && Grid.GetColumn(e) == col);
                            isSameColor = element!.Fill.Equals(app.color);
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

        /// <summary>Check if there is 4 same pieces on at least one diagonal of the grid.</summary>
        /// <returns>true if there is a line of 4 in the grid, false if not.</returns>
        private bool CheckBottomLeftDiagonals(MainWindow app)
        {
            for (int col = 0; col < 4; col++)
            {
                for (int row = 6; row > 3; row--)
                {
                    bool isSameColor = false;

                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row - i && Grid.GetColumn(e) == col + i);
                            isSameColor = element!.Fill.Equals(app.color);
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

        /// <summary>Check if there is 4 same pieces on at least one diagonal of the grid.</summary>
        /// <returns>true if there is a line of 4 in the grid, false if not.</returns>
        public bool CheckTopLeftDiagonals(MainWindow app)
        {
            for (int col = 0; col < 4; col++)
            {
                for (int row = 1; row < 4; row++)
                {
                    bool isSameColor = false;
                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row + i && Grid.GetColumn(e) == col + i);
                            isSameColor = element!.Fill.Equals(app.color);
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
    }
}
