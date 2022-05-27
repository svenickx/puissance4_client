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

namespace p4_client.Model
{
    public class CustomGrid: Grid
    {
        private readonly MainWindow MainWindow;
        private readonly List<Rectangle> rects = new();

        public CustomGrid(MainWindow MainWindow)
        {
            this.MainWindow = MainWindow;
        }

        /// <summary>Call different funtions to check wether there is a line of the 4 same pieces on the grid.</summary>
        /// <returns>true if there is a line of 4 in the grid, false if not.</returns>
        public bool CheckEndGame(SolidColorBrush color)
        {
            return CheckRows(color) || CheckColumns(color) || CheckTopLeftDiagonals(color) || CheckBottomLeftDiagonals(color);
        }

        /// <summary>Check if there is 4 same pieces on at least one row of the grid.</summary>
        /// <returns>true if there is a line of 4 in the grid, false if not.</returns>
        private bool CheckRows(SolidColorBrush color)
        {
            for (int row = 1; row < 7; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    bool isSameColor = false;

                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)this.MainWindow.grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col + i);
                            isSameColor = element!.Fill.Equals(color);
                            this.rects.Add(element);
                        });
                        if (!isSameColor)
                        {
                            this.rects.Clear();
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
        private bool CheckColumns(SolidColorBrush color)
        {
            for (int col = 0; col < 7; col++)
            {
                for (int row = 1; row < 4; row++)
                {
                    bool isSameColor = false;

                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)this.MainWindow.grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row + i && Grid.GetColumn(e) == col);
                            isSameColor = element!.Fill.Equals(color);
                            this.rects.Add(element);
                        });
                        if (!isSameColor)
                        {
                            this.rects.Clear();
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
        private bool CheckBottomLeftDiagonals(SolidColorBrush color)
        {
            for (int col = 0; col < 4; col++)
            {
                for (int row = 6; row > 3; row--)
                {
                    bool isSameColor = false;

                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)this.MainWindow.grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row - i && Grid.GetColumn(e) == col + i);
                            isSameColor = element!.Fill.Equals(color);
                            this.rects.Add(element);
                        });
                        if (!isSameColor)
                        {
                            this.rects.Clear();
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
        public bool CheckTopLeftDiagonals(SolidColorBrush color)
        {
            for (int col = 0; col < 4; col++)
            {
                for (int row = 1; row < 4; row++)
                {
                    bool isSameColor = false;
                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.Invoke(() => {
                            var element = (Rectangle?)this.MainWindow.grille.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == row + i && Grid.GetColumn(e) == col + i);
                            isSameColor = element!.Fill.Equals(color);
                            this.rects.Add(element);
                        });
                        if (!isSameColor)
                        {
                            this.rects.Clear();
                            break;
                        }
                    }
                    if (!isSameColor) continue;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Blinking animation for lines of 4 when received from Socket</summary>
        public void BlinkRectanglesWithDispatcher()
        {
            for (int i = 0; i < 4; i++)
            {
                foreach (Rectangle rect in this.rects)
                {
                    Dispatcher.Invoke(() =>
                    {
                        rect.Fill = Brushes.Black;
                    });
                }
                Thread.Sleep(500);
                foreach (Rectangle rect in this.rects)
                {
                    Dispatcher.Invoke(() =>
                    {
                        rect.Fill = Brushes.White;
                    });
                }
                Thread.Sleep(500);
            }
        }

        /// <summary>Blinking animation for lines of 4 when played</summary>
        public async void BlinkRectanglesWithoutDispatcher()
        {
            for (int i = 0; i < 4; i++)
            {
                foreach (Rectangle rect in this.rects)
                {
                    rect.Fill = Brushes.Black;
                }
                await Task.Delay(500);
                foreach (Rectangle rect in this.rects)
                {
                    rect.Fill = Brushes.White;
                }
                await Task.Delay(500);
            }
        }
    }
}
