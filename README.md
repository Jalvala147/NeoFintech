# NeoFintech

Sistema bancario web en ASP.NET Core (Razor + SQLite) para cuentas, movimientos, préstamos, rifas y personal.

## Requisitos

- .NET SDK 9

```bash
dotnet restore
dotnet run
```

La app queda en `http://localhost:5088` (o el puerto de `launchSettings.json`).

## Cuentas de demostración

Al arrancar se restablecen estas claves:

| Rol | Cuenta | Contraseña |
|---|---|---|
| Empleado y gerente | `10000` | `Empleado123` |
| Cliente | `72235` | `waeee` |
| Cliente | `93386` | `eeeaaa` |

## Qué incluye

- UI moderna y responsive (móvil y escritorio)
- Solicitar cuenta, login, cambio y recuperación de contraseña
- Saldo, depósito, retiro, transferencia y comprobantes
- Préstamos con aprobación, pagos y amortización
- Rifa por depósitos de $500+
- Panel de empleado y gerencia

## Recuperar contraseña

Necesitas número de cuenta, CURP y fecha de nacimiento. La nueva clave debe tener al menos 6 caracteres.

Para `72235`: CURP `sdfg`, fecha `2022-11-30`.
