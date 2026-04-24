using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.jest.sdk
{
    /// <summary>
    /// Provides internal/experimental SDK functionality.
    /// These methods may change without notice.
    /// </summary>
    public class Internal
    {
        /// <summary>
        /// Internal constructor to prevent external instantiation.
        /// </summary>
        internal Internal() { }

        /// <summary>
        /// Reserves a login message for SMS registration flow.
        /// </summary>
        /// <param name="options">The reservation options.</param>
        /// <returns>A task resolving to the reservation response JSON.</returns>
        public JestSDKTask<ReserveLoginMessageResponse> ReserveLoginMessageAsync(ReserveLoginMessageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.message))
            {
                throw new ArgumentException("message is required", nameof(options));
            }

            var task = new JestSDKTask<ReserveLoginMessageResponse>();
            var optionsJson = JsonConvert.SerializeObject(options);
            var reserveTask = JsBridge.ReserveLoginMessage(optionsJson);

            reserveTask.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }

                    string json = t.GetResult();
                    var response = JsonConvert.DeserializeObject<ReserveLoginMessageResponse>(json);
                    task.SetResult(response);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });

            return task;
        }

        /// <summary>
        /// Sends a previously reserved login message.
        /// </summary>
        /// <param name="reservation">The reservation to send.</param>
        public void SendReservedLoginMessage(LoginReservation reservation)
        {
            if (reservation == null)
            {
                throw new ArgumentNullException(nameof(reservation));
            }

            var reservationJson = JsonConvert.SerializeObject(reservation);
            JsBridge.SendReservedLoginMessage(reservationJson);
        }

        /// <summary>
        /// Validates a player name against platform rules.
        /// </summary>
        /// <param name="name">The name to validate.</param>
        /// <returns>A task resolving to the validation result.</returns>
        public JestSDKTask<NameValidationResult> ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name cannot be empty", nameof(name));
            }

            var task = new JestSDKTask<NameValidationResult>();
            var inner = JsBridge.ValidateName(name);
            inner.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        task.SetException(t.Exception);
                        return;
                    }
                    var response = JsonConvert.DeserializeObject<NameValidationResult>(t.GetResult());
                    task.SetResult(response);
                }
                catch (Exception e)
                {
                    task.SetException(e);
                }
            });
            return task;
        }

        /// <summary>
        /// Captures an onboarding analytics event. Event names should match
        /// one of the values in <see cref="OnboardingEvents"/>.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">Optional event properties.</param>
        public void CaptureOnboardingEvent(string eventName, Dictionary<string, object> properties = null)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException("eventName cannot be empty", nameof(eventName));
            }
            string propertiesJson = properties != null ? JsonConvert.SerializeObject(properties) : null;
            JsBridge.CaptureOnboardingEvent(eventName, propertiesJson);
        }

        #region Nested Classes

        /// <summary>
        /// Options for reserving a login message.
        /// </summary>
        [Serializable]
        public class ReserveLoginMessageOptions
        {
            /// <summary>
            /// The message to reserve.
            /// </summary>
            public string message;

            /// <summary>
            /// Optional SMS keywords to trigger registration.
            /// </summary>
            public List<string> keywords;

            /// <summary>
            /// Optional custom reply message.
            /// </summary>
            public CustomTextMessage replyMessage;

            /// <summary>
            /// Optional custom reminder message.
            /// </summary>
            public CustomTextMessage reminderMessage;

            /// <summary>
            /// Optional entry payload data.
            /// </summary>
            public Dictionary<string, object> entryPayload;
        }

        /// <summary>
        /// Custom text message structure.
        /// </summary>
        [Serializable]
        public class CustomTextMessage
        {
            /// <summary>
            /// Main notification text.
            /// </summary>
            public string body;

            /// <summary>
            /// SMS fallback text.
            /// </summary>
            public string plainText;

            /// <summary>
            /// Call-to-action button text.
            /// </summary>
            public string ctaText;
        }

        /// <summary>
        /// Response from reserving a login message.
        /// </summary>
        [Serializable]
        public class ReserveLoginMessageResponse
        {
            /// <summary>
            /// The reservation if successful.
            /// </summary>
            public LoginReservation reservation;

            /// <summary>
            /// Error code if the reservation failed.
            /// </summary>
            public string error;
        }

        /// <summary>
        /// A login message reservation.
        /// </summary>
        [Serializable]
        public class LoginReservation
        {
            /// <summary>
            /// The reservation ID.
            /// </summary>
            public string id;

            /// <summary>
            /// The reserved message.
            /// </summary>
            public string message;
        }

        /// <summary>
        /// Result of a name validation check.
        /// </summary>
        [Serializable]
        public class NameValidationResult
        {
            /// <summary>
            /// "valid" when the name passes all checks; "invalid" otherwise.
            /// </summary>
            public string status;

            /// <summary>
            /// When <see cref="status"/> is "invalid", the reason: "too_short", "too_long", or "inappropriate".
            /// </summary>
            public string validationError;
        }

        /// <summary>
        /// Valid event names for <see cref="CaptureOnboardingEvent"/>.
        /// </summary>
        public static class OnboardingEvents
        {
            public const string SmsModalShow = "sms_modal_show";
            public const string ExitPopupShow = "exit_popup_show";
            public const string ContinueExitClick = "continue_exit_click";
            public const string AbandonExitClick = "abandon_exit_click";
            public const string GameInstanceCreate = "game_instance_create";
            public const string GameLoadStart = "game_load_start";
            public const string GameLoadComplete = "game_load_complete";
            public const string GameSceneInitialize = "game_scene_initialize";
            public const string GameSceneEnter = "game_scene_enter";
            public const string UniqueTextStart = "unique_text_start";
            public const string UniqueTextSubmit = "unique_text_submit";
            public const string OnboardingComplete = "onboarding_complete";
            public const string OnboardingFail = "onboarding_fail";
            public const string FirstInteraction = "first_interaction";
            public const string SmsButtonTap = "sms_button_tap";
        }

        #endregion
    }
}
