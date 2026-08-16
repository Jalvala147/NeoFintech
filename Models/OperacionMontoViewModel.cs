using System.ComponentModel.DataAnnotations;

namespace ProyectoIndursa.Models;

public class OperacionMontoViewModel
{
    [Required(ErrorMessage = "Ingresa el monto")]
    [Range(1, 100000, ErrorMessage = "El monto debe estar entre $1 y $100,000")]
    [Display(Name = "Monto")]
    public decimal Monto { get; set; }
}
