using System.Collections.Generic;

namespace JestSDK
{
    /// <summary>
    /// Interface for mocking bridge functionality in the Jest SDK.
    /// Provides mock implementations for player data, notifications, and event tracking.
    /// </summary>
    public interface IBridgeMock
    {
        /// <summary>
        /// Gets the unique identifier for the current player.
        /// </summary>
        string playerId { get; }

        /// <summary>
        /// Gets the registration status of the current player.
        /// </summary>
        string isRegistered { get; }


        /// <summary>
        /// Retrieves a stored player value by its key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve</param>
        /// <returns>The stored value associated with the key</returns>
        string GetPlayerValue(string key);

        /// <summary>
        /// Stores a player value with the specified key.
        /// </summary>
        /// <param name="key">The key to store the value under</param>
        /// <param name="value">The value to store</param>
        void SetPlayerValue(string key, string value);

        /// <summary>
        /// Schedules a notification with the specified options.
        /// </summary>
        /// <param name="options">JSON string containing notification options</param>
        void ScheduleNotification(string options);

        /// <summary>
        /// Captures an event with the specified name and properties.
        /// </summary>
        /// <param name="eventName">Name of the event to capture</param>
        /// <param name="properties">JSON string containing event properties</param>
        void CaptureEvent(string eventName, string properties);

        /// <summary>
        /// Retrieves an event by its name.
        /// </summary>
        /// <param name="eventName">Name of the event to retrieve</param>
        /// <returns>The event data as a JSON string</returns>
        string GetEvent(string eventName);

        /// <summary>
        /// Gets all scheduled notifications.
        /// </summary>
        /// <returns>A list of notifications as JSON strings</returns>
        List<string> GetNotifications();

        /// <summary>
        /// Game-specific entry payload, that was used to launch the game.
        /// </summary>
        /// <returns>A map of strings->objects</returns>
        string GetEntryPayload();

        /// <summary>
        /// Sets Game-specific entry payload.
        /// </summary>
        /// <param name="payload">A string-object map containing the payload.</param>
        void SetEntryPayload(Dictionary<string, object> payload);
    }
}
