mergeInto(LibraryManager.library, {
  $JestSDKHelper: {
    mockPlayer: { playerId: "mock-player-id", registered: false, data: {} },
    sdkChecked: false,
    usePostMessage: false,
    postMessageInitialized: false,
    pendingCalls: {},
    cachedPlayer: null,
    cachedEntryPayload: null,
    sdkContextId: null,
    sdkVersion: "unity-sdk-1.0",
    playerDataUpdateId: 0,

    // Get sdkContextId from URL parameter
    getSdkContextId: function() {
      if (JestSDKHelper.sdkContextId) return JestSDKHelper.sdkContextId;
      try {
        var params = new URLSearchParams(window.location.search);
        JestSDKHelper.sdkContextId = params.get('sdk_context_id') || 'unity-' + Date.now();
      } catch (e) {
        JestSDKHelper.sdkContextId = 'unity-' + Date.now();
      }
      return JestSDKHelper.sdkContextId;
    },

    // Generate conversation ID for request/response correlation
    generateConversationId: function() {
      try {
        return self.crypto.randomUUID();
      } catch (e) {
        return 'conv_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
      }
    },

    // Check if message is from TextClub
    isTextClubMessage: function(data) {
      return data && typeof data === 'object' && data.source_channel === 'textclub';
    },

    // Send message to parent using TextClub protocol
    sendMessage: function(message) {
      var fullMessage = {
        source_channel: 'textclub',
        sdkContextId: JestSDKHelper.getSdkContextId(),
        sdkVersion: JestSDKHelper.sdkVersion,
        conversationId: message.conversationId || JestSDKHelper.generateConversationId()
      };
      for (var key in message) {
        fullMessage[key] = message[key];
      }
      window.parent.postMessage(fullMessage, '*');
    },

    // Send message and wait for response
    sendAndWaitForResponse: function(message, responseType) {
      return new Promise(function(resolve, reject) {
        var conversationId = JestSDKHelper.generateConversationId();
        message.conversationId = conversationId;

        JestSDKHelper.pendingCalls[conversationId] = {
          resolve: resolve,
          reject: reject,
          responseType: responseType
        };

        // Timeout after 30 seconds
        setTimeout(function() {
          if (JestSDKHelper.pendingCalls[conversationId]) {
            delete JestSDKHelper.pendingCalls[conversationId];
            reject(new Error('PostMessage timeout: ' + message.type));
          }
        }, 30000);

        JestSDKHelper.sendMessage(message);
      });
    },

    // PostMessage bridge for cross-origin iframe communication (bypasses COEP)
    initPostMessageBridge: function() {
      if (JestSDKHelper.postMessageInitialized) return;
      JestSDKHelper.postMessageInitialized = true;

      window.addEventListener('message', function(event) {
        var data = event.data;
        if (!JestSDKHelper.isTextClubMessage(data)) return;

        // Handle SetPlayer - initial player data from parent
        if (data.type === 'SetPlayer') {
          console.log("[JestSDK] Received SetPlayer from parent");
          JestSDKHelper.cachedPlayer = data.player;
          // Entry payload comes in player.entryPayload or separately
          if (data.player && data.player.entryPayload) {
            JestSDKHelper.cachedEntryPayload = data.player.entryPayload;
          }
        }

        // Handle responses - match by conversationId
        if (data.conversationId && JestSDKHelper.pendingCalls[data.conversationId]) {
          var pending = JestSDKHelper.pendingCalls[data.conversationId];
          delete JestSDKHelper.pendingCalls[data.conversationId];

          // Handle different response types
          switch (data.type) {
            case 'GetGameProductsResponse':
              if (data.success) {
                pending.resolve(data.products || []);
              } else {
                pending.reject(new Error('Failed to get products'));
              }
              break;

            case 'BeginPlatformPurchaseResponse':
              if (data.result === 'success') {
                pending.resolve({
                  purchaseToken: data.purchase.purchaseToken,
                  status: 'success',
                  purchase: data.purchase,
                  purchaseSigned: data.purchaseSigned
                });
              } else if (data.result === 'cancel') {
                pending.resolve({ status: 'canceled' });
              } else {
                pending.reject(new Error(data.error || 'Purchase failed'));
              }
              break;

            case 'CompletePlatformPurchaseResponse':
              if (data.result === 'success') {
                pending.resolve({ success: true });
              } else {
                pending.reject(new Error(data.error || 'Complete purchase failed'));
              }
              break;

            case 'GetIncompletePurchasesResponse':
              if (data.success) {
                pending.resolve({
                  purchases: data.purchases || [],
                  purchasesSigned: data.purchasesSigned
                });
              } else {
                pending.reject(new Error('Failed to get incomplete purchases'));
              }
              break;

            case 'GetPlayerSignedResponse':
              if (data.success) {
                pending.resolve({
                  player: data.player,
                  playerSigned: data.playerSigned
                });
              } else {
                pending.reject(new Error('Failed to get signed player'));
              }
              break;

            case 'ShareReferralLinkResponse':
              if (data.success) {
                pending.resolve({ canceled: data.canceled });
              } else {
                pending.reject(new Error(data.error || 'Share failed'));
              }
              break;

            case 'GetReferralsResponse':
              if (data.success) {
                pending.resolve({
                  referrals: data.referrals,
                  referralsSigned: data.referralsSigned
                });
              } else {
                pending.reject(new Error(data.error || 'Failed to get referrals'));
              }
              break;

            case 'GetFeatureFlagResponse':
              if (data.success) {
                pending.resolve(data.value);
              } else {
                pending.reject(new Error(data.error || 'Failed to get feature flag'));
              }
              break;

            case 'LoginLeaseAcquired':
              if (data.success) {
                pending.resolve({ link: data.link });
              } else {
                pending.reject(new Error(data.error || 'Failed to acquire login lease'));
              }
              break;

            case 'AckUpdatePlayerData':
              pending.resolve();
              break;

            default:
              // Generic success/error handling
              if (data.success === false || data.error) {
                pending.reject(new Error(data.error || 'Request failed'));
              } else {
                pending.resolve(data);
              }
          }
        }
      });

      // Send Initialized message to parent
      JestSDKHelper.sendMessage({
        type: 'Initialized',
        sdkContextId: JestSDKHelper.getSdkContextId(),
        windowLocationHref: window.location.href
      });
      console.log("[JestSDK] PostMessage bridge initialized (TextClub protocol)");
    },

    // Create PostMessage-based SDK proxy using TextClub protocol
    createPostMessageSDK: function() {
      JestSDKHelper.initPostMessageBridge();
      var helper = JestSDKHelper;

      return {
        isReady: function() {
          // isReady is implicit - if we got SetPlayer, we're ready
          // Just return resolved promise since Initialized was sent
          return Promise.resolve();
        },
        getPlayer: function() {
          return helper.cachedPlayer || { playerId: "", registered: false, data: {} };
        },
        getEntryPayload: function() {
          return helper.cachedEntryPayload || {};
        },
        getPlayerDataVal: function(key) {
          var player = helper.cachedPlayer;
          return player && player.data ? player.data[key] : undefined;
        },
        setPlayerDataVal: function(key, value) {
          if (!helper.cachedPlayer) helper.cachedPlayer = { data: {} };
          if (!helper.cachedPlayer.data) helper.cachedPlayer.data = {};
          helper.cachedPlayer.data[key] = value;

          // Send UpdatePlayerData message
          var update = {};
          update[key] = value;
          helper.sendMessage({
            type: 'UpdatePlayerData',
            id: ++helper.playerDataUpdateId,
            update: update
          });
        },
        flush: function() {
          // Flush sends pending player data updates and waits for ack
          var id = helper.playerDataUpdateId;
          if (id === 0) return Promise.resolve();

          return helper.sendAndWaitForResponse({
            type: 'UpdatePlayerData',
            id: id,
            update: helper.cachedPlayer ? helper.cachedPlayer.data : {}
          }, 'AckUpdatePlayerData');
        },
        getPlayerSigned: function() {
          return helper.sendAndWaitForResponse({
            type: 'GetPlayerSigned'
          }, 'GetPlayerSignedResponse');
        },
        login: function(opts) {
          helper.sendMessage({
            type: 'BeginPlatformLogin',
            entryPayload: opts && opts.entryPayload ? JSON.stringify(opts.entryPayload) : undefined
          });
        },
        notifications: {
          scheduleNotification: function(opts) {
            // Convert to V2 format
            helper.sendMessage({
              type: 'ScheduleNotificationV2',
              identifier: opts.identifier,
              body: opts.body || opts.message,
              ctaText: opts.ctaText || 'Play Now',
              priority: opts.priority || 'medium',
              scheduledAt: opts.scheduledAt instanceof Date ? opts.scheduledAt.toISOString() : opts.scheduledAt,
              imageReference: opts.image,
              entryPayload: opts.entryPayload
            });
          },
          unscheduleNotification: function(opts) {
            helper.sendMessage({
              type: 'UnscheduleNotification',
              identifier: opts.identifier
            });
          },
        },
        payments: {
          getProducts: function() {
            return helper.sendAndWaitForResponse({
              type: 'GetGameProducts'
            }, 'GetGameProductsResponse');
          },
          beginPurchase: function(opts) {
            return helper.sendAndWaitForResponse({
              type: 'BeginPlatformPurchase',
              productSku: opts.productSku
            }, 'BeginPlatformPurchaseResponse');
          },
          completePurchase: function(opts) {
            return helper.sendAndWaitForResponse({
              type: 'CompletePlatformPurchase',
              purchaseToken: opts.purchaseToken
            }, 'CompletePlatformPurchaseResponse');
          },
          getIncompletePurchases: function() {
            return helper.sendAndWaitForResponse({
              type: 'GetIncompletePurchases'
            }, 'GetIncompletePurchasesResponse');
          },
        },
        referrals: {
          shareReferralLink: function(opts) {
            return helper.sendAndWaitForResponse({
              type: 'ShareReferralLink',
              reference: opts.reference,
              entryPayload: opts.entryPayload,
              shareTitle: opts.shareTitle,
              shareText: opts.shareText,
              onboardingSlug: opts.onboardingSlug
            }, 'ShareReferralLinkResponse');
          },
          listReferrals: function() {
            return helper.sendAndWaitForResponse({
              type: 'GetReferrals'
            }, 'GetReferralsResponse');
          },
        },
        internal: {
          redirectToGame: function(opts) {
            var msg = { type: 'RedirectToGame' };
            if (opts.redirectToFlagship) {
              msg.redirectToFlagship = true;
            } else {
              msg.gameSlug = opts.gameSlug;
            }
            msg.entryPayload = opts.entryPayload ? JSON.stringify(opts.entryPayload) : undefined;
            msg.skipGameExitConfirm = opts.skipGameExitConfirm;
            helper.sendMessage(msg);
          },
          redirectToExplorePage: function() {
            helper.sendMessage({ type: 'RedirectToExplorePage' });
          },
          openPrivacyPolicy: function() {
            helper.sendMessage({ type: 'OpenLegalPage', page: 'privacy' });
          },
          openTermsOfService: function() {
            helper.sendMessage({ type: 'OpenLegalPage', page: 'terms' });
          },
          openCopyright: function() {
            helper.sendMessage({ type: 'OpenLegalPage', page: 'copyright' });
          },
          getFeatureFlag: function(key) {
            return helper.sendAndWaitForResponse({
              type: 'GetFeatureFlag',
              key: key
            }, 'GetFeatureFlagResponse');
          },
          reserveLoginMessageAsync: function(opts) {
            return helper.sendAndWaitForResponse({
              type: 'AcquireLoginLease',
              message: opts.message,
              replyMessage: opts.replyMessage || null,
              reminderMessage: opts.reminderMessage || null,
              keywords: opts.keywords || null,
              entryPayload: opts.entryPayload ? JSON.stringify(opts.entryPayload) : undefined
            }, 'LoginLeaseAcquired');
          },
          sendReservedLoginMessage: function(res) {
            // This is handled by the platform after login lease is acquired
            // The link from LoginLeaseAcquired response is used directly
            console.log("[JestSDK] sendReservedLoginMessage - use the link from reserveLoginMessageAsync");
          },
        },
      };
    },

    ensureSDK: function() {
      if (typeof window !== 'undefined' && !window.JestSDK) {
        // Check if we're running inside an iframe (platform)
        var inIframe = false;
        try {
          inIframe = window.self !== window.top;
        } catch (e) {
          inIframe = true; // Cross-origin iframe
        }

        if (inIframe) {
          // Running inside platform iframe - try to access parent SDK
          var canAccessParent = false;
          try {
            // This will throw if cross-origin (COEP blocked)
            canAccessParent = !!window.parent.JestSDK;
          } catch (e) {
            canAccessParent = false;
          }

          if (canAccessParent) {
            // Direct access works - use parent's SDK
            console.log("[JestSDK] Using parent window SDK directly");
            window.JestSDK = window.parent.JestSDK;
            return;
          }

          // Cross-origin or COEP blocked - use postMessage bridge
          if (!JestSDKHelper.sdkChecked) {
            JestSDKHelper.sdkChecked = true;
            console.log("[JestSDK] Cross-origin iframe detected, using postMessage bridge");
          }
          JestSDKHelper.usePostMessage = true;
          window.JestSDK = JestSDKHelper.createPostMessageSDK();
          return;
        }

        console.warn("[JestSDK] No SDK found, creating mock for standalone testing");
        var mp = JestSDKHelper.mockPlayer;
        window.JestSDK = {
          isReady: function() { return Promise.resolve(); },
          getPlayer: function() { return mp; },
          getEntryPayload: function() { return {}; },
          getPlayerDataVal: function(key) { return mp.data[key]; },
          setPlayerDataVal: function(key, value) { mp.data[key] = value; },
          flush: function() { return Promise.resolve(); },
          getPlayerSigned: function() { return Promise.resolve({ player: mp, playerSigned: "mock-signed" }); },
          login: function(opts) { console.log("[Mock] login:", opts); },
          notifications: {
            scheduleNotification: function(opts) { console.log("[Mock] scheduleNotification:", opts); },
            unscheduleNotification: function(opts) { console.log("[Mock] unscheduleNotification:", opts); },
          },
          payments: {
            getProducts: function() { return Promise.resolve([]); },
            beginPurchase: function(opts) { return Promise.resolve({ purchaseToken: "mock-token", status: "mock" }); },
            completePurchase: function(opts) { return Promise.resolve({ success: true }); },
            getIncompletePurchases: function() { return Promise.resolve({ purchases: [] }); },
          },
          referrals: {
            shareReferralLink: function(opts) { console.log("[Mock] shareReferralLink:", opts); return Promise.resolve({ canceled: false }); },
            listReferrals: function() { return Promise.resolve({ referrals: {}, referralsSigned: "mock-signed" }); },
          },
          internal: {
            redirectToGame: function(opts) { console.log("[Mock] redirectToGame:", opts); },
            redirectToExplorePage: function() { console.log("[Mock] redirectToExplorePage"); },
            openPrivacyPolicy: function() { console.log("[Mock] openPrivacyPolicy"); },
            openTermsOfService: function() { console.log("[Mock] openTermsOfService"); },
            openCopyright: function() { console.log("[Mock] openCopyright"); },
            getFeatureFlag: function(key) { return Promise.resolve(undefined); },
            reserveLoginMessageAsync: function(opts) { return Promise.resolve({ reservationId: "mock-id" }); },
            sendReservedLoginMessage: function(res) { console.log("[Mock] sendReservedLoginMessage:", res); },
          },
        };
      }
    },
    marshalString: function(value) {
      if (value === undefined || value === null) value = "";
      var str = String(value);
      var length = lengthBytesUTF8(str) + 1;
      var ptr = _malloc(length);
      if (!ptr) return 0;
      stringToUTF8(str, ptr, length);
      return ptr;
    }
  },

  JS_getPlayerId__deps: ['$JestSDKHelper'],
  JS_getPlayerId: function () {
    JestSDKHelper.ensureSDK();
    const val = window.JestSDK.getPlayer().playerId;
    return JestSDKHelper.marshalString(val);
  },

  JS_getPlayerData__deps: ['$JestSDKHelper'],
  JS_getPlayerData: function () {
    JestSDKHelper.ensureSDK();
    const val = window.JestSDK.getPlayer();
    return JestSDKHelper.marshalString(JSON.stringify(val));
  },

  JS_getEntryPayload__deps: ['$JestSDKHelper'],
  JS_getEntryPayload: function () {
    JestSDKHelper.ensureSDK();
    const payload = window.JestSDK.getEntryPayload();
    return JestSDKHelper.marshalString(JSON.stringify(payload));
  },

  JS_getIsRegistered__deps: ['$JestSDKHelper'],
  JS_getIsRegistered: function () {
    JestSDKHelper.ensureSDK();
    const val = window.JestSDK.getPlayer().registered ? "true" : "false";
    return JestSDKHelper.marshalString(val);
  },

  JS_getPlayerValue__deps: ['$JestSDKHelper'],
  JS_getPlayerValue: function (key) {
    JestSDKHelper.ensureSDK();
    const val = window.JestSDK.getPlayerDataVal(UTF8ToString(key));
    return JestSDKHelper.marshalString(val);
  },

  JS_setPlayerValue__deps: ['$JestSDKHelper'],
  JS_setPlayerValue: function (key, value) {
    JestSDKHelper.ensureSDK();
    window.JestSDK.setPlayerDataVal(UTF8ToString(key), UTF8ToString(value));
  },

  JS_deletePlayerValue__deps: ['$JestSDKHelper'],
  JS_deletePlayerValue: function (key) {
    JestSDKHelper.ensureSDK();
    window.JestSDK.setPlayerDataVal(UTF8ToString(key), undefined);
  },

  JS_flush__deps: ['$JestSDKHelper'],
  JS_flush: function (taskPtr, successCallback, errorCallback) {
    JestSDKHelper.ensureSDK();
    window.JestSDK.flush()
      .then(function () {
        {{{ makeDynCall("vi", 'successCallback') }}}(taskPtr);
      })
      .catch(function (err) {
        var msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_scheduleNotificationV2__deps: ['$JestSDKHelper'],
  JS_scheduleNotificationV2: function (options) {
    JestSDKHelper.ensureSDK();
    let opts = JSON.parse(UTF8ToString(options));
    opts.scheduledAt = new Date(opts.scheduledAt);
    if (opts.entryPayload) {
      opts.entryPayload = JSON.parse(opts.entryPayload);
    }
    if (typeof opts.image === "string" && opts.image === "") {
      delete opts.image;
    }
    window.JestSDK.notifications.scheduleNotification(opts);
  },

  JS_unscheduleNotificationV2__deps: ['$JestSDKHelper'],
  JS_unscheduleNotificationV2: function (identifier) {
    JestSDKHelper.ensureSDK();
    window.JestSDK.notifications.unscheduleNotification({
      identifier: UTF8ToString(identifier),
    });
  },

  JS_login__deps: ['$JestSDKHelper'],
  JS_login: function (payload) {
    JestSDKHelper.ensureSDK();
    const payloadJson = UTF8ToString(payload);
    let data = {};
    try {
      data = JSON.parse(payloadJson);
    } catch (e) {
      console.error("Invalid JSON passed:", payloadJson);
    }
    console.log("➡ Received from Unity:", data);
    window.JestSDK.login({ entryPayload: data });
  },

  JS_initSdk__deps: ['$JestSDKHelper'],
  JS_initSdk: function (taskPtr, successCallback, errorCallback) {
    // Check if we're in an iframe
    var inIframe = false;
    try {
      inIframe = window.self !== window.top;
    } catch (e) {
      inIframe = true;
    }

    function proceedWithInit() {
      JestSDKHelper.ensureSDK();
      window.JestSDK.isReady()
        .then(function () {
          console.log("[JestSDK] SDK initialized successfully");
          {{{ makeDynCall('vi', 'successCallback') }}}(taskPtr);
        })
        .catch(function (err) {
          var msg = err && err.message ? err.message : String(err);
          {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
        });
    }

    if (inIframe && !window.JestSDK) {
      console.log("[JestSDK] Running in iframe, checking access mode...");

      // Try to access parent SDK directly first
      var canAccessParent = false;
      try {
        canAccessParent = !!window.parent.JestSDK;
      } catch (e) {
        console.log("[JestSDK] Cannot access parent directly (COEP/cross-origin), using TextClub postMessage bridge");
      }

      if (canAccessParent) {
        // Direct access works
        console.log("[JestSDK] Direct parent access available");
        proceedWithInit();
      } else {
        // Use postMessage bridge with TextClub protocol
        // This sends Initialized message and waits for SetPlayer
        JestSDKHelper.ensureSDK();

        // Wait for SetPlayer message (cachedPlayer will be set)
        var maxWaitMs = 5000;
        var pollIntervalMs = 100;
        var waited = 0;

        var pollInterval = setInterval(function() {
          waited += pollIntervalMs;
          if (JestSDKHelper.cachedPlayer) {
            clearInterval(pollInterval);
            console.log("[JestSDK] Received SetPlayer via postMessage after " + waited + "ms");
            proceedWithInit();
          } else if (waited >= maxWaitMs) {
            clearInterval(pollInterval);
            console.warn("[JestSDK] Timeout waiting for SetPlayer after " + maxWaitMs + "ms, proceeding anyway");
            proceedWithInit();
          }
        }, pollIntervalMs);
      }
    } else {
      // Not in iframe, or SDK already present
      proceedWithInit();
    }
  },

  JS_callAsyncVoid__deps: ['$JestSDKHelper'],
  JS_callAsyncVoid: function (
    taskPtr,
    callName,
    successCallback,
    errorCallback
  ) {
    JestSDKHelper.ensureSDK();
    var name = UTF8ToString(callName);
    console.log("[JestSDK] JS_callAsyncVoid called:", name);

    if (typeof window.JestSDK[name] !== 'function') {
      console.error("[JestSDK] Method not found:", name);
      {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString("Method not found: " + name));
      return;
    }

    window.JestSDK[name]()
      .then(function () {
        console.log("[JestSDK] JS_callAsyncVoid success:", name);
        {{{ makeDynCall("vi", 'successCallback') }}}(taskPtr);
      })
      .catch(function (err) {
        console.error("[JestSDK] JS_callAsyncVoid error:", name, err);
        var msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_callAsyncNumber__deps: ['$JestSDKHelper'],
  JS_callAsyncNumber: function (
    taskPtr,
    callName,
    successCallback,
    errorCallback
  ) {
    JestSDKHelper.ensureSDK();
    window.JestSDK[UTF8ToString(callName)]()
      .then(function (result) {
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(result));
      })
      .catch(function (err) {
        var msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_callAsyncString__deps: ['$JestSDKHelper'],
  JS_callAsyncString: function (
    taskPtr,
    callName,
    successCallback,
    errorCallback
  ) {
    JestSDKHelper.ensureSDK();
    window.JestSDK[UTF8ToString(callName)]()
      .then(function (result) {
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(result));
      })
      .catch(function (err) {
        var msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_getProducts__deps: ['$JestSDKHelper'],
  JS_getProducts: function (taskPtr, successCallback, errorCallback) {
    JestSDKHelper.ensureSDK();
    window.JestSDK.payments.getProducts()
      .then(function (products) {
        console.log("[JestSDK] getProducts success:", products);
        const json = JSON.stringify(products);
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(json));
      })
      .catch(function (err) {
        console.error("[JestSDK] getProducts error:", err);
        const msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_beginPurchase__deps: ['$JestSDKHelper'],
  JS_beginPurchase: function (taskPtr, sku, successCallback, errorCallback) {
    JestSDKHelper.ensureSDK();
    const productSku = UTF8ToString(sku);
    window.JestSDK.payments.beginPurchase({ productSku })
      .then(function (result) {
        const json = JSON.stringify(result);
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(json));
      })
      .catch(function (err) {
        const msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_completePurchase__deps: ['$JestSDKHelper'],
  JS_completePurchase: function (taskPtr, purchaseToken, successCallback, errorCallback) {
    JestSDKHelper.ensureSDK();
    const token = UTF8ToString(purchaseToken);
    window.JestSDK.payments.completePurchase({ purchaseToken: token })
      .then(function (result) {
        const json = JSON.stringify(result);
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(json));
      })
      .catch(function (err) {
        const msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_getIncompletePurchases__deps: ['$JestSDKHelper'],
  JS_getIncompletePurchases: function (taskPtr, successCallback, errorCallback) {
    JestSDKHelper.ensureSDK();
    window.JestSDK.payments.getIncompletePurchases()
      .then(function (result) {
        const json = JSON.stringify(result);
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(json));
      })
      .catch(function (err) {
        const msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_openReferralDialog__deps: ['$JestSDKHelper'],
  JS_openReferralDialog: function (taskPtr, optionsJson, successCallback, errorCallback) {
    JestSDKHelper.ensureSDK();
    try {
      const options = JSON.parse(UTF8ToString(optionsJson));
      const result = window.JestSDK.referrals.shareReferralLink(options);

      // Handle both promise and non-promise returns
      if (result && typeof result.then === 'function') {
        result
          .then(function () {
            {{{ makeDynCall("vi", 'successCallback') }}}(taskPtr);
          })
          .catch(function (err) {
            const msg = err && err.message ? err.message : String(err);
            {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
          });
      } else {
        // Synchronous call - immediately succeed
        {{{ makeDynCall("vi", 'successCallback') }}}(taskPtr);
      }
    } catch (err) {
      const msg = err && err.message ? err.message : String(err);
      {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
    }
  },

  JS_listReferrals__deps: ['$JestSDKHelper'],
  JS_listReferrals: function (taskPtr, successCallback, errorCallback) {
    JestSDKHelper.ensureSDK();
    // Transform API response format to match C# model
    // API returns: {referrals: {"ref1": ["user1"], "ref2": []}, referralsSigned: "..."}
    // C# expects:  {referrals: [{reference: "ref1", registrations: ["user1"]}, ...], referralsSigned: "..."}
    function transformResponse(data) {
      if (!data) return {referrals: [], referralsSigned: ""};

      const referralsArray = [];
      if (data.referrals && typeof data.referrals === 'object' && !Array.isArray(data.referrals)) {
        for (const reference in data.referrals) {
          referralsArray.push({
            reference: reference,
            registrations: data.referrals[reference] || []
          });
        }
      } else if (Array.isArray(data.referrals)) {
        // Already in array format
        return data;
      }

      return {
        referrals: referralsArray,
        referralsSigned: data.referralsSigned || ""
      };
    }

    try {
      const result = window.JestSDK.referrals.listReferrals();

      // Handle both promise and non-promise returns
      if (result && typeof result.then === 'function') {
        result
          .then(function (data) {
            const transformed = transformResponse(data);
            const json = JSON.stringify(transformed);
            {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(json));
          })
          .catch(function (err) {
            const msg = err && err.message ? err.message : String(err);
            {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
          });
      } else {
        // Synchronous call - result is the data directly
        const transformed = transformResponse(result);
        const json = JSON.stringify(transformed);
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(json));
      }
    } catch (err) {
      const msg = err && err.message ? err.message : String(err);
      {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
    }
  },

  JS_redirectToGame__deps: ['$JestSDKHelper'],
  JS_redirectToGame: function (optionsJson) {
    JestSDKHelper.ensureSDK();
    const options = JSON.parse(UTF8ToString(optionsJson));
    window.JestSDK.internal.redirectToGame(options);
  },

  JS_openLegalPage__deps: ['$JestSDKHelper'],
  JS_openLegalPage: function (page) {
    JestSDKHelper.ensureSDK();
    const pageType = UTF8ToString(page);
    switch (pageType) {
      case "privacy":
        window.JestSDK.internal.openPrivacyPolicy();
        break;
      case "terms":
        window.JestSDK.internal.openTermsOfService();
        break;
      case "copyright":
        window.JestSDK.internal.openCopyright();
        break;
      default:
        console.warn("[JestSDK] Unknown legal page type:", pageType);
    }
  },

  JS_getPlayerSigned__deps: ['$JestSDKHelper'],
  JS_getPlayerSigned: function (taskPtr, successCallback, errorCallback) {
    JestSDKHelper.ensureSDK();
    window.JestSDK.getPlayerSigned()
      .then(function (result) {
        const json = JSON.stringify(result);
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(json));
      })
      .catch(function (err) {
        const msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_debugRegister: function () {
    const random = Math.floor(Math.random() * 10000000).toString().padStart(7, "0");
    const phoneNumber = "+1555" + random;
    window.parent.postMessage({ type: "debug-register", phoneNumber: phoneNumber }, "*");
  },

  JS_redirectToExplorePage__deps: ['$JestSDKHelper'],
  JS_redirectToExplorePage: function () {
    JestSDKHelper.ensureSDK();
    window.JestSDK.internal.redirectToExplorePage();
  },

  JS_getFeatureFlag__deps: ['$JestSDKHelper'],
  JS_getFeatureFlag: function (taskPtr, key, successCallback, errorCallback) {
    JestSDKHelper.ensureSDK();
    const flagKey = UTF8ToString(key);
    window.JestSDK.internal.getFeatureFlag(flagKey)
      .then(function (result) {
        const value = result !== undefined ? result : "";
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(value));
      })
      .catch(function (err) {
        const msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_reserveLoginMessage__deps: ['$JestSDKHelper'],
  JS_reserveLoginMessage: function (taskPtr, optionsJson, successCallback, errorCallback) {
    JestSDKHelper.ensureSDK();
    const options = JSON.parse(UTF8ToString(optionsJson));
    window.JestSDK.internal.reserveLoginMessageAsync(options)
      .then(function (result) {
        const json = JSON.stringify(result);
        {{{ makeDynCall("vii", 'successCallback') }}}(taskPtr, JestSDKHelper.marshalString(json));
      })
      .catch(function (err) {
        const msg = err && err.message ? err.message : String(err);
        {{{ makeDynCall("vii", 'errorCallback') }}}(taskPtr, JestSDKHelper.marshalString(msg));
      });
  },

  JS_sendReservedLoginMessage__deps: ['$JestSDKHelper'],
  JS_sendReservedLoginMessage: function (reservationJson) {
    JestSDKHelper.ensureSDK();
    const reservation = JSON.parse(UTF8ToString(reservationJson));
    window.JestSDK.internal.sendReservedLoginMessage(reservation);
  },

});
