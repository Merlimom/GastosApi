using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations;

public partial class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> entity)
    {
        entity
             .HasKey(x => x.Id);

        entity
            .Property(x => x.Amount)
            .IsRequired();

        entity
            .Property(x => x.Date)
            .IsRequired();

        entity
            .Property(x => x.Description)
            .IsRequired();

        entity
            .HasOne(x => x.User)
            .WithMany(x => x.Expenses)
            .HasForeignKey(x => x.UserId);

        entity
            .HasOne(x => x.Category)
            .WithMany(x => x.Expenses)
            .HasForeignKey(x => x.CategoryId);
    }
}
