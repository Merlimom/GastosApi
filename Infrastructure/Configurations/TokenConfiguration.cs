using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> entity)
    {
        entity
            .HasKey(x => x.Id);

        entity
            .Property(x => x.Email)
            .IsRequired();

        entity
            .Property(x => x.Token)
            .IsRequired();

        entity
            .Property(x => x.ExpirationDate)
            .IsRequired();

        // Restricción de unicidad, un mismo usuario no puede tener múltiples tokens activos
        entity
            .HasIndex(x => new { x.Email, x.Token })
            .IsUnique();
    }
}
