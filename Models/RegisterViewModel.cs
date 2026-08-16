using System.ComponentModel.DataAnnotations;

namespace ProyectoIndursa.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Se requiere un nombre válido")]
    [StringLength(30)]
    [Display(Name = "Nombre(s)")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Se requiere apellido paterno")]
    [StringLength(30)]
    [Display(Name = "Apellido paterno")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [Required(ErrorMessage = "Se requiere apellido materno")]
    [StringLength(30)]
    [Display(Name = "Apellido materno")]
    public string ApellidoMaterno { get; set; } = string.Empty;

    [Required(ErrorMessage = "Se requiere fecha de nacimiento")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateTime? FechaDeNacimiento { get; set; }

    [Required(ErrorMessage = "Se requiere CURP")]
    [StringLength(18, MinimumLength = 4, ErrorMessage = "El CURP debe tener entre 4 y 18 caracteres")]
    [Display(Name = "CURP")]
    public string Curp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Se requiere una contraseña")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
