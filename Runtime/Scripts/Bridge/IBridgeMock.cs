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
        /// Schedules a rich notification with the specified options.
        /// </summary>
        /// <param name="options">A JSON string containing the notification options.</param>
        void ScheduleNotificationV2(string options);

        /// <summary>
        /// Unschedules a version 2 notification using the specified key.
        /// </summary>
        /// <param name="key">The key of the notification to unschedule.</param>
        void UnscheduleNotificationV2(string key);

        /// <summary>
        /// Retrieves all scheduled rich notifications.
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

        /// <summary>
        /// Opens a referral share dialog with the specified options.
        /// </summary>
        /// <param name="optionsJson">A JSON string containing the referral options.</param>
        void OpenReferralDialog(string optionsJson);

        /// <summary>
        /// Retrieves the list referrals response.
        /// </summary>
        /// <returns>A JSON string containing referral data.</returns>
        string GetListReferralsResponse();

        /// <summary>
        /// Redirects to another game or the flagship game.
        /// </summary>
        /// <param name="optionsJson">A JSON string containing redirect options.</param>
        void RedirectToGame(string optionsJson);

        /// <summary>
        /// Opens a legal page (privacy policy, terms of service, or copyright).
        /// </summary>
        /// <param name="page">The page type: "privacy", "terms", or "copyright".</param>
        void OpenLegalPage(string page);

        /// <summary>
        /// Retrieves the signed player response for server verification.
        /// </summary>
        /// <returns>A JSON string containing signed player data.</returns>
        string GetPlayerSignedResponse();
    }
}
