using com.jest.sdk;
using UnityEngine;

namespace com.jest.demo
{
    public class LegalController : MonoBehaviour
    {
        public void OpenPrivacyPolicy()
        {
            JestSDK.Instance.OpenPrivacyPolicy();
            UIManager.Instance.m_toastUI.ShowToast("Opening Privacy Policy...");
        }

        public void OpenTermsOfService()
        {
            JestSDK.Instance.OpenTermsOfService();
            UIManager.Instance.m_toastUI.ShowToast("Opening Terms of Service...");
        }

        public void OpenCopyright()
        {
            JestSDK.Instance.OpenCopyright();
            UIManager.Instance.m_toastUI.ShowToast("Opening Copyright...");
        }
    }
}
