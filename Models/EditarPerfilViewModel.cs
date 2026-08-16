using System.ComponentModel.DataAnnotations;

namespace ProyectoIndursa.Models;

public class EditarPerfilViewModel
{
    [Required]
    [StringLength(30)]
    [Display(Name = "Nombre(s)")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Display(Name = "Apellido paterno")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Display(Name = "Apellido materno")]
    public string ApellidoMaterno { get; set; } = string.Empty;
}
