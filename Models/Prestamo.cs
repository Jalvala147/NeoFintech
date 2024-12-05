using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoIndursa.Models
{

[Table("prestamo")]
public partial class Prestamo
{
    [Key]
    [Column(TypeName = "INT")]
    public int Folio { get; set; }

    [Column(TypeName = "pesos")]
    public decimal Cantidad { get; set; }

    [InverseProperty("FolioNavigation")]
    public virtual ICollection<DatosPrestamo> DatosPrestamos { get; } = new List<DatosPrestamo>();

    [InverseProperty("FolioNavigation")]
    public virtual ICollection<EstadoPrestamo> EstadoPrestamos { get; } = new List<EstadoPrestamo>();
}
}