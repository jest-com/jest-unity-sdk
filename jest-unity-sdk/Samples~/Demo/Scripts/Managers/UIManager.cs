using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;


namespace com.jest.demo
{

    public class UIManager : MonoBehaviour
    {


        public static UIManager Instance { get; private set; }




        [SerializeField] private CanvasGroup m_panelCanvasGroup;
        [SerializeField] private RectTransform m_loadingUI;
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
        }




        public void ShowLoadingSpinner()
        {
            m_loadingUI.gameObject.SetActive(true);
        }

        public void HideLoadingSpinner()
        {
            m_loadingUI.gameObject.SetActive(false);
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

    }
}