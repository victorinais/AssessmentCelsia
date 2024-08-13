using app.Models;
using Microsoft.EntityFrameworkCore;

namespace app.Data;

public class BaseDbContext : DbContext
{
    public BaseDbContext(DbContextOptions<BaseDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Transaccion> Transacciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de la entidad Usuarios
            var salt = PasswordHelper.GenerateSalt();
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Email = "robinson.cortes@riwi.io",
                    Salt = salt,
                    PasswordHash = PasswordHelper.HashPassword("password", salt),
                    IsAdmin = true
                },
                new Usuario
                {
                    Id = 2,
                    Email = "vicmarin50@gmail.com",
                    Salt = salt,
                    PasswordHash = PasswordHelper.HashPassword("password", salt),
                    IsAdmin = true
                }
            );

        // Configuracion de la entidad Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.ClienteId);
            entity.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.NumeroIdentificacion)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(c => c.Direccion)
                .HasMaxLength(200);

            entity.Property(c => c.Telefono)
                .HasMaxLength(20);

            entity.Property(c => c.CorreoElectronico)
                .HasMaxLength(100);
        });

        // Configuración de la entidad Factura
        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(f => f.FacturaId);

            entity.Property(f => f.NumeroFactura)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(f => f.PeriodoFacturacion)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(f => f.MontoFacturado)
                .HasColumnType("decimal(18,2)");

            entity.Property(f => f.MontoPagado)
                .HasColumnType("decimal(18,2)");

            entity.HasOne(f => f.Cliente)
                .WithMany(c => c.Facturas)
                .HasForeignKey(f => f.ClienteId);
        });

        // Configuración de la entidad Transaccion
        modelBuilder.Entity<Transaccion>(entity =>
        {
            entity.HasKey(t => t.TransaccionId);

            entity.Property(t => t.FechaHora)
                .IsRequired()
                .HasColumnType("datetime"); // Cambiado a 'datetime' en lugar de 'datetime2'

            entity.Property(t => t.Monto)
                .HasColumnType("decimal(18,2)");

            entity.Property(t => t.Estado)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(t => t.Tipo)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(t => t.PlataformaUtilizada)
                .HasMaxLength(50);

            entity.HasOne(t => t.Cliente)
                .WithMany(c => c.Transacciones)
                .HasForeignKey(t => t.ClienteId);

            entity.HasOne(t => t.Factura)
                .WithMany(f => f.Transacciones)
                .HasForeignKey(t => t.FacturaId);
        });
    }
}