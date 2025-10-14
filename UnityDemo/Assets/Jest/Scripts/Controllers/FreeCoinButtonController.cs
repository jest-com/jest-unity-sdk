using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreeCoinButtonController : MonoBehaviour
{
    private Button m_freeCoinsButton;

    private void Awake()
    {
        m_freeCoinsButton = GetComponent<Button>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_freeCoinsButton.onClick.AddListener(FreeCoins);
        StartCoroutine(UpdateFreeCoinsButtonState());
    }

    private IEnumerator UpdateFreeCoinsButtonState()
    {
        while (true)
        {
            if (FreeCoinsManager.Instance.IsRewardAvailable())
            {
                m_freeCoinsButton.interactable = true;
                TMP_Text tmpText = m_freeCoinsButton.GetComponentInChildren<TMP_Text>();
                tmpText.text = "Get Free Coins!";
            }
            else
            {
                m_freeCoinsButton.interactable = false;
                TMP_Text tmpText = m_freeCoinsButton.GetComponentInChildren<TMP_Text>();
                tmpText.text = FreeCoinsManager.Instance.GetTimeRemainingString();
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void FreeCoins()
    {
        if (FreeCoinsManager.Instance.IsRewardAvailable())
        {
            FreeCoinsManager.Instance.ClaimReward();
        }
    }

}
