using Banking.Application.Abstractions;
using Banking.Application.Dtos;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;

namespace Banking.Application.Services;

public sealed class TransactionService(
    IAccountRepository accounts,
    IAccountTransactionRepository transactions,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ITransactionService
{
    public Task<TransactionDto> DepositAsync(string accountNumber, decimal amount, CancellationToken cancellationToken) =>
        ApplyAsync(accountNumber, (account, now) => account.Deposit(amount, now), cancellationToken);

    public Task<TransactionDto> WithdrawAsync(string accountNumber, decimal amount, CancellationToken cancellationToken) =>
        ApplyAsync(accountNumber, (account, now) => account.Withdraw(amount, now), cancellationToken);

    public async Task<TransactionHistoryDto> GetHistoryAsync(string accountNumber, CancellationToken cancellationToken)
    {
        var account = await GetAccountAsync(accountNumber, cancellationToken);
        var history = await transactions.GetByAccountIdAsync(account.Id, cancellationToken);

        return new TransactionHistoryDto(
            account.AccountNumber,
            history.Select(t => t.ToDto()).ToList());
    }

    private async Task<TransactionDto> ApplyAsync(
        string accountNumber,
        Func<Account, DateTime, AccountTransaction> operation,
        CancellationToken cancellationToken)
    {
        var account = await GetAccountAsync(accountNumber, cancellationToken);

        // Las reglas (monto válido, fondos suficientes) viven en la entidad; si fallan lanza
        // una excepción de negocio antes de que se registre nada.
        var transaction = operation(account, timeProvider.GetUtcNow().UtcDateTime);

        await transactions.AddAsync(transaction, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken); // saldo + movimiento en una sola transacción

        return transaction.ToDto();
    }

    private async Task<Account> GetAccountAsync(string accountNumber, CancellationToken cancellationToken) =>
        await accounts.GetByAccountNumberAsync(accountNumber, cancellationToken)
            ?? throw new NotFoundException("La cuenta indicada no existe.");
}
