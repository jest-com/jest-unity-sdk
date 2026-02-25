using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides functionality for managing referrals within the Jest SDK.
    /// Supports opening referral share dialogs and retrieving referral statistics.
    /// </summary>
    public class Referrals
    {
        /// <summary>
        /// Internal constructor to prevent external instantiation.
        /// </summary>
        internal Referrals() { }

        #region Public Methods

        /// <summary>
        /// Opens a native share dialog with a referral link.
        /// </summary>
        /// <param name="options">The referral dialog configuration options.</param>
        /// <returns>A task that completes when the share dialog has been handled.</returns>
        /// <exception cref="ArgumentException">Thrown when reference is null or empty.</exception>
        public JestSDKTask OpenReferralDialog(OpenDialogOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.reference))
            {
                throw new ArgumentException("Reference cannot be null or empty", nameof(options));
            }

            return JsBridge.OpenReferralDialog(options.ToJson());
        }

        /// <summary>
        /// Retrieves the player's referral statistics.
        /// </summary>
        /// <returns>A task resolving to the referral list response.</returns>
        public JestSDKTask<ListReferralsResponse> ListReferrals()
        {
            var task = new JestSDKTask<ListReferralsResponse>();
            var listReferralsTask = JsBridge.ListReferrals();

            listReferralsTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }

                    string json = t.GetResult();
                    var response = JsonConvert.DeserializeObject<ListReferralsResponse>(json);
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
        /// Configuration options for opening a referral dialog.
        /// </summary>
        [Serializable]
        public class OpenDialogOptions
        {
            /// <summary>
            /// The referral code/reference identifier. Required.
            /// </summary>
            public string reference;

            /// <summary>
            /// Optional custom data payload to pass with the referral.
            /// </summary>
            public Dictionary<string, object> entryPayload;

            /// <summary>
            /// Optional title for the share dialog.
            /// </summary>
            public string shareTitle;

            /// <summary>
            /// Optional text for the share message.
            /// </summary>
            public string shareText;

            /// <summary>
            /// Optional onboarding slug for custom onboarding flows.
            /// </summary>
            public string onboardingSlug;

            /// <summary>
            /// Serializes the options to JSON for the bridge.
            /// </summary>
            internal string ToJson()
            {
                var jsonObj = new Dictionary<string, object>
                {
                    ["reference"] = reference
                };

                if (entryPayload != null && entryPayload.Count > 0)
                {
                    jsonObj["entryPayload"] = entryPayload;
                }

                if (!string.IsNullOrEmpty(shareTitle))
                {
                    jsonObj["shareTitle"] = shareTitle;
                }

                if (!string.IsNullOrEmpty(shareText))
                {
                    jsonObj["shareText"] = shareText;
                }

                if (!string.IsNullOrEmpty(onboardingSlug))
                {
                    jsonObj["onboardingSlug"] = onboardingSlug;
                }

                return JsonConvert.SerializeObject(jsonObj);
            }
        }

        /// <summary>
        /// Represents a single referral entry with its registration data.
        /// </summary>
        [Serializable]
        public class ReferralInfo
        {
            /// <summary>
            /// The referral code/reference identifier.
            /// </summary>
            public string reference;

            /// <summary>
            /// List of registration IDs associated with this referral.
            /// </summary>
            public List<string> registrations;
        }

        /// <summary>
        /// Response containing the player's referral statistics.
        /// </summary>
        [Serializable]
        public class ListReferralsResponse
        {
            /// <summary>
            /// List of referrals made by the player.
            /// </summary>
            public List<ReferralInfo> referrals;

            /// <summary>
            /// Signed payload of the referrals data for verification.
            /// </summary>
            public string referralsSigned;
        }

        #endregion
    }
}
