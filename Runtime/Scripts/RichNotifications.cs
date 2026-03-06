using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides functionality for managing rich notifications within the Jest SDK.
    /// Supports scheduling, unscheduling, and retrieving version 2 notifications with extended data payloads.
    /// </summary>
    public class RichNotifications
    {
        /// <summary>
        /// Defines the severity or importance level of a notification.
        /// </summary>
        public enum Severity
        {
            /// <summary>Low priority notification.</summary>
            Low,

            /// <summary>Medium priority notification.</summary>
            Medium,

            /// <summary>High priority notification.</summary>
            High
        }

        /// <summary>Maximum number of characters allowed in the CTA text.</summary>
        public const int CTA_CHAR_LIMIT = 25;

        /// <summary>Maximum number of days a notification can be scheduled ahead.</summary>
        public const int MAX_SCHEDULE_DAYS = 7;

        /// <summary>
        /// Internal constructor to prevent external instantiation.
        /// </summary>
        internal RichNotifications() { }

        /// <summary>
        /// Schedules a new rich notification using the specified options.
        /// </summary>
        /// <param name="options">The notification configuration options.</param>
        /// <exception cref="ArgumentException">Thrown when options fail validation.</exception>
        public void ScheduleNotification(Options options)
        {
            if (string.IsNullOrEmpty(options.body))
                throw new ArgumentException("body is required");

            if (string.IsNullOrEmpty(options.ctaText))
                throw new ArgumentException("ctaText is required");

            if (options.ctaText.Length > CTA_CHAR_LIMIT)
                throw new ArgumentException($"ctaText must be {CTA_CHAR_LIMIT} characters or fewer");

            if (string.IsNullOrEmpty(options.identifier))
                throw new ArgumentException("identifier is required");

            bool hasDate = options.date != default;
            bool hasDays = options.scheduledInDays.HasValue;

            if (!hasDate && !hasDays)
                throw new ArgumentException("Either date or scheduledInDays must be provided");

            if (hasDate && hasDays)
                throw new ArgumentException("date and scheduledInDays are mutually exclusive");

            if (hasDays && (options.scheduledInDays.Value < 1 || options.scheduledInDays.Value > MAX_SCHEDULE_DAYS))
                throw new ArgumentException($"scheduledInDays must be between 1 and {MAX_SCHEDULE_DAYS}");

            if (hasDate)
            {
                var maxDate = DateTime.UtcNow.AddDays(MAX_SCHEDULE_DAYS);
                if (options.date.ToUniversalTime() > maxDate)
                    throw new ArgumentException($"date must be within {MAX_SCHEDULE_DAYS} days");
            }

            var payload = options.ToJson();
            JsBridge.ScheduleNotificationV2(payload);
        }

        /// <summary>
        /// Unschedules a previously scheduled notification by its unique key.
        /// </summary>
        /// <param name="uniqueKey">The unique identifier of the notification to remove.</param>
        public void UnscheduleNotification(string uniqueKey)
        {
            JsBridge.UnscheduleNotificationV2(uniqueKey);
        }

        /// <summary>
        /// Retrieves all currently scheduled rich notifications.
        /// </summary>
        /// <returns>A list of <see cref="Options"/> objects representing scheduled notifications.</returns>
        internal List<Options> GetNotifications()
        {
            return JsBridge
                .GetNotificationsV2()
                .Select(Options.FromJson)
                .ToList();
        }

        /// <summary>
        /// Represents the configuration options for a rich notification, including text, imagery, timing, and data payload.
        /// Implements Unity’s <see cref="ISerializationCallbackReceiver"/> to manage JSON serialization of complex fields.
        /// </summary>
        [Serializable]
        public class Options : ISerializationCallbackReceiver
        {
            [SerializeField] private string scheduledAt;
            [SerializeField] private string entryPayload;
            [SerializeField] private string priority;

            /// <summary>
            /// The main body text of the notification.
            /// </summary>
            public string body;

            /// <summary>
            /// The text displayed on the call-to-action (CTA) button.
            /// Must be 1-25 characters.
            /// </summary>
            public string ctaText;

            /// <summary>
            /// The reference to a pre-approved image to display with the notification.
            /// </summary>
            public string imageReference;

            /// <summary>
            /// The severity or importance level of this notification.
            /// </summary>
            [NonSerialized]
            public Severity notificationPriority = Severity.Low;

            /// <summary>
            /// The unique identifier of the notification. Required.
            /// Notifications can be rescheduled or unscheduled using this identifier.
            /// </summary>
            public string identifier;

            /// <summary>
            /// Private backing field for entry payload data.
            /// </summary>
            private Dictionary<string, object> _entryPayloadData = new();

            /// <summary>
            /// Additional custom key-value data payload associated with the notification.
            /// </summary>
            public Dictionary<string, object> entryPayloadData
            {
                get => _entryPayloadData;
                set => _entryPayloadData = value ?? new Dictionary<string, object>();
            }

            /// <summary>
            /// The scheduled date and time of the notification.
            /// Must be within 7 days. Mutually exclusive with <see cref="scheduledInDays"/>.
            /// </summary>
            [NonSerialized]
            public DateTime date;

            /// <summary>
            /// Schedule the notification in a specified number of days (1-7).
            /// Uses fuzzy timing without an exact time.
            /// Mutually exclusive with <see cref="date"/>.
            /// </summary>
            public int? scheduledInDays;

            /// <summary>
            /// Called automatically after deserialization.
            /// Converts string-based JSON fields into structured objects.
            /// </summary>
            public void OnAfterDeserialize()
            {
                // Parse the scheduled date.
                date = DateTimeExtensions.FromJsString(scheduledAt);

                // Deserialize the embedded data payload.
                var newData = Convert.FromString<Dictionary<string, object>>(entryPayload);
                _entryPayloadData.Clear();

                foreach (var item in newData)
                {
                    _entryPayloadData.Add(item.Key, item.Value);
                }

                // Convert stored string back to Severity enum.
                notificationPriority = (Severity)Enum.Parse(typeof(Severity), priority, true);
            }

            /// <summary>
            /// Called automatically before serialization.
            /// Converts structured data into serializable JSON strings.
            /// </summary>
            public void OnBeforeSerialize()
            {
                if (scheduledInDays.HasValue)
                {
                    // When using scheduledInDays, don't serialize scheduledAt
                    scheduledAt = null;
                }
                else
                {
                    scheduledAt = date.ToJsString();
                }
                entryPayload = Convert.ToString(_entryPayloadData);
                priority = notificationPriority.ToString().ToLower();
            }

            /// <summary>
            /// Serializes this notification options to JSON for the bridge.
            /// </summary>
            /// <returns>JSON string representation of the notification options.</returns>
            internal string ToJson()
            {
                var jsonObj = new Dictionary<string, object>
                {
                    ["body"] = body,
                    ["ctaText"] = ctaText,
                    ["priority"] = notificationPriority.ToString().ToLower(),
                    ["identifier"] = identifier
                };

                // Add optional imageReference
                if (!string.IsNullOrEmpty(imageReference))
                {
                    jsonObj["imageReference"] = imageReference;
                }

                // Add entryPayload if present
                if (_entryPayloadData != null && _entryPayloadData.Count > 0)
                {
                    jsonObj["entryPayload"] = _entryPayloadData;
                }

                // Add scheduling - mutually exclusive
                if (scheduledInDays.HasValue)
                {
                    jsonObj["scheduledInDays"] = scheduledInDays.Value;
                }
                else
                {
                    jsonObj["scheduledAt"] = date.ToJsString();
                }

                return JsonConvert.SerializeObject(jsonObj);
            }

            /// <summary>
            /// Deserializes a JSON string (created by ToJson) back into an Options object.
            /// </summary>
            internal static Options FromJson(string json)
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (dict == null) return new Options();

                var options = new Options
                {
                    body = dict.TryGetValue("body", out var b) ? b?.ToString() : null,
                    ctaText = dict.TryGetValue("ctaText", out var cta) ? cta?.ToString() : null,
                    imageReference = dict.TryGetValue("imageReference", out var img) ? img?.ToString() : null,
                    identifier = dict.TryGetValue("identifier", out var id) ? id?.ToString() : null
                };

                // Parse priority
                if (dict.TryGetValue("priority", out var priority) && priority != null)
                {
                    if (Enum.TryParse<Severity>(priority.ToString(), true, out var sev))
                    {
                        options.notificationPriority = sev;
                    }
                }

                // Parse date - JsonConvert may auto-convert to DateTime
                if (dict.TryGetValue("scheduledAt", out var scheduledAt) && scheduledAt != null)
                {
                    if (scheduledAt is DateTime dt)
                    {
                        options.date = dt;
                    }
                    else
                    {
                        options.date = DateTimeExtensions.FromJsString(scheduledAt.ToString());
                    }
                }

                // Parse scheduledInDays
                if (dict.TryGetValue("scheduledInDays", out var days) && days != null)
                {
                    if (int.TryParse(days.ToString(), out var daysInt))
                    {
                        options.scheduledInDays = daysInt;
                    }
                }

                // Parse entryPayload
                if (dict.TryGetValue("entryPayload", out var payload) && payload != null)
                {
                    options._entryPayloadData = Convert.FromString<Dictionary<string, object>>(
                        JsonConvert.SerializeObject(payload));
                }

                return options;
            }
        }
    }
}
