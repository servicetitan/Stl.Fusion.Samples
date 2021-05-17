using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Stl.Fusion.Authentication;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;
using Templates.Blazor1.Abstractions;

namespace Templates.Blazor1.UI.Services
{
    [BasePath("todo")]
    public interface ITodoClient
    {
        [Post("addOrUpdate")]
        Task<Todo> AddOrUpdate([Body] AddOrUpdateTodoCommand command, CancellationToken cancellationToken = default);
        [Post("remove")]
        Task Remove([Body] RemoveTodoCommand command, CancellationToken cancellationToken = default);

        [Get("tryGet")]
        Task<Todo?> TryGet(Session session, string id, CancellationToken cancellationToken = default);
        [Get("list")]
        Task<Todo[]> List(Session session, PageRef<string> pageRef, CancellationToken cancellationToken = default);
    }
}
