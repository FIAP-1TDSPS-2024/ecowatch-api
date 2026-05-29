using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoWatch.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNovosCamposOcorrencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Limpeza Idempotente: Remove as colunas presas da tentativa anterior.
            // O erro -904 (coluna não encontrada) é ignorado para garantir a execução segura.
            migrationBuilder.Sql(@"
                BEGIN
                    EXECUTE IMMEDIATE 'ALTER TABLE ""OCORRENCIAS"" DROP COLUMN ""Area""';
                EXCEPTION WHEN OTHERS THEN
                    IF SQLCODE != -904 THEN RAISE; END IF;
                END;
            ");

            migrationBuilder.Sql(@"
                BEGIN
                    EXECUTE IMMEDIATE 'ALTER TABLE ""OCORRENCIAS"" DROP COLUMN ""Distancia""';
                EXCEPTION WHEN OTHERS THEN
                    IF SQLCODE != -904 THEN RAISE; END IF;
                END;
            ");

            migrationBuilder.Sql(@"
                BEGIN
                    EXECUTE IMMEDIATE 'ALTER TABLE ""OCORRENCIAS"" DROP COLUMN ""Urgencia""';
                EXCEPTION WHEN OTHERS THEN
                    IF SQLCODE != -904 THEN RAISE; END IF;
                END;
            ");

            // Recria as colunas corretamente como Nullable
            migrationBuilder.AddColumn<double>(
                name: "Area",
                table: "OCORRENCIAS",
                type: "BINARY_DOUBLE",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Distancia",
                table: "OCORRENCIAS",
                type: "BINARY_DOUBLE",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Urgencia",
                table: "OCORRENCIAS",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "OCORRENCIAS");

            migrationBuilder.DropColumn(
                name: "Distancia",
                table: "OCORRENCIAS");

            migrationBuilder.DropColumn(
                name: "Urgencia",
                table: "OCORRENCIAS");
        }
    }
}