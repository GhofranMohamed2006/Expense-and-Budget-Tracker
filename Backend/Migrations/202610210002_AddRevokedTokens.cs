using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Pennywise.Api.Data;

namespace Pennywise.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610210002_AddRevokedTokens")]
public sealed class AddRevokedTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE IF NOT EXISTS "RevokedTokens" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_RevokedTokens" PRIMARY KEY AUTOINCREMENT,
                "TokenId" TEXT NOT NULL,
                "ExpiresAt" TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_RevokedTokens_TokenId"
                ON "RevokedTokens" ("TokenId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException(
            "This migration cannot be rolled back without revalidating active sessions.");
}
