using Microsoft.EntityFrameworkCore;
using ProyectoIndursa.IndursaContext;

namespace ProyectoIndursa.Data;

public static class Schema
{
    public static void Ensure(IndursaDB db)
    {
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS movimiento (
              Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              No_Cuenta INT NOT NULL,
              Tipo TEXT NOT NULL,
              Monto REAL NOT NULL,
              Saldo_Resultante REAL NOT NULL,
              Referencia TEXT,
              Concepto TEXT,
              Fecha TEXT NOT NULL
            );
            """);

        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS notificacion (
              Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              No_Cuenta INT NOT NULL,
              Mensaje TEXT NOT NULL,
              Leida INTEGER NOT NULL DEFAULT 0,
              Fecha TEXT NOT NULL
            );
            """);

        TryAddColumn(db, "cuenta", "Motivo_Rechazo", "TEXT");
        TryAddColumn(db, "estado_prestamo", "Motivo_Rechazo", "TEXT");
        TryAddColumn(db, "rifa", "Ganador", "INTEGER NOT NULL DEFAULT 0");
        TryAddColumn(db, "empleado", "Dias_Vacaciones", "INTEGER NOT NULL DEFAULT 0");
    }

    private static void TryAddColumn(IndursaDB db, string table, string column, string type)
    {
        try
        {
            db.Database.ExecuteSqlRaw($"ALTER TABLE {table} ADD COLUMN {column} {type}");
        }
        catch (Exception)
        {
            // La columna ya existe.
        }
    }
}
