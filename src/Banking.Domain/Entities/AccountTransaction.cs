using Banking.Domain.Enums;

namespace Banking.Domain.Entities;

/// <summary>
/// Movimiento inmutable de una cuenta. Guarda el saldo resultante (saldo histórico)
/// para poder reconstruir el historial sin recalcular.
/// </summary>
public sealed class AccountTransaction
{
    private AccountTransaction()
    {
        // Requerido por EF Core.
    }

    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }

    /// <summary>Número de operación dentro de la cuenta; desempata movimientos con el mismo timestamp.</summary>
    public int Sequence { get; private set; }

    public DateTime TimestampUtc { get; private set; }

    internal static AccountTransaction Create(
        Guid accountId,
        TransactionType type,
        decimal amount,
        decimal balanceAfter,
        int sequence,
        DateTime timestampUtc) => new()
    {
        Id = Guid.CreateVersion7(),
        AccountId = accountId,
        Type = type,
        Amount = amount,
        BalanceAfter = balanceAfter,
        Sequence = sequence,
        TimestampUtc = timestampUtc
    };
}
