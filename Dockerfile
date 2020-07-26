# Create a base image with code and restored NuGet packages
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster as base
WORKDIR /src

COPY ["src/", "src/"]
COPY ["docs/", "docs/"]

COPY Samples.sln .
COPY Directory.Build.props .
COPY Directory.Build.targets .
COPY Packages.props .

# Collect application artifacts
FROM base as build
RUN dotnet build -c

# Create runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.1-alpine as runtime
RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /src
COPY --from=build /src .
WORKDIR /Blazor/Server/bin/Debug
ENTRYPOINT ["dotnet", "StlFusionSamples.Blazor.Server.dll"]
