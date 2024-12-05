using System;
using System.Collections.Generic;
using ProyectoIndursa.Models;
using Microsoft.EntityFrameworkCore;

namespace ProyectoIndursa.IndursaContext
{
public partial class IndursaDB : DbContext
{
    public IndursaDB()
    {
    }

    public IndursaDB(DbContextOptions<IndursaDB> options)
        : base(options)
    {
    }

    public virtual DbSet<Cuentum> Cuenta { get; set; }

    public virtual DbSet<DatosPrestamo> DatosPrestamos { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<EstadoPrestamo> EstadoPrestamos { get; set; }

    public virtual DbSet<Gerente> Gerentes { get; set; }

    public virtual DbSet<InfoCuentum> InfoCuenta { get; set; }

    public virtual DbSet<Prestamo> Prestamos { get; set; }

    public virtual DbSet<Rifa> Rifas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Filename=Indursa.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cuentum>(entity =>
        {
            entity.Property(e => e.NoCuenta).ValueGeneratedNever();
        });

        modelBuilder.Entity<DatosPrestamo>(entity =>
        {
            entity.HasOne(d => d.FolioNavigation).WithMany(p => p.DatosPrestamos).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SolicitadoPorNavigation).WithMany(p => p.DatosPrestamos).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.Property(e => e.Nomina).ValueGeneratedNever();
        });

        modelBuilder.Entity<EstadoPrestamo>(entity =>
        {
            entity.HasOne(d => d.FolioNavigation).WithMany(p => p.EstadoPrestamos).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Gerente>(entity =>
        {
            entity.HasOne(d => d.NominaNavigation).WithMany(p => p.Gerentes).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<InfoCuentum>(entity =>
        {
            entity.Property(e => e.Saldo).HasDefaultValueSql("10000");

            entity.HasOne(d => d.NoCuentaNavigation).WithMany(p => p.InfoCuenta).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.Property(e => e.Folio).ValueGeneratedNever();
        });

        modelBuilder.Entity<Rifa>(entity =>
        {
            entity.HasOne(d => d.CuentaNavigation).WithMany(p => p.Rifas).OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
}