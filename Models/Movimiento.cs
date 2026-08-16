using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIndursa.Models;

[Table("movimiento")]
public class Movimiento
{
    [Key]
    public long Id { get; set; }

    [Column("No_Cuenta")]
    public int NoCuenta { get; set; }

    [Column(TypeName = "varchar(40)")]
    public string Tipo { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    [Column("Saldo_Resultante")]
    public decimal SaldoResultante { get; set; }

    [Column(TypeName = "varchar(80)")]
    public string? Referencia { get; set; }

    [Column(TypeName = "varchar(200)")]
    public string Concepto { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }
}
