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
    /// <remarks>
    /// <b>Sandbox testing:</b> sandbox users see real product prices in the
    /// game UI, but the platform checkout modal makes clear that no charge
    /// will be made and the resulting purchase records 0 credits.
    /// </remarks>
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
        public JestSDKTask<PurchaseResult> BeginPurchase(string sku)
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
        /// Initiates an in-app purchase for the specified product SKU.
        /// </summary>
        /// <param name="sku">The product SKU to purchase.</param>
        /// <returns>
        /// A <see cref="JestSDKTask{TResult}"/> resolving to a <see cref="PurchaseResult"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when sku is null or empty.</exception>
        [Obsolete("Use BeginPurchase() instead")]
        public JestSDKTask<PurchaseResult> Purchase(string sku) => BeginPurchase(sku);

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
        /// Starts the platform checkout flow for a subscription.
        /// </summary>
        /// <param name="subscriptionSku">The SKU of the subscription to subscribe to.</param>
        /// <returns>
        /// A <see cref="JestSDKTask{TResult}"/> resolving to a <see cref="SubscriptionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when subscriptionSku is null or empty.</exception>
        public JestSDKTask<SubscriptionResult> BeginSubscription(string subscriptionSku)
        {
            if (string.IsNullOrWhiteSpace(subscriptionSku))
            {
                throw new ArgumentException("Subscription SKU cannot be null or empty", nameof(subscriptionSku));
            }

            var task = new JestSDKTask<SubscriptionResult>();
            JsBridge.BeginSubscription(subscriptionSku).ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }
                    string json = t.GetResult();
                    var result = JsonConvert.DeserializeObject<SubscriptionResult>(json);
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

        /// <summary>
        /// Lists subscription offers for this game along with the player's current entitlement on each.
        /// For guest players, the returned subscriptions list is empty.
        /// </summary>
        /// <returns>
        /// A <see cref="JestSDKTask{TResult}"/> resolving to a <see cref="GetSubscriptionsResponse"/>.
        /// </returns>
        public JestSDKTask<GetSubscriptionsResponse> GetSubscriptions()
        {
            var task = new JestSDKTask<GetSubscriptionsResponse>();
            JsBridge.GetSubscriptions().ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }

                    string json = t.GetResult();
                    var response = JsonConvert.DeserializeObject<GetSubscriptionsResponse>(json);
                    task.SetResult(response);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });

            return task;
        }

        /// <summary>
        /// Opens a cancellation confirmation dialog for the specified subscription.
        /// If the user confirms, the subscription is cancelled at the end of the current billing period.
        /// </summary>
        /// <param name="subscriptionSku">The SKU of the subscription to cancel.</param>
        /// <returns>
        /// A <see cref="JestSDKTask{TResult}"/> resolving to a <see cref="CancelSubscriptionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when subscriptionSku is null or empty.</exception>
        public JestSDKTask<CancelSubscriptionResult> CancelSubscription(string subscriptionSku)
        {
            if (string.IsNullOrWhiteSpace(subscriptionSku))
            {
                throw new ArgumentException("Subscription SKU cannot be null or empty", nameof(subscriptionSku));
            }

            var task = new JestSDKTask<CancelSubscriptionResult>();
            JsBridge.CancelSubscription(subscriptionSku).ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }
                    string json = t.GetResult();
                    var result = JsonConvert.DeserializeObject<CancelSubscriptionResult>(json);
                    task.SetResult(result);
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
        /// Represents a subscription offer and the player's current entitlement on it.
        /// </summary>
        [Serializable]
        public class SubscriptionData
        {
            /// <summary>The SKU of the subscription.</summary>
            [JsonProperty("sku")]
            public string Sku;

            /// <summary>The display name of the subscription.</summary>
            [JsonProperty("displayName")]
            public string DisplayName;

            /// <summary>The display description of the subscription, or null.</summary>
            [JsonProperty("displayDescription")]
            public string DisplayDescription;

            /// <summary>Price in the specified currency, in decimal.</summary>
            [JsonProperty("price")]
            public decimal Price;

            /// <summary>ISO currency code, e.g. "USD".</summary>
            [JsonProperty("currency")]
            public string Currency;

            /// <summary>Billing period: "monthly", "yearly", or "weekly".</summary>
            [JsonProperty("billingPeriod")]
            public string BillingPeriod;

            /// <summary>Entitlement status: "active" if the player holds this subscription, "inactive" otherwise.</summary>
            [JsonProperty("status")]
            public string Status;
        }

        /// <summary>
        /// Represents the response returned by <see cref="Payment.GetSubscriptions"/>.
        /// </summary>
        [Serializable]
        public class GetSubscriptionsResponse
        {
            /// <summary>The list of subscription offers with the player's current entitlement on each.</summary>
            [JsonProperty("subscriptions")]
            public List<SubscriptionData> Subscriptions;

            /// <summary>HS256 JWS of the subscriptions payload for server-side verification.</summary>
            [JsonProperty("signed")]
            public string Signed;
        }

        /// <summary>
        /// Represents the result of a subscription cancellation flow.
        /// </summary>
        [Serializable]
        public class CancelSubscriptionResult
        {
            /// <summary>The result status: "success", "cancel", or "error".</summary>
            [JsonProperty("result")]
            public string Result;

            /// <summary>
            /// Error code when result is "error":
            /// "internal_error", "not_found", "not_active", or "guest_not_allowed".
            /// </summary>
            [JsonProperty("error")]
            public string Error;
        }

        /// <summary>
        /// Represents the result of initiating a subscription checkout flow.
        /// </summary>
        [Serializable]
        public class SubscriptionResult
        {
            /// <summary>The result status: "success", "cancel", or "error".</summary>
            [JsonProperty("result")]
            public string Result;

            /// <summary>
            /// Error code when result is "error":
            /// "internal_error", "invalid_subscription", "already_subscribed", or "guest_not_allowed".
            /// </summary>
            [JsonProperty("error")]
            public string Error;

            /// <summary>The subscription data on success.</summary>
            [JsonProperty("subscription")]
            public SubscriptionData Subscription;

            /// <summary>The serialized and signed subscription data for server-side verification.</summary>
            [JsonProperty("subscriptionSigned")]
            public string SubscriptionSigned;
        }

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
            /// Error message if the purchase process failed (e.g., internal_error, invalid_product).
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
            /// The total value in Jest Tokens.
            /// </summary>
            public decimal credits;

            /// <summary>
            /// The timestamp (Unix epoch) when the purchase was created.
            /// </summary>
            public long createdAt;

            /// <summary>
            /// The timestamp (Unix epoch) when the purchase was completed.
            /// </summary>
            public long? completedAt;

            /// <summary>
            /// The estimated USD share of revenue from this purchase that the publisher will receive.
            /// </summary>
            public decimal estimatedRevenue;

            /// <summary>
            /// Price paid for this purchase in the specified currency, in decimal.
            /// </summary>
            public decimal price;

            /// <summary>
            /// ISO currency code for the price, e.g. "USD", "EUR".
            /// </summary>
            public string currency;
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
            /// The price of the product in the specified currency, in decimal (e.g., 9.99 for $9.99).
            /// </summary>
            public double price;

            /// <summary>
            /// ISO currency code for the price, e.g. "USD", "EUR".
            /// </summary>
            public string currency;
        }

        #endregion
    }
}
