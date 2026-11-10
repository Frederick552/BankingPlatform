using System.Text.RegularExpressions;
using Banking.Application.Services;
using Banking.Tests.Support;

namespace Banking.Tests;

public class AccountNumberGeneratorTests
{
    private static readonly DateTimeOffset Today = new(2026, 6, 18, 10, 30, 0, TimeSpan.Zero);

    private static AccountNumberGenerator CreateGenerator(DateTimeOffset? now = null) =>
        new(new FakeTimeProvider(now ?? Today));

    [Fact]
    public void Generate_HasExactLengthOf17Characters()
    {
        var number = CreateGenerator().Generate();

        Assert.Equal(17, number.Length); // "ACC-" + 8 + "-" + 4
    }

    [Fact]
    public void Generate_StartsWithAccPrefix()
    {
        var number = CreateGenerator().Generate();

        Assert.StartsWith("ACC-", number);
    }

    [Fact]
    public void Generate_ContainsTodaysDate()
    {
        var number = CreateGenerator(Today).Generate();

        Assert.Equal("20260618", number.Split('-')[1]);
    }

    [Fact]
    public void Generate_UsesTheCurrentDate_WhenTheClockMoves()
    {
        var clock = new FakeTimeProvider(Today);
        var generator = new AccountNumberGenerator(clock);

        clock.Advance(TimeSpan.FromDays(1));

        Assert.Contains("-20260619-", generator.Generate());
    }

    [Fact]
    public void Generate_MatchesFormatWithFourNumericDigits()
    {
        var generator = CreateGenerator();

        for (var i = 0; i < 100; i++)
            Assert.Matches(@"^ACC-\d{8}-\d{4}$", generator.Generate());
    }

    [Fact]
    public void Generate_ProducesUniqueNumbers_ForConsecutiveCalls()
    {
        var generator = CreateGenerator();

        var numbers = Enumerable.Range(0, 1_000).Select(_ => generator.Generate()).ToList();

        Assert.Equal(numbers.Count, numbers.Distinct().Count());
    }

    [Fact]
    public void Generate_CoversAllTenThousandSuffixesWithoutRepeating()
    {
        var generator = CreateGenerator();

        var suffixes = Enumerable.Range(0, 10_000)
            .Select(_ => Regex.Match(generator.Generate(), @"\d{4}$").Value)
            .ToHashSet();

        Assert.Equal(10_000, suffixes.Count);
    }
}
