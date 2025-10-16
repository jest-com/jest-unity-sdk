using System.Collections.Generic;
using UnityEngine;

namespace com.jest.sdk
{
    /// <summary>
    /// A mock implementation of IBridgeMock for testing purposes.
    /// Provides in-memory storage for events, player values, and notifications.
    /// </summary>
    public class TestBridgeMock : IBridgeMock
    {
        private Dictionary<string, string> _events = new();
        private Dictionary<string, string> _playerValues = new();
        private List<string> _notifications = new();

        private string _entryPayload;

        /// <summary>
        /// Gets the unique identifier for the player.
        /// </summary>
        public string playerId { get; }

        /// <summary>
        /// Returns all serialized player data as JSON.
        /// </summary>
        public string playerData => JsonUtility.ToJson(this, true);


        /// <summary>
        /// Gets whether the player is registered as a string representation.
        /// </summary>
        public string isRegistered { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TestBridgeMock class.
        /// </summary>
        /// <param name="playerId">The unique identifier for the player.</param>
        /// <param name="isRegistered">Whether the player is registered.</param>
        public TestBridgeMock(string playerId, bool isRegistered)
        {
            this.playerId = playerId;
            this.isRegistered = isRegistered.ToString();
        }

        /// <summary>
        /// Captures an event with the specified name and properties.
        /// </summary>
        /// <param name="eventName">The name of the event to capture.</param>
        /// <param name="properties">The properties associated with the event in JSON format.</param>
        public void CaptureEvent(string eventName, string properties)
        {
            _events.Add(eventName, properties);
        }

        /// <summary>
        /// Retrieves a player value by its key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The value associated with the specified key, or empty string if not found.</returns>
        public string GetPlayerValue(string key)
        {
            if (_playerValues.TryGetValue(key, out var value))
            {
                return value;
            }
            return "";
        }

        /// <summary>
        /// Schedules a notification using the provided options.
        /// </summary>
        /// <param name="options">The notification options in JSON format.</param>
        public void ScheduleNotification(string options)
        {
            _notifications.Add(options);
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
        /// Retrieves the properties of an event by its name.
        /// </summary>
        /// <param name="eventName">The name of the event to retrieve.</param>
        /// <returns>The properties of the event in JSON format, or empty string if not found.</returns>
        public string GetEvent(string eventName)
        {
            if (_events.TryGetValue(eventName, out var evnt))
            {
                return evnt;
            }

            return "";
        }

        /// <summary>
        /// Gets all scheduled notifications.
        /// </summary>
        /// <returns>A list of notification options in JSON format.</returns>
        public List<string> GetNotifications()
        {
            return _notifications;
        }

        /// <summary>
        /// Game-specific entry payload, that was used to launch the game.
        /// </summary>
        /// <returns>A map of strings->objects</returns>
        public string GetEntryPayload()
        {
            return _entryPayload;
        }

        /// <summary>
        /// Sets Game-specific entry payload.
        /// </summary>
        /// <param name="payload">A string-object map containing the payload.</param>
        public void SetEntryPayload(Dictionary<string, object> payload)
        {
            _entryPayload = Convert.ToString(payload);
        }

        /// <summary>
        /// Mark user as logged in.
        /// </summary>
        /// <param name="payload">A string-object map containing the payload.</param>
        public void Login(string payload)
        {
            this.isRegistered = true.ToString();
            _entryPayload = payload;
        }

    }
}
