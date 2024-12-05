using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoIndursa.Models
{

[Table("cuenta")]
public partial class Cuentum
{
    [Key]
    [Column("No_Cuenta", TypeName = "INT")]
    public int NoCuenta { get; set; }

    [Column(TypeName = "varchar(16)")]
    public string Password { get; set; } = null!;

    [Column("Tipo_Cuenta", TypeName = "tinyint")]
    public int TipoCuenta { get; set; }

    [InverseProperty("SolicitadoPorNavigation")]
    public virtual ICollection<DatosPrestamo> DatosPrestamos { get; } = new List<DatosPrestamo>();

    [InverseProperty("NoCuentaNavigation")]
    public virtual ICollection<Empleado> Empleados { get; } = new List<Empleado>();

    [InverseProperty("NoCuentaNavigation")]
    public virtual ICollection<InfoCuentum> InfoCuenta { get; } = new List<InfoCuentum>();

    [InverseProperty("CuentaNavigation")]
    public virtual ICollection<Rifa> Rifas { get; } = new List<Rifa>();

    [InverseProperty("NoCuentaNavigation")]
    public virtual ICollection<Usuario> Usuarios { get; } = new List<Usuario>();
}
}