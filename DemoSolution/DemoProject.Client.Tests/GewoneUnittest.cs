using AwesomeAssertions;
using BlazorApp1.Client.Components;

namespace DemoProject.Client.Tests;

[TestClass]
public sealed class GewoneUnittest
{
    [TestMethod]
    public void TestMethod1()
    {
        var sut = new Autocompleter<Car>();
        sut.Query = "b";
        sut.Data = [
            new() { Make = "Cupra", Model = "Born" },
            new() { Make = "Peugeot", Model = "208" },
            new() { Make = "Renault", Model = "5" },
            new() { Make = "Opel", Model = "Astra" },
            new() { Make = "Opel", Model = "Corsa" },
            new() { Make = "Ferrari", Model = "Enzo" },
            new() { Make = "Tesla", Model = "Roadster" },
            new() { Make = "Bugati", Model = "Veyron" },
        ];

        sut.Autocomplete();

        //var expected = new List<Car> { sut.Data[0], sut.Data[^1] };
        //CollectionAssert.AreEquivalent(expected, sut.Suggestions);

        var expected = new List<Car>
        {
            new() { Make = "Cupra", Model = "Born" },
            new() { Make = "Bugati", Model = "Veyron" },
        };
        sut.Suggestions.Should().BeEquivalentTo(expected);
    }
}
