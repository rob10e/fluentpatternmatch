

using FluentPatternMatch.Extensions;

namespace FluentPatternMatch;

public static class Program
{
    public abstract record MyUnion
    {
        public record CaseA(int Num) : MyUnion;
        public record CaseB(string Text) : MyUnion;
    }

    static async Task Main(string[] args)
    {
        // Switch/case via extension
        const int n = 42;
        var txt = n.Switch<int, string>()
            .Case(1, () => "One")
            .CaseInRange(10, 100, () => "Big range")
            .Default(() => "Other");
        Console.WriteLine($"Switch: {txt}");

        // Pattern match as one-liner
        var simple = n.Match(
            (x => x == 1, () => "One"),
            (x => x == 2, () => "Two"),
            (_ => true, () => "Other"));
        Console.WriteLine($"Match: {simple}");

        // Type/discriminated-union matching
        object o = new MyUnion.CaseB("hello!");
        var union = o.MatchType(
            (typeof(MyUnion.CaseA), _ => "Type A"),
            (typeof(MyUnion.CaseB), _ => "Type B"));
        Console.WriteLine($"Type match: {union}");

        // Null match
        string? nullable = null;
        var nullResult = nullable.MatchNull(
            () => "Is null",
            s => $"Value: {s}");
        Console.WriteLine($"Null: {nullResult}");

        // String pattern helpers
        var sresult = "foobar".SwitchString<string>()
            .CaseContains("foo", () => "Contains foo")
            .CaseStartsWith("bar", () => "Starts with bar")
            .CaseEndsWith("ar", () => "Ends with ar")
            .Default(() => "No match");
        Console.WriteLine($"String helpers: {sresult}");

        // Async match
        var asyncResult = await "Hello".Switch<string, bool>()
            .CaseAsync(s => s != null && s.StartsWith('H'), async () => { await Task.Delay(10); return true; }, "StartsWith H")
            .DefaultAsync(() => Task.FromResult(false));
        Console.WriteLine($"Async: {asyncResult}");

        // Collection bulk match
        var nums = new[] { 1, 2, 3, 4 };
        var bulk = nums.SwitchMany<int, string>(m =>
        {
            m.CaseIsOneOf(new[] { 2, 4 }, () => "Even")
                .CaseInRange(1, 3, () => "Low")
                .Default(() => "Other");
            return m;
        });
        Console.WriteLine("Bulk: " + string.Join(", ", bulk));
    }
}