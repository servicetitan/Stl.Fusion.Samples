using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stl.Fusion;
using Samples.Blazor.Abstractions;
// using Samples.Helpers;
using Stl.Fusion.Bridge;
using Stl.Fusion.EntityFramework;

namespace Samples.Blazor.Server.Services
{
    [ComputeService(typeof(IBoardService))]
    public class BoardService : DbServiceBase<AppDbContext>, IBoardService
    {
        public BoardService(
            IServiceProvider services)
            : base(services)
        {
        }

        public virtual async Task<long> GetPlayerCountAsync(string boardId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            return await dbContext.Players.AsQueryable()
            .Where(p => p.PlayerBoard.BoardId == boardId)
            .LongCountAsync(cancellationToken)
            .ConfigureAwait(false);
        }
        
        public virtual async Task<long> GetPlayerCountWithoutCloneAsync(string boardId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            // var t1 = Task.Run(() => dbContext.Players.AsQueryable()
                // .Where(p => p.PlayerBoard.BoardId == boardId && !p.IsClone)
                // .LongCountAsync(cancellationToken)
                // .ConfigureAwait(false));
            // Task.WhenAll(t1);
            // return await t1.Result;
            return await dbContext.Players.AsQueryable()
                .Where(p => p.PlayerBoard.BoardId == boardId && !p.IsClone)
                .LongCountAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<Player> GetPlayerAsync(long id, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var player = dbContext.Players.FirstOrDefault(p => p.Id == id);
            if (player == null) {
                throw new ApplicationException("Please reload this page.");
            }
            return player;
        }
        
        public virtual async Task<Player> GetPlayerBySessionAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var player = dbContext.Players.FirstOrDefault(p => p.SessionId == sessionId);
            if (player == null) {
                throw new ApplicationException("Please reload this page.");
            }
            return player;
        }
        
        public virtual async Task<Board> GetBoardStateAsync(string boardId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var board = dbContext.Boards.FirstOrDefault(b => b.BoardId == boardId)
                        ?? throw new ApplicationException("Please reload this page.");
            return board;
        }

        public virtual Task<List<Player>> GetBoardPlayersAsync(string boardId, CancellationToken cancellationToken = default)
        {
            using var dbContext = CreateDbContext();
            var players = dbContext.Players.AsQueryable().Where(p => p.PlayerBoard.BoardId == boardId).ToList();
            return Task.FromResult(players);
        }
        
        public virtual async Task<Board> GetBoardAsync(string boardId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var board = dbContext.Boards.AsQueryable().FirstOrDefault(b => b.BoardId == boardId);
            return board ?? await CreateBoardAsync(boardId, cancellationToken);
        }

        
        public async Task<Board> ChangeBoardStateAsync(string boardId, int squareIndex, bool turnX, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var board = dbContext.Boards.FirstOrDefault(b => b.BoardId == boardId)
                        ?? throw new ApplicationException("Please reload this page.");
            if (board.BoardState[squareIndex] == ' ') {
                board.BoardState = GetNewValueString(board.BoardState, squareIndex, turnX).Result;
                board.IsXTurn = !turnX;
                dbContext.Boards.Update(board);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            // Computed.Invalidate(() => GetBoardAsync(boardId, cancellationToken));
            Computed.Invalidate();
            return board;
        }
        
        public async Task<Board> CreateBoardAsync(string boardId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var boardEntry = dbContext.Boards.Add(new Board() {
                BoardId = boardId
            });
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var board = boardEntry.Entity;

            // Computed.Invalidate(() => GetBoardAsync(boardId, cancellationToken));
            Computed.Invalidate();
            return board;
        }
        
        public async Task<Board> ClearBoardAsync(string boardId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var board = dbContext.Boards.AsQueryable().FirstOrDefault(b => b.BoardId == boardId);
            if (board == null)
                throw new ApplicationException("Please reload this page.");
            board.BoardState = "         ";
            board.IsXTurn = true;
            dbContext.Boards.Update(board);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            // Computed.Invalidate(() => GetBoardAsync(boardId, CancellationToken.None));
            Computed.Invalidate();
            return board;
        }

        // public async Task<(bool, long)> CreatePlayerAsync(string boardId, string sessionId, bool isClone, CancellationToken cancellationToken = default)
        // {
        //     bool res = false;
        //     long resId = 0;
        //     await using var dbContext = CreateDbContext();
        //     var board = dbContext.Boards.AsQueryable().FirstOrDefault(b => b.BoardId == boardId);
        //     if (board == null)
        //         throw new ApplicationException("Please reload this page.");
        //     var count = await GetPlayerCountWithoutCloneAsync(boardId, cancellationToken);
        //     if (count < 1) {
        //         dbContext.Boards.Update(board);
        //         var playerEntry = dbContext.Players.Add(new Player() {
        //             PlayerBoard = board,
        //             SessionId = sessionId,
        //             IsClone = false,
        //             IsXPlayer = true
        //         });
        //         var player = playerEntry.Entity;
        //         await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        //         var cloneEntry = dbContext.Players.Add(new Player() {
        //             PlayerBoard = board,
        //             SessionId = sessionId,
        //             IsClone = true,
        //             IsXPlayer = false
        //         });
        //         await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        //         res = true;
        //         resId = player.Id;
        //     } else {
        //         var firstPlayer = dbContext.Players.AsQueryable().
        //                               FirstOrDefault(p => p.PlayerBoard.BoardId == boardId && !p.IsClone)
        //                           ?? throw new ApplicationException("Please reload this page.");
        //         await RemoveClones(boardId, cancellationToken);
        //         dbContext.Boards.Update(board);
        //         var secondEntry = dbContext.Players.Add(new Player() {
        //             PlayerBoard = board,
        //             IsXPlayer = !firstPlayer.IsXPlayer,
        //             SessionId = sessionId,
        //             IsClone = false
        //         });
        //         await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        //         res = true;
        //         resId = secondEntry.Entity.Id;
        //     }
        //     return (res, resId);
        // }
        
        public async Task<(bool, long)> CreatePlayerAsync(string boardId, string sessionId, bool isXPlayer, CancellationToken cancellationToken = default)
        {
            bool res = false;
            long resId = 0;
            await using var dbContext = CreateDbContext();
            var board = await GetBoardAsync(boardId, cancellationToken);
            dbContext.Boards.Update(board);
            RemoveClones(boardId, cancellationToken);
            var playerEntry = dbContext.Players.Add(new Player() {
                PlayerBoard = board,
                SessionId = sessionId,
                IsClone = false,
                IsXPlayer = isXPlayer
            });
            var player = playerEntry.Entity;
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            res = true;
            resId = player.Id;
            // Computed.Invalidate(() => GetPlayerCountWithoutCloneAsync(boardId, cancellationToken));
            Computed.Invalidate();
            return (res, resId);
        }

        public async Task<Player> CreatePlayerCloneAsync(long id, string boardId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var player = GetPlayerAsync(id, cancellationToken).Result;
            var board = await GetBoardAsync(boardId, cancellationToken);
            dbContext.Boards.Update(board);
            var cloneEntry = dbContext.Players.Add(new Player() {
                PlayerBoard = board,
                IsXPlayer = !player.IsXPlayer,
                PlayerId = player.PlayerId,
                SessionId = player.SessionId,
                IsClone = true
            });
            var clone = cloneEntry.Entity;
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            // Computed.Invalidate(() => GetPlayerAsync(clone.Id, CancellationToken.None));
            Computed.Invalidate();
            // Computed.Invalidate(() => GetPlayerCountWithoutCloneAsync(boardId, CancellationToken.None));
            // Computed.Invalidate();
            return clone;
        }
        
        // Helpers

        protected virtual Task<string> GetNewValueString(string oldString, int squareNumber, bool isX)
        {
            var ch = isX ? 'X' : 'O';
            var newArray = oldString.ToCharArray();
            newArray[squareNumber] = ch;
            return Task.FromResult(new string(newArray));
        }

        protected virtual async Task RemoveClones(string boardId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            var board = await GetBoardAsync(boardId,cancellationToken);
            if (board == null)
                throw new ApplicationException("Please reload this page.");
            var clones = dbContext.Players.AsQueryable().Where(p => p.PlayerBoard.BoardId == boardId && p.IsClone);
            dbContext.Players.RemoveRange(clones);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}