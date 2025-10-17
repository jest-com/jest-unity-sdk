using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.jest.sdk
{
    public class NotificationsV2
    {
        internal NotificationsV2() { }

        public void ScheduleNotification(Options options)
        {
            string payload = JsonUtility.ToJson(options);
            JsBridge.ScheduleNotificationV2(payload);
        }

        [System.Serializable]
        public class Options
        {
            public string plainText;
            public string body;
            public string ctaText;
            public string image; // optional, base64 data URL
            public string priority = "low";
            public string identifier;
            public EntryPayload entryPayload; // or Dictionary if appropriate
            public string scheduledAt; // must be ISO format

            public void SetDate(DateTime date)
            {
                scheduledAt = date.ToString("o");
            }
        }

        [System.Serializable]
        public class EntryPayload
        {
            public Dictionary<string, object> data = new();
        }
    }
}
