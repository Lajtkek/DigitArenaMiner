FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DigitArenaMiner/DigitArenaMiner.csproj", "DigitArenaMiner/"]
RUN dotnet restore "DigitArenaMiner/DigitArenaMiner.csproj"
COPY . .
WORKDIR "/src/DigitArenaMiner"
RUN dotnet build "DigitArenaMiner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DigitArenaMiner.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DigitArenaMiner.dll"]
