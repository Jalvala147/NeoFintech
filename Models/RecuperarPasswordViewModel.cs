using System.ComponentModel.DataAnnotations;

namespace ProyectoIndursa.Models;

public class RecuperarPasswordViewModel
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Ingresa tu número de cuenta")]
    [Display(Name = "Número de cuenta")]
    public int NoCuenta { get; set; }

    [Required]
    [Display(Name = "CURP")]
    public string Curp { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateTime? FechaDeNacimiento { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string Nueva { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Nueva), ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar contraseña")]
    public string Confirmacion { get; set; } = string.Empty;
}
