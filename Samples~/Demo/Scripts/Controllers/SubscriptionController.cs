using UnityEngine;
using UnityEngine.UI;
using com.jest.sdk;
using static com.jest.sdk.Payment;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace com.jest.demo
{

    public interface ISubscriptionController
    {
        void BeginSubscription(string subscriptionSku);
        void CancelSubscription(string subscriptionSku);
    }

    public class SubscriptionController : MonoBehaviour, ISubscriptionController
    {
        [SerializeField] private Transform m_subscriptionCellPrefab;
        [SerializeField] private Transform m_subscriptionCellsContainer;
        [SerializeField] private RectTransform m_mainContainer;

        private List<SubscriptionData> m_subscriptionsList;

        public void GetSubscriptions()
        {
            UIManager.Instance.ShowLoadingSpinner();
            JestSDK.Instance.Payment.GetSubscriptions().ContinueWith(t => {
                try
                {
                    if (t.IsFaulted)
                    {
                        UIManager.Instance.m_toastUI.ShowToast("Get Subscriptions Failed: " + t.Exception.Message);
                    }
                    else
                    {
                        m_subscriptionsList = t.Result?.Subscriptions ?? new List<SubscriptionData>();
                        UpdateSubscriptionsInUI();
                        if (m_subscriptionCellsContainer != null)
                        {
                            StartCoroutine(RefreshLayout(m_subscriptionCellsContainer.GetComponent<RectTransform>()));
                        }
                        UIManager.Instance.m_toastUI.ShowToast($"Subscriptions loaded: {m_subscriptionsList.Count}");
                    }
                }
                catch (Exception e)
                {
                    UIManager.Instance.m_toastUI.ShowToast("Get Subscriptions Failed: " + e.Message);
                }
                UIManager.Instance.HideLoadingSpinner();
            });
        }

        private IEnumerator RefreshLayout(RectTransform parentContainer)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentContainer);
            if (m_mainContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_mainContainer);
            }
            Canvas.ForceUpdateCanvases();
        }

        private void RefreshSubscriptionsIfRendered()
        {
            if (m_subscriptionCellsContainer != null && m_subscriptionCellPrefab != null)
            {
                GetSubscriptions();
            }
        }

        private void UpdateSubscriptionsInUI()
        {
            if (m_subscriptionCellsContainer == null || m_subscriptionCellPrefab == null)
            {
                return;
            }

            foreach (Transform child in m_subscriptionCellsContainer)
            {
                Destroy(child.gameObject);
            }

            if (m_subscriptionsList == null || m_subscriptionsList.Count == 0)
            {
                return;
            }

            foreach (SubscriptionData subscription in m_subscriptionsList)
            {
                SubscriptionCellUI cellUI = Instantiate(m_subscriptionCellPrefab, m_subscriptionCellsContainer)
                    .GetComponent<SubscriptionCellUI>();
                cellUI.Setup(subscription, this);
            }
        }

        public void BeginSubscription(string subscriptionSku)
        {
            UIManager.Instance.ShowLoadingSpinner();
            JestSDK.Instance.Payment.BeginSubscription(subscriptionSku).ContinueWith(t => {
                try
                {
                    if (t.IsFaulted)
                    {
                        UIManager.Instance.m_toastUI.ShowToast("Subscription Failed: " + t.Exception.Message);
                    }
                    else if (t.Result.Result == "success")
                    {
                        UIManager.Instance.m_toastUI.ShowToast("Subscribed to: " + t.Result.Subscription.DisplayName);
                        RefreshSubscriptionsIfRendered();
                    }
                    else if (t.Result.Result == "cancel")
                    {
                        UIManager.Instance.m_toastUI.ShowToast("Subscription cancelled");
                    }
                    else
                    {
                        UIManager.Instance.m_toastUI.ShowToast("Subscription Failed: " + t.Result.Error);
                    }
                }
                catch (Exception e)
                {
                    UIManager.Instance.m_toastUI.ShowToast("Subscription failed: " + e.Message);
                }
                UIManager.Instance.HideLoadingSpinner();
            });
        }

        public void CancelSubscription(string subscriptionSku)
        {
            UIManager.Instance.ShowLoadingSpinner();
            JestSDK.Instance.Payment.CancelSubscription(subscriptionSku).ContinueWith(t => {
                try
                {
                    if (t.IsFaulted)
                    {
                        UIManager.Instance.m_toastUI.ShowToast("Cancel Subscription Failed: " + t.Exception.Message);
                    }
                    else if (t.Result.Result == "success")
                    {
                        UIManager.Instance.m_toastUI.ShowToast("Subscription cancelled");
                        RefreshSubscriptionsIfRendered();
                    }
                    else if (t.Result.Result == "cancel")
                    {
                        UIManager.Instance.m_toastUI.ShowToast("Cancellation dismissed");
                    }
                    else
                    {
                        UIManager.Instance.m_toastUI.ShowToast("Cancel Subscription Failed: " + t.Result.Error);
                    }
                }
                catch (Exception e)
                {
                    UIManager.Instance.m_toastUI.ShowToast("Cancel Subscription failed: " + e.Message);
                }
                UIManager.Instance.HideLoadingSpinner();
            });
        }
    }
}
