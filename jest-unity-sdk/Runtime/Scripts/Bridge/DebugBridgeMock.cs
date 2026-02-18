using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.jest.sdk
{
    /// <summary>
    /// Mock implementation of <see cref="IBridgeMock"/> that logs operations to the Unity console.
    /// Used for debugging and testing Jest SDK functionality without platform dependencies.
    /// </summary>
    public class DebugBridgeMock : IBridgeMock
    {
        /// <summary>
        /// Gets the unique identifier for the player.
        /// </summary>
        public string playerId { get; }

        /// <summary>
        /// Gets the serialized player data.
        /// </summary>
        public string playerData { get; }

        /// <summary>
        /// Gets a string indicating whether the player is registered.
        /// </summary>
        public string isRegistered { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugBridgeMock"/> class.
        /// </summary>
        /// <param name="playerId">The unique identifier for the player.</param>
        /// <param name="isRegistered">Indicates whether the player is registered.</param>
        public DebugBridgeMock(string playerId, bool isRegistered)
        {
            this.playerId = playerId;
            this.playerData = playerId;
            this.isRegistered = isRegistered.ToString();
        }

        /// <summary>
        /// Logs an event capture request to the Unity console.
        /// </summary>
        /// <param name="eventName">The name of the event to capture.</param>
        /// <param name="properties">The event properties in JSON format.</param>
        public void CaptureEvent(string eventName, string properties)
        {
            Debug.Log($"[JestSDK] CaptureEvent {eventName} {properties}");
        }

        /// <summary>
        /// Logs a player value retrieval request to the Unity console.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The key itself as a mock value.</returns>
        public string GetPlayerValue(string key)
        {
            Debug.Log($"[JestSDK] GetPlayerValue {key}");
            return key;
        }

        /// <summary>
        /// Logs a notification scheduling request to the Unity console.
        /// </summary>
        /// <param name="options">The notification options in JSON format.</param>
        public void ScheduleNotification(string options)
        {
            Debug.Log($"[JestSDK] ScheduleNotification {options}");
        }

        /// <summary>
        /// Logs a version 2 notification scheduling request to the Unity console.
        /// </summary>
        /// <param name="options">The notification options in JSON format.</param>
        public void ScheduleNotificationV2(string options)
        {
            Debug.Log($"[JestSDK] ScheduleNotificationV2 {options}");
        }

        /// <summary>
        /// Logs a version 2 notification unscheduling request to the Unity console.
        /// </summary>
        /// <param name="key">The notification key.</param>
        public void UnscheduleNotificationV2(string key)
        {
            Debug.Log($"[JestSDK] UnscheduleNotificationV2 {key}");
        }

        /// <summary>
        /// Logs a player value update request to the Unity console.
        /// </summary>
        /// <param name="key">The key of the value to update.</param>
        /// <param name="value">The new value to assign.</param>
        public void SetPlayerValue(string key, string value)
        {
            Debug.Log($"[JestSDK] SetPlayerValue {key} {value}");
        }

        /// <summary>
        /// Logs a player value delete request to the Unity console.
        /// </summary>
        /// <param name="key">The key of the value to delete.</param>
        public void DeletePlayerValue(string key)
        {
            Debug.Log($"[JestSDK] DeletePlayerValue {key}");
        }

        /// <summary>
        /// Retrieves mock event data for the specified event.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <returns>An empty string representing mock event data.</returns>
        public string GetEvent(string eventName)
        {
            return "";
        }

        /// <summary>
        /// Retrieves a list of scheduled notifications.
        /// </summary>
        /// <returns>An empty list representing mock notification data.</returns>
        public List<string> GetNotifications()
        {
            return new List<string>();
        }

        /// <summary>
        /// Retrieves a list of scheduled version 2 notifications.
        /// </summary>
        /// <returns>An empty list representing mock notification data.</returns>
        public List<string> GetNotificationsV2()
        {
            return new List<string>();
        }

        /// <summary>
        /// Retrieves the game-specific entry payload used to launch the game.
        /// </summary>
        /// <returns>An empty string representing mock entry payload data.</returns>
        public string GetEntryPayload()
        {
            return "";
        }

        /// <summary>
        /// Logs a request to set the game-specific entry payload.
        /// </summary>
        /// <param name="payload">A dictionary containing the payload data.</param>
        public void SetEntryPayload(Dictionary<string, object> payload)
        {
            Debug.Log($"[JestSDK] SetEntryPayload {Convert.ToString(payload)}");
        }

        /// <summary>
        /// Logs a user login event to the Unity console and updates registration status.
        /// </summary>
        /// <param name="payload">A JSON string representing the login payload.</param>
        public void Login(string payload)
        {
            isRegistered = true.ToString();
            Debug.Log($"[JestSDK] Logged In {isRegistered}");
        }

        /// <summary>
        /// Logs a product retrieval request to the Unity console.
        /// </summary>
        /// <returns>An empty string representing mock product data.</returns>
        public string GetProducts()
        {
            Debug.Log($"[JestSDK] GetProducts");
            return "";
        }

        /// <summary>
        /// Logs a purchase response retrieval request to the Unity console.
        /// </summary>
        /// <returns>An empty string representing mock purchase response data.</returns>
        public string GetPurchaseResponse()
        {
            Debug.Log($"[JestSDK] GetPurchaseResponse");
            return "";
        }

        /// <summary>
        /// Logs an incomplete purchase response retrieval request to the Unity console.
        /// </summary>
        /// <returns>An empty string representing mock incomplete purchase data.</returns>
        public string GetIncompletePurchaseResponse()
        {
            Debug.Log($"[JestSDK] GetIncompletePurchaseResponse");
            return "";
        }

        /// <summary>
        /// Logs a complete purchase response retrieval request to the Unity console.
        /// </summary>
        /// <returns>An empty string representing mock complete purchase data.</returns>
        public string GetPurchaseCompleteResponse()
        {
            Debug.Log($"[JestSDK] GetPurchaseCompleteResponse");
            return "";
        }

        /// <summary>
        /// Logs an open referral dialog request to the Unity console.
        /// </summary>
        /// <param name="optionsJson">The referral options in JSON format.</param>
        public void OpenReferralDialog(string optionsJson)
        {
            Debug.Log($"[JestSDK] OpenReferralDialog {optionsJson}");
        }

        /// <summary>
        /// Logs a list referrals request to the Unity console.
        /// </summary>
        /// <returns>An empty referrals response.</returns>
        public string GetListReferralsResponse()
        {
            Debug.Log("[JestSDK] GetListReferralsResponse");
            return "{\"referrals\":[],\"referralsSigned\":\"\"}";
        }

        /// <summary>
        /// Logs a redirect to game request to the Unity console.
        /// </summary>
        /// <param name="optionsJson">The redirect options in JSON format.</param>
        public void RedirectToGame(string optionsJson)
        {
            Debug.Log($"[JestSDK] RedirectToGame {optionsJson}");
        }
    }
}
