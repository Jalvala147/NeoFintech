namespace ProyectoIndursa.Models;

public class AmortizacionItem
{
    public int Numero { get; set; }
    public decimal Capital { get; set; }
    public decimal Interes { get; set; }
    public decimal Total { get; set; }
    public bool Pagado { get; set; }
}
