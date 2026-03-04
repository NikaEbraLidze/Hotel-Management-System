using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveManagerAndGuestEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Guests_GuestId",
                table: "Reservations");

            migrationBuilder.Sql(
                """
                INSERT INTO [AspNetUsers]
                    ([Id], [FirstName], [LastName], [PersonalNumber], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
                     [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed],
                     [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
                SELECT
                    g.[Id],
                    g.[FirstName],
                    g.[LastName],
                    g.[PersonalNumber],
                    CONCAT('guest_', REPLACE(CONVERT(varchar(36), g.[Id]), '-', '')),
                    UPPER(CONCAT('guest_', REPLACE(CONVERT(varchar(36), g.[Id]), '-', ''))),
                    NULL,
                    NULL,
                    CAST(0 AS bit),
                    NULL,
                    CONVERT(nvarchar(36), NEWID()),
                    CONVERT(nvarchar(36), NEWID()),
                    g.[PhoneNumber],
                    CAST(0 AS bit),
                    CAST(0 AS bit),
                    NULL,
                    CAST(0 AS bit),
                    0
                FROM [Guests] g
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [AspNetUsers] u
                    WHERE u.[Id] = g.[Id]
                );
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [AspNetUsers]
                    ([Id], [FirstName], [LastName], [PersonalNumber], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
                     [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed],
                     [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
                SELECT
                    m.[Id],
                    m.[FirstName],
                    m.[LastName],
                    m.[PersonalNumber],
                    CONCAT('manager_', REPLACE(CONVERT(varchar(36), m.[Id]), '-', '')),
                    UPPER(CONCAT('manager_', REPLACE(CONVERT(varchar(36), m.[Id]), '-', ''))),
                    m.[Email],
                    UPPER(m.[Email]),
                    CAST(0 AS bit),
                    NULL,
                    CONVERT(nvarchar(36), NEWID()),
                    CONVERT(nvarchar(36), NEWID()),
                    m.[PhoneNumber],
                    CAST(0 AS bit),
                    CAST(0 AS bit),
                    NULL,
                    CAST(0 AS bit),
                    0
                FROM [Managers] m
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [AspNetUsers] u
                    WHERE u.[Id] = m.[Id]
                );
                """);

            migrationBuilder.DropTable(
                name: "Guests");

            migrationBuilder.DropTable(
                name: "Managers");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_GuestId",
                table: "Reservations",
                column: "GuestId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_GuestId",
                table: "Reservations");

            migrationBuilder.CreateTable(
                name: "Guests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PersonalNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Managers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PersonalNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Managers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Managers_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Guests_PersonalNumber",
                table: "Guests",
                column: "PersonalNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guests_PhoneNumber",
                table: "Guests",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_Email",
                table: "Managers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_HotelId",
                table: "Managers",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_PersonalNumber",
                table: "Managers",
                column: "PersonalNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_PhoneNumber",
                table: "Managers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO [Guests] ([Id], [FirstName], [LastName], [PersonalNumber], [PhoneNumber])
                SELECT DISTINCT
                    u.[Id],
                    COALESCE(NULLIF(u.[FirstName], ''), 'Guest'),
                    COALESCE(NULLIF(u.[LastName], ''), 'User'),
                    COALESCE(NULLIF(u.[PersonalNumber], ''), LEFT(REPLACE(CONVERT(varchar(36), u.[Id]), '-', ''), 11)),
                    COALESCE(NULLIF(u.[PhoneNumber], ''), CONCAT('+', LEFT(REPLACE(CONVERT(varchar(36), u.[Id]), '-', ''), 14)))
                FROM [AspNetUsers] u
                INNER JOIN [Reservations] r ON r.[GuestId] = u.[Id]
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [Guests] g
                    WHERE g.[Id] = u.[Id]
                );
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Guests_GuestId",
                table: "Reservations",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
