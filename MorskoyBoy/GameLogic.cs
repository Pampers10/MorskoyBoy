using System;
using System.Linq;

namespace MorskoyBoy
{
    public class GameLogic
    {
        public Board PlayerBoard { get; }
        public Board ComputerBoard { get; }
        public bool IsGameOver { get; private set; }
        public bool IsPlayerTurn { get; set; }
        private readonly ComputerPlayer computerPlayer;
        private readonly int[] shipCounts = { 4, 3, 2, 1 };

        public GameLogic(int boardSize)
        {
            PlayerBoard = new Board(boardSize);
            ComputerBoard = new Board(boardSize); // Гарантированная инициализация
            computerPlayer = new ComputerPlayer(ComputerBoard);
            IsPlayerTurn = true;
            IsGameOver = false;
            shipCounts = new int[] { 4, 3, 2, 1 }; // Явная инициализация
        }

        public bool PlacePlayerShip(int row, int col, int size, bool isHorizontal)
        {
            if (size < 1 || size > 4 || shipCounts[size - 1] <= 0)
                return false;

            if (PlayerBoard.CanPlaceShip(row, col, size, isHorizontal))
            {
                PlayerBoard.PlaceShip(row, col, size, isHorizontal);
                shipCounts[size - 1]--;
                return true;
            }
            return false;
        }

        public void PlaceComputerShips()
        {
            int[] shipSizes = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
            var random = new Random();
            const int maxAttempts = 1000;

            foreach (int size in shipSizes)
            {
                int attempts = 0;
                bool placed = false;

                while (!placed && attempts < maxAttempts)
                {
                    attempts++;
                    int row = random.Next(ComputerBoard.Size);
                    int col = random.Next(ComputerBoard.Size);
                    bool isHorizontal = random.Next(2) == 0;

                    if (isHorizontal && col + size > ComputerBoard.Size) continue;
                    if (!isHorizontal && row + size > ComputerBoard.Size) continue;

                    if (ComputerBoard.CanPlaceShip(row, col, size, isHorizontal))
                    {
                        ComputerBoard.PlaceShip(row, col, size, isHorizontal);
                        placed = true;
                    }
                }

                if (!placed)
                {
                    ComputerBoard.InitializeBoard();
                    PlaceComputerShips();
                    return;
                }
            }
        }

        public bool MakePlayerMove(int row, int col)
        {
            if (!IsPlayerTurn || IsGameOver || ComputerBoard.GetCell(row, col).IsHit)
                return false;

            bool isHit = ComputerBoard.MakeMove(row, col);

            if (ComputerBoard.IsAllShipsSunk())
                IsGameOver = true;
            else if (!isHit)
                IsPlayerTurn = false;

            return isHit;
        }

        public Tuple<int, int, bool> MakeComputerMove()
        {
            if (IsPlayerTurn || IsGameOver) return null;

            var move = computerPlayer.MakeMove();
            if (move == null) return null;

            int row = move.Item1;
            int col = move.Item2;

            bool isHit = PlayerBoard.MakeMove(row, col);

            if (isHit)
            {
                computerPlayer.RecordHit(row, col);
                if (IsShipSunk(row, col))
                    computerPlayer.ClearHits();

                if (PlayerBoard.IsAllShipsSunk())
                    IsGameOver = true;
            }
            else
            {
                IsPlayerTurn = true;
            }

            return Tuple.Create(row, col, isHit);
        }

        private bool IsShipSunk(int row, int col)
        {
            var shipCells = FindShipCells(row, col);
            return shipCells.All(c => PlayerBoard.GetCell(c.Item1, c.Item2).IsHit);
        }

        private System.Collections.Generic.List<Tuple<int, int>> FindShipCells(int row, int col)
        {
            var cells = new System.Collections.Generic.List<Tuple<int, int>>();
            if (!PlayerBoard.GetCell(row, col).IsOccupied)
                return cells;

            bool isHorizontal = true;
            if ((row > 0 && PlayerBoard.GetCell(row - 1, col).IsOccupied) ||
                (row < PlayerBoard.Size - 1 && PlayerBoard.GetCell(row + 1, col).IsOccupied))
            {
                isHorizontal = false;
            }

            if (isHorizontal)
            {
                int left = col;
                while (left > 0 && PlayerBoard.GetCell(row, left - 1).IsOccupied) left--;

                int right = col;
                while (right < PlayerBoard.Size - 1 && PlayerBoard.GetCell(row, right + 1).IsOccupied) right++;

                for (int c = left; c <= right; c++)
                    cells.Add(Tuple.Create(row, c));
            }
            else
            {
                int top = row;
                while (top > 0 && PlayerBoard.GetCell(top - 1, col).IsOccupied) top--;

                int bottom = row;
                while (bottom < PlayerBoard.Size - 1 && PlayerBoard.GetCell(bottom + 1, col).IsOccupied) bottom++;

                for (int r = top; r <= bottom; r++)
                    cells.Add(Tuple.Create(r, col));
            }

            return cells;
        }

        public bool StartGame()
        {
            return shipCounts.All(count => count == 0);
        }

        public int[] GetPlayerShipCounts()
        {
            return (int[])shipCounts.Clone();
        }


    }
}