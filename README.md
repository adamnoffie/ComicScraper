# Comic Scraper

A console application that scrapes comic strip images from various websites, detects new strips since the last run, and emails them as an HTML digest with embedded images.

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

Application settings are loaded from `appsettings.json` (required) with optional overrides from `appsettings.local.json`. Both files are copied to the output directory on build. Use `appsettings.local.json` for secrets and personal overrides — it should not be committed to source control.

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

Example:
```json
{
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

### Comics.txt

Defines which comics to scrape. This file must be in the same directory as the executable (it is copied there automatically on build).

The file has two sections:

- **`[regex]`** — Named regex patterns that can be reused across comics. Each line is a name (starting with `rgx`) followed by a regex with a named `url` capture group (and optional `alt`/`title` groups).
- **`[comics]`** — One comic per line: `Title, URL, Regex`. The regex can be a literal pattern or a name from the `[regex]` section.

Lines starting with `#` are comments. Lines starting with `##` are commented-out comic entries.

## Program Flow

1. **Load settings** — `AppSettings.Load()` reads `appsettings.json` (and `appsettings.local.json` if present).
2. **Read config files** — Parses `Comics.txt` for comic definitions and named regexes. Reads `History.xml` (if it exists) to restore previous-run state (last image URL and size per comic).
3. **Scrape each comic** — For each comic entry:
   - Fetches the comic's webpage HTML.
   - Applies the comic's regex to extract the strip image URL (and optional alt text / title).
   - Does an HTTP `HEAD` request to get the image's content size.
   - Compares the image URL and size to the previous run. If different, downloads the image to the `Comics/` directory. If the same, the comic is marked as not new and skipped.
4. **Send email** — If any new comics were found, composes an HTML email with all new strips embedded as inline images and sends it via SMTP to the configured recipients.
5. **Save history** — Writes the updated comic state (image URLs and sizes) back to `History.xml` for the next run.