using Microsoft.Data.Sqlite;

namespace TheBonding.Infrastructure.Data;

/// <summary>
/// Creates SQLite connections for the application database.
/// The database path is injected at startup from MauiProgram.cs
/// so Infrastructure has no dependency on MAUI platform APIs.
/// Foreign key enforcement and WAL mode are set at initialization time
/// by DatabaseInitializer — not per-connection — for performance.
/// </summary>
public sealed class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string dbPath)
    {
        _connectionString = $"Data Source={dbPath};Foreign Keys=True;";
    }

    public SqliteConnection CreateConnection() =>
        new(_connectionString);
}
