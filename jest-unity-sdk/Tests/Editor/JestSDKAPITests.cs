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
        public void Notifications_ScheduleNotificationV2_StoresNotification()
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

        [System.Serializable]
        private struct TestEventData
        {
            public string stringValue;
            public int numberValue;
        }
    }
}
