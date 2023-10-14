FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TelegramToTrello.csproj", "./"]
RUN dotnet restore "TelegramToTrello.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "TelegramToTrello.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramToTrello.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramToTrello.dll"]
