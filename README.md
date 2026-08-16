# NeoFintech

Sistema bancario web en ASP.NET Core (Razor + SQLite) para cuentas, movimientos, préstamos, rifas y personal.

## Requisitos

- .NET SDK 9
- Al clonar: `dotnet restore` y `dotnet run`

La app queda en `http://localhost:5088` o en el puerto que indique `Properties/launchSettings.json`.

## Cuentas de demostración

Al arrancar se restablecen estas claves:

| Rol | Cuenta | Contraseña |
|---|---|---|
| Empleado y gerente | `10000` | `Empleado123` |
| Cliente | `72235` | `waeee` |
| Cliente | `93386` | `eeeaaa` |

Las cuentas `21440` y `69691` están rechazadas a propósito.

## Qué se puede hacer

- Solicitar cuenta (CURP de 18 caracteres) y esperar aprobación
- Iniciar sesión, cambiar o recuperar contraseña
- Ver saldo, depositar, retirar, transferir y guardar comprobante
- Pedir un préstamo, pagarlo y ver la amortización
- Participar en la rifa con depósitos de $500 o más
- Como empleado: aprobar o rechazar cuentas y préstamos, sortear la rifa
- Como gerente: dar de alta empleados, vacaciones y desactivar cuentas

## Recuperar contraseña

Necesitas número de cuenta, CURP y fecha de nacimiento. La nueva clave debe tener al menos 6 caracteres.

Para `72235`: CURP `sdfg`, fecha `2022-11-30`.
