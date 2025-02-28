using System;

namespace MorskoyBoy
{
    public class GameLogic
    {
        private Board playerBoard;
        private Board computerBoard;
        private ComputerPlayer computerPlayer;

        public GameLogic(Board playerBoard, Board computerBoard)
        {
            this.playerBoard = playerBoard;
            this.computerBoard = computerBoard;
            this.computerPlayer = new ComputerPlayer(computerBoard);
        }

        public bool MakeMove(int row, int col)
        {
            return computerBoard.MakeMove(row, col);
        }

        public Tuple<int, int, bool> ComputerMakeMove()
        {
            var move = computerPlayer.MakeMove();
            bool isHit = playerBoard.MakeMove(move.Item1, move.Item2);
            return new Tuple<int, int, bool>(move.Item1, move.Item2, isHit);
        }

        public bool IsGameOver()
        {
            return playerBoard.IsAllShipsSunk() || computerBoard.IsAllShipsSunk();
        }
    }
}
