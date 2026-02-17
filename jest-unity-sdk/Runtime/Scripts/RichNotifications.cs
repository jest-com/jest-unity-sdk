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
            High,

            /// <summary>Critical priority notification requiring immediate attention.</summary>
            Critical
        }

        /// <summary>
        /// Internal constructor to prevent external instantiation.
        /// </summary>
        internal RichNotifications() { }

        /// <summary>
        /// Schedules a new rich notification using the specified options.
        /// </summary>
        /// <param name="options">The notification configuration options.</param>
        public void ScheduleNotification(Options options)
        {
            string payload = JsonUtility.ToJson(options);
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
                .Select(n => JsonUtility.FromJson<Options>(n))
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
            /// The title or plain text displayed in the notification.
            /// </summary>
            public string plainText;

            /// <summary>
            /// The main body text of the notification.
            /// </summary>
            public string body;

            /// <summary>
            /// The text displayed on the call-to-action (CTA) button.
            /// </summary>
            public string ctaText;

            /// <summary>
            /// The URL or path to an image displayed with the notification.
            /// </summary>
            public string image;

            /// <summary>
            /// The severity or importance level of this notification.
            /// </summary>
            [NonSerialized]
            public Severity notificationPriority = Severity.Low;

            /// <summary>
            /// The unique identifier of the notification.
            /// </summary>
            public string identifier;

            /// <summary>
            /// Additional custom key-value data payload associated with the notification.
            /// </summary>
            public readonly Dictionary<string, object> data = new();

            /// <summary>
            /// The scheduled date and time of the notification.
            /// </summary>
            [NonSerialized]
            public DateTime date;

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
                data.Clear();

                foreach (var item in newData)
                {
                    data.Add(item.Key, item.Value);
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
                scheduledAt = date.ToJsString();
                entryPayload = Convert.ToString(data);
                priority = notificationPriority.ToString().ToLower();
            }
        }
    }
}
