FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY DigitArenaMiner.csproj .
RUN dotnet restore "DigitArenaMiner.csproj"
COPY . .
RUN dotnet publish "DigitArenaMiner.csproj" -c Release -o /publish

FROM lajtkek/digitminer_run:latest as final
WORKDIR /app
COPY --from=build /publish .

ENTRYPOINT ["dotnet", "DigitArenaMiner.dll"]