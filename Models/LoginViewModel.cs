using System.ComponentModel.DataAnnotations;

namespace ProyectoIndursa.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu número de cuenta")]
    [Range(1, int.MaxValue, ErrorMessage = "Ingresa tu número de cuenta")]
    [Display(Name = "Número de cuenta")]
    public int NoCuenta { get; set; }

    [Required(ErrorMessage = "Ingresa tu contraseña")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
}
