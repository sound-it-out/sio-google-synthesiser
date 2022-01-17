using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIO.Migrations.Migrations.SIO.Projection
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoogleSynthesizeFailure",
                columns: table => new
                {
                    Subject = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentSubject = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleSynthesizeFailure", x => x.Subject);
                });

            migrationBuilder.CreateTable(
                name: "GoogleSynthesizeQueue",
                columns: table => new
                {
                    Subject = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentSubject = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleSynthesizeQueue", x => x.Subject);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionState",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionState", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSynthesizeFailure_DocumentSubject",
                table: "GoogleSynthesizeFailure",
                column: "DocumentSubject");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSynthesizeQueue_DocumentSubject",
                table: "GoogleSynthesizeQueue",
                column: "DocumentSubject");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoogleSynthesizeFailure");

            migrationBuilder.DropTable(
                name: "GoogleSynthesizeQueue");

            migrationBuilder.DropTable(
                name: "ProjectionState");
        }
    }
}
