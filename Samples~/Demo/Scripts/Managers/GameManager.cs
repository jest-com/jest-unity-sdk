using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using com.jest.sdk;
using System;



namespace com.jest.demo
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public event EventHandler<EventArgs> OnGameStateChanged;


        public void TriggerGameStateChangeEvent()
        {
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Awake()
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
            InitJestSDK();
        }

        private void InitJestSDK()
        {
            ShowLoading();
            JestSDK.Instance.Init().ContinueWith(t =>
            {
                Debug.Log("InitJestSDK Success");
                TriggerGameStateChangeEvent();
                HideLoading();
            });
        }

        private void ShowLoading()
        {
            UIManager.Instance?.HidePanel();
            UIManager.Instance?.ShowLoadingSpinner();
        }

        private void HideLoading()
        {
            UIManager.Instance?.HideLoadingSpinner();
            UIManager.Instance?.ShowPanel();
        }



        internal void OnLoginAction(Dictionary<string, object> payload)
        {
            JestSDK.Instance.Login(payload);
            TriggerGameStateChangeEvent();
        }

    }
}