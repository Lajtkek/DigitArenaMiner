FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY DigitArenaMiner.csproj .
RUN dotnet restore "DigitArenaMiner.csproj"
COPY . .
RUN dotnet publish "DigitArenaMiner.csproj" -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as final
WORKDIR /app
COPY --from=build /publish .


RUN useradd -m app
RUN chown -R app:app /app

USER app
RUN mkdir -p /app
RUN chmod -R 777 .

USER root
RUN apt-get update && apt-get upgrade
USER root
RUN apt-get install -y python3

USER app
ENTRYPOINT ["dotnet", "DigitArenaMiner.dll"]