using System.Collections.Generic;
using com.jest.sdk;
using TMPro;
using UnityEngine;


namespace com.jest.demo
{
    public class EventsController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField m_eventNameTextInput;
        [SerializeField] private TMP_InputField m_eventParamsTextInput;

        public void SendEvent()
        {
            string eventName = m_eventNameTextInput.text;
            string eventParam = m_eventParamsTextInput.text;

            if (string.IsNullOrEmpty(eventName))
            {
                UIManager.Instance.m_toastUI.ShowToast("Event name is empty");
                return;
            }

            Dictionary<string, object> properties;
            try
            {
                properties = Convert.FromString<Dictionary<string, object>>(eventParam);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                properties = new Dictionary<string, object>();
            }

            JestSDK.Instance?.Analytics.CaptureEvent(eventName, properties);
            UIManager.Instance.m_toastUI.ShowToast("Event sent successfully.");

            m_eventNameTextInput.text = string.Empty;
            m_eventParamsTextInput.text = "{\"score\":10, \"coins\":100}";

            GameManager.Instance.TriggerGameStateChangeEvent();
        }
    }
}