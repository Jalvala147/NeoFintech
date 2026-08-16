namespace ProyectoIndursa.Models;

public class CuentaListaItem
{
    public int NoCuenta { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string Curp { get; set; } = string.Empty;
    public int TipoCuenta { get; set; }

    public string EstadoTexto => TipoCuenta switch
    {
        1 => "Pendiente",
        2 => "Aceptada",
        3 => "Rechazada",
        _ => "Desconocido"
    };
}
