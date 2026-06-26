using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeclarationEmployer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddValidationAndAnomaliesFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "validation");

            migrationBuilder.CreateTable(
                name: "validation_runs",
                schema: "validation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    declaration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    blocking_count = table.Column<int>(type: "integer", nullable: false),
                    warning_count = table.Column<int>(type: "integer", nullable: false),
                    info_count = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validation_runs", x => x.id);
                    table.ForeignKey(
                        name: "FK_validation_runs_declarations_declaration_id",
                        column: x => x.declaration_id,
                        principalSchema: "declaration",
                        principalTable: "declarations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "validation_results",
                schema: "validation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    validation_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    declaration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    annex_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    line_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    field_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    justification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validation_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_validation_results_declarations_declaration_id",
                        column: x => x.declaration_id,
                        principalSchema: "declaration",
                        principalTable: "declarations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_validation_results_validation_runs_validation_run_id",
                        column: x => x.validation_run_id,
                        principalSchema: "validation",
                        principalTable: "validation_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_validation_results_declaration_id",
                schema: "validation",
                table: "validation_results",
                column: "declaration_id");

            migrationBuilder.CreateIndex(
                name: "IX_validation_results_severity",
                schema: "validation",
                table: "validation_results",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "IX_validation_results_status",
                schema: "validation",
                table: "validation_results",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_validation_results_validation_run_id",
                schema: "validation",
                table: "validation_results",
                column: "validation_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_validation_runs_declaration_id",
                schema: "validation",
                table: "validation_runs",
                column: "declaration_id");

            migrationBuilder.CreateIndex(
                name: "IX_validation_runs_started_at",
                schema: "validation",
                table: "validation_runs",
                column: "started_at");

            migrationBuilder.CreateIndex(
                name: "IX_validation_runs_status",
                schema: "validation",
                table: "validation_runs",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "validation_results",
                schema: "validation");

            migrationBuilder.DropTable(
                name: "validation_runs",
                schema: "validation");
        }
    }
}
