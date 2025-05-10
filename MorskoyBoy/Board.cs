namespace MorskoyBoy
{
    public class Board
    {
        public int Size { get; }
        private readonly Cell[,] cells;

        public Board(int size)
        {
            Size = size;
            cells = new Cell[size, size];
            InitializeBoard();
        }

        public void InitializeBoard()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    cells[i, j] = new Cell();
                }
            }
        }

        public bool CanPlaceShip(int row, int col, int size, bool isHorizontal)
        {
            if (!IsIndexValid(row, col)) return false;

            for (int i = -1; i <= (isHorizontal ? size : 1); i++)
            {
                for (int j = -1; j <= (isHorizontal ? 1 : size); j++)
                {
                    int r = isHorizontal ? row + j : row + i;
                    int c = isHorizontal ? col + i : col + j;

                    if (IsIndexValid(r, c) && cells[r, c].IsOccupied)
                        return false;
                }
            }
            return true;
        }

        public void PlaceShip(int row, int col, int size, bool isHorizontal)
        {
            if (isHorizontal && col + size > Size) return;
            if (!isHorizontal && row + size > Size) return;

            for (int i = 0; i < size; i++)
            {
                int r = isHorizontal ? row : row + i;
                int c = isHorizontal ? col + i : col;

                if (IsIndexValid(r, c))
                    cells[r, c].IsOccupied = true;
            }
        }

        public bool MakeMove(int row, int col)
        {
            if (cells[row, col].IsHit) return false;
            cells[row, col].IsHit = true;
            return cells[row, col].IsOccupied;
        }

        public bool IsAllShipsSunk()
        {
            foreach (var cell in cells)
            {
                if (cell.IsOccupied && !cell.IsHit)
                    return false;
            }
            return true;
        }

        public Cell GetCell(int row, int col) => cells[row, col];

        private bool IsIndexValid(int row, int col)
        {
            return row >= 0 && row < Size && col >= 0 && col < Size;
        }
    }

    public class Cell
    {
        public bool IsOccupied { get; set; }
        public bool IsHit { get; set; }
    }
}