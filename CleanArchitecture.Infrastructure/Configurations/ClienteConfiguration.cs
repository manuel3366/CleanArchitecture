using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.Property(cliente => cliente.Nombres)
            .IsRequired()
            .HasMaxLength(MaxLengths.Cliente.Nombres);

        builder.Property(cliente => cliente.Apellidos)
            .IsRequired()
            .HasMaxLength(MaxLengths.Cliente.Apellidos);

        builder.Property(cliente => cliente.DocumentoIdentidad)
            .IsRequired()
            .HasMaxLength(MaxLengths.Cliente.DocumentoIdentidad);

        builder.Property(cliente => cliente.Email)
            .IsRequired()
            .HasMaxLength(MaxLengths.Cliente.Email);

        builder.Property(cliente => cliente.Telefono)
            .HasMaxLength(MaxLengths.Cliente.Telefono);

        builder.Property(cliente => cliente.Estado)
            .IsRequired()
            .HasMaxLength(MaxLengths.Cliente.Estado);

        builder.Property(cliente => cliente.FechaRegistro)
            .IsRequired();

        builder.Property(cliente => cliente.FechaActualizacion)
            .IsRequired();

        builder.HasIndex(cliente => cliente.DocumentoIdentidad)
            .IsUnique();

        builder.HasIndex(cliente => cliente.Email)
            .IsUnique();

        builder.HasIndex(cliente => cliente.Estado);
        builder.HasIndex(cliente => cliente.FechaRegistro);
    }
}
