namespace FluentPatternMatch.Extensions;

/// <summary>
/// Provides async extension methods for fluent pattern matching, enabling chaining of <c>DefaultAsync</c> after <c>CaseAsync</c> calls.
/// </summary>
public static class FluentPatternMatchAsyncExtensions
{
    /// <summary>
    /// Allows chaining <c>.DefaultAsync(...)</c> after <c>.CaseAsync(...)</c> when using async-fluent chaining.
    /// </summary>
    /// <typeparam name="T">The value type to match.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="matcherTask">The matcher task to continue from.</param>
    /// <param name="action">The async result function for the default case.</param>
    /// <param name="label">Optional label for diagnostics.</param>
    /// <returns>A task producing the result of the default case.</returns>
    public static async Task<TResult?> DefaultAsync<T, TResult>(
        this Task<FluentPatternMatch<T, TResult>> matcherTask,
        Func<Task<TResult>> action,
        string? label = null) =>
        await (await matcherTask.ConfigureAwait(false)).DefaultAsync(action, label).ConfigureAwait(false);

    /// <summary>
    /// Allows chaining <c>.DefaultAsync(...)</c> (void version) after <c>.CaseAsync(...)</c> in an async chain.
    /// </summary>
    /// <typeparam name="T">The value type to match.</typeparam>
    /// <typeparam name="TResult">The result type of the match.</typeparam>
    /// <param name="matcherTask">The matcher task to continue from.</param>
    /// <param name="action">The async action for the default case.</param>
    /// <param name="label">Optional label for diagnostics.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DefaultAsync<T, TResult>(
        this Task<FluentPatternMatch<T, TResult>> matcherTask,
        Func<Task> action,
        string? label = null) =>
        await (await matcherTask.ConfigureAwait(false)).DefaultAsync(action, label).ConfigureAwait(false);
}