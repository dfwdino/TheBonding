using Microsoft.Data.Sqlite;

namespace TheBonding.Infrastructure.Data;

/// <summary>
/// Creates the SQLite schema and seeds non-sensitive reference data on first launch.
///
/// Called once at app startup from MauiProgram.cs before DI is fully used.
///
/// What this does:
///   - Enables WAL journal mode for better read/write performance.
///   - Creates all tables (IF NOT EXISTS — safe to call every launch).
///   - Creates indexes.
///   - Seeds LookupCategory rows (plaintext — not sensitive).
///
/// What this does NOT do:
///   - Seed LookupItem values. Those are encrypted and require Key2,
///     which only exists after the user completes first-launch setup.
///     LookupItem seeding is handled by LookupSeederService after setup.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly DbConnectionFactory _factory;

    public DatabaseInitializer(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        await EnableWalModeAsync(connection);
        await CreateTablesAsync(connection);
        await CreateIndexesAsync(connection);
        await SeedLookupCategoriesAsync(connection);
    }

    // -------------------------------------------------------------------------
    // WAL mode — set once, persisted in the database file.
    // -------------------------------------------------------------------------

    private static async Task EnableWalModeAsync(SqliteConnection connection)
    {
        await ExecuteAsync(connection, "PRAGMA journal_mode = WAL;");
    }

    // -------------------------------------------------------------------------
    // Table creation
    // -------------------------------------------------------------------------

    private static async Task CreateTablesAsync(SqliteConnection connection)
    {
        // AppSettings: single row, inserted during first-launch setup.
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS AppSettings (
                Id             INTEGER PRIMARY KEY,
                Salt           TEXT    NOT NULL,
                AuthVerifier   TEXT    NOT NULL,
                EncryptedKey2  TEXT    NOT NULL,
                FailedAttempts INTEGER NOT NULL DEFAULT 0,
                LockoutUntil   TEXT    NULL,
                IsSetup        INTEGER NOT NULL DEFAULT 0
            );
            """);

        // UserProfile: single row, created on first profile save.
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS UserProfile (
                Id       INTEGER PRIMARY KEY,
                DataBlob TEXT    NOT NULL
            );
            """);

        // Partner: one row per partner.
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS Partner (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                DataBlob     TEXT    NOT NULL,
                IsActive     INTEGER NOT NULL DEFAULT 1,
                LastUsedDate TEXT    NOT NULL,
                CreatedDate  TEXT    NOT NULL
            );
            """);

        // PartnerHealthStatus: one row per test entry per partner.
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS PartnerHealthStatus (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                PartnerId   INTEGER NOT NULL,
                DataBlob    TEXT    NOT NULL,
                TestedDate  TEXT    NOT NULL,
                CreatedDate TEXT    NOT NULL,
                FOREIGN KEY (PartnerId) REFERENCES Partner(Id)
            );
            """);

        // Activity: one row per logged activity event.
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS Activity (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                PartnerId    INTEGER NULL,
                DataBlob     TEXT    NOT NULL,
                OccurredDate TEXT    NOT NULL,
                CreatedDate  TEXT    NOT NULL,
                FOREIGN KEY (PartnerId) REFERENCES Partner(Id)
            );
            """);

        // LookupCategory: groups of lookup values (plaintext names).
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS LookupCategory (
                Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                Name     TEXT    NOT NULL,
                IsSystem INTEGER NOT NULL DEFAULT 0
            );
            """);

        // UserHealthStatus: the app owner's own STI/STD testing history.
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS UserHealthStatus (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                DataBlob    TEXT    NOT NULL,
                TestedDate  TEXT    NOT NULL,
                CreatedDate TEXT    NOT NULL
            );
            """);

        // LookupItem: individual values within a category (encrypted).
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS LookupItem (
                Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                CategoryId     INTEGER NOT NULL,
                EncryptedValue TEXT    NOT NULL,
                SortOrder      INTEGER NOT NULL DEFAULT 0,
                IsDefault      INTEGER NOT NULL DEFAULT 1,
                FOREIGN KEY (CategoryId) REFERENCES LookupCategory(Id)
            );
            """);
    }

    // -------------------------------------------------------------------------
    // Index creation
    // -------------------------------------------------------------------------

    private static async Task CreateIndexesAsync(SqliteConnection connection)
    {
        await ExecuteAsync(connection,
            "CREATE INDEX IF NOT EXISTS IX_Partner_IsActive ON Partner (IsActive, LastUsedDate);");

        await ExecuteAsync(connection,
            "CREATE INDEX IF NOT EXISTS IX_PartnerHealthStatus_PartnerId ON PartnerHealthStatus (PartnerId, TestedDate);");

        await ExecuteAsync(connection,
            "CREATE INDEX IF NOT EXISTS IX_Activity_OccurredDate ON Activity (OccurredDate);");

        await ExecuteAsync(connection,
            "CREATE INDEX IF NOT EXISTS IX_Activity_PartnerId ON Activity (PartnerId);");

        await ExecuteAsync(connection,
            "CREATE INDEX IF NOT EXISTS IX_UserHealthStatus_TestedDate ON UserHealthStatus (TestedDate);");

        await ExecuteAsync(connection,
            "CREATE INDEX IF NOT EXISTS IX_LookupItem_CategoryId ON LookupItem (CategoryId, SortOrder);");
    }

    // -------------------------------------------------------------------------
    // Seed lookup categories — plaintext, safe to insert on every first launch.
    // -------------------------------------------------------------------------

    private static async Task SeedLookupCategoriesAsync(SqliteConnection connection)
    {
        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM LookupCategory;";
        var count = (long)(await checkCmd.ExecuteScalarAsync() ?? 0L);
        if (count > 0) return;

        // IsSystem = 1 — user cannot delete the category itself,
        // but can still add, edit, and delete individual items within it.
        var categories = new (string Name, int IsSystem)[]
        {
            ("Activity Type",       1),
            ("Relationship Type",   1),
            ("Contraception",       1),
            ("Location",            1),
            ("Mood",                1),
            ("Gender Identity",     1),
            ("Sexual Orientation",  1),
            ("Health Test Type",    1),
            ("Health Test Result",  1)
        };

        foreach (var (name, isSystem) in categories)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO LookupCategory (Name, IsSystem)
                VALUES (@name, @isSystem);
                """;
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@isSystem", isSystem);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static async Task ExecuteAsync(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }
}
