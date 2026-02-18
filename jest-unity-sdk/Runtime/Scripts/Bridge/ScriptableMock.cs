using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace com.jest.sdk
{
    public enum PurchaseReult
    {
        success,
        error
    }

    /// <summary>
    /// ScriptableObject implementation of <see cref="IBridgeMock"/> for testing and debugging purposes.
    /// Allows simulation of player data, events, notifications, and purchase flows within the Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "JestSDKMock", menuName = "JestSDK/Mock")]
    public class ScriptableMock : ScriptableObject, IBridgeMock
    {
        [SerializeField] private string _playerId;
        [SerializeField] private bool _isRegistered;
        [SerializeField] private List<ValuePair> _values;

        [Header("Notifications")]
        [SerializeField] private List<Notifications.Options> _notifications;
        [SerializeField] private List<RichNotifications.Options> _notificationsV2;

        [Header("Events")]
        [SerializeField] private List<ValuePair> _events;

        [Header("Entry Payload")]
        [SerializeField] private List<ValuePair> _entryPayload;

        [Header("Payment")]
        [Tooltip("Select purchase response")]
        [SerializeField] private PurchaseReult _purchaseResult;

        [Tooltip("Select incomplete purchase response")]
        [SerializeField] private PurchaseReult _purchaseCompleteResult;

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

        public PurchaseReult purchaseResult => _purchaseResult;
        public PurchaseReult purchaseCompleteResult => _purchaseCompleteResult;

        /// <summary>
        /// Captures an event with the specified name and properties.
        /// </summary>
        public void CaptureEvent(string eventName, string properties)
        {
            _events.Add(new ValuePair { key = eventName, value = properties });
        }

        /// <summary>
        /// Retrieves a player value by its key.
        /// </summary>
        public string GetPlayerValue(string key)
        {
            return _values.Find(vp => vp.key == key).value;
        }

        /// <summary>
        /// Sets a player value with the specified key.
        /// </summary>
        public void SetPlayerValue(string key, string value)
        {
            int index = _values.FindIndex(vp => vp.key == key);
            var pair = new ValuePair { key = key, value = value };

            if (index >= 0)
                _values[index] = pair;
            else
                _values.Add(pair);
        }

        /// <summary>
        /// Deletes a player value with the specified key.
        /// </summary>
        public void DeletePlayerValue(string key)
        {
            _values.RemoveAll(vp => vp.key == key);
        }

        /// <summary>
        /// Schedules a notification using the provided options.
        /// </summary>
        public void ScheduleNotification(string options)
        {
            _notifications.Add(JsonUtility.FromJson<Notifications.Options>(options));
        }

        /// <summary>
        /// Schedules a version 2 notification using the provided options.
        /// </summary>
        public void ScheduleNotificationV2(string options)
        {
            _notificationsV2.Add(JsonUtility.FromJson<RichNotifications.Options>(options));
        }

        /// <summary>
        /// Unschedules a version 2 notification using the specified key.
        /// </summary>
        public void UnscheduleNotificationV2(string key)
        {
            _notificationsV2.RemoveAll(n => n.identifier == key);
        }

        /// <summary>
        /// Retrieves the properties of an event by its name.
        /// </summary>
        public string GetEvent(string eventName)
        {
            return _events.Find(e => e.key == eventName).value;
        }

        /// <summary>
        /// Retrieves all scheduled notifications.
        /// </summary>
        public List<string> GetNotifications()
        {
            return _notifications.Select(n => JsonUtility.ToJson(n)).ToList();
        }

        /// <summary>
        /// Retrieves all scheduled version 2 notifications.
        /// </summary>
        public List<string> GetNotificationsV2()
        {
            return _notificationsV2.Select(n => JsonUtility.ToJson(n)).ToList();
        }

        /// <summary>
        /// Retrieves the game-specific entry payload used to launch the game.
        /// </summary>
        public string GetEntryPayload()
        {
            var result = new Dictionary<string, object>();
            foreach (var pair in _entryPayload)
            {
                if (!result.ContainsKey(pair.key))
                    result.Add(pair.key, pair.value);
            }

            return Convert.ToString(result);
        }

        /// <summary>
        /// Sets the game-specific entry payload.
        /// </summary>
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
        public void Login(string payload)
        {
            _isRegistered = true;
            Dictionary<string, object> payloadDictionary = Convert.FromString<Dictionary<string, object>>(payload);
            SetEntryPayload(payloadDictionary);
        }

        /// <summary>
        /// Retrieves available in-app purchase products.
        /// </summary>
        public string GetProducts()
        {
            return "[{\"sku\":\"gems_100\",\"name\":\"100 Gems\",\"description\":\"Get 100 gems to use in the game\",\"price\":99.0},{\"sku\":\"gems_500\",\"name\":\"500 Gems\",\"description\":\"Get 500 gems to use in the game\",\"price\":499.0}]";
        }

        /// <summary>
        /// Retrieves the in-app purchase response.
        /// </summary>
        public string GetPurchaseResponse()
        {
            switch (_purchaseResult)
            {
                case PurchaseReult.success:
                    return "{\"result\":\"success\",\"purchase\":{\"purchaseToken\":\"mock_token_bcwux13xvm4\",\"productSku\":\"gems_100\",\"credits\":99,\"createdAt\":1761729039,\"completedAt\":null},\"purchaseSigned\":\"JWS\"}";
                default:
                    return "{\"result\":\"error\",\"error\":\"internal_error\"}";
            }
        }

        /// <summary>
        /// Retrieves the incomplete purchase response.
        /// </summary>
        public string GetIncompletePurchaseResponse()
        {
            return "{\"hasMore\":false,\"purchasesSigned\":\"JWS\",\"purchases\":[{\"purchaseToken\":\"mock_token_bcwux13xvm4\",\"productSku\":\"gems_100\",\"credits\":99,\"createdAt\":1761729039,\"completedAt\":null}]}";
        }

        /// <summary>
        /// Retrieves the complete purchase response.
        /// </summary>
        public string GetPurchaseCompleteResponse()
        {
            switch (_purchaseCompleteResult)
            {
                case PurchaseReult.success:
                    return "{\"result\":\"success\"}";
                default:
                    return "{\"result\":\"error\",\"error\":\"internal_error\"}";
            }
        }

        /// <summary>
        /// Opens a referral share dialog with the specified options.
        /// </summary>
        public void OpenReferralDialog(string optionsJson)
        {
            Debug.Log($"[JestSDK] OpenReferralDialog {optionsJson}");
        }

        /// <summary>
        /// Retrieves the list referrals response.
        /// </summary>
        public string GetListReferralsResponse()
        {
            return "{\"referrals\":[{\"reference\":\"mock-ref\",\"registrations\":[\"user1\"]}],\"referralsSigned\":\"mock_signed\"}";
        }

        /// <summary>
        /// Logs a redirect to game request.
        /// </summary>
        public void RedirectToGame(string optionsJson)
        {
            Debug.Log($"[JestSDK] RedirectToGame {optionsJson}");
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
