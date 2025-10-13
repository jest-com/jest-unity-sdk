using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JestSDK
{
    /// <summary>
    /// Handles scheduling and management of notifications in the Jest SDK.
    /// </summary>
    public class Notifications
    {
        internal Notifications() { }

        /// <summary>
        /// Schedules a notification with the specified options.
        /// </summary>
        /// <param name="options">The configuration options for the notification.</param>
        public void ScheduleNotification(Options options)
        {
            JsBridge.ScheduleNotification(JsonUtility.ToJson(options));
        }

        /// <summary>
        /// Retrieves all scheduled notifications.
        /// </summary>
        /// <returns>A list of scheduled notification options.</returns>
        internal List<Options> GetNotifications()
        {
            return JsBridge.GetNotifications().
                            Select(n => JsonUtility.FromJson<Options>(n)).ToList();
        }

        /// <summary>
        /// Configuration options for scheduling a notification.
        /// </summary>
        [System.Serializable]
        public class Options : ISerializationCallbackReceiver
        {
            [SerializeField] private string scheduledAt;
            [SerializeField] private string metadata;

            /// <summary>
            /// The message to display in the notification.
            /// </summary>
            public string message;

            /// <summary>
            /// Determines whether to attempt sending a push notification.
            /// </summary>
            public bool attemptPushNotification;

            /// <summary>
            /// A unique key to prevent duplicate notifications.
            /// </summary>
            public string deduplicationKey;

            /// <summary>
            /// Additional custom metadata associated with the notification.
            /// </summary>
            public readonly Dictionary<string, object> data = new();

            /// <summary>
            /// The scheduled date and time for the notification.
            /// </summary>
            [System.NonSerialized] public System.DateTime date;

            public void OnAfterDeserialize()
            {
                date = DateTimeExtensions.FromJsString(scheduledAt);
                var newData = Convert.FromString<Dictionary<string, object>>(metadata);
                data.Clear();
                foreach (var item in newData)
                {
                    data.Add(item.Key, item.Value);
                }
            }

            public void OnBeforeSerialize()
            {
                scheduledAt = date.ToJsString();
                metadata = Convert.ToString(data);
            }
        }
    }
}
