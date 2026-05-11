using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using com.jest.sdk;

namespace com.jest.demo
{
    public class BotAvatarController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private TMP_InputField m_usernameInput;

        [Header("Output")]
        [SerializeField] private Image m_avatarImage;
        [SerializeField] private TextMeshProUGUI m_urlLabel;

        public void GetBotAvatar()
        {
            string username = m_usernameInput != null ? m_usernameInput.text?.Trim() : null;
            if (string.IsNullOrEmpty(username))
            {
                UIManager.Instance.m_toastUI.ShowToast("Username is required");
                return;
            }

            string url = JestSDK.Instance.GetBotAvatar(username, 128);
            if (m_urlLabel != null)
            {
                m_urlLabel.text = url;
            }

            StopAllCoroutines();
            StartCoroutine(LoadAvatarTexture(url));
        }

        private IEnumerator LoadAvatarTexture(string url)
        {
            using var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                UIManager.Instance.m_toastUI.ShowToast("Avatar load failed: " + request.error);
                yield break;
            }

            if (m_avatarImage != null)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                m_avatarImage.sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
        }
    }
}
