using System;
using System.Linq;

namespace Samples.BoardGames.Abstractions
{
    public class GomokuEngine : GameEngine<GomokuState, GomokuMove>
    {
        public static int BoardSize { get; } = 19;
        public override string Id => "Gomoku";
        public override string Title => "Gomoku (Five in a Row)";
        public override int MinPlayerCount => 2;
        public override int MaxPlayerCount => 2;

        public override GomokuState New()
            => new() {
                PlayerScores = new long[2],
                Board = new CharBoard(BoardSize)
            };

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
            state = state with {
                Board = nextBoard,
                MoveIndex = state.MoveIndex + 1,
            };
            if (CheckGameEnded(nextBoard, move)) {
                var playerScores = state.PlayerScores.ToArray();
                playerScores[move.PlayerIndex] = 1;
                state = state with {
                    GameEndMessage = $"@Player{move.PlayerIndex} won.",
                    PlayerScores = playerScores,
                };
            }
            return state;
        }

        public char GetPlayerMarker(int playerIndex)
            => playerIndex == 0 ? 'X' : 'O';

        private bool CheckGameEnded(CharBoard board, GomokuMove lastMove)
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

    public record GomokuState : GameState
    {
        public int MoveIndex { get; init; }
        public CharBoard Board { get; init; }
    }

    public record GomokuMove : GameMove
    {
        public int Row { get; init; }
        public int Column { get; init; }
        public int PlayerIndex { get; init; }
    }
}
