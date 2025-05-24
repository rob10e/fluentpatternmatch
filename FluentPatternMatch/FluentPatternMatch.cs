using FluentPatternMatch.Models;

namespace FluentPatternMatch;

/// <summary>
/// Fluent, flexible pattern matcher for C# supporting synchronous and asynchronous cases, with diagnostics and error handling.
/// </summary>
/// <typeparam name="T">The value type to match.</typeparam>
/// <typeparam name="TResult">The result type of the match.</typeparam>
public class FluentPatternMatch<T, TResult>
{
    private bool _matched = false;
    private readonly bool _breakOnMatch;
    private readonly List<PatternLogEntry> _matchLogs = new();
    private readonly List<TResult> _allResults = new();
    private readonly Func<Exception, object?, bool>? _globalErrorHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentPatternMatch{T, TResult}"/> class.
    /// </summary>
    /// <param name="value">The value to match against.</param>
    /// <param name="breakOnMatch">If true, stops after the first match (default: true).</param>
    /// <param name="globalErrorHandler">Optional global error handler for case errors.</param>
    public FluentPatternMatch(
        T? value,
        bool breakOnMatch = true,
        Func<Exception, object?, bool>? globalErrorHandler = null)
    {
        Value = value;
        _breakOnMatch = breakOnMatch;
        _globalErrorHandler = globalErrorHandler;
        Result = default;
    }

    #region Case Overloads

    /// <summary>
    /// Adds a case with a predicate and result function.
    /// </summary>
    /// <param name="predicate">Predicate to determine if the case matches.</param>
    /// <param name="action">Result function if matched.</param>
    /// <param name="label">Optional label for diagnostics.</param>
    /// <param name="errorHandler">Optional error handler for this case.</param>
    /// <returns>The matcher for chaining.</returns>
    public FluentPatternMatch<T, TResult> Case(
        Func<T?, bool> predicate,
        Func<TResult> action,
        string? label = null,
        Func<Exception, object?, bool>? errorHandler = null)
    {
        if (_matched && _breakOnMatch) return this;
        try
        {
            if (predicate(Value))
            {
                Result = action();
                _allResults.Add(Result);
                _matched = true;
                AddLog(label ?? "Case", Value, Result, null);
                if (_breakOnMatch) return this;
            }
        }
        catch (Exception ex)
        {
            HandleCaseError(ex, Value, errorHandler);
        }
        return this;
    }

    /// <summary>
    /// Adds a case with a predicate and void action.
    /// </summary>
    public FluentPatternMatch<T, TResult> Case(
        Func<T?, bool> predicate,
        Action action,
        string? label = null,
        Func<Exception, object?, bool>? errorHandler = null)
    {
        if (_matched && _breakOnMatch) return this;
        try
        {
            if (predicate(Value))
            {
                action();
                _matched = true;
                AddLog(label ?? "Case", Value, null, null);
                if (_breakOnMatch) return this;
            }
        }
        catch (Exception ex)
        {
            HandleCaseError(ex, Value, errorHandler);
        }
        return this;
    }

    /// <summary>
    /// Adds a case for a specific value.
    /// </summary>
    public FluentPatternMatch<T, TResult> Case(
        T? value,
        Func<TResult> action,
        string? label = null,
        Func<Exception, object?, bool>? errorHandler = null)
        => Case(x => EqualityComparer<T?>.Default.Equals(x, value), action, label, errorHandler);

    /// <summary>
    /// Adds a case for a specific value with a void action.
    /// </summary>
    public FluentPatternMatch<T, TResult> Case(
        T? value,
        Action action,
        string? label = null,
        Func<Exception, object?, bool>? errorHandler = null)
        => Case(x => EqualityComparer<T?>.Default.Equals(x, value), action, label, errorHandler);

    /// <summary>
    /// Adds a case for a specific type, using a result function.
    /// </summary>
    /// <typeparam name="TCase">The type to match.</typeparam>
    public FluentPatternMatch<T, TResult> Case<TCase>(
        Func<TCase, TResult> action,
        string? label = null,
        Func<Exception, object?, bool>? errorHandler = null)
        where TCase : class
        => Case(
            x => x is TCase,
            () => action((TCase)(object)Value!),
            label ?? typeof(TCase).Name,
            errorHandler
        );

    #endregion

    #region Default Overloads

    /// <summary>
    /// Executes the default result function if no previous case matched.
    /// </summary>
    /// <param name="action">Default result function.</param>
    /// <param name="label">Optional label for diagnostics.</param>
    /// <returns>The result.</returns>
    public TResult? Default(Func<TResult> action, string? label = null)
    {
        if (_matched) return Result;
        Result = action();
        _allResults.Add(Result);
        AddLog(label ?? "Default", Value, Result, null);
        return Result;
    }

    /// <summary>
    /// Executes the default void action if no previous case matched.
    /// </summary>
    public void Default(Action action, string? label = null)
    {
        if (_matched) return;
        action();
        AddLog(label ?? "Default", Value, null, null);
    }

    #endregion

    #region Async Case Overloads

    /// <summary>
    /// Adds an asynchronous case with a predicate and async result function.
    /// </summary>
    public async Task<FluentPatternMatch<T, TResult>> CaseAsync(
        Func<T?, bool> predicate,
        Func<Task<TResult>> action,
        string? label = null,
        Func<Exception, object?, bool>? errorHandler = null)
    {
        if (_matched && _breakOnMatch) return this;
        try
        {
            if (predicate(Value))
            {
                Result = await action();
                _allResults.Add(Result);
                _matched = true;
                AddLog(label ?? "CaseAsync", Value, Result, null);
                if (_breakOnMatch) return this;
            }
        }
        catch (Exception ex)
        {
            HandleCaseError(ex, Value, errorHandler);
        }
        return this;
    }

    /// <summary>
    /// Adds an asynchronous case with a predicate and async void action.
    /// </summary>
    public async Task<FluentPatternMatch<T, TResult>> CaseAsync(
        Func<T?, bool> predicate,
        Func<Task> action,
        string? label = null,
        Func<Exception, object?, bool>? errorHandler = null)
    {
        if (_matched && _breakOnMatch) return this;
        try
        {
            if (predicate(Value))
            {
                await action();
                _matched = true;
                AddLog(label ?? "CaseAsync", Value, null, null);
                if (_breakOnMatch) return this;
            }
        }
        catch (Exception ex)
        {
            HandleCaseError(ex, Value, errorHandler);
        }
        return this;
    }

    /// <summary>
    /// Adds an asynchronous case for a specific type, using an async result function.
    /// </summary>
    /// <typeparam name="TCase">The type to match.</typeparam>
    public async Task<FluentPatternMatch<T, TResult>> CaseAsync<TCase>(
        Func<TCase, Task<TResult>> action,
        string? label = null,
        Func<Exception, object?, bool>? errorHandler = null)
        where TCase : class
        => await CaseAsync(
            x => x is TCase,
            () => action((TCase)(object)Value!),
            label ?? typeof(TCase).Name,
            errorHandler
        );

    /// <summary>
    /// Adds an asynchronous case for a specific value, using an async result function.
    /// </summary>
    public async Task<FluentPatternMatch<T, TResult>> CaseAsync(
        T? value,
        Func<Task<TResult>> action,
        string? label = null,
        Func<Exception, object?, bool>? errorHandler = null)
        => await CaseAsync(x => EqualityComparer<T?>.Default.Equals(x, value), action, label, errorHandler);

    /// <summary>
    /// Adds an asynchronous case for a specific value, using an async void action.
    /// </summary>
    public async Task<FluentPatternMatch<T, TResult>> CaseAsync(
        T? value,
        Func<Task> action,
        string? label = null,
        Func<Exception, object?, bool>? errorHandler = null)
        => await CaseAsync(x => EqualityComparer<T?>.Default.Equals(x, value), action, label, errorHandler);

    #endregion

    #region Async Default

    /// <summary>
    /// Executes the default async result function if no previous case matched.
    /// </summary>
    /// <param name="action">Default async result function.</param>
    /// <param name="label">Optional label for diagnostics.</param>
    /// <returns>The result.</returns>
    public async Task<TResult?> DefaultAsync(Func<Task<TResult>> action, string? label = null)
    {
        if (_matched) return Result;
        Result = await action();
        _allResults.Add(Result);
        AddLog(label ?? "DefaultAsync", Value, Result, null);
        return Result;
    }

    /// <summary>
    /// Executes the default async void action if no previous case matched.
    /// </summary>
    public async Task DefaultAsync(Func<Task> action, string? label = null)
    {
        if (!_matched)
        {
            await action();
            AddLog(label ?? "DefaultAsync", Value, null, null);
        }
    }

    #endregion

    #region Error Handling

    private void HandleCaseError(Exception ex, object? value, Func<Exception, object?, bool>? errorHandler)
    {
        var handled = false;
        if (errorHandler != null)
        {
            handled = errorHandler(ex, value);
        }
        if (!handled && _globalErrorHandler != null)
        {
            handled = _globalErrorHandler(ex, value);
        }
        if (!handled)
        {
            // throw; // rethrow if unhandled
        }
        AddLog("Error", value, null, ex);
    }

    #endregion

    #region Diagnostics & Utilities

    private void AddLog(string label, object? value, object? result, Exception? ex)
    {
        _matchLogs.Add(new PatternLogEntry
        {
            Timestamp = DateTime.UtcNow,
            Index = _matchLogs.Count,
            Label = label,
            Value = value,
            Result = result,
            Exception = ex
        });
    }

    /// <summary>
    /// Gets the value being matched.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the result of the first matched case, or default if none matched.
    /// </summary>
    public TResult? Result { get; private set; }

    /// <summary>
    /// Gets all results from matched cases (if <c>breakOnMatch</c> is false).
    /// </summary>
    public IReadOnlyList<TResult> AllResults => _allResults;
    /// <summary>
    /// Gets the log of all pattern match attempts and results.
    /// </summary>
    public IReadOnlyList<PatternLogEntry> MatchLogs => _matchLogs;

    /// <summary>
    /// Implicitly converts the matcher to its result.
    /// </summary>
    public static implicit operator TResult?(FluentPatternMatch<T, TResult> s) => s.Result;

    #endregion
}