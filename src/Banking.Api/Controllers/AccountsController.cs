using Banking.Application.Dtos;
using Banking.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController(
    IAccountService accountService,
    ITransactionService transactionService) : ControllerBase
{
    /// <summary>Abre una cuenta para un cliente existente. El número de cuenta se autogenera.</summary>
    [HttpPost]
    [ProducesResponseType<AccountDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> Create(
        [FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var account = await accountService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetBalance), new { accountNumber = account.AccountNumber }, account);
    }

    /// <summary>Consulta el saldo actual a partir del número de cuenta.</summary>
    [HttpGet("{accountNumber}/balance")]
    [ProducesResponseType<BalanceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BalanceDto>> GetBalance(string accountNumber, CancellationToken cancellationToken) =>
        Ok(await accountService.GetBalanceAsync(accountNumber, cancellationToken));

    /// <summary>Registra un depósito (incrementa el saldo).</summary>
    [HttpPost("{accountNumber}/deposits")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> Deposit(
        string accountNumber, [FromBody] TransactionRequest request, CancellationToken cancellationToken) =>
        Ok(await transactionService.DepositAsync(accountNumber, request.Amount, cancellationToken));

    /// <summary>Registra un retiro (disminuye el saldo). Se rechaza si no hay fondos suficientes.</summary>
    [HttpPost("{accountNumber}/withdrawals")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> Withdraw(
        string accountNumber, [FromBody] TransactionRequest request, CancellationToken cancellationToken) =>
        Ok(await transactionService.WithdrawAsync(accountNumber, request.Amount, cancellationToken));

    /// <summary>Historial cronológico de movimientos, con el saldo resultante de cada uno.</summary>
    [HttpGet("{accountNumber}/transactions")]
    [ProducesResponseType<TransactionHistoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionHistoryDto>> GetTransactions(
        string accountNumber, CancellationToken cancellationToken) =>
        Ok(await transactionService.GetHistoryAsync(accountNumber, cancellationToken));
}
