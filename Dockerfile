# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore first (layer caching)
COPY ComicScraper/ComicScraper.csproj ComicScraper/
RUN dotnet restore ComicScraper/ComicScraper.csproj -r linux-musl-x64

# Copy everything else and publish
COPY ComicScraper/ ComicScraper/
RUN dotnet publish ComicScraper/ComicScraper.csproj \
    -c Release \
    -r linux-musl-x64 \
    --self-contained \
    -p:PublishSingleFile=true \
    -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Create data directory for persistent state
RUN mkdir -p /app/data

# Default DataPath to /app/data so History.json, Log.txt, Comics/ live there
ENV DataPath=/app/data

ENTRYPOINT ["./ComicScraper"]
