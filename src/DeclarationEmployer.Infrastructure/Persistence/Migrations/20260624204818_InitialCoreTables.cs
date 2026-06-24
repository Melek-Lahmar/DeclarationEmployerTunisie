using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeclarationEmployer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "cabinet");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    entity_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    details = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "cabinet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raison_sociale = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    matricule_fiscal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cle = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    categorie = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    code_tva = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    etablissement = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    activite = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    adresse = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ville = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    code_postal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    telephone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fiscal_years",
                schema: "cabinet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_years", x => x.id);
                    table.ForeignKey(
                        name: "FK_fiscal_years_clients_client_company_id",
                        column: x => x.client_company_id,
                        principalSchema: "cabinet",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_action",
                schema: "audit",
                table: "audit_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_entity_name",
                schema: "audit",
                table: "audit_logs",
                column: "entity_name");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_occurred_at",
                schema: "audit",
                table: "audit_logs",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_clients_code",
                schema: "cabinet",
                table: "clients",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clients_matricule_fiscal",
                schema: "cabinet",
                table: "clients",
                column: "matricule_fiscal");

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_years_client_company_id_year",
                schema: "cabinet",
                table: "fiscal_years",
                columns: new[] { "client_company_id", "year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "auth",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_user_name",
                schema: "auth",
                table: "users",
                column: "user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "fiscal_years",
                schema: "cabinet");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "clients",
                schema: "cabinet");
        }
    }
}
