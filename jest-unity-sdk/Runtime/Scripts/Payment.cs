using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides functionality for handling in-app purchases within the Jest SDK.
    /// Includes methods for retrieving products, initiating purchases,
    /// completing transactions, and fetching incomplete purchases.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Payment"/> class.
        /// Internal constructor to restrict external instantiation.
        /// </summary>
        internal Payment() { }

        #region Public Methods

        /// <summary>
        /// Retrieves a list of available in-app purchase products.
        /// </summary>
        /// <returns>
        /// A <see cref="JestSDKTask{TResult}"/> resolving to a list of <see cref="Product"/> objects.
        /// </returns>
        public JestSDKTask<List<Product>> GetProducts()
        {
            var task = new JestSDKTask<List<Product>>();
            var getProductsTask = JsBridge.GetProducts();

            getProductsTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }

                    string json = t.GetResult();
                    var products = JsonConvert.DeserializeObject<List<Product>>(json);
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
        /// A <see cref="JestSDKTask{TResult}"/> resolving to a <see cref="PurchaseResult"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when sku is null or empty.</exception>
        public JestSDKTask<PurchaseResult> Purchase(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                throw new ArgumentException("SKU cannot be null or empty", nameof(sku));
            }

            var task = new JestSDKTask<PurchaseResult>();
            JsBridge.BeginPurchase(sku).ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }
                    string json = t.GetResult();
                    var result = JsonConvert.DeserializeObject<PurchaseResult>(json);
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
        /// A <see cref="JestSDKTask{TResult}"/> resolving to a <see cref="PurchaseCompleteResult"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when purchaseToken is null or empty.</exception>
        public JestSDKTask<PurchaseCompleteResult> CompletePurchase(string purchaseToken)
        {
            if (string.IsNullOrWhiteSpace(purchaseToken))
            {
                throw new ArgumentException("Purchase token cannot be null or empty", nameof(purchaseToken));
            }

            var task = new JestSDKTask<PurchaseCompleteResult>();
            var purchaseCompleteTask = JsBridge.CompletePurchase(purchaseToken);

            purchaseCompleteTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }

                    string json = t.GetResult();
                    var result = JsonConvert.DeserializeObject<PurchaseCompleteResult>(json);
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
        /// A <see cref="JestSDKTask{TResult}"/> resolving to an <see cref="IncompletePurchasesResponse"/>.
        /// </returns>
        public JestSDKTask<IncompletePurchasesResponse> GetIncompletePurchases()
        {
            var task = new JestSDKTask<IncompletePurchasesResponse>();
            var getIncompletePurchasesTask = JsBridge.GetIncompletePurchases();

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
                    var response = JsonConvert.DeserializeObject<IncompletePurchasesResponse>(json);
                    task.SetResult(response);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });

            return task;
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// Represents the response returned when retrieving incomplete purchases.
        /// </summary>
        [Serializable]
        public class IncompletePurchasesResponse
        {
            /// <summary>
            /// Indicates whether more incomplete purchases are available for retrieval.
            /// </summary>
            public bool hasMore;

            /// <summary>
            /// The serialized and signed list of purchases.
            /// </summary>
            public string purchasesSigned;

            /// <summary>
            /// A list of incomplete purchase data entries.
            /// </summary>
            public List<PurchaseData> purchases;
        }

        /// <summary>
        /// Represents the result of completing a purchase.
        /// </summary>
        [Serializable]
        public class PurchaseCompleteResult
        {
            /// <summary>
            /// The result status (e.g., "success", "error").
            /// </summary>
            public string result;

            /// <summary>
            /// Error message if the completion process failed 
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
            /// The result status (e.g., "success", "cancel", "error")
            /// </summary>
            public string result;

            /// <summary>
            /// Error message if the purchase process failed.(e.g., login_required, internal_error, invalid_product).
            /// </summary>
            public string error;

            /// <summary>
            /// The purchase data associated with this transaction.
            /// </summary>
            public PurchaseData purchase;

            /// <summary>
            /// The serialized and signed purchase data.
            /// </summary>
            public string purchaseSigned;
        }

        /// <summary>
        /// Represents details of a single purchase transaction.
        /// </summary>
        [Serializable]
        public class PurchaseData
        {
            /// <summary>
            /// The unique purchase token.
            /// </summary>
            public string purchaseToken;

            /// <summary>
            /// The SKU of the purchased product.
            /// </summary>
            public string productSku;

            /// <summary>
            /// The number of credits purchased (if applicable).
            /// </summary>
            public long credits;

            /// <summary>
            /// The timestamp (Unix epoch) when the purchase was created.
            /// </summary>
            public long createdAt;

            /// <summary>
            /// The timestamp (Unix epoch) when the purchase was completed.
            /// </summary>
            public long? completedAt;
        }

        /// <summary>
        /// Represents a purchasable product.
        /// </summary>
        [Serializable]
        public class Product
        {
            /// <summary>
            /// The unique SKU identifier for the product.
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

        #endregion
    }
}
