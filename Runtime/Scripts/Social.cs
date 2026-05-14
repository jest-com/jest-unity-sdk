using System;

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
        /// Returns the current player's avatar URL, sized for Unity texture loading when possible.
        /// </summary>
        /// <param name="size">Supported sizes are 64, 128, 256, 512, and 1000 (default). Other values are bucketed down to the next supported size.</param>
        /// <returns>A CDN URL for the player avatar image, or null when no avatar is available.</returns>
        public string GetPlayerAvatar(int size = 1000)
        {
            return JsBridge.GetPlayerAvatar(size);
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
    }
}
