using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIO.Migrations.Migrations.SIO.GoogleSynthesizerStore
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "log");

            migrationBuilder.CreateTable(
                name: "Command",
                schema: "log",
                columns: table => new
                {
                    SequenceNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Command", x => x.SequenceNo);
                });

            migrationBuilder.CreateTable(
                name: "Event",
                schema: "log",
                columns: table => new
                {
                    SequenceNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StreamId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CausationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Actor = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ScheduledPublication = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.SequenceNo);
                });

            migrationBuilder.CreateTable(
                name: "Query",
                schema: "log",
                columns: table => new
                {
                    SequenceNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Query", x => x.SequenceNo);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Command_CorrelationId",
                schema: "log",
                table: "Command",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Command_Id",
                schema: "log",
                table: "Command",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Command_Name",
                schema: "log",
                table: "Command",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Command_Subject",
                schema: "log",
                table: "Command",
                column: "Subject");

            migrationBuilder.CreateIndex(
                name: "IX_Command_UserId",
                schema: "log",
                table: "Command",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_Actor",
                schema: "log",
                table: "Event",
                column: "Actor");

            migrationBuilder.CreateIndex(
                name: "IX_Event_CausationId",
                schema: "log",
                table: "Event",
                column: "CausationId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_CorrelationId",
                schema: "log",
                table: "Event",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_Id",
                schema: "log",
                table: "Event",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Event_Name",
                schema: "log",
                table: "Event",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Event_StreamId",
                schema: "log",
                table: "Event",
                column: "StreamId");

            migrationBuilder.CreateIndex(
                name: "IX_Query_CorrelationId",
                schema: "log",
                table: "Query",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Query_Id",
                schema: "log",
                table: "Query",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Query_Name",
                schema: "log",
                table: "Query",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Query_UserId",
                schema: "log",
                table: "Query",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Command",
                schema: "log");

            migrationBuilder.DropTable(
                name: "Event",
                schema: "log");

            migrationBuilder.DropTable(
                name: "Query",
                schema: "log");
        }
    }
}
