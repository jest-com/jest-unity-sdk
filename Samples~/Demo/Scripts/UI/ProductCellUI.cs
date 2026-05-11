using TMPro;
using UnityEngine;
using UnityEngine.UI;
using com.jest.sdk;
using static com.jest.sdk.Payment;


namespace com.jest.demo
{

    public class ProductCellUI : MonoBehaviour
    {

        [SerializeField] public TextMeshProUGUI m_productNameText;
        [SerializeField] public TextMeshProUGUI m_productDescriptionText;
        [SerializeField] public TextMeshProUGUI m_productPriceText;


        private Payment.Product m_product;
        private IShopController m_shopController;


        public void Setup(Payment.Product product, IShopController shopController)
        {
            m_product = product;
            m_shopController = shopController;
            m_productNameText.text = m_product.name;
            m_productDescriptionText.text = m_product.description;
            if (m_productPriceText != null)
            {
                m_productPriceText.text = $"{m_product.price:F2} {m_product.currency}";
            }
        }


        public void Purchase()
        {

            m_shopController.Purchase(m_product.sku);

        }
    }
}