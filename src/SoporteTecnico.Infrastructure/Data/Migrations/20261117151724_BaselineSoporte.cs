
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoporteTecnico.Infrastructure.Data.Migrations
{
    public partial class BaselineSoporte : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La estructura ya existe en la base de datos.
            // Esta migración solo establece el punto de referencia.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No eliminar las tablas existentes al revertir el baseline.
        }
    }
}
