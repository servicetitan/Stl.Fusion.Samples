:<<BATCH
    @echo off
    dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- %*
    exit /b
BATCH

#!/bin/sh
dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- "%@"
