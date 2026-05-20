using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystemAI.Infrastructure.Migrations
{
    public partial class AddCompanyMigrationExternalIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceCompanyId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "AspNetUsers",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceCompanyId",
                table: "Apartments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Apartments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SourceCompanyId_ExternalId",
                table: "AspNetUsers",
                columns: new[] { "SourceCompanyId", "ExternalId" },
                unique: true,
                filter: "\"SourceCompanyId\" IS NOT NULL AND \"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Apartments_SourceCompanyId_ExternalId",
                table: "Apartments",
                columns: new[] { "SourceCompanyId", "ExternalId" },
                unique: true,
                filter: "\"SourceCompanyId\" IS NOT NULL AND \"ExternalId\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Apartments_SourceCompanyId_ExternalId",
                table: "Apartments");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SourceCompanyId_ExternalId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "SourceCompanyId",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SourceCompanyId",
                table: "AspNetUsers");
        }
    }
}
