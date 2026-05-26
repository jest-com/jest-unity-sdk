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

        private static readonly string[] Scenarios = { "core", "player-data", "commerce-read" };
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

            JestSDK.Instance.SetLoadingProgress(50);
            JestSDK.Instance.SetLoadingProgress(100);

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

            JestSDK.Instance.Player.Delete(key);
            var deletedValue = JestSDK.Instance.Player.Get(key);
            assertions.Add(RegressionAssertion.Condition(
                "player data delete clears value",
                string.IsNullOrEmpty(deletedValue),
                deletedValue,
                "empty value"));

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
            }

            return assertions;
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
