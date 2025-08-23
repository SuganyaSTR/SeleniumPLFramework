using NUnit.Framework;

// Global NUnit parallel execution configuration
// Parallel execution disabled for sequential test execution

// Set the level of parallelism to 1 for sequential execution
[assembly: LevelOfParallelism(1)]

// Disable parallel execution for sequential testing
[assembly: Parallelizable(ParallelScope.None)]
