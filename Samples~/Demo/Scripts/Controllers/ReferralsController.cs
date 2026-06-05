using System.Collections.Generic;
using com.jest.sdk;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.jest.demo
{
    public class ReferralsController : MonoBehaviour
    {
        [Header("Open Referral Dialog")]
        [SerializeField] private TMP_InputField m_referenceInput;
        [SerializeField] private TMP_InputField m_entryPayloadInput;
        [SerializeField] private TMP_InputField m_onboardingSlugInput;

        // Optional: a UI Image whose sprite is sent as the referral link's OG preview.
        // Leave unassigned to fall back to the game's static share image.
        [SerializeField] private Image m_shareImage;

        // Optional: a UI toggle controlling whether the share image is attached at
        // runtime. When left unassigned, the image is attached whenever a texture is set.
        [SerializeField] private Toggle m_useShareImageToggle;

        [Header("List Referrals Output")]
        [SerializeField] private TextMeshProUGUI m_referralsOutputText;

        public async void OpenReferralDialog()
        {
            string reference = m_referenceInput.text;

            if (string.IsNullOrEmpty(reference))
            {
                UIManager.Instance.m_toastUI.ShowToast("Reference is required");
                return;
            }

            var options = new Referrals.OpenDialogOptions
            {
                reference = reference,
                onboardingSlug = string.IsNullOrEmpty(m_onboardingSlugInput?.text) ? null : m_onboardingSlugInput.text,
                // Personalized OG preview for the referral landing page. Omitted (null)
                // when no texture is assigned, preserving the game's default share image.
                shareImage = BuildShareImageDataUrl(),
                // Example: notify the referrer once their first invited friend joins.
                // NotificationTemplates = new List<Referrals.ReferralNotificationTemplate>
                // {
                //     new Referrals.ReferralNotificationTemplate
                //     {
                //         MinConversionCount = 1,
                //         Variants = new List<Referrals.ReferralNotificationVariant>
                //         {
                //             new Referrals.ReferralNotificationVariant
                //             {
                //                 Body = "Your friend just joined!",
                //                 CtaText = "Play",
                //             }
                //         }
                //     }
                // }
            };

            Debug.Log(options.shareImage == null
                ? "[Jest ShareImage] options.shareImage is null — the platform will use the default share image."
                : $"[Jest ShareImage] Referral will include a custom shareImage ({options.shareImage.Length} chars).");

            // Parse entry payload if provided
            string entryPayloadJson = m_entryPayloadInput.text;
            if (!string.IsNullOrEmpty(entryPayloadJson))
            {
                try
                {
                    options.entryPayload = Convert.FromString<Dictionary<string, object>>(entryPayloadJson);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                    UIManager.Instance.m_toastUI.ShowToast("Invalid entry payload JSON");
                    return;
                }
            }

            UIManager.Instance.ShowLoadingSpinner();
            try
            {
                await JestSDK.Instance.Referrals.OpenReferralDialog(options);
                UIManager.Instance.m_toastUI.ShowToast("Referral dialog opened");
            }
            catch (System.Exception e)
            {
                UIManager.Instance.m_toastUI.ShowToast("Failed: " + e.Message);
            }
            UIManager.Instance.HideLoadingSpinner();
        }

        // Encodes the assigned Image's sprite into a base64 data URL accepted by the SDK.
        // Accepted MIME: image/png, image/jpeg, image/webp; the data URL must be <= 2 MB.
        // The sprite's source texture must have Read/Write enabled to be encodable. For a
        // sprite packed into an atlas, sprite.texture is the whole atlas — this sample
        // assumes a standalone sprite.
        // Returns null when the toggle is off or no image/sprite is assigned.
        private string BuildShareImageDataUrl()
        {
            if (m_useShareImageToggle != null && !m_useShareImageToggle.isOn)
            {
                Debug.Log("[Jest ShareImage] Toggle is off — not attaching a share image.");
                return null;
            }

            if (m_shareImage == null)
            {
                Debug.Log("[Jest ShareImage] No Image assigned (m_shareImage is null). Wire the Image in the inspector.");
                return null;
            }

            Sprite sprite = m_shareImage.sprite;
            if (sprite == null)
            {
                Debug.Log("[Jest ShareImage] The assigned Image has no sprite (m_shareImage.sprite is null).");
                return null;
            }

            // JestUtils handles compressed / non-readable textures (and the Y-flip) so the
            // sample doesn't have to. Any other SDK API that accepts an image data URL can
            // reuse the same helper.
            string dataUrl = JestUtils.SpriteToDataUrl(sprite);
            Debug.Log(dataUrl == null
                ? "[Jest ShareImage] Encoding failed — see the error above."
                : $"[Jest ShareImage] Built data URL (~{dataUrl.Length / 1024} KB; platform limit ~2 MB).");
            return dataUrl;
        }

        public async void ListReferrals()
        {
            UIManager.Instance.ShowLoadingSpinner();
            try
            {
                var task = JestSDK.Instance.Referrals.ListReferrals();
                await task;

                if (task.IsCompleted)
                {
                    var response = task.Result;
                    string output = "Referrals:\n";

                    if (response.referrals != null && response.referrals.Count > 0)
                    {
                        foreach (var referral in response.referrals)
                        {
                            output += $"- {referral.reference}: {referral.registrations?.Count ?? 0} registrations\n";
                        }
                    }
                    else
                    {
                        output += "No referrals found";
                    }

                    if (m_referralsOutputText != null)
                    {
                        m_referralsOutputText.text = output;
                    }

                    UIManager.Instance.m_toastUI.ShowToast("Referrals loaded");
                }
            }
            catch (System.Exception e)
            {
                UIManager.Instance.m_toastUI.ShowToast("Failed: " + e.Message);
            }
            UIManager.Instance.HideLoadingSpinner();
        }
    }
}
