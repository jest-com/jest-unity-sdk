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


        private Payment.Product m_product;
        private IShopController m_shopController;


        public void Setup(Payment.Product product, IShopController shopController)
        {
            m_product = product;
            m_shopController = shopController;
            m_productNameText.text = m_product.name;
            m_productDescriptionText.text = m_product.description;
        }


        public void Purchase()
        {

            m_shopController.Purchase(m_product.sku);

        }
    }
}