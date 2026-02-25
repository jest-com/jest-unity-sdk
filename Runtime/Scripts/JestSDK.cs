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
        /// Provides access to rich notification management functionality.
        /// </summary>
        public readonly RichNotifications RichNotifications = new();

        /// <summary>
        /// Provides access to analytics tracking and reporting.
        /// </summary>
        public readonly Analytics Analytics = new();

        /// <summary>
        /// Provides access to player-related functionality and data.
        /// </summary>
        public readonly Player Player = new();

        /// <summary>
        /// Provides access to purchase related functionality and data.
        /// </summary>
        public readonly Payment Payment = new();

        /// <summary>
        /// Provides access to referral functionality and statistics.
        /// </summary>
        public readonly Referrals Referrals = new();

        /// <summary>
        /// Provides access to navigation functionality for redirecting between games.
        /// </summary>
        public readonly Navigation Navigation = new();

        /// <summary>
        /// Provides access to internal/experimental SDK functionality.
        /// These methods may change without notice.
        /// </summary>
        public readonly Internal Internal = new();

        /// <summary>
        /// Initializes the Jest SDK and ensures it's ready for use.
        /// </summary>
        /// <returns>A task that completes when the SDK is ready</returns>
        public JestSDKTask Init()
        {
            return JsBridge.Init();
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
        /// Logs in the user with the optional payload data.
        /// </summary>
        /// <param name="payload">Optional entry payload data to pass during login.</param>
        /// <exception cref="System.InvalidOperationException">Thrown when the player is already logged in.</exception>
        public void Login(Dictionary<string, object> payload = null)
        {
            if (Player.isRegistered)
            {
                throw new System.InvalidOperationException("Player is already logged in");
            }

            string entryPayloadString = payload != null ? Convert.ToString(payload) : null;
            JsBridge.Login(entryPayloadString);
        }

        /// <summary>
        /// Opens the privacy policy page.
        /// </summary>
        public void OpenPrivacyPolicy()
        {
            JsBridge.OpenLegalPage("privacy");
        }

        /// <summary>
        /// Opens the terms of service page.
        /// </summary>
        public void OpenTermsOfService()
        {
            JsBridge.OpenLegalPage("terms");
        }

        /// <summary>
        /// Opens the copyright page.
        /// </summary>
        public void OpenCopyright()
        {
            JsBridge.OpenLegalPage("copyright");
        }

        /// <summary>
        /// Triggers debug registration flow. In WebGL, this posts a message to the parent window
        /// to bypass the normal login flow for testing purposes.
        /// </summary>
        public void DebugRegister()
        {
            JsBridge.DebugRegister();
        }

        /// <summary>
        /// Gets the value of a feature flag by key.
        /// </summary>
        /// <param name="key">The feature flag key to look up.</param>
        /// <returns>A task resolving to the flag value, or empty string if not found.</returns>
        public JestSDKTask<string> GetFeatureFlag(string key)
        {
            return JsBridge.GetFeatureFlag(key);
        }
    }
}
