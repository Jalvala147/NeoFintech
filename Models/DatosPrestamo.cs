using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoIndursa.Models
{

[Table("datos_prestamo")]
public partial class DatosPrestamo
{
    [Key]
    [Column("NFolio")]
    public long Nfolio { get; set; }

    [Column("Fecha_Expedicion", TypeName = "datetime")]
    public DateTime FechaExpedicion { get; set; }

    [Column("Fecha_Aprobacion", TypeName = "datetime")]
    public DateTime FechaAprobacion { get; set; }

    [Column("Fecha_Liquidacion", TypeName = "datetime")]
    public DateTime FechaLiquidacion { get; set; }

    [Column("Fecha_Limite", TypeName = "datetime")]
    public DateTime FechaLimite { get; set; }

    [Column(TypeName = "INT")]
    public int Folio { get; set; }

    [Column("Solicitado_Por", TypeName = "INT")]
    public int SolicitadoPor { get; set; }

    [ForeignKey("Folio")]
    [InverseProperty("DatosPrestamos")]
    public virtual Prestamo FolioNavigation { get; set; } = null!;

    [ForeignKey("SolicitadoPor")]
    [InverseProperty("DatosPrestamos")]
    public virtual Cuentum SolicitadoPorNavigation { get; set; } = null!;
}
}