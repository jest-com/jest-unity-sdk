using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace com.jest.sdk
{
    public class Payment
    {

        internal Payment() { }

        public JestSDKTask<List<Product>> GetProducts()
        {
            var task = new JestSDKTask<List<Product>>(); // This will be the final returned task
            JestSDKTask<string> getProductsTask = JsBridge.GetProducts();
            getProductsTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }
                    string json = getProductsTask.GetResult();
                    List<Product> products = JsonConvert.DeserializeObject<List<Product>>(json);
                    task.SetResult(products);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });
            return task;
        }

        public JestSDKTask<PurchaseResult> Purchase(string sku)
        {
            var task = new JestSDKTask<PurchaseResult>(); // This will be the final returned task
            JestSDKTask<string> purchaseTask = JsBridge.BeginPurchase(sku);
            purchaseTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }
                    string json = purchaseTask.GetResult();
                    PurchaseResult result = JsonConvert.DeserializeObject<PurchaseResult>(json);
                    task.SetResult(result);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });
            return task;
        }

        public JestSDKTask<PurchaseCompleteResult> CompletePurchase(string purchaseToken)
        {
            var task = new JestSDKTask<PurchaseCompleteResult>(); // This will be the final returned task
            JestSDKTask<string> purchaseCompleteTask = JsBridge.CompletePurchase(purchaseToken);

            purchaseCompleteTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }
                    string json = purchaseCompleteTask.GetResult();
                    PurchaseCompleteResult result = JsonConvert.DeserializeObject<PurchaseCompleteResult>(json);
                    task.SetResult(result);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });
            return task;
        }

        public JestSDKTask<List<IncompletePurchase>> GetIncompletePurchases()
        {
            var task = new JestSDKTask<List<IncompletePurchase>>(); // This will be the final returned task
            JestSDKTask<string> getIncompletePurchasesTask = JsBridge.GetIncompletePurchases();
            getIncompletePurchasesTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }
                    string json = t.GetResult();
                    IncompletePurchasesResponse productsResponse = JsonConvert.DeserializeObject<IncompletePurchasesResponse>(json);
                    task.SetResult(productsResponse.purchases);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });
            return task;
        }

        [Serializable]
        public class IncompletePurchasesResponse
        {
            public bool hasMore;
            public List<IncompletePurchase> purchases;
        }

        [Serializable]
        public class IncompletePurchase
        {
            public string productSku;
            public string purchaseToken;
            public long timestamp;
            public string platform;
        }


        [System.Serializable]
        public class PurchaseCompleteResult
        {
            public string result;
            public string error;
        }

        [System.Serializable]
        public class PurchaseResult
        {
            public string result;
            public string error;
            public string purchaseToken;
        }



        [System.Serializable]
        public class Product 
        {
            public string sku;
            public string name;
            public string description;
            public float price;
        }

    }
}
