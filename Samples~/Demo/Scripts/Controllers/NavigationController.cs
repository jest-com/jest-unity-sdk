using System.Collections.Generic;
using com.jest.sdk;
using TMPro;
using UnityEngine;

namespace com.jest.demo
{
    public class NavigationController : MonoBehaviour
    {
        [Header("Redirect to Game")]
        [SerializeField] private TMP_InputField m_gameSlugInput;
        [SerializeField] private TMP_InputField m_entryPayloadInput;

        public void RedirectToFlagshipGame()
        {
            string entryPayloadJson = m_entryPayloadInput.text;
            Navigation.RedirectToFlagshipGameOptions options = null;

            if (!string.IsNullOrEmpty(entryPayloadJson))
            {
                try
                {
                    var payload = Convert.FromString<Dictionary<string, object>>(entryPayloadJson);
                    options = new Navigation.RedirectToFlagshipGameOptions
                    {
                        entryPayload = payload
                    };
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                    UIManager.Instance.m_toastUI.ShowToast("Invalid entry payload JSON");
                    return;
                }
            }

            JestSDK.Instance.Navigation.RedirectToFlagshipGame(options);
            UIManager.Instance.m_toastUI.ShowToast("Redirecting to flagship game...");
        }

        public void RedirectToGame()
        {
            string gameSlug = m_gameSlugInput.text;

            if (string.IsNullOrEmpty(gameSlug))
            {
                UIManager.Instance.m_toastUI.ShowToast("Game slug is required");
                return;
            }

            var options = new Navigation.RedirectToGameOptions
            {
                gameSlug = gameSlug
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

            JestSDK.Instance.Navigation.RedirectToGame(options);
            UIManager.Instance.m_toastUI.ShowToast("Redirecting to " + gameSlug + "...");
        }

        public void RedirectToExplorePage()
        {
            JestSDK.Instance.Navigation.RedirectToExplorePage();
            UIManager.Instance.m_toastUI.ShowToast("Redirecting to explore page...");
        }
    }
}
