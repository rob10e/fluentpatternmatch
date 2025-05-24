namespace FluentPatternMatch.Models;

/// <summary>
/// Represents a log entry for a pattern match attempt, including value, result, exception, and label.
/// </summary>
public record PatternLogEntry
{
    /// <summary>
    /// The UTC timestamp of the log entry.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// The index of the log entry in the match sequence.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// The label describing the case or action.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// The value being matched.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// The result of the match, if any.
    /// </summary>
    public object? Result { get; init; }

    /// <summary>
    /// The exception thrown during matching, if any.
    /// </summary>
    public Exception? Exception { get; init; }
}