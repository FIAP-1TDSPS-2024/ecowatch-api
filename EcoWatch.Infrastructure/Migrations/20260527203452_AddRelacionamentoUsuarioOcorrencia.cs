using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoWatch.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRelacionamentoUsuarioOcorrencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId",
                table: "OCORRENCIAS",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_OCORRENCIAS_UsuarioId",
                table: "OCORRENCIAS",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_OCORRENCIAS_USUARIOS_UsuarioId",
                table: "OCORRENCIAS",
                column: "UsuarioId",
                principalTable: "USUARIOS",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OCORRENCIAS_USUARIOS_UsuarioId",
                table: "OCORRENCIAS");

            migrationBuilder.DropIndex(
                name: "IX_OCORRENCIAS_UsuarioId",
                table: "OCORRENCIAS");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "OCORRENCIAS");
        }
    }
}
