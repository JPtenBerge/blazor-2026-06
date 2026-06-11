using AwesomeAssertions;
using BlazorApp1.Client.Components;
using Bunit;

namespace DemoProject.Client.Tests;

class Car
{
    public string Make { get; set; }
    public string Model { get; set; }

}

[TestClass]
public sealed class Test1
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

    [TestMethod]
    public void IntegrationTest()
    {
        var cars = new List<Car> {
            new() { Make = "Cupra", Model = "Born" },
            new() { Make = "Peugeot", Model = "208" },
            new() { Make = "Renault", Model = "5" },
            new() { Make = "Opel", Model = "Astra" },
            new() { Make = "Opel", Model = "Corsa" },
            new() { Make = "Ferrari", Model = "Enzo" },
            new() { Make = "Tesla", Model = "Roadster" },
            new() { Make = "Bugati", Model = "Veyron" },

        };

        var ctx = new Bunit.TestContext();
        //ctx.ComponentFactories.

        var fixture = ctx.Render<Autocompleter<Car>>(parameters =>
        {
            parameters.Add(p => p.Data, cars);
            parameters.Add(p => p.ItemTemplate, item => $"{item.Make} {item.Model}");
        });
        fixture.Instance.Query = "b";
        fixture.Instance.Autocomplete();
        fixture.Render();

        fixture.FindAll("li").Count.Should().Be(2);
        fixture.Markup.Should().ContainAll("Cupra Born", "Bugati Veyron");
    }

}
