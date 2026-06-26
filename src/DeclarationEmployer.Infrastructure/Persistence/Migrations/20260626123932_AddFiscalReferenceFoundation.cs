using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeclarationEmployer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFiscalReferenceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fiscal");

            migrationBuilder.CreateTable(
                name: "rule_sets",
                schema: "fiscal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    source_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    source_reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rule_sets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "annex_definitions",
                schema: "fiscal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_official_mapping_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annex_definitions", x => x.id);
                    table.ForeignKey(
                        name: "FK_annex_definitions_rule_sets_rule_set_id",
                        column: x => x.rule_set_id,
                        principalSchema: "fiscal",
                        principalTable: "rule_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rate_definitions",
                schema: "fiscal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    label = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    rate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    source_reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rate_definitions", x => x.id);
                    table.ForeignKey(
                        name: "FK_rate_definitions_rule_sets_rule_set_id",
                        column: x => x.rule_set_id,
                        principalSchema: "fiscal",
                        principalTable: "rule_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "field_definitions",
                schema: "fiscal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    annex_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    label = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    data_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    length = table.Column<int>(type: "integer", nullable: true),
                    position_start = table.Column<int>(type: "integer", nullable: true),
                    position_end = table.Column<int>(type: "integer", nullable: true),
                    padding_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    default_value = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    source_reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_definitions", x => x.id);
                    table.ForeignKey(
                        name: "FK_field_definitions_annex_definitions_annex_definition_id",
                        column: x => x.annex_definition_id,
                        principalSchema: "fiscal",
                        principalTable: "annex_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_annex_definitions_is_active",
                schema: "fiscal",
                table: "annex_definitions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_annex_definitions_is_official_mapping_confirmed",
                schema: "fiscal",
                table: "annex_definitions",
                column: "is_official_mapping_confirmed");

            migrationBuilder.CreateIndex(
                name: "IX_annex_definitions_rule_set_id_code",
                schema: "fiscal",
                table: "annex_definitions",
                columns: new[] { "rule_set_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_field_definitions_annex_definition_id_code",
                schema: "fiscal",
                table: "field_definitions",
                columns: new[] { "annex_definition_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_field_definitions_is_confirmed",
                schema: "fiscal",
                table: "field_definitions",
                column: "is_confirmed");

            migrationBuilder.CreateIndex(
                name: "IX_rate_definitions_is_confirmed",
                schema: "fiscal",
                table: "rate_definitions",
                column: "is_confirmed");

            migrationBuilder.CreateIndex(
                name: "IX_rate_definitions_rule_set_id_code",
                schema: "fiscal",
                table: "rate_definitions",
                columns: new[] { "rule_set_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rule_sets_is_active",
                schema: "fiscal",
                table: "rule_sets",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_rule_sets_year_code",
                schema: "fiscal",
                table: "rule_sets",
                columns: new[] { "year", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "field_definitions",
                schema: "fiscal");

            migrationBuilder.DropTable(
                name: "rate_definitions",
                schema: "fiscal");

            migrationBuilder.DropTable(
                name: "annex_definitions",
                schema: "fiscal");

            migrationBuilder.DropTable(
                name: "rule_sets",
                schema: "fiscal");
        }
    }
}
