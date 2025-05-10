using System;
using System.Collections.Generic;
using System.Linq;

namespace MorskoyBoy
{
    public class ComputerPlayer
    {
        private readonly Board board;
        private readonly Random random;
        private readonly List<Tuple<int, int>> possibleTargets;
        private readonly Stack<Tuple<int, int>> hitStack;
        private Tuple<int, int> firstHit;
        private (int, int) currentDirection;
        private bool isHunting;

        public ComputerPlayer(Board board)
        {
            this.board = board;
            this.random = new Random();
            this.possibleTargets = new List<Tuple<int, int>>();
            this.hitStack = new Stack<Tuple<int, int>>();
            InitializePossibleTargets();
        }

        private void InitializePossibleTargets()
        {
            possibleTargets.Clear();
            for (int row = 0; row < board.Size; row++)
            {
                for (int col = 0; col < board.Size; col++)
                {
                    if (!board.GetCell(row, col).IsHit)
                        possibleTargets.Add(Tuple.Create(row, col));
                }
            }
            Shuffle(possibleTargets);
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[j], list[i]) = (list[i], list[j]);
            }
        }

        public Tuple<int, int> MakeMove()
        {
            if (hitStack.Count > 0)
            {
                var target = hitStack.Pop();
                if (!board.GetCell(target.Item1, target.Item2).IsHit)
                    return target;
                return MakeMove();
            }

            if (isHunting)
                ClearHits();

            if (possibleTargets.Count == 0)
                InitializePossibleTargets();

            var newTarget = possibleTargets.FirstOrDefault(t => !board.GetCell(t.Item1, t.Item2).IsHit);
            if (newTarget != null)
            {
                possibleTargets.Remove(newTarget);
                return newTarget;
            }

            InitializePossibleTargets();
            return possibleTargets.Count > 0 ? possibleTargets[0] : Tuple.Create(0, 0);
        }

        public void RecordHit(int row, int col)
        {
            possibleTargets.RemoveAll(t => t.Item1 == row && t.Item2 == col);

            if (firstHit == null)
            {
                firstHit = Tuple.Create(row, col);
                isHunting = true;
                AddSmartTargetsAround(row, col);
            }
            else
            {
                if (firstHit.Item1 == row)
                    currentDirection = (0, col > firstHit.Item2 ? 1 : -1);
                else if (firstHit.Item2 == col)
                    currentDirection = (row > firstHit.Item1 ? 1 : -1, 0);

                AddTargetsInDirection(row, col);
            }
        }

        private void AddSmartTargetsAround(int row, int col)
        {
            foreach (var dir in new (int, int)[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
            {
                int newRow = row + dir.Item1;
                int newCol = col + dir.Item2;
                if (IsValidTarget(newRow, newCol))
                    hitStack.Push(Tuple.Create(newRow, newCol));
            }
        }

        private void AddTargetsInDirection(int row, int col)
        {
            if (currentDirection != (0, 0))
            {
                int newRow = row + currentDirection.Item1;
                int newCol = col + currentDirection.Item2;
                if (IsValidTarget(newRow, newCol))
                    hitStack.Push(Tuple.Create(newRow, newCol));

                newRow = firstHit.Item1 - currentDirection.Item1;
                newCol = firstHit.Item2 - currentDirection.Item2;
                if (IsValidTarget(newRow, newCol))
                    hitStack.Push(Tuple.Create(newRow, newCol));
            }
        }

        private bool IsValidTarget(int row, int col)
        {
            return row >= 0 && row < board.Size &&
                   col >= 0 && col < board.Size &&
                   !board.GetCell(row, col).IsHit &&
                   !hitStack.Any(t => t.Item1 == row && t.Item2 == col);
        }

        public void ClearHits()
        {
            hitStack.Clear();
            firstHit = null;
            isHunting = false;
            currentDirection = (0, 0);
        }
    }
}