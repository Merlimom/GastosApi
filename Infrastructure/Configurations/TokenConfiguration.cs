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
            .Property(x => x.Token)
            .IsRequired();

        entity
            .Property(x => x.ExpirationDate)
            .IsRequired();
    }
}
