using System.Collections.Generic;

namespace com.jest.sdk
{
    /// <summary>
    /// Defines the contract for mocking bridge functionality in the Jest SDK.
    /// Provides mock implementations for player data management, notifications, and event tracking.
    /// </summary>
    public interface IBridgeMock
    {
        /// <summary>
        /// Gets the unique identifier for the current player.
        /// </summary>
        string playerId { get; }

        /// <summary>
        /// Gets the serialized player data as a JSON string.
        /// </summary>
        string playerData { get; }

        /// <summary>
        /// Gets the registration status of the current player.
        /// </summary>
        string isRegistered { get; }

        /// <summary>
        /// Retrieves a stored player value by its key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The stored value associated with the specified key.</returns>
        string GetPlayerValue(string key);

        /// <summary>
        /// Stores a player value with the specified key.
        /// </summary>
        /// <param name="key">The key under which to store the value.</param>
        /// <param name="value">The value to store.</param>
        void SetPlayerValue(string key, string value);

        /// <summary>
        /// Deletes a player value with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to delete.</param>
        void DeletePlayerValue(string key);

        /// <summary>
        /// Schedules a notification with the specified options.
        /// </summary>
        /// <param name="options">A JSON string containing the notification options.</param>
        void ScheduleNotification(string options);

        /// <summary>
        /// Schedules a version 2 notification with the specified options.
        /// </summary>
        /// <param name="options">A JSON string containing the notification options.</param>
        void ScheduleNotificationV2(string options);

        /// <summary>
        /// Unschedules a version 2 notification using the specified key.
        /// </summary>
        /// <param name="key">The key of the notification to unschedule.</param>
        void UnscheduleNotificationV2(string key);

        /// <summary>
        /// Captures an event with the specified name and properties.
        /// </summary>
        /// <param name="eventName">The name of the event to capture.</param>
        /// <param name="properties">A JSON string containing the event properties.</param>
        void CaptureEvent(string eventName, string properties);

        /// <summary>
        /// Retrieves an event by its name.
        /// </summary>
        /// <param name="eventName">The name of the event to retrieve.</param>
        /// <returns>A JSON string containing the event data.</returns>
        string GetEvent(string eventName);

        /// <summary>
        /// Retrieves all scheduled notifications.
        /// </summary>
        /// <returns>A list of notification data as JSON strings.</returns>
        List<string> GetNotifications();

        /// <summary>
        /// Retrieves all scheduled version 2 notifications.
        /// </summary>
        /// <returns>A list of notification data as JSON strings.</returns>
        List<string> GetNotificationsV2();

        /// <summary>
        /// Retrieves the game-specific entry payload used to launch the game.
        /// </summary>
        /// <returns>A JSON string containing the entry payload.</returns>
        string GetEntryPayload();

        /// <summary>
        /// Sets the game-specific entry payload.
        /// </summary>
        /// <param name="payload">A dictionary containing the payload data.</param>
        void SetEntryPayload(Dictionary<string, object> payload);

        /// <summary>
        /// Marks the user as logged in.
        /// </summary>
        /// <param name="payload">A JSON string containing login payload data.</param>
        void Login(string payload);

        /// <summary>
        /// Retrieves available in-app purchase products.
        /// </summary>
        /// <returns>A JSON string containing product data.</returns>
        string GetProducts();

        /// <summary>
        /// Retrieves the in-app purchase response.
        /// </summary>
        /// <returns>A JSON string containing purchase response data.</returns>
        string GetPurchaseResponse();

        /// <summary>
        /// Retrieves the incomplete purchase response.
        /// </summary>
        /// <returns>A JSON string containing incomplete purchase response data.</returns>
        string GetIncompletePurchaseResponse();

        /// <summary>
        /// Retrieves the complete purchase response.
        /// </summary>
        /// <returns>A JSON string containing complete purchase response data.</returns>
        string GetPurchaseCompleteResponse();
    }
}
