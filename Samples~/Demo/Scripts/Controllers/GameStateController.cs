using com.jest.sdk;
using TMPro;
using UnityEngine;

namespace com.jest.demo
{

    public class GameStateController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField m_keyTextInput;
        [SerializeField] private TMP_InputField m_valueTextInput;
        [SerializeField] private TMP_InputField m_deleteKeyTextInput;

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

        public void DeletePlayerData()
        {
            string key = m_deleteKeyTextInput.text;

            if (string.IsNullOrEmpty(key))
            {
                UIManager.Instance.m_toastUI.ShowToast("Key is empty");
                return;
            }

            JestSDK.Instance.Player.Delete(key);
            UIManager.Instance.m_toastUI.ShowToast("Player Data Deleted");
            GameManager.Instance.TriggerGameStateChangeEvent();
            m_deleteKeyTextInput.text = default;
        }

        public async void FlushPlayerData()
        {
            UIManager.Instance.ShowLoadingSpinner();
            try
            {
                await JestSDK.Instance.Player.Flush();
                UIManager.Instance.m_toastUI.ShowToast("Player Data Flushed");
            }
            catch (System.Exception e)
            {
                UIManager.Instance.m_toastUI.ShowToast("Flush failed: " + e.Message);
            }
            UIManager.Instance.HideLoadingSpinner();
        }

        public async void GetSignedPlayerData()
        {
            UIManager.Instance.ShowLoadingSpinner();
            try
            {
                var task = JestSDK.Instance.Player.GetSigned();
                await task;

                if (task.IsCompleted)
                {
                    var response = task.Result;
                    UIManager.Instance.m_toastUI.ShowToast($"Signed: {response.player.playerId}, registered: {response.player.registered}");
                }
            }
            catch (System.Exception e)
            {
                UIManager.Instance.m_toastUI.ShowToast("GetSigned failed: " + e.Message);
            }
            UIManager.Instance.HideLoadingSpinner();
        }
    }
}
