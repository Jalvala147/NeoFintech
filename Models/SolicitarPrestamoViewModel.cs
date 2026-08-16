using System.ComponentModel.DataAnnotations;

namespace ProyectoIndursa.Models;

public class SolicitarPrestamoViewModel
{
    [Required(ErrorMessage = "Ingresa la cantidad")]
    [Range(1, 1_000_000, ErrorMessage = "La cantidad debe ser mayor a 0")]
    [Display(Name = "Cantidad solicitada")]
    public decimal Cantidad { get; set; }
}
