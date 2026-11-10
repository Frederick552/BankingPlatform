using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.FullName).IsRequired().HasMaxLength(Customer.FullNameMaxLength);
        builder.Property(c => c.Gender).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.MonthlyIncome).HasPrecision(18, 2);
    }
}

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.AccountNumber).IsRequired().HasMaxLength(17);

        // Garantía física contra números de cuenta duplicados.
        builder.HasIndex(a => a.AccountNumber).IsUnique();

        builder.Property(a => a.Balance).HasPrecision(18, 2);

        // Concurrencia optimista: dos retiros simultáneos no pueden sobregirar la cuenta.
        builder.Property(a => a.Version).IsConcurrencyToken();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AccountTransactionConfiguration : IEntityTypeConfiguration<AccountTransaction>
{
    public void Configure(EntityTypeBuilder<AccountTransaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.BalanceAfter).HasPrecision(18, 2);

        builder.HasIndex(t => new { t.AccountId, t.Sequence }).IsUnique();

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
