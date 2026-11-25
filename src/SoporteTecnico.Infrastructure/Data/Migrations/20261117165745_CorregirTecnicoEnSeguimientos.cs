using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoporteTecnico.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorregirTecnicoEnSeguimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Seguimientos_TipoTecnico",
                table: "Seguimientos");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Seguimientos_TipoTecnico",
                table: "Seguimientos",
                sql: "([Tipo] = 'Creado' AND [TecnicoId] IS NULL)\nOR\n([Tipo] IN ('Asignado', 'Reasignado', 'Resuelto', 'Cerrado')\n    AND [TecnicoId] IS NOT NULL)\nOR\n([Tipo] = 'Nota')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Seguimientos_TipoTecnico",
                table: "Seguimientos");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Seguimientos_TipoTecnico",
                table: "Seguimientos",
                sql: "([Tipo] IN ('Asignado', 'Reasignado') AND [TecnicoId] IS NOT NULL)\nOR\n([Tipo] IN ('Creado', 'Nota', 'Resuelto', 'Cerrado')\n    AND [TecnicoId] IS NULL)");
        }
    }
}
