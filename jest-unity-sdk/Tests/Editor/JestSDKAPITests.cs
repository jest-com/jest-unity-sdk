using NUnit.Framework;
using UnityEngine;
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
        public void Analytics_CaptureEvent_StoresEventData()
        {
            var eventName = "test-event";
            var eventData = new TestEventData { stringValue = "test", numberValue = 42 };
            JestSDK.Instance.Analytics.CaptureEvent(eventName, eventData);

            var parsedEvent = JestSDK.Instance.Analytics.GetEvent<TestEventData>(eventName);

            Assert.AreEqual(eventData, parsedEvent);
        }

        [Test]
        public void Analytics_CaptureEvent_StoresEventData_Dictionary()
        {
            var eventName = "test-event";
            var eventData = new Dictionary<string, object> { { "stringValue", "test" }, { "numberValue", 42 } };
            JestSDK.Instance.Analytics.CaptureEvent(eventName, eventData);

            var parsedData = JestSDK.Instance.Analytics.GetEvent(eventName);

            Assert.AreEqual(eventData, parsedData);
        }

        [Test]
        public void Notifications_ScheduleNotification_StoresNotification()
        {
            var date = DateTime.Now;
            var options = new Notifications.Options
            {
                message = "Test notification",
                date = date,
                deduplicationKey = "test-key",
                attemptPushNotification = true,
                data = { { "stringValue", "test" }, { "numberValue", 42 } }
            };

            JestSDK.Instance.Notifications.ScheduleNotification(options);

            var notifications = JestSDK.Instance.Notifications.GetNotifications();
            Assert.That(notifications, Has.Count.EqualTo(1));

            var result = notifications[0];
            Assert.AreEqual(options.attemptPushNotification, result.attemptPushNotification);
            Assert.AreEqual(options.deduplicationKey, result.deduplicationKey);
            Assert.AreEqual(options.date, result.date);
            Assert.AreEqual(options.data, result.data);
            Assert.AreEqual(options.message, result.message);
        }

        [Test]
        public void Notifications_ScheduleRichNotification_StoresNotification()
        {
            var options = new RichNotifications.Options
            {
                plainText = "Test notification",
                body = "Test Body",
                ctaText = "Play Now!",
                data = { { "stringValue", "test" }, { "numberValue", 42 } },
                identifier = "test-key",
                date = DateTime.Now,
                notificationPriority = RichNotifications.Severity.Low
            };
            
            JestSDK.Instance.RichNotifications.ScheduleNotification(options);
            var notifications = JestSDK.Instance.RichNotifications.GetNotifications();
            Assert.That(notifications, Has.Count.EqualTo(1));
            var result = notifications[0];
            Assert.AreEqual(options.plainText, result.plainText);
            Assert.AreEqual(options.body, result.body);
            Assert.AreEqual(options.ctaText, result.ctaText);
            Assert.AreEqual(options.date, result.date);
            Assert.AreEqual(options.data, result.data);
            Assert.AreEqual(options.identifier, result.identifier);
            Assert.AreEqual(options.notificationPriority, result.notificationPriority);
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
            Assert.AreEqual(products[0].price, result.purchase.credits);
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
        public void Payment_CompletePurchase_ThrowsWhenNotRegistered()
        {
            var unregisteredMock = new TestBridgeMock(testId, false);
            JsBridge.SetMock(unregisteredMock);

            Assert.Throws<InvalidOperationException>(() =>
                JestSDK.Instance.Payment.CompletePurchase("mock_token"));

            // Restore original mock
            JsBridge.SetMock(_mock);
        }

        [Test]
        public void Payment_GetIncompletePurchases_ThrowsWhenNotRegistered()
        {
            var unregisteredMock = new TestBridgeMock(testId, false);
            JsBridge.SetMock(unregisteredMock);

            Assert.Throws<InvalidOperationException>(() =>
                JestSDK.Instance.Payment.GetIncompletePurchases());

            // Restore original mock
            JsBridge.SetMock(_mock);
        }

        [Test]
        public void RichNotifications_ScheduleNotification_WithImageReference()
        {
            var options = new RichNotifications.Options
            {
                plainText = "Test notification",
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
                plainText = "Test notification",
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
                plainText = "Test notification",
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
        public void RichNotifications_DeprecatedAliases_StillWork()
        {
            var options = new RichNotifications.Options();

            // Test deprecated 'image' alias
            #pragma warning disable CS0618 // Disable obsolete warning for test
            options.image = "test-image-url";
            Assert.That(options.imageReference, Is.EqualTo("test-image-url"));
            Assert.That(options.image, Is.EqualTo("test-image-url"));

            // Test deprecated 'data' alias
            options.data["testKey"] = "testValue";
            Assert.That(options.entryPayloadData["testKey"], Is.EqualTo("testValue"));
            Assert.That(options.data["testKey"], Is.EqualTo("testValue"));
            #pragma warning restore CS0618
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
    }
}
