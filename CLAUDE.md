# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

Target platforms are **Android** and **Windows only** (no iOS or Mac — intentionally excluded in the `.csproj`).

```powershell
# Build for Windows
dotnet build src/TheBonding.Maui/TheBonding.Maui.csproj -f net10.0-windows10.0.19041.0

# Build for Android (debug — signed with sideload.keystore)
dotnet build src/TheBonding.Maui/TheBonding.Maui.csproj -f net10.0-android

# Build for Android (release — produces .aab for Play Store)
dotnet publish src/TheBonding.Maui/TheBonding.Maui.csproj -f net10.0-android -c Release

# Run on Windows
dotnet run --project src/TheBonding.Maui/TheBonding.Maui.csproj -f net10.0-windows10.0.19041.0
```

If the build fails with "Assets file doesn't have a target", delete `src/TheBonding.Maui/obj` and `bin` then re-run — do not use `--no-restore`.

There are no automated tests in this repository.

## Versioning

`ApplicationDisplayVersion` uses 3-part semver required by MAUI's resizetizer: **`1.YYYY.MMDD`** where MMDD is month+day with no leading zero (e.g. May 17 → `517`). MAUI automatically appends `.0` when building the Windows MSIX manifest. Increment `ApplicationVersion` (integer) on every Play Store upload.

## Signing

All Android builds (debug and release) use `src/TheBonding.Maui/Signing/sideload.keystore` so every build produces the same APK signature regardless of machine. This prevents "package conflicts with existing package" errors when sideloading updates.

When submitting to the Play Store, add a production keystore and split the signing config into Debug (sideload key) and Release (production key) property groups. Production keystore passwords go in environment variables — never in the csproj.

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
- `MainLayout` handles swipe-right-from-left-edge to open the nav drawer and swipe-left to close via `TouchEventArgs` — no JS required.

## Platform-Specific Patterns

JS-based file downloads and the native share sheet differ between platforms. Use MAUI native APIs and guard with `#if ANDROID` / `#else`:

| Operation | Android | Windows |
|---|---|---|
| File pick (import) | `FilePicker.Default.PickAsync()` | `FilePicker.Default.PickAsync()` — same API, no guard needed |
| File save/share (export) | `Share.Default.RequestAsync(new ShareFileRequest(...))` writing to `FileSystem.CacheDirectory` first | `JS.InvokeVoidAsync("downloadTextFile", ...)` |

The `#if ANDROID` guard is only needed for the **save/share** path. `FilePicker` works on both platforms without a guard — see `Import.razor` (no conditional) vs `Export.razor` (conditional on save).

`AndroidManifest.xml` has `allowBackup="false"` (intentional — prevents encrypted database from being included in Android backups) and no INTERNET or network permissions (the Blazor WebView uses a local asset loader that does not require them).

## Adding New Services

When adding a new service, register it in **both** extension methods:

- `TheBonding.Infrastructure/ServiceCollectionExtensions.cs` → `AddInfrastructureServices()` for repositories and infrastructure services.
- `TheBonding.Application/ServiceCollectionExtensions.cs` → `AddApplicationServices()` for application-layer services.

All services are registered as **Singleton**. `DbConnectionFactory` is the only singleton registered directly in `MauiProgram.cs` (it needs the platform-specific DB path before DI is built).

## App Routes

| Route | Page | Layout |
|---|---|---|
| `/` | `Home.razor` — redirects to setup/unlock/dashboard | AuthLayout |
| `/setup` | `Setup.razor` — first-launch password creation | AuthLayout |
| `/unlock` | `Unlock.razor` — password entry | AuthLayout |
| `/dashboard` | `Dashboard.razor` — stats + recent events | MainLayout |
| `/events` | `Events.razor` — event list | MainLayout |
| `/events/new` | `EventForm.razor` | MainLayout |
| `/events/{id}/edit` | `EventForm.razor` | MainLayout |
| `/partners` | `Partners.razor` — partner list | MainLayout |
| `/partners/new` | `PartnerForm.razor` | MainLayout |
| `/partners/{id}` | `PartnerDetail.razor` | MainLayout |
| `/partners/{id}/edit` | `PartnerForm.razor` | MainLayout |
| `/profile` | `Profile.razor` — user profile + health history | MainLayout |
| `/lookups` | `Lookups.razor` — manage list categories/items | MainLayout |
| `/export` | `Export.razor` | MainLayout |
| `/import` | `Import.razor` | MainLayout |
| `/data-privacy` | `DataPrivacy.razor` — clear/reset data | MainLayout |

## Data Management

`DataManagementService` (Infrastructure) exposes two wipe operations:

- `ClearPersonalDataAsync()` — deletes Activity, PartnerHealthStatus, Partner, UserProfile, UserHealthStatus rows but leaves AppSettings and LookupItems intact (app stays set up and unlockable).
- `FullResetAsync()` — additionally deletes LookupItem and AppSettings rows; the app returns to first-launch state.

## Miscellaneous

- `LangVersion=preview` is set in the csproj — C# preview features are available.
- `MauiProgram.StartupError` (static string?) is set if DB init or auth load fails at startup. `Home.razor` reads it and renders the error instead of routing.
- `CheckboxList.razor` (`Components/Shared/`) is a reusable multi-select checkbox list used by `EventForm.razor` for lookup-backed multi-value fields (activity types, positions, roles, etc.).

## Store Readiness

**Windows Store** — `Package.appxmanifest` has two `TODO` fields (`Identity Name` and `Publisher CN`) that must be filled from Microsoft Partner Center → App management → App identity. The line `<WindowsPackageType>None</WindowsPackageType>` in the csproj must be removed when building for Store submission (switches from unpackaged to MSIX). Windows tile PNG assets are in `Platforms/Windows/Assets/`.

**Privacy policy** — hosted at `https://dfwdino.github.io/TheBonding/` via `docs/index.html` (GitHub Pages, `/docs` folder). Update the effective date in that file when the policy changes.
