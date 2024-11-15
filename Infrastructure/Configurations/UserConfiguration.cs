using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
namespace Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity
            .HasKey(x => x.Id);

        entity
            .Property(x => x.Name)
            .IsRequired();

        entity
            .Property(x => x.Email)
            .IsRequired();

        entity
            .Property(x => x.Password)
            .IsRequired();

        entity
            .Property(e => e.CreationDate);
            //.HasDefaultValueSql("GETDATE()");

        entity
            .Property(e => e.UpdateDate);
            //.HasDefaultValueSql("GETDATE()");


        entity
            .HasMany(x => x.Categories)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        entity
            .HasMany(x => x.Expenses)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);
    }
}
