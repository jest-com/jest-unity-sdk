using com.jest.sdk;
using TMPro;
using UnityEngine;

namespace com.jest.demo
{
    public class GameStateViewController : MonoBehaviour
    {

        [SerializeField] public TextMeshProUGUI m_playerStateText;

        private void Start()
        {
            GameManager.Instance.OnGameStateChanged += GameStateController_OnGameStateChanged;
        }

        private void GameStateController_OnGameStateChanged(object sender, System.EventArgs e)
        {
            m_playerStateText.text = JestSDK.Instance.Player.GetPlayerData();
        }

        public void CopyPlayerState()
        {
            GUIUtility.systemCopyBuffer = m_playerStateText.text;
            UIManager.Instance.m_toastUI.ShowToast("Copied!");
        }
    }
}