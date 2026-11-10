using Banking.Domain.Enums;
using Banking.Domain.Exceptions;

namespace Banking.Domain.Entities;

public sealed class Account
{
    private Account()
    {
        // Requerido por EF Core.
        AccountNumber = string.Empty;
    }

    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Balance { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Se incrementa en cada movimiento. Funciona como token de concurrencia optimista
    /// y como número de operación del historial.
    /// </summary>
    public int Version { get; private set; }

    public static Account Open(Guid customerId, string accountNumber, decimal initialBalance, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new DomainValidationException("El número de cuenta es obligatorio.");

        Money.EnsureValidNonNegative(initialBalance, "El saldo inicial");

        return new Account
        {
            Id = Guid.CreateVersion7(),
            AccountNumber = accountNumber,
            CustomerId = customerId,
            Balance = initialBalance,
            CreatedAtUtc = nowUtc
        };
    }

    public AccountTransaction Deposit(decimal amount, DateTime nowUtc)
    {
        Money.EnsureValidTransactionAmount(amount);

        Balance += amount;
        Version++;

        return AccountTransaction.Create(Id, TransactionType.Deposit, amount, Balance, Version, nowUtc);
    }

    public AccountTransaction Withdraw(decimal amount, DateTime nowUtc)
    {
        Money.EnsureValidTransactionAmount(amount);

        if (amount > Balance)
            throw new InsufficientFundsException();

        Balance -= amount;
        Version++;

        return AccountTransaction.Create(Id, TransactionType.Withdrawal, amount, Balance, Version, nowUtc);
    }
}
