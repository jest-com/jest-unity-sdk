using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace com.jest.sdk
{
    /// <summary>
    /// ScriptableObject implementation of <see cref="IBridgeMock"/> for testing and debugging purposes.
    /// Allows simulation of player data, events, notifications, and purchase flows within the Unity Editor.
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
        [SerializeField] private List<Payment.Product> _purchaseProducts;
        [SerializeField] private Payment.PurchaseResult _purchaseResponse;
        [SerializeField] private Payment.IncompletePurchasesResponse _incompletePurchaseResponse;
        [SerializeField] private Payment.PurchaseCompleteResult _purchaseCompleteResponse;

        /// <summary>
        /// Gets the unique identifier for the player.
        /// </summary>
        public string playerId => _playerId;

        /// <summary>
        /// Gets all serialized player data as a JSON string.
        /// </summary>
        public string playerData => JsonUtility.ToJson(this, true);

        /// <summary>
        /// Gets whether the player is registered, represented as a string value.
        /// </summary>
        public string isRegistered => _isRegistered.ToString();

        /// <summary>
        /// Captures an event with the specified name and properties.
        /// </summary>
        /// <param name="eventName">The name of the event to capture.</param>
        /// <param name="properties">A JSON string containing the event properties.</param>
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
        /// <param name="options">A JSON string containing the notification options.</param>
        public void ScheduleNotification(string options)
        {
            _notifications.Add(JsonUtility.FromJson<Notifications.Options>(options));
        }

        /// <summary>
        /// Schedules a version 2 notification using the provided options.
        /// </summary>
        /// <param name="options">A JSON string containing the notification options.</param>
        public void ScheduleNotificationV2(string options)
        {
            _notificationsV2.Add(JsonUtility.FromJson<RichNotifications.Options>(options));
        }

        /// <summary>
        /// Unschedules a version 2 notification using the specified key.
        /// </summary>
        /// <param name="key">The identifier of the notification to unschedule.</param>
        public void UnscheduleNotificationV2(string key)
        {
            _notificationsV2.RemoveAll(n => n.identifier == key);
        }

        /// <summary>
        /// Retrieves the properties of an event by its name.
        /// </summary>
        /// <param name="eventName">The name of the event to retrieve.</param>
        /// <returns>A JSON string containing the event properties.</returns>
        public string GetEvent(string eventName)
        {
            return _events.Find(e => e.key == eventName).value;
        }

        /// <summary>
        /// Retrieves all scheduled notifications.
        /// </summary>
        /// <returns>A list of notification options as JSON strings.</returns>
        public List<string> GetNotifications()
        {
            return _notifications.Select(n => JsonUtility.ToJson(n)).ToList();
        }

        /// <summary>
        /// Retrieves all scheduled version 2 notifications.
        /// </summary>
        /// <returns>A list of notification options as JSON strings.</returns>
        public List<string> GetNotificationsV2()
        {
            return _notificationsV2.Select(n => JsonUtility.ToJson(n)).ToList();
        }

        /// <summary>
        /// Retrieves the game-specific entry payload used to launch the game.
        /// </summary>
        /// <returns>A JSON string representing the entry payload.</returns>
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
        /// Sets the game-specific entry payload.
        /// </summary>
        /// <param name="payload">A dictionary containing the payload data.</param>
        public void SetEntryPayload(Dictionary<string, object> payload)
        {
            foreach (var pair in payload)
            {
                _entryPayload.Add(new ValuePair { key = pair.Key, value = pair.Value.ToString() });
            }
        }

        /// <summary>
        /// Marks the user as logged in and updates the entry payload using the provided data.
        /// </summary>
        /// <param name="payload">A JSON string representing the login payload.</param>
        public void Login(string payload)
        {
            _isRegistered = true;
            Dictionary<string, object> payloadDictionary = Convert.FromString<Dictionary<string, object>>(payload);
            SetEntryPayload(payloadDictionary);
        }

        /// <summary>
        /// Retrieves available in-app purchase products.
        /// </summary>
        /// <returns>A JSON string containing the list of mock products.</returns>
        public string GetProducts()
        {
            return JsonConvert.SerializeObject(_purchaseProducts);
        }

        /// <summary>
        /// Retrieves the in-app purchase response.
        /// </summary>
        /// <returns>A JSON string containing the mock purchase response.</returns>
        public string GetPurchaseResponse()
        {
            return JsonConvert.SerializeObject(_purchaseResponse);
        }

        /// <summary>
        /// Retrieves the incomplete purchase response.
        /// </summary>
        /// <returns>A JSON string containing mock incomplete purchase data.</returns>
        public string GetIncompletePurchaseResponse()
        {
            return JsonConvert.SerializeObject(_incompletePurchaseResponse);
        }

        /// <summary>
        /// Retrieves the complete purchase response.
        /// </summary>
        /// <returns>A JSON string containing mock complete purchase data.</returns>
        public string GetPurchaseCompleteResponse()
        {
            return JsonConvert.SerializeObject(_purchaseCompleteResponse);
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            Debug.Log("[JestSDKMock] Enabled");
            JsBridge.SetMock(this);
        }

        private void OnDisable()
        {
            Debug.Log("[JestSDKMock] Disabled");
            JsBridge.SetMock(null);
        }
#endif

        /// <summary>
        /// Represents a key-value pair used for storing player values, events, and payload entries.
        /// </summary>
        [System.Serializable]
        public struct ValuePair
        {
            /// <summary>
            /// The key of the pair.
            /// </summary>
            public string key;

            /// <summary>
            /// The value associated with the key.
            /// </summary>
            public string value;
        }
    }
}
