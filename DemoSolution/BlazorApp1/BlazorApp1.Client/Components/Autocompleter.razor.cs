using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorApp1.Client.Components;

public partial class Autocompleter<T>
{
    [Parameter] public List<T> Data { get; set; }
    [Parameter] public EventCallback<T> OnSelect { get; set; }
    public string Query { get; set; }
    public List<T>? Suggestions { get; set; }
    public int? ActiveSuggestionIndex { get; set; }

    void Autocomplete()
    {
        Console.WriteLine($"autocmopleting! {Query}");

        if (string.IsNullOrEmpty(Query))
        {
            Suggestions = null;
            return;
        }

        Suggestions = new();

        foreach (var item in Data)
        {
            // reflection

            var props = item.GetType().GetProperties().Where(x => x.PropertyType == typeof(string));
            foreach (var prop in props)
            {
                var value = prop.GetValue(item) as string;
                if (!string.IsNullOrEmpty(value) && value.Contains(Query, StringComparison.OrdinalIgnoreCase))
                {
                    Suggestions.Add(item);
                    break;
                }
            }
        }
    }

    async Task HandleKeydown(KeyboardEventArgs args)
    {
        if (args.Key == "ArrowDown")
        {
            Next();
        }
        else if (args.Key == "ArrowUp")
        {
            //Previous();
        }
        else if (args.Key == "Enter")
        {
            await Select();
        }
    }

    void Next()
    {
        if (Suggestions is null) return;

        if (!ActiveSuggestionIndex.HasValue)
        {
            ActiveSuggestionIndex = 0;
            return;
        }
        ActiveSuggestionIndex = (ActiveSuggestionIndex + 1) % Suggestions.Count;
    }

    async Task Select()
    {
        if (Suggestions is null || !ActiveSuggestionIndex.HasValue) return;
        await OnSelect.InvokeAsync(Suggestions[ActiveSuggestionIndex.Value]);
    }
}
