using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeclarationEmployer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpccaOfficialAnnexModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "annex_number",
                schema: "declaration",
                table: "generated_files",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "encoding",
                schema: "declaration",
                table: "generated_files",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "expected_line_length",
                schema: "declaration",
                table: "generated_files",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_official",
                schema: "declaration",
                table: "generated_files",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "line_count",
                schema: "declaration",
                table: "generated_files",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "validation_status",
                schema: "declaration",
                table: "generated_files",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "act_code",
                schema: "declaration",
                table: "declarations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "activity",
                schema: "declaration",
                table: "declaration_beneficiaries",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "job_title",
                schema: "declaration",
                table: "declaration_beneficiaries",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero_adresse",
                schema: "cabinet",
                table: "clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "annex_a1_details",
                schema: "declaration",
                columns: table => new
                {
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_situation = table.Column<int>(type: "integer", nullable: false),
                    dependent_children_count = table.Column<int>(type: "integer", nullable: false),
                    work_period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    work_period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    work_period_days = table.Column<int>(type: "integer", nullable: false),
                    taxable_income = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    benefits_in_kind = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    gross_taxable_income = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    reinvested_income = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    common_regime_withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    foreign_employee_withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    social_solidarity_contribution = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    net_paid_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annex_a1_details", x => x.line_id);
                    table.ForeignKey(
                        name: "FK_annex_a1_details_declaration_lines_line_id",
                        column: x => x.line_id,
                        principalSchema: "declaration",
                        principalTable: "declaration_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "annex_a2_details",
                schema: "declaration",
                columns: table => new
                {
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_type = table.Column<int>(type: "integer", nullable: false),
                    gross_professional_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    real_regime_fees_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    board_and_securities_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    occasional_work_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    real_estate_capital_gain_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    hotel_rent_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    artist_remuneration_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    public_sector_vat_withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    net_paid_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annex_a2_details", x => x.line_id);
                    table.ForeignKey(
                        name: "FK_annex_a2_details_declaration_lines_line_id",
                        column: x => x.line_id,
                        principalSchema: "declaration",
                        principalTable: "declaration_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "annex_a3_details",
                schema: "declaration",
                columns: table => new
                {
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    savings_account_interest = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    other_movable_capital_income = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    non_established_bank_loan_interest = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    net_paid_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annex_a3_details", x => x.line_id);
                    table.ForeignKey(
                        name: "FK_annex_a3_details_declaration_lines_line_id",
                        column: x => x.line_id,
                        principalSchema: "declaration",
                        principalTable: "declaration_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "annex_a4_details",
                schema: "declaration",
                columns: table => new
                {
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_type = table.Column<int>(type: "integer", nullable: false),
                    professional_amount_rate = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    professional_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    construction_work_rate = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    construction_work_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    real_estate_capital_gain_rate = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    real_estate_capital_gain_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    securities_capital_gain_rate = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    securities_capital_gain_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    securities_income_rate = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    securities_income_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    privileged_tax_regime_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    vat_withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    net_paid_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annex_a4_details", x => x.line_id);
                    table.ForeignKey(
                        name: "FK_annex_a4_details_declaration_lines_line_id",
                        column: x => x.line_id,
                        principalSchema: "declaration",
                        principalTable: "declaration_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "annex_a5_details",
                schema: "declaration",
                columns: table => new
                {
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchases_from_ten_percent_companies = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    purchases_from_fifteen_percent_companies = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    purchases_from_two_thirds_deduction_businesses = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    purchases_from_other_businesses = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    vat_withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    delivery_platform_three_percent_withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    net_paid_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annex_a5_details", x => x.line_id);
                    table.ForeignKey(
                        name: "FK_annex_a5_details_declaration_lines_line_id",
                        column: x => x.line_id,
                        principalSchema: "declaration",
                        principalTable: "declaration_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "annex_a6_details",
                schema: "declaration",
                columns: table => new
                {
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rebate_type = table.Column<int>(type: "integer", nullable: false),
                    rebate_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    flat_regime_sales_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    flat_regime_sales_advance_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    gambling_income_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    gambling_withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    distribution_network_sales_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    distribution_network_withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    cash_collections_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    alcohol_sales_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    alcohol_sales_advance_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annex_a6_details", x => x.line_id);
                    table.ForeignKey(
                        name: "FK_annex_a6_details_declaration_lines_line_id",
                        column: x => x.line_id,
                        principalSchema: "declaration",
                        principalTable: "declaration_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "annex_a7_details",
                schema: "declaration",
                columns: table => new
                {
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paid_amount_type = table.Column<int>(type: "integer", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    withheld_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    net_paid_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annex_a7_details", x => x.line_id);
                    table.ForeignKey(
                        name: "FK_annex_a7_details_declaration_lines_line_id",
                        column: x => x.line_id,
                        principalSchema: "declaration",
                        principalTable: "declaration_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "annex_a1_details",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "annex_a2_details",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "annex_a3_details",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "annex_a4_details",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "annex_a5_details",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "annex_a6_details",
                schema: "declaration");

            migrationBuilder.DropTable(
                name: "annex_a7_details",
                schema: "declaration");

            migrationBuilder.DropColumn(
                name: "annex_number",
                schema: "declaration",
                table: "generated_files");

            migrationBuilder.DropColumn(
                name: "encoding",
                schema: "declaration",
                table: "generated_files");

            migrationBuilder.DropColumn(
                name: "expected_line_length",
                schema: "declaration",
                table: "generated_files");

            migrationBuilder.DropColumn(
                name: "is_official",
                schema: "declaration",
                table: "generated_files");

            migrationBuilder.DropColumn(
                name: "line_count",
                schema: "declaration",
                table: "generated_files");

            migrationBuilder.DropColumn(
                name: "validation_status",
                schema: "declaration",
                table: "generated_files");

            migrationBuilder.DropColumn(
                name: "act_code",
                schema: "declaration",
                table: "declarations");

            migrationBuilder.DropColumn(
                name: "activity",
                schema: "declaration",
                table: "declaration_beneficiaries");

            migrationBuilder.DropColumn(
                name: "job_title",
                schema: "declaration",
                table: "declaration_beneficiaries");

            migrationBuilder.DropColumn(
                name: "numero_adresse",
                schema: "cabinet",
                table: "clients");
        }
    }
}
