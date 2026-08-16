using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIndursa.Models;

[Table("notificacion")]
public class Notificacion
{
    [Key]
    public long Id { get; set; }

    [Column("No_Cuenta")]
    public int NoCuenta { get; set; }

    [Column(TypeName = "varchar(300)")]
    public string Mensaje { get; set; } = string.Empty;

    public bool Leida { get; set; }

    public DateTime Fecha { get; set; }
}
