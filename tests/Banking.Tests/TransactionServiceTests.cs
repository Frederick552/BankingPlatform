using Banking.Application.Services;
using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;
using Banking.Tests.Support;

namespace Banking.Tests;

public class TransactionServiceTests
{
    private const string AccountNumber = "ACC-20260618-0001";

    private readonly InMemoryStore _store = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 6, 18, 10, 0, 0, TimeSpan.Zero));
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _service = new TransactionService(_store, _store, _store, _clock);
        _store.Accounts.Add(Account.Open(Guid.NewGuid(), AccountNumber, 100m, _clock.GetUtcNow().UtcDateTime));
    }

    [Fact]
    public async Task Deposit_IncreasesBalance_AndPersistsTransaction()
    {
        var result = await _service.DepositAsync(AccountNumber, 50m, CancellationToken.None);

        Assert.Equal(150m, _store.Accounts[0].Balance);
        Assert.Equal(TransactionType.Deposit, result.Type);
        Assert.Equal(150m, result.BalanceAfter);
        Assert.Single(_store.Transactions);
        Assert.Equal(1, _store.SaveCount);
    }

    [Fact]
    public async Task Withdraw_WithSufficientFunds_DecreasesBalance()
    {
        var result = await _service.WithdrawAsync(AccountNumber, 40m, CancellationToken.None);

        Assert.Equal(60m, _store.Accounts[0].Balance);
        Assert.Equal(TransactionType.Withdrawal, result.Type);
        Assert.Equal(60m, result.BalanceAfter);
    }

    [Fact]
    public async Task Withdraw_WithInsufficientFunds_IsRejected_AndNothingIsPersisted()
    {
        await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            _service.WithdrawAsync(AccountNumber, 500m, CancellationToken.None));

        Assert.Equal(100m, _store.Accounts[0].Balance);
        Assert.Empty(_store.Transactions);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task Deposit_ToUnknownAccount_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DepositAsync("ACC-20260618-9999", 10m, CancellationToken.None));
    }

    [Fact]
    public async Task History_IsChronological_WithHistoricalBalancePerMovement()
    {
        await _service.DepositAsync(AccountNumber, 50m, CancellationToken.None);   // 150
        _clock.Advance(TimeSpan.FromMinutes(1));
        await _service.WithdrawAsync(AccountNumber, 30m, CancellationToken.None);  // 120
        _clock.Advance(TimeSpan.FromMinutes(1));
        await _service.DepositAsync(AccountNumber, 5m, CancellationToken.None);    // 125

        var history = await _service.GetHistoryAsync(AccountNumber, CancellationToken.None);

        Assert.Equal(AccountNumber, history.AccountNumber);
        Assert.Equal(new[] { 150m, 120m, 125m }, history.Transactions.Select(t => t.BalanceAfter).ToArray());
        Assert.Equal(
            new[] { TransactionType.Deposit, TransactionType.Withdrawal, TransactionType.Deposit },
            history.Transactions.Select(t => t.Type).ToArray());

        var timestamps = history.Transactions.Select(t => t.TimestampUtc).ToList();
        Assert.Equal(timestamps.OrderBy(t => t).ToList(), timestamps);

        Assert.Equal(3, history.Transactions.Select(t => t.TransactionId).Distinct().Count());
    }

    [Fact]
    public async Task History_OfUnknownAccount_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetHistoryAsync("ACC-20260618-9999", CancellationToken.None));
    }
}
