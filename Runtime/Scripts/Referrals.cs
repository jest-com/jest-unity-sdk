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
            /// Optional templates used to notify the referrer when invited players convert.
            /// Each template applies above its <c>minConversionCount</c> threshold; the server
            /// picks the template with the highest matching threshold and a variant from within it.
            /// </summary>
            public List<ReferralNotificationTemplate> notificationTemplates;

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

                if (notificationTemplates != null && notificationTemplates.Count > 0)
                {
                    jsonObj["notificationTemplates"] = notificationTemplates;
                }

                return JsonConvert.SerializeObject(jsonObj);
            }
        }

        /// <summary>
        /// A single notification variant sent to the referrer when a conversion threshold is met.
        /// </summary>
        [Serializable]
        public class ReferralNotificationVariant
        {
            /// <summary>Optional title displayed above the notification body.</summary>
            public string title;

            /// <summary>Main body text of the notification.</summary>
            public string body;

            /// <summary>Call-to-action button label.</summary>
            public string ctaText;

            /// <summary>Optional reference to a pre-approved image asset.</summary>
            public string imageReference;
        }

        /// <summary>
        /// A notification template that applies when the referrer's conversion count
        /// reaches <see cref="minConversionCount"/>.
        /// </summary>
        [Serializable]
        public class ReferralNotificationTemplate
        {
            /// <summary>
            /// Minimum number of conversions required for this template to be eligible.
            /// The server selects the template with the highest matching threshold.
            /// </summary>
            public int minConversionCount;

            /// <summary>The pool of variants the server may choose from.</summary>
            public List<ReferralNotificationVariant> variants;
        }

        /// <summary>
        /// Represents a player who joined via a referral link.
        /// </summary>
        [Serializable]
        public class ReferralRegistration
        {
            /// <summary>
            /// The player ID of the referred user.
            /// </summary>
            public string playerId;

            /// <summary>
            /// ISO datetime string of when the player joined.
            /// </summary>
            public string joinedAt;
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
            /// List of players who joined via this referral.
            /// </summary>
            public List<ReferralRegistration> registrations;
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
