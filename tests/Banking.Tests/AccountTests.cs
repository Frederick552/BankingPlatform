using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;

namespace Banking.Tests;

public class AccountTests
{
    private static readonly DateTime Now = new(2026, 6, 18, 10, 0, 0, DateTimeKind.Utc);

    private static Account OpenAccount(decimal initialBalance = 100m) =>
        Account.Open(Guid.NewGuid(), "ACC-20260618-0001", initialBalance, Now);

    [Fact]
    public void Deposit_IncreasesBalance_AndReturnsTransactionWithResultingBalance()
    {
        var account = OpenAccount(100m);

        var transaction = account.Deposit(50m, Now);

        Assert.Equal(150m, account.Balance);
        Assert.Equal(TransactionType.Deposit, transaction.Type);
        Assert.Equal(50m, transaction.Amount);
        Assert.Equal(150m, transaction.BalanceAfter);
        Assert.Equal(account.Id, transaction.AccountId);
    }

    [Fact]
    public void Withdraw_WithSufficientFunds_DecreasesBalance()
    {
        var account = OpenAccount(100m);

        var transaction = account.Withdraw(30m, Now);

        Assert.Equal(70m, account.Balance);
        Assert.Equal(TransactionType.Withdrawal, transaction.Type);
        Assert.Equal(70m, transaction.BalanceAfter);
    }

    [Fact]
    public void Withdraw_OfExactBalance_LeavesAccountAtZero()
    {
        var account = OpenAccount(100m);

        account.Withdraw(100m, Now);

        Assert.Equal(0m, account.Balance);
    }

    [Fact]
    public void Withdraw_WithInsufficientFunds_ThrowsAndLeavesAccountUntouched()
    {
        var account = OpenAccount(100m);
        var versionBefore = account.Version;

        Assert.Throws<InsufficientFundsException>(() => account.Withdraw(100.01m, Now));

        Assert.Equal(100m, account.Balance);
        Assert.Equal(versionBefore, account.Version);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(10.123)]
    public void Deposit_WithInvalidAmount_ThrowsValidationException(double amount)
    {
        var account = OpenAccount();

        Assert.Throws<DomainValidationException>(() => account.Deposit((decimal)amount, Now));
        Assert.Equal(100m, account.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Withdraw_WithNonPositiveAmount_ThrowsValidationException(double amount)
    {
        var account = OpenAccount();

        Assert.Throws<DomainValidationException>(() => account.Withdraw((decimal)amount, Now));
    }

    [Fact]
    public void Open_WithNegativeInitialBalance_ThrowsValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            Account.Open(Guid.NewGuid(), "ACC-20260618-0001", -1m, Now));
    }

    [Fact]
    public void Movements_GetIncreasingSequenceNumbers()
    {
        var account = OpenAccount();

        var first = account.Deposit(10m, Now);
        var second = account.Withdraw(5m, Now);

        Assert.Equal(1, first.Sequence);
        Assert.Equal(2, second.Sequence);
    }
}
