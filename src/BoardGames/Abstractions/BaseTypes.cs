using System;
using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;
using Stl.Time;

namespace Samples.BoardGames.Abstractions
{
    public interface IGameEngine
    {
        string Id { get; }
        string Title { get; }
        string Icon { get; }
        int MinPlayerCount { get; }
        int MaxPlayerCount { get; }
        bool AutoStart { get; }

        GameState New();
        GameState Move(GameState state, GameMove move);
    }

    public abstract class GameEngine<TGameState, TGameMove> : IGameEngine
        where TGameState : GameState
        where TGameMove : GameMove
    {
        public abstract string Id { get; }
        public abstract string Title { get; }
        public abstract string Icon { get; }
        public abstract int MinPlayerCount { get; }
        public abstract int MaxPlayerCount { get; }
        public abstract bool AutoStart { get; }

        public abstract TGameState New();
        public abstract TGameState Move(TGameState state, TGameMove move);

        GameState IGameEngine.New() => New();
        GameState IGameEngine.Move(GameState state, GameMove move)
            => Move((TGameState) state, (TGameMove) move);
    }

    public abstract record GameMove(Moment Time)
    {
        protected GameMove() : this(default(Moment)) { }
    }

    public record GameState
    {
        public long[] PlayerScores { get; init; } = Array.Empty<long>();
        public bool IsGameEnded => !string.IsNullOrEmpty(GameEndMessage);
        public string GameEndMessage { get; init; } = "";
    }

    public record CharBoard
    {
        private static readonly ConcurrentDictionary<int, CharBoard> EmptyCache = new();
        public static CharBoard Empty(int size) => EmptyCache.GetOrAdd(size, size1 => new CharBoard(size1));

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

        public CharBoard(int size)
        {
            if (size < 1)
                throw new ArgumentOutOfRangeException(nameof(size));
            Size = size;
            Cells = new string(' ', size * size);
        }

        [JsonConstructor]
        public CharBoard(int size, string cells)
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

        public CharBoard Set(int r, int c, char value)
        {
            if (r < 0 || r >= Size)
                throw new ArgumentOutOfRangeException(nameof(r));
            if (c < 0 || c >= Size)
                throw new ArgumentOutOfRangeException(nameof(c));
            var cellIndex = GetCellIndex(r, c);
            var newCells = Cells.Substring(0, cellIndex) + value + Cells.Substring(cellIndex + 1);
            return new CharBoard(Size, newCells);
        }
    }
}
