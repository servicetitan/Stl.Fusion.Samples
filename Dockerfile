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

FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /samples
COPY ["src/", "src/"]
COPY ["docs/", "docs/"]
COPY Samples.sln .
RUN dotnet build -c:Debug
RUN dotnet build -c:Release --no-restore

# Create HelloWorld sample image
FROM build as sample_hello_world
WORKDIR /samples/src/HelloWorld/bin/Debug/net5.0

# Create HelloBlazorServer sample image
FROM build as sample_hello_blazor_server
WORKDIR /samples/src/HelloBlazorServer
ENTRYPOINT ["dotnet", "bin/Debug/net5.0/Samples.HelloBlazorServer.dll"]

# Create Blazor sample image
FROM build as sample_blazor
WORKDIR /samples/src/Blazor/Server
ENTRYPOINT ["dotnet", "bin/Debug/net5.0/Samples.Blazor.Server.dll"]

# Create Caching Server sample image
FROM build as sample_caching_server
WORKDIR /samples/src/Caching/Server
ENTRYPOINT ["dotnet", "bin/Release/net5.0/Samples.Caching.Server.dll"]

# Create Caching Client sample image
FROM build as sample_caching_client
WORKDIR /samples/src/Caching/Client/bin/Release/net5.0


# Websites

FROM build as publish
WORKDIR /samples
RUN dotnet publish -c:Release -f:net5.0 --no-build --no-restore src/Blazor/Server/Server.csproj

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine as runtime
RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /samples
COPY --from=publish /samples .

# Create Blazor sample image for website
FROM runtime as sample_blazor_ws
WORKDIR /samples/src/Blazor/Server
ENTRYPOINT ["dotnet", "bin/Release/net5.0/publish/Samples.Blazor.Server.dll"]

