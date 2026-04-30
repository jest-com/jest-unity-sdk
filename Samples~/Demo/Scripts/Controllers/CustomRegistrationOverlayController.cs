using System;
using System.Collections.Generic;
using com.jest.sdk;
using UnityEngine;
using UnityEngine.UI;

namespace com.jest.demo
{
    /// <summary>
    /// Sample scene controller for a game-rendered registration panel.
    /// Assign the show button, panel, register button, and close button in the Inspector.
    /// </summary>
    public class CustomRegistrationOverlayController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Button m_showCustomRegistrationButton;
        [SerializeField] private GameObject m_customRegistrationPanel;
        [SerializeField] private Button m_registerButton;
        [SerializeField] private Button m_closeButton;

        [Header("Overlay Options")]
        [SerializeField] private RegistrationOverlay.Theme m_theme = RegistrationOverlay.Theme.Dark;

        private RegistrationOverlay.Handle _registrationOverlayHandle;
        private bool _listenersBound;

        public void Start()
        {
            BindButtonListeners();
            SetPanelVisible(false);
        }

        public void OnDestroy()
        {
            UnbindButtonListeners();
        }

        private void BindButtonListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            _listenersBound = true;
            if (m_showCustomRegistrationButton == null)
            {
                Debug.LogWarning("[JestSDK] CustomRegistrationOverlayController needs a show button reference.");
            }

            if (m_customRegistrationPanel == null)
            {
                Debug.LogWarning("[JestSDK] CustomRegistrationOverlayController needs a custom panel reference.");
            }

            if (m_registerButton == null)
            {
                Debug.LogWarning("[JestSDK] CustomRegistrationOverlayController needs a register button reference.");
            }

            if (m_closeButton == null)
            {
                Debug.LogWarning("[JestSDK] CustomRegistrationOverlayController needs a close button reference.");
            }

            m_showCustomRegistrationButton?.onClick.AddListener(ShowCustomRegistration);
            m_registerButton?.onClick.AddListener(OnRegisterPressed);
            m_closeButton?.onClick.AddListener(OnClosePressed);
        }

        private void UnbindButtonListeners()
        {
            if (!_listenersBound)
            {
                return;
            }

            _listenersBound = false;
            m_showCustomRegistrationButton?.onClick.RemoveListener(ShowCustomRegistration);
            m_registerButton?.onClick.RemoveListener(OnRegisterPressed);
            m_closeButton?.onClick.RemoveListener(OnClosePressed);
        }

        public void ShowCustomRegistration()
        {
            try
            {
                if (_registrationOverlayHandle != null)
                {
                    SetPanelVisible(true);
                    return;
                }

                SetPanelVisible(true);

                _registrationOverlayHandle = JestSDK.Instance.ShowRegistrationOverlay(
                    new RegistrationOverlay.Options
                    {
                        Theme = m_theme,
                        EntryPayload = new Dictionary<string, object>
                        {
                            { "source", "unity_sample_custom_registration" }
                        },
                        OnClose = HandleOverlayClosed
                    });

                _registrationOverlayHandle.OnError += HandleOverlayError;
            }
            catch (Exception e)
            {
                SetPanelVisible(false);
                _registrationOverlayHandle = null;
                Debug.LogWarning($"[JestSDK] Failed to show custom registration overlay: {e.Message}");
            }
        }

        private void OnRegisterPressed()
        {
            if (_registrationOverlayHandle == null)
            {
                return;
            }

            _registrationOverlayHandle.LoginButtonAction();
        }

        private void OnClosePressed()
        {
            if (_registrationOverlayHandle == null)
            {
                SetPanelVisible(false);
                return;
            }

            _registrationOverlayHandle.CloseButtonAction();
        }

        private void HandleOverlayClosed()
        {
            SetPanelVisible(false);
            _registrationOverlayHandle = null;
        }

        private void HandleOverlayError(Exception exception)
        {
            SetPanelVisible(false);
            _registrationOverlayHandle = null;
            Debug.LogWarning($"[JestSDK] Custom registration overlay error: {exception.Message}");
        }

        private void SetPanelVisible(bool visible)
        {
            if (m_customRegistrationPanel != null)
            {
                m_customRegistrationPanel.SetActive(visible);
            }
        }
    }
}
