using System;
using System.Runtime.CompilerServices;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides an awaiter for JestSDKTask that implements the async/await pattern.
    /// </summary>
    public readonly struct JestSDKTaskAwaiter : INotifyCompletion
    {
        /// <summary>
        /// Gets whether the task has completed.
        /// </summary>
        public bool IsCompleted => Task.IsCompleted;

        /// <summary>
        /// The underlying task being awaited.
        /// </summary>
        private JestSDKTask Task { get; }

        /// <summary>
        /// Initializes a new instance of JestSDKTaskAwaiter.
        /// </summary>
        /// <param name="task">The task to be awaited</param>
        public JestSDKTaskAwaiter(JestSDKTask task) => Task = task;

        /// <summary>
        /// Gets the result of the task.
        /// </summary>
        public void GetResult() => Task.GetResult();

        /// <summary>
        /// Schedules the continuation action to be invoked when the task completes.
        /// </summary>
        /// <param name="continuation">The action to invoke when the task completes</param>
        public void OnCompleted(Action continuation) => Task.OnCompleted(continuation);
    }
}
