mergeInto(LibraryManager.library, {
  JS_SdkRegressionListen: function (gameObjectNamePtr) {
    var gameObjectName = UTF8ToString(gameObjectNamePtr);

    if (!window.__jestSdkRegressionBridge) {
      window.__jestSdkRegressionBridge = {
        gameObjectName: gameObjectName,
        listening: false
      };
    }

    var bridge = window.__jestSdkRegressionBridge;
    bridge.gameObjectName = gameObjectName;

    if (bridge.listening) {
      return;
    }

    bridge.listening = true;
    window.addEventListener("message", function (event) {
      var data = event.data;
      if (!data || typeof data !== "object") {
        return;
      }

      if (
        data.source !== "textclub-sdk-regression" ||
        data.protocolVersion !== 1 ||
        data.type !== "runScenario" ||
        data.engine !== "unity"
      ) {
        return;
      }

      SendMessage(bridge.gameObjectName, "HandleRegressionCommand", JSON.stringify(data));
    });
  },

  JS_SdkRegressionPostMessage: function (jsonPtr) {
    var json = UTF8ToString(jsonPtr);
    var message = JSON.parse(json);
    window.parent.postMessage(message, "*");
  }
});
