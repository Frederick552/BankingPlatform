using Banking.Domain.Enums;
using Banking.Domain.Exceptions;

namespace Banking.Domain.Entities;

public sealed class Customer
{
    public const int FullNameMaxLength = 200;

    private Customer()
    {
        // Requerido por EF Core.
        FullName = string.Empty;
    }

    public Guid Id { get; private set; }
    public string FullName { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public decimal MonthlyIncome { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static Customer Create(
        string fullName,
        DateOnly dateOfBirth,
        Gender gender,
        decimal monthlyIncome,
        DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainValidationException("El nombre completo es obligatorio.");

        fullName = fullName.Trim();
        if (fullName.Length > FullNameMaxLength)
            throw new DomainValidationException($"El nombre completo no puede exceder {FullNameMaxLength} caracteres.");

        if (dateOfBirth >= DateOnly.FromDateTime(nowUtc))
            throw new DomainValidationException("La fecha de nacimiento debe ser una fecha pasada.");

        if (!Enum.IsDefined(gender))
            throw new DomainValidationException("El sexo/género indicado no es válido.");

        Money.EnsureValidNonNegative(monthlyIncome, "Los ingresos mensuales");

        return new Customer
        {
            Id = Guid.CreateVersion7(),
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            MonthlyIncome = monthlyIncome,
            CreatedAtUtc = nowUtc
        };
    }
}
