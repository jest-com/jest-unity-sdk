using System.Collections.Generic;
using com.jest.sdk;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace com.jest.demo
{

    public class LoginController : MonoBehaviour
    {

        [SerializeField] private Button m_loginButton;
        [SerializeField] public TMP_InputField m_payload;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GameManager.Instance.OnGameStateChanged += LoginController_OnGameStateChanged;
        }

        private void LoginController_OnGameStateChanged(object sender, System.EventArgs e)
        {
            m_loginButton.interactable = !JestSDK.Instance.Player.isRegistered;
        }

        public void OnLoginPressed()
        {
            OnLoginPressed(m_payload.text);
        }

        private void OnLoginPressed(string payload)
        {
            Dictionary<string, object> payloadDictionary;
            try
            {
                payloadDictionary = Convert.FromString<Dictionary<string, object>>(payload);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                payloadDictionary = new Dictionary<string, object>();
            }
            GameManager.Instance.OnLoginAction(payloadDictionary);
        }
    }
}