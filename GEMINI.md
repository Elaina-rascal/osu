# osu! (lazer)

## Project Overview

osu!lazer is the next major version of the rhythm game osu!, built from scratch using the [osu!framework](https://github.com/ppy/osu-framework). It is an open-source project designed to eventually replace the current stable client. The codebase is primarily C# and .NET 8.0.

## Directory Structure

*   **`osu.Game/`**: Contains the core game logic, screens, overlays, and shared components. This is where most game-specific code resides.
*   **`osu.Desktop/`**: The desktop platform specific project (Windows, Linux, macOS). Contains the entry point (`Program.cs`) for desktop builds.
*   **`osu.Android/`, `osu.iOS/`**: Platform-specific projects for mobile devices.
*   **`osu.Game.Rulesets.*/`**: Projects containing the logic for specific game modes (Osu, Taiko, Catch, Mania).
*   **`osu.Game.Tests/`**: Unit and visual tests for the game.
*   **`Templates/`**: Templates for creating new rulesets.
*   **`assets/`**: Basic assets like logos.

## Development Workflow

### Prerequisites
*   .NET 8.0 SDK

### Building and Running

**Command Line:**
To run the desktop version of the game:
```bash
dotnet run --project osu.Desktop
```
*   **Note:** By default, this runs in `Debug` mode. `Debug` mode uses a separate database and environment (development mode).
*   **Warning:** **Do not run in `Release` configuration during development** unless strictly necessary for performance benchmarking. `Release` mode may interact with your stable installation's files or local realm database in unexpected ways for a dev environment.

**IDE:**
*   Open one of the Solution Filter (`.slnf`) files instead of the main `.sln` to save resources:
    *   `osu.Desktop.slnf` (Recommended for general dev)
    *   `osu.Android.slnf`
    *   `osu.iOS.slnf`

### Testing

Run unit tests via the CLI:
```bash
dotnet test
```

Visual tests are a core part of the workflow (using osu!framework's visual test browser), typically run via the `osu.Game.Tests` project within an IDE.

### Code Style & Analysis

The project enforces strict coding standards.
*   **Format Code:** Run `dotnet format` or use your IDE's formatting tools.
*   **Analysis:** Use `InspectCode.ps1` (PowerShell) or `InspectCode.sh` (Bash) to run JetBrains InspectCode analysis.
*   **Configuration:**
    *   `.editorconfig`: Defines coding style rules.
    *   `CodeAnalysis/`: Contains specific analyzer configurations.
    *   C# 12.0 features are used.
    *   Nullable reference types are **enabled**.

## Contribution Guidelines

*   **Branches:** Always use topic branches (e.g., `fix-score-display`, `feat-new-mod`). Do not commit directly to `master`.
*   **Pull Requests:**
    *   Target `master` for most changes.
    *   Target `pp-dev` for changes affecting star rating or PP calculation.
    *   Ensure all new features and bug fixes have accompanying tests.
*   **Dependencies:** When modifying `osu-framework` or `osu-resources` alongside `osu`, use the `UseLocalFramework.sh` / `UseLocalResources.sh` scripts to link local copies.

## Key Technologies
*   **Language:** C#
*   **Framework:** osu!framework (game engine/UI framework)
*   **Database:** Realm (local data storage)
*   **Graphics:** Veldrid (via osu!framework)
