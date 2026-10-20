using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Pennywise.Api.Data;

namespace Pennywise.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610210001_InitialUnifiedSchema")]
public sealed class InitialUnifiedSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE IF NOT EXISTS "Users" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
                "FullName" TEXT NOT NULL,
                "Email" TEXT NOT NULL,
                "PasswordHash" TEXT NOT NULL,
                "Phone" TEXT NULL,
                "Country" TEXT NULL,
                "CreatedAt" TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");

            CREATE TABLE IF NOT EXISTS "Preferences" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Preferences" PRIMARY KEY AUTOINCREMENT,
                "UserId" INTEGER NOT NULL,
                "Currency" TEXT NOT NULL,
                "Language" TEXT NOT NULL,
                "Theme" TEXT NOT NULL,
                "DateFormat" TEXT NOT NULL,
                "EmailNotifications" INTEGER NOT NULL,
                "BudgetAlerts" INTEGER NOT NULL,
                "MonthlySummary" INTEGER NOT NULL,
                CONSTRAINT "FK_Preferences_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Preferences_UserId" ON "Preferences" ("UserId");

            CREATE TABLE IF NOT EXISTS "Categories" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Categories" PRIMARY KEY AUTOINCREMENT,
                "UserId" INTEGER NOT NULL,
                "Name" TEXT NOT NULL,
                "Type" TEXT NOT NULL,
                "Icon" TEXT NOT NULL,
                "ColorIndex" INTEGER NOT NULL,
                CONSTRAINT "AK_Categories_Id_UserId" UNIQUE ("Id", "UserId"),
                CONSTRAINT "FK_Categories_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Categories_UserId_Type_Name"
                ON "Categories" ("UserId", "Type", "Name" COLLATE NOCASE);

            CREATE TABLE IF NOT EXISTS "Transactions" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY AUTOINCREMENT,
                "UserId" INTEGER NOT NULL,
                "CategoryId" INTEGER NOT NULL,
                "Amount" TEXT NOT NULL,
                "Date" TEXT NOT NULL,
                "Note" TEXT NULL,
                CONSTRAINT "FK_Transactions_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_Transactions_Categories_CategoryId_UserId"
                    FOREIGN KEY ("CategoryId", "UserId")
                    REFERENCES "Categories" ("Id", "UserId") ON DELETE RESTRICT
            );
            CREATE INDEX IF NOT EXISTS "IX_Transactions_UserId_Date"
                ON "Transactions" ("UserId", "Date");

            CREATE TABLE IF NOT EXISTS "UserBudgets" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_UserBudgets" PRIMARY KEY AUTOINCREMENT,
                "UserId" INTEGER NOT NULL,
                "CategoryId" INTEGER NOT NULL,
                "Name" TEXT NOT NULL,
                "Limit" TEXT NOT NULL,
                CONSTRAINT "FK_UserBudgets_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_UserBudgets_Categories_CategoryId_UserId"
                    FOREIGN KEY ("CategoryId", "UserId")
                    REFERENCES "Categories" ("Id", "UserId") ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserBudgets_UserId_CategoryId"
                ON "UserBudgets" ("UserId", "CategoryId");

            CREATE TABLE IF NOT EXISTS "UserGoals" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_UserGoals" PRIMARY KEY AUTOINCREMENT,
                "UserId" INTEGER NOT NULL,
                "Name" TEXT NOT NULL,
                "Target" TEXT NOT NULL,
                "Saved" TEXT NOT NULL,
                "Date" TEXT NULL,
                CONSTRAINT "FK_UserGoals_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserGoals_UserId_Name"
                ON "UserGoals" ("UserId", "Name" COLLATE NOCASE);

            CREATE TRIGGER IF NOT EXISTS "TR_Transactions_UserCategory_Insert"
            BEFORE INSERT ON "Transactions"
            WHEN NOT EXISTS (
                SELECT 1 FROM "Categories"
                WHERE "Id" = NEW."CategoryId" AND "UserId" = NEW."UserId"
            )
            BEGIN
                SELECT RAISE(ABORT, 'Transaction category must belong to its user.');
            END;

            CREATE TRIGGER IF NOT EXISTS "TR_Transactions_UserCategory_Update"
            BEFORE UPDATE OF "UserId", "CategoryId" ON "Transactions"
            WHEN NOT EXISTS (
                SELECT 1 FROM "Categories"
                WHERE "Id" = NEW."CategoryId" AND "UserId" = NEW."UserId"
            )
            BEGIN
                SELECT RAISE(ABORT, 'Transaction category must belong to its user.');
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException(
            "This additive migration cannot be rolled back without risking existing user data.");
}
