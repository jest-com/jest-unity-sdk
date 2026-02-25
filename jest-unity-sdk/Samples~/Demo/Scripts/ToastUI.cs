using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


namespace com.jest.demo
{

    public class ToastUI : MonoBehaviour
    {
        public TMP_Text toastText;
        public CanvasGroup canvasGroup;
        public float duration = 2f;

        public void ShowToast(string message)
        {
            StopAllCoroutines();
            toastText.text = message;
            StartCoroutine(FadeToast());
        }

        private IEnumerator FadeToast()
        {
            canvasGroup.alpha = 1;
            yield return new WaitForSeconds(duration);

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = 1 - t;
                yield return null;
            }
        }
    }
}