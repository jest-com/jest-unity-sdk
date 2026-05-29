using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using com.jest.sdk;
using UnityEngine;

namespace com.jest.sdk.regression
{
    public sealed class SdkRegressionRunner : MonoBehaviour
    {
        private const int ProtocolVersion = 1;
        private const string CommandSource = "textclub-sdk-regression";
        private const string SampleSource = "jest-sdk-sample-regression";
        private const string Engine = "unity";
        private const string RunnerObjectName = "JestSdkRegressionRunner";

        private static readonly string[] Scenarios =
        {
            "core",
            "player-data",
            "commerce-read",
            "commerce-errors",
            "notifications",
            "social",
            "referrals-read",
            "referrals-share",
            "internal",
            "legal",
            "guardrails"
        };
        private static SdkRegressionRunner s_instance;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JS_SdkRegressionListen(string gameObjectName);

        [DllImport("__Internal")]
        private static extern void JS_SdkRegressionPostMessage(string json);
#else
        private static void JS_SdkRegressionListen(string gameObjectName) { }

        private static void JS_SdkRegressionPostMessage(string json)
        {
            Debug.Log("[SdkRegressionRunner] " + json);
        }
#endif

        public static void EnsureStarted()
        {
            if (s_instance != null)
            {
                s_instance.PostRunnerReady();
                return;
            }

            var runnerObject = new GameObject(RunnerObjectName);
            DontDestroyOnLoad(runnerObject);
            s_instance = runnerObject.AddComponent<SdkRegressionRunner>();
            s_instance.PostRunnerReady();
        }

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            gameObject.name = RunnerObjectName;
            JS_SdkRegressionListen(RunnerObjectName);
        }

        public async void HandleRegressionCommand(string json)
        {
            var assertions = new List<RegressionAssertion>();
            RunScenarioCommand command = null;

            try
            {
                command = com.jest.sdk.Convert.FromString<RunScenarioCommand>(json);
                if (!IsSupportedCommand(command))
                {
                    return;
                }

                PostLog(command, "info", "Running Unity SDK regression scenario: " + command.scenario);
                assertions = await RunScenario(command);
                PostScenarioResult(command, assertions, null);
            }
            catch (Exception exception)
            {
                if (command != null && IsSupportedCommand(command))
                {
                    assertions.Add(RegressionAssertion.Fail(
                        "scenario completed without exception",
                        exception.GetType().Name,
                        "no exception",
                        exception.Message));
                    PostScenarioResult(command, assertions, exception);
                    return;
                }

                PostLog(null, "error", "Failed to handle SDK regression command: " + exception.Message);
            }
        }

        private static bool IsSupportedCommand(RunScenarioCommand command)
        {
            return command != null
                && command.source == CommandSource
                && command.protocolVersion == ProtocolVersion
                && command.type == "runScenario"
                && command.engine == Engine;
        }

        private static Task<List<RegressionAssertion>> RunScenario(RunScenarioCommand command)
        {
            switch (command.scenario)
            {
                case "core":
                    return RunCoreScenario(command);
                case "player-data":
                    return RunPlayerDataScenario(command);
                case "commerce-read":
                    return RunCommerceReadScenario();
                case "commerce-errors":
                    return RunCommerceErrorsScenario();
                case "notifications":
                    return RunNotificationsScenario(command);
                case "social":
                    return RunSocialScenario();
                case "referrals-read":
                    return RunReferralsReadScenario();
                case "referrals-share":
                    return RunReferralsShareScenario(command);
                case "internal":
                    return RunInternalScenario(command);
                case "legal":
                    return RunLegalScenario();
                case "guardrails":
                    return RunGuardrailsScenario();
                default:
                    throw new InvalidOperationException("Unknown SDK regression scenario: " + command.scenario);
            }
        }

        private static Task<List<RegressionAssertion>> RunCoreScenario(RunScenarioCommand command)
        {
            var assertions = new List<RegressionAssertion>();
            var expectedPayload = command.options?.entryPayloadExpected ?? new Dictionary<string, object>();
            var actualPayload = JestSDK.Instance.GetEntryPayload();

            foreach (var expected in expectedPayload)
            {
                actualPayload.TryGetValue(expected.Key, out var actual);
                assertions.Add(RegressionAssertion.Equal(
                    "entry payload includes " + expected.Key,
                    actual,
                    expected.Value));
            }

            var playerId = JestSDK.Instance.Player.id;
            assertions.Add(RegressionAssertion.Condition(
                "player id is available",
                !string.IsNullOrWhiteSpace(playerId),
                playerId,
                "non-empty player id"));

            var isRegistered = JestSDK.Instance.Player.isRegistered;
            assertions.Add(RegressionAssertion.Condition(
                "player registration state is readable",
                true,
                isRegistered,
                "boolean"));

            assertions.Add(RegressionAssertion.Condition(
                "player username property is readable",
                true,
                JestSDK.Instance.Player.username,
                "string or null"));
            assertions.Add(RegressionAssertion.Condition(
                "player avatar property is readable",
                true,
                JestSDK.Instance.Player.avatarUrl,
                "string or null"));

            JestSDK.Instance.SetLoadingProgress(50);
            JestSDK.Instance.SetLoadingProgress(100);
            JestSDK.Instance.MarkGameLoaded();
            JestSDK.Instance.MarkGameLoaded(); // second call must be a no-op
            assertions.Add(RegressionAssertion.Condition(
                "mark game loaded completed without error",
                true,
                true,
                true));

            var captureEventName = string.IsNullOrWhiteSpace(command.options?.captureEventName)
                ? "sdk_regression_core"
                : command.options.captureEventName;
            JestSDK.Instance.CaptureEvent(captureEventName, new Dictionary<string, object>
            {
                { "source", "sdk-regression" },
                { "engine", Engine },
                { "runId", command.runId }
            });
            assertions.Add(RegressionAssertion.Condition(
                "capture event was sent",
                true,
                captureEventName,
                "event name"));

            return Task.FromResult(assertions);
        }

        private static async Task<List<RegressionAssertion>> RunPlayerDataScenario(RunScenarioCommand command)
        {
            var assertions = new List<RegressionAssertion>();
            var key = string.IsNullOrWhiteSpace(command.options?.playerDataKey)
                ? "sdk-regression:" + command.runId
                : command.options.playerDataKey;
            var value = "unity-value:" + command.runId;

            JestSDK.Instance.Player.Set(key, value);
            assertions.Add(RegressionAssertion.Condition("player data set did not throw", true, key, "set key"));

            var actualValue = JestSDK.Instance.Player.Get(key);
            assertions.Add(RegressionAssertion.Equal("player data round-trips", actualValue, value));

            var tryGetValue = JestSDK.Instance.Player.TryGet(key, out string tryGetActual);
            assertions.Add(RegressionAssertion.Condition("player data TryGet returns true", tryGetValue, true, true));
            assertions.Add(RegressionAssertion.Equal("player data TryGet returns value", tryGetActual, value));

            var intKey = key + ":int";
            JestSDK.Instance.Player.Set(intKey, 42);
            assertions.Add(RegressionAssertion.Equal("player data generic int round-trips", JestSDK.Instance.Player.Get<int>(intKey), 42));
            var tryGetInt = JestSDK.Instance.Player.TryGet<int>(intKey, out int tryGetIntActual);
            assertions.Add(RegressionAssertion.Condition("player data generic TryGet returns true", tryGetInt, true, true));
            assertions.Add(RegressionAssertion.Equal("player data generic TryGet returns value", tryGetIntActual, 42));

            var playerDataJson = JestSDK.Instance.Player.GetPlayerData();
            assertions.Add(RegressionAssertion.Condition(
                "player data snapshot is readable",
                !string.IsNullOrWhiteSpace(playerDataJson),
                playerDataJson,
                "non-empty json"));

            var signed = await JestSDK.Instance.Player.GetSigned();
            assertions.Add(RegressionAssertion.Condition("player signed response is not null", signed != null, signed == null ? null : "response", "response"));
            if (signed != null)
            {
                assertions.Add(RegressionAssertion.Condition(
                    "player signed payload is present",
                    !string.IsNullOrWhiteSpace(signed.playerSigned),
                    signed.playerSigned,
                    "non-empty signed payload"));
                assertions.Add(RegressionAssertion.Equal(
                    "player signed id matches player id",
                    signed.player?.playerId,
                    JestSDK.Instance.Player.id));
            }

            JestSDK.Instance.Player.Delete(key);
            var deletedValue = JestSDK.Instance.Player.Get(key);
            assertions.Add(RegressionAssertion.Condition(
                "player data delete clears value",
                string.IsNullOrEmpty(deletedValue),
                deletedValue,
                "empty value"));
            JestSDK.Instance.Player.Delete(intKey);

            await JestSDK.Instance.Player.Flush();
            assertions.Add(RegressionAssertion.Condition("player data flush completed", true, key, "flush success"));

            return assertions;
        }

        private static async Task<List<RegressionAssertion>> RunCommerceReadScenario()
        {
            var assertions = new List<RegressionAssertion>();
            var products = await JestSDK.Instance.Payment.GetProducts();

            assertions.Add(RegressionAssertion.Condition(
                "products response is not null",
                products != null,
                products == null ? null : products.Count.ToString(CultureInfo.InvariantCulture),
                "product list"));

            if (products != null && products.Count > 0)
            {
                var product = products[0];
                assertions.Add(RegressionAssertion.Condition(
                    "first product has sku",
                    !string.IsNullOrWhiteSpace(product.sku),
                    product.sku,
                    "non-empty sku"));
                assertions.Add(RegressionAssertion.Condition(
                    "first product has currency",
                    !string.IsNullOrWhiteSpace(product.currency),
                    product.currency,
                    "non-empty currency"));
                assertions.Add(RegressionAssertion.Condition(
                    "first product has non-negative price",
                    product.price >= 0,
                    product.price,
                    "non-negative price"));
            }

            var incompletePurchases = await JestSDK.Instance.Payment.GetIncompletePurchases();
            assertions.Add(RegressionAssertion.Condition(
                "incomplete purchases response is not null",
                incompletePurchases != null,
                incompletePurchases == null ? null : "response",
                "response"));
            if (incompletePurchases != null)
            {
                assertions.Add(RegressionAssertion.Condition(
                    "incomplete purchases list is present",
                    incompletePurchases.purchases != null,
                    incompletePurchases.purchases == null ? null : incompletePurchases.purchases.Count.ToString(CultureInfo.InvariantCulture),
                    "purchase list"));
                assertions.Add(RegressionAssertion.Condition(
                    "incomplete purchases signed payload is readable",
                    true,
                    incompletePurchases.purchasesSigned,
                    "signed payload or empty"));
            }

            var subscriptions = await JestSDK.Instance.Payment.GetSubscriptions();
            assertions.Add(RegressionAssertion.Condition(
                "subscriptions response is not null",
                subscriptions != null,
                subscriptions == null ? null : "response",
                "response"));
            if (subscriptions != null)
            {
                assertions.Add(RegressionAssertion.Condition(
                    "subscriptions list is present",
                    subscriptions.Subscriptions != null,
                    subscriptions.Subscriptions == null ? null : subscriptions.Subscriptions.Count.ToString(CultureInfo.InvariantCulture),
                    "subscription list"));
                assertions.Add(RegressionAssertion.Condition(
                    "subscriptions signed payload is readable",
                    true,
                    subscriptions.Signed,
                    "signed payload or empty"));
            }

            return assertions;
        }

        private static async Task<List<RegressionAssertion>> RunCommerceErrorsScenario()
        {
            var assertions = new List<RegressionAssertion>();
            const string MissingSku = "sdk-regression-missing-sku";
            const string MissingPurchaseToken = "sdk-regression-missing-token";
            const string MissingSubscriptionSku = "sdk-regression-missing-subscription";

            var purchase = await JestSDK.Instance.Payment.BeginPurchase(MissingSku);
            assertions.Add(RegressionAssertion.Condition("begin purchase error response is not null", purchase != null, purchase == null ? null : "response", "response"));
            if (purchase != null)
            {
                assertions.Add(RegressionAssertion.Equal("begin purchase reports invalid product", purchase.result, "error"));
                assertions.Add(RegressionAssertion.Equal("begin purchase error code is invalid_product", purchase.error, "invalid_product"));
            }

#pragma warning disable CS0618
            var legacyPurchase = await JestSDK.Instance.Payment.Purchase(MissingSku);
#pragma warning restore CS0618
            assertions.Add(RegressionAssertion.Condition("legacy purchase error response is not null", legacyPurchase != null, legacyPurchase == null ? null : "response", "response"));
            if (legacyPurchase != null)
            {
                assertions.Add(RegressionAssertion.Equal("legacy purchase delegates to begin purchase", legacyPurchase.result, "error"));
                assertions.Add(RegressionAssertion.Equal("legacy purchase error code is invalid_product", legacyPurchase.error, "invalid_product"));
            }

            var completePurchase = await JestSDK.Instance.Payment.CompletePurchase(MissingPurchaseToken);
            assertions.Add(RegressionAssertion.Condition("complete purchase error response is not null", completePurchase != null, completePurchase == null ? null : "response", "response"));
            if (completePurchase != null)
            {
                assertions.Add(RegressionAssertion.Equal("complete purchase reports error", completePurchase.result, "error"));
                assertions.Add(RegressionAssertion.Equal("complete purchase error code is invalid_token", completePurchase.error, "invalid_token"));
            }

            var cancelSubscription = await JestSDK.Instance.Payment.CancelSubscription(MissingSubscriptionSku);
            assertions.Add(RegressionAssertion.Condition("cancel subscription error response is not null", cancelSubscription != null, cancelSubscription == null ? null : "response", "response"));
            if (cancelSubscription != null)
            {
                assertions.Add(RegressionAssertion.Equal("cancel subscription reports error", cancelSubscription.Result, "error"));
                assertions.Add(RegressionAssertion.Equal("cancel subscription error code is not_found", cancelSubscription.Error, "not_found"));
            }

            return assertions;
        }

        private static async Task<List<RegressionAssertion>> RunNotificationsScenario(RunScenarioCommand command)
        {
            var assertions = new List<RegressionAssertion>();
            var identifier = "sdk-regression-" + command.runId;

            await JestSDK.Instance.RichNotifications.ScheduleNotification(new RichNotifications.Options
            {
                body = "SDK regression notification",
                title = "Regression",
                ctaText = "Open",
                identifier = identifier,
                scheduledInDays = 1,
                notificationPriority = RichNotifications.Severity.Medium,
                entryPayloadData = new Dictionary<string, object>
                {
                    { "source", "sdk-regression" },
                    { "runId", command.runId }
                }
            });
            assertions.Add(RegressionAssertion.Condition("notification schedule completed", true, identifier, "scheduled"));

            await JestSDK.Instance.RichNotifications.UnscheduleNotification(identifier);
            assertions.Add(RegressionAssertion.Condition("notification unschedule completed", true, identifier, "unscheduled"));

            assertions.Add(ExpectThrows<ArgumentNullException>(
                "notification schedule rejects null options",
                () => JestSDK.Instance.RichNotifications.ScheduleNotification(null)));
            assertions.Add(ExpectThrows<ArgumentException>(
                "notification schedule requires body",
                () => JestSDK.Instance.RichNotifications.ScheduleNotification(new RichNotifications.Options
                {
                    ctaText = "Open",
                    identifier = identifier + ":missing-body",
                    scheduledInDays = 1
                })));
            assertions.Add(ExpectThrows<ArgumentException>(
                "notification schedule requires cta text",
                () => JestSDK.Instance.RichNotifications.ScheduleNotification(new RichNotifications.Options
                {
                    body = "Hello",
                    identifier = identifier + ":missing-cta",
                    scheduledInDays = 1
                })));
            assertions.Add(ExpectThrows<ArgumentException>(
                "notification schedule requires date or days",
                () => JestSDK.Instance.RichNotifications.ScheduleNotification(new RichNotifications.Options
                {
                    body = "Hello",
                    ctaText = "Open",
                    identifier = identifier + ":missing-time"
                })));
            assertions.Add(ExpectThrows<ArgumentException>(
                "notification schedule rejects date and days together",
                () => JestSDK.Instance.RichNotifications.ScheduleNotification(new RichNotifications.Options
                {
                    body = "Hello",
                    ctaText = "Open",
                    identifier = identifier + ":both-times",
                    date = DateTime.UtcNow.AddDays(1),
                    scheduledInDays = 1
                })));
            assertions.Add(ExpectThrows<ArgumentException>(
                "notification unschedule requires identifier",
                () => JestSDK.Instance.RichNotifications.UnscheduleNotification("")));

            return assertions;
        }

        private static Task<List<RegressionAssertion>> RunSocialScenario()
        {
            var assertions = new List<RegressionAssertion>();
            var botAvatar = JestSDK.Instance.Social.GetBotAvatar("sdk-regression", 128);
            var sameBotAvatar = JestSDK.Instance.Social.GetBotAvatar("sdk-regression", 128);

            assertions.Add(RegressionAssertion.Condition(
                "bot avatar url is present",
                !string.IsNullOrWhiteSpace(botAvatar),
                botAvatar,
                "non-empty url"));
            assertions.Add(RegressionAssertion.Equal("bot avatar is deterministic", sameBotAvatar, botAvatar));

            var profile = JestSDK.Instance.Social.GetProfile(128);
            assertions.Add(RegressionAssertion.Condition(
                "profile helper is callable",
                true,
                profile == null ? null : profile.Username,
                "profile or null"));

#pragma warning disable CS0618
            var playerAvatar = JestSDK.Instance.Social.GetPlayerAvatar(128);
#pragma warning restore CS0618
            assertions.Add(RegressionAssertion.Condition(
                "legacy player avatar helper is callable",
                true,
                playerAvatar,
                "url or null"));

            return Task.FromResult(assertions);
        }

        private static async Task<List<RegressionAssertion>> RunReferralsReadScenario()
        {
            var assertions = new List<RegressionAssertion>();
            var referrals = await JestSDK.Instance.Referrals.ListReferrals();

            assertions.Add(RegressionAssertion.Condition(
                "referrals response is not null",
                referrals != null,
                referrals == null ? null : "response",
                "response"));
            if (referrals != null)
            {
                assertions.Add(RegressionAssertion.Condition(
                    "referrals list is present",
                    referrals.referrals != null,
                    referrals.referrals == null ? null : referrals.referrals.Count.ToString(CultureInfo.InvariantCulture),
                    "referral list"));
                assertions.Add(RegressionAssertion.Condition(
                    "referrals signed payload is readable",
                    true,
                    referrals.referralsSigned,
                    "signed payload or empty"));
            }

            return assertions;
        }

        private static async Task<List<RegressionAssertion>> RunReferralsShareScenario(RunScenarioCommand command)
        {
            var assertions = new List<RegressionAssertion>();
            var reference = "sdk-regression-" + command.runId;

            await JestSDK.Instance.Referrals.OpenReferralDialog(new Referrals.OpenDialogOptions
            {
                reference = reference,
                entryPayload = new Dictionary<string, object>
                {
                    { "source", "sdk-regression" },
                    { "runId", command.runId }
                },
                shareTitle = "SDK regression",
                shareText = "Testing referral bridge",
                ShareImage = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
                NotificationTemplates = new List<Referrals.ReferralNotificationTemplate>
                {
                    new Referrals.ReferralNotificationTemplate
                    {
                        MinConversionCount = 1,
                        Variants = new List<Referrals.ReferralNotificationVariant>
                        {
                            new Referrals.ReferralNotificationVariant
                            {
                                Title = "Referral",
                                Body = "A referred player joined.",
                                CtaText = "Open"
                            }
                        }
                    }
                }
            });
            assertions.Add(RegressionAssertion.Condition("referral share dialog completed", true, reference, "completed"));

            return assertions;
        }

        private static async Task<List<RegressionAssertion>> RunInternalScenario(RunScenarioCommand command)
        {
            var assertions = new List<RegressionAssertion>();

            var featureFlag = await JestSDK.Instance.GetFeatureFlag("sdk-regression-missing-flag");
            assertions.Add(RegressionAssertion.Condition(
                "feature flag lookup completes",
                featureFlag != null,
                featureFlag,
                "string or empty"));

            var nameValidation = await JestSDK.Instance.Internal.ValidateName("sdkregression");
            assertions.Add(RegressionAssertion.Condition(
                "name validation response is not null",
                nameValidation != null,
                nameValidation == null ? null : nameValidation.status,
                "validation response"));

            JestSDK.Instance.Internal.CaptureOnboardingEvent(
                Internal.OnboardingEvents.GameLoadComplete,
                new Dictionary<string, object>
                {
                    { "source", "sdk-regression" },
                    { "runId", command.runId }
                });
            assertions.Add(RegressionAssertion.Condition(
                "onboarding event capture completed",
                true,
                Internal.OnboardingEvents.GameLoadComplete,
                "event captured"));

            assertions.Add(ExpectThrows<ArgumentNullException>(
                "reserve login message rejects null options",
                () => JestSDK.Instance.Internal.ReserveLoginMessageAsync(null)));
            assertions.Add(ExpectThrows<ArgumentException>(
                "reserve login message requires message",
                () => JestSDK.Instance.Internal.ReserveLoginMessageAsync(new Internal.ReserveLoginMessageOptions())));
            assertions.Add(ExpectThrows<ArgumentNullException>(
                "send reserved login message rejects null reservation",
                () => JestSDK.Instance.Internal.SendReservedLoginMessage(null)));
            assertions.Add(ExpectThrows<ArgumentException>(
                "validate name rejects empty name",
                () => JestSDK.Instance.Internal.ValidateName("")));
            assertions.Add(ExpectThrows<ArgumentException>(
                "capture onboarding event rejects empty name",
                () => JestSDK.Instance.Internal.CaptureOnboardingEvent("")));

            return assertions;
        }

        private static Task<List<RegressionAssertion>> RunLegalScenario()
        {
            var assertions = new List<RegressionAssertion>();

            JestSDK.Instance.OpenPrivacyPolicy();
            assertions.Add(RegressionAssertion.Condition("privacy policy bridge call completed", true, "privacy", "opened"));

            JestSDK.Instance.OpenTermsOfService();
            assertions.Add(RegressionAssertion.Condition("terms of service bridge call completed", true, "terms", "opened"));

            JestSDK.Instance.OpenCopyright();
            assertions.Add(RegressionAssertion.Condition("copyright bridge call completed", true, "copyright", "opened"));

            return Task.FromResult(assertions);
        }

        private static Task<List<RegressionAssertion>> RunGuardrailsScenario()
        {
            var assertions = new List<RegressionAssertion>();

            assertions.Add(ExpectThrows<ArgumentException>("player get rejects null key", () => JestSDK.Instance.Player.Get(null)));
            assertions.Add(ExpectThrows<ArgumentException>("player get rejects empty key", () => JestSDK.Instance.Player.Get("")));
            assertions.Add(ExpectThrows<ArgumentException>("player generic get rejects null key", () => JestSDK.Instance.Player.Get<int>(null)));
            assertions.Add(ExpectThrows<ArgumentException>("player TryGet rejects null key", () => JestSDK.Instance.Player.TryGet(null, out string _)));
            assertions.Add(ExpectThrows<ArgumentException>("player TryGet rejects empty key", () => JestSDK.Instance.Player.TryGet("", out string _)));
            assertions.Add(ExpectThrows<ArgumentException>("player set rejects empty key", () => JestSDK.Instance.Player.Set("", "value")));
            assertions.Add(ExpectThrows<ArgumentException>("player generic set rejects null key", () => JestSDK.Instance.Player.Set<int>(null, 42)));
            assertions.Add(ExpectThrows<ArgumentException>("player delete rejects empty key", () => JestSDK.Instance.Player.Delete("")));
            assertions.Add(ExpectThrows<ArgumentException>("player delete rejects null key", () => JestSDK.Instance.Player.Delete(null)));
            assertions.Add(ExpectThrows<ArgumentException>("capture event rejects null event name", () => JestSDK.Instance.CaptureEvent(null)));
            assertions.Add(ExpectThrows<ArgumentException>("capture event rejects empty event name", () => JestSDK.Instance.CaptureEvent("")));
            assertions.Add(ExpectThrows<InvalidOperationException>("login rejects already registered player", () => JestSDK.Instance.Login()));
            assertions.Add(ExpectThrows<InvalidOperationException>("registration overlay rejects already registered player", () => JestSDK.Instance.ShowRegistrationOverlay()));
            assertions.Add(ExpectThrows<ArgumentNullException>("navigation redirect rejects null options", () => JestSDK.Instance.Navigation.RedirectToGame(null)));
            assertions.Add(ExpectThrows<ArgumentException>("navigation redirect rejects empty slug", () => JestSDK.Instance.Navigation.RedirectToGame(new Navigation.RedirectToGameOptions())));
            assertions.Add(ExpectThrows<ArgumentException>("navigation redirect rejects whitespace slug", () => JestSDK.Instance.Navigation.RedirectToGame(new Navigation.RedirectToGameOptions { gameSlug = " " })));
            assertions.Add(ExpectThrows<ArgumentException>("payment begin purchase rejects null sku", () => JestSDK.Instance.Payment.BeginPurchase(null)));
            assertions.Add(ExpectThrows<ArgumentException>("payment begin purchase rejects empty sku", () => JestSDK.Instance.Payment.BeginPurchase("")));
            assertions.Add(ExpectThrows<ArgumentException>("payment complete purchase rejects null token", () => JestSDK.Instance.Payment.CompletePurchase(null)));
#pragma warning disable CS0618
            assertions.Add(ExpectThrows<ArgumentException>("payment legacy purchase rejects empty sku", () => JestSDK.Instance.Payment.Purchase("")));
#pragma warning restore CS0618
            assertions.Add(ExpectThrows<ArgumentException>("payment complete purchase rejects empty token", () => JestSDK.Instance.Payment.CompletePurchase("")));
            assertions.Add(ExpectThrows<ArgumentException>("subscription begin rejects null sku", () => JestSDK.Instance.Payment.BeginSubscription(null)));
            assertions.Add(ExpectThrows<ArgumentException>("subscription begin rejects empty sku", () => JestSDK.Instance.Payment.BeginSubscription("")));
            assertions.Add(ExpectThrows<ArgumentException>("subscription cancel rejects null sku", () => JestSDK.Instance.Payment.CancelSubscription(null)));
            assertions.Add(ExpectThrows<ArgumentException>("subscription cancel rejects empty sku", () => JestSDK.Instance.Payment.CancelSubscription("")));
            assertions.Add(ExpectThrows<ArgumentException>("notification unschedule rejects null identifier", () => JestSDK.Instance.RichNotifications.UnscheduleNotification(null)));
            assertions.Add(ExpectThrows<ArgumentNullException>("referral dialog rejects null options", () => JestSDK.Instance.Referrals.OpenReferralDialog(null)));
            assertions.Add(ExpectThrows<ArgumentException>("referral dialog rejects empty reference", () => JestSDK.Instance.Referrals.OpenReferralDialog(new Referrals.OpenDialogOptions())));
            assertions.Add(ExpectThrows<ArgumentException>("referral dialog rejects whitespace reference", () => JestSDK.Instance.Referrals.OpenReferralDialog(new Referrals.OpenDialogOptions { reference = " " })));

            return Task.FromResult(assertions);
        }

        private static RegressionAssertion ExpectThrows<TException>(string name, Action action)
            where TException : Exception
        {
            try
            {
                action();
                return RegressionAssertion.Fail(name, "no exception", typeof(TException).Name, "Expected exception was not thrown.");
            }
            catch (TException exception)
            {
                return RegressionAssertion.Condition(name, true, exception.GetType().Name, typeof(TException).Name);
            }
            catch (Exception exception)
            {
                return RegressionAssertion.Fail(name, exception.GetType().Name, typeof(TException).Name, exception.Message);
            }
        }

        private void PostRunnerReady()
        {
            PostMessage(new RunnerReadyMessage
            {
                source = SampleSource,
                protocolVersion = ProtocolVersion,
                type = "runnerReady",
                engine = Engine,
                scenarios = Scenarios,
                sdkVersion = SdkVersion.Value
            });
        }

        private static void PostLog(RunScenarioCommand command, string level, string message)
        {
            PostMessage(new ScenarioLogMessage
            {
                source = SampleSource,
                protocolVersion = ProtocolVersion,
                type = "scenarioLog",
                engine = Engine,
                scenario = command?.scenario,
                runId = command?.runId,
                level = level,
                message = message
            });
        }

        private static void PostScenarioResult(
            RunScenarioCommand command,
            List<RegressionAssertion> assertions,
            Exception exception)
        {
            PostMessage(new ScenarioResultMessage
            {
                source = SampleSource,
                protocolVersion = ProtocolVersion,
                type = "scenarioResult",
                engine = Engine,
                scenario = command.scenario,
                runId = command.runId,
                status = exception == null && assertions.All(assertion => assertion.pass) ? "pass" : "fail",
                assertions = assertions,
                error = exception == null ? null : new RegressionError
                {
                    message = exception.Message,
                    stack = exception.StackTrace
                }
            });
        }

        private static void PostMessage(object message)
        {
            JS_SdkRegressionPostMessage(com.jest.sdk.Convert.ToString(message));
        }

        [Serializable]
        private sealed class RunScenarioCommand
        {
            public string source;
            public int protocolVersion;
            public string type;
            public string engine;
            public string scenario;
            public string runId;
            public RunScenarioOptions options;
        }

        [Serializable]
        private sealed class RunScenarioOptions
        {
            public string playerDataKey;
            public Dictionary<string, object> entryPayloadExpected;
            public string captureEventName;
        }

        [Serializable]
        private sealed class RunnerReadyMessage
        {
            public string source;
            public int protocolVersion;
            public string type;
            public string engine;
            public string[] scenarios;
            public string sdkVersion;
        }

        [Serializable]
        private sealed class ScenarioLogMessage
        {
            public string source;
            public int protocolVersion;
            public string type;
            public string engine;
            public string scenario;
            public string runId;
            public string level;
            public string message;
        }

        [Serializable]
        private sealed class ScenarioResultMessage
        {
            public string source;
            public int protocolVersion;
            public string type;
            public string engine;
            public string scenario;
            public string runId;
            public string status;
            public List<RegressionAssertion> assertions;
            public RegressionError error;
        }

        [Serializable]
        private sealed class RegressionError
        {
            public string message;
            public string stack;
        }

        [Serializable]
        private sealed class RegressionAssertion
        {
            public string name;
            public bool pass;
            public string actual;
            public string expected;
            public string details;

            public static RegressionAssertion Equal(string name, object actual, object expected)
            {
                var actualValue = FormatValue(actual);
                var expectedValue = FormatValue(expected);
                return new RegressionAssertion
                {
                    name = name,
                    pass = actualValue == expectedValue,
                    actual = actualValue,
                    expected = expectedValue
                };
            }

            public static RegressionAssertion Condition(string name, bool pass, object actual, object expected)
            {
                return new RegressionAssertion
                {
                    name = name,
                    pass = pass,
                    actual = FormatValue(actual),
                    expected = FormatValue(expected)
                };
            }

            public static RegressionAssertion Fail(string name, object actual, object expected, string details)
            {
                return new RegressionAssertion
                {
                    name = name,
                    pass = false,
                    actual = FormatValue(actual),
                    expected = FormatValue(expected),
                    details = details
                };
            }

            private static string FormatValue(object value)
            {
                if (value == null)
                {
                    return null;
                }

                if (value is string stringValue)
                {
                    return stringValue;
                }

                if (value is IFormattable formattable)
                {
                    return formattable.ToString(null, CultureInfo.InvariantCulture);
                }

                return com.jest.sdk.Convert.ToString(value);
            }
        }
    }
}
