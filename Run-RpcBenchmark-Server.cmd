:<<BATCH
    @echo off
    set serverCoreCount=6
    if NOT "%1"=="" set serverCoreCount=%1
    start cmd /C dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- server https://0.0.0.0:22444/
    timeout 3
    powershell.exe -File SetAffinity.ps1 Samples.RpcBenchmark %serverCoreCount%
    exit /b
BATCH

#!/bin/sh
dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- server https://0.0.0.0:22444/

