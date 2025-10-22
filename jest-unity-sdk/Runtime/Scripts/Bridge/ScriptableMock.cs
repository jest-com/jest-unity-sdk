using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.jest.sdk
{
    /// <summary>
    /// A scriptable object that implements IBridgeMock for testing and debugging purposes.
    /// Allows mocking of player data, events, and notifications in the Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "JestSDKMock", menuName = "JestSDK/Mock")]
    public class ScriptableMock : ScriptableObject, IBridgeMock
    {
        [SerializeField] private string _playerId;
        [SerializeField] private bool _isRegistered;
        [SerializeField] private List<Notifications.Options> _notifications;
        [SerializeField] private List<RichNotifications.Options> _notificationsV2;
        [SerializeField] private List<ValuePair> _values;
        [SerializeField] private List<ValuePair> _events;
        [SerializeField] private List<ValuePair> _entryPayload;

        /// <summary>
        /// Gets the unique identifier for the player.
        /// </summary>
        public string playerId => _playerId;

        /// <summary>
        /// Returns all serialized player data as JSON.
        /// </summary>
        public string playerData => JsonUtility.ToJson(this, true);


        /// <summary>
        /// Gets whether the player is registered as a string representation.
        /// </summary>
        public string isRegistered => _isRegistered.ToString();

        /// <summary>
        /// Captures an event with the specified name and properties.
        /// </summary>
        /// <param name="eventName">The name of the event to capture.</param>
        /// <param name="properties">The properties associated with the event in JSON format.</param>
        public void CaptureEvent(string eventName, string properties)
        {
            _events.Add(new ValuePair { key = eventName, value = properties });
        }

        /// <summary>
        /// Retrieves a player value by its key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The value associated with the specified key.</returns>
        public string GetPlayerValue(string key)
        {
            return _values.Find(vp => vp.key == key).value;
        }

        /// <summary>
        /// Sets a player value with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to store.</param>
        public void SetPlayerValue(string key, string value)
        {
            int index = _values.FindIndex(vp => vp.key == key);
            var pair = new ValuePair { key = key, value = value };
            if (index >= 0)
            {
                _values[index] = pair;
            }
            else
            {
                _values.Add(pair);
            }
        }

        /// <summary>
        /// Schedules a notification using the provided options.
        /// </summary>
        /// <param name="options">The notification options in JSON format.</param>
        public void ScheduleNotification(string options)
        {
            _notifications.Add(JsonUtility.FromJson<Notifications.Options>(options));
        }

        /// <summary>
        /// Schedule v2 notification request to the Unity console.
        /// </summary>
        /// <param name="options">The notification options in JSON format.</param>
        public void ScheduleNotificationV2(string options)
        {
            _notificationsV2.Add(JsonUtility.FromJson<RichNotifications.Options>(options));
        }

        /// <summary>
        /// Unschedule v2 notification request to the Unity console.
        /// </summary>
        /// <param name="key">The notification key in string format.</param>
        public void UnscheduleNotificationV2(string key)
        {
            _notificationsV2.RemoveAll(n => n.identifier == key);
        }

        /// <summary>
        /// Retrieves the properties of an event by its name.
        /// </summary>
        /// <param name="eventName">The name of the event to retrieve.</param>
        /// <returns>The properties of the event in JSON format.</returns>
        public string GetEvent(string eventName)
        {
            return _events.Find(e => e.key == eventName).value;
        }

        /// <summary>
        /// Gets all scheduled notifications.
        /// </summary>
        /// <returns>A list of notification options in JSON format.</returns>
        public List<string> GetNotifications()
        {
            return _notifications.Select(n => JsonUtility.ToJson(n)).ToList();
        }

        /// <summary>
        /// Gets all scheduled notifications v2.
        /// </summary>
        /// <returns>A list of notification options in JSON format.</returns>
        public List<string> GetNotificationsV2()
        {
            return _notificationsV2.Select(n => JsonUtility.ToJson(n)).ToList();
        }

        /// <summary>
        /// Game-specific entry payload, that was used to launch the game.
        /// </summary>
        /// <returns>A map of strings->objects</returns>
        public string GetEntryPayload()
        {
            var result = new Dictionary<string, object>();
            foreach (var pair in _entryPayload)
            {
                if (!result.ContainsKey(pair.key))
                {
                    result.Add(pair.key, pair.value);
                }
            }

            return Convert.ToString(result);
        }

        /// <summary>
        /// Sets Game-specific entry payload.
        /// </summary>
        /// <param name="payload">A string-object map containing the payload.</param>
        public void SetEntryPayload(Dictionary<string, object> payload)
        {
            foreach (var pair in payload)
            {
                _entryPayload.Add(new ValuePair { key = pair.Key, value = pair.Value.ToString() });
            }
        }

        /// <summary>
        /// Mark user as logged in.
        /// </summary>
        /// <param name="payload">A string-object map containing the payload.</param>
        public void Login(string payload)
        {
            _isRegistered = true;
            Dictionary<string, object> payloadDictionary = Convert.FromString<Dictionary<string, object>>(payload);
            SetEntryPayload(payloadDictionary);
        }

        public string GetProducts()
        {
            return "";
        }



#if UNITY_EDITOR
        private void OnEnable()
        {
            Debug.Log("OnEnable");
            JsBridge.SetMock(this);
        }

        private void OnDisable()
        {
            Debug.Log("OnEnable");
            JsBridge.SetMock(this);
        }
#endif

        /// <summary>
        /// Represents a key-value pair for storing player values and events.
        /// </summary>
        [System.Serializable]
        public struct ValuePair
        {
            /// <summary>The key of the pair.</summary>
            public string key;
            /// <summary>The value of the pair.</summary>
            public string value;
        }
    }
}
