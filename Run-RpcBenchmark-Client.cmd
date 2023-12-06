:<<BATCH
    @dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- client %*
    exit /b
BATCH

#!/bin/sh
dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- "client %@"
