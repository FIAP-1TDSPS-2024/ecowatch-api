using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoWatch.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OCORRENCIAS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Latitude = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Longitude = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    TipoOcorrencia = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    DetalhesAdicionais = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DataOcorrenciaUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OCORRENCIAS", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OCORRENCIAS");
        }
    }
}
