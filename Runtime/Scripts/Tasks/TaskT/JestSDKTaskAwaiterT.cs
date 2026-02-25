using System;
using System.Runtime.CompilerServices;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides an awaiter for JestSDKTask<T> that implements the async/await pattern.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the associated task</typeparam>
    public readonly struct JestSDKTaskAwaiter<T> : INotifyCompletion
    {
        /// <summary>
        /// Gets whether the task has completed.
        /// </summary>
        public bool IsCompleted => Task.IsCompleted;

        /// <summary>
        /// The underlying task being awaited.
        /// </summary>
        private JestSDKTask<T> Task { get; }

        /// <summary>
        /// Initializes a new instance of JestSDKTaskAwaiter<T>;.
        /// </summary>
        /// <param name="task">The task to be awaited</param>
        public JestSDKTaskAwaiter(JestSDKTask<T> task) => Task = task;

        /// <summary>
        /// Gets the result of the task.
        /// </summary>
        /// <returns>The result value of type T from the completed task</returns>
        public T GetResult() => Task.GetResult();

        /// <summary>
        /// Schedules the continuation action to be invoked when the task completes.
        /// </summary>
        /// <param name="continuation">The action to invoke when the task completes</param>
        public void OnCompleted(Action continuation) => Task.OnCompleted(continuation);
    }
}
