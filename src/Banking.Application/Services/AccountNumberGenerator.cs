using Banking.Application.Abstractions;

namespace Banking.Application.Services;

/// <summary>
/// Genera números con el formato <c>ACC-YYYYMMDD-XXXX</c>.
/// Los 4 dígitos finales son pseudoaleatorios y secuenciales: parten de un punto aleatorio y avanzan
/// con un paso coprimo de 10.000, por lo que 10.000 llamadas consecutivas nunca repiten sufijo
/// (y aun así no se ven correlativos). Debe registrarse como singleton.
/// </summary>
public sealed class AccountNumberGenerator(TimeProvider timeProvider) : IAccountNumberGenerator
{
    public const string Prefix = "ACC-";

    private const int Modulus = 10_000;
    private const int Step = 7_919; // primo, por lo tanto coprimo con 10.000 => permutación completa

    private long _sequence = Random.Shared.Next(Modulus);

    public string Generate()
    {
        var today = timeProvider.GetUtcNow().UtcDateTime;
        var n = Interlocked.Increment(ref _sequence);
        var suffix = (int)(n * Step % Modulus);

        return $"{Prefix}{today:yyyyMMdd}-{suffix:D4}";
    }
}
