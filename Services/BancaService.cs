using Microsoft.EntityFrameworkCore;
using ProyectoIndursa.Helpers;
using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.Models;

namespace ProyectoIndursa.Services;

public class BancaService
{
    private readonly IndursaDB _db;

    public BancaService(IndursaDB db)
    {
        _db = db;
    }

    public OperacionResultado Depositar(int noCuenta, decimal monto, string concepto)
    {
        if (monto <= 0 || monto > Banca.DepositoMaximo)
        {
            return Fallo("El monto del depósito no es válido.");
        }

        using var tx = _db.Database.BeginTransaction();
        var info = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta);
        if (info == null)
        {
            return Fallo("No se encontró la cuenta.");
        }

        info.Saldo += monto;
        var movimiento = Registrar(noCuenta, "Deposito", monto, info.Saldo, null, concepto);
        if (monto >= Banca.DepositoRifaMinimo)
        {
            _db.Rifas.Add(new Rifa
            {
                Cuenta = noCuenta,
                FechaBoleto = DateTime.Now,
                Ganador = 0
            });
        }

        _db.SaveChanges();
        tx.Commit();
        return Ok("Depósito realizado.", movimiento, info.Saldo);
    }

    public OperacionResultado Retirar(int noCuenta, decimal monto, string concepto)
    {
        if (monto <= 0)
        {
            return Fallo("El monto del retiro no es válido.");
        }

        using var tx = _db.Database.BeginTransaction();
        var info = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta);
        if (info == null)
        {
            return Fallo("No se encontró la cuenta.");
        }

        if (info.Saldo < monto)
        {
            return Fallo("Saldo insuficiente.");
        }

        info.Saldo -= monto;
        var movimiento = Registrar(noCuenta, "Retiro", monto, info.Saldo, null, concepto);
        _db.SaveChanges();
        tx.Commit();
        return Ok("Retiro realizado.", movimiento, info.Saldo);
    }

    public OperacionResultado Transferir(int origen, int destino, decimal monto, string? concepto)
    {
        if (origen == destino)
        {
            return Fallo("No puedes transferir a tu misma cuenta.");
        }

        if (monto <= 0)
        {
            return Fallo("El monto de la transferencia no es válido.");
        }

        using var tx = _db.Database.BeginTransaction();
        var cuentaDestino = _db.Cuenta.FirstOrDefault(c => c.NoCuenta == destino);
        if (cuentaDestino == null || cuentaDestino.TipoCuenta != 2)
        {
            return Fallo("La cuenta destino no existe o no está activa.");
        }

        var infoOrigen = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == origen);
        var infoDestino = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == destino);
        if (infoOrigen == null || infoDestino == null)
        {
            return Fallo("No se encontró una de las cuentas.");
        }

        if (infoOrigen.Saldo < monto)
        {
            return Fallo("Saldo insuficiente.");
        }

        var detalle = string.IsNullOrWhiteSpace(concepto) ? "Transferencia" : concepto.Trim();
        infoOrigen.Saldo -= monto;
        infoDestino.Saldo += monto;
        var movimiento = Registrar(origen, "TransferenciaEnvio", monto, infoOrigen.Saldo, destino.ToString(), $"{detalle} a {destino}");
        Registrar(destino, "TransferenciaRecibo", monto, infoDestino.Saldo, origen.ToString(), $"{detalle} de {origen}");
        Notificar(destino, $"Recibiste ${monto:N2} de la cuenta {origen}.");
        _db.SaveChanges();
        tx.Commit();
        return Ok("Transferencia realizada.", movimiento, infoOrigen.Saldo);
    }

    public OperacionResultado AcreditarPrestamo(int noCuenta, decimal monto, int folio)
    {
        var info = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta);
        if (info == null)
        {
            return Fallo("No se encontró la cuenta.");
        }

        info.Saldo += monto;
        var movimiento = Registrar(noCuenta, "Prestamo", monto, info.Saldo, folio.ToString(), $"Depósito de préstamo folio {folio}");
        _db.SaveChanges();
        return Ok("Préstamo acreditado.", movimiento, info.Saldo);
    }

    public OperacionResultado PagarPrestamo(int noCuenta, int folio)
    {
        using var tx = _db.Database.BeginTransaction();
        var estado = _db.EstadoPrestamos.FirstOrDefault(e => e.Folio == folio);
        var prestamo = _db.Prestamos.FirstOrDefault(p => p.Folio == folio);
        var datos = _db.DatosPrestamos.FirstOrDefault(d => d.Folio == folio);
        if (estado == null || prestamo == null || datos == null || datos.SolicitadoPor != noCuenta)
        {
            return Fallo("No se encontró el préstamo.");
        }

        if (estado.Estado != 1)
        {
            return Fallo("Este préstamo no está activo.");
        }

        var info = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta);
        if (info == null)
        {
            return Fallo("No se encontró la cuenta.");
        }

        var mensualidad = Banca.Mensualidad(prestamo.Cantidad);
        var mora = DateTime.Now.Date > datos.FechaLimite.Date ? Banca.MoraFija : 0;
        var total = mensualidad + mora;
        if (info.Saldo < total)
        {
            return Fallo($"Saldo insuficiente. Necesitas ${total:N2}.");
        }

        info.Saldo -= total;
        estado.PagoRealizados += 1;
        estado.PagoPedientes = Math.Max(0, estado.PagoPedientes - 1);
        if (estado.PagoPedientes == 0)
        {
            estado.Estado = 3;
            datos.FechaLiquidacion = DateTime.Now;
            Notificar(noCuenta, $"Liquidaste el préstamo folio {folio}.");
        }

        var concepto = mora > 0
            ? $"Pago préstamo {folio} + mora ${mora:N2}"
            : $"Pago préstamo folio {folio}";
        var movimiento = Registrar(noCuenta, "PagoPrestamo", total, info.Saldo, folio.ToString(), concepto);
        _db.SaveChanges();
        tx.Commit();
        return Ok(estado.Estado == 3 ? "Préstamo liquidado." : "Pago registrado.", movimiento, info.Saldo);
    }

    public OperacionResultado SortearRifa()
    {
        using var tx = _db.Database.BeginTransaction();
        var boletos = _db.Rifas.Where(r => r.Ganador == 0).ToList();
        if (boletos.Count == 0)
        {
            return Fallo("No hay boletos participantes.");
        }

        var ganador = boletos[Random.Shared.Next(boletos.Count)];
        ganador.Ganador = 1;
        var info = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == ganador.Cuenta);
        if (info == null)
        {
            return Fallo("La cuenta ganadora ya no existe.");
        }

        info.Saldo += Banca.PremioRifa;
        var movimiento = Registrar(ganador.Cuenta, "PremioRifa", Banca.PremioRifa, info.Saldo, ganador.NoBoleto.ToString(), "Premio de rifa");
        Notificar(ganador.Cuenta, $"Ganaste la rifa con el boleto {ganador.NoBoleto}. Se acreditaron ${Banca.PremioRifa:N2}.");
        _db.SaveChanges();
        tx.Commit();
        return Ok($"Ganó la cuenta {ganador.Cuenta} con el boleto {ganador.NoBoleto}.", movimiento, info.Saldo);
    }

    public void Notificar(int noCuenta, string mensaje)
    {
        _db.Notificaciones.Add(new Notificacion
        {
            NoCuenta = noCuenta,
            Mensaje = mensaje,
            Leida = false,
            Fecha = DateTime.Now
        });
    }

    public List<AmortizacionItem> Amortizacion(decimal cantidad, long pagosRealizados)
    {
        var mensualidad = Banca.Mensualidad(cantidad);
        var interes = Math.Round(cantidad * Banca.TasaInteres / Banca.Plazos, 2);
        var capital = Math.Round(mensualidad - interes, 2);
        return Enumerable.Range(1, Banca.Plazos).Select(n => new AmortizacionItem
        {
            Numero = n,
            Capital = capital,
            Interes = interes,
            Total = mensualidad,
            Pagado = n <= pagosRealizados
        }).ToList();
    }

    private Movimiento Registrar(int noCuenta, string tipo, decimal monto, decimal saldo, string? referencia, string concepto)
    {
        var movimiento = new Movimiento
        {
            NoCuenta = noCuenta,
            Tipo = tipo,
            Monto = monto,
            SaldoResultante = saldo,
            Referencia = referencia,
            Concepto = concepto,
            Fecha = DateTime.Now
        };
        _db.Movimientos.Add(movimiento);
        return movimiento;
    }

    private static OperacionResultado Fallo(string mensaje) => new(false, mensaje, null);

    private static OperacionResultado Ok(string mensaje, Movimiento movimiento, decimal saldo) =>
        new(true, mensaje, new ComprobanteViewModel
        {
            Folio = movimiento.Id,
            NoCuenta = movimiento.NoCuenta,
            Tipo = movimiento.Tipo,
            Monto = movimiento.Monto,
            SaldoResultante = saldo,
            Referencia = movimiento.Referencia,
            Concepto = movimiento.Concepto,
            Fecha = movimiento.Fecha
        });
}

public record OperacionResultado(bool Ok, string Mensaje, ComprobanteViewModel? Comprobante);
