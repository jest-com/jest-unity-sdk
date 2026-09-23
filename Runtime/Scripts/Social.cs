using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides social/profile helpers.
    /// </summary>
    public class Social
    {
        private const string CloudflareImageProxy = "https://cdn.jestpub.com/cdn-cgi/image/";
        private const string BotAvatarBaseUrl = "https://cdn.jest.com/avatars/bot/";
        private const int AvailableAvatars = 1000;

        internal Social() { }

        /// <summary>
        /// Returns a CDN URL for a bot avatar, deterministically seeded by <paramref name="username"/>.
        /// Use the smallest <paramref name="size"/> that fits your UI for best performance.
        /// </summary>
        /// <param name="username">Used as a seed; the same username always returns the same avatar.</param>
        /// <param name="size">Supported sizes are 64, 128, 256, 512, and 1000 (default). Other values are bucketed down to the next supported size.</param>
        /// <returns>A CDN URL for the bot avatar image.</returns>
        public string GetBotAvatar(string username, int size = 1000)
        {
            return JsBridge.GetBotAvatar(username ?? "", size);
        }

        /// <summary>
        /// Returns the current player's profile (username and sized avatar URL), or <c>null</c>
        /// when the SDK is not initialized or the player has no profile.
        /// </summary>
        /// <param name="avatarSize">Supported sizes are 64, 128, 256, 512, and 1000 (default). Other values are bucketed down to the next supported size.</param>
        /// <returns>A <see cref="PlayerProfile"/> with <see cref="PlayerProfile.AvatarUrl"/> resized via the Cloudflare image proxy, or <c>null</c>.</returns>
        public PlayerProfile GetProfile(int avatarSize = 1000)
        {
            string json = JsBridge.GetProfile(avatarSize);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<PlayerProfile>(json);
        }

        /// <summary>
        /// Returns the current player's avatar URL, sized for Unity texture loading when possible.
        /// </summary>
        /// <param name="size">Supported sizes are 64, 128, 256, 512, and 1000 (default). Other values are bucketed down to the next supported size.</param>
        /// <returns>A CDN URL for the player avatar image, or null when no avatar is available.</returns>
        [Obsolete("Use GetProfile(avatarSize).AvatarUrl instead.")]
        public string GetPlayerAvatar(int size = 1000)
        {
            return JsBridge.GetPlayerAvatar(size);
        }

        /// <summary>
        /// Opens the platform's share sheet for an image — the same sheet the platform's own
        /// screenshot button shows, offering chat, the native share sheet and download. The
        /// player picks where it goes and writes the caption, so this never shares on the
        /// player's behalf without a tap.
        /// </summary>
        /// <param name="image">Base64 PNG (raw or data URL) to share. Omit it to have the platform
        /// capture the canvas, or your registered screenshot provider.</param>
        /// <param name="entryPayload">Handed back to your game when a player opens the shared
        /// message, so a code or coupon travels with it.</param>
        /// <returns>A task resolving to a <see cref="ShareImageResponse"/>. <c>Canceled</c> is
        /// <c>true</c> when the sheet closed without sharing, including a player who left the page
        /// while the post was still landing, so it is not proof that nothing was posted.</returns>
        public JestSDKTask<ShareImageResponse> ShareImage(string image = null, Dictionary<string, object> entryPayload = null)
        {
            var jsonObj = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(image))
            {
                jsonObj["image"] = image;
            }

            if (entryPayload != null && entryPayload.Count > 0)
            {
                jsonObj["entryPayload"] = entryPayload;
            }

            string optionsJson = JsonConvert.SerializeObject(jsonObj);

            var task = new JestSDKTask<ShareImageResponse>();
            var shareImageTask = JsBridge.ShareImage(optionsJson);

            shareImageTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }

                    string json = t.GetResult();
                    var response = JsonConvert.DeserializeObject<ShareImageResponse>(json);
                    task.SetResult(response);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });

            return task;
        }

        internal static string GetBotAvatarFallback(string username, int size)
        {
            int bucketed = BucketAvatarSize(size);
            double rand = SeedRandomFirst(username ?? "");
            int index = (int)Math.Floor(rand * AvailableAvatars);
            string botUrl = $"{BotAvatarBaseUrl}{index}.webp";
            return BuildCloudflareImageUrl(botUrl, bucketed);
        }

        internal static string GetPlayerAvatarFallback(string avatarUrl, int size)
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                return null;
            }

            if (avatarUrl.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
                avatarUrl.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase))
            {
                return avatarUrl;
            }

            return BuildCloudflareImageUrl(avatarUrl, BucketAvatarSize(size));
        }

        internal static string GetProfileFallback(string username, string avatarUrl, int size)
        {
            string sizedAvatar = GetPlayerAvatarFallback(avatarUrl, size);
            var profile = new PlayerProfile
            {
                Username = username ?? "",
                AvatarUrl = sizedAvatar,
            };
            return JsonConvert.SerializeObject(profile);
        }

        private static int BucketAvatarSize(int size)
        {
            if (size >= 1000) return 1000;
            if (size >= 512) return 512;
            if (size >= 256) return 256;
            if (size >= 128) return 128;
            return 64;
        }

        // Bit-compatible with @textclub/common/prng's first seeded random value.
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
            return $"{CloudflareImageProxy}format=auto%2Cfit=cover%2Cwidth={width}%2C/{Uri.EscapeDataString(imageUrl)}";
        }

        /// <summary>
        /// The current player's profile (username and avatar URL).
        /// </summary>
        [Serializable]
        public class PlayerProfile
        {
            /// <summary>
            /// The player's display username.
            /// </summary>
            [JsonProperty("username")]
            public string Username;

            /// <summary>
            /// The player's avatar URL, or null when no avatar is available.
            /// </summary>
            [JsonProperty("avatarUrl")]
            public string AvatarUrl;
        }

        /// <summary>
        /// Result of a <see cref="ShareImage"/> call.
        /// </summary>
        [Serializable]
        public class ShareImageResponse
        {
            /// <summary>
            /// True when the share sheet closed without sharing. Not proof that nothing was
            /// posted — a player who left the page while the post was still landing also reports
            /// <c>true</c>.
            /// </summary>
            [JsonProperty("canceled")]
            public bool Canceled;
        }
    }
}
