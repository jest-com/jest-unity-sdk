using UnityEngine;
using com.jest.sdk;
using static com.jest.sdk.Payment;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;



namespace com.jest.demo
{

    public interface IShopController
    {
        void CompletePurchase(string purchaseToken);
        void Purchase(string sku);
    }

    public class PaymentController : MonoBehaviour, IShopController
    {
        [SerializeField] private Transform m_productCellPrefab;
        [SerializeField] private Transform m_productCellsContainer;
        [SerializeField] private RectTransform m_mainContainer;

        [SerializeField] private Transform m_completePurchaseCellPrefab;
        [SerializeField] private Transform m_completePurchaseCellsContainer;

        private List<Product> m_productsList;
        private List<IncompletePurchase> m_incompletePurchaseList;

        public async void LoadProducts()
        {
            UIManager.Instance.ShowLoadingSpinner();
            JestSDKTask<List<Product>> productLoadTask = JestSDK.Instance.Payment.GetProducts();

            try
            {
                await productLoadTask;

                if (productLoadTask.IsCompleted)
                {
                    m_productsList = productLoadTask.Result;
                    UpdateProductsInUI();
                    StartCoroutine(RefreshLayout(m_productCellsContainer.GetComponent<RectTransform>()));
                }
                else
                {
                    UIManager.Instance.m_toastUI.ShowToast("Products loading failed: " + productLoadTask.Exception.Message);
                }
            }
            catch (Exception e)
            {
                UIManager.Instance.m_toastUI.ShowToast("Products loading failed: " + e.Message);
            }

            UIManager.Instance.HideLoadingSpinner();
        }

        private void UpdateProductsInUI()
        {
            foreach (Transform child in m_productCellsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (Product p in m_productsList)
            {
                ProductCellUI cellUI = Instantiate(m_productCellPrefab, m_productCellsContainer)
                    .GetComponent<ProductCellUI>();
                cellUI.Setup(p, this);
            }
        }

        public IEnumerator RefreshLayout(RectTransform parentContainer)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentContainer);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_mainContainer);
            Canvas.ForceUpdateCanvases();
        }

        public void LoadIncompletePurchases()
        {
            UIManager.Instance.ShowLoadingSpinner();
            JestSDKTask<List<IncompletePurchase>> incompletePurchasesTask = JestSDK.Instance.Payment.GetIncompletePurchases();
            incompletePurchasesTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    UIManager.Instance.m_toastUI.ShowToast("Incomplete purchase loading failed");
                    UIManager.Instance.HideLoadingSpinner();
                    return;
                }
                m_incompletePurchaseList = t.Result;
                UpdateIncompletePurchasesInUI();
                StartCoroutine(RefreshLayout(m_completePurchaseCellsContainer.GetComponent<RectTransform>()));
                UIManager.Instance.HideLoadingSpinner();
            });
        }

        private void UpdateIncompletePurchasesInUI()
        {
            foreach (Transform child in m_completePurchaseCellsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (IncompletePurchase purchase in m_incompletePurchaseList)
            {
                Product product = GetProductBySku(purchase.productSku);
                if (product != null)
                {
                    IncompleteCellUI cellUI = Instantiate(m_completePurchaseCellPrefab, m_completePurchaseCellsContainer)
                        .GetComponent<IncompleteCellUI>();
                    cellUI.Setup(product, purchase, this);
                }
            }
        }

        private Product GetProductBySku(string requiredSku)
        {
            return m_productsList?.FirstOrDefault(product => product.sku == requiredSku);
        }

        public async void CompletePurchase(string purchaseToken)
        {
            UIManager.Instance.ShowLoadingSpinner();
            JestSDKTask<PurchaseCompleteResult> task = JestSDK.Instance.Payment.CompletePurchase(purchaseToken);

            try
            {
                await task;

                if (task.Result.result == "success")
                {
                    UIManager.Instance.m_toastUI.ShowToast("Purchase completion success!");
                    m_incompletePurchaseList.RemoveAll(p => p.purchaseToken == purchaseToken);
                    UpdateIncompletePurchasesInUI();
                    StartCoroutine(RefreshLayout(m_completePurchaseCellsContainer.GetComponent<RectTransform>()));
                }
                else
                {
                    UIManager.Instance.m_toastUI.ShowToast("Purchase completion error: " + task.Result.error);
                }
            }
            catch (Exception e)
            {
                UIManager.Instance.m_toastUI.ShowToast("Purchase completion error: " + e.Message);
            }

            UIManager.Instance.HideLoadingSpinner();
        }

        public async void Purchase(string sku)
        {
            UIManager.Instance.ShowLoadingSpinner();
            JestSDKTask<PurchaseResult> task = JestSDK.Instance.Payment.Purchase(sku);

            try
            {
                await task;

                if (task.Result.result == "success")
                {
                    UIManager.Instance.m_toastUI.ShowToast("Purchase success!");
                }
                else
                {
                    UIManager.Instance.m_toastUI.ShowToast("Purchase failed: " + task.Result.error);
                }
            }
            catch (Exception e)
            {
                UIManager.Instance.m_toastUI.ShowToast("Purchase failed: " + e.Message);
            }

            UIManager.Instance.HideLoadingSpinner();
        }
    }
}