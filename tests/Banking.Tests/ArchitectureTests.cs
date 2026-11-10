using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Banking.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Controllers_DoNotDependOnDbContext()
    {
        var controllers = typeof(Program).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))
            .ToList();

        Assert.NotEmpty(controllers);

        foreach (var controller in controllers)
        {
            var dependencies = controller
                .GetConstructors()
                .SelectMany(c => c.GetParameters().Select(p => p.ParameterType))
                .Concat(controller
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Select(f => f.FieldType));

            Assert.DoesNotContain(dependencies, type => typeof(DbContext).IsAssignableFrom(type));
        }
    }
}
