using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoIndursa.Models
{

[Table("gerente")]
public partial class Gerente
{
    [Key]
    [Column("No_Gerente")]
    public long NoGerente { get; set; }

    [Column(TypeName = "INT")]
    public long Nomina { get; set; }

    [Column("Dias_Vacaciones", TypeName = "INT")]
    public long DiasVacaciones { get; set; }

    [ForeignKey("Nomina")]
    [InverseProperty("Gerentes")]
    public virtual Empleado NominaNavigation { get; set; } = null!;
}
}