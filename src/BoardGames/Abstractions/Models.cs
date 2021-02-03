using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reactive;
using Stl.Fusion.Authentication;

namespace Samples.BoardGames.Abstractions
{
    // API-level entities

    public enum GameStage
    {
        Created = 0,
        Running = 1,
        Ended = 0x10,
    }

    public record Game
    {
        public record CreateCommand(Session Session, string Type) : ISessionCommand<Game> {
            public CreateCommand() : this(Session.Null, "") { }
        }
        public record JoinCommand(Session Session, string Id) : ISessionCommand<Unit> {
            public JoinCommand() : this(Session.Null, "") { }
        }
        public record StartCommand(Session Session, string Id) : ISessionCommand<Unit> {
            public StartCommand() : this(Session.Null, "") { }
        }
        public record MoveCommand(Session Session, string Id, GameMove Move) : ISessionCommand<Unit> {
            public MoveCommand() : this(Session.Null, "", null!) { }
        }
        public record EditCommand(Session Session, string Id, bool IsPublic) : ISessionCommand<Unit> {
            public EditCommand() : this(Session.Null, "", false) { }
        }

        public string Id { get; init; } = "";
        public string EngineId { get; init; } = "";
        public long UserId { get; init; }
        public bool IsPublic { get; init; }
        public ImmutableList<GamePlayer> Players { get; init; } = ImmutableList<GamePlayer>.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? StartedAt { get; init; }
        public DateTime? EndedAt { get; init; }
        public GameStage Stage { get; init; }
        public GameState? State { get; init; }
        public string GameEndMessage { get; init; } = "";
    }

    public record GamePlayer
    {
        public long UserId { get; set; }
        public long Score { get; set; }
    }
}
