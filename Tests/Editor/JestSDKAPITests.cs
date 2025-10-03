using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace JestSDK.Tests
{
    public class JestSDKAPITests
    {
        private JestSDK _jest;
        private TestBridgeMock _mock;

        private string testId = "test-player-id";

        [SetUp]
        public void Setup()
        {
            _mock = new TestBridgeMock(testId, true);
            JsBridge.SetMock(_mock);
            _jest = new JestSDK();
        }

        [Test]
        public void Player_GetId_ReturnsCorrectId()
        {
            Assert.That(_jest.player.id, Is.EqualTo(testId));
        }

        [Test]
        public void Player_IsRegistered_ReturnsCorrectValue()
        {
            Assert.That(_jest.player.isRegistered, Is.True);
        }

        [Test]
        public void Player_SetAndGetValue_WorksWithPrimitives()
        {
            _jest.player.Set("bool", true);
            _jest.player.Set("int", 42);
            _jest.player.Set("float", 3.14f);
            _jest.player.Set("string", "test");

            Assert.That(_jest.player.Get<bool>("bool"), Is.True);
            Assert.That(_jest.player.Get<int>("int"), Is.EqualTo(42));
            Assert.That(_jest.player.Get<float>("float"), Is.EqualTo(3.14f));
            Assert.That(_jest.player.Get<string>("string"), Is.EqualTo("test"));
        }

        [Test]
        public void Player_TryGet_ReturnsCorrectValue()
        {
            const string key = "key";
            const string value = "value";
            const string anotherKey = "anotherKey";

            _jest.player.Set(key, value);

            Assert.That(_jest.player.TryGet(key, out string val) && val == value);
            Assert.That(!_jest.player.TryGet(anotherKey, out string anotherVal));
        }

        [Test]
        public void Player_SetAndGetValue_WorksWithVectors()
        {
            var vector2 = new Vector2(1, 2);
            var vector3 = new Vector3(1, 2, 3);

            _jest.player.Set("vector2", vector2);
            _jest.player.Set("vector3", vector3);

            Assert.That(_jest.player.Get<Vector2>("vector2"), Is.EqualTo(vector2));
            Assert.That(_jest.player.Get<Vector3>("vector3"), Is.EqualTo(vector3));
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

            var actualPayload = _jest.GetEntryPayload();

            Assert.That(actualPayload, Is.EqualTo(expectedPayload));
        }

        [Test]
        public void Analytics_CaptureEvent_StoresEventData()
        {
            var eventName = "test-event";
            var eventData = new TestEventData { stringValue = "test", numberValue = 42 };
            _jest.analytics.CaptureEvent(eventName, eventData);

            var parsedEvent = _jest.analytics.GetEvent<TestEventData>(eventName);

            Assert.AreEqual(eventData, parsedEvent);
        }

        [Test]
        public void Analytics_CaptureEvent_StoresEventData_Dictionary()
        {
            var eventName = "test-event";
            var eventData = new Dictionary<string, object> { { "stringValue", "test" }, { "numberValue", 42 } };
            _jest.analytics.CaptureEvent(eventName, eventData);

            var parsedData = _jest.analytics.GetEvent(eventName);

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

            _jest.notifications.ScheduleNotification(options);

            var notifications = _jest.notifications.GetNotifications();
            Assert.That(notifications, Has.Count.EqualTo(1));

            var result = notifications[0];
            Assert.AreEqual(options.attemptPushNotification, result.attemptPushNotification);
            Assert.AreEqual(options.deduplicationKey, result.deduplicationKey);
            Assert.AreEqual(options.date, result.date);
            Assert.AreEqual(options.data, result.data);
            Assert.AreEqual(options.message, result.message);
        }

        [System.Serializable]
        private struct TestEventData
        {
            public string stringValue;
            public int numberValue;
        }
    }
}
