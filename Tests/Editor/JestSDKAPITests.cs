using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Collections.Generic;
using com.jest.sdk;

namespace com.jest.sdk.Tests
{
    public class JestSDKAPITests
    {
        private TestBridgeMock _mock;

        private string testId = "test-player-id";

        [SetUp]
        public void Setup()
        {
            _mock = new TestBridgeMock(testId, true);
            JsBridge.SetMock(_mock);
            JestSDK.Instance.Init();
        }

        [Test]
        public void Player_GetId_ReturnsCorrectId()
        {
            Assert.That(JestSDK.Instance.Player.id, Is.EqualTo(testId));
        }

        [Test]
        public void Player_IsRegistered_ReturnsCorrectValue()
        {
            Assert.That(JestSDK.Instance.Player.isRegistered, Is.True);
        }

        [Test]
        public void Player_SetAndGetValue_WorksWithPrimitives()
        {
            JestSDK.Instance.Player.Set("bool", true);
            JestSDK.Instance.Player.Set("int", 42);
            JestSDK.Instance.Player.Set("float", 3.14f);
            JestSDK.Instance.Player.Set("string", "test");

            Assert.That(JestSDK.Instance.Player.Get<bool>("bool"), Is.True);
            Assert.That(JestSDK.Instance.Player.Get<int>("int"), Is.EqualTo(42));
            Assert.That(JestSDK.Instance.Player.Get<float>("float"), Is.EqualTo(3.14f));
            Assert.That(JestSDK.Instance.Player.Get<string>("string"), Is.EqualTo("test"));
        }

        [Test]
        public void Player_TryGet_ReturnsCorrectValue()
        {
            const string key = "key";
            const string value = "value";
            const string anotherKey = "anotherKey";

            JestSDK.Instance.Player.Set(key, value);

            Assert.That(JestSDK.Instance.Player.TryGet(key, out string val) && val == value);
            Assert.That(!JestSDK.Instance.Player.TryGet(anotherKey, out string anotherVal));
        }

        [Test]
        public void Player_SetAndGetValue_WorksWithVectors()
        {
            var vector2 = new Vector2(1, 2);
            var vector3 = new Vector3(1, 2, 3);

            JestSDK.Instance.Player.Set("vector2", vector2);
            JestSDK.Instance.Player.Set("vector3", vector3);

            Assert.That(JestSDK.Instance.Player.Get<Vector2>("vector2"), Is.EqualTo(vector2));
            Assert.That(JestSDK.Instance.Player.Get<Vector3>("vector3"), Is.EqualTo(vector3));
        }

        // Bot avatar tests — vectors verified against the JS @textclub/common/prng
        // implementation so all SDKs produce the same avatar for the same username.

        [Test]
        public void RichNotifications_ScheduleNotification_AllowsPlatformValidatedBodyLength()
        {
            var options = new RichNotifications.Options
            {
                body = new string('x', 2001),
                ctaText = "Play",
                identifier = "test",
                scheduledInDays = 1,
            };

            var task = JestSDK.Instance.RichNotifications.ScheduleNotification(options);

            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsFaulted, Is.False);
        }

        [Test]
        public void RichNotifications_ScheduleNotification_AllowsPlatformValidatedTitleLength()
        {
            var options = new RichNotifications.Options
            {
                body = "Hello",
                title = new string('x', 201),
                ctaText = "Play",
                identifier = "test",
                scheduledInDays = 1,
            };

            var task = JestSDK.Instance.RichNotifications.ScheduleNotification(options);

            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsFaulted, Is.False);
        }

        [Test]
        public void RichNotifications_ScheduleNotification_AllowsPlatformValidatedCtaLength()
        {
            var options = new RichNotifications.Options
            {
                body = "Hello",
                ctaText = new string('x', 51),
                identifier = "test-cta-at-limit",
                scheduledInDays = 1,
            };

            var task = JestSDK.Instance.RichNotifications.ScheduleNotification(options);

            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsFaulted, Is.False);
        }

        [Test]
        public void GetBotAvatar_ReturnsKnownVectors()
        {
            Assert.That(JestSDK.Instance.Social.GetBotAvatar(""), Does.EndWith("https%3A%2F%2Fcdn.jest.com%2Favatars%2Fbot%2F115.webp"));
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("a"), Does.EndWith("https%3A%2F%2Fcdn.jest.com%2Favatars%2Fbot%2F748.webp"));
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("test"), Does.EndWith("https%3A%2F%2Fcdn.jest.com%2Favatars%2Fbot%2F736.webp"));
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("alice"), Does.EndWith("https%3A%2F%2Fcdn.jest.com%2Favatars%2Fbot%2F982.webp"));
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("Bob"), Does.EndWith("https%3A%2F%2Fcdn.jest.com%2Favatars%2Fbot%2F160.webp"));
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("bot"), Does.EndWith("https%3A%2F%2Fcdn.jest.com%2Favatars%2Fbot%2F991.webp"));
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("user_42"), Does.EndWith("https%3A%2F%2Fcdn.jest.com%2Favatars%2Fbot%2F592.webp"));
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("hello world"), Does.EndWith("https%3A%2F%2Fcdn.jest.com%2Favatars%2Fbot%2F256.webp"));
        }

        [Test]
        public void GetBotAvatar_WithSize_WrapsInCloudflareProxy()
        {
            var url = JestSDK.Instance.Social.GetBotAvatar("test", 128);
            Assert.That(url, Is.EqualTo("https://cdn.jestpub.com/cdn-cgi/image/format=auto%2Cfit=cover%2Cwidth=128%2C/https%3A%2F%2Fcdn.jest.com%2Favatars%2Fbot%2F736.webp"));
        }

        [Test]
        public void GetBotAvatar_BucketsDownIntermediateSizes()
        {
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("bot", 999), Is.EqualTo(JestSDK.Instance.Social.GetBotAvatar("bot", 512)));
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("bot", 200), Is.EqualTo(JestSDK.Instance.Social.GetBotAvatar("bot", 128)));
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("bot", 1), Is.EqualTo(JestSDK.Instance.Social.GetBotAvatar("bot", 64)));
        }

        [Test]
        public void GetBotAvatar_SupplementaryPlaneUsername()
        {
            // Emoji 🎮 (U+1F3AE) — JS encodes as a UTF-16 surrogate pair, and C# strings are
            // UTF-16 natively, so iterating chars matches String.charCodeAt directly.
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("🎮"), Does.EndWith("https%3A%2F%2Fcdn.jest.com%2Favatars%2Fbot%2F740.webp"));
        }

        [Test]
        public void GetBotAvatar_IsDeterministic()
        {
            Assert.That(JestSDK.Instance.Social.GetBotAvatar("alice"), Is.EqualTo(JestSDK.Instance.Social.GetBotAvatar("alice")));
        }

        [Test]
        public void GetPlayerAvatar_ReturnsNull_WhenNoAvatarUrl()
        {
            _mock.avatarUrl = "";
            Assert.That(JestSDK.Instance.Social.GetPlayerAvatar(), Is.Null);
        }

        [Test]
        public void GetPlayerAvatar_WrapsInCloudflareProxy_WhenAvatarSet()
        {
            _mock.avatarUrl = "https://cdn.jest.com/avatars/abc.webp";
            Assert.That(
                JestSDK.Instance.Social.GetPlayerAvatar(256),
                Is.EqualTo("https://cdn.jestpub.com/cdn-cgi/image/format=auto%2Cfit=cover%2Cwidth=256%2C/https%3A%2F%2Fcdn.jest.com%2Favatars%2Fabc.webp")
            );
        }

        [Test]
        public void GetPlayerAvatar_WrapsInCloudflareProxyAtDefaultSize()
        {
            // Unlike GetBotAvatar, GetPlayerAvatar always wraps — the underlying file is WebP
            // and we want a Unity-decodable format even at the largest size.
            _mock.avatarUrl = "https://cdn.jest.com/avatars/abc.webp";
            Assert.That(
                JestSDK.Instance.Social.GetPlayerAvatar(),
                Is.EqualTo("https://cdn.jestpub.com/cdn-cgi/image/format=auto%2Cfit=cover%2Cwidth=1000%2C/https%3A%2F%2Fcdn.jest.com%2Favatars%2Fabc.webp")
            );
        }

        [Test]
        public void GetPlayerAvatar_BucketsDownIntermediateSizes()
        {
            _mock.avatarUrl = "https://cdn.jest.com/avatars/abc.webp";
            Assert.That(JestSDK.Instance.Social.GetPlayerAvatar(999), Is.EqualTo(JestSDK.Instance.Social.GetPlayerAvatar(512)));
            Assert.That(JestSDK.Instance.Social.GetPlayerAvatar(200), Is.EqualTo(JestSDK.Instance.Social.GetPlayerAvatar(128)));
            Assert.That(JestSDK.Instance.Social.GetPlayerAvatar(1), Is.EqualTo(JestSDK.Instance.Social.GetPlayerAvatar(64)));
        }

        [Test]
        public void GetPlayerAvatar_PassesThroughLocalhostUrls()
        {
            _mock.avatarUrl = "http://localhost:3000/avatars/abc.webp";
            Assert.That(JestSDK.Instance.Social.GetPlayerAvatar(256), Is.EqualTo("http://localhost:3000/avatars/abc.webp"));
        }

        [Test]
        public void GetEntryPayload_ReturnsCorrectValue()
        {
            var expectedPayload = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 42 }
            };
            _mock.SetEntryPayload(expectedPayload);

            var actualPayload = JestSDK.Instance.GetEntryPayload();

            Assert.That(actualPayload, Is.EqualTo(expectedPayload));
        }

        [Test]
        public void RichNotifications_ScheduleNotification_StoresNotification()
        {
            var options = new RichNotifications.Options
            {
                body = "Test Body",
                ctaText = "Play Now!",
                identifier = "test-key",
                date = DateTime.Now,
                notificationPriority = RichNotifications.Severity.Low
            };
            options.entryPayloadData["stringValue"] = "test";
            options.entryPayloadData["numberValue"] = 42;

            var task = JestSDK.Instance.RichNotifications.ScheduleNotification(options);
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsFaulted, Is.False);

            var notifications = JestSDK.Instance.RichNotifications.GetNotifications();
            Assert.That(notifications, Has.Count.EqualTo(1));
            var result = notifications[0];
            Assert.AreEqual(options.body, result.body);
            Assert.AreEqual(options.ctaText, result.ctaText);
            Assert.AreEqual(options.date, result.date);
            Assert.AreEqual(options.identifier, result.identifier);
            Assert.AreEqual(options.notificationPriority, result.notificationPriority);
        }

        [Test]
        public void RichNotifications_UnscheduleNotification_CompletesAndRemovesNotification()
        {
            var options = new RichNotifications.Options
            {
                body = "Test Body",
                ctaText = "Play Now!",
                identifier = "remove-me",
                scheduledInDays = 1,
            };
            JestSDK.Instance.RichNotifications.ScheduleNotification(options);

            var task = JestSDK.Instance.RichNotifications.UnscheduleNotification("remove-me");

            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsFaulted, Is.False);
            Assert.That(JestSDK.Instance.RichNotifications.GetNotifications(), Is.Empty);
        }


        [Test]
        public void GetProducts_ReturnsExpectedProducts()
        {
            var task = JestSDK.Instance.Payment.GetProducts();
            var products = task.GetResult();

            Assert.That(products, Is.Not.Null);
            Assert.That(products, Has.Count.EqualTo(1));
        }

        [Test]
        public void Purchase_Success_ReturnsExpectedResult()
        {
            _mock.purchaseResult = PurchaseReult.success;
            var task = JestSDK.Instance.Payment.GetProducts();
            var products = task.GetResult();
            var purchaseTask = JestSDK.Instance.Payment.Purchase(products[0].sku);
            var result = purchaseTask.GetResult();
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("success", result.result);
            Assert.That(result.purchase, Is.Not.Null);
            Assert.AreEqual(products[0].sku, result.purchase.productSku);
            Assert.AreEqual((decimal)products[0].price, result.purchase.credits);
#pragma warning disable CS0618
            Assert.That(result.purchase.estimatedRevenue, Is.EqualTo(0m));
#pragma warning restore CS0618
        }

        [Test]
        public void Purchase_Error_ReturnsErrorResult()
        {
            _mock.purchaseResult = PurchaseReult.error;
            var task = JestSDK.Instance.Payment.Purchase("invalid_sku");
            var result = task.GetResult();
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("error", result.result);
            Assert.AreEqual("internal_error", result.error);
        }

        [Test]
        public void CompletePurchase_Success_ReturnsSuccessResult()
        {
            _mock.purchaseCompleteResult = PurchaseReult.success;
            var task = JestSDK.Instance.Payment.CompletePurchase("mock_token_bcwux13xvm4");
            var result = task.GetResult();
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("success", result.result);
            Assert.That(result.error, Is.Null);
        }

        [Test]
        public void CompletePurchase_Error_ReturnsErrorResult()
        {
            _mock.purchaseCompleteResult = PurchaseReult.error;
            var task = JestSDK.Instance.Payment.CompletePurchase("mock_token_invalid");
            var result = task.GetResult();
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("error", result.result);
            Assert.AreEqual("internal_error", result.error);
        }

        [Test]
        public void GetIncompletePurchases_ReturnsExpectedResponse()
        {
            var task = JestSDK.Instance.Payment.GetIncompletePurchases();
            var response = task.GetResult();

            Assert.That(response, Is.Not.Null);
            Assert.That(response.hasMore, Is.False);
            Assert.That(response.purchasesSigned, Is.Not.Empty);
            Assert.That(response.purchases, Has.Count.EqualTo(1));
        }

        [System.Serializable]
        private struct TestEventData
        {
            public string stringValue;
            public int numberValue;
        }

        #region Error Scenario Tests

        [Test]
        public void Player_Get_ThrowsOnNullKey()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Get(null));
        }

        [Test]
        public void Player_Get_ThrowsOnEmptyKey()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Get(""));
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Get("   "));
        }

        [Test]
        public void Player_GetGeneric_ThrowsOnEmptyKey()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Get<int>(null));
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Get<int>(""));
        }

        [Test]
        public void Player_Set_ThrowsOnEmptyKey()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Set(null, "value"));
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Set("", "value"));
        }

        [Test]
        public void Player_SetGeneric_ThrowsOnEmptyKey()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Set<int>(null, 42));
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Set<int>("", 42));
        }

        [Test]
        public void Player_TryGet_ThrowsOnEmptyKey()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.TryGet(null, out _));
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.TryGet("", out _));
        }

        [Test]
        public void Payment_Purchase_ThrowsOnNullSku()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Payment.Purchase(null));
        }

        [Test]
        public void Payment_Purchase_ThrowsOnEmptySku()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Payment.Purchase(""));
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Payment.Purchase("   "));
        }

        [Test]
        public void Payment_CompletePurchase_ThrowsOnNullToken()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Payment.CompletePurchase(null));
        }

        [Test]
        public void Payment_CompletePurchase_ThrowsOnEmptyToken()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Payment.CompletePurchase(""));
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Payment.CompletePurchase("   "));
        }

        [Test]
        public void Convert_FromString_ThrowsOnInvalidVector2Format()
        {
            Assert.Throws<ArgumentException>(() => Convert.FromString<Vector2>("invalid"));
            Assert.Throws<ArgumentException>(() => Convert.FromString<Vector2>("(1)"));
            Assert.Throws<ArgumentException>(() => Convert.FromString<Vector2>("(a,b)"));
        }

        [Test]
        public void Convert_FromString_ThrowsOnInvalidVector3Format()
        {
            Assert.Throws<ArgumentException>(() => Convert.FromString<Vector3>("invalid"));
            Assert.Throws<ArgumentException>(() => Convert.FromString<Vector3>("(1,2)"));
            Assert.Throws<ArgumentException>(() => Convert.FromString<Vector3>("(a,b,c)"));
        }

        [Test]
        public void Convert_FromString_ThrowsOnNullOrEmptyVector()
        {
            Assert.Throws<ArgumentException>(() => Convert.FromString<Vector2>(null));
            Assert.Throws<ArgumentException>(() => Convert.FromString<Vector2>(""));
            Assert.Throws<ArgumentException>(() => Convert.FromString<Vector3>(null));
            Assert.Throws<ArgumentException>(() => Convert.FromString<Vector3>(""));
        }

        [Test]
        public void Convert_FromString_HandlesWhitespaceInVectors()
        {
            // Should handle vectors with spaces around components
            var vector2 = Convert.FromString<Vector2>("(1, 2)");
            Assert.That(vector2, Is.EqualTo(new Vector2(1, 2)));

            var vector3 = Convert.FromString<Vector3>("( 1 , 2 , 3 )");
            Assert.That(vector3, Is.EqualTo(new Vector3(1, 2, 3)));
        }

        #endregion

        #region Phase 1 API Update Tests

        [Test]
        public void Player_Delete_RemovesValue()
        {
            const string key = "deleteTestKey";
            JestSDK.Instance.Player.Set(key, "someValue");
            Assert.That(JestSDK.Instance.Player.TryGet(key, out string _), Is.True);

            JestSDK.Instance.Player.Delete(key);
            Assert.That(JestSDK.Instance.Player.TryGet(key, out string _), Is.False);
        }

        [Test]
        public void Player_Delete_ThrowsOnNullKey()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Delete(null));
        }

        [Test]
        public void Player_Delete_ThrowsOnEmptyKey()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Delete(""));
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Player.Delete("   "));
        }

        [Test]
        public void Player_Flush_CompletesSuccessfully()
        {
            var task = JestSDK.Instance.Player.Flush();
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsFaulted, Is.False);
        }

        [Test]
        public void Payment_BeginPurchase_Success_ReturnsExpectedResult()
        {
            _mock.purchaseResult = PurchaseReult.success;
            var task = JestSDK.Instance.Payment.GetProducts();
            var products = task.GetResult();
            var purchaseTask = JestSDK.Instance.Payment.BeginPurchase(products[0].sku);
            var result = purchaseTask.GetResult();
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("success", result.result);
            Assert.That(result.purchase, Is.Not.Null);
        }

        [Test]
        public void Payment_BeginPurchase_ThrowsOnNullSku()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Payment.BeginPurchase(null));
        }

        [Test]
        public void Payment_BeginPurchase_ThrowsOnEmptySku()
        {
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Payment.BeginPurchase(""));
            Assert.Throws<ArgumentException>(() => JestSDK.Instance.Payment.BeginPurchase("   "));
        }

        [Test]
        public void Login_ThrowsWhenAlreadyLoggedIn()
        {
            // Mock is set up with isRegistered = true
            Assert.Throws<InvalidOperationException>(() => JestSDK.Instance.Login());
        }

        [Test]
        public void Login_WorksWithNullPayload()
        {
            // Create a new mock with isRegistered = false
            var unregisteredMock = new TestBridgeMock(testId, false);
            JsBridge.SetMock(unregisteredMock);

            Assert.DoesNotThrow(() => JestSDK.Instance.Login());

            // Restore original mock
            JsBridge.SetMock(_mock);
        }

        [Test]
        public void Login_WorksWithPayload()
        {
            var unregisteredMock = new TestBridgeMock(testId, false);
            JsBridge.SetMock(unregisteredMock);

            var payload = new Dictionary<string, object> { { "key", "value" } };
            Assert.DoesNotThrow(() => JestSDK.Instance.Login(payload));

            // Restore original mock
            JsBridge.SetMock(_mock);
        }

        [Test]
        public void ShowRegistrationOverlay_ReturnsHandleForUnregisteredPlayer()
        {
            var unregisteredMock = new TestBridgeMock(testId, false);
            JsBridge.SetMock(unregisteredMock);

            var handle = JestSDK.Instance.ShowRegistrationOverlay();

            Assert.That(handle, Is.Not.Null);

            JsBridge.SetMock(_mock);
        }

        [Test]
        public void ShowRegistrationOverlay_InvokesOnCloseOption()
        {
            var unregisteredMock = new TestBridgeMock(testId, false);
            JsBridge.SetMock(unregisteredMock);
            var closed = false;

            JestSDK.Instance.ShowRegistrationOverlay(new RegistrationOverlay.Options
            {
                OnClose = () => closed = true
            });

            Assert.That(closed, Is.True);

            JsBridge.SetMock(_mock);
        }

        [Test]
        public void ShowRegistrationOverlay_ThrowsWhenAlreadyLoggedIn()
        {
            Assert.Throws<InvalidOperationException>(() =>
                JestSDK.Instance.ShowRegistrationOverlay());
        }

        [Test]
        public void RegistrationOverlay_HandleActionsDoNotThrow()
        {
            var unregisteredMock = new TestBridgeMock(testId, false);
            JsBridge.SetMock(unregisteredMock);

            var handle = JestSDK.Instance.ShowRegistrationOverlay(new RegistrationOverlay.Options
            {
                Theme = RegistrationOverlay.Theme.Light,
                EntryPayload = new Dictionary<string, object> { { "source", "test" } }
            });

            Assert.DoesNotThrow(() => handle.LoginButtonAction());
            Assert.DoesNotThrow(() => handle.CloseButtonAction());

            JsBridge.SetMock(_mock);
        }


        [Test]
        public void RichNotifications_ScheduleNotification_WithImageReference()
        {
            var options = new RichNotifications.Options
            {
                body = "Test Body",
                ctaText = "Play Now!",
                imageReference = "image://test-image",
                identifier = "test-key",
                date = DateTime.Now,
                notificationPriority = RichNotifications.Severity.High
            };

            JestSDK.Instance.RichNotifications.ScheduleNotification(options);
            var notifications = JestSDK.Instance.RichNotifications.GetNotifications();
            Assert.That(notifications, Has.Count.EqualTo(1));
            // Note: imageReference serialization is handled by ToJson()
        }

        [Test]
        public void RichNotifications_ScheduleNotification_WithEntryPayloadData()
        {
            var options = new RichNotifications.Options
            {
                body = "Test Body",
                ctaText = "Play Now!",
                identifier = "test-key",
                date = DateTime.Now,
                notificationPriority = RichNotifications.Severity.Medium
            };
            options.entryPayloadData["key1"] = "value1";
            options.entryPayloadData["key2"] = 42;

            JestSDK.Instance.RichNotifications.ScheduleNotification(options);
            var notifications = JestSDK.Instance.RichNotifications.GetNotifications();
            Assert.That(notifications, Has.Count.EqualTo(1));
        }

        [Test]
        public void RichNotifications_ScheduleNotification_WithScheduledInDays()
        {
            var options = new RichNotifications.Options
            {
                body = "Test Body",
                ctaText = "Play Now!",
                identifier = "test-key",
                scheduledInDays = 7,
                notificationPriority = RichNotifications.Severity.Low
            };

            // This should not throw - scheduledInDays is used instead of date
            Assert.DoesNotThrow(() => JestSDK.Instance.RichNotifications.ScheduleNotification(options));
        }

        [Test]
        public void RichNotifications_ImageReference_Works()
        {
            var options = new RichNotifications.Options();
            options.imageReference = "test-image-url";
            Assert.That(options.imageReference, Is.EqualTo("test-image-url"));
        }

        #endregion

        #region Referrals Tests

        [Test]
        public void Referrals_OpenReferralDialog_CompletesSuccessfully()
        {
            var options = new Referrals.OpenDialogOptions
            {
                reference = "test-ref-123",
                shareTitle = "Join me!",
                shareText = "Check out this game"
            };

            var task = JestSDK.Instance.Referrals.OpenReferralDialog(options);
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsFaulted, Is.False);
        }

        [Test]
        public void Referrals_OpenReferralDialog_WithEntryPayload()
        {
            var options = new Referrals.OpenDialogOptions
            {
                reference = "test-ref-456",
                entryPayload = new Dictionary<string, object>()
            };

            var task = JestSDK.Instance.Referrals.OpenReferralDialog(options);
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.IsFaulted, Is.False);
        }

        [Test]
        public void Referrals_OpenReferralDialog_ThrowsOnNullOptions()
        {
            Assert.Throws<ArgumentNullException>(() =>
                JestSDK.Instance.Referrals.OpenReferralDialog(null));
        }

        [Test]
        public void Referrals_OpenReferralDialog_ThrowsOnNullReference()
        {
            var options = new Referrals.OpenDialogOptions
            {
                reference = null,
                shareTitle = "Test"
            };

            Assert.Throws<ArgumentException>(() =>
                JestSDK.Instance.Referrals.OpenReferralDialog(options));
        }

        [Test]
        public void Referrals_OpenReferralDialog_ThrowsOnEmptyReference()
        {
            var options = new Referrals.OpenDialogOptions
            {
                reference = "",
                shareTitle = "Test"
            };

            Assert.Throws<ArgumentException>(() =>
                JestSDK.Instance.Referrals.OpenReferralDialog(options));
        }

        [Test]
        public void Referrals_ListReferrals_ReturnsExpectedResponse()
        {
            var task = JestSDK.Instance.Referrals.ListReferrals();
            var response = task.GetResult();

            Assert.That(response, Is.Not.Null);
            Assert.That(response.referrals, Is.Not.Null);
            Assert.That(response.referrals, Has.Count.EqualTo(1));
            Assert.That(response.referrals[0].reference, Is.EqualTo("test-ref-123"));
            Assert.That(response.referrals[0].registrations, Has.Count.EqualTo(2));
            Assert.That(response.referrals[0].registrations[0].playerId, Is.EqualTo("user1"));
            Assert.That(response.referrals[0].registrations[0].joinedAt, Is.Not.Empty);
            Assert.That(response.referralsSigned, Is.Not.Empty);
        }

        #endregion

        #region Navigation Tests

        [Test]
        public void Navigation_RedirectToFlagshipGame_CompletesSuccessfully()
        {
            Assert.DoesNotThrow(() => JestSDK.Instance.Navigation.RedirectToFlagshipGame());
        }

        [Test]
        public void Navigation_RedirectToFlagshipGame_WithOptions()
        {
            var options = new Navigation.RedirectToFlagshipGameOptions
            {
                entryPayload = new Dictionary<string, object>()
            };

            Assert.DoesNotThrow(() => JestSDK.Instance.Navigation.RedirectToFlagshipGame(options));
        }

        [Test]
        public void Navigation_RedirectToGame_CompletesSuccessfully()
        {
            var options = new Navigation.RedirectToGameOptions
            {
                gameSlug = "test-game"
            };

            Assert.DoesNotThrow(() => JestSDK.Instance.Navigation.RedirectToGame(options));
        }

        [Test]
        public void Navigation_RedirectToGame_WithAllOptions()
        {
            var options = new Navigation.RedirectToGameOptions
            {
                gameSlug = "test-game",
                entryPayload = new Dictionary<string, object>(),
                skipGameExitConfirm = true
            };

            Assert.DoesNotThrow(() => JestSDK.Instance.Navigation.RedirectToGame(options));
        }

        [Test]
        public void Navigation_RedirectToGame_ThrowsOnNullOptions()
        {
            Assert.Throws<ArgumentNullException>(() =>
                JestSDK.Instance.Navigation.RedirectToGame(null));
        }

        [Test]
        public void Navigation_RedirectToGame_ThrowsOnNullGameSlug()
        {
            var options = new Navigation.RedirectToGameOptions
            {
                gameSlug = null
            };

            Assert.Throws<ArgumentException>(() =>
                JestSDK.Instance.Navigation.RedirectToGame(options));
        }

        [Test]
        public void Navigation_RedirectToGame_ThrowsOnEmptyGameSlug()
        {
            var options = new Navigation.RedirectToGameOptions
            {
                gameSlug = ""
            };

            Assert.Throws<ArgumentException>(() =>
                JestSDK.Instance.Navigation.RedirectToGame(options));
        }

        #endregion

        #region Legal Pages Tests

        [Test]
        public void OpenPrivacyPolicy_CompletesSuccessfully()
        {
            Assert.DoesNotThrow(() => JestSDK.Instance.OpenPrivacyPolicy());
        }

        [Test]
        public void OpenTermsOfService_CompletesSuccessfully()
        {
            Assert.DoesNotThrow(() => JestSDK.Instance.OpenTermsOfService());
        }

        [Test]
        public void OpenCopyright_CompletesSuccessfully()
        {
            Assert.DoesNotThrow(() => JestSDK.Instance.OpenCopyright());
        }

        #endregion

        #region Player GetSigned Tests

        [Test]
        public void Player_GetSigned_ReturnsExpectedResponse()
        {
            var task = JestSDK.Instance.Player.GetSigned();
            var response = task.GetResult();

            Assert.That(response, Is.Not.Null);
            Assert.That(response.player, Is.Not.Null);
            Assert.That(response.player.playerId, Is.EqualTo(testId));
            Assert.That(response.player.registered, Is.True);
            Assert.That(response.playerSigned, Is.Not.Empty);
        }

        #endregion
    }
}
