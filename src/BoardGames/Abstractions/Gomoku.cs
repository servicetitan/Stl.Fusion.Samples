using System;
using System.Linq;

namespace Samples.BoardGames.Abstractions
{
    public class Gomoku : BoardGame<GomokuState, GomokuMove>
    {
        public static int BoardSize { get; } = 19;
        public override string Type => "Gomoku";
        public override string Title => "Gomoku (Five in a Row)";
        public override int PlayerCount => 2;

        public override GomokuState New()
            => new() { Board = new GameBoard(BoardSize) };

        public override GomokuState Move(GomokuState state, GomokuMove move)
        {
            if (state.IsGameEnded)
                throw new ApplicationException("Game is already ended.");
            if (move.PlayerIndex != state.MoveIndex % 2)
                throw new ApplicationException("It's another player's turn.");
            var board = state.Board;
            if (board[move.Row, move.Column] != ' ')
                throw new ApplicationException("The cell is already occupied.");
            var nextBoard = board.Set(move.Row, move.Column, GetPlayerMarker(move.PlayerIndex));
            return new GomokuState() {
                Board = nextBoard,
                MoveIndex = state.MoveIndex + 1,
                IsGameEnded = CheckGameEnded(nextBoard, move),
            };
        }

        public char GetPlayerMarker(int playerIndex)
            => playerIndex == 0 ? 'X' : 'O';

        private bool CheckGameEnded(GameBoard board, GomokuMove lastMove)
        {
            var marker = GetPlayerMarker(lastMove.PlayerIndex);
            int Count(int dr, int dc)
                => Enumerable.Range(0, 5)
                    .Select(i => board[lastMove.Row + dr * i, lastMove.Column + dc * i])
                    .TakeWhile(c => c == marker)
                    .Take(5)
                    .Count();
            int SymmetricCount(int dr, int dc)
                => Count(dr, dc) + Count(-dr, -dc) - 1;
            bool IsGameEnded(int dr, int dc)
                => SymmetricCount(dr, dc) >= 5;
            return IsGameEnded(0, 1) || IsGameEnded(1, 0) || IsGameEnded(1, 1) || IsGameEnded(-1, 1);
        }
    }

    public record GomokuState : GameState { }

    public record GomokuMove : GameMove
    {
        public int Row { get; init; }
        public int Column { get; init; }
        public int PlayerIndex { get; init; }
    }
}
