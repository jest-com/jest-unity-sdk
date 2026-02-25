using com.jest.sdk;
using UnityEngine;

namespace com.jest.demo
{
    public class DebugRegisterController : MonoBehaviour
    {
        public void OnDebugRegisterPressed()
        {
            JestSDK.Instance.DebugRegister();
            UIManager.Instance.m_toastUI.ShowToast("Debug register initiated...");
        }
    }
}
