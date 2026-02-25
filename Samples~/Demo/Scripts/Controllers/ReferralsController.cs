using System.Collections.Generic;
using com.jest.sdk;
using TMPro;
using UnityEngine;

namespace com.jest.demo
{
    public class ReferralsController : MonoBehaviour
    {
        [Header("Open Referral Dialog")]
        [SerializeField] private TMP_InputField m_referenceInput;
        [SerializeField] private TMP_InputField m_entryPayloadInput;
        [SerializeField] private TMP_InputField m_onboardingSlugInput;

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
                onboardingSlug = string.IsNullOrEmpty(m_onboardingSlugInput?.text) ? null : m_onboardingSlugInput.text
            };

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
