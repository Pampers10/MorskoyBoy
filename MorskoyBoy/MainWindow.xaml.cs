    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    namespace MorskoyBoy
    {
        public partial class MainWindow : Window
        {
            private GameLogic gameLogic;
            private int selectedShipSize = 0;
            private bool isHorizontal = true;
            private bool isPlacingShips = true;

            public MainWindow()
            {
                InitializeComponent();
                InitializeGame();
                UpdateShipCountText();
            }

            private void InitializeGame()
            {
                gameLogic = new GameLogic(10);
                DrawBoard(PlayerBoard, gameLogic.PlayerBoard, true);
                DrawBoard(ComputerBoard, gameLogic.ComputerBoard, false);
            }

            private void DrawBoard(Grid grid, Board board, bool showShips)
            {
                grid.Children.Clear();
                grid.ColumnDefinitions.Clear();
                grid.RowDefinitions.Clear();

                for (int i = 0; i < board.Size; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.RowDefinitions.Add(new RowDefinition());
                }

                for (int row = 0; row < board.Size; row++)
                {
                    for (int col = 0; col < board.Size; col++)
                    {
                        var cell = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1),
                            Background = Brushes.Azure
                        };

                        Grid.SetRow(cell, row);
                        Grid.SetColumn(cell, col);
                        grid.Children.Add(cell);

                        var cellContent = new Grid();
                        cell.Child = cellContent;

                        if (showShips && board.GetCell(row, col).IsOccupied)
                        {
                            cellContent.Background = Brushes.DarkBlue;
                        }

                        if (board.GetCell(row, col).IsHit)
                        {
                            var marker = new Ellipse
                            {
                                Width = 20,
                                Height = 20,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                Fill = board.GetCell(row, col).IsOccupied ? Brushes.Red : Brushes.LightGray
                            };
                            cellContent.Children.Add(marker);
                        }
                    }
                }

                // Дополнительный проход для закрашивания клеток вокруг убитых кораблей
                for (int row = 0; row < board.Size; row++)
                {
                    for (int col = 0; col < board.Size; col++)
                    {
                        if (board.GetCell(row, col).IsOccupied &&
                            board.GetCell(row, col).IsHit &&
                            IsShipSunk(board, row, col))
                        {
                            MarkAdjacentCells(grid, board, row, col);
                        }
                    }
                }
            }

            private bool IsShipSunk(Board board, int row, int col)
            {
                var shipCells = FindShipCells(board, row, col);
                return shipCells.All(c => board.GetCell(c.Item1, c.Item2).IsHit);
            }

            private void MarkAdjacentCells(Grid grid, Board board, int row, int col)
            {
                var shipCells = FindShipCells(board, row, col);
                var cellsToMark = new HashSet<Tuple<int, int>>();

                foreach (var cell in shipCells)
                {
                    for (int r = -1; r <= 1; r++)
                    {
                        for (int c = -1; c <= 1; c++)
                        {
                            int newRow = cell.Item1 + r;
                            int newCol = cell.Item2 + c;

                            if (newRow >= 0 && newRow < board.Size &&
                                newCol >= 0 && newCol < board.Size &&
                                !shipCells.Any(sc => sc.Item1 == newRow && sc.Item2 == newCol))
                            {
                                cellsToMark.Add(Tuple.Create(newRow, newCol));
                            }
                        }
                    }
                }

                foreach (var cell in cellsToMark)
                {
                    var gridCell = GetCellFromGrid(grid, cell.Item1, cell.Item2);
                    if (gridCell is Border border && border.Child is Grid cellContent)
                    {
                        if (!cellContent.Children.OfType<Rectangle>().Any())
                        {
                            var marker = new Rectangle
                            {
                                Fill = Brushes.LightGray,
                                Opacity = 0.7,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch
                            };
                            cellContent.Children.Add(marker);
                            board.GetCell(cell.Item1, cell.Item2).IsHit = true;
                        }
                    }
                }
            }

            private List<Tuple<int, int>> FindShipCells(Board board, int row, int col)
            {
                var cells = new List<Tuple<int, int>>();
                if (!board.GetCell(row, col).IsOccupied)
                    return cells;

                var visited = new HashSet<Tuple<int, int>>();
                var queue = new Queue<Tuple<int, int>>();
                queue.Enqueue(Tuple.Create(row, col));
                visited.Add(Tuple.Create(row, col));

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    cells.Add(current);

                    foreach (var dir in new (int, int)[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                    {
                        int newRow = current.Item1 + dir.Item1;
                        int newCol = current.Item2 + dir.Item2;

                        if (newRow >= 0 && newRow < board.Size &&
                            newCol >= 0 && newCol < board.Size &&
                            board.GetCell(newRow, newCol).IsOccupied &&
                            !visited.Contains(Tuple.Create(newRow, newCol)))
                        {
                            visited.Add(Tuple.Create(newRow, newCol));
                            queue.Enqueue(Tuple.Create(newRow, newCol));
                        }
                    }
                }

                return cells;
            }

            private UIElement GetCellFromGrid(Grid grid, int row, int col)
            {
                foreach (UIElement element in grid.Children)
                {
                    if (Grid.GetRow(element) == row && Grid.GetColumn(element) == col)
                    {
                        return element;
                    }
                }
                return null;
            }

            private void PlayerBoard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (!isPlacingShips || selectedShipSize == 0) return;

                var point = e.GetPosition(PlayerBoard);
                int row = (int)(point.Y / (PlayerBoard.ActualHeight / gameLogic.PlayerBoard.Size));
                int col = (int)(point.X / (PlayerBoard.ActualWidth / gameLogic.PlayerBoard.Size));

                if (gameLogic.PlacePlayerShip(row, col, selectedShipSize, isHorizontal))
                {
                    DrawBoard(PlayerBoard, gameLogic.PlayerBoard, true);
                    UpdateShipCountText();
                }
                else
                {
                    MessageBox.Show("Нельзя разместить корабль здесь! Корабли должны находиться на расстоянии минимум 1 клетки друг от друга.",
                        "Ошибка размещения", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            private async void ComputerBoard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (isPlacingShips || !gameLogic.IsPlayerTurn || gameLogic.IsGameOver)
                    return;

                var point = e.GetPosition(ComputerBoard);
                int row = (int)(point.Y / (ComputerBoard.ActualHeight / gameLogic.ComputerBoard.Size));
                int col = (int)(point.X / (ComputerBoard.ActualWidth / gameLogic.ComputerBoard.Size));

                if (gameLogic.ComputerBoard.GetCell(row, col).IsHit)
                {
                    InfoTextBlock.Text = "Вы уже стреляли в эту клетку!";
                    return;
                }

                bool isHit = gameLogic.MakePlayerMove(row, col);
                DrawBoard(ComputerBoard, gameLogic.ComputerBoard, false);

                if (isHit)
                {
                    InfoTextBlock.Text = "Попадание!";

                    if (IsShipSunk(gameLogic.ComputerBoard, row, col))
                    {
                        InfoTextBlock.Text += " Корабль уничтожен!";
                        DrawBoard(ComputerBoard, gameLogic.ComputerBoard, false);
                    }

                    if (gameLogic.ComputerBoard.IsAllShipsSunk())
                    {
                        EndGame(true);
                        return;
                    }
                }
                else
                {
                    InfoTextBlock.Text = "Промах! Ход компьютера.";
                    await Task.Delay(500);
                    await ComputerTurnAsync();
                }
            }

            private async Task ComputerTurnAsync()
            {
                int maxComputerMoves = 10;
                while (!gameLogic.IsPlayerTurn && !gameLogic.IsGameOver && maxComputerMoves-- > 0)
                {
                    var computerMove = gameLogic.MakeComputerMove();
                    if (computerMove == null) break;

                    InfoTextBlock.Text += $"\nКомпьютер стреляет в {computerMove.Item1 + 1},{computerMove.Item2 + 1} - " +
                                        $"{(computerMove.Item3 ? "Попадание!" : "Промах!")}";

                    if (computerMove.Item3 && IsShipSunk(gameLogic.PlayerBoard, computerMove.Item1, computerMove.Item2))
                    {
                        InfoTextBlock.Text += " Корабль уничтожен!";
                    }

                    DrawBoard(PlayerBoard, gameLogic.PlayerBoard, true);

                    if (gameLogic.PlayerBoard.IsAllShipsSunk())
                    {
                        EndGame(false);
                        break;
                    }

                    if (!computerMove.Item3) break;

                    await Task.Delay(500);
                }
            }

            private void SelectShip_Click(object sender, RoutedEventArgs e)
            {
                selectedShipSize = int.Parse((sender as Button).Tag.ToString());
            }

            private void ToggleOrientation_Click(object sender, RoutedEventArgs e)
            {
                isHorizontal = !isHorizontal;
                ToggleOrientationButton.Content = isHorizontal ? "Горизонтально" : "Вертикально";
            }

            private void StartGame_Click(object sender, RoutedEventArgs e)
            {
                if (gameLogic.StartGame())
                {
                    isPlacingShips = false;
                    gameLogic.PlaceComputerShips();
                    InfoTextBlock.Text = "Игра началась! Ваш ход.";
                    ToggleOrientationButton.IsEnabled = false;

                    // Явно отключаем кнопки выбора кораблей
                    Button1Cell.IsEnabled = false;
                    Button2Cell.IsEnabled = false;
                    Button3Cell.IsEnabled = false;
                    Button4Cell.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("Разместите все корабли перед началом игры!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            private void UpdateShipCountText()
            {
                var counts = gameLogic.GetPlayerShipCounts();
                PlayerShipCountText.Text = $"Осталось разместить:\n" +
                                         $"1-клеточных: {counts[0]}\n" +
                                         $"2-клеточных: {counts[1]}\n" +
                                         $"3-клеточных: {counts[2]}\n" +
                                         $"4-клеточных: {counts[3]}";
            }

            private void EndGame(bool playerWon)
            {
                string message = playerWon ? "Поздравляем! Вы победили!" : "Вы проиграли! Попробуйте еще раз.";
                MessageBox.Show(message, "Игра окончена", MessageBoxButton.OK, MessageBoxImage.Information);

                InitializeGame();
                selectedShipSize = 0;
                isHorizontal = true;
                isPlacingShips = true;
                InfoTextBlock.Text = "";
                ToggleOrientationButton.IsEnabled = true;
                ToggleOrientationButton.Content = "Горизонтально";
                UpdateShipCountText();

                // Явно включаем кнопки выбора кораблей по именам
                Button1Cell.IsEnabled = true;
                Button2Cell.IsEnabled = true;
                Button3Cell.IsEnabled = true;
                Button4Cell.IsEnabled = true;
            }
        }
    }