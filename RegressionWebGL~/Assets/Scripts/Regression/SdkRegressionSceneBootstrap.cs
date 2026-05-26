using com.jest.sdk;
using UnityEngine;

namespace com.jest.sdk.regression
{
    public sealed class SdkRegressionSceneBootstrap : MonoBehaviour
    {
        private void Start()
        {
            JestSDK.Instance.Init(new InitOptions
            {
                AutoLoginReminders = false
            }).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("[SdkRegression] SDK initialization failed: " + task.Exception);
                    return;
                }

                SdkRegressionRunner.EnsureStarted();
            });
        }
    }
}
