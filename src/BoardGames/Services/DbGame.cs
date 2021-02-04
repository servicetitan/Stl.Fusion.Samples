using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Samples.BoardGames.Abstractions;
using Stl.Collections;
using Stl.Serialization;
using Stl.Time;

namespace Samples.BoardGames.Services
{
    [Table("Games")]
    [Index(nameof(Stage), nameof(IsPublic), nameof(CreatedAt))]
    [Index(nameof(UserId), nameof(Stage), nameof(CreatedAt))]
    [Index(nameof(UserId), nameof(CreatedAt), nameof(Stage))]
    public class DbGame
    {
        private readonly JsonSerialized<GameState?> _state = new(default(GameState?));
        private DateTime _createdAt;
        private DateTime? _startedAt;
        private DateTime? _endedAt;

        [Key] public string Id { get; set; } = "";
        public string EngineId { get; set; } = "";
        public long UserId { get; set; }
        public bool IsPublic { get; set; }

        public List<DbGamePlayer> Players { get; set; } = new();
        public string StateJson { get; set; } = "";

        public DateTime CreatedAt {
            get => _createdAt.DefaultKind(DateTimeKind.Utc);
            set => _createdAt = value.DefaultKind(DateTimeKind.Utc);
        }

        public DateTime? StartedAt {
            get => _startedAt.DefaultKind(DateTimeKind.Utc);
            set => _startedAt = value.DefaultKind(DateTimeKind.Utc);
        }

        public DateTime? EndedAt {
            get => _endedAt.DefaultKind(DateTimeKind.Utc);
            set => _endedAt = value.DefaultKind(DateTimeKind.Utc);
        }

        [NotMapped, JsonIgnore]
        public GameState? State {
            get => _state.Value;
            set => _state.Value = value;
        }

        public GameStage Stage { get; set; }
        public string GameEndMessage { get; set; } = "";

        public Game ToModel()
            => new() {
                Id = Id,
                EngineId = EngineId,
                UserId = UserId,
                IsPublic = IsPublic,
                CreatedAt = CreatedAt,
                StartedAt = StartedAt,
                EndedAt = EndedAt,
                Stage = Stage,
                GameEndMessage = GameEndMessage,
                Players = Players.OrderBy(p => p.Index).Select(p => p.ToModel()).ToImmutableList(),
            };

        public void UpdateFrom(Game game)
        {
            EngineId = game.EngineId;
            UserId = game.UserId;
            IsPublic = game.IsPublic;
            CreatedAt = game.CreatedAt;
            StartedAt = game.StartedAt;
            EndedAt = game.EndedAt;
            Stage = game.Stage;
            GameEndMessage = game.GameEndMessage;

            var players = game.Players.ToDictionary(p => p.UserId);
            var dbPlayers = Players.Where(p => players.ContainsKey(p.UserId)).ToDictionary(p => p.UserId);
            Players = new List<DbGamePlayer>();
            var playerIndex = 0;
            foreach (var player in game.Players) {
                var dbPlayer = dbPlayers.GetValueOrDefault(player.UserId) ?? new DbGamePlayer();
                dbPlayer.UpdateFrom(player, game, playerIndex);
                Players.Add(dbPlayer);
                playerIndex++;
            }
        }
    }
}
