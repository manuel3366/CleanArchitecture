using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.Property(client => client.Nombres)
            .IsRequired()
            .HasMaxLength(MaxLengths.Client.Nombres);

        builder.Property(client => client.Apellidos)
            .IsRequired()
            .HasMaxLength(MaxLengths.Client.Apellidos);

        builder.Property(client => client.DocumentoIdentidad)
            .IsRequired()
            .HasMaxLength(MaxLengths.Client.DocumentoIdentidad);

        builder.HasIndex(client => client.DocumentoIdentidad)
            .IsUnique();

        builder.Property(client => client.Email)
            .IsRequired()
            .HasMaxLength(MaxLengths.Client.Email);

        builder.HasIndex(client => client.Email)
            .IsUnique();

        builder.Property(client => client.Telefono)
            .HasMaxLength(MaxLengths.Client.Telefono);

        builder.Property(client => client.Estado)
            .IsRequired()
            .HasMaxLength(MaxLengths.Client.Estado);

        builder.Property(client => client.FechaRegistro)
            .IsRequired();

        builder.Property(client => client.FechaActualizacion)
            .IsRequired();
    }
}
