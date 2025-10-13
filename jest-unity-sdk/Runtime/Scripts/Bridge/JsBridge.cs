using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace JestSDK
{
    internal static class JsBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        [DllImport("__Internal")]
        private static extern string JS_getEntryPayload();

        [DllImport("__Internal")]
        private static extern string JS_getPlayerId();

        [DllImport("__Internal")]
        private static extern string JS_getIsRegistered();

        [DllImport("__Internal")]
        private static extern string JS_getPlayerValue(string key);

        [DllImport("__Internal")]
        private static extern void JS_setPlayerValue(string key, string value);

        [DllImport("__Internal")]
        private static extern void JS_scheduleNotification(string options);

        [DllImport("__Internal")]
        private static extern void JS_captureEvent(string eventName, string properties);

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
#else
        private static string JS_getEntryPayload() { return _bridgeMock.GetEntryPayload(); }

        private static string JS_getPlayerId() { return _bridgeMock.playerId; }

        private static string JS_getIsRegistered() { return _bridgeMock.isRegistered; }

        private static string JS_getPlayerValue(string key) { return _bridgeMock.GetPlayerValue(key); }

        private static void JS_setPlayerValue(string key, string value) { _bridgeMock.SetPlayerValue(key, value); }

        private static void JS_scheduleNotification(string options) { _bridgeMock.ScheduleNotification(options); }

        private static void JS_captureEvent(string eventName, string properties) { _bridgeMock.CaptureEvent(eventName, properties); }

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

        internal static void ScheduleNotification(string options)
        {
            JS_scheduleNotification(options);
        }

        internal static void CaptureEvent(string eventName, string properties)
        {
            JS_captureEvent(eventName, properties);
        }

        internal static string GetEvent(string eventName)
        {
            return _bridgeMock.GetEvent(eventName);
        }

        internal static List<string> GetNotifications()
        {
            return _bridgeMock.GetNotifications();
        }

        internal static JestSDKTask Init()
        {
            return new JestSDKTask((System.IntPtr ptr) => { JS_initSdk(ptr, HandleSuccess, HandleError); });
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

        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr, float>))]
        public static void HandleSuccessNumber(System.IntPtr taskPtr, float result) => HandleSuccess(taskPtr, result);


        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr, string>))]
        public static void HandleSuccessString(System.IntPtr taskPtr, string result) => HandleSuccess(taskPtr, result);


        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr, string>))]
        public static void HandleErrorNumber(System.IntPtr taskPtr, string error) => HandleError<float>(taskPtr, error);


        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr, string>))]
        public static void HandleErrorString(System.IntPtr taskPtr, string error) => HandleError<string>(taskPtr, error);


        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr, string>))]
        public static void HandleError(System.IntPtr taskPtr, string error)
        {
            GCHandle handle = GCHandle.FromIntPtr(taskPtr);
            var task = (JestSDKTask)handle.Target;
            task.SetException(new System.Exception(error));
            handle.Free();
        }

        [MonoPInvokeCallback(typeof(System.Action<System.IntPtr>))]
        public static void HandleSuccess(System.IntPtr taskPtr)
        {
            GCHandle handle = GCHandle.FromIntPtr(taskPtr);
            var task = (JestSDKTask)handle.Target;
            task.SetResult();
            handle.Free();
        }

        public static void HandleSuccess<T>(System.IntPtr taskPtr, T result)
        {
            GCHandle handle = GCHandle.FromIntPtr(taskPtr);
            var task = (JestSDKTask<T>)handle.Target;
            task.SetResult(result);
            handle.Free();
        }

        public static void HandleError<T>(System.IntPtr taskPtr, string error)
        {
            GCHandle handle = GCHandle.FromIntPtr(taskPtr);
            var task = (JestSDKTask<T>)handle.Target;
            task.SetException(new System.Exception(error));
            handle.Free();
        }
    }
}
