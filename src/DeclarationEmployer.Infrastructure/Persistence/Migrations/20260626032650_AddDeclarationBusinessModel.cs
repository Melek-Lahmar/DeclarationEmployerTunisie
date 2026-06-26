using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeclarationEmployer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeclarationBusinessModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "archive");

            migrationBuilder.CreateTable(
                name: "archived_documents",
                schema: "archive",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    declaration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fiscal_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    relative_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    sha256_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    archived_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    archived_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archived_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_archived_documents_clients_client_company_id",
                        column: x => x.client_company_id,
                        principalSchema: "cabinet",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_archived_documents_declarations_declaration_id",
                        column: x => x.declaration_id,
                        principalSchema: "declaration",
                        principalTable: "declarations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_archived_documents_fiscal_years_fiscal_year_id",
                        column: x => x.fiscal_year_id,
                        principalSchema: "cabinet",
                        principalTable: "fiscal_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "declaration_annexes",
                schema: "declaration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    declaration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    annex_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_declaration_annexes", x => x.id);
                    table.ForeignKey(
                        name: "FK_declaration_annexes_declarations_declaration_id",
                        column: x => x.declaration_id,
                        principalSchema: "declaration",
                        principalTable: "declarations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "declaration_anomalies",
                schema: "declaration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    declaration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    entity_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resolved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_declaration_anomalies", x => x.id);
                    table.ForeignKey(
                        name: "FK_declaration_anomalies_declarations_declaration_id",
                        column: x => x.declaration_id,
                        principalSchema: "declaration",
                        principalTable: "declarations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "declaration_beneficiaries",
                schema: "declaration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    declaration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identifier_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    full_name_or_company_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_resident = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_declaration_beneficiaries", x => x.id);
                    table.ForeignKey(
                        name: "FK_declaration_beneficiaries_declarations_declaration_id",
                        column: x => x.declaration_id,
                        principalSchema: "declaration",
                        principalTable: "declarations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "generated_files",
                schema: "declaration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    declaration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    relative_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    sha256_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generated_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_generated_files_declarations_declaration_id",
                        column: x => x.declaration_id,
                        principalSchema: "declaration",
                        principalTable: "declarations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "declaration_lines",
                schema: "declaration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    declaration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    annex_id = table.Column<Guid>(type: "uuid", nullable: true),
                    beneficiary_id = table.Column<Guid>(type: "uuid", nullable: true),
                    operation_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fiscal_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    gross_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    taxable_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    rate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    document_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_declaration_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_declaration_lines_declaration_annexes_annex_id",
                        column: x => x.annex_id,
                        principalSchema: "declaration",
                        principalTable: "declaration_annexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_declaration_lines_declaration_beneficiaries_beneficiary_id",
                        column: x => x.beneficiary_id,
                        principalSchema: "declaration",
                        principalTable: "declaration_beneficiaries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_declaration_lines_declarations_declaration_id",
                        column: x => x.declaration_id,
                        principalSchema: "declaration",
                        principalTable: "declarations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_archived_documents_client_company_id_fiscal_year_id",
                schema: "archive",
                table: "archived_documents",
                columns: new[] { "client_company_id", "fiscal_year_id" });

            migrationBuilder.CreateIndex(
                name: "IX_archived_documents_declaration_id",
                schema: "archive",
                table: "archived_documents",
                column: "declaration_id");

            migrationBuilder.CreateIndex(
                name: "IX_archived_documents_document_type",
                schema: "archive",
                table: "archived_documents",
                column: "document_type");

            migrationBuilder.CreateIndex(
                name: "IX_archived_documents_fiscal_year_id",
                schema: "archive",
                table: "archived_documents",
                column: "fiscal_year_id");

            migrationBuilder.CreateIndex(
                name: "IX_declaration_annexes_declaration_id",
                schema: "declaration",
                table: "declaration_annexes",
                column: "declaration_id");

            migrationBuilder.CreateIndex(
                name: "IX_declaration_annexes_declaration_id_annex_code",
                schema: "declaration",
                table: "declaration_annexes",
                columns: new[] { "declaration_id", "annex_code" });

            migrationBuilder.CreateIndex(
                name: "IX_declaration_anomalies_declaration_id",
                schema: "declaration",
                table: "declaration_anomalies",
                column: "declaration_id");

            migrationBuilder.CreateIndex(
                name: "IX_declaration_anomalies_is_resolved",
                schema: "declaration",
                table: "declaration_anomalies",
                column: "is_resolved");

            migrationBuilder.CreateIndex(
                name: "IX_declaration_anomalies_severity",
                schema: "declaration",
                table: "declaration_anomalies",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "IX_declaration_beneficiaries_declaration_id",
                schema: "declaration",
                table: "declaration_beneficiaries",
                column: "declaration_id");

            migrationBuilder.CreateIndex(
                name: "IX_declaration_beneficiaries_declaration_id_identifier",
                schema: "declaration",
                table: "declaration_beneficiaries",
                columns: new[] { "declaration_id", "identifier" });

            migrationBuilder.CreateIndex(
                name: "IX_declaration_lines_annex_id",
                schema: "declaration",
                table: "declaration_lines",
                column: "annex_id");

            migrationBuilder.CreateIndex(
                name: "IX_declaration_lines_beneficiary_id",
                schema: "declaration",
                table: "declaration_lines",
                column: "beneficiary_id");

            migrationBuilder.CreateIndex(
                name: "IX_declaration_lines_declaration_id",
                schema: "declaration",
                table: "declaration_lines",
                column: "declaration_id");

            migrationBuilder.CreateIndex(
                name: "IX_declaration_lines_status",
                schema: "declaration",
                table: "declaration_lines",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_generated_files_declaration_id",
                schema: "declaration",
                table: "generated_files",
                column: "declaration_id");

            migrationBuilder.CreateIndex(
                name: "IX_generated_files_file_type",
                schema: "declaration",
                table: "generated_files",
                column: "file_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "archived_documents",
                schema: "archive");

            migrationBuilder.DropTable(
                name: "declaration_anomalies",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "declaration_lines",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "generated_files",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "declaration_annexes",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "declaration_beneficiaries",
                schema: "declaration");
        }
    }
}
