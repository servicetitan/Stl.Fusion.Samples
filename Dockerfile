# Create a base image with code and restored NuGet packages
FROM mcr.microsoft.com/dotnet/sdk:latest as build
WORKDIR /samples
COPY ["src/", "src/"]
COPY ["docs/", "docs/"]
COPY Samples.sln .
RUN dotnet build -c Debug
RUN dotnet build -c Release

# Create Tutorial image
FROM build as tutorial
WORKDIR /samples/docs/tutorial
RUN dotnet tool update -g Microsoft.dotnet-try
ENV PATH="$PATH:/root/.dotnet/tools"
ENV DOTNET_TRY_CLI_TELEMETRY_OPTOUT=1
RUN apt-get update
RUN apt-get install -y simpleproxy
RUN echo "simpleproxy -L 50005 -R 127.0.0.1:50004 &" >> start.sh
RUN echo "dotnet try --port 50004 /samples/docs/tutorial" >> start.sh
ENTRYPOINT ["sh", "start.sh"]

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
