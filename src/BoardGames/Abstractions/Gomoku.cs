using System;
using System.Linq;
using Stl.Time;

namespace Samples.BoardGames.Abstractions
{
    public record GomokuState(CharBoard Board, int MoveIndex = 0) : GameState
    {
        public GomokuState() : this((CharBoard) null!) { }
    }

    public record GomokuMove(int PlayerIndex, int Row, int Column, Moment Time = default) : GameMove(Time)
    {
        public GomokuMove() : this(0, 0, 0) { }
    }

    public class GomokuEngine : GameEngine<GomokuState, GomokuMove>
    {
        public static int BoardSize { get; } = 19;

        public override string Id => "gomoku";
        public override string Title => "Gomoku (Five in a Row)";
        public override string Icon => "fa-border-all";
        public override int MinPlayerCount => 2;
        public override int MaxPlayerCount => 2;
        public override bool AutoStart => true;

        public override GomokuState New()
            => new(CharBoard.Empty(BoardSize)) {
                PlayerScores = new long[2],
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
                    GameEndMessage = $"|@p{move.PlayerIndex}| won.",
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
}
