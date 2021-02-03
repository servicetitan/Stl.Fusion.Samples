using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;
using Samples.BoardGames.Abstractions;
using Stl.Async;
using Stl.CommandR;
using Stl.Fusion.Authentication;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.Operations;

namespace Samples.BoardGames.Services
{
    [ComputeService(typeof(IGameService))]
    public class GameService : DbServiceBase<AppDbContext>, IGameService
    {
        protected Dictionary<string, IGameEngine> GameEngines { get; }
        protected IAuthService AuthService { get; }
        protected DbEntityResolver<AppDbContext, string, DbGame> GameResolver { get; }

        public GameService(IServiceProvider services) : base(services)
        {
            GameEngines = services.GetRequiredService<IEnumerable<IGameEngine>>().ToDictionary(e => e.Id);
            AuthService = services.GetRequiredService<IAuthService>();
            GameResolver = services.GetRequiredService<DbEntityResolver<AppDbContext, string, DbGame>>();
        }

        // Commands

        public virtual async Task<Game> CreateAsync(Game.CreateCommand command, CancellationToken cancellationToken = default)
        {
            var (session, engineId) = command;
            var engine = GameEngines[engineId]; // Just to check it exists
            var context = CommandContext.GetCurrent();
            if (Computed.IsInvalidating()) {
                var invGameId = context.Items.Get<OperationItem<string>>().Value;
                FindAsync(invGameId, session, default).Ignore();
                return default!;
            }

            var user = await AuthService.GetUserAsync(session, cancellationToken).ConfigureAwait(false);
            user = user.MustBeAuthenticated();
            var userId = long.Parse(user.Id);

            await using var dbContext = await CreateCommandDbContextAsync(cancellationToken).ConfigureAwait(false);

            var game = new Game() {
                Id = Ulid.NewUlid().ToString(),
                EngineId = engineId,
                UserId = userId,
                CreatedAt = Clock.Now,
                Stage = GameStage.Created,
                Players = ImmutableList<GamePlayer>.Empty.Add(new GamePlayer() { UserId = userId })
            };
            var dbGame = new DbGame();
            dbGame.UpdateFrom(game);
            dbContext.Add(game);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            context.Items.Set(OperationItem.New(game.Id));
            return game;
        }

        public virtual async Task JoinAsync(Game.JoinCommand command, CancellationToken cancellationToken = default)
        {
            var (session, id) = command;
            var context = CommandContext.GetCurrent();
            if (Computed.IsInvalidating()) {
                var invGameId = context.Items.Get<OperationItem<string>>().Value;
                FindAsync(invGameId, session, default).Ignore();
                return;
            }

            var user = await AuthService.GetUserAsync(session, cancellationToken).ConfigureAwait(false);
            user = user.MustBeAuthenticated();
            var userId = long.Parse(user.Id);

            await using var dbContext = await CreateCommandDbContextAsync(cancellationToken).ConfigureAwait(false);
            var dbGame = await GetDbGame(dbContext, id, cancellationToken).ConfigureAwait(false);
            var game = dbGame.ToModel();

            if (game.Stage != GameStage.Created)
                throw new InvalidOperationException("Game has already been started.");
            if (game.Players.Any(p => p.UserId == userId))
                throw new InvalidOperationException("You've already joined this game.");
            var engine = GameEngines[game.EngineId];
            if (game.Players.Count > engine.MaxPlayerCount)
                throw new InvalidOperationException("You can't join this game: there too many players already.");

            game.Players.Add(new GamePlayer() { UserId = userId });
            dbGame.UpdateFrom(game);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            context.Items.Set(OperationItem.New(game.Id));
        }

        public virtual async Task StartAsync(Game.StartCommand command, CancellationToken cancellationToken = default)
        {
            var (session, id) = command;
            var context = CommandContext.GetCurrent();
            if (Computed.IsInvalidating()) {
                var invGameId = context.Items.Get<OperationItem<string>>().Value;
                FindAsync(invGameId, session, default).Ignore();
                return;
            }

            var user = await AuthService.GetUserAsync(session, cancellationToken).ConfigureAwait(false);
            user = user.MustBeAuthenticated();
            var userId = long.Parse(user.Id);

            await using var dbContext = await CreateCommandDbContextAsync(cancellationToken).ConfigureAwait(false);
            var dbGame = await GetDbGame(dbContext, id, cancellationToken).ConfigureAwait(false);
            var game = dbGame.ToModel();

            if (game.Stage != GameStage.Created)
                throw new InvalidOperationException("Game has already been started.");
            if (game.UserId != userId)
                throw new InvalidOperationException("Only the creator of the game can start it.");
            var engine = GameEngines[game.EngineId];
            if (game.Players.Count < engine.MinPlayerCount)
                throw new InvalidOperationException(
                    $"{engine.MinPlayerCount - game.Players.Count} more player(s) must join to start the game.");
            if (game.Players.Count > engine.MaxPlayerCount)
                throw new InvalidOperationException(
                    $"Too many players: {engine.MaxPlayerCount - game.Players.Count} player(s) must leave to start the game.");

            game = game with {
                StartedAt = Clock.Now,
                Stage = GameStage.Running,
            };
            dbGame.UpdateFrom(game);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            context.Items.Set(OperationItem.New(game.Id));
        }

        public virtual async Task MoveAsync(Game.MoveCommand command, CancellationToken cancellationToken = default)
        {
            var (session, id, move) = command;
            var context = CommandContext.GetCurrent();
            if (Computed.IsInvalidating()) {
                var invGameId = context.Items.Get<OperationItem<string>>().Value;
                FindAsync(invGameId, session, default).Ignore();
                return;
            }

            var user = await AuthService.GetUserAsync(session, cancellationToken).ConfigureAwait(false);
            user = user.MustBeAuthenticated();
            var userId = long.Parse(user.Id);

            await using var dbContext = await CreateCommandDbContextAsync(cancellationToken).ConfigureAwait(false);
            var dbGame = await GetDbGame(dbContext, id, cancellationToken).ConfigureAwait(false);
            var game = dbGame.ToModel();

            if (game.Stage != GameStage.Running)
                throw new InvalidOperationException("Game has already ended or hasn't started yet.");
            if (game.Players.All(p => p.UserId != userId))
                throw new InvalidOperationException("You aren't a participant of this game.");
            var engine = GameEngines[game.EngineId];

            var state = game.State ?? engine.New();
            state = engine.Move(state, move);
            game = game with { State = state };
            if (state.IsGameEnded) {
                var players = game.Players.Select((p, index) => new GamePlayer() {
                    UserId = p.UserId,
                    Score = state.PlayerScores[index],
                }).ToImmutableList();
                game = game with {
                    EndedAt = Clock.Now,
                    Stage = GameStage.Ended,
                    GameEndMessage = state.GameEndMessage,
                    Players = players,
                };
            }
            dbGame.UpdateFrom(game);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            context.Items.Set(OperationItem.New(game.Id));
        }

        public virtual async Task EditAsync(Game.EditCommand command, CancellationToken cancellationToken = default)
        {
            var (session, id, isPublic) = command;
            var context = CommandContext.GetCurrent();
            if (Computed.IsInvalidating()) {
                var invGameId = context.Items.Get<OperationItem<string>>().Value;
                FindAsync(invGameId, session, default).Ignore();
                return;
            }

            var user = await AuthService.GetUserAsync(session, cancellationToken).ConfigureAwait(false);
            user = user.MustBeAuthenticated();

            await using var dbContext = await CreateCommandDbContextAsync(cancellationToken).ConfigureAwait(false);
            var dbGame = await GetDbGame(dbContext, id, cancellationToken).ConfigureAwait(false);
            dbGame.IsPublic = isPublic;
            var game = dbGame.ToModel();
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            context.Items.Set(OperationItem.New(game.Id));
        }

        // Queries

        public virtual async Task<Game?> FindAsync(string id, Session session, CancellationToken cancellationToken = default)
        {
            var dbGame = await GameResolver.TryGetAsync(id, cancellationToken).ConfigureAwait(false);
            return dbGame?.ToModel();
        }

        // Protected methods

        protected async Task<DbGame> GetDbGame(AppDbContext dbContext, string id, CancellationToken cancellationToken)
        {
            var dbGame = await FindDbGame(dbContext, id, cancellationToken).ConfigureAwait(false);
            if (dbGame == null)
                throw new KeyNotFoundException("Game not found.");
            return dbGame;
        }

        protected async Task<DbGame?> FindDbGame(AppDbContext dbContext, string id, CancellationToken cancellationToken)
        {
            var dbGame = await dbContext.Games.FindAsync(ComposeKey(id), cancellationToken).ConfigureAwait(false);
            if (dbGame == null)
                return null;
            await dbContext.Entry(dbGame).Collection(nameof(dbGame.Players))
                .LoadAsync(cancellationToken).ConfigureAwait(false);
            return dbGame;
        }
    }
}
