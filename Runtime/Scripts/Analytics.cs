using System.Collections.Generic;
using UnityEngine;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides analytics functionality for tracking and retrieving events.
    /// </summary>
    public class Analytics
    {
        internal Analytics() { }

        /// <summary>
        /// Captures an analytics event with structured properties.
        /// </summary>
        /// <typeparam name="T">The type of the properties object</typeparam>
        /// <param name="name">The name of the event</param>
        /// <param name="properties">The properties associated with the event</param>
        public void CaptureEvent<T>(string name, T properties) where T : struct
        {
            JsBridge.CaptureEvent(name, JsonUtility.ToJson(properties));
        }

        /// <summary>
        /// Captures an analytics event with dictionary properties.
        /// </summary>
        /// <param name="name">The name of the event</param>
        /// <param name="properties">The properties associated with the event</param>
        public void CaptureEvent(string name, Dictionary<string, object> properties)
        {
            JsBridge.CaptureEvent(name, Convert.ToString(properties));
        }

        internal T GetEvent<T>(string name) where T : struct
        {
            return JsonUtility.FromJson<T>(JsBridge.GetEvent(name));
        }

        internal Dictionary<string, object> GetEvent(string name)
        {
            return Convert.FromString<Dictionary<string, object>>(JsBridge.GetEvent(name));
        }
    }
}
