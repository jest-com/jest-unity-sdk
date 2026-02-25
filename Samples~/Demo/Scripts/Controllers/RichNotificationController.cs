using System;
using System.Collections.Generic;
using System.Linq;
using com.jest.sdk;
using TMPro;
using UnityEngine;


namespace com.jest.demo
{

    public class RichNotificationController : MonoBehaviour
    {

        [SerializeField] private TMP_InputField m_notificationBodyInput;
        [SerializeField] private TMP_InputField m_fallbackBodyInput;
        [SerializeField] private TMP_InputField m_callToActionInput;
        [SerializeField] private TMP_InputField m_imageBase64Input;
        [SerializeField] private TMP_InputField m_minutesFromNowInput;
        [SerializeField] private TMP_InputField m_uniqueKeyInput;
        [SerializeField] private TMP_InputField m_entryPayloadInput;
        [SerializeField] private TMP_Dropdown m_priorityDropDown;
        [SerializeField] private TMP_InputField m_unscheduleUniqueKeyInput;
        RichNotifications.Severity m_selected = RichNotifications.Severity.Low;

        private void Start()
        {
            var names = Enum.GetNames(typeof(RichNotifications.Severity)).ToList();

            // Optionally convert to lowercase or use EnumMember values manually
            for (int i = 0; i < names.Count; i++)
            {
                names[i] = names[i].ToLower();
            }

            m_priorityDropDown.ClearOptions();
            m_priorityDropDown.AddOptions(names);

            m_priorityDropDown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private void OnDropdownValueChanged(int index)
        {
            m_selected = (RichNotifications.Severity)index;
        }

        public void UseRedImage()
        {
            m_imageBase64Input.text = "data:image/png;base64,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";
        }

        public void ScheduleNotification()
        {

            if (!JestSDK.Instance.Player.isRegistered)
            {
                UIManager.Instance.m_toastUI.ShowToast("Login first to schedule the notification.");
                return;
            }

            string body = m_notificationBodyInput.text;
            string fallback = m_fallbackBodyInput.text;
            string callToAction = m_callToActionInput.text;
            string imageString = m_imageBase64Input.text;
            string timeString = m_minutesFromNowInput.text;
            string uniqueKey = m_uniqueKeyInput.text;
            string entryPayloadJson = m_entryPayloadInput.text;



            if (string.IsNullOrEmpty(body))
            {
                UIManager.Instance.m_toastUI.ShowToast("Notification body is empty");
                return;
            }

            if (string.IsNullOrEmpty(fallback))
            {
                UIManager.Instance.m_toastUI.ShowToast("Fallback body is empty");
                return;
            }

            if (string.IsNullOrEmpty(callToAction))
            {
                UIManager.Instance.m_toastUI.ShowToast("Call to action text is empty");
                return;
            }

            if (string.IsNullOrEmpty(timeString))
            {
                UIManager.Instance.m_toastUI.ShowToast("Minutes field is empty");
                return;
            }

            if (string.IsNullOrEmpty(uniqueKey))
            {
                UIManager.Instance.m_toastUI.ShowToast("Unique key is empty");
                return;
            }



            RichNotifications.Options options = new RichNotifications.Options
            {
                plainText = body,
                body = fallback,
                ctaText = callToAction,
                imageReference = imageString,
                notificationPriority = m_selected,
                identifier = uniqueKey,
                date = System.DateTime.Now.AddMinutes(float.Parse(timeString)),
            };


            if (!string.IsNullOrEmpty(entryPayloadJson))
            {
                Dictionary<string, object> payloadDataObject = null;

                try
                {
                    payloadDataObject = com.jest.sdk.Convert.FromString<Dictionary<string, object>>(entryPayloadJson);

                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    payloadDataObject = null;
                }
                foreach (var kvp in payloadDataObject)
                {
                    options.entryPayloadData[kvp.Key] = kvp.Value;
                }

            }

            JestSDK.Instance?.RichNotifications?.ScheduleNotification(options);
            UIManager.Instance.m_toastUI.ShowToast("Rich Notification scheduled successfully.");


            m_notificationBodyInput.text = "";
            m_fallbackBodyInput.text = "";
            m_callToActionInput.text = "";
            m_imageBase64Input.text = "";
            m_minutesFromNowInput.text = "";
            m_uniqueKeyInput.text = "";

            GameManager.Instance.TriggerGameStateChangeEvent();
        }


        public void UnscheduleNotification()
        {
            if (!JestSDK.Instance.Player.isRegistered)
            {
                UIManager.Instance.m_toastUI.ShowToast("Login first to unschedule the notification.");
                return;
            }
            string uniqueKey = m_unscheduleUniqueKeyInput.text;
            if (string.IsNullOrEmpty(uniqueKey))
            {
                UIManager.Instance.m_toastUI.ShowToast("Unique key is empty");
                return;
            }
            JestSDK.Instance.RichNotifications.UnscheduleNotification(uniqueKey);
            UIManager.Instance.m_toastUI.ShowToast("Rich Notification unscheduled successfully.");
            m_unscheduleUniqueKeyInput.text = "";
            GameManager.Instance.TriggerGameStateChangeEvent();
        }

    }
}