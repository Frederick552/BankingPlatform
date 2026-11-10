namespace Banking.Domain.Exceptions;

/// <summary>
/// Base de todas las excepciones de negocio. Su mensaje es seguro para mostrarse al cliente de la API.
/// El mapeo a códigos HTTP vive en la capa de presentación (GlobalExceptionHandler).
/// </summary>
public abstract class BusinessException(string message) : Exception(message);

/// <summary>El recurso solicitado (cliente, cuenta, ...) no existe.</summary>
public sealed class NotFoundException(string message) : BusinessException(message);

/// <summary>Los datos recibidos violan una regla de negocio o de validación.</summary>
public sealed class DomainValidationException(string message) : BusinessException(message);

/// <summary>El retiro supera el saldo disponible de la cuenta.</summary>
public sealed class InsufficientFundsException()
    : BusinessException("Fondos insuficientes para realizar el retiro.");

/// <summary>La cuenta fue modificada por otra operación concurrente; el cliente puede reintentar.</summary>
public sealed class ConcurrencyConflictException()
    : BusinessException("La cuenta fue modificada por otra operación. Intente nuevamente.");
