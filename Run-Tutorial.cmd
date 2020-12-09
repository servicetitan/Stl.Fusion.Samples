@echo off
dotnet build

pushd "docs\tutorial"
dotnet try --port 50005
popd
