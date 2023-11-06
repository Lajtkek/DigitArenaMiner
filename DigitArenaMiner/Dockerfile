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

USER root
RUN apt-get update && apt-get upgrade
RUN apt-get install -y wget
RUN apt-get install -y python3
RUN apt-get install -y ffmpeg

USER app
RUN mkdir -p /app
RUN mkdir -p /app/Downloads
RUN mkdir -p /app/Downloads/YTDLP
RUN wget bettervse.lajtkep.dev/yt-dlp -o /app/Downloads/YTDLP/yt-dlp

RUN mkdir -p /app/Downloads/FFMPEG
RUN wget bettervse.lajtkep.dev/ffmpeg -o /app/Downloads/FFMPEG/ffmpeg
RUN chmod -R 777 /app

USER app
ENTRYPOINT ["dotnet", "DigitArenaMiner.dll"]