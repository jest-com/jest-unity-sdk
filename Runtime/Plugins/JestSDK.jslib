mergeInto(LibraryManager.library, {
  JS_getPlayerId: function () {
    const val = window.JestSDK.getPlayer().playerId;
    return marshalString(val);
  },

  JS_getEntryPayload: function () {
    const payload = window.JestSDK.getEntryPayload();
    return marshalString(JSON.stringify(payload));
  },

  JS_getIsRegistered: function () {
    const val = window.JestSDK.getPlayer().registered ? "true" : "false";
    return marshalString(val);
  },

  JS_getPlayerValue: function (key) {
    const val = window.JestSDK.getPlayerDataVal(UTF8ToString(key));
    return marshalString(val);
  },

  JS_setPlayerValue: function (key, value) {
    window.JestSDK.setPlayerDataVal(UTF8ToString(key), UTF8ToString(value));
  },

  JS_scheduleNotification: function (options) {
    let opts = JSON.parse(UTF8ToString(options));

    opts.scheduledAt = new Date(opts.scheduledAt);
    opts.metadata = JSON.parse(opts.metadata);

    window.JestSDK.scheduleNotification(opts);
  },

  JS_captureEvent: function (eventName, properties) {
    window.JestSDK.captureEvent(
      UTF8ToString(eventName),
      JSON.parse(UTF8ToString(properties))
    );
  },

  JS_login: function (payload) {
    window.JestSDK.login(JSON.parse(UTF8ToString(payload)));
  },

  JS_initSdk: function (taskPtr, successCallback, errorCallback) {
    window.JestSdk.isReady()
      .then(function () {
        {{{ makeDynCall('vi', 'successCallback') }}}(taskPtr);
      })
      .catch(function (err) {
        var msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, marshalString(msg));
      });
  },

  JS_callAsyncVoid: function (
    taskPtr,
    callName,
    successCallback,
    errorCallback
  ) {
    window.JestSDK[UTF8ToString(callName)]()
      .then(function () {
        {{{ makeDynCall("vi", 'successCallback') }}}(taskPtr);
      })
      .catch(function (error) {
        var msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, marshalString(msg));
      });
  },

  JS_callAsyncNumber: function (
    taskPtr,
    callName,
    successCallback,
    errorCallback
  ) {
    window.JestSdk[UTF8ToString(callName)]
      .then(function (result) {
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, marshalString(result));
      })
      .catch(function (error) {
        var msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, marshalString(msg));
      });
  },

  JS_callAsyncString: function (
    taskPtr,
    callName,
    successCallback,
    errorCallback
  ) {
    window.JestSDK[UTF8ToString(callName)]
      .then(function (result) {
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, marshalString(result));
      })
      .catch(function (error) {
        var msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, marshalString(msg));
      });
  },
});
