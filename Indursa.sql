CREATE TABLE "cuenta" (
  "No_Cuenta" int NOT NULL PRIMARY KEY,
  "Password" varchar(16),
  "Tipo_Cuenta" tinyint NOT NULL
);

CREATE TABLE "info_Cuenta" (
  "NoUsuario" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "No_Cuenta" int NOT NULL,
  "Saldo" "pesos" NOT NULL DEFAULT 10000,
  CONSTRAINT "infoNoCuenta_cuentaNoCuenta" FOREIGN KEY ("No_Cuenta") REFERENCES "cuenta" ("No_Cuenta") ON DELETE NO ACTION ON UPDATE NO ACTION
);
 
CREATE TABLE "empleado" (
  "Nomina" int NOT NULL PRIMARY KEY,
  "No_Cuenta" int,
  CONSTRAINT "empleadoNoCuenta_cuentaNoCuenta" FOREIGN KEY ("No_Cuenta") REFERENCES "cuenta" ("No_Cuenta") ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE "gerente" (
  "No_Gerente" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Nomina" int NOT NULL,
  "Dias_Vacaciones" int NOT NULL,
  CONSTRAINT "gerentesNomina_empleadoNomina" FOREIGN KEY ("Nomina") REFERENCES "empleado" ("Nomina") ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE "prestamo" (
  "Folio" int NOT NULL PRIMARY KEY,
  "Cantidad" "pesos" NOT NULL
);

CREATE TABLE "estado_prestamo" (
  "NPrestamo" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Folio" int NOT NULL,
  "Pago_Realizados" tinyint NOT NULL,
  "Pago_Pedientes" tinyint NOT NULL,
  "Estado" tinyint DEFAULT NULL,
  CONSTRAINT "estadoFolio_prestamoFolio" FOREIGN KEY ("Folio") REFERENCES "prestamo" ("Folio") ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE "datos_prestamo" (
  "NFolio" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Fecha_Expedicion" "datetime" NOT NULL,
  "Fecha_Aprobacion" "datetime" NOT NULL,
  "Fecha_Liquidacion" "datetime" NOT NULL,
  "Fecha_Limite" "datetime" NOT NULL,
  "Folio" int NOT NULL,
  "Solicitado_Por" int NOT NULL,
  CONSTRAINT "datosFolio_prestamoFolio" FOREIGN KEY ("Folio") REFERENCES "prestamo" ("Folio") ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT "datosSolicitado_cuentaNoCuenta" FOREIGN KEY ("Solicitado_Por") REFERENCES "cuenta" ("No_Cuenta") ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE "rifa" (
  "No_Boleto" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Cuenta" int NOT NULL,
  "Fecha_Boleto" "datetime" NOT NULL,
  CONSTRAINT "rifaCuenta_cuentaNoCuenta" FOREIGN KEY ("Cuenta") REFERENCES "cuenta" ("No_Cuenta") ON DELETE NO ACTION ON UPDATE NO ACTION
);


CREATE TABLE "usuario" (
  "CURP" varchar(19) NOT NULL PRIMARY KEY,
  "Nombre(s)" varchar(30) NOT NULL,
  "Apellido_Paterno" varchar(30) NOT NULL,
  "Apellido_Materno" varchar(30) NOT NULL,
  "Fecha de Nacimiento" "datetime" NOT NULL,
  "No_Cuenta" int,
  CONSTRAINT "usuarioNoCuenta_cuentaNoCUenta" FOREIGN KEY ("No_Cuenta") REFERENCES "cuenta" ("No_Cuenta") ON DELETE NO ACTION
);


