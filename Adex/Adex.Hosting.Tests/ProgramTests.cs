using System.Reflection;
using Xunit;

namespace Adex.Hosting.Tests;

public class ProgramTests
{
    [Theory]
    [InlineData(typeof(Adex.Mcp.Program))]
    [InlineData(typeof(Adex.Mvc.Program))]
    [InlineData(typeof(Adex.WebApi.Program))]
    [InlineData(typeof(Adex.Importer.Program))]
    public void MainIsExplicitAndAsynchronous(Type program)
    {
        var main = program.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(main);
        Assert.Equal(typeof(Task), main.ReturnType);
        Assert.Equal(typeof(string[]), Assert.Single(main.GetParameters()).ParameterType);
    }
}
