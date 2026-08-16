using System.ComponentModel.DataAnnotations;

namespace ProyectoIndursa.Models;

public class CambiarPasswordViewModel
{
    [Required(ErrorMessage = "Ingresa tu contraseña actual")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string Actual { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string Nueva { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Nueva), ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar nueva contraseña")]
    public string Confirmacion { get; set; } = string.Empty;
}
