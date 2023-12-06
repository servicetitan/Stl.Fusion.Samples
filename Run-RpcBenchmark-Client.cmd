:<<BATCH
    @dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- client %*
BATCH

#!/bin/sh
dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- "client %@"
