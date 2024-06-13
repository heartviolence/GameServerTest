#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 9999

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["Server/Server/Server.csproj", "Server/Server/"]
COPY ["ServerShared/ServerShared.csproj", "ServerShared/"]
COPY ["Server/DBMigrations/DBMigrations.csproj", "Server/DBMigrations/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "./Server/Server/./Server.csproj"
COPY . .
WORKDIR "/src/Server/Server"
RUN dotnet build "./Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.dll"]