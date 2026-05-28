using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoWatch.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsuarioPerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Localidade",
                table: "USUARIOS",
                type: "NVARCHAR2(2000)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RaioAlertasKm",
                table: "USUARIOS",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Localidade",
                table: "USUARIOS");

            migrationBuilder.DropColumn(
                name: "RaioAlertasKm",
                table: "USUARIOS");
        }
    }
}
