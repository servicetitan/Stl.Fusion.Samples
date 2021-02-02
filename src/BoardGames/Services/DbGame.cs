using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Samples.BoardGames.Abstractions;
using Stl.Serialization;
using Stl.Time;

namespace Samples.BoardGames.Services
{
    [Table("Games")]
    [Index(nameof(Stage), nameof(CreatedAt))]
    [Index(nameof(Stage), nameof(StartedAt))]
    [Index(nameof(Stage), nameof(EndedAt))]
    public class DbGame
    {
        private readonly JsonSerialized<GameState?> _state = new(default(GameState?));
        private DateTime _createdAt;
        private DateTime? _startedAt;
        private DateTime? _endedAt;

        [Key] public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public bool IsPublic { get; set; }

        public List<DbGamePlayer> Players { get; } = new();
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

        public void UpdateStage()
            => Stage = EndedAt.HasValue ? GameStage.Ended
                : StartedAt.HasValue ? GameStage.Running
                : GameStage.Created;
    }
}
