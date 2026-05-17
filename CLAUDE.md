# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

Target platforms are **Android** and **Windows only** (no iOS or Mac — intentionally excluded in the `.csproj`).

```powershell
# Build for Windows
dotnet build src/TheBonding.Maui/TheBonding.Maui.csproj -f net10.0-windows10.0.19041.0

# Build for Android
dotnet build src/TheBonding.Maui/TheBonding.Maui.csproj -f net10.0-android

# Run on Windows
dotnet run --project src/TheBonding.Maui/TheBonding.Maui.csproj -f net10.0-windows10.0.19041.0
```

There are no automated tests in this repository.

## Architecture

Clean Architecture split across five projects:

| Project | Role |
|---|---|
| `TheBonding.Domain` | Entity classes — no dependencies |
| `TheBonding.Shared` | DTO/Record types passed between layers |
| `TheBonding.Application` | Service + repository interfaces, service implementations, `Result<T>` |
| `TheBonding.Infrastructure` | Repository implementations (raw ADO.NET), `EncryptionService`, `DatabaseInitializer`, `DataManagementService` |
| `TheBonding.Maui` | .NET MAUI shell + Blazor Hybrid WebView + Razor pages |

Dependency direction: `Maui → Application + Infrastructure`, `Infrastructure → Application`, `Application → Domain + Shared`.

## Encryption & Key Model

All sensitive data is encrypted at rest. Understanding the two-key model is essential before touching any data layer:

- **Key1** — derived from the user's password via PBKDF2-SHA256 (600k iterations). Never stored, exists in memory transiently only during unlock.
- **Key2** — 256-bit random key generated once at setup. Stored encrypted under Key1. Held in `IAuthService.CurrentKey2` (in-memory only) while the app is unlocked, zeroed on Lock.
- **AuthVerifier** — `SHA-256(Key1)`, stored to confirm correct password without storing Key1.
- All entity rows use a `DataBlob` column: JSON serialized then AES-256-CBC encrypted with Key2.
- `LookupItem.EncryptedValue` is AES-256-CBC with Key2. `LookupCategory.Name` is plaintext (not sensitive).
- Lockout: 5 failed unlock attempts → 10-minute lockout persisted in `AppSettings`.

## Startup Sequence

`MauiProgram.CreateMauiApp()` runs three blocking steps before any UI renders:
1. `DatabaseInitializer.InitializeAsync()` — creates schema (idempotent), seeds `LookupCategory` rows.
2. `IAuthService.LoadAsync()` — reads `AppSettings` to populate `IsSetup`.
3. `Home.razor` (`/`) redirects to `/setup`, `/unlock`, or `/dashboard` based on auth state.

`LookupItem` seeding happens *after* setup (requires Key2) via `LookupSeederService.SeedDefaultsAsync(key2)`, called at the end of `SetupAsync` and again on each successful `UnlockAsync`.

## Data Access Pattern

- `DbConnectionFactory` is the single entry point for SQLite connections — always use it, never open connections directly.
- Repositories use raw `Microsoft.Data.Sqlite` (no ORM). They accept `IEncryptionService` and retrieve Key2 from `IAuthService.CurrentKey2`. If `CurrentKey2` is null the app is locked and they return `Result.Failure(...)`.
- Services are all registered as **Singleton** so Key2 and cached state survive for the app lifetime.
- All service methods return `Result` or `Result<T>` — check `IsSuccess`/`IsFailure` and surface `Error` in the UI.

### Entity → Summary flow

Repositories return `Domain.Entities.*` objects with raw encrypted `DataBlob` strings. Services call `IEncryptionService.DecryptBlob<TRecord>(blob, key2)` to deserialize into `Shared.Models.*Record` types, then wrap both in a `Shared.Models.*Summary`. UI pages bind against `*Summary` — never against raw entities.

### Schema evolution

`DatabaseInitializer` uses `CREATE TABLE IF NOT EXISTS` — idempotent for table creation. Adding a **new column** to an existing table requires an explicit `ALTER TABLE` statement appended to `CreateTablesAsync`. There is no migration framework; add a guard (`SELECT COUNT(*) FROM pragma_table_info`) if the column may already exist on upgraded installs.

### `ActivityRecord` backward compatibility

`ActivityRecord.PartnerId` (singular) is kept only for records written before multi-partner support was added. All new writes use `PartnerIds` (list). When reading, treat `PartnerId` as a fallback if `PartnerIds` is empty.

## UI / Blazor Conventions

- Two layouts: `AuthLayout` (login, setup, unlock) and `MainLayout` (authenticated shell with hamburger nav + lock button).
- Pages check `AuthService.IsUnlocked` in `OnInitialized` and redirect to `/unlock` if false.
- Data loads in `OnAfterRenderAsync(firstRender)`, not `OnInitializedAsync`, to avoid blocking the initial render.
- `StateHasChanged()` is called manually after async data loads complete.
