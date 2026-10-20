using System;

namespace com.jest.sdk
{
    /// <summary>
    /// Which kind of haptics <see cref="Haptics.Vibrate(int)"/> produces on this device.
    /// </summary>
    public enum HapticsCapability
    {
        /// <summary>
        /// No haptics.
        /// </summary>
        None,

        /// <summary>
        /// The browser has <c>navigator.vibrate</c> (Android, and desktop Chromium).
        /// </summary>
        Vibrate,

        /// <summary>
        /// iPhone on iOS 18 or later; a tap that asked for a vibration ticks on release.
        /// </summary>
        Tap
    }

    /// <summary>
    /// Provides device vibration. Beta.
    /// </summary>
    public class Haptics
    {
        internal Haptics() { }

        /// <summary>
        /// Same as <c>navigator.vibrate()</c>, but also works on iPhone, where it plays one native
        /// tick when the current tap on the game canvas is released. On iPhone only a call made
        /// while the finger is down counts, so call it from the frame that sees the touch begin
        /// (<see cref="UnityEngine.TouchPhase.Began"/>), and the first call installs a layer, so
        /// its tap does not tick. A duration of 0 cancels an ongoing vibration.
        /// </summary>
        /// <param name="durationMs">Vibration duration in milliseconds.</param>
        /// <returns>Whether the request was accepted; on iPhone, whether the current tap ticks.</returns>
        public bool Vibrate(int durationMs)
        {
            return Vibrate(new[] { durationMs });
        }

        /// <summary>
        /// Vibrates with a pattern of alternating vibration and pause durations in milliseconds.
        /// iPhone ignores the pattern and plays a single tick; see <see cref="Vibrate(int)"/>.
        /// </summary>
        /// <param name="pattern">Alternating vibration and pause durations in milliseconds.</param>
        /// <returns>Whether the request was accepted; on iPhone, whether the current tap ticks.</returns>
        /// <exception cref="ArgumentNullException">Thrown when pattern is null.</exception>
        public bool Vibrate(int[] pattern)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            return JsBridge.Vibrate(Convert.ToString(pattern));
        }

        /// <summary>
        /// Reports which kind of haptics <see cref="Vibrate(int)"/> produces on this device.
        /// </summary>
        public HapticsCapability GetCapability()
        {
            switch (JsBridge.GetHapticsCapability())
            {
                case "vibrate":
                    return HapticsCapability.Vibrate;
                case "tap":
                    return HapticsCapability.Tap;
                default:
                    return HapticsCapability.None;
            }
        }
    }
}
