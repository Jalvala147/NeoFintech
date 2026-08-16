namespace ProyectoIndursa.Models;

public class PrestamoListaItem
{
    public int Folio { get; set; }
    public decimal Cantidad { get; set; }
    public long PagoRealizados { get; set; }
    public long PagoPedientes { get; set; }
    public int Estado { get; set; }
    public DateTime FechaExpedicion { get; set; }
    public int SolicitadoPor { get; set; }
    public string Solicitante { get; set; } = string.Empty;
    public decimal Mensualidad { get; set; }
    public DateTime FechaLimite { get; set; }
    public bool Vencido { get; set; }
    public string? MotivoRechazo { get; set; }

    public string EstadoTexto => Estado switch
    {
        0 => "Pendiente",
        1 => "Activo",
        2 => "Rechazado",
        3 => "Liquidado",
        _ => "Desconocido"
    };
}
