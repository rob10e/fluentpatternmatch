namespace FluentPatternMatch.Extensions;

/// <summary>
/// Provides expressive extension methods for fluent pattern matching using <see cref="FluentPatternMatch{T, TResult}"/>.
/// </summary>
public static class FluentPatternMatchExtensions
{
    /// <summary>
    /// Starts a <see cref="FluentPatternMatch{string, TResult}"/> for a string value with an explicit result type.
    /// </summary>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="value">The string value to match.</param>
    /// <param name="breakOnMatch">If true, stops after the first match (default: true).</param>
    /// <param name="globalErrorHandler">Optional global error handler for case errors.</param>
    /// <returns>A new <see cref="FluentPatternMatch{string, TResult}"/> instance.</returns>
    public static FluentPatternMatch<string, TResult> SwitchString<TResult>(
        this string value,
        bool breakOnMatch = true,
        Func<Exception, object?, bool>? globalErrorHandler = null)
        => new(value, breakOnMatch, globalErrorHandler);

    /// <summary>
    /// Starts a <see cref="FluentPatternMatch{T, TResult}"/> for any value.
    /// </summary>
    /// <typeparam name="T">The value type to match.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="value">The value to match.</param>
    /// <param name="breakOnMatch">If true, stops after the first match (default: true).</param>
    /// <param name="globalErrorHandler">Optional global error handler for case errors.</param>
    /// <returns>A new <see cref="FluentPatternMatch{T, TResult}"/> instance.</returns>
    public static FluentPatternMatch<T, TResult> Switch<T, TResult>(
        this T value,
        bool breakOnMatch = true,
        Func<Exception, object?, bool>? globalErrorHandler = null)
        => new(value, breakOnMatch, globalErrorHandler);

    /// <summary>
    /// Starts an asynchronous <see cref="FluentPatternMatch{T, TResult}"/> for any value, configured by an async lambda.
    /// </summary>
    /// <typeparam name="T">The value type to match.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="value">The value to match.</param>
    /// <param name="configure">An async lambda to configure the matcher.</param>
    /// <param name="breakOnMatch">If true, stops after the first match (default: true).</param>
    /// <param name="globalErrorHandler">Optional global error handler for case errors.</param>
    /// <returns>A task producing a configured <see cref="FluentPatternMatch{T, TResult}"/>.</returns>
    public static Task<FluentPatternMatch<T, TResult>> SwitchAsync<T, TResult>(
        this T value,
        Func<FluentPatternMatch<T, TResult>, Task<FluentPatternMatch<T, TResult>>> configure,
        bool breakOnMatch = true,
        Func<Exception, object?, bool>? globalErrorHandler = null)
        => configure(new FluentPatternMatch<T, TResult>(value, breakOnMatch, globalErrorHandler));

    /// <summary>
    /// Immediately evaluates value cases (like switch/case) and returns the result.
    /// </summary>
    /// <typeparam name="T">The value type to match.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="value">The value to match.</param>
    /// <param name="cases">Cases as value-result pairs.</param>
    /// <returns>The result of the first matching case, or default if none match.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no case matches and no default is provided.</exception>
    public static TResult Match<T, TResult>(this T value, params (T When, Func<TResult> Result)[] cases)
    {
        var matcher = new FluentPatternMatch<T, TResult>(value);
        foreach (var (when, result) in cases)
            matcher.Case(when, result);
        return matcher.Default(() => default!) ?? throw new InvalidOperationException("No match found.");
    }

    /// <summary>
    /// Immediately evaluates predicate cases (pattern matching) and returns the result.
    /// </summary>
    /// <typeparam name="T">The value type to match.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="value">The value to match.</param>
    /// <param name="cases">Cases as predicate-result pairs.</param>
    /// <returns>The result of the first matching predicate, or default if none match.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no case matches and no default is provided.</exception>
    public static TResult Match<T, TResult>(this T value, params (Func<T, bool> When, Func<TResult> Result)[] cases)
    {
        var matcher = new FluentPatternMatch<T, TResult>(value);
        foreach (var (when, result) in cases)
            matcher.Case(when!, result);
        return matcher.Default(() => default!) ?? throw new InvalidOperationException("No match found.");
    }

    /// <summary>
    /// Pattern matches on the runtime type (discriminated union style).
    /// </summary>
    /// <typeparam name="TBase">The base type to match.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="value">The value to match.</param>
    /// <param name="cases">Cases as type-result pairs.</param>
    /// <returns>The result of the first matching type, or default if none match.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no case matches and no default is provided.</exception>
    public static TResult MatchType<TBase, TResult>(
        this TBase value,
        params (Type WhenType, Func<TBase, TResult> Result)[] cases)
    {
        var matcher = new FluentPatternMatch<TBase, TResult>(value);
        foreach (var (whenType, result) in cases)
        {
            matcher.Case(x => x != null && x.GetType() == whenType, () => result(value));
        }

        return matcher.Default(() => default!) ?? throw new InvalidOperationException("No match found.");
    }

    /// <summary>
    /// Matches on null or not-null values.
    /// </summary>
    /// <typeparam name="T">The value type to match.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="value">The value to match.</param>
    /// <param name="whenNull">Result if value is null.</param>
    /// <param name="whenNotNull">Result if value is not null.</param>
    /// <returns>The result of the appropriate case.</returns>
    public static TResult MatchNull<T, TResult>(
        this T? value,
        Func<TResult> whenNull,
        Func<T, TResult> whenNotNull)
    {
        if (value is null)
            return whenNull();
        return whenNotNull(value);
    }

    /// <summary>
    /// Applies pattern matching to each item in a collection and collects the results.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="source">The collection to match.</param>
    /// <param name="configure">A function to configure the matcher for each item.</param>
    /// <returns>An enumerable of all match results.</returns>
    public static IEnumerable<TResult> SwitchMany<T, TResult>(
        this IEnumerable<T> source,
        Func<FluentPatternMatch<T, TResult>, FluentPatternMatch<T, TResult>> configure)
    {
        foreach (var item in source)
        {
            var matcher = new FluentPatternMatch<T, TResult>(item, breakOnMatch: false);
            configure(matcher);
            foreach (var result in matcher.AllResults)
                yield return result;
        }
    }

    /// <summary>
    /// Adds a regular expression pattern match case for strings.
    /// </summary>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="matcher">The matcher to add the case to.</param>
    /// <param name="pattern">The regex pattern.</param>
    /// <param name="action">The result function, given the regex match.</param>
    /// <param name="label">Optional label for diagnostics.</param>
    /// <returns>The matcher for chaining.</returns>
    public static FluentPatternMatch<string, TResult> CaseRegex<TResult>(
        this FluentPatternMatch<string, TResult> matcher,
        string pattern,
        Func<System.Text.RegularExpressions.Match, TResult> action,
        string? label = null)
    {
        return matcher.Case(
            s =>
            {
                if (s is null) return false;
                var m = System.Text.RegularExpressions.Regex.Match(s, pattern);
                return m.Success;
            },
            () => action(System.Text.RegularExpressions.Regex.Match(matcher.Value!, pattern)),
            label ?? $"Regex: {pattern}");
    }

    /// <summary>
    /// Adds a pattern match case for records by value equality.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="matcher">The matcher to add the case to.</param>
    /// <param name="value">The record value to match.</param>
    /// <param name="action">The result function.</param>
    /// <param name="label">Optional label for diagnostics.</param>
    /// <returns>The matcher for chaining.</returns>
    public static FluentPatternMatch<T, TResult> CaseRecord<T, TResult>(
        this FluentPatternMatch<T, TResult> matcher,
        T value,
        Func<TResult> action,
        string? label = null)
        where T : notnull
    {
        return matcher.Case(x => EqualityComparer<T>.Default.Equals(x!, value), action, label ?? $"Record: {value}");
    }

    /// <summary>
    /// Adds a pattern match case for enum values using a predicate.
    /// </summary>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="matcher">The matcher to add the case to.</param>
    /// <param name="predicate">Predicate to match enum values.</param>
    /// <param name="action">The result function.</param>
    /// <param name="label">Optional label for diagnostics.</param>
    /// <returns>The matcher for chaining.</returns>
    public static FluentPatternMatch<TEnum, TResult> CaseEnum<TResult, TEnum>(
        this FluentPatternMatch<TEnum, TResult> matcher,
        Func<TEnum, bool> predicate,
        Func<TResult> action,
        string? label = null)
        where TEnum : Enum =>
        matcher.Case(predicate!, action, label);

    /// <summary>
    /// Adds a pattern match case for values that are one of a given set.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="matcher">The matcher to add the case to.</param>
    /// <param name="values">The set of values to match.</param>
    /// <param name="action">The result function.</param>
    /// <param name="label">Optional label for diagnostics.</param>
    /// <returns>The matcher for chaining.</returns>
    public static FluentPatternMatch<T, TResult> CaseIsOneOf<T, TResult>(
        this FluentPatternMatch<T, TResult> matcher,
        IEnumerable<T> values,
        Func<TResult> action,
        string? label = null) =>
        matcher.Case(x => new HashSet<T>(values).Contains(x!), action, label);

    /// <summary>
    /// Adds a pattern match case for strings that contain a given substring.
    /// </summary>
    public static FluentPatternMatch<string, TResult> CaseContains<TResult>(
        this FluentPatternMatch<string, TResult> matcher,
        string substring,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(s => s != null && s.Contains(substring), action, label);

    /// <summary>
    /// Adds a pattern match case for strings that start with a given substring.
    /// </summary>
    public static FluentPatternMatch<string, TResult> CaseStartsWith<TResult>(
        this FluentPatternMatch<string, TResult> matcher,
        string start,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(s => s != null && s.StartsWith(start), action, label);

    /// <summary>
    /// Adds a pattern match case for strings that end with a given substring.
    /// </summary>
    public static FluentPatternMatch<string, TResult> CaseEndsWith<TResult>(
        this FluentPatternMatch<string, TResult> matcher,
        string end,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(s => s != null && s.EndsWith(end), action, label);

    /// <summary>
    /// Adds a pattern match case for integers within a specified range (inclusive).
    /// </summary>
    public static FluentPatternMatch<int, TResult> CaseInRange<TResult>(
        this FluentPatternMatch<int, TResult> matcher,
        int min,
        int max,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(x => x >= min && x <= max, action, label);

    /// <summary>
    /// Adds a pattern match case for integers greater than a specified value.
    /// </summary>
    public static FluentPatternMatch<int, TResult> CaseGreaterThan<TResult>(
        this FluentPatternMatch<int, TResult> matcher,
        int value,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(x => x > value, action, label ?? $">{value}");

    /// <summary>
    /// Adds a pattern match case for integers less than a specified value.
    /// </summary>
    public static FluentPatternMatch<int, TResult> CaseLessThan<TResult>(
        this FluentPatternMatch<int, TResult> matcher,
        int value,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(x => x < value, action, label ?? $"<{value}");

    /// <summary>
    /// Adds a pattern match case for integers equal to a specified value.
    /// </summary>
    public static FluentPatternMatch<int, TResult> CaseEquals<TResult>(
        this FluentPatternMatch<int, TResult> matcher,
        int value,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(x => x == value, action, label ?? $"=={value}");

    /// <summary>
    /// Adds a pattern match case for doubles within a specified range (inclusive).
    /// </summary>
    public static FluentPatternMatch<double, TResult> CaseInRange<TResult>(
        this FluentPatternMatch<double, TResult> matcher,
        double min,
        double max,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(x => x >= min && x <= max, action, label);

    /// <summary>
    /// Adds a pattern match case for decimals within a specified range (inclusive).
    /// </summary>
    public static FluentPatternMatch<decimal, TResult> CaseInRange<TResult>(
        this FluentPatternMatch<decimal, TResult> matcher,
        decimal min,
        decimal max,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(x => x >= min && x <= max, action, label);

    /// <summary>
    /// Adds a pattern match case for boolean true.
    /// </summary>
    public static FluentPatternMatch<bool, TResult> CaseTrue<TResult>(
        this FluentPatternMatch<bool, TResult> matcher,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(x => x, action, label ?? "True");

    /// <summary>
    /// Adds a pattern match case for boolean false.
    /// </summary>
    public static FluentPatternMatch<bool, TResult> CaseFalse<TResult>(
        this FluentPatternMatch<bool, TResult> matcher,
        Func<TResult> action,
        string? label = null)
        => matcher.Case(x => x == false, action, label ?? "False");
}
