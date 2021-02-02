using System;
using System.Text;
using Newtonsoft.Json;
using Stl.Time;

namespace Samples.BoardGames.Abstractions
{
    public interface IBoardGame
    {
        string Title { get; }
        int PlayerCount { get; }

        GameState New();
        GameState Move(GameState state, GameMove move);
    }

    public abstract class BoardGame<TGameState, TGameMove> : IBoardGame
        where TGameState : GameState
        where TGameMove : GameMove
    {
        public abstract string Type { get; }
        public abstract string Title { get; }
        public abstract int PlayerCount { get; }

        public abstract TGameState New();
        public abstract TGameState Move(TGameState state, TGameMove move);

        GameState IBoardGame.New() => New();
        GameState IBoardGame.Move(GameState state, GameMove move)
            => Move((TGameState) state, (TGameMove) move);
    }

    public abstract record GameMove
    {
        public Moment Time { get; init; }
    }

    public record GameState
    {
        public GameBoard Board { get; init; } = null!;
        public int MoveIndex { get; init; }
        public bool IsGameEnded { get; init; }
    }

    public record GameBoard
    {
        public int Size { get; }
        public string Cells { get; }

        public char this[int r, int c] {
            get {
                var cellIndex = GetCellIndex(r, c);
                if (cellIndex < 0 || cellIndex >= Cells.Length)
                    return ' ';
                return Cells[cellIndex];
            }
        }

        public string this[int r] {
            get {
                var startIndex = GetCellIndex(r, 0);
                if (startIndex < 0 || startIndex >= Cells.Length)
                    return "";
                return Cells.Substring(startIndex, Size);
            }
        }

        public GameBoard(int size)
        {
            if (size < 1)
                throw new ArgumentOutOfRangeException(nameof(size));
            Size = size;
            Cells = new string(' ', size * size);
        }

        [JsonConstructor]
        public GameBoard(int size, string cells)
        {
            if (size < 1)
                throw new ArgumentOutOfRangeException(nameof(size));
            if (size * size != cells.Length)
                throw new ArgumentOutOfRangeException(nameof(size));
            Size = size;
            Cells = cells;
        }

        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.AppendFormat("Cells[{0}x{0}] = [\r\n", Size);
            for (var rowIndex = 0; rowIndex < Size; rowIndex++)
                builder.AppendFormat("  |{0}|\r\n", this[rowIndex]);
            builder.Append("  ]");
            return true;
        }

        public int GetCellIndex(int r, int c) => r * Size + c;

        public GameBoard Set(int r, int c, char value)
        {
            if (r < 0 || r >= Size)
                throw new ArgumentOutOfRangeException(nameof(r));
            if (c < 0 || c >= Size)
                throw new ArgumentOutOfRangeException(nameof(c));
            var cellIndex = GetCellIndex(r, c);
            var newCells = Cells.Substring(0, cellIndex) + c + Cells.Substring(cellIndex + 1);
            return new GameBoard(Size, newCells);
        }
    }
}
