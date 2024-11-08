using ApiSecuityServer.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiSecuityServer.Data.Mapping;

internal sealed class ClientEntityMap : IEntityTypeConfiguration<ClientEntity>
{
    public void Configure(EntityTypeBuilder<ClientEntity> builder)
    {
        builder.ToTable("ClientTable")
            .HasKey(m => m.Id);

        builder.HasIndex(o => o.Id);
    }
}