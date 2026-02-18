using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.jest.sdk
{
    /// <summary>
    /// Mock implementation of <see cref="IBridgeMock"/> for testing and development.
    /// Provides in-memory storage for events, player values, and notifications without platform dependencies.
    /// </summary>
    public class TestBridgeMock : IBridgeMock
    {
        private readonly Dictionary<string, string> _events = new();
        private readonly Dictionary<string, string> _playerValues = new();
        private readonly List<string> _notifications = new();
        private readonly List<RichNotifications.Options> _notificationsV2 = new();
        private string _entryPayload;

        /// <summary>
        /// Gets the unique identifier for the player.
        /// </summary>
        public string playerId { get; }

        /// <summary>
        /// Gets all serialized player data as a JSON string.
        /// </summary>
        public string playerData => JsonUtility.ToJson(this, true);

        /// <summary>
        /// Gets the registration status of the player as a string representation.
        /// </summary>
        public string isRegistered { get; private set; }

        public PurchaseReult purchaseResult { get; set; }
        public PurchaseReult purchaseCompleteResult { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="TestBridgeMock"/> class.
        /// </summary>
        /// <param name="playerId">The unique identifier for the player.</param>
        /// <param name="isRegistered">Indicates whether the player is registered.</param>
        public TestBridgeMock(string playerId, bool isRegistered)
        {
            this.playerId = playerId;
            this.isRegistered = isRegistered.ToString();
        }

        /// <summary>
        /// Captures an event with the specified name and properties.
        /// </summary>
        /// <param name="eventName">The name of the event to capture.</param>
        /// <param name="properties">A JSON string containing event properties.</param>
        public void CaptureEvent(string eventName, string properties)
        {
            _events[eventName] = properties;
        }

        /// <summary>
        /// Retrieves a player value by its key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The stored value if found; otherwise, an empty string.</returns>
        public string GetPlayerValue(string key)
        {
            return _playerValues.TryGetValue(key, out var value) ? value : "";
        }

        /// <summary>
        /// Sets a player value with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to store.</param>
        public void SetPlayerValue(string key, string value)
        {
            _playerValues[key] = value;
        }

        /// <summary>
        /// Deletes a player value with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to delete.</param>
        public void DeletePlayerValue(string key)
        {
            _playerValues.Remove(key);
        }

        /// <summary>
        /// Schedules a notification using the provided options.
        /// </summary>
        /// <param name="options">A JSON string containing notification options.</param>
        public void ScheduleNotification(string options)
        {
            _notifications.Add(options);
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
        /// Retrieves an event by its name.
        /// </summary>
        /// <param name="eventName">The name of the event to retrieve.</param>
        /// <returns>A JSON string containing event properties, or an empty string if not found.</returns>
        public string GetEvent(string eventName)
        {
            return _events.TryGetValue(eventName, out var evnt) ? evnt : "";
        }

        /// <summary>
        /// Retrieves all scheduled notifications.
        /// </summary>
        /// <returns>A list of notification data in JSON format.</returns>
        public List<string> GetNotifications()
        {
            return _notifications;
        }

        /// <summary>
        /// Retrieves all scheduled version 2 notifications.
        /// </summary>
        /// <returns>A list of notification data in JSON format.</returns>
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
            return _entryPayload;
        }

        /// <summary>
        /// Sets the game-specific entry payload.
        /// </summary>
        /// <param name="payload">A dictionary containing the payload data.</param>
        public void SetEntryPayload(Dictionary<string, object> payload)
        {
            _entryPayload = Convert.ToString(payload);
        }

        /// <summary>
        /// Marks the user as logged in and stores the provided payload.
        /// </summary>
        /// <param name="payload">A JSON string representing login payload data.</param>
        public void Login(string payload)
        {
            isRegistered = true.ToString();
            _entryPayload = payload;
        }

        /// <summary>
        /// Retrieves available in-app purchase products.
        /// </summary>
        /// <returns>A JSON string representing product data.</returns>
        public string GetProducts()
        {
            Debug.Log("[JestSDK] GetProducts");
            return "[{\"sku\":\"gems_100\",\"name\":\"100 Gems\",\"description\":\"Get 100 gems to use in the game\",\"price\":99.0}]";
        }

        /// <summary>
        /// Retrieves the in-app purchase response.
        /// </summary>
        /// <returns>A JSON string representing purchase response data.</returns>
        public string GetPurchaseResponse()
        {
            Debug.Log("[JestSDK] GetPurchaseResponse");
            switch (purchaseResult)
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
        /// <returns>A JSON string representing incomplete purchase response data.</returns>
        public string GetIncompletePurchaseResponse()
        {
            Debug.Log("[JestSDK] GetIncompletePurchaseResponse");
            return "{\"hasMore\":false,\"purchasesSigned\":\"JWS\",\"purchases\":[{\"purchaseToken\":\"mock_token_bcwux13xvm4\",\"productSku\":\"gems_100\",\"credits\":99,\"createdAt\":1761729039,\"completedAt\":null}]}";
        }

        /// <summary>
        /// Retrieves the complete purchase response.
        /// </summary>
        /// <returns>A JSON string representing complete purchase response data.</returns>
        public string GetPurchaseCompleteResponse()
        {
            Debug.Log("[JestSDK] GetPurchaseCompleteResponse");
            switch (purchaseCompleteResult)
            {
                case PurchaseReult.success:
                    return "{\"result\":\"success\"}";
                default:
                    return "{\"result\":\"error\",\"error\":\"internal_error\"}";
            }
        }

        /// <summary>
        /// Logs an open referral dialog request.
        /// </summary>
        /// <param name="optionsJson">The referral options in JSON format.</param>
        public void OpenReferralDialog(string optionsJson)
        {
            Debug.Log($"[JestSDK] OpenReferralDialog {optionsJson}");
        }

        /// <summary>
        /// Retrieves the list referrals response.
        /// </summary>
        /// <returns>A JSON string containing mock referral data.</returns>
        public string GetListReferralsResponse()
        {
            Debug.Log("[JestSDK] GetListReferralsResponse");
            return "{\"referrals\":[{\"reference\":\"test-ref-123\",\"registrations\":[\"user1\",\"user2\"]}],\"referralsSigned\":\"mock_signed_data\"}";
        }

        /// <summary>
        /// Logs a redirect to game request.
        /// </summary>
        /// <param name="optionsJson">The redirect options in JSON format.</param>
        public void RedirectToGame(string optionsJson)
        {
            Debug.Log($"[JestSDK] RedirectToGame {optionsJson}");
        }

        /// <summary>
        /// Logs an open legal page request.
        /// </summary>
        /// <param name="page">The page type.</param>
        public void OpenLegalPage(string page)
        {
            Debug.Log($"[JestSDK] OpenLegalPage {page}");
        }

        /// <summary>
        /// Returns mock signed player response.
        /// </summary>
        /// <returns>A JSON string containing mock signed player data.</returns>
        public string GetPlayerSignedResponse()
        {
            Debug.Log("[JestSDK] GetPlayerSignedResponse");
            return $"{{\"player\":{{\"playerId\":\"{playerId}\",\"registered\":{isRegistered.ToLower()}}},\"playerSigned\":\"mock_signed_data\"}}";
        }
    }
}
