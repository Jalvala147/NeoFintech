using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoIndursa.Models
{

[Table("empleado")]
public partial class Empleado
{
    [Key]
    [Column(TypeName = "INT")]
    public long Nomina { get; set; }

    [Column("No_Cuenta", TypeName = "INT")]
    public int NoCuenta { get; set; }

    [InverseProperty("NominaNavigation")]
    public virtual ICollection<Gerente> Gerentes { get; } = new List<Gerente>();

    [ForeignKey("NoCuenta")]
    [InverseProperty("Empleados")]
    public virtual Cuentum? NoCuentaNavigation { get; set; }
}
}
