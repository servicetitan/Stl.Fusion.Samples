# Create a base image with code and restored NuGet packages
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster as build
WORKDIR /samples
COPY ["src/", "src/"]
COPY ["docs/", "docs/"]
COPY Samples.sln .
RUN dotnet build

# Create Tutorial image
FROM build as tutorial
WORKDIR /samples/docs/tutorial
RUN dotnet tool install -g --add-source "https://dotnet.myget.org/F/dotnet-try/api/v3/index.json" Microsoft.dotnet-try
ENV PATH="$PATH:/root/.dotnet/tools"
ENV DOTNET_TRY_CLI_TELEMETRY_OPTOUT=1
RUN apt-get update
RUN apt-get install -y simpleproxy
RUN echo "simpleproxy -L 50005 -R 127.0.0.1:50004 &" >> start.sh
RUN echo "dotnet try --port 50004 /samples/docs/tutorial" >> start.sh
ENTRYPOINT ["sh", "start.sh"]

# Create Blazor sample image
FROM build as sample_blazor
WORKDIR /samples/src/Blazor
ENTRYPOINT ["dotnet", "Server/bin/Debug/netcoreapp3.1/Samples.Blazor.Server.dll"]
