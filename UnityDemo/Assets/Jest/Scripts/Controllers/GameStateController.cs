using com.jest.sdk;
using TMPro;
using UnityEngine;
namespace com.jest.demo
{

    public class GameStateController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField m_keyTextInput;
        [SerializeField] private TMP_InputField m_valueTextInput;



        public void AddPlayerData()
        {
            string key = m_keyTextInput.text;
            string val = m_valueTextInput.text;


            if (string.IsNullOrEmpty(key))
            {
                UIManager.Instance.m_toastUI.ShowToast("Key is empty");
                return;
            }

            if (string.IsNullOrEmpty(val))
            {
                UIManager.Instance.m_toastUI.ShowToast("Value is empty");
                return;
            }

            JestSDK.Instance.Player.Set(key, val);
            UIManager.Instance.m_toastUI.ShowToast("Player Data Added");
            GameManager.Instance.TriggerGameStateChangeEvent();
            m_keyTextInput.text = default;
            m_valueTextInput.text = default;


        }
    }
}
