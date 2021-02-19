# Part 10: Multi-Host Invalidation and CQRS with Operations Framework

If you read [Part 8](./Part08.md), you know that multi-host 
invalidation requires the following components:
1. **Command execution pipeline.**
2. **Command logger** - a handler in this pipeline responsible 
   for logging commands to some persistent store - and ideally,
   doing this as part of command's own transaction.
3. **Command log reader** - a service watching for command log 
   updates made by other processes.
4. An API allowing to "replay" a command in invalidation mode -
   i.e. run a part of command's logic responsible solely for 
   invalidation.
   
Operations Framework implements this in a very robust way.

## Operation Framework

Useful definitions:

OF
: A shortcut for Operations Framework used further

Operation
: An action that could be logged into operation log and replayed.
  So far only commands could act as such actions, but for now
  the framework implies there might be other kinds of 
  operations too. So operation is ~ whatever OF can handle as
  an operation, including commands.

It worth mentioning that OF has almost zero dependency on
Fusion. You can use it for other purposes too 
(e.g. audit logging) - with or without Fusion. 
Moreover, you can easily remove all Fusion-specific services
it has from IoC container to completely disable 
its Fusion-specific behaviors.

### Enabling Operations Framework

1. Add the following `DbSet` to your `DbContext` (`AppDbContext` further):
```cs
public DbSet<DbOperation> Operations { get; protected set; } = null!;
```

2. Add the following code to your server-side IoC container 
   configuration block 
   (typically it is `Startup.ConfigureServices` method):

```cs
services.AddDbContextServices<AppDbContext>(builder => {
    builder.AddDbOperations((_, o) => {
        // Default unconditional wake up period: 0.25s
        o.UnconditionalWakeUpPeriod = TimeSpan.FromSeconds(1); 
    });
    // Optionally enable file-based log change tracker 
    builder.AddFileBasedDbOperationLogChangeTracking(someSharedFilePath);
    // Or, if you use PostgreSQL, use this instead of above line
    // builder.AddNpgsqlDbOperationLogChangeTracking();
});
```

> Note that OF works solely on server side, so you don't have
> to make similar changes in Blazor app's IoC container 
> configuration code.

What happens here?
- `AddDbContextServices<TDbContext>(Action<DbContextBuilder<TDbcontext>>)` 
  is a convenience helper allowing methods like `AddDbOperations`
  to be implemented as extension methods to `DbContextBuilder<TDbcontext>`,
  so you as a user of such methods need to specify `TDbContext` type
  just once - when you call `AddDbContextServices`. In other
  words, `AddDbContextServices` does nothing itself, but allows
  services registered inside its builder block to be dependent on 
  `TDbContext` type.
- `AddDbOperations` does nearly all the job. I'll cover every service 
  it registers in details further.
- And finally, `AddXxxDbOperationLogChangeTracking` adds one of two
  services implementing log change tracking notification / listening.
  It's totally fine to omit any of these calls - in this case
  operation log reader will be waking up only unconditionally, which
  happens 4 times per second by default, so other hosts running 
  your code may see 0.25s delay in invalidations of data changed by
  their peers. You can reduce this delay, of course, but doing this
  means you'll be hitting the database more frequently with operation
  log tail requests. `AddXxxDbOperationLogChangeTracking` methods
  make this part way more efficient by explicitly notifying the log
  reader to read the tail as soon as they know for sure one of their
  peers updated it:
  - `AddFileBasedDbOperationLogChangeTracking` relies on a shared file
    to pass these notifications. Any peer that updates operation log
    also "touches" this file (just update its modify date), and all 
    other peers are using `FileSystemWatcher`-s to know about these
    touches as soon as they happen. And once they happen, they "wake up"
    the operation log reader.
  - `AddNpgsqlDbOperationLogChangeTracking` does ~ the same, but 
    relying on PostgreSQL's 
    [NOTIFY / LISTEN](https://www.postgresql.org/docs/13/sql-notify.html)
    feature - basically, a built-in message queue.
    If you use PostgreSQL, you should almost definitely use it.
    It's also a bit more efficient than file-based notifications,
    because such notifications also bear the Id of the agent
    that made the change, so the listening process on that agent
    has a change to ignore any of its own notifications.
  - Right now there are no other log change tracking options, but 
    more are upcoming. And it's fairly easy to add your own - 
    e.g. [PostgreSQL log change tracking](https://github.com/servicetitan/Stl.Fusion/tree/master/src/Stl.Fusion.EntityFramework.Npgsql/Operations)
    requires less than 200 lines of code, and you need to change
    maybe just 30-40 of these lines in your own tracker.
    
## Using Operations Framework

It's actually as simple as it could be. Let's look how use of 
Operations Framework requires you to transform the code
of your action handlers:

Before:

```cs
public async Task<ChatMessage> PostMessageAsync(
    long userId, string text, CancellationToken cancellationToken = default)
{
    await using var dbContext = CreateDbContext().ReadWrite();
    // Actual code...

    // Invalidation
    using (Computed.Invalidate())
        PseudoGetAnyChatTailAsync().Ignore();
    return message;
}
```

After:

1. You need a dedicated type for every command. 
   In this case it's going to be:

```cs
public record PostMessageCommand(string Text, Session Session) : ICommand<ChatMessage>
{
    // Default constructor is needed for JSON deserialization
    public PostMessageCommand() : this(null!, Session.Null) { }
}
```

Notice that above type implements `ICommand<ChatMessage>` - the
generic parameter here tells the type of result this command returns,
and regular handlers for this command (non-filtering ones) should
use matching `Task<T>` as their return type.

Finally, note that such types don't have to be records - 
I use records mainly because they are immutable + support `with`
syntax from C# 9, but overall, there is no requirement like
"every command has to be a record".

2. The action handler should be transformed into a command handler:
   - Make it a virtual method tagged with `[CommandHandler]`.
     You can apply it to corresponding interface member as well, 
     the attribute is "inherited" and can be "overriden" like
     `[ComputeMethod]` does too.
   - As any method-based command handler (see [Part 8](./Part08.md)), 
     this method should have 
     `Task<TResult> AnyName(TCommand, [dependencies, ]CancellationToken)`
     signature.
   - `Task<TResult>` can be simply `Task` for any command returning 
     `Unit` (i.e. implementing `ICommand<Unit>` interface), as well
     as for any filtering handlers (because they might process 
     commands of multiple result types).
     
3. The invalidation block inside the handler should be transformed 
   as well:
   - Move it to the very beginning of the method
   - Replace `using (Computed.Invalidate()) { Code(); }` construction 
     there with
     `if (Computed.IsInvalidating()) { Code(); return default!; }`.
   - If your service derives from `DbServiceBase` or `DbAsyncProcessBase`,
     you should use its protected `CreateCommandDbContextAsync` method
     to get `DbContext` where you are going to make changes.
     You still have to call `SaveAsync` on this `DbContext` in the end.

Here is how transformed handler for above command should look like:
     
```cs

[CommandHandler]
public virtual async Task<ChatMessage> PostMessageAsync(
    IChatService.PostMessageCommand command, CancellationToken cancellationToken = default)
{
    // CommandContext is typically used inside both branches, so
    // it makes sense to get it before anything else.
    var context = CommandContext.GetCurrent();
      
    if (Computed.IsInvalidating()) {
        PseudoGetAnyChatTailAsync().Ignore();
        return default!;
    }

    // Notice I 
    await using var dbContext = await CreateCommandDbContextAsync(cancellationToken);
    // The same action handler code as it was in example above.
}

```
## How all of this works?

Let's list all the pipeline handlers in their invocation order:

### 1. `PreparedCommandHandler`, priority:
- `1_000_000_001` for `IPreparedCommand`
- `1_000_000_000` for `IAsyncPreparedCommand`

This filtering handler simply invokes `Prepare` / `PrepareAsync` method
on the command, and in fact, its role is to trigger self-validation
for the command before anything else.

You may find out this handler is actually a part of `Stl.CommandR`,
and it's auto-registered when you call `.AddCommander(...)`, so
it's a "system" command validation handler.

The only command that currently implements validation is
`ServerSideCommandBase<TResult>` - actually, a super-important 
base type for commands that can be invoked on server-side only.
Check out 
[its source code](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.CommandR/Commands/ServerSideCommandBase.cs) -
it's super simple, and it's clear how it's supposed to work:
- If you inherit your command from `ServerSideCommandBase<TResult>`
  it will expose `IsServerSide` property, which isn't serialized.
  So creating a command with `IsServerSide = true` is possible
  on the client, but this flag will be lost once it gets 
  deserialized on the server side.
- `IServerSideCommand` implements `IPreparedCommand`, and 
  `ServerSideCommandBase.Prepare` fails if `IsServerSide == false`.
  So you can run such commands on server by explicitly settings
  this flag to `true` there - the best way to do this is to use
  `MarkServerSide()` extension method, which returns the command
  of the same type, but with this flag set to `true` 
  
### 2. `NestedCommandLogger`, priority: 11_000

This filter is responsible for logging all nested commands.
You are free to call one command from another, and it's implied
that each command implements its own invalidation properly,
so "parent" commands shouldn't do anything special to process
invalidations for "child" commands - thanks to this handler.

I won't dig deeply into the details of how it works yet,
let's just assume it does the job - for now :) 

### 3. `TransientOperationScopeProvider`, priority: 10_000

It is the outermost, "catch-all" operation scope provider 
for commands that don't use any other (real) operation scopes. 

Let me explain what all of this means now :) 

Your app may have a few different types of `DbContext`-s, 
or maybe even other (non-EF) storages. 
And since it's a bad idea to assume we run distributed 
transactions across all of them, OF assumes each of these 
storages (i.e. `DbContext` types) has its own operation log,
and an operation entry is added to this log inside the same
transaction, that run operation's own logic.

So to achieve that, OF assumes there are "operation scope providers" -
command filters that publish different implementations of
`IOperationScope` via `CommandContext.Items` (in case you don't
remember, `CommandContext.Items` is `HttpContext.Items` analog
in CommandR-s world). And when the final command handler runs,
it should pick the right one of these scopes to get access 
to the underlying storage. Here is how this happens, if we're 
talking about EF:
- `DbOperationScopeProvider<TDbContext>` 
  [creates and "injects"](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion.EntityFramework/Operations/DbOperationScopeProvider.cs#L54)
  `DbOperationScope<TDbContext>` into
  `CommandContext.GetCurrent().Items` collection.
- Once your service needs to access `SomeDbContext` from the
  command handler, it typically calls its protected
  `CreateCommandDbContextAsync` method, which 
  ["pulls" the `DbOperationScope<SomeDbContext>`](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion.EntityFramework/DbServiceBase.cs#L38)
  and asks it to provide actual `SomeDbContext`.
- Finally, `DbOperationScope.CreateDbContext` does all the magic:
  when you call this method for the first time, not only it creates 
  "primary" `DbContext`, but also starts a new transaction there. 
  And when you call it later, it creates a `DbContext` that
  [shares the same `DbConnection`](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion.EntityFramework/DbOperationScope.cs#L107)
  as the the "primary" one. 
  
In other words, `DbOperationScope` ensures that all `DbContext`-s
you get via it share the same connection and transaction. 
In addition, it ensures that 
[an operation entry is added to the operation log before this 
transaction gets committed, and the fact commit actually  happened 
is verified in case of failure](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion.EntityFramework/DbOperationScope.cs#L133).
If you're curious why it makes sense to do this, 
[see this page]( https://docs.microsoft.com/en-us/ef/ef6/fundamentals/connection-resiliency/commit-failures).

Now, back to `TransientOperationScopeProvider` - its job
is to provide an operation scope for commands that don't use
other operation scopes - e.g. the ones that change only 
in-memory state. If your command doesn't use one of APIs 
"pinning" it to some other operation scope, this is the 
scope it's going to use implicitly.

Finally, it has another grand role: it runs so-called
operation completion for all operations, i.e. not only
the transient ones. And this piece deserves its own 
section:

### What is Operation Completion?

It's a process that happens on invocation of 
`OperationCompletionNotifier.NotifyCompletedAsync(operation)`.
`IOperationCompletionNotifier` is a service simply "distributes" such
notifications to `IOperationCompletionListener`-s after eliminating
all *duplicate notifications* (based on `IOperation.Id`). By default,
it remembers up to 10K of up to 1-hour-old operations (more precisely,
their `Id`-s and commit times).

Even though it invokes all the handlers concurrently,
`NotifyCompletedAsync` completes when *all* 
`IOperationCompletionListener.OnOperationCompletedAsync` handlers
complete. So once `NotifyCompletedAsync` completes, you
can be certain that every of these "follow up" actions is already 
completed as well. 

[`CompletionProducer` (check it out, it's tiny)](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion/Operations/Internal/CompletionProducer.cs#L34) -
is probably the most important one of such listeners.
The critical part of its job is actually a single line:
```cs
await Commander.CallAsync(Completion.New(operation), true).ConfigureAwait(false);
```

Two things are happening here:

1. It creates [`Completion<TCommand>` object](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion/Operations/Completion.cs) -
   in fact, a command as well!
2. It runs this command via `Commander.CallAsync(completion, true)`.

The last argument (`isolate = true`) indicates that `ExecutionContext` 
flow will be suppressed for this `Commander` invocation, 
so the pipeline for this command won't "inherit" any of 
`AsyncLocal`-s, including `CommandContext.Current`.
In other words, the command will run in a new top-level 
`CommandContext` and won't have a chance to "inherit" 
any state via async locals.

For the note, it's a kind of overkill, because 
`OperationCompletionNotifier` also suppresses `ExecutionContext` 
flow when it runs listeners. But... Just in case :)

Now, notice that `ICompletion` implements `IMetaCommand` interface - 
a tagging interface for various "follow up" commands that aren't 
executed directly, but invoked by some pipeline handlers.
Some of generic command handlers have special checks 
for such commands - e.g. you might notice that 
`Completion<SomeCommand>` will never push another 
`Completion<Completion<SomeCommand>>` into the pipeline 
due to one of such checks.

Any `IMetaCommand` implements `IServerSideCommand`,
so it will run successfully only when it's marked as such.
And indeed, the `Completion.New` does this:
```cs
public static ICompletion New(IOperation operation)
{
    var command = (ICommand?) operation.Command
        ?? throw Errors.OperationHasNoCommand(nameof(operation));
    var tCompletion = typeof(Completion<>).MakeGenericType(command.GetType());
    var completion = (ICompletion) tCompletion.CreateInstance(operation)!;
    return completion.MarkServerSide();
}
```

Above code also shows that the actual type of command becomes 
a value of generic parameter of `Completion<T>` type. 
So if you want to implement a *reaction to completion* of e.g. 
`MyCommand` - just declare a filtering command handler 
for `ICompletion<MyCommand>`. And yes, it's better to use 
`ICompletion<T>` rather than `Completion<T>` in such handlers.

So what is operation completion?
- It's invocation of 
  `OperationCompletionNotifier.NotifyCompletedAsync(operation)`
- Which in turn invokes all operation completion listeners
  - One of such listeners - `CompletionProducer` - reacts
    to this by creating a "meta command" of
    `ICompletion<TCommand>` type and invoking `Commander` for
    this new command. 
    Later you'll learn the invalidation pass is actually 
    triggered by a handler reacting to this command.
  - And if you registered any of operation log change notifiers,
    all of them currently implement `IOperationCompletionListener` 
    notifying their peers that operation log was just updated.
    
Now, a couple good questions:

> Q: Why `NotifyCompletedAsync` doesn't return instantly?
Why it bothers to await completion of each and every handler? 

This ensures that once the invocation of this method 
from `TransientOperationScopeProvider`
is completed, every follow-up action related to it is 
completed as well, including invalidation. 

In other words, our command processing pipeline is built 
in such a way that once a command completes, you can be 
fully certain that any pipeline-based follow-up action
is completed for it as well - including invalidation.

> Q: What else invokes `NotifyCompletedAsync`?

Just `DbOperationLogReader` -
[see how it does this](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion.EntityFramework/Operations/DbOperationLogReader.cs#L51).

As you might notice, it skips all local commands, and a big
comment there explains why it does so.

> Q: So every host invokes some logic for every command
> running on other hosts?

Yes. All of this means that:
- Even though there are typically way more queries than
  commands,  some actions (e.g. presence info updates) 
  might be quite frequent. And you should avoid hitting 
  the DB or running any resource-consuming activities
  inside your invalidation blocks. Especially -
  inside such blocks for frequent actions.
- If you know for sure that at some point you'll 
  reach the scale that won't allow you to rely on
  a single operation log (e.g. an extremely high 
  frequency of "read tail" calls from ~ hundreds of 
  hosts may bring it down), or e.g. that even 
  replaying the invalidations for every command
  won't be possible - you need to think how to 
  partition your system.
  
For the note, invalidations are extremely fast - 
it's safe to assume they are ~ as fast as identical
calls resolving via `IComptuted` instances, i.e.
it's safe to assume you can run ~ a 1 million of 
invalidations per second per HT core, which 
means that an extremely high command rate is 
needed to "flood" OF's invalidation pipeline,
and most likely it won't be due to the cost of
invalidation. JSON deserialization and
CommandR pipeline itself is much more likely 
to become a bottleneck under extreme load.

Ok, back to our command execution pipeline :)

### 4. `DbOperationScopeProvider<TDbContext>`, priority: 1000

This filter provides `DbOperationScope<TDbContext>`, i.e. the
"real" operation scope for your operations. As you probably
already guessed, the fact this filter exists in the pipeline
doesn't mean it always creates some `DbContext` and
transaction to commit the operation to. 
This happens if and only if:
- You the `DbOperationScope<TDbContext>` it created for you - e.g. by calling
  `CommandContext.GetCurrent().Items.Get<DbOperationScope<AppDbContext>>()`
- And ask this scope to provide a `DbContext` by calling its 
  `CreateDbContextAsync` method, which indicates
  you're going to use this operation scope.

> Note: if your service derives from `DbServiceBase` or `DbAsyncProcessBase`, 
they provide `CreateCommandDbContextAsync` method, which is actually 
a shortcut for above actions. If you like the idea of such shortcuts, 
derive your DB-related services from one of these types or their 
descendants like `DbWakeSleepProcessBase`.

### 5. `InvalidateOnCompletionCommandHandler`, priority: 100

Let's look at its handler declaration first:
```cs
[CommandHandler(Priority = 100, IsFilter = true)]
public async Task OnCommandAsync(
  ICompletion command, CommandContext context, CancellationToken cancellationToken)
{ 
    //  ... 
}
```

As you might guess, it reacts to the *completion* of any command, 
and runs the original command **plus** every nested
command logged during its execution in the "invalidation mode" -
i.e. inside `Computed.Invalidate()` block.
This is why your command handlers need a branch checking for 
`Computed.IsInvalidating() == true` running the invalidation logic
there!

You're [welcome to see what it actually does](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion/Operations/Internal/InvalidateOnCompletionCommandHandler.cs#L45) -
it's a fairly simple code, the only tricky piece is related to nested operations.

On a positive side, `InvalidateOnCompletionCommandHandler` is the last
filter in the pipeline, so we can switch to this topic + one other important 
aspect - **operation items**.

## Operation items

API endpoint: `commandContext.Operation().Items`

It's actually a pretty simple abstraction allowing you to store
some data together with the operation - so once its completion
is "played" on this or other hosts, this data is readily available.

I'll show how it's used in one of Fusion's built-in command handlers -
[`SignOutCommand` handler of `DbAuthService`](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion.EntityFramework/Authentication/DbAuthService.cs#L91):

```cs
public virtual async Task SignOutAsync(
    SignOutCommand command, CancellationToken cancellationToken = default)
{
    var (session, force) = command;
    var context = CommandContext.GetCurrent();
    if (Computed.IsInvalidating()) {
        GetSessionInfoAsync(session, default).Ignore();
        var invSessionInfo = context.Operation().Items.TryGet<SessionInfo>();
        if (invSessionInfo != null) {
            TryGetUserAsync(invSessionInfo.UserId, default).Ignore();
            GetUserSessionsAsync(invSessionInfo.UserId, default).Ignore();
        }
        return;
    }

    await using var dbContext = await CreateCommandDbContextAsync(cancellationToken).ConfigureAwait(false);

    var dbSessionInfo = await Sessions.FindOrCreateAsync(dbContext, session, cancellationToken).ConfigureAwait(false);
    var sessionInfo = dbSessionInfo.ToModel();
    if (sessionInfo.IsSignOutForced)
        return;

    context.Operation().Items.Set(sessionInfo);
    sessionInfo = sessionInfo with {
        LastSeenAt = Clock.Now,
        AuthenticatedIdentity = "",
        UserId = "",
        IsSignOutForced = force,
    };
    await Sessions.CreateOrUpdateAsync(dbContext, sessionInfo, cancellationToken).ConfigureAwait(false);
}
```

First, look at this line inside the invalidation block:
```cs
var invSessionInfo = context.Operation().Items.TryGet<SessionInfo>()
```

It tries to pull `SessionInfo` object from `Operation().Items`. But why?
Well, because needs **pre-sign-out** `SessionInfo` that still contains
`UserId`. And the code that goes after this call invalidates results of
a few other methods related to this user.

The code that stores this info is located below:

```cs
context.Operation().Items.Set(sessionInfo);
```

As you see, it stores `sessionInfo` object into
`context.Operation().Items` right before creating its copy 
with wiped out `UserId` - in other words, *it saves
the info it wipes for the invalidation logic*.

And this is precisely the purpose of this API - to pass
some information related to the operation to "follow up" actions
(currently "invalidation pass" is the only follow-up action).
As you might guess, this info is stored in the DB along with
the operation, so peer hosts will see it as well while
running their own invalidation logic. 

> Q: How this differs from `CommandContext.Items`?

`CommandContext.Items` live only while the top-level command runs.
They aren't persisted anywhere, and thus they won't be available
on peer hosts too.

But importantly, both these objects are `OptionSet`-s.
Check out [its source code](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl/Collections/OptionSet.cs)
to learn how it works - again, it's a fairly tiny class.

## Nested command logging

I'll be brief here. Nested commands are logged into one
of operation items - you may quickly find that its type is 
`ImmutableList<NestedCommandEntry>`.
- The logging is happening in `NestedCommandLogger` type.
  You might notice that nested commands of nested commands 
  are properly logged too - moreover, their `Operation().Items`
  are captured & stored independently as well! In other words,
  you're free to call other commands from your commands w/o a need 
  to worry about their invalidation piece of work (it will happen) 
  or collisions of their operation items with yours.
- The "invalidation mode replay" of these commands is performed by 
  `InvalidateOnCompletionCommandHandler`.

There is nothing like a "generic" handler triggering completion 
for such commands - as you might guess, completion is meaningful 
for top-level commands only. Nested commands are captured
and stored solely to simplify invalidation, and if this piece
won't be there, you'd have to manually duplicate any logic 
triggering commands both in the "main" and in the "invalidation" 
sections. Luckily, I'm a big fan of DRY, so I had no choice 
other than solving this problem once and forever :)

## How can I learn Operation Framework deeper?

The easiest way to find all the components used by Operations 
Framework is to see the implementation of `DbContextBuilder.AddDbOperations`
and `IServiceCollection.AddFusion` (more precisely, `FusionBuilder` 
constructor). Links to the source code of both methods:
- https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion/FusionBuilder.cs#L34
- https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion.EntityFramework/DbContextBuilder.cs#L46

The services added in `FusionBuilder` constructor (i.e. the ones
that are used no matter what) are:

```cs
// CommandR, command completion and invalidation
var commander = Services.AddCommander();
Services.TryAddSingleton<AgentInfo>();
Services.TryAddSingleton<InvalidationInfoProvider>();

// Transient operation scope & its provider
Services.TryAddTransient<TransientOperationScope>();
Services.TryAddSingleton<TransientOperationScopeProvider>();
commander.AddHandlers<TransientOperationScopeProvider>();

// Nested command logger
Services.TryAddSingleton<NestedCommandLogger>();
commander.AddHandlers<NestedCommandLogger>();

// Operation completion - notifier & producer
Services.TryAddSingleton<OperationCompletionNotifier.Options>();
Services.TryAddSingleton<IOperationCompletionNotifier, OperationCompletionNotifier>();
Services.TryAddSingleton<CompletionProducer.Options>();
Services.TryAddEnumerable(ServiceDescriptor.Singleton(
    typeof(IOperationCompletionListener),
    typeof(CompletionProducer)));

// Command completion handler performing invalidations
Services.TryAddSingleton<InvalidateOnCompletionCommandHandler.Options>();
Services.TryAddSingleton<InvalidateOnCompletionCommandHandler>();
commander.AddHandlers<InvalidateOnCompletionCommandHandler>();
```

I'll briefly describe the services I didn't mention yet; 
as for anything else, this piece of code is the best option to
start digging into OF deeper :)

`AgentInfo` is a simple type allowing OF to check if an
operation is originating from this or some other process.
Check out its [source code](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion/Operations/AgentInfo.cs) - 
it's tiny. 

> You might be confused by `Symbol` type there - it's actually
> just a string with cached result of `GetHashCode` (actually,
> a struct with both these fields + a number of overloaded
> operations & implicit conversions).
> `Symbol`-s are used in Fusion to speed up string comparisons
> and dictionary lookups. If you know for sure that for
> certain strings you'll do lots of equality comparisons -
> try using `Symbol` instead.

As you see, `AgentInfo.Id` is ~ a `Symbol` that includes:
- Machine name
- Unique process ID
- Unique ID for every new `AgentInfo` you create. This
  ensures that you can run a number of IoC containers with
  different services inside the same process & use OF
  to "sync" them. This is super useful for testing any
  OF related aspects (e.g. that all of your commands are
  actually "replayed" in invalidation mode on other hosts).

`InvalidationInfoProvider` is a service that tells whether
a given command requires invalidation. 
Its default implementation returns `true` for any command with 
a final handler that lives inside `IComputeService`, but 
not `IReplicaService`
([details](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion/Operations/InvalidationInfoProvider.cs#L38)).

Why not `IReplicaService`, you might guess? Because
replica services are allowed to register their command
handlers on the client side too, and since these handler 
are routing commands to corresponding server-side services,
the invalidation and any other post-processing should
happen there, but not on the client.

Ok, now let's look at `DbContextBuilder.AddDbOperations`:

```cs
// Common services
Services.TryAddSingleton<IDbOperationLog<TDbContext>, DbOperationLog<TDbContext, TDbOperation>>();

// DbOperationScope & its CommandR handler
Services.TryAddTransient<DbOperationScope<TDbContext>>();
Services.TryAddSingleton<DbOperationScopeProvider<TDbContext>>();
Services.AddCommander().AddHandlers<DbOperationScopeProvider<TDbContext>>();

// DbOperationLogReader - hosted service!
Services.TryAddSingleton(c => {
    var options = new DbOperationLogReader<TDbContext>.Options();
    logReaderOptionsBuilder?.Invoke(c, options);
    return options;
});
Services.TryAddSingleton<DbOperationLogReader<TDbContext>>();
Services.AddHostedService(c => c.GetRequiredService<DbOperationLogReader<TDbContext>>());

// DbOperationLogTrimmer - hosted service!
Services.TryAddSingleton(c => {
    var options = new DbOperationLogTrimmer<TDbContext>.Options();
    logTrimmerOptionsBuilder?.Invoke(c, options);
    return options;
});
Services.TryAddSingleton<DbOperationLogTrimmer<TDbContext>>();
Services.AddHostedService(c => c.GetRequiredService<DbOperationLogTrimmer<TDbContext>>());
```

`DbOperationLog` is a repository-like service providing access
to DB operation log.

The rest is self-explanatory (or was covered earlier). 

P.S. I certainly realize that even though OF's usage is fairly
simple on the outside, there is a complex API with many moving 
parts inside. And probably, some bugs.
So if you get stuck, please don't hesistate reaching me out
on [Discord](https://discord.gg/EKEwv6d). My nickname is "AY"
(Alex Yakunin), and I'll be happy to help.

#### [Next: Epilogue &raquo;](./PartFF.md) | [Tutorial Home](./README.md)

