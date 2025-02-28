using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MorskoyBoy
{
    public partial class MainWindow : Window
    {
        private Board playerBoard;
        private Board computerBoard;
        private GameLogic gameLogic;
        private bool isPlacingShips = true;
        private int currentShipSize = 1; // Начнем с самого маленького корабля
        private int[] shipCounts = { 4, 3, 2, 1 }; // Количество кораблей каждого размера
        private int currentShipIndex = 0; // Индекс текущего типа корабля

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            playerBoard = new Board(10);
            computerBoard = new Board(10);
            gameLogic = new GameLogic(playerBoard, computerBoard);

            // Инициализация UI для игровых полей
            InitializeBoardUI(PlayerBoard, playerBoard);
            InitializeBoardUI(ComputerBoard, computerBoard);
        }

        private void InitializeBoardUI(Grid boardGrid, Board board)
        {
            boardGrid.Children.Clear();
            for (int i = 0; i < board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    // Создание кнопок для каждой клетки поля
                    Button button = new Button();
                    button.Width = 40;
                    button.Height = 40;
                    button.Tag = new Tuple<int, int>(i, j);
                    button.Click += BoardButton_Click;
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    boardGrid.Children.Add(button);
                }
            }

            // Установка строк и столбцов для Grid
            for (int i = 0; i < board.Size; i++)
            {
                boardGrid.RowDefinitions.Add(new RowDefinition());
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        private void PlayerBoard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isPlacingShips)
            {
                Point clickPosition = e.GetPosition(PlayerBoard);
                int row = (int)(clickPosition.Y / 40);
                int col = (int)(clickPosition.X / 40);

                // Проверка возможности размещения корабля
                if (playerBoard.CanPlaceShip(row, col, currentShipSize, true))
                {
                    playerBoard.PlaceShip(row, col, currentShipSize, true);
                    UpdateBoardUI(PlayerBoard, playerBoard);

                    // Переходим к следующему кораблю
                    shipCounts[currentShipIndex]--;
                    if (shipCounts[currentShipIndex] == 0)
                    {
                        currentShipIndex++;
                        if (currentShipIndex < shipCounts.Length)
                        {
                            currentShipSize = currentShipIndex + 1;
                        }
                        else
                        {
                            isPlacingShips = false; // Закончили расстановку
                        }
                    }
                }
            }
        }

        private void SelectShip_Click(object sender, RoutedEventArgs e)
        {
            if (isPlacingShips)
            {
                Button button = sender as Button;
                currentShipSize = int.Parse(button.Tag.ToString());
                currentShipIndex = currentShipSize - 1;
            }
        }

        private void BoardButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlacingShips)
            {
                // Логика обработки хода
                MakeMove((Button)sender);
            }
        }

        private void MakeMove(Button button)
        {
            var position = (Tuple<int, int>)button.Tag;

            // Логика обработки хода
            bool isHit = gameLogic.MakeMove(position.Item1, position.Item2);
            button.Background = isHit ? Brushes.Red : Brushes.Blue;

            // Ход компьютера
            var computerMove = gameLogic.ComputerMakeMove();
            UpdateButton(PlayerBoard, computerMove.Item1, computerMove.Item2, computerMove.Item3);

            // Проверка на конец игры
            if (gameLogic.IsGameOver())
            {
                InfoTextBlock.Text = "Игра окончена!";
            }
        }

        private void UpdateButton(Grid boardGrid, int row, int col, bool isHit)
        {
            foreach (Button button in boardGrid.Children)
            {
                var position = (Tuple<int, int>)button.Tag;
                if (position.Item1 == row && position.Item2 == col)
                {
                    button.Background = isHit ? Brushes.Red : Brushes.Blue;
                    break;
                }
            }
        }

        private void UpdateBoardUI(Grid boardGrid, Board board)
        {
            foreach (Button button in boardGrid.Children)
            {
                var position = (Tuple<int, int>)button.Tag;
                var cell = board.GetCell(position.Item1, position.Item2);
                if (cell.IsOccupied)
                {
                    button.Background = Brushes.Gray;
                }
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            // Завершаем расстановку кораблей и начинаем игру
            isPlacingShips = false;
            InfoTextBlock.Text = "Игра началась!";
        }
    }
}
