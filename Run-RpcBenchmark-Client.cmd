@echo off
dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- client %*
