# Tutorial

FROM mcr.microsoft.com/dotnet/sdk:3.1 as tutorial
WORKDIR /samples
COPY ["docs/", "docs/"]
WORKDIR /samples/docs/tutorial
ENV DOTNET_TRY_CLI_TELEMETRY_OPTOUT=1
RUN dotnet tool update -g Microsoft.dotnet-try
ENV PATH="$PATH:/root/.dotnet/tools"
RUN apt-get update
RUN apt-get install -y simpleproxy
RUN echo "simpleproxy -L 50005 -R localhost:50004 -v &" >start.sh
RUN echo "dotnet try --port 50004 /samples/docs/tutorial" >>start.sh
ENTRYPOINT ["sh", "start.sh"]


# Samples

FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
#RUN dotnet workload install wasm-tools
WORKDIR /samples
COPY ["src/", "src/"]
COPY ["templates/", "templates/"]
COPY ["docs/", "docs/"]
COPY Samples.sln .
RUN dotnet build -c:Debug
RUN dotnet build -c:Release --no-restore

# Create HelloWorld sample image
FROM build as sample_hello_world
WORKDIR /samples/src/HelloWorld/bin/Debug/net8.0

# Create HelloCart sample image
FROM build as sample_hello_cart
WORKDIR /samples/src/HelloCart/bin/Debug/net8.0

# Create HelloBlazorServer sample image
FROM build as sample_hello_blazor_server
WORKDIR /samples/src/HelloBlazorServer
ENTRYPOINT ["dotnet", "bin/Debug/net8.0/Samples.HelloBlazorServer.dll"]

# Create HelloBlazorHybrid sample image
FROM build as sample_hello_blazor_hybrid
WORKDIR /samples/src/HelloBlazorHybrid/Server
ENTRYPOINT ["dotnet", "bin/Debug/net8.0/Samples.HelloBlazorHybrid.Server.dll"]

# Create Blazor sample image
FROM build as sample_blazor
WORKDIR /samples/src/Blazor/Server
ENTRYPOINT ["dotnet", "bin/Debug/net8.0/Samples.Blazor.Server.dll"]

# Create MiniRpc sample image
FROM build as sample_mini_rpc
WORKDIR /samples/src/MiniRpc/MiniRpc
ENTRYPOINT ["dotnet", "bin/Release/net8.0/Samples.MiniRpc.dll"]

# Create MultiServerRpc sample image
FROM build as sample_multi_server_rpc
WORKDIR /samples/src/MultiServerRpc/MultiServerRpc
ENTRYPOINT ["dotnet", "bin/Release/net8.0/Samples.MultiServerRpc.dll"]

# Create Benchmark sample image
FROM build as sample_benchmark
WORKDIR /samples/src/Benchmark/Benchmark
ENTRYPOINT ["dotnet", "bin/Release/net8.0/Samples.Benchmark.dll"]

# Create RpcBenchmark sample image
FROM build as sample_rpc_benchmark
WORKDIR /samples/src/RpcBenchmark/RpcBenchmark
ENTRYPOINT ["dotnet", "bin/Release/net8.0/Samples.RpcBenchmark.dll"]


# Websites

FROM build as publish
WORKDIR /samples
RUN dotnet publish -c:Release --no-build --no-restore src/Blazor/Server/Server.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
WORKDIR /samples
COPY --from=publish /samples .

# Create Blazor sample image for website
FROM runtime as sample_blazor_ws
WORKDIR /samples/src/Blazor/Server
ENV Logging__Console__FormatterName=
ENV Server__GitHubClientId=7d519556dd8207a36355
ENV Server__GitHubClientSecret=8e161ca4799b7e76e1c25429728db6b2430f2057
ENTRYPOINT ["dotnet", "bin/Release/net8.0/publish/Samples.Blazor.Server.dll"]
