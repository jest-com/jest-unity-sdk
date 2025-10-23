using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides methods for handling in-app purchases within the Jest SDK.
    /// Includes APIs for retrieving available products, initiating purchases,
    /// completing transactions, and fetching incomplete purchases.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Payment"/> class.
        /// Internal constructor to prevent external instantiation.
        /// </summary>
        internal Payment() { }

        /// <summary>
        /// Retrieves a list of available in-app purchase products.
        /// </summary>
        /// <returns>
        /// A <see cref="JestSDKTask{TResult}"/> that resolves to a list of <see cref="Product"/> objects.
        /// </returns>
        public JestSDKTask<List<Product>> GetProducts()
        {
            var task = new JestSDKTask<List<Product>>();
            JestSDKTask<string> getProductsTask = JsBridge.GetProducts();

            // Continue the asynchronous operation.
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

        /// <summary>
        /// Initiates an in-app purchase for the specified product SKU.
        /// </summary>
        /// <param name="sku">The product SKU to purchase.</param>
        /// <returns>
        /// A <see cref="JestSDKTask{TResult}"/> that resolves to a <see cref="PurchaseResult"/>.
        /// </returns>
        public JestSDKTask<PurchaseResult> Purchase(string sku)
        {
            var task = new JestSDKTask<PurchaseResult>();
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

        /// <summary>
        /// Completes a pending purchase using the provided purchase token.
        /// </summary>
        /// <param name="purchaseToken">The token associated with the purchase to complete.</param>
        /// <returns>
        /// A <see cref="JestSDKTask{TResult}"/> that resolves to a <see cref="PurchaseCompleteResult"/>.
        /// </returns>
        public JestSDKTask<PurchaseCompleteResult> CompletePurchase(string purchaseToken)
        {
            var task = new JestSDKTask<PurchaseCompleteResult>();
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

        /// <summary>
        /// Retrieves a list of incomplete purchases that have not yet been completed.
        /// </summary>
        /// <returns>
        /// A <see cref="JestSDKTask{TResult}"/> that resolves to a list of <see cref="IncompletePurchase"/> objects.
        /// </returns>
        public JestSDKTask<List<IncompletePurchase>> GetIncompletePurchases()
        {
            var task = new JestSDKTask<List<IncompletePurchase>>();
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
                    IncompletePurchasesResponse response = JsonConvert.DeserializeObject<IncompletePurchasesResponse>(json);
                    task.SetResult(response.purchases);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });

            return task;
        }

        /// <summary>
        /// Represents the response returned when retrieving incomplete purchases.
        /// </summary>
        [Serializable]
        public class IncompletePurchasesResponse
        {
            /// <summary>
            /// Indicates whether there are more incomplete purchases available for retrieval.
            /// </summary>
            public bool hasMore;

            /// <summary>
            /// A list of incomplete purchases.
            /// </summary>
            public List<IncompletePurchase> purchases;
        }

        /// <summary>
        /// Represents a single incomplete purchase record.
        /// </summary>
        [Serializable]
        public class IncompletePurchase
        {
            /// <summary>
            /// The product SKU associated with the incomplete purchase.
            /// </summary>
            public string productSku;

            /// <summary>
            /// The unique purchase token.
            /// </summary>
            public string purchaseToken;

            /// <summary>
            /// The timestamp of when the purchase was initiated.
            /// </summary>
            public long timestamp;

            /// <summary>
            /// The platform on which the purchase occurred (e.g., Android, iOS).
            /// </summary>
            public string platform;
        }

        /// <summary>
        /// Represents the result of completing a purchase.
        /// </summary>
        [Serializable]
        public class PurchaseCompleteResult
        {
            /// <summary>
            /// The result status (e.g., "success", "failure").
            /// </summary>
            public string result;

            /// <summary>
            /// Any error message returned during the completion process.
            /// </summary>
            public string error;
        }

        /// <summary>
        /// Represents the result of initiating a purchase.
        /// </summary>
        [Serializable]
        public class PurchaseResult
        {
            /// <summary>
            /// The result status (e.g., "success", "failure").
            /// </summary>
            public string result;

            /// <summary>
            /// Any error message returned during the purchase process.
            /// </summary>
            public string error;

            /// <summary>
            /// The unique purchase token generated for this transaction.
            /// </summary>
            public string purchaseToken;
        }

        /// <summary>
        /// Represents a purchasable product.
        /// </summary>
        [Serializable]
        public class Product
        {
            /// <summary>
            /// The unique product SKU.
            /// </summary>
            public string sku;

            /// <summary>
            /// The display name of the product.
            /// </summary>
            public string name;

            /// <summary>
            /// The description of the product.
            /// </summary>
            public string description;

            /// <summary>
            /// The price of the product, in the smallest currency unit (e.g., cents).
            /// </summary>
            public float price;
        }
    }
}
