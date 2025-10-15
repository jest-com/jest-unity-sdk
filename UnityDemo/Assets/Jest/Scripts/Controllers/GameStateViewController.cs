using com.unity.jest;
using TMPro;
using UnityEngine;

public class GameStateViewController : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI m_playerStateText;

    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += GameStateController_OnGameStateChanged;   
    }

    private void GameStateController_OnGameStateChanged(object sender, System.EventArgs e)
    {
        m_playerStateText.text = JestSDK.Instance.player.GetPlayerData();
    }
}
