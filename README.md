# SagaFlow: A C# Saga Pattern Library

## Introduction

SagaFlow is a C# library designed to make it easier to implement the Saga pattern, widely used for managing complex, long-running transactions in distributed systems. The library helps in maintaining transaction state, automating rollback in case of failure, and is built with extensibility in mind.

## Features

- Built-in state management
- Support for asynchronous execution of Saga steps
- Event-based logging and state transition
- In-memory state repository
- Highly extensible, supporting custom state repositories

## Usage Example

Below is a simple example of how to use SagaFlow:

```csharp
using SagaFlow;

// Define your state
public class MyState
{
    public int Value { get; set; }
}

// Initialize Saga Coordinator
var repo = new InMemorySagaStateRepository<MyState>();
var steps = new List<ISagaStep<MyState>> { /* Add your steps here */ };
var saga = new SagaCoordinator<MyState>(repo, steps);

// Execute Saga
var state = new MyState();
bool success = await saga.ExecuteAsync(Guid.NewGuid(), state, TimeSpan.FromMinutes(5));

// Check if Saga is completed successfully
if (success) 
{
    // Transaction is complete
}
```

## Events

SagaFlow provides two key events to track the transaction:

1. `OnStepCompleted`: Triggered when a step is successfully completed.
2. `OnStepFailed`: Triggered when a step fails during execution.

