using System.Collections.Generic;
using UnityEngine;

namespace com.unity.jest
{
    /// <summary>
    /// A debug implementation of IBridgeMock that logs operations to the Unity console.
    /// Used for debugging and testing purposes.
    /// </summary>
    public class DebugBridgeMock : IBridgeMock
    {
        /// <summary>
        /// Gets the unique identifier for the player.
        /// </summary>
        public string playerId { get; }

        /// <summary>
        /// Gets whether the player is registered as a string representation.
        /// </summary>
        public string isRegistered { get; private set; }

        /// <summary>
        /// Initializes a new instance of the DebugBridgeMock class.
        /// </summary>
        /// <param name="playerId">The unique identifier for the player.</param>
        /// <param name="isRegistered">Whether the player is registered.</param>
        public DebugBridgeMock(string playerId, bool isRegistered)
        {
            this.playerId = playerId;
            this.isRegistered = isRegistered.ToString();
        }

        /// <summary>
        /// Logs a capture event to the Unity console.
        /// </summary>
        /// <param name="eventName">The name of the event to capture.</param>
        /// <param name="properties">The properties associated with the event in JSON format.</param>
        public void CaptureEvent(string eventName, string properties)
        {
            Debug.Log($"[JestSDK] CaptureEvent {eventName} {properties}");
        }

        /// <summary>
        /// Logs a get player value request to the Unity console.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The key as the mock value.</returns>
        public string GetPlayerValue(string key)
        {
            Debug.Log($"[JestSDK] GetPlayerValue {key}");
            return key;
        }

        /// <summary>
        /// Logs a schedule notification request to the Unity console.
        /// </summary>
        /// <param name="options">The notification options in JSON format.</param>
        public void ScheduleNotification(string options)
        {
            Debug.Log($"[JestSDK] ScheduleNotification {options}");
        }

        /// <summary>
        /// Logs a set player value request to the Unity console.
        /// </summary>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to store.</param>
        public void SetPlayerValue(string key, string value)
        {
            Debug.Log($"[JestSDK] SetPlayerValue {key} {value}");
        }

        /// <summary>
        /// Gets the properties of an event by its name.
        /// </summary>
        /// <param name="eventName">The name of the event to retrieve.</param>
        /// <returns>An empty string as mock event data.</returns>
        public string GetEvent(string eventName)
        {
            return "";
        }

        /// <summary>
        /// Gets all scheduled notifications.
        /// </summary>
        /// <returns>An empty list as mock notification data.</returns>
        public List<string> GetNotifications()
        {
            return new List<string>();
        }

        /// <summary>
        /// Game-specific entry payload, that was used to launch the game.
        /// </summary>
        /// <returns>A map of strings->objects</returns>
        public string GetEntryPayload()
        {
            return "";
        }

        /// <summary>
        /// Sets Game-specific entry payload.
        /// </summary>
        /// <param name="payload">A string-object map containing the payload.</param>
        public void SetEntryPayload(Dictionary<string, object> payload)
        {
            Debug.Log($"[JestSDK] SetEntryPayload {Convert.ToString(payload)}");
        }


        /// <summary>
        /// Mark user as logged in.
        /// </summary>
        /// <param name="payload">A string-object map containing the payload.</param>
        public void Login(string payload)
        {
            this.isRegistered = true.ToString();
            Debug.Log($"[JestSDK] Logged In {this.isRegistered}");
        }
    }
}
