namespace ProyectoIndursa.Helpers;

public static class Banca
{
    public const decimal PrestamoSaldoMinimo = 10000;
    public const decimal PrestamoMinimo = 1000;
    public const decimal TasaInteres = 0.10m;
    public const int Plazos = 12;
    public const decimal MoraFija = 50;
    public const decimal DepositoRifaMinimo = 500;
    public const decimal PremioRifa = 1000;
    public const decimal DepositoMaximo = 100000;
    public const decimal MultiploMaximoPrestamo = 2;

    public static decimal Mensualidad(decimal cantidad) =>
        Math.Round(cantidad * (1 + TasaInteres) / Plazos, 2);

    public static decimal MaximoPrestamo(decimal saldo) =>
        Math.Round(Math.Min(saldo * MultiploMaximoPrestamo, 100000), 2);
}
