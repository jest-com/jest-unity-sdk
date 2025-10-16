using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace com.jest.sdk
{
    /// <summary>
    /// Represents a custom task implementation for asynchronous operations in Jest SDK.
    /// Similar to System.Threading.Tasks.Task but optimized for Unity context.
    /// </summary>
    [AsyncMethodBuilder(typeof(JestSDKTaskBuilder))]
    public class JestSDKTask
    {
        /// <summary>
        /// Gets the exception that caused the task to fail, if any.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets whether the task has failed due to an exception.
        /// </summary>
        public bool IsFaulted { get => Exception != null; }

        /// <summary>
        /// Gets whether the task has completed execution.
        /// </summary>
        public bool IsCompleted { get; private set; }

        private JestSDKTaskAwaiter Awaiter { get; }

        private event Action Finished;

        /// <summary>
        /// Gets an awaiter used to await this task.
        /// </summary>
        /// <returns>A task awaiter</returns>
        public JestSDKTaskAwaiter GetAwaiter() => Awaiter;

        /// <summary>
        /// Initializes a new instance of JestSDKTask.
        /// </summary>
        public JestSDKTask() => Awaiter = new JestSDKTaskAwaiter(this);

        /// <summary>
        /// Initializes a new instance of JestSDKTask with a native action.
        /// </summary>
        /// <param name="action">The native action to execute</param>
        internal JestSDKTask(Action<IntPtr> action)
        {
            Awaiter = new JestSDKTaskAwaiter(this);
            GCHandle handle = GCHandle.Alloc(this);
            IntPtr taskPtr = GCHandle.ToIntPtr(handle);
            action(taskPtr);
        }

        /// <summary>
        /// Waits for the task to complete using a Unity coroutine.
        /// </summary>
        /// <returns>An IEnumerator that can be used in a Unity coroutine.</returns>
        public IEnumerator WaitForCompletion()
        {
            while (!IsCompleted)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Creates a continuation task that executes when this task completes.
        /// </summary>
        /// <param name="action">The action to execute when this task completes.</param>
        /// <returns>A new task that will complete after the continuation is executed.</returns>
        public JestSDKTask ContinueWith(Action<JestSDKTask> action)
        {
            var continuationTask = new JestSDKTask();
            OnCompleted(() =>
            {
                action(this);
                continuationTask.SetResult();
            });
            return continuationTask;
        }

        /// <summary>
        /// Gets the result of the task. Throws an exception if the task faulted.
        /// </summary>
        public void GetResult()
        {
            if (!IsCompleted)
            {
                throw new InvalidOperationException("Task is not completed");
            }

            if (IsFaulted)
            {
                throw this.Exception;
            }
        }

        /// <summary>
        /// Sets the task's state to completed successfully.
        /// </summary>
        public void SetResult()
        {
            if (IsCompleted)
            {
                throw new InvalidOperationException("SetResult failed. Task is already completed.");
            }

            IsCompleted = true;
            Finished?.Invoke();
        }

        /// <summary>
        /// Sets the task's state to completed due to an exception.
        /// </summary>
        /// <param name="exception">The exception that caused the task to fault</param>
        public void SetException(Exception e)
        {
            if (IsCompleted)
            {
                throw new InvalidOperationException("SetException failed. Task is already completed.", e);
            }

            IsCompleted = true;
            Exception = e;
            Finished?.Invoke();
        }

        /// <summary>
        /// Registers a callback to be invoked when the task completes.
        /// </summary>
        /// <param name="continuation">The action to invoke upon completion</param>
        internal void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation();
            }
            else
            {
                Finished += continuation;
            }
        }
    }
}
