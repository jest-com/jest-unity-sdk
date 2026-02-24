using com.jest.sdk;
using TMPro;
using UnityEngine;

namespace com.jest.demo
{
    public class FeatureFlagsController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField m_flagKeyInput;
        [SerializeField] private TextMeshProUGUI m_resultText;

        public async void GetFeatureFlag()
        {
            string key = m_flagKeyInput.text;

            if (string.IsNullOrEmpty(key))
            {
                UIManager.Instance.m_toastUI.ShowToast("Flag key is required");
                return;
            }

            UIManager.Instance.ShowLoadingSpinner();
            try
            {
                var task = JestSDK.Instance.GetFeatureFlag(key);
                await task;

                if (task.IsCompleted)
                {
                    string value = task.Result;
                    if (string.IsNullOrEmpty(value))
                    {
                        m_resultText.text = $"Flag \"{key}\" = undefined (not found)";
                    }
                    else
                    {
                        m_resultText.text = $"Flag \"{key}\" = \"{value}\"";
                    }
                    UIManager.Instance.m_toastUI.ShowToast("Feature flag retrieved");
                }
            }
            catch (System.Exception e)
            {
                m_resultText.text = $"Error: {e.Message}";
                UIManager.Instance.m_toastUI.ShowToast("Failed: " + e.Message);
            }
            UIManager.Instance.HideLoadingSpinner();
        }
    }
}
