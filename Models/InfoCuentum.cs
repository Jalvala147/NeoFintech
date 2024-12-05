using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoIndursa.Models
{

[Table("info_Cuenta")]
public partial class InfoCuentum
{
    [Key]
    public long NoUsuario { get; set; }

    [Column("No_Cuenta", TypeName = "INT")]
    public int NoCuenta { get; set; }

    [Column(TypeName = "pesos")]
    public decimal Saldo { get; set; }

    [ForeignKey("NoCuenta")]
    [InverseProperty("InfoCuenta")]
    public virtual Cuentum NoCuentaNavigation { get; set; } = null!;
}
}
