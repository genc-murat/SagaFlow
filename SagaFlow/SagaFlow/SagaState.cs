namespace SagaFlow;

/// <summary>
/// Enumerates the possible states of a saga in its lifecycle.
/// </summary>
public enum SagaState
{
    /// <summary>
    /// Indicates that the saga has not been started yet.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Indicates that the saga has started but is not yet completed.
    /// </summary>
    PartiallyCompleted,

    /// <summary>
    /// Indicates that the saga has successfully completed all its steps.
    /// </summary>
    Completed,

    /// <summary>
    /// Indicates that the saga has failed and may require rollback or manual intervention.
    /// </summary>
    Failed,

    /// <summary>
    /// Indicates an unknown or uninitialized state of the saga.
    /// </summary>
    Unknown
}

