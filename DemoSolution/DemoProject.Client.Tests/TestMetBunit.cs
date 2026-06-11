using AwesomeAssertions;
using BlazorApp1.Client.Components;
using Bunit;

namespace DemoProject.Client.Tests;

[TestClass]
public sealed class TestMetBunit : BunitContext
{
    [TestMethod]
    public void IntegrationTest()
    {
        var cars = new List<Car>
        {
            new() { Make = "Cupra", Model = "Born" },
            new() { Make = "Peugeot", Model = "208" },
            new() { Make = "Renault", Model = "5" },
            new() { Make = "Opel", Model = "Astra" },
            new() { Make = "Opel", Model = "Corsa" },
            new() { Make = "Ferrari", Model = "Enzo" },
            new() { Make = "Tesla", Model = "Roadster" },
            new() { Make = "Bugati", Model = "Veyron" },
        };

        var fixture = Render<Autocompleter<Car>>(parameters =>
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
