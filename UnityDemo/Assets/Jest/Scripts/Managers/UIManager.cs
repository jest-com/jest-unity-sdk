using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{


    public static UIManager Instance { get; private set; }




    [SerializeField] private CanvasGroup m_panelCanvasGroup;
    [SerializeField] private RectTransform m_loadingUI;
    [SerializeField] private TMP_InputField m_nameTextInput;
    [SerializeField] private TextMeshProUGUI m_coinsText;
    [SerializeField] private TextMeshProUGUI m_levelText;
    [SerializeField] private Button m_loginButton;
    [SerializeField] public ToastUI m_toastUI;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
         m_nameTextInput.onEndEdit.AddListener(OnNameEditDone);
    }

    public void EarnCoin()
    {
        GameManager.Instance.AddCoin();
    }


    public void LevelUp()
    {
        GameManager.Instance.IncrementLevel();
    }

    public void ChangeName()
    {
        m_nameTextInput.Select();
        m_nameTextInput.ActivateInputField();
    }


    void OnNameEditDone(string newText)
    {
        GameManager.Instance.SetName(newText);
    }


    public void ShowLoadingSpinner()
    {
        m_loadingUI.gameObject.SetActive(true);
    }

    public void HideLoadingSpinner()
    {
        m_loadingUI.gameObject.SetActive(false);
    }

    public void SetCoinsText(int coins)
    {
        m_coinsText.text = coins.ToString();
    }

    public void SetLevelText(int levelNumber)
    {
        m_levelText.text = levelNumber.ToString();
    }

    public void SetNameText(string name)
    {
        m_nameTextInput.text = name;
    }


    public void ShowPanel()
    {
        m_panelCanvasGroup.alpha = 1f;
        m_panelCanvasGroup.blocksRaycasts = true;
        m_panelCanvasGroup.interactable = true;
    }

    public void HidePanel()
    {
        m_panelCanvasGroup.alpha = 0f;
        m_panelCanvasGroup.blocksRaycasts = false;
        m_panelCanvasGroup.interactable = false;
    }

    public void EnableLoginButton(bool enable)
    {
        m_loginButton?.gameObject.SetActive(enable);
    }

    public void OnLoginPressed()
    {
        GameManager.Instance.OnLoginAction();
    }

    public void OnTestNotificationPressed()
    {
        m_toastUI.ShowToast("Scheduled test notification for 1 minute.");
        GameManager.Instance.ScheduleTestNotification();
    }
}
