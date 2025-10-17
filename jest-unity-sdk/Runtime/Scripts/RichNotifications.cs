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

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Severity
        {
            [EnumMember(Value = "low")]
            Low,

            [EnumMember(Value = "medium")]
            Medium,

            [EnumMember(Value = "high")]
            High,

            [EnumMember(Value = "critical")]
            Critical
        }



        internal RichNotifications() { }

        public void ScheduleNotification(Options options)
        {
            string payload = JsonUtility.ToJson(options);
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

            [SerializeField] private string dateString;
            [SerializeField] private string dataString;


            public string plainText;
            public string body;
            public string ctaText;
            public string image; // optional
            public Severity severity = Severity.Low;
            public string identifier;

            public readonly Dictionary<string, object> data = new();
            [System.NonSerialized] public System.DateTime date;

            public void OnAfterDeserialize()
            {
                date = DateTimeExtensions.FromJsString(dateString);
                var newData = Convert.FromString<Dictionary<string, object>>(dataString);
                data.Clear();
                foreach (var item in newData)
                {
                    data.Add(item.Key, item.Value);
                }
            }

            public void OnBeforeSerialize()
            {
                dateString = date.ToJsString();
                dataString = Convert.ToString(data);
            }
        }
    }
}
