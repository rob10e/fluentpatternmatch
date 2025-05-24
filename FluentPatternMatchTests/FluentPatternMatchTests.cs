using FluentPatternMatch;
using FluentPatternMatch.Extensions;

namespace FluentPatternMatchTests;

public class FluentPatternMatchTests
{
    [Fact]
    public void ValueMatch_ReturnsExpected()
    {
        var result = 5.Switch<int, string>()
            .Case(3, () => "Three")
            .Case(5, () => "Five")
            .Default(() => "Other");
        Assert.Equal("Five", result);
    }

    [Fact]
    public void PredicateMatch_LogsMatch()
    {
        var matcher = 42.Switch<int, string>()
            .Case(x => x > 10, () => "Big", "BigMatch");
        matcher.Default(() => "Small", "SmallMatch");
        Assert.Contains("BigMatch", string.Join(",", matcher.MatchLogs));
    }

    [Fact]
    public async Task AsyncCase_MatchesAsync()
    {
        var sw = await "Hi".Switch<string, bool>()
            .CaseAsync(x => x == "Hi", async () => { await Task.Delay(1); return true; }, "MatchHi")
            .DefaultAsync(() => Task.FromResult(false), "NoMatch");
        Assert.True(sw);
    }

    [Fact]
    public void TypeCase_DiscriminatedUnion()
    {
        object o = new Program.MyUnion.CaseA(99);
        var result = o.MatchType(
            (typeof(Program.MyUnion.CaseA), _ => "Num"),
            (typeof(Program.MyUnion.CaseB), _ => "Text"));
        Assert.Equal("Num", result);
    }

    [Fact]
    public void NullMatch()
    {
        string? n = null;
        var res = n.MatchNull(() => "Null", s => s);
        Assert.Equal("Null", res);
    }

    [Fact]
    public void BulkSwitch()
    {
        var nums = new[] { 1, 2, 3 };
        var all = nums.SwitchMany<int, string>(m =>
        {
            m.CaseIsOneOf([2], () => "Even")
                .CaseInRange(1, 2, () => "Low")
                .Default(() => "Other");
            return m;
        }).ToList();
        Assert.Contains("Even", all);
        Assert.Contains("Low", all);
    }
}