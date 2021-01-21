# Part 9: CommandR

[Stl.CommandR](https://www.nuget.org/packages/Stl.CommandR/)
is [MediatR](hhttps://github.com/jbogard/MediatR)-like library helping
to implement CQRS-style command handlers. 
Together with a set of other abstractions it enables you to
get the pipeline described in the previous section with 
almost no extra code.

> This part of the tutorial will cover CommandR itself. The next one
will show how to use it together with other Fusion services
to implement a robust CQRS pipeline.


Even though CommandR solves the same problem as MediatR, it offers
a few new features:
- Unified handler pipeline. Any CommandR handler
  can act either as a filter (~ middleware-like handler)
  or as the final one. MediatR supports pipeline behaviors, which
  are similar to filtering handlers in CommandR, but
  they are the same for all commands.
  And this feature is actually quite useful - e.g.
  built-in filter for `IPreparedCommand` helps to unify validation.
- `CommandContext` - an `HttpContext`-like type helping to 
  non-handler code to store and access the state associated with
  the currently running command. Even though command contexts can
  be nested (commands may invoke other commands), the whole
  hierarchy of them is always available.
- Convention-based command handler discovery and invocation. 
  You don't have to implement `ICommandHandler<TCommand, TResult>` 
  every time you  want to add a handler - any async method tagged 
  with `[CommandHandler]`
  and having command as its first parameter, and `CancellationToken` as the
  last one works; all other arguments are resolved via IoC container.
- AOP-style command handlers. 
  Such handlers are virtual async methods with two arguments:
  `(command, cancellationToken)`. To make AOP part work, 
  the type declaring such handlers must be registered with
  `AddCommandService(...)` - 
  an extension method to `IServiceCollection` that registers
  a runtime-generated proxy instead of the actual implementation type. 
  The proxy ensures any call to such method is *still* routed via 
  `Commander.CallAsync(command)` to invoke the whole pipeline 
  for this command - i.e. all other handlers associated 
  with it. 
  In other words, such handlers can be invoked directly or via 
  `Commander`, but the result is always the same.
    
Since many of you are familiar with MediatR, here is the map
of its terms to CommandR terms:

| MediatR | CommandR |
|-|-|
| `IMediator` | `ICommander` |
| `IServiceCollection.AddMediatR` | `IServiceCollection.AddCommander` |
| `IServiceCollection.AddMediatR(assembly)` | `IServiceCollection.AttributeScanner().AddServicesFrom(assembly)` assuming you tag your command handler services with `[AddCommandHandlers]` or `[CommandService]` - in other words, CommandR doesn't have its own type scanner, but listed attributes allow you to use `AttributeScanner` from `Stl.DependencyInjection` to get the same result (and even more - e.g. scope-based registration)
| `IServiceProvider.GetRequiredService<IMediator>` | `IServiceProvider.GetRequiredService<ICommander>` or `IServiceProvider.Commander()` |
| `IMediatR.Send(command, cancellationToken)` | `ICommander.CallAsync(command, cancellationToken)` |
| `IRequest<TResult>` | `ICommand<TResult>` |
| `IRequest` | `ICommand<Unit>` |
| `IRequestHandler<TCommand, TResult>` | `ICommandHandler<TCommand, TResult>` |
| `IRequestHandler<TCommand, Unit>` | `ICommandHandler<TCommand, Unit>` |
| `RequestHandler<TCommand, Unit>` (synchronous) | No synchronous handlers: sorry, IMO they don't add enough value to justify having an extra set of interfaces for them |
| `INotification` | No special type for notifications: any command is allowed to have N filtering handlers, so all you need is to declare all of them but one as filters |
| Pipeline behaviors (`IPipelineBehavior<TRequest, TResponse>` & other types) | No special types for pipeline behaviors: any filtering handler is a pipeline behavior |
| Exception handlers | No special type for exception handlers: any filtering handler can do this |
| Polymorphic dispatch | Works the same way |

You might notice the API offered by CommandR is somewhat simpler -
at least while you don't use some of its unique features mentioned 
earlier.

**TBD:** Examples and the rest of this part.

#### [Next: Epilogue &raquo;](./PartFF.md) | [Tutorial Home](./README.md)

