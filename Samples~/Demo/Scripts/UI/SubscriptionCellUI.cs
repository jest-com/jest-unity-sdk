using System;
using TMPro;
using UnityEngine;
using com.jest.sdk;
using static com.jest.sdk.Payment;
using UnityEngine.UI;

namespace com.jest.demo
{

    public class SubscriptionCellUI : MonoBehaviour
    {

        [SerializeField] public TextMeshProUGUI m_nameText;
        [SerializeField] public TextMeshProUGUI m_descriptionText;
        [SerializeField] public TextMeshProUGUI m_priceText;
        [SerializeField] public TextMeshProUGUI m_statusText;
        [SerializeField] public Button m_subscribeButton;
        [SerializeField] public Button m_cancelButton;


        private Payment.SubscriptionData m_subscription;
        private ISubscriptionController m_subscriptionController;


        public void Setup(Payment.SubscriptionData subscription, ISubscriptionController subscriptionController)
        {
            m_subscription = subscription;
            m_subscriptionController = subscriptionController;

            if (m_nameText != null)
            {
                m_nameText.text = m_subscription.DisplayName;
            }
            if (m_descriptionText != null)
            {
                m_descriptionText.text = m_subscription.DisplayDescription;
            }
            if (m_priceText != null)
            {
                m_priceText.text = $"{m_subscription.Price:F2} {m_subscription.Currency} / {m_subscription.BillingPeriod}";
            }
            if (m_statusText != null)
            {
                m_statusText.text = m_subscription.Status;
            }

            bool isActive = string.Equals(m_subscription.Status, "active", StringComparison.OrdinalIgnoreCase);
            if (m_subscribeButton != null)
            {
                m_subscribeButton.interactable = !isActive;
            }
            if (m_cancelButton != null)
            {
                m_cancelButton.interactable = isActive;
            }
        }


        public void Subscribe()
        {
            m_subscriptionController.BeginSubscription(m_subscription.Sku);
        }


        public void Cancel()
        {
            m_subscriptionController.CancelSubscription(m_subscription.Sku);
        }
    }
}
