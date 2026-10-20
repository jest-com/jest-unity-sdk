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
                var entrySource = JestSDK.Instance.GetEntrySource();
                Debug.Log($"Entry source: {entrySource.Type}" + (entrySource.Campaign != null ? $" (campaign {entrySource.Campaign.Id})" : ""));
                JestSDK.Instance.MarkGameLoaded();
                JestSDK.Instance.Lifecycle.OnHide += () => Debug.Log("Game hidden");
                JestSDK.Instance.Lifecycle.OnShow += () => Debug.Log("Game shown");
                JestSDK.Instance.Lifecycle.OnExitRequested += () => Debug.Log("Platform exit requested");
                Debug.Log($"Haptics capability: {JestSDK.Instance.Haptics.GetCapability()}");
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



        internal async void OnLoginAction(Dictionary<string, object> payload)
        {
            // Login resolves when the player dismisses the login popup, or immediately
            // if they are already registered. Await it so the UI refreshes once the
            // flow actually finishes rather than the moment the popup opens.
            await JestSDK.Instance.Login(payload);
            TriggerGameStateChangeEvent();
        }

    }
}