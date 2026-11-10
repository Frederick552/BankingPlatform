using Banking.Domain.Enums;

namespace Banking.Application.Dtos;

// ---- Clientes ----
public sealed record CreateCustomerRequest(
    string FullName,
    DateOnly DateOfBirth,
    Gender Gender,
    decimal MonthlyIncome);

public sealed record CustomerDto(
    Guid Id,
    string FullName,
    DateOnly DateOfBirth,
    Gender Gender,
    decimal MonthlyIncome,
    DateTime CreatedAtUtc);

// ---- Cuentas ----
public sealed record CreateAccountRequest(Guid CustomerId, decimal InitialBalance);

public sealed record AccountDto(
    string AccountNumber,
    Guid CustomerId,
    decimal Balance,
    DateTime CreatedAtUtc);

public sealed record BalanceDto(string AccountNumber, decimal Balance, DateTime AsOfUtc);

// ---- Transacciones ----
public sealed record TransactionRequest(decimal Amount);

public sealed record TransactionDto(
    Guid TransactionId,
    TransactionType Type,
    decimal Amount,
    DateTime TimestampUtc,
    decimal BalanceAfter);

public sealed record TransactionHistoryDto(
    string AccountNumber,
    IReadOnlyList<TransactionDto> Transactions);
