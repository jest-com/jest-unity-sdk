using UnityEngine;

namespace com.jest.sdk
{
    /// <summary>
    /// Manages player-related data and operations in the Jest SDK.
    /// </summary>
    public sealed class Player
    {
        /// <summary>
        /// Gets the unique identifier for the current player.
        /// </summary>
        public string id => JsBridge.GetPlayerId();

        /// <summary>
        /// Gets whether the current player is registered in the system.
        /// </summary>
        public bool isRegistered
        {
            get
            {
                var isReg = JsBridge.GetIsRegistered();
                if (bool.TryParse(isReg, out bool result))
                {
                    return result;
                }
                else
                {
                    Debug.LogError($"Could not parse JS_getIsRegistered. Value: {isReg}");
                    return false;
                }
            }
        }

        internal Player() { }

        /// <summary>
        /// Retrieves a string value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <returns>The value associated with the key, or an empty string if the key is missing</returns>
        /// <exception cref="System.ArgumentException">Thrown when key is null or empty.</exception>
        public string Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new System.ArgumentException("Key cannot be null or empty", nameof(key));
            }
            return JsBridge.GetPlayerValue(key);
        }


        /// <summary>
        /// Retrieves a string value associated with the specified player.
        /// </summary>
        /// <returns>Player data as string</returns>
        public string GetPlayerData()
        {
            return JsBridge.GetPlayerData();
        }


        /// <summary>
        /// Attempts to retrieve a string value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <param name="value">When this method returns, contains the value associated with the key, if found; otherwise, the default value</param>
        /// <returns>true if the key was found; otherwise, false</returns>
        /// <exception cref="System.ArgumentException">Thrown when key is null or empty.</exception>
        public bool TryGet(string key, out string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new System.ArgumentException("Key cannot be null or empty", nameof(key));
            }
            value = JsBridge.GetPlayerValue(key);
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Attempts to retrieve and convert a value of type T associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to</typeparam>
        /// <param name="key">The key to look up</param>
        /// <param name="value">When this method returns, contains the converted value associated with the key, if found; otherwise, the default value</param>
        /// <returns>true if the key was found and the value was successfully converted; otherwise, false</returns>
        public bool TryGet<T>(string key, out T value)
        {
            value = default;
            if (TryGet(key, out string val))
            {
                value = Convert.FromString<T>(val);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves and converts a value of type T associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to</typeparam>
        /// <param name="key">The key to look up</param>
        /// <returns>The converted value associated with the key</returns>
        /// <exception cref="System.ArgumentException">Thrown when key is null or empty.</exception>
        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new System.ArgumentException("Key cannot be null or empty", nameof(key));
            }
            var str = JsBridge.GetPlayerValue(key);
            return Convert.FromString<T>(str);
        }

        /// <summary>
        /// Sets a value of type T for the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to set</typeparam>
        /// <param name="key">The key to associate the value with</param>
        /// <param name="value">The value to set</param>
        /// <exception cref="System.ArgumentException">Thrown when key is null or empty.</exception>
        public void Set<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new System.ArgumentException("Key cannot be null or empty", nameof(key));
            }
            JsBridge.SetPlayerValue(key, Convert.ToString(value));
        }

        /// <summary>
        /// Sets a string value for the specified key.
        /// </summary>
        /// <param name="key">The key to associate the value with</param>
        /// <param name="value">The string value to set</param>
        /// <exception cref="System.ArgumentException">Thrown when key is null or empty.</exception>
        public void Set(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new System.ArgumentException("Key cannot be null or empty", nameof(key));
            }
            JsBridge.SetPlayerValue(key, value);
        }
    }
}
