using System;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides app visibility and platform exit events.
    /// </summary>
    public class Lifecycle
    {
        internal Lifecycle()
        {
            JsBridge.LifecycleHide += () => OnHide?.Invoke();
            JsBridge.LifecycleShow += () => OnShow?.Invoke();
            JsBridge.LifecycleExitRequested += () => OnExitRequested?.Invoke();
        }

        /// <summary>
        /// Fires when the game document changes from visible to hidden.
        /// </summary>
        public event Action OnHide;

        /// <summary>
        /// Fires when the game document changes from hidden to visible.
        /// </summary>
        public event Action OnShow;

        /// <summary>
        /// Fires when the platform begins an exit flow for the game.
        /// </summary>
        public event Action OnExitRequested;
    }
}
