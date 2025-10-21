using com.jest.sdk;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationController : MonoBehaviour
{

    [SerializeField] private TMP_InputField m_messageTextInput;
    [SerializeField] private TMP_InputField m_timeTextInput;
    [SerializeField] private Toggle m_toggleInput;
    [SerializeField] private TMP_InputField m_deduplicationTextInput;



    public void ScheduleNotification()
    {
        if(!JestSDK.Instance.Player.isRegistered)
        {
            UIManager.Instance.m_toastUI.ShowToast("Login first to schedule the notification.");
            return;
        }
        string message = m_messageTextInput.text;
        string timeString = m_timeTextInput.text;
        bool sendPush = m_toggleInput.isOn;
        string deduplicationKey = m_deduplicationTextInput.text;


        if (string.IsNullOrEmpty(message))
        {
            UIManager.Instance.m_toastUI.ShowToast("Message is empty");
            return;
        }

        if (string.IsNullOrEmpty(timeString))
        {
            UIManager.Instance.m_toastUI.ShowToast("Time is empty");
            return;
        }

        if (string.IsNullOrEmpty(deduplicationKey))
        {
            deduplicationKey = SharedUtils.GetUnixTime().ToString();
        }

        JestSDK.Instance?.Notifications?.ScheduleNotification(new Notifications.Options
        {
            message = message,
            date = System.DateTime.Now.AddMinutes(float.Parse(timeString)),
            attemptPushNotification = sendPush,
            deduplicationKey = deduplicationKey
        });

        UIManager.Instance.m_toastUI.ShowToast("Notification scheduled successfully.");


        m_messageTextInput.text = default;
        m_timeTextInput.text = default;
        m_deduplicationTextInput.text = default;
        m_toggleInput.isOn = default;

        GameManager.Instance.TriggerGameStateChangeEvent();


    }


}
