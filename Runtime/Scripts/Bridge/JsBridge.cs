using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.Scripting;

namespace com.jest.sdk
{
    internal static class JsBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        [DllImport("__Internal")]
        private static extern string JS_getEntryPayload();

        [DllImport("__Internal")]
        private static extern string JS_getPlayerId();

        [DllImport("__Internal")]
        private static extern string JS_getPlayerData();

        [DllImport("__Internal")]
        private static extern string JS_getIsRegistered();

        [DllImport("__Internal")]
        private static extern string JS_getPlayerValue(string key);

        [DllImport("__Internal")]
        private static extern void JS_setPlayerValue(string key, string value);

        [DllImport("__Internal")]
        private static extern void JS_deletePlayerValue(string key);

        [DllImport("__Internal")]
        private static extern void JS_flush(IntPtr taskPtr, Action<IntPtr> onSuccess, Action<IntPtr, string> onError);

        [DllImport("__Internal")]
        private static extern void JS_scheduleNotificationV2(string optionsJson);

        [DllImport("__Internal")]
        private static extern void JS_unscheduleNotificationV2(string identifier);

        [DllImport("__Internal")]
        private static extern void JS_callAsyncVoid(System.IntPtr ptr, string call, System.Action<System.IntPtr> successCallback,
                                         System.Action<System.IntPtr, string> errorCallback);

        [DllImport("__Internal")]
        private static extern void JS_callAsyncString(System.IntPtr ptr, string call, System.Action<System.IntPtr, string> successCallback,
                                         System.Action<System.IntPtr, string> errorCallback);

        [DllImport("__Internal")]
        private static extern void JS_callAsyncNumber(System.IntPtr ptr, string call, System.Action<System.IntPtr, float> successCallback,
                                         System.Action<System.IntPtr, string> errorCallback);

        [DllImport("__Internal")]
        private static extern void JS_initSdk(System.IntPtr ptr, System.Action<System.IntPtr> successCallback,
                                         System.Action<System.IntPtr, string> errorCallback);

        [DllImport("__Internal")]
        private static extern void JS_login(string payload);

        [DllImport("__Internal")]
        private static extern void JS_getProducts(IntPtr taskPtr, Action<IntPtr, string> onSuccess, Action<IntPtr, 
                                    string> onError);

        [DllImport("__Internal")]
        private static extern void JS_beginPurchase(IntPtr taskPtr, string sku, Action<IntPtr, string> onSuccess, 
                                    Action<IntPtr, string> onError);

        [DllImport("__Internal")]
        private static extern void JS_completePurchase(IntPtr taskPtr, string purchaseToken, Action<IntPtr, 
                                    string> onSuccess, Action<IntPtr, string> onError);

        [DllImport("__Internal")]
        private static extern void JS_getIncompletePurchases(IntPtr taskPtr, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError);

        [DllImport("__Internal")]
        private static extern void JS_openReferralDialog(IntPtr taskPtr, string optionsJson,
                                    Action<IntPtr> onSuccess, Action<IntPtr, string> onError);

        [DllImport("__Internal")]
        private static extern void JS_listReferrals(IntPtr taskPtr, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError);

        [DllImport("__Internal")]
        private static extern void JS_redirectToGame(string optionsJson);

        [DllImport("__Internal")]
        private static extern void JS_openLegalPage(string page);

        [DllImport("__Internal")]
        private static extern void JS_getPlayerSigned(IntPtr taskPtr, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError);

        [DllImport("__Internal")]
        private static extern void JS_debugRegister();

        [DllImport("__Internal")]
        private static extern void JS_redirectToExplorePage();

        [DllImport("__Internal")]
        private static extern void JS_getFeatureFlag(IntPtr taskPtr, string key, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError);

        [DllImport("__Internal")]
        private static extern void JS_reserveLoginMessage(IntPtr taskPtr, string optionsJson, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError);

        [DllImport("__Internal")]
        private static extern void JS_sendReservedLoginMessage(string reservationJson);

#else
        private static string JS_getEntryPayload() { return _bridgeMock.GetEntryPayload(); }

        private static string JS_getPlayerId() { return _bridgeMock.playerId; }

        private static string JS_getPlayerData() { return _bridgeMock.playerData; }

        private static string JS_getIsRegistered() { return _bridgeMock.isRegistered; }

        private static string JS_getPlayerValue(string key) { return _bridgeMock.GetPlayerValue(key); }

        private static void JS_setPlayerValue(string key, string value) { _bridgeMock.SetPlayerValue(key, value); }

        private static void JS_deletePlayerValue(string key) { _bridgeMock.DeletePlayerValue(key); }

        private static void JS_flush(IntPtr taskPtr, Action<IntPtr> onSuccess, Action<IntPtr, string> onError)
        { onSuccess(taskPtr); }

        private static void JS_scheduleNotificationV2(string optionsJson) { _bridgeMock.ScheduleNotificationV2(optionsJson); }

        private static void JS_unscheduleNotificationV2(string identifier) { _bridgeMock.UnscheduleNotificationV2(identifier); }

        private static void JS_callAsyncNumber(System.IntPtr ptr, string call, System.Action<System.IntPtr, float> successCallback,
                                         System.Action<System.IntPtr, string> errorCallback)
        { successCallback(ptr, 0); }

        private static void JS_callAsyncString(System.IntPtr ptr, string call, System.Action<System.IntPtr, string> successCallback,
                                         System.Action<System.IntPtr, string> errorCallback)
        { successCallback(ptr, ""); }

        private static void JS_callAsyncVoid(System.IntPtr ptr, string call, System.Action<System.IntPtr> successCallback,
                                         System.Action<System.IntPtr, string> errorCallback)
        { successCallback(ptr); }

        private static void JS_initSdk(System.IntPtr ptr, System.Action<System.IntPtr> successCallback,
                                 System.Action<System.IntPtr, string> errorCallback)
        { successCallback(ptr); }

        private static void JS_login(string payload)
        { _bridgeMock.Login(payload); }


        private static void JS_getProducts(IntPtr taskPtr, Action<IntPtr, string> onSuccess, Action<IntPtr,
                                    string> onError)
        {
            onSuccess(taskPtr, _bridgeMock.GetProducts());
        }

        private static void JS_beginPurchase(IntPtr taskPtr, string sku, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError)
        {
            if (bool.TryParse(_bridgeMock.isRegistered, out bool isRegistered) && isRegistered)
            {
                onSuccess(taskPtr, _bridgeMock.GetPurchaseResponse());
            }
            else
            {
                onError(taskPtr, "Login Required");
            }
        }

        private static void JS_completePurchase(IntPtr taskPtr, string purchaseToken,
                                    Action<IntPtr, string> onSuccess, Action<IntPtr, string> onError)
        {
            onSuccess(taskPtr, _bridgeMock.GetPurchaseCompleteResponse());

        }
        private static void JS_getIncompletePurchases(IntPtr taskPtr, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError)
        {
            if (bool.TryParse(_bridgeMock.isRegistered, out bool isRegistered) && isRegistered)
            {
                onSuccess(taskPtr, _bridgeMock.GetIncompletePurchaseResponse());
            }
            else
            {
                onError(taskPtr, "Login Required");
            }
        }

        private static void JS_openReferralDialog(IntPtr taskPtr, string optionsJson,
                                    Action<IntPtr> onSuccess, Action<IntPtr, string> onError)
        {
            _bridgeMock.OpenReferralDialog(optionsJson);
            onSuccess(taskPtr);
        }

        private static void JS_listReferrals(IntPtr taskPtr, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError)
        {
            onSuccess(taskPtr, _bridgeMock.GetListReferralsResponse());
        }

        private static void JS_redirectToGame(string optionsJson)
        {
            _bridgeMock.RedirectToGame(optionsJson);
        }

        private static void JS_openLegalPage(string page)
        {
            _bridgeMock.OpenLegalPage(page);
        }

        private static void JS_getPlayerSigned(IntPtr taskPtr, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError)
        {
            onSuccess(taskPtr, _bridgeMock.GetPlayerSignedResponse());
        }

        private static void JS_debugRegister()
        {
            // In editor mode, just simulate login
            _bridgeMock.Login(null);
            UnityEngine.Debug.Log("[JestSDK] Debug register triggered (mock)");
        }

        private static void JS_redirectToExplorePage()
        {
            UnityEngine.Debug.Log("[JestSDK] Redirect to explore page (mock)");
        }

        private static void JS_getFeatureFlag(IntPtr taskPtr, string key, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError)
        {
            // Return empty string in mock (simulates undefined flag)
            onSuccess(taskPtr, "");
        }

        private static void JS_reserveLoginMessage(IntPtr taskPtr, string optionsJson, Action<IntPtr, string> onSuccess,
                                    Action<IntPtr, string> onError)
        {
            // Return mock reservation
            var mockResponse = "{\"reservation\":{\"id\":\"mock-reservation-id\",\"message\":\"mock-message\"}}";
            onSuccess(taskPtr, mockResponse);
            UnityEngine.Debug.Log("[JestSDK] Reserve login message (mock)");
        }

        private static void JS_sendReservedLoginMessage(string reservationJson)
        {
            UnityEngine.Debug.Log($"[JestSDK] Send reserved login message (mock): {reservationJson}");
        }

#endif

        private static IBridgeMock _bridgeMock = new DebugBridgeMock("playerId", true);

        internal static void SetMock(IBridgeMock mock)
        {
            _bridgeMock = mock;
        }

        internal static void ClearMock(IBridgeMock mock)
        {
            if (_bridgeMock == mock)
            {
                _bridgeMock = new DebugBridgeMock("playerId", true);
            }
        }

        internal static string GetEntryPayload()
        {
            return JS_getEntryPayload();
        }

        internal static string GetPlayerId()
        {
            return JS_getPlayerId();
        }

        internal static string GetPlayerData()
        {
            return JS_getPlayerData();
        }

        internal static string GetIsRegistered()
        {
            return JS_getIsRegistered();
        }

        internal static string GetPlayerValue(string key)
        {
            return JS_getPlayerValue(key);
        }

        internal static void SetPlayerValue(string key, string value)
        {
            JS_setPlayerValue(key, value);
        }

        internal static void DeletePlayerValue(string key)
        {
            JS_deletePlayerValue(key);
        }

        internal static JestSDKTask Flush()
        {
            return new JestSDKTask((System.IntPtr ptr) => { JS_flush(ptr, HandleSuccess, HandleError); });
        }

        internal static void ScheduleNotificationV2(string options)
        {
            JS_scheduleNotificationV2(options);
        }

        internal static void UnscheduleNotificationV2(string options)
        {
            JS_unscheduleNotificationV2(options);
        }

        internal static List<string> GetNotificationsV2()
        {
            return _bridgeMock.GetNotificationsV2();
        }

        internal static JestSDKTask Init()
        {
            return new JestSDKTask((System.IntPtr ptr) => { JS_initSdk(ptr, HandleSuccess, HandleError); });
        }

        internal static void Login(string payload)
        {
            JS_login(payload);
        }

        internal static JestSDKTask<string> GetProducts()
        {
            return new JestSDKTask<string>((System.IntPtr ptr) => { JS_getProducts(ptr, HandleSuccessString, HandleErrorString); });
        }

        internal static JestSDKTask<string> BeginPurchase(string sku)
        {
            return new JestSDKTask<string>((System.IntPtr ptr) => { JS_beginPurchase(ptr, sku, HandleSuccessString, HandleErrorString); });
        }

        internal static JestSDKTask<string> CompletePurchase(string purchaseToken)
        {
            return new JestSDKTask<string>((System.IntPtr ptr) => { JS_completePurchase(ptr, purchaseToken, HandleSuccessString, HandleErrorString); });
        }
        internal static JestSDKTask<string> GetIncompletePurchases()
        {
            return new JestSDKTask<string>((System.IntPtr ptr) => { JS_getIncompletePurchases(ptr, HandleSuccessString, HandleErrorString); });
        }

        internal static JestSDKTask OpenReferralDialog(string optionsJson)
        {
            return new JestSDKTask((System.IntPtr ptr) => { JS_openReferralDialog(ptr, optionsJson, HandleSuccess, HandleError); });
        }

        internal static JestSDKTask<string> ListReferrals()
        {
            return new JestSDKTask<string>((System.IntPtr ptr) => { JS_listReferrals(ptr, HandleSuccessString, HandleErrorString); });
        }

        internal static void RedirectToGame(string optionsJson)
        {
            JS_redirectToGame(optionsJson);
        }

        internal static void OpenLegalPage(string page)
        {
            JS_openLegalPage(page);
        }

        internal static JestSDKTask<string> GetPlayerSigned()
        {
            return new JestSDKTask<string>((System.IntPtr ptr) => { JS_getPlayerSigned(ptr, HandleSuccessString, HandleErrorString); });
        }

        internal static JestSDKTask CallAsyncVoid(string call)
        {
            return new JestSDKTask((System.IntPtr ptr) => { JS_callAsyncVoid(ptr, call, HandleSuccess, HandleError); });
        }

        internal static JestSDKTask<float> CallAsyncNumber(string call)
        {
            return new JestSDKTask<float>((System.IntPtr ptr) =>
                            { JS_callAsyncNumber(ptr, call, HandleSuccessNumber, HandleErrorNumber); });
        }

        internal static JestSDKTask<string> CallAsyncString(string call)
        {
            return new JestSDKTask<string>((System.IntPtr ptr) =>
                            { JS_callAsyncString(ptr, call, HandleSuccessString, HandleErrorString); });
        }

        internal static void DebugRegister()
        {
            JS_debugRegister();
        }

        internal static void RedirectToExplorePage()
        {
            JS_redirectToExplorePage();
        }

        internal static JestSDKTask<string> GetFeatureFlag(string key)
        {
            return new JestSDKTask<string>((System.IntPtr ptr) => { JS_getFeatureFlag(ptr, key, HandleSuccessString, HandleErrorString); });
        }

        internal static JestSDKTask<string> ReserveLoginMessage(string optionsJson)
        {
            return new JestSDKTask<string>((System.IntPtr ptr) => { JS_reserveLoginMessage(ptr, optionsJson, HandleSuccessString, HandleErrorString); });
        }

        internal static void SendReservedLoginMessage(string reservationJson)
        {
            JS_sendReservedLoginMessage(reservationJson);
        }

        [Preserve]
        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr, float>))]
        public static void HandleSuccessNumber(System.IntPtr taskPtr, float result) => HandleSuccess(taskPtr, result);

        [Preserve]
        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr, string>))]
        public static void HandleSuccessString(System.IntPtr taskPtr, string result) => HandleSuccess(taskPtr, result);

        [Preserve]
        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr, string>))]
        public static void HandleErrorNumber(System.IntPtr taskPtr, string error) => HandleError<float>(taskPtr, error);

        [Preserve]
        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr, string>))]
        public static void HandleErrorString(System.IntPtr taskPtr, string error) => HandleError<string>(taskPtr, error);

        [Preserve]
        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr, string>))]
        public static void HandleError(System.IntPtr taskPtr, string error)
        {
            if (taskPtr == IntPtr.Zero)
            {
                UnityEngine.Debug.LogError($"[JestSDK] HandleError received null pointer. Error: {error}");
                return;
            }
            GCHandle handle = GCHandle.FromIntPtr(taskPtr);
            var task = (JestSDKTask)handle.Target;
            task.SetException(new System.Exception(error));
            handle.Free();
        }

        [Preserve]
        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr>))]
        public static void HandleSuccess(System.IntPtr taskPtr)
        {
            if (taskPtr == IntPtr.Zero)
            {
                UnityEngine.Debug.LogError("[JestSDK] HandleSuccess received null pointer");
                return;
            }
            GCHandle handle = GCHandle.FromIntPtr(taskPtr);
            var task = (JestSDKTask)handle.Target;
            task.SetResult();
            handle.Free();
        }

        public static void HandleSuccess<T>(System.IntPtr taskPtr, T result)
        {
            if (taskPtr == IntPtr.Zero)
            {
                UnityEngine.Debug.LogError($"[JestSDK] HandleSuccess<{typeof(T).Name}> received null pointer");
                return;
            }
            GCHandle handle = GCHandle.FromIntPtr(taskPtr);
            var task = (JestSDKTask<T>)handle.Target;
            task.SetResult(result);
            handle.Free();
        }

        public static void HandleError<T>(System.IntPtr taskPtr, string error)
        {
            if (taskPtr == IntPtr.Zero)
            {
                UnityEngine.Debug.LogError($"[JestSDK] HandleError<{typeof(T).Name}> received null pointer. Error: {error}");
                return;
            }
            GCHandle handle = GCHandle.FromIntPtr(taskPtr);
            var task = (JestSDKTask<T>)handle.Target;
            task.SetException(new System.Exception(error));
            handle.Free();
        }
    }
}
