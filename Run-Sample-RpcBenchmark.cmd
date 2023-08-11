@echo off
dotnet build -c Release

set DOTNET_ReadyToRun=0
set DOTNET_TieredPGO=1
set DOTNET_TC_QuickJitForLoops=1
dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj
