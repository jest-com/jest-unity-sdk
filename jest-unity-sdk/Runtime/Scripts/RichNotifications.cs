using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace com.jest.sdk
{
    public class RichNotifications
    {

        public enum Severity
        {
            Low,
            Medium,
            High,
            Critical
        }



        internal RichNotifications() { }

        public void ScheduleNotification(Options options)
        {
            string payload = JsonUtility.ToJson(options);
            Debug.Log("NotificationsV2::" + payload);
            JsBridge.ScheduleNotificationV2(payload);
        }

        /// <summary>
        /// Retrieves all scheduled notifications.
        /// </summary>
        /// <returns>A list of scheduled notification options.</returns>
        internal List<Options> GetNotifications()
        {
            return JsBridge.GetNotificationsV2().Select(n => JsonUtility.FromJson<Options>(n)).ToList();
        }


        [System.Serializable]
        public class Options : ISerializationCallbackReceiver
        {

            [SerializeField] private string scheduledAt;
            [SerializeField] private string entryPayload;
            [SerializeField] private string priority;


            public string plainText;
            public string body;
            public string ctaText;
            public string image; // optional
            [System.NonSerialized] public Severity notificationPriority = Severity.Low;
            public string identifier;

            public readonly Dictionary<string, object> data = new();
            [System.NonSerialized] public System.DateTime date;

            public void OnAfterDeserialize()
            {
                date = DateTimeExtensions.FromJsString(scheduledAt);
                var newData = Convert.FromString<Dictionary<string, object>>(entryPayload);
                data.Clear();
                foreach (var item in newData)
                {
                    data.Add(item.Key, item.Value);
                }
                notificationPriority = Enum.Parse<Severity>(priority, ignoreCase: true);
            }

            public void OnBeforeSerialize()
            {
                scheduledAt = date.ToJsString();
                entryPayload = Convert.ToString(data);
                priority = notificationPriority.ToString().ToLower();
            }
        }
    }
}
