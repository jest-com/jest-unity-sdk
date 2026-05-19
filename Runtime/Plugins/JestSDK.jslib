mergeInto(LibraryManager.library, {
  $JestSDKHelper: {
    sdkUrl: "https://cdn.jest.com/sdk/latest/jestsdk.js",
    sdkLoadPromise: null,
    initialized: false,
    sdkVersion: "unity-sdk-unknown",
    overlayHandles: {},

    loadSdk: function () {
      if (typeof window === "undefined") {
        return Promise.reject(new Error("JestSDK requires a browser window"));
      }

      if (window.JestSDK) {
        return Promise.resolve(window.JestSDK);
      }

      if (JestSDKHelper.sdkLoadPromise) {
        return JestSDKHelper.sdkLoadPromise;
      }

      JestSDKHelper.sdkLoadPromise = new Promise(function (resolve, reject) {
        var script = document.createElement("script");
        script.src = JestSDKHelper.sdkUrl;
        script.async = true;
        script.onload = function () {
          if (window.JestSDK) {
            resolve(window.JestSDK);
          } else {
            reject(new Error("Loaded jestsdk.js, but window.JestSDK was not defined"));
          }
        };
        script.onerror = function () {
          reject(new Error("Failed to load " + JestSDKHelper.sdkUrl));
        };
        document.head.appendChild(script);
      });

      return JestSDKHelper.sdkLoadPromise;
    },

    getSdk: function () {
      if (!window.JestSDK) {
        throw new Error("JestSDK is not initialized. Call JestSDK.Instance.Init() first.");
      }
      return window.JestSDK;
    },

    marshalString: function (value) {
      if (value === undefined || value === null) value = "";
      var str = String(value);
      var length = lengthBytesUTF8(str) + 1;
      var ptr = _malloc(length);
      if (!ptr) return 0;
      stringToUTF8(str, ptr, length);
      return ptr;
    },

    stringifyValue: function (value) {
      if (value === undefined || value === null) return "";
      if (typeof value === "string") return value;
      try {
        return JSON.stringify(value);
      } catch (e) {
        return String(value);
      }
    },

    parseJson: function (jsonPtr) {
      var raw = UTF8ToString(jsonPtr);
      if (!raw) return {};
      return JSON.parse(raw);
    },

    toErrorMessage: function (err) {
      return err && err.message ? err.message : String(err);
    },

    asPromise: function (value) {
      if (value && typeof value.then === "function") {
        return value;
      }
      return Promise.resolve(value);
    },

    callVoidTask: function (taskPtr, successCallback, errorCallback, fn) {
      try {
        JestSDKHelper.asPromise(fn())
          .then(function () {
            {{{ makeDynCall("vi", 'successCallback') }}}(taskPtr);
          })
          .catch(function (err) {
            {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(JestSDKHelper.toErrorMessage(err)));
          });
      } catch (err) {
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(JestSDKHelper.toErrorMessage(err)));
      }
    },

    callStringTask: function (taskPtr, successCallback, errorCallback, fn) {
      try {
        JestSDKHelper.asPromise(fn())
          .then(function (result) {
            {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(result));
          })
          .catch(function (err) {
            {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(JestSDKHelper.toErrorMessage(err)));
          });
      } catch (err) {
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(JestSDKHelper.toErrorMessage(err)));
      }
    },

    callNumberTask: function (taskPtr, successCallback, errorCallback, fn) {
      try {
        JestSDKHelper.asPromise(fn())
          .then(function (result) {
            {{{ makeDynCall("vif", 'successCallback') }}}(taskPtr, Number(result) || 0);
          })
          .catch(function (err) {
            {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(JestSDKHelper.toErrorMessage(err)));
          });
      } catch (err) {
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(JestSDKHelper.toErrorMessage(err)));
      }
    }
  },

  JS_initSdk__deps: ['$JestSDKHelper'],
  JS_initSdk: function (taskPtr, autoLoginReminders, sdkVersion, successCallback, errorCallback) {
    var version = UTF8ToString(sdkVersion);
    if (version) {
      JestSDKHelper.sdkVersion = version;
    }

    JestSDKHelper.loadSdk()
      .then(function (sdk) {
        return sdk.init({
          autoLoginReminders: !!autoLoginReminders,
          sdkVersion: JestSDKHelper.sdkVersion
        });
      })
      .then(function () {
        JestSDKHelper.initialized = true;
        {{{ makeDynCall("vi", 'successCallback') }}}(taskPtr);
      })
      .catch(function (err) {
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(JestSDKHelper.toErrorMessage(err)));
      });
  },

  JS_getEntryPayload__deps: ['$JestSDKHelper'],
  JS_getEntryPayload: function () {
    var payload = JestSDKHelper.getSdk().getEntryPayload();
    return JestSDKHelper.marshalString(JSON.stringify(payload || {}));
  },

  JS_getPlayerId__deps: ['$JestSDKHelper'],
  JS_getPlayerId: function () {
    var player = JestSDKHelper.getSdk().getPlayer();
    return JestSDKHelper.marshalString(player && player.playerId);
  },

  JS_getPlayerData__deps: ['$JestSDKHelper'],
  JS_getPlayerData: function () {
    var sdk = JestSDKHelper.getSdk();
    var data = sdk.getPlayerData ? sdk.getPlayerData() : sdk.data.getAll();
    var player = sdk.getPlayer();
    return JestSDKHelper.marshalString(JSON.stringify(Object.assign({}, player || {}, {
      playerData: data || {},
      data: data || {}
    })));
  },

  JS_getIsRegistered__deps: ['$JestSDKHelper'],
  JS_getIsRegistered: function () {
    var player = JestSDKHelper.getSdk().getPlayer();
    return JestSDKHelper.marshalString(player && player.registered ? "true" : "false");
  },

  JS_getPlayerUsername__deps: ['$JestSDKHelper'],
  JS_getPlayerUsername: function () {
    var player = JestSDKHelper.getSdk().getPlayer();
    return JestSDKHelper.marshalString(player && player.username);
  },

  JS_getPlayerAvatarUrl__deps: ['$JestSDKHelper'],
  JS_getPlayerAvatarUrl: function () {
    var player = JestSDKHelper.getSdk().getPlayer();
    return JestSDKHelper.marshalString(player && player.avatarUrl);
  },

  JS_getBotAvatar__deps: ['$JestSDKHelper'],
  JS_getBotAvatar: function (username, size) {
    var result = JestSDKHelper.getSdk().social.getBotAvatar({
      username: UTF8ToString(username),
      size: size
    });
    return JestSDKHelper.marshalString(result);
  },

  JS_getPlayerAvatar__deps: ['$JestSDKHelper'],
  JS_getPlayerAvatar: function (size) {
    var profile = JestSDKHelper.getSdk().social.getProfile({ avatarSize: size });
    if (!profile || !profile.avatarUrl) {
      return 0;
    }
    return JestSDKHelper.marshalString(profile.avatarUrl);
  },

  JS_getProfile__deps: ['$JestSDKHelper'],
  JS_getProfile: function (size) {
    var profile = JestSDKHelper.getSdk().social.getProfile({ avatarSize: size });
    if (!profile) {
      return 0;
    }
    return JestSDKHelper.marshalString(JSON.stringify(profile));
  },

  JS_getPlayerValue__deps: ['$JestSDKHelper'],
  JS_getPlayerValue: function (key) {
    var value = JestSDKHelper.getSdk().data.get(UTF8ToString(key));
    return JestSDKHelper.marshalString(JestSDKHelper.stringifyValue(value));
  },

  JS_setPlayerValue__deps: ['$JestSDKHelper'],
  JS_setPlayerValue: function (key, value) {
    JestSDKHelper.getSdk().data.set(UTF8ToString(key), UTF8ToString(value));
  },

  JS_deletePlayerValue__deps: ['$JestSDKHelper'],
  JS_deletePlayerValue: function (key) {
    JestSDKHelper.getSdk().data.delete(UTF8ToString(key));
  },

  JS_flush__deps: ['$JestSDKHelper'],
  JS_flush: function (taskPtr, successCallback, errorCallback) {
    JestSDKHelper.callVoidTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().data.flush();
    });
  },

  JS_scheduleNotificationV2__deps: ['$JestSDKHelper'],
  JS_scheduleNotificationV2: function (taskPtr, options, successCallback, errorCallback) {
    JestSDKHelper.callVoidTask(taskPtr, successCallback, errorCallback, function () {
      var opts = JestSDKHelper.parseJson(options);
      if (opts.scheduledAt) {
        opts.scheduledAt = new Date(opts.scheduledAt);
      }
      if (typeof opts.entryPayload === "string") {
        opts.entryPayload = JSON.parse(opts.entryPayload);
      }
      return JestSDKHelper.getSdk().notifications.scheduleNotification(opts);
    });
  },

  JS_unscheduleNotificationV2__deps: ['$JestSDKHelper'],
  JS_unscheduleNotificationV2: function (taskPtr, identifier, successCallback, errorCallback) {
    JestSDKHelper.callVoidTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().notifications.unscheduleNotification({
        identifier: UTF8ToString(identifier)
      });
    });
  },

  JS_callAsyncVoid__deps: ['$JestSDKHelper'],
  JS_callAsyncVoid: function (taskPtr, callName, successCallback, errorCallback) {
    JestSDKHelper.callVoidTask(taskPtr, successCallback, errorCallback, function () {
      var sdk = JestSDKHelper.getSdk();
      var name = UTF8ToString(callName);
      if (typeof sdk[name] !== "function") {
        throw new Error("Method not found: " + name);
      }
      return sdk[name]();
    });
  },

  JS_callAsyncString__deps: ['$JestSDKHelper'],
  JS_callAsyncString: function (taskPtr, callName, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      var sdk = JestSDKHelper.getSdk();
      var name = UTF8ToString(callName);
      if (typeof sdk[name] !== "function") {
        throw new Error("Method not found: " + name);
      }
      return sdk[name]();
    });
  },

  JS_callAsyncNumber__deps: ['$JestSDKHelper'],
  JS_callAsyncNumber: function (taskPtr, callName, successCallback, errorCallback) {
    JestSDKHelper.callNumberTask(taskPtr, successCallback, errorCallback, function () {
      var sdk = JestSDKHelper.getSdk();
      var name = UTF8ToString(callName);
      if (typeof sdk[name] !== "function") {
        throw new Error("Method not found: " + name);
      }
      return sdk[name]();
    });
  },

  JS_login__deps: ['$JestSDKHelper'],
  JS_login: function (payload) {
    var entryPayload = {};
    var payloadJson = UTF8ToString(payload);
    if (payloadJson) {
      entryPayload = JSON.parse(payloadJson);
    }
    JestSDKHelper.getSdk().login({ entryPayload: entryPayload });
  },

  JS_getProducts__deps: ['$JestSDKHelper'],
  JS_getProducts: function (taskPtr, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().payments.getProducts().then(function (products) {
        return JSON.stringify(products);
      });
    });
  },

  JS_beginPurchase__deps: ['$JestSDKHelper'],
  JS_beginPurchase: function (taskPtr, sku, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().payments.beginPurchase({
        productSku: UTF8ToString(sku)
      }).then(function (result) {
        return JSON.stringify(result);
      });
    });
  },

  JS_completePurchase__deps: ['$JestSDKHelper'],
  JS_completePurchase: function (taskPtr, purchaseToken, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().payments.completePurchase({
        purchaseToken: UTF8ToString(purchaseToken)
      }).then(function (result) {
        return JSON.stringify(result);
      });
    });
  },

  JS_getIncompletePurchases__deps: ['$JestSDKHelper'],
  JS_getIncompletePurchases: function (taskPtr, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().payments.getIncompletePurchases().then(function (result) {
        return JSON.stringify(result);
      });
    });
  },

  JS_beginSubscription__deps: ['$JestSDKHelper'],
  JS_beginSubscription: function (taskPtr, subscriptionSku, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().payments.beginSubscription({
        subscriptionSku: UTF8ToString(subscriptionSku)
      }).then(function (result) {
        return JSON.stringify(result);
      });
    });
  },

  JS_openReferralDialog__deps: ['$JestSDKHelper'],
  JS_openReferralDialog: function (taskPtr, optionsJson, successCallback, errorCallback) {
    JestSDKHelper.callVoidTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().referrals.shareReferralLink(JestSDKHelper.parseJson(optionsJson));
    });
  },

  JS_listReferrals__deps: ['$JestSDKHelper'],
  JS_listReferrals: function (taskPtr, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().referrals.listReferrals().then(function (data) {
        var referralsArray = [];
        if (data && data.referrals && !Array.isArray(data.referrals)) {
          for (var reference in data.referrals) {
            referralsArray.push({
              reference: reference,
              registrations: data.referrals[reference] || []
            });
          }
        } else if (data && Array.isArray(data.referrals)) {
          referralsArray = data.referrals;
        }
        return JSON.stringify({
          referrals: referralsArray,
          referralsSigned: data && data.referralsSigned ? data.referralsSigned : ""
        });
      });
    });
  },

  JS_redirectToGame__deps: ['$JestSDKHelper'],
  JS_redirectToGame: function (optionsJson) {
    var sdk = JestSDKHelper.getSdk();
    var options = JestSDKHelper.parseJson(optionsJson);
    if (options.redirectToFlagship) {
      sdk.internal.redirectToFlagshipGame({
        entryPayload: options.entryPayload ? JSON.parse(options.entryPayload) : undefined
      });
      return;
    }
    sdk.internal.redirectToGame({
      gameSlug: options.gameSlug,
      entryPayload: options.entryPayload ? JSON.parse(options.entryPayload) : undefined,
      skipGameExitConfirm: options.skipGameExitConfirm
    });
  },

  JS_openLegalPage__deps: ['$JestSDKHelper'],
  JS_openLegalPage: function (page) {
    var sdk = JestSDKHelper.getSdk();
    switch (UTF8ToString(page)) {
      case "privacy":
        sdk.internal.openPrivacyPolicy();
        break;
      case "terms":
        sdk.internal.openTermsOfService();
        break;
      case "copyright":
        sdk.internal.openCopyright();
        break;
      default:
        console.warn("[JestSDK] Unknown legal page type");
    }
  },

  JS_getPlayerSigned__deps: ['$JestSDKHelper'],
  JS_getPlayerSigned: function (taskPtr, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().getPlayerSigned().then(function (result) {
        return JSON.stringify(result);
      });
    });
  },

  JS_debugRegister: function () {
    var random = Math.floor(Math.random() * 10000000).toString().padStart(7, "0");
    var phoneNumber = "+1555" + random;
    window.parent.postMessage({ type: "debug-register", phoneNumber: phoneNumber }, "*");
  },

  JS_redirectToExplorePage__deps: ['$JestSDKHelper'],
  JS_redirectToExplorePage: function () {
    JestSDKHelper.getSdk().internal.redirectToExplorePage();
  },

  JS_getFeatureFlag__deps: ['$JestSDKHelper'],
  JS_getFeatureFlag: function (taskPtr, key, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().internal.getFeatureFlag(UTF8ToString(key)).then(function (value) {
        return value === undefined || value === null ? "" : value;
      });
    });
  },

  JS_reserveLoginMessage__deps: ['$JestSDKHelper'],
  JS_reserveLoginMessage: function (taskPtr, optionsJson, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().internal.reserveLoginMessageAsync(JestSDKHelper.parseJson(optionsJson)).then(function (result) {
        return JSON.stringify(result);
      });
    });
  },

  JS_sendReservedLoginMessage__deps: ['$JestSDKHelper'],
  JS_sendReservedLoginMessage: function (reservationJson) {
    JestSDKHelper.getSdk().internal.sendReservedLoginMessage(JestSDKHelper.parseJson(reservationJson));
  },

  JS_setLoadingProgress__deps: ['$JestSDKHelper'],
  JS_setLoadingProgress: function (progress) {
    JestSDKHelper.getSdk().setLoadingProgress(progress);
  },

  JS_beginPlatformRegistrationOverlay__deps: ['$JestSDKHelper'],
  JS_beginPlatformRegistrationOverlay: function (taskPtr, optionsJson, onClose, onError) {
    try {
      var opts = JestSDKHelper.parseJson(optionsJson);
      var conversationId = opts.conversationId;
      var notified = false;
      var handle = JestSDKHelper.getSdk().showRegistrationOverlay({
        theme: opts.theme,
        entryPayload: opts.entryPayload,
        onClose: function () {
          if (notified) return;
          notified = true;
          if (conversationId) delete JestSDKHelper.overlayHandles[conversationId];
          {{{ makeDynCall("vi", 'onClose') }}}(taskPtr);
        }
      });
      if (conversationId) {
        JestSDKHelper.overlayHandles[conversationId] = handle;
      }
    } catch (err) {
      {{{ makeDynCall("vii", 'onError') }}}(taskPtr, JestSDKHelper.marshalString(JestSDKHelper.toErrorMessage(err)));
    }
  },

  JS_platformRegistrationOverlayLogin__deps: ['$JestSDKHelper'],
  JS_platformRegistrationOverlayLogin: function (conversationId) {
    var convId = UTF8ToString(conversationId);
    var handle = JestSDKHelper.overlayHandles[convId];
    if (handle && typeof handle.loginButtonAction === "function") {
      handle.loginButtonAction();
    }
  },

  JS_dismissPlatformRegistrationOverlay__deps: ['$JestSDKHelper'],
  JS_dismissPlatformRegistrationOverlay: function (conversationId) {
    var convId = UTF8ToString(conversationId);
    var handle = JestSDKHelper.overlayHandles[convId];
    if (handle && typeof handle.closeButtonAction === "function") {
      handle.closeButtonAction();
    }
  },

  JS_validateName__deps: ['$JestSDKHelper'],
  JS_validateName: function (taskPtr, name, successCallback, errorCallback) {
    JestSDKHelper.callStringTask(taskPtr, successCallback, errorCallback, function () {
      return JestSDKHelper.getSdk().internal.validateName(UTF8ToString(name)).then(function (result) {
        return JSON.stringify(result);
      });
    });
  },

  JS_captureOnboardingEvent__deps: ['$JestSDKHelper'],
  JS_captureOnboardingEvent: function (eventName, propertiesJson) {
    var name = UTF8ToString(eventName);
    var raw = UTF8ToString(propertiesJson);
    var props = undefined;
    if (raw) {
      props = JSON.parse(raw);
    }
    JestSDKHelper.getSdk().internal.captureOnboardingEvent(name, props);
  }
});
