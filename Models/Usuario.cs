using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoIndursa.Models
{

[Table("usuario")]
public partial class Usuario
{
    [Key]
    [Required(ErrorMessage ="Se requiere Curp")]
    [Column("CURP", TypeName = "varchar(19)")]
    public string Curp { get; set; } = null!;

    [Required(ErrorMessage ="Se requiere Nombre Válido")]
    [Column("Nombre(s)", TypeName = "varchar(30)")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage ="Se requiere Apellido Válido")]
    [Column("Apellido_Paterno", TypeName = "varchar(30)")]
    public string ApellidoPaterno { get; set; } = null!;

    [Required(ErrorMessage ="Se requiere Apellido Válido")]
    [Column("Apellido_Materno", TypeName = "varchar(30)")]
    public string ApellidoMaterno { get; set; } = null!;

    [Required(ErrorMessage ="Se requiere Fecha")]
    [Column("Fecha de Nacimiento", TypeName = "datetime")]
    public DateTime FechaDeNacimiento { get; set; }

    [Column("No_Cuenta", TypeName = "INT")]
    public int NoCuenta { get; set; }

    [ForeignKey("NoCuenta")]
    [InverseProperty("Usuarios")]
    public virtual Cuentum? NoCuentaNavigation { get; set; }
}
}
