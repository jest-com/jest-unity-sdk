using System.Collections.Generic;
using com.jest.sdk;
using TMPro;
using UnityEngine;

namespace com.jest.demo
{
    public class RegistrationLeaseController : MonoBehaviour
    {
        [Header("Reserve Login Message")]
        [SerializeField] private TMP_InputField m_messageInput;
        [SerializeField] private TMP_InputField m_keywordsInput;
        [SerializeField] private TMP_InputField m_entryPayloadInput;

        [Header("Output")]
        [SerializeField] private TextMeshProUGUI m_resultText;

        private Internal.LoginReservation _currentReservation;

        public async void ReserveLoginMessage()
        {
            string message = m_messageInput.text;

            if (string.IsNullOrEmpty(message))
            {
                UIManager.Instance.m_toastUI.ShowToast("Message is required");
                return;
            }

            var options = new Internal.ReserveLoginMessageOptions
            {
                message = message
            };

            // Parse keywords if provided (comma-separated)
            string keywordsText = m_keywordsInput?.text;
            if (!string.IsNullOrEmpty(keywordsText))
            {
                var keywords = new List<string>();
                foreach (var keyword in keywordsText.Split(','))
                {
                    var trimmed = keyword.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        keywords.Add(trimmed);
                    }
                }
                if (keywords.Count > 0)
                {
                    options.keywords = keywords;
                }
            }

            // Parse entry payload if provided
            string entryPayloadJson = m_entryPayloadInput?.text;
            if (!string.IsNullOrEmpty(entryPayloadJson))
            {
                try
                {
                    options.entryPayload = Convert.FromString<Dictionary<string, object>>(entryPayloadJson);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                    UIManager.Instance.m_toastUI.ShowToast("Invalid entry payload JSON");
                    return;
                }
            }

            UIManager.Instance.ShowLoadingSpinner();
            try
            {
                var task = JestSDK.Instance.Internal.ReserveLoginMessageAsync(options);
                await task;

                if (task.IsCompleted)
                {
                    var response = task.Result;
                    if (response.reservation != null)
                    {
                        _currentReservation = response.reservation;
                        m_resultText.text = $"Reserved!\nID: {response.reservation.id}";
                        UIManager.Instance.m_toastUI.ShowToast("Login message reserved");
                    }
                    else if (!string.IsNullOrEmpty(response.error))
                    {
                        m_resultText.text = $"Error: {response.error}";
                        UIManager.Instance.m_toastUI.ShowToast("Failed: " + response.error);
                    }
                }
            }
            catch (System.Exception e)
            {
                m_resultText.text = $"Error: {e.Message}";
                UIManager.Instance.m_toastUI.ShowToast("Failed: " + e.Message);
            }
            UIManager.Instance.HideLoadingSpinner();
        }

        public void SendReservedLoginMessage()
        {
            if (_currentReservation == null)
            {
                UIManager.Instance.m_toastUI.ShowToast("No reservation - reserve first");
                return;
            }

            try
            {
                JestSDK.Instance.Internal.SendReservedLoginMessage(_currentReservation);
                m_resultText.text = "Message sent!";
                UIManager.Instance.m_toastUI.ShowToast("Reserved login message sent");
                _currentReservation = null;
            }
            catch (System.Exception e)
            {
                m_resultText.text = $"Error: {e.Message}";
                UIManager.Instance.m_toastUI.ShowToast("Failed: " + e.Message);
            }
        }
    }
}
