using System;

namespace MorskoyBoy
{
    public class Board
    {
        public int Size { get; private set; }
        private Cell[,] cells;

        public Board(int size)
        {
            Size = size;
            cells = new Cell[size, size];
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    cells[i, j] = new Cell();
                }
            }
        }

        public bool MakeMove(int row, int col)
        {
            if (cells[row, col].IsOccupied)
            {
                cells[row, col].IsHit = true;
                return true;
            }
            return false;
        }

        public bool IsAllShipsSunk()
        {
            foreach (var cell in cells)
            {
                if (cell.IsOccupied && !cell.IsHit)
                {
                    return false;
                }
            }
            return true;
        }

        public Cell GetCell(int row, int col)
        {
            return cells[row, col];
        }

        public bool CanPlaceShip(int startRow, int startCol, int size, bool isHorizontal)
        {
            // Проверка возможности размещения корабля
            if (isHorizontal)
            {
                if (startCol + size > Size) return false;
                for (int i = 0; i < size; i++)
                {
                    if (cells[startRow, startCol + i].IsOccupied) return false;
                }
            }
            else
            {
                if (startRow + size > Size) return false;
                for (int i = 0; i < size; i++)
                {
                    if (cells[startRow + i, startCol].IsOccupied) return false;
                }
            }
            return true;
        }

        public void PlaceShip(int startRow, int startCol, int size, bool isHorizontal)
        {
            if (isHorizontal)
            {
                for (int i = 0; i < size; i++)
                {
                    cells[startRow, startCol + i].IsOccupied = true;
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    cells[startRow + i, startCol].IsOccupied = true;
                }
            }
        }
    }

    public class Cell
    {
        public bool IsOccupied { get; set; }
        public bool IsHit { get; set; }
    }
}
