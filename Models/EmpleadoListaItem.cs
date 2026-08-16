namespace ProyectoIndursa.Models;

public class EmpleadoListaItem
{
    public long Nomina { get; set; }
    public int NoCuenta { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public long DiasVacaciones { get; set; }
    public bool EsGerente { get; set; }
    public bool Activo { get; set; }
}
