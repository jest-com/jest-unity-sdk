using System;
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
        /// Provides access to the platform registration overlay flow.
        /// </summary>
        public readonly RegistrationOverlay RegistrationOverlay = new();

        /// <summary>
        /// Initializes the Jest SDK and ensures it's ready for use.
        /// </summary>
        /// <returns>A task that completes when the SDK is ready</returns>
        public JestSDKTask Init()
        {
            return JsBridge.Init();
        }

        /// <summary>
        /// Initializes the Jest SDK with the specified options.
        /// </summary>
        /// <param name="options">Configuration options for SDK initialization.</param>
        /// <returns>A task that completes when the SDK is ready</returns>
        public JestSDKTask Init(InitOptions options)
        {
            return JsBridge.Init(options?.AutoLoginReminders ?? true);
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
        /// Shows the platform registration overlay and returns actions that game UI can wire to login and close buttons.
        /// </summary>
        /// <param name="options">Optional overlay configuration.</param>
        /// <returns>A handle exposing the login and close actions for this overlay.</returns>
        public RegistrationOverlay.Handle ShowRegistrationOverlay(RegistrationOverlay.Options options = null)
        {
            return RegistrationOverlay.Show(options);
        }

        /// <summary>
        /// Opens the privacy policy page.
        /// </summary>
        /// <remarks>Internal use — not part of the supported public API.</remarks>
        public void OpenPrivacyPolicy()
        {
            JsBridge.OpenLegalPage("privacy");
        }

        /// <summary>
        /// Opens the terms of service page.
        /// </summary>
        /// <remarks>Internal use — not part of the supported public API.</remarks>
        public void OpenTermsOfService()
        {
            JsBridge.OpenLegalPage("terms");
        }

        /// <summary>
        /// Opens the copyright page.
        /// </summary>
        /// <remarks>Internal use — not part of the supported public API.</remarks>
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

        /// <summary>
        /// Reports loading progress to the platform loading screen overlay.
        /// Only works when the game's loading screen mode is set to "manual" in the management console.
        /// The overlay is shown automatically when the game loads in manual mode.
        /// </summary>
        /// <param name="progress">Loading progress from 0 to 100. Setting progress to 100 dismisses the overlay.</param>
        public void SetLoadingProgress(float progress)
        {
            JsBridge.SetLoadingProgress(progress);
        }

        private const string CLOUDFLARE_IMAGE_PROXY = "https://cdn.jestpub.com/cdn-cgi/image/";
        private const string BOT_AVATAR_BASE_URL = "https://cdn.jest.com/avatar/bot/";
        private const int AVAILABLE_AVATARS = 1000;

        /// <summary>
        /// Returns a CDN URL for a bot avatar, deterministically seeded by <paramref name="username"/>.
        /// Use the smallest <paramref name="size"/> that fits your UI for best performance.
        /// </summary>
        /// <param name="username">Used as a seed; the same username always returns the same avatar.</param>
        /// <param name="size">Supported sizes are 64, 128, 256, 512, and 1000 (default). Other values are bucketed down to the next supported size.</param>
        /// <returns>A CDN URL for the bot avatar image.</returns>
        public string GetBotAvatar(string username, int size = 1000)
        {
            int bucketed = BucketBotAvatarSize(size);
            double rand = SeedRandomFirst(username ?? "");
            int index = (int)Math.Floor(rand * AVAILABLE_AVATARS);
            string botUrl = $"{BOT_AVATAR_BASE_URL}{index}.webp";
            return bucketed >= AVAILABLE_AVATARS
                ? botUrl
                : BuildCloudflareImageUrl(botUrl, bucketed);
        }

        private static int BucketBotAvatarSize(int size)
        {
            if (size >= 1000) return 1000;
            if (size >= 512) return 512;
            if (size >= 256) return 256;
            if (size >= 128) return 128;
            return 64;
        }

        // Computes the first sfc32 PRNG output seeded with cyrb128(seed), in [0, 1).
        // Bit-compatible with the JS implementation in @textclub/common/prng so all
        // SDKs return the same avatar for the same username. C# strings are UTF-16,
        // so iterating chars matches JS String.charCodeAt directly.
        private static double SeedRandomFirst(string seed)
        {
            unchecked
            {
                uint h1 = 1779033703u, h2 = 3144134277u, h3 = 1013904242u, h4 = 2773480762u;
                foreach (char c in seed)
                {
                    uint k = c;
                    h1 = h2 ^ ((h1 ^ k) * 597399067u);
                    h2 = h3 ^ ((h2 ^ k) * 2869860233u);
                    h3 = h4 ^ ((h3 ^ k) * 951274213u);
                    h4 = h1 ^ ((h4 ^ k) * 2716044179u);
                }
                h1 = (h3 ^ (h1 >> 18)) * 597399067u;
                h2 = (h4 ^ (h2 >> 22)) * 2869860233u;
                h3 = (h1 ^ (h3 >> 17)) * 951274213u;
                h4 = (h2 ^ (h4 >> 19)) * 2716044179u;
                h1 ^= h2 ^ h3 ^ h4;
                h2 ^= h1; h3 ^= h1; h4 ^= h1;
                uint t = h1 + h2 + h4;
                return (double)t / 4294967296.0;
            }
        }

        private static string BuildCloudflareImageUrl(string imageUrl, int width)
        {
            return $"{CLOUDFLARE_IMAGE_PROXY}format=auto%2Cfit=cover%2Cwidth={width}%2C/{Uri.EscapeDataString(imageUrl)}";
        }
    }

    /// <summary>
    /// Configuration options for Jest SDK initialization.
    /// </summary>
    public class InitOptions
    {
        /// <summary>
        /// Whether to show automatic login reminder popups for unregistered users.
        /// When set to false, disables the platform's automatic login reminders.
        /// Manual login via <see cref="JestSDK.Login"/> is unaffected.
        /// Defaults to true.
        /// </summary>
        public bool AutoLoginReminders = true;
    }
}
