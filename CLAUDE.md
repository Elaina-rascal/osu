# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is the osu! rhythm game client (codenamed "lazer"), the future and final iteration of the osu! game. It's a cross-platform C#/.NET 8.0 application supporting Windows, macOS, Linux, Android, and iOS.

## Development Environment

### Prerequisites
- .NET 8.0 SDK
- Recommended IDEs: Visual Studio, JetBrains Rider, or VS Code with C# Dev Kit

### Platform-Specific Development
- **Desktop**: Use `osu.Desktop.slnf` (most common)
- **Android**: Use `osu.Android.slnf` + `dotnet workload install android`
- **iOS**: Use `osu.iOS.slnf` + `dotnet workload install ios`

## Build Commands

### From CLI
```bash
# Run desktop version
dotnet run --project osu.Desktop

# Build with release configuration (for performance testing)
dotnet run --project osu.Desktop -c Release

# Restore packages if build fails
dotnet restore osu.Desktop.slnf
```

### From IDE
Load the appropriate `.slnf` file rather than the main `.sln` to reduce dependencies and hide irrelevant platforms.

## Testing

### Test Framework
- Uses NUnit 3.14.0 with Microsoft.NET.Test.Sdk
- Test projects follow naming pattern: `*.Tests` (e.g., `osu.Game.Tests`, `osu.Game.Rulesets.Osu.Tests`)

### Running Tests
```bash
# Build first
dotnet build -c Debug -warnaserror osu.Desktop.slnf

# Run all tests (from CI workflow)
dotnet test \
  osu.Game.Tests/bin/Debug/**/osu.Game.Tests.dll \
  osu.Game.Rulesets.Osu.Tests/bin/Debug/**/osu.Game.Rulesets.Osu.Tests.dll \
  osu.Game.Rulesets.Taiko.Tests/bin/Debug/**/osu.Game.Rulesets.Taiko.Tests.dll \
  osu.Game.Rulesets.Catch.Tests/bin/Debug/**/osu.Game.Rulesets.Catch.Tests.dll \
  osu.Game.Rulesets.Mania.Tests/bin/Debug/**/osu.Game.Rulesets.Mania.Tests.dll \
  osu.Game.Tournament.Tests/bin/Debug/**/osu.Game.Tournament.Tests.dll \
  Templates/**/*.Tests/bin/Debug/**/*.Tests.dll
```

### Test Execution Modes
Tests can run in two threading modes (set via `OSU_EXECUTION_MODE` environment variable):
- `SingleThread`
- `MultiThreaded`

## Code Quality Tools

### Code Formatting
```bash
dotnet format
```

### Code Analysis
- JetBrains InspectCode: `dotnet jb inspectcode osu.Desktop.slnf`
- CodeFileSanity: `dotnet codefilesanity`
- NVika for report parsing: `dotnet nvika parsereport`

### Banned APIs
Check `CodeAnalysis/BannedSymbols.txt` for prohibited APIs. Common restrictions:
- Don't use `object.Equals` - use `IEquatable<T>` or `EqualityComparer<T>.Default`
- Don't use `Task.Wait()` or `Task.Result` - use `WaitSafely()` and `GetResultSafely()`
- Avoid Humanizer methods - use `StringDehumanizeExtensions` instead

## Architecture Overview

### Core Structure
- **osu.Game**: Core game logic and shared components
- **osu.Game.Rulesets.\***: Game mode implementations (Osu, Mania, Taiko, Catch)
- **osu.Desktop/osu.Android/osu.iOS**: Platform-specific implementations
- **Templates/**: Custom ruleset development templates

### Key Architectural Patterns

#### Ruleset System (Plugin Architecture)
- Each ruleset is a separate project implementing `Ruleset` base class
- Rulesets define their own `DrawableRuleset<T>`, beatmap converters, score processors
- Loaded dynamically via `RulesetStore`/`RealmRulesetStore`

#### Dependency Injection
- Uses `[Cached]` and `[Resolved]` attributes extensively
- Managed through `DependencyContainer`

#### Component System
- `Drawable` objects are composable UI components
- `Container` system for layout management
- `Bindable` system for data binding and state management

### Key Entry Points
- **Desktop**: `osu.Desktop/Program.cs` → `DesktopGameHost` → `OsuGameDesktop`
- **Android**: `osu.Android/OsuGameActivity.cs` → `AndroidGameActivity` → `OsuGameAndroid`
- **iOS**: `osu.iOS/AppDelegate.cs` → `GameApplicationDelegate` → `OsuGameIOS`

### Core Game Systems
1. **Input**: `osu.Game/Input/` - `OsuUserInputManager`, `RealmKeyBindingStore`
2. **Audio**: `osu.Game/Audio/` - `PreviewTrackManager`, `HitSampleInfo`
3. **Graphics**: `osu.Game/Graphics/` - `OsuColour`, `OsuFont`, `OsuIcon`
4. **UI**: `osu.Game/Screens/` and `osu.Game/Overlays/` - `OsuScreen`, `OsuScreenStack`
5. **Online**: `osu.Game/Online/` - `APIProvider`, multiplayer, chat, leaderboards
6. **Data**: `BeatmapManager`, `ScoreManager`, `SkinManager` with Realm database

### RL Module (New Addition)
- `osu.Game/RL/TCP.cs`: TCP data sender for external RL agents
- `osu.Game/RL/player.cs`: Custom player class exposing game state to RL agents
- Used for sending game state (mouse position, next hit object info) to external AI agents

## Code Style Guidelines

### File Headers
All C# files must include the license header:
```
Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
See the LICENCE file in the repository root for full licence text.
```

### EditorConfig Rules
- Indentation: 4 spaces for C#, 2 spaces for project files
- Line endings: CRLF
- UTF-8-BOM for project files
- Trim trailing whitespace

### Naming Conventions
- Follow .NET naming conventions
- Use `IDE1006` analyzer for naming style enforcement

## Cross-Repository Development

### Testing Framework/Resource Changes
When testing changes in dependent repositories (osu-framework, osu-resources):
```bash
# macOS/Linux
./UseLocalFramework.sh
./UseLocalResources.sh

# Windows
.\UseLocalFramework.ps1
.\UseLocalResources.ps1
```

Assumes repositories are checked out in adjacent directories:
```
|- osu            // this repository
|- osu-framework
|- osu-resources
```

## Custom Ruleset Development

Ruleset templates are available in `Templates/` directory. Custom rulesets can:
- Reuse osu! beatmap library and game engine
- Define new gameplay mechanics
- Have custom UI and scoring systems

## Important Notes

- Use platform-specific `.slnf` files for development to reduce build times
- Performance testing should use `-c Release` configuration
- Mobile platform builds require workload installation: `dotnet workload install android/ios`
- Code analysis runs as part of CI and should pass before committing
- The project uses Realm for local data storage with custom extensions for notifications