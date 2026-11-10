using Banking.Application.Abstractions;
using Banking.Application.Dtos;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;

namespace Banking.Application.Services;

public sealed class AccountService(
    ICustomerRepository customers,
    IAccountRepository accounts,
    IAccountNumberGenerator accountNumberGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IAccountService
{
    private const int MaxNumberGenerationAttempts = 10;

    public async Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException("El cliente indicado no existe.");

        var accountNumber = await GenerateUniqueAccountNumberAsync(cancellationToken);

        var account = Account.Open(
            customer.Id,
            accountNumber,
            request.InitialBalance,
            timeProvider.GetUtcNow().UtcDateTime);

        await accounts.AddAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return account.ToDto();
    }

    public async Task<BalanceDto> GetBalanceAsync(string accountNumber, CancellationToken cancellationToken)
    {
        var account = await accounts.GetByAccountNumberAsync(accountNumber, cancellationToken)
            ?? throw new NotFoundException("La cuenta indicada no existe.");

        return new BalanceDto(account.AccountNumber, account.Balance, timeProvider.GetUtcNow().UtcDateTime);
    }

    // Con 4 dígitos hay 10.000 combinaciones por día: se reintenta ante colisión.
    // El índice único en base de datos es la garantía final.
    private async Task<string> GenerateUniqueAccountNumberAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxNumberGenerationAttempts; attempt++)
        {
            var candidate = accountNumberGenerator.Generate();
            if (!await accounts.AccountNumberExistsAsync(candidate, cancellationToken))
                return candidate;
        }

        throw new InvalidOperationException(
            "No fue posible generar un número de cuenta único tras varios intentos.");
    }
}
