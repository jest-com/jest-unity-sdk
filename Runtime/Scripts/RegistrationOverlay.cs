using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.jest.sdk
{
    /// <summary>
    /// Presents the platform registration overlay while the game renders its own UI
    /// for the login and close buttons. Call <see cref="Show"/> and wire the
    /// returned handle's <see cref="Handle.LoginButtonAction"/> and
    /// <see cref="Handle.CloseButtonAction"/> to those buttons.
    /// </summary>
    public class RegistrationOverlay
    {
        internal RegistrationOverlay() { }

        /// <summary>
        /// Displays the registration overlay and returns a handle used to drive it.
        /// </summary>
        /// <param name="options">Optional overlay configuration.</param>
        /// <returns>A <see cref="Handle"/> exposing button actions and an <see cref="Handle.OnClose"/> event.</returns>
        public Handle Show(Options options = null)
        {
            var conversationId = Guid.NewGuid().ToString();
            var payload = new Dictionary<string, object>
            {
                ["conversationId"] = conversationId,
                ["theme"] = options?.Theme == Theme.Light ? "light" : "dark",
            };

            if (options?.EntryPayload != null && options.EntryPayload.Count > 0)
            {
                payload["entryPayload"] = options.EntryPayload;
            }

            var handle = new Handle(conversationId);
            var beginTask = JsBridge.BeginPlatformRegistrationOverlay(JsonConvert.SerializeObject(payload));

            beginTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    handle.InvokeError(t.Exception);
                    return;
                }
                handle.InvokeClose();
            });

            return handle;
        }

        /// <summary>
        /// Theme for the registration overlay.
        /// </summary>
        public enum Theme
        {
            /// <summary>Dark theme (default).</summary>
            Dark,
            /// <summary>Light theme.</summary>
            Light,
        }

        /// <summary>
        /// Configuration options for <see cref="Show"/>.
        /// </summary>
        public class Options
        {
            /// <summary>Overlay theme. Defaults to <see cref="Theme.Dark"/>.</summary>
            public Theme Theme = Theme.Dark;

            /// <summary>
            /// Optional payload accessible via <see cref="JestSDK.GetEntryPayload"/> after registration.
            /// </summary>
            public Dictionary<string, object> EntryPayload;
        }

        /// <summary>
        /// Handle used to control a live registration overlay.
        /// </summary>
        public sealed class Handle
        {
            private readonly string _conversationId;
            private bool _closed;

            /// <summary>
            /// Fires when the overlay has been closed (either via the user or <see cref="CloseButtonAction"/>).
            /// </summary>
            public event Action OnClose;

            /// <summary>
            /// Fires if the bridge reports an error while running the overlay.
            /// </summary>
            public event Action<Exception> OnError;

            internal Handle(string conversationId)
            {
                _conversationId = conversationId;
            }

            /// <summary>
            /// Signals that the user tapped the game-rendered login button.
            /// </summary>
            public void LoginButtonAction()
            {
                JsBridge.PlatformRegistrationOverlayLogin(_conversationId);
            }

            /// <summary>
            /// Signals that the user tapped the game-rendered close button.
            /// </summary>
            public void CloseButtonAction()
            {
                JsBridge.DismissPlatformRegistrationOverlay();
            }

            internal void InvokeClose()
            {
                if (_closed) return;
                _closed = true;
                OnClose?.Invoke();
            }

            internal void InvokeError(Exception e)
            {
                OnError?.Invoke(e);
            }
        }
    }
}
