namespace SagaFlow;

/// <summary>
/// Coordinates the execution of saga steps and maintains the saga state.
/// </summary>
/// <typeparam name="TState">The type of the saga state.</typeparam>
public class SagaCoordinator<TState>
{
    private readonly List<SagaLogEntry<TState>> _log = new();

    /// <summary>
    /// Event triggered when a saga step is completed successfully.
    /// </summary>
    public event SagaStepCompletedHandler<TState> OnStepCompleted;

    /// <summary>
    /// Event triggered when a saga step fails.
    /// </summary>
    public event SagaStepFailedHandler<TState> OnStepFailed;

    private readonly List<ISagaStep<TState>> _steps;

    private SagaState _currentState = SagaState.NotStarted;

    /// <summary>
    /// Delegate for handling saga state changes.
    /// </summary>
    public delegate void SagaStateChangedDelegate(SagaState newState, SagaLogEntry<TState> logEntry);

    /// <summary>
    /// Event triggered when the saga state changes.
    /// </summary>
    public event SagaStateChangedDelegate OnSagaStateChanged;

    private readonly ISagaStateRepository<TState> _repository;

    /// <summary>
    /// Initializes a new instance of the SagaCoordinator class.
    /// </summary>
    /// <param name="repository">The saga state repository.</param>
    /// <param name="steps">The initial set of saga steps.</param>
    public SagaCoordinator(ISagaStateRepository<TState> repository, IEnumerable<ISagaStep<TState>> steps)
    {
        _steps = steps.ToList();
        _repository = repository;
    }

    /// <summary>
    /// Asynchronously saves the state of a saga to the repository.
    /// </summary>
    /// <param name="sagaId">The unique identifier for the saga.</param>
    /// <param name="logEntry">The log entry to be saved, which encapsulates the state and other metadata.</param>
    /// <returns>A Task representing the asynchronous save operation.</returns>
    private async Task SaveState(Guid sagaId, SagaLogEntry<TState> logEntry)
    {
        await _repository.SaveAsync(sagaId, logEntry);
    }

    /// <summary>
    /// Asynchronously changes the current state of the saga, logs it, and triggers state change events.
    /// </summary>
    /// <param name="sagaId">The unique identifier of the saga.</param>
    /// <param name="newState">The new state that the saga will transition to.</param>
    /// <param name="step">The saga step responsible for the state change.</param>
    /// <returns>A Task representing the asynchronous state change operation.</returns>
    private async Task ChangeState(Guid sagaId, SagaState newState, ISagaStep<TState> step)
    {
        var logEntry = new SagaLogEntry<TState>(step, newState);
        _log.Add(logEntry);
        _currentState = newState;
        OnSagaStateChanged?.Invoke(newState, logEntry);
        await SaveState(sagaId, logEntry);
    }

    /// <summary>
    /// Adds a new saga step to the coordinator.
    /// </summary>
    /// <param name="step">The saga step to add.</param>
    public void AddStep(ISagaStep<TState> step)
    {
        _steps.Add(step);
    }

    /// <summary>
    /// Checks whether all dependencies for a given saga step have been met.
    /// </summary>
    /// <param name="step">The saga step whose dependencies need to be checked.</param>
    /// <returns>
    /// True if all dependencies have been met (or if there are no dependencies), otherwise false.
    /// </returns>
    private bool DependenciesMet(ISagaStep<TState> step)
    {
        var dependencies = step.Dependencies;
        return dependencies == null || dependencies.All(d => _log.Any(log => log.Step.GetType() == d && log.State == SagaState.Completed));
    }

    /// <summary>
    /// Executes the saga steps asynchronously.
    /// </summary>
    /// <param name="sagaId">The unique identifier of the saga.</param>
    /// <param name="state">The current state of the saga.</param>
    /// <param name="timeout">The time limit for executing the saga steps.</param>
    /// <returns>A Task that resolves to a boolean indicating whether the saga execution was successful.</returns>
    public async Task<bool> ExecuteAsync(Guid sagaId, TState state, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);

        foreach (var step in _steps)
        {
            if (!DependenciesMet(step))
            {
                throw new InvalidOperationException($"Dependencies not met for step {step.GetType().Name}");
            }

            if (!step.CanExecute(_currentState))
            {
                continue;
            }

            try
            {
                await step.ExecuteAsync(state, cts.Token);
                SagaState newState = step.NextState(_currentState, true);
                await ChangeState(sagaId, newState, step);

                OnStepCompleted?.Invoke(step, state); // Fire the event

                await step.PublishEventAsync(_currentState, state);
            }
            catch (Exception ex)
            {
                SagaState newState = step.NextState(_currentState, false);
                await ChangeState(sagaId, newState, step);

                OnStepFailed?.Invoke(step, state, ex); // Fire the event

                await RollbackAsync(state, cts.Token);
                return false;
            }
        }

        return _currentState == SagaState.Completed;
    }

    /// <summary>
    /// Asynchronously rolls back all the completed saga steps in reverse order.
    /// </summary>
    /// <param name="state">The current state of the saga.</param>
    /// <param name="cancellationToken">Cancellation token for the task.</param>
    /// <returns>A Task representing the asynchronous rollback operation.</returns>
    private async Task RollbackAsync(TState state, CancellationToken cancellationToken)
    {
        foreach (var step in _steps.Reverse<ISagaStep<TState>>())
        {
            if (_log.Any(log => log.Step == step && log.State == SagaState.Completed))
            {
                await step.RollbackAsync(state, cancellationToken);
                _log.Add(new SagaLogEntry<TState>(step, SagaState.Failed));
            }
        }
    }
}
