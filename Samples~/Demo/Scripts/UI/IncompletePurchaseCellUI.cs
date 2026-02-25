using TMPro;
using UnityEngine;
using UnityEngine.UI;
using com.jest.sdk;
using static com.jest.sdk.Payment;


namespace com.jest.demo
{

    public class IncompleteCellUI : MonoBehaviour
    {

        [SerializeField] public TextMeshProUGUI m_productNameText;
        [SerializeField] public TextMeshProUGUI m_productDescriptionText;


        private Payment.PurchaseData m_purchase;
        private Payment.Product m_product;
        private IShopController m_shopController;


        public void Setup(Payment.Product product, Payment.PurchaseData purchase, IShopController shopController)
        {
            m_product = product;
            m_purchase = purchase;
            m_shopController = shopController;
            m_productNameText.text = m_product.name;
            m_productDescriptionText.text = m_purchase.purchaseToken;
        }


        public void CompletePurchase()
        {
            m_shopController.CompletePurchase(m_purchase.purchaseToken);
        }
    }
}