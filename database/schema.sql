-- =============================================================================
-- schema.sql
-- The Bonding — SQLite reference schema (documentation only)
--
-- This schema is NOT run directly. Tables are created at first launch by
-- DatabaseInitializer.cs in TheBonding.Infrastructure.
--
-- Design notes:
--   - Single-user local app. No Users table. No Admin table.
--   - AppSettings holds the one-time setup state and auth material.
--   - All personal data columns are encrypted AES-256 blobs (TEXT = Base64).
--   - Structural columns (dates, FK ids, flags) are stored plaintext so
--     the app can sort and filter without decrypting.
--   - Lookup item VALUES are encrypted — user-created values could reveal
--     personal preferences. Category NAMES are plaintext (structural labels).
--   - Foreign key enforcement is enabled per-connection via PRAGMA.
--   - WAL journal mode is enabled at initialization for better performance.
--   - All datetimes stored as ISO-8601 TEXT (SQLite best practice).
--   - AuthVerifier = SHA-256(Key1). Lets the app verify the entered
--     password is correct without storing the password or Key1.
-- =============================================================================

PRAGMA foreign_keys = ON;
PRAGMA journal_mode = WAL;

-- ---------------------------------------------------------------------------
-- AppSettings: single row — app setup state and auth material.
-- Inserted once during first-launch setup. Never updated except for
-- FailedAttempts and LockoutUntil on bad unlock attempts.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AppSettings (
    Id             INTEGER PRIMARY KEY,          -- always 1
    Salt           TEXT    NOT NULL,             -- Base64 PBKDF2 salt
    AuthVerifier   TEXT    NOT NULL,             -- Base64 SHA-256(Key1) for password check
    EncryptedKey2  TEXT    NOT NULL,             -- Base64 Key2 encrypted with Key1 (AES-256-CBC)
    FailedAttempts INTEGER NOT NULL DEFAULT 0,
    LockoutUntil   TEXT    NULL,                 -- ISO-8601 or NULL
    IsSetup        INTEGER NOT NULL DEFAULT 0    -- 0 = first launch, 1 = setup complete
);

-- ---------------------------------------------------------------------------
-- UserProfile: single row — encrypted personal data about the device owner.
-- Created on first profile save, not at setup.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS UserProfile (
    Id       INTEGER PRIMARY KEY,   -- always 1
    DataBlob TEXT    NOT NULL        -- encrypted JSON blob
);

-- ---------------------------------------------------------------------------
-- Partner: one row per partner profile.
-- IsActive and LastUsedDate are plaintext for sorting/filtering.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Partner (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
    DataBlob     TEXT    NOT NULL,               -- encrypted JSON blob
    IsActive     INTEGER NOT NULL DEFAULT 1,     -- 1=active, 0=inactive
    LastUsedDate TEXT    NOT NULL,               -- ISO-8601
    CreatedDate  TEXT    NOT NULL                -- ISO-8601
);

-- ---------------------------------------------------------------------------
-- PartnerHealthStatus: one row per test/status entry per partner.
-- Full history is kept — inserts and hard deletes only, no updates.
-- TestedDate is plaintext for sorting.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PartnerHealthStatus (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    PartnerId  INTEGER NOT NULL,
    DataBlob   TEXT    NOT NULL,                -- encrypted JSON blob
    TestedDate TEXT    NOT NULL,                -- ISO-8601
    CreatedDate TEXT   NOT NULL,                -- ISO-8601
    FOREIGN KEY (PartnerId) REFERENCES Partner(Id)
);

-- ---------------------------------------------------------------------------
-- Activity: one row per logged activity event.
-- OccurredDate is plaintext for sorting/display.
-- PartnerId nullable — some activities may not have a linked partner.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Activity (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
    PartnerId    INTEGER NULL,
    DataBlob     TEXT    NOT NULL,              -- encrypted JSON blob
    OccurredDate TEXT    NOT NULL,              -- ISO-8601
    CreatedDate  TEXT    NOT NULL,              -- ISO-8601
    FOREIGN KEY (PartnerId) REFERENCES Partner(Id)
);

-- ---------------------------------------------------------------------------
-- LookupCategory: groups of lookup values.
-- Names are plaintext — these are structural labels, not sensitive data.
-- IsSystem = 1 means the category cannot be deleted by the user.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS LookupCategory (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Name     TEXT    NOT NULL,
    IsSystem INTEGER NOT NULL DEFAULT 0         -- 1 = cannot be deleted
);

-- ---------------------------------------------------------------------------
-- LookupItem: individual lookup values within a category.
-- EncryptedValue is the user-visible label (encrypted — may reveal preferences).
-- IsDefault = 1 means it was seeded with the app (user can still delete it).
-- Seeded at first launch via code, not SQL, because values require Key2.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS LookupItem (
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoryId     INTEGER NOT NULL,
    EncryptedValue TEXT    NOT NULL,            -- encrypted lookup label
    SortOrder      INTEGER NOT NULL DEFAULT 0,
    IsDefault      INTEGER NOT NULL DEFAULT 1,  -- 1=app default, 0=user added
    FOREIGN KEY (CategoryId) REFERENCES LookupCategory(Id)
);

-- ---------------------------------------------------------------------------
-- Indexes
-- ---------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS IX_Partner_IsActive
    ON Partner (IsActive, LastUsedDate);

CREATE INDEX IF NOT EXISTS IX_PartnerHealthStatus_PartnerId
    ON PartnerHealthStatus (PartnerId, TestedDate);

CREATE INDEX IF NOT EXISTS IX_Activity_OccurredDate
    ON Activity (OccurredDate);

CREATE INDEX IF NOT EXISTS IX_Activity_PartnerId
    ON Activity (PartnerId);

CREATE INDEX IF NOT EXISTS IX_LookupItem_CategoryId
    ON LookupItem (CategoryId, SortOrder);
