<a href="https://www.buymeacoffee.com/rob10e" target="_blank">
<img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 48px !important;width: 217px !important;" >
</a>
<form action="https://www.paypal.com/donate" method="post" target="_top">
<input type="hidden" name="business" value="74QKWV23NLD9Q" />
<input type="hidden" name="no_recurring" value="0" />
<input type="hidden" name="currency_code" value="USD" />
<input type="image" src="https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif" border="0" name="submit" title="PayPal - The safer, easier way to pay online!" alt="Donate with PayPal button" />
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1" />
</form>


## Installation
You can install the package via NuGet:

```bash
dotnet add package FluentPatternMatch
```
or by using the NuGet Package Manager Console:

```bash
Install-Package FluentPatternMatch
```
or by adding it to your `.csproj` file:

```xml
<PackageReference Include="FluentPatternMatch" Version="1.0.0" />
```


# FluentPatternMatch\<T, TResult\>

A robust, fluent, and fully-featured C# pattern-matching utility for expressive, readable, and safe value, predicate, and type-based matching.

## Key Features

- **Fluent API** for building match expressions
- **Synchronous and asynchronous** matching (including async/await support)
- **Value, predicate, and type-based** case matching
- **First-match or all-matches** (configurable)
- **Comprehensive extension methods** for common patterns:
  - Null/not-null
  - Ranges (int, double, decimal)
  - String patterns (contains, starts/ends with, regex)
  - Enum and record matching
  - Collections (map with pattern matching)
- **Discriminated union/type matching** (runtime type)
- **Diagnostics and logging** of match attempts and results
- **Custom error handling** (per-case and global)
- **Chaining and one-liner support** for concise code



## Usage Examples

```csharp
// Fluent, extension-based matching
var result = input.Switch<string, int>()
    .Case("A", () => 1)
    .CaseStartsWith("B", () => 2)
    .Default(() => -1);

// Concise tuple-based matching
var quick = n.Match(
    (1, () => "One"),
    (2, () => "Two"),
    (_ => true, () => "Other"));

// Type-based (discriminated union style)
var typed = union.MatchType(
    (typeof(MyUnion.CaseA), _ => "A"),
    (typeof(MyUnion.CaseB), _ => "B"));

// Null/not-null handling
var nullable = nullableString.MatchNull(() => "Null", s => $"Value: {s}");

// Asynchronous matching
var asyncResult = await input.Switch<string, bool>()
    .CaseAsync(x => x == "A", async () => true)
    .DefaultAsync(async () => false);

// Pattern matching over collections
var all = new[] { 1, 2, 3 }.SwitchMany(m => m
    .CaseInRange(1, 2, () => "Low")
    .Default(() => "Other"));

// Regex and advanced string patterns
var regexMatch = input.Switch<string, string>()
    .CaseRegex(@"^\d+$", m => $"Digits: {m.Value}")
    .Default(() => "Not digits");

// Enum, record, and set membership
var enumResult = myEnum.Switch<MyEnum, string>()
    .CaseEnum(e => e == MyEnum.Foo, () => "Foo")
    .Default(() => "Other");

var recordResult = myRecord.Switch<MyRecord, string>()
    .CaseRecord(new MyRecord("X", 1), () => "Matched X/1")
    .Default(() => "Other");

var setResult = value.Switch<int, string>()
    .CaseIsOneOf(new[] { 1, 3, 5 }, () => "Odd")
    .Default(() => "Even");
```

#License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.


## Support & Donations

If you find this project useful, you can support future development:

- [☕ Buy Me a Coffee](https://www.buymeacoffee.com/rob10e)
- [❤️ GitHub Sponsors](https://github.com/sponsors/rob10e)
- [PayPal](https://www.paypal.com/donate/?business=74QKWV23NLD9Q&no_recurring=0&currency_code=USD)

Your support is greatly appreciated!
