using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeclarationEmployer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBackupManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "backup");

            migrationBuilder.CreateTable(
                name: "backup_records",
                schema: "backup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    stored_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    sha256_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_backup_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "backup_events",
                schema: "backup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    backup_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_backup_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_backup_events_backup_records_backup_record_id",
                        column: x => x.backup_record_id,
                        principalSchema: "backup",
                        principalTable: "backup_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_backup_events_backup_record_id",
                schema: "backup",
                table: "backup_events",
                column: "backup_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_backup_events_occurred_at",
                schema: "backup",
                table: "backup_events",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_backup_records_created_at",
                schema: "backup",
                table: "backup_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_backup_records_status",
                schema: "backup",
                table: "backup_records",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "backup_events",
                schema: "backup");

            migrationBuilder.DropTable(
                name: "backup_records",
                schema: "backup");
        }
    }
}
