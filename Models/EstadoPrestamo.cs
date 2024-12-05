using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoIndursa.Models
{

[Table("estado_prestamo")]
public partial class EstadoPrestamo
{
    [Key]
    [Column("NPrestamo")]
    public long Nprestamo { get; set; }

    [Column(TypeName = "INT")]
    public int Folio { get; set; }

    [Column("Pago_Realizados", TypeName = "tinyint")]
    public long PagoRealizados { get; set; }

    [Column("Pago_Pedientes", TypeName = "tinyint")]
    public long PagoPedientes { get; set; }

    [Column(TypeName = "tinyint")]
    public int Estado { get; set; }

    [ForeignKey("Folio")]
    [InverseProperty("EstadoPrestamos")]
    public virtual Prestamo FolioNavigation { get; set; } = null!;
}
}