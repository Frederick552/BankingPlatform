using Banking.Domain.Exceptions;

namespace Banking.Domain.Entities;

/// <summary>Reglas compartidas para montos monetarios.</summary>
internal static class Money
{
    public static void EnsureValidTransactionAmount(decimal amount)
    {
        if (amount <= 0)
            throw new DomainValidationException("El monto debe ser mayor que cero.");

        EnsureTwoDecimals(amount);
    }

    public static void EnsureValidNonNegative(decimal amount, string fieldName)
    {
        if (amount < 0)
            throw new DomainValidationException($"{fieldName} no puede ser negativo.");

        EnsureTwoDecimals(amount);
    }

    private static void EnsureTwoDecimals(decimal amount)
    {
        if (decimal.Round(amount, 2) != amount)
            throw new DomainValidationException("El monto admite como máximo 2 decimales.");
    }
}
