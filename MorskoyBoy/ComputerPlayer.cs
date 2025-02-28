using System;

namespace MorskoyBoy
{
    public class ComputerPlayer
    {
        private Board board;
        private Random random;

        public ComputerPlayer(Board board)
        {
            this.board = board;
            this.random = new Random();
        }

        public Tuple<int, int> MakeMove()
        {
            int row, col;
            do
            {
                row = random.Next(board.Size);
                col = random.Next(board.Size);
            } while (board.GetCell(row, col).IsHit);

            return new Tuple<int, int>(row, col);
        }
    }
}
