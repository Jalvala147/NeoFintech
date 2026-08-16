using System.ComponentModel.DataAnnotations;

namespace ProyectoIndursa.Models;

public class TransferirViewModel
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Ingresa la cuenta destino")]
    [Display(Name = "Cuenta destino")]
    public int CuentaDestino { get; set; }

    [Required]
    [Range(1, 100000, ErrorMessage = "El monto debe estar entre $1 y $100,000")]
    [Display(Name = "Monto")]
    public decimal Monto { get; set; }

    [StringLength(120)]
    [Display(Name = "Concepto")]
    public string? Concepto { get; set; }
}
