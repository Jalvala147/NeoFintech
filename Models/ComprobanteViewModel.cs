namespace ProyectoIndursa.Models;

public class ComprobanteViewModel
{
    public long Folio { get; set; }
    public int NoCuenta { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal SaldoResultante { get; set; }
    public string? Referencia { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}
