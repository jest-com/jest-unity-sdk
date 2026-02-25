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

        #endregion
    }
}
