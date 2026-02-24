using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides navigation functionality for redirecting players between games.
    /// </summary>
    public class Navigation
    {
        /// <summary>
        /// Internal constructor to prevent external instantiation.
        /// </summary>
        internal Navigation() { }

        /// <summary>
        /// Redirects the player from an onboarding game to the linked flagship game.
        /// If the current game is not configured as an onboarding game, nothing happens.
        /// </summary>
        /// <param name="options">Optional navigation options including entry payload.</param>
        public void RedirectToFlagshipGame(RedirectToFlagshipGameOptions options = null)
        {
            var message = new RedirectMessage
            {
                redirectToFlagship = true,
                entryPayload = options?.entryPayload != null ? Convert.ToString(options.entryPayload) : null,
                skipGameExitConfirm = false // Always show exit dialog for flagship redirect
            };

            JsBridge.RedirectToGame(JsonConvert.SerializeObject(message));
        }

        /// <summary>
        /// Redirects the player to a specific game by its slug.
        /// </summary>
        /// <param name="options">Navigation options including the target game slug.</param>
        /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
        /// <exception cref="ArgumentException">Thrown when gameSlug is null or empty.</exception>
        public void RedirectToGame(RedirectToGameOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.gameSlug))
            {
                throw new ArgumentException("gameSlug cannot be null or empty", nameof(options.gameSlug));
            }

            var message = new RedirectMessage
            {
                gameSlug = options.gameSlug,
                entryPayload = options.entryPayload != null ? Convert.ToString(options.entryPayload) : null,
                skipGameExitConfirm = options.skipGameExitConfirm
            };

            JsBridge.RedirectToGame(JsonConvert.SerializeObject(message));
        }

        /// <summary>
        /// Redirects the player to the explore page of the Jest platform.
        /// </summary>
        public void RedirectToExplorePage()
        {
            JsBridge.RedirectToExplorePage();
        }

        /// <summary>
        /// Options for redirecting to the flagship game.
        /// </summary>
        [Serializable]
        public class RedirectToFlagshipGameOptions
        {
            /// <summary>
            /// Optional payload data to pass to the flagship game.
            /// This will be accessible via GetEntryPayload() in the target game.
            /// </summary>
            public Dictionary<string, object> entryPayload;
        }

        /// <summary>
        /// Options for redirecting to a specific game.
        /// </summary>
        [Serializable]
        public class RedirectToGameOptions
        {
            /// <summary>
            /// The slug identifier of the target game. Required.
            /// </summary>
            public string gameSlug;

            /// <summary>
            /// Optional payload data to pass to the target game.
            /// This will be accessible via GetEntryPayload() in the target game.
            /// </summary>
            public Dictionary<string, object> entryPayload;

            /// <summary>
            /// If true, skips the game exit confirmation dialog. Defaults to false.
            /// </summary>
            public bool skipGameExitConfirm;
        }

        /// <summary>
        /// Internal message structure for the redirect bridge call.
        /// </summary>
        [Serializable]
        private class RedirectMessage
        {
            public bool? redirectToFlagship;
            public string gameSlug;
            public string entryPayload;
            public bool skipGameExitConfirm;
        }
    }
}
