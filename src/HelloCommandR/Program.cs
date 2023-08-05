using Samples.HelloCommandR;

var services = new ServiceCollection()
    .AddCommander(c => c.AddService<GreetingService>())
    .BuildServiceProvider();
var commander = services.Commander();

await commander.Call(new SayCommand("Hello!"));
await commander.Call(new SayCommand("All these calls work the same way!"));
await commander.Run(new SayCommand("")); // This call won't throw an exception
