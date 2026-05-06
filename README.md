# Comic Scraper

A console application that scrapes comic strip images from various websites, detects new strips since the last run, and emails them as an HTML digest with embedded images.

Home on Docker Hub: https://hub.docker.com/r/noffie/comicscraper

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- VS Code (recommended) or any editor

## Building

### From the command line

```sh
dotnet build ComicScraper/ComicScraper.csproj
```

### Publishing a self-contained executable

**Windows (x64):**
```sh
dotnet publish ComicScraper/ComicScraper.csproj -c Release -r win-x64
```

**Linux (x64):**
```sh
dotnet publish ComicScraper/ComicScraper.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
```

### From VS Code

Pre-configured tasks are available in `.vscode/tasks.json`:

- **build** — Debug build
- **publish (win-x64)** — Release publish for Windows
- **publish (linux-x64)** — Release publish for Linux (self-contained single file)

Run them via **Terminal > Run Task** or `Ctrl+Shift+B`.

## Configuration

### appsettings.json

Application settings are loaded from `appsettings.json` (required) with optional overrides from several additional sources. Settings from later sources override earlier ones:

1. `appsettings.json` — bundled with the application (required)
2. `appsettings.local.json` — local development overrides (optional, not committed to source control)
3. `<DataPath>/appsettings.json` — user override in the data directory (auto-seeded from the bundled default on first run if not present)
4. `<DataPath>/appsettings.local.json` — user secrets in the data directory (optional)
5. Environment variables — highest priority, using `__` as the section separator (e.g. `Smtp__Password`)

On first run, if no `appsettings.json` exists in `DataPath`, the bundled default is automatically copied there as a starting point for customization.

In Docker, `DataPath` defaults to `/app/data`. Edit the auto-seeded `appsettings.json` in the mounted data volume to override the bundled defaults without modifying the image.

| Key | Description |
|-----|-------------|
| `UserAgent` | The `User-Agent` HTTP header string sent with all web requests. |
| `HttpHeaders` | A key-value map of additional HTTP headers applied to every request (e.g. `Accept`, `Accept-Language`, `Sec-Fetch-*`). |
| `EmailUseSSL` | `true`/`false` — whether the SMTP connection uses SSL. |
| `EmailToAddresses` | Comma-separated list of recipient addresses, e.g. `"Name <email>, Name2 <email2>"`. |
| `Smtp:From` | Sender address for the digest email. |
| `Smtp:Host` | SMTP server hostname (e.g. `smtp.gmail.com`). |
| `Smtp:Port` | SMTP server port (e.g. `587`). |
| `Smtp:UserName` | SMTP authentication username. |
| `Smtp:Password` | SMTP authentication password. |
| `Regexes` | A key-value map of named regex patterns (e.g. `rgxGoComics`). Each regex must contain a named `url` capture group, with optional `alt` and `title` groups. Referenced by name in `Comics` entries. |
| `Comics` | An array of comic definitions. Each entry has `Title`, `Url`, and `Regex` (either a name from `Regexes` or an inline regex pattern). |
| `DataPath` | Directory for runtime data and config overrides (`History.json`, `Log.txt`, `Comics/`, and `appsettings.json`). Defaults to the current working directory. Set via environment variable. |

All settings can also be overridden via environment variables using `__` (double underscore) as the section separator (e.g. `Smtp__Password`). Environment variables take the highest precedence.

> **Note:** `DataPath` is resolved from the environment variable before config files are loaded, so it cannot be set inside `appsettings.json`. Use the `DataPath` environment variable or accept the default (current working directory).

Example:
```json
{
  // Reusable regex patterns, referenced by name in Comics below.
  "Regexes": {
    "rgxGoComics": "\"contentUrl\":\"(?<url>https://featureassets\\.gocomics\\.com/.+?)['\"]" 
  },

  // Comics to scrape. Regex can be a name from Regexes or an inline pattern.
  "Comics": [
    { "Title": "Arlo and Janis", "Url": "https://www.gocomics.com/arloandjanis", "Regex": "rgxGoComics" },
    { "Title": "XKCD", "Url": "https://xkcd.com/", "Regex": "<img src=\"...(inline regex)...\"" }
  ],

  "UserAgent": "Mozilla/5.0 ...",
  "HttpHeaders": {
    "Accept": "text/html,application/xhtml+xml,...",
    "Accept-Language": "en-US,en;q=0.9",
    "Sec-Fetch-Dest": "document",
    "Sec-Fetch-Mode": "navigate",
    "Sec-Fetch-Site": "none",
    "Sec-Fetch-User": "?1",
    "Upgrade-Insecure-Requests": "1"
  },
  "EmailUseSSL": true,
  "EmailToAddresses": "You <you@example.com>",
  "Smtp": {
    "From": "Comic Scraper <sender@example.com>",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "sender@example.com",
    "Password": "your-app-password"
  }
}
```

> **Note:** Standard JSON does not support comments, but `Microsoft.Extensions.Configuration.Json` (used by this application) supports `//` line comments and `/* */` block comments. The bundled `appsettings.json` uses comments to document settings.

## Program Flow

1. **Load settings** — reads `appsettings.json` (and overlay files). If no `appsettings.json` exists in `DataPath`, the bundled default is automatically copied there.
2. **Read config** — Loads `Regexes` and `Comics` sections from the merged configuration. Reads `History.json` (if it exists) to restore previous-run state (last image URL and size per comic).
3. **Scrape each comic** — For each comic entry:
   - Fetches the comic's webpage HTML.
   - Applies the comic's regex to extract the strip image URL (and optional alt text / title).
   - Does an HTTP `HEAD` request to get the image's content size.
   - Compares the image URL and size to the previous run. If different, downloads the image to the `Comics/` directory. If the same, the comic is marked as not new and skipped.
4. **Send email** — If any new comics were found, composes an HTML email with all new strips embedded as inline images and sends it via SMTP to the configured recipients.
5. **Save history** — Writes the updated comic state (image URLs and sizes) back to `History.json` for the next run.

## Docker

### Building the image

```sh
docker build -t comicscraper .
```

### Running with environment variables

```sh
docker run --rm \
  -v ./data:/app/data \
  -e Smtp__Host=smtp.gmail.com \
  -e Smtp__Port=587 \
  -e Smtp__UserName=you@gmail.com \
  -e Smtp__Password=your-app-password \
  -e "Smtp__From=Comic Scraper <you@gmail.com>" \
  -e "EmailToAddresses=You <you@example.com>" \
  comicscraper
```

### Running with a config file in the data volume

Place your own `appsettings.json` in the mounted data directory to override the bundled defaults:

```sh
# Create a data directory and add your config
mkdir -p ./data
cp appsettings.json ./data/appsettings.json   # start from the default, then edit

docker run --rm \
  -v ./data:/app/data \
  comicscraper
```

You can also bind-mount a config file directly into the app directory:

```sh
docker run --rm \
  -v ./data:/app/data \
  -v ./appsettings.local.json:/app/appsettings.local.json:ro \
  comicscraper
```

### Using Docker Compose

1. Copy `.env.example` to `.env` and fill in your values.
2. Run:

```sh
docker compose up
```

### Persistent data

The container writes `History.json`, `Log.txt`, and `Comics/` to `/app/data` by default. On first run, the bundled `appsettings.json` is automatically copied to `/app/data/appsettings.json` if not already present. Mount a host directory or Docker volume to `/app/data` to persist state and configuration between runs.

Without persistence, every run will re-download and re-email all comics.

### Customizing comics

Comics and regexes are configured in `appsettings.json`. On first run with a mounted data volume, the default config is auto-seeded to `./data/appsettings.json`. Edit it to add, remove, or modify comics:

```sh
# First run seeds ./data/appsettings.json automatically
docker run --rm -v ./data:/app/data comicscraper

# Edit ./data/appsettings.json with your own comics, then run again
docker run --rm -v ./data:/app/data comicscraper
```

### Environment variables

All `appsettings.json` keys can be overridden via environment variables. Use `__` (double underscore) as the separator for nested keys:

| Variable | Example |
|---|---|
| `Smtp__Host` | `smtp.gmail.com` |
| `Smtp__Port` | `587` |
| `Smtp__UserName` | `you@gmail.com` |
| `Smtp__Password` | `your-app-password` |
| `Smtp__From` | `Comic Scraper <you@gmail.com>` |
| `EmailToAddresses` | `You <you@example.com>` |
| `EmailUseSSL` | `true` |
| `DataPath` | `/app/data` (default in container) |

### Publishing to Docker Hub

```sh
docker tag comicscraper yourusername/comicscraper:latest
docker push yourusername/comicscraper:latest
```