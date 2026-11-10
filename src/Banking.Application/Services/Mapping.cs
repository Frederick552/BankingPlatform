using Banking.Application.Dtos;
using Banking.Domain.Entities;

namespace Banking.Application.Services;

internal static class Mapping
{
    public static CustomerDto ToDto(this Customer c) =>
        new(c.Id, c.FullName, c.DateOfBirth, c.Gender, c.MonthlyIncome, c.CreatedAtUtc);

    public static AccountDto ToDto(this Account a) =>
        new(a.AccountNumber, a.CustomerId, a.Balance, a.CreatedAtUtc);

    public static TransactionDto ToDto(this AccountTransaction t) =>
        new(t.Id, t.Type, t.Amount, t.TimestampUtc, t.BalanceAfter);
}
