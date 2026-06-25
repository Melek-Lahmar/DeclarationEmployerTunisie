using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeclarationEmployer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeclarationsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "declaration");

            migrationBuilder.CreateTable(
                name: "declarations",
                schema: "declaration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fiscal_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    locked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    locked_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_declarations", x => x.id);
                    table.ForeignKey(
                        name: "FK_declarations_clients_client_company_id",
                        column: x => x.client_company_id,
                        principalSchema: "cabinet",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_declarations_fiscal_years_fiscal_year_id",
                        column: x => x.fiscal_year_id,
                        principalSchema: "cabinet",
                        principalTable: "fiscal_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "declaration_events",
                schema: "declaration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    declaration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_declaration_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_declaration_events_declarations_declaration_id",
                        column: x => x.declaration_id,
                        principalSchema: "declaration",
                        principalTable: "declarations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_declaration_events_declaration_id",
                schema: "declaration",
                table: "declaration_events",
                column: "declaration_id");

            migrationBuilder.CreateIndex(
                name: "IX_declaration_events_occurred_at",
                schema: "declaration",
                table: "declaration_events",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_declarations_client_company_id_fiscal_year_id",
                schema: "declaration",
                table: "declarations",
                columns: new[] { "client_company_id", "fiscal_year_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_declarations_fiscal_year_id",
                schema: "declaration",
                table: "declarations",
                column: "fiscal_year_id");

            migrationBuilder.CreateIndex(
                name: "IX_declarations_status",
                schema: "declaration",
                table: "declarations",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "declaration_events",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "declarations",
                schema: "declaration");
        }
    }
}
