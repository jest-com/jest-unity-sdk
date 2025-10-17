using System.Collections.Generic;
using UnityEngine;

namespace com.jest.sdk
{
    /// <summary>
    /// Main entry point for the Jest SDK, implemented as a Singleton.
    /// </summary>
    public sealed class JestSDK
    {
        /// <summary>
        /// Global access point to the JestSDK instance.
        /// </summary>
        public static readonly JestSDK Instance = new JestSDK();

        // Private constructor prevents external instantiation.
        private JestSDK() { }

        /// <summary>
        /// Provides access to notification management functionality.
        /// </summary>
        public readonly Notifications notifications = new();

        /// <summary>
        /// Provides access to notification management functionality.
        /// </summary>
        public readonly RichNotifications richNotifications = new();

        /// <summary>
        /// Provides access to analytics tracking and reporting.
        /// </summary>
        public readonly Analytics analytics = new();

        /// <summary>
        /// Provides access to player-related functionality and data.
        /// </summary>
        public readonly Player player = new();

        /// <summary>
        /// Initializes the Jest SDK and ensures it's ready for use.
        /// </summary>
        /// <returns>A task that completes when the SDK is ready</returns>
        public JestSDKTask Init()
        {
            return JsBridge.CallAsyncVoid("isReady");
        }

        /// <summary>
        /// Retrieves the entry payload data associated with the current session entry.
        /// </summary>
        /// <returns>A dictionary containing the entry payload data</returns>
        public Dictionary<string, object> GetEntryPayload()
        {
            string payloadString = JsBridge.GetEntryPayload();
            if (string.IsNullOrEmpty(payloadString))
            {
                return new Dictionary<string, object>();
            }

            return Convert.FromString<Dictionary<string, object>>(payloadString);
        }

        /// <summary>
        /// Logs in the user with the provided payload data.
        /// </summary>
        public void Login(Dictionary<string, object> payload)
        {
            string entryPayloadString = Convert.ToString(payload);
            JsBridge.Login(entryPayloadString);
        }
    }
}
