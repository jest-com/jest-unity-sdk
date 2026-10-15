using System;
using Newtonsoft.Json;

namespace com.jest.sdk
{
    /// <summary>
    /// How the player entered the current game session.
    /// Read <see cref="Type"/> first; only the fields for that type are set, the rest are null.
    /// </summary>
    [Serializable]
    public class EntrySource
    {
        /// <summary>
        /// One of <c>explore</c>, <c>home</c>, <c>profile</c>, <c>chat</c>, <c>notification</c>,
        /// <c>jest_ad</c>, <c>external_ad</c>, <c>referral</c>, <c>direct</c>, or <c>unknown</c>.
        /// </summary>
        [JsonProperty("type")]
        public string Type;

        /// <summary>
        /// For <c>explore</c>: <c>feed</c> or <c>search</c>.
        /// For <c>chat</c>: <c>flair</c>, <c>link</c>, <c>screenshot</c>, or <c>room_header</c>.
        /// </summary>
        [JsonProperty("via")]
        public string Via;

        /// <summary>
        /// For <c>home</c>: <c>recents</c>, <c>recommended</c>, <c>activity</c>, or <c>all_games</c>.
        /// </summary>
        [JsonProperty("section")]
        public string Section;

        /// <summary>
        /// For <c>profile</c>: <c>own</c> or <c>other</c>.
        /// </summary>
        [JsonProperty("whose")]
        public string Whose;

        /// <summary>
        /// For <c>notification</c> and <c>referral</c>: <c>game</c> or <c>platform</c>.
        /// </summary>
        [JsonProperty("origin")]
        public string Origin;

        /// <summary>
        /// For <c>notification</c>: the identifier this game passed when scheduling the notification,
        /// or null when unavailable.
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier;

        /// <summary>
        /// For <c>jest_ad</c>: the Jest Ads campaign that brought the player in.
        /// </summary>
        [JsonProperty("campaign")]
        public EntrySourceCampaign Campaign;

        /// <summary>
        /// For <c>external_ad</c>: the decoded UTM parameters of the ad link.
        /// </summary>
        [JsonProperty("utm")]
        public EntrySourceUtm Utm;
    }

    /// <summary>
    /// A Jest Ads campaign attached to a <c>jest_ad</c> entry source.
    /// </summary>
    [Serializable]
    public class EntrySourceCampaign
    {
        /// <summary>
        /// Stable campaign ID, as used in Ads Manager URLs.
        /// </summary>
        [JsonProperty("id")]
        public string Id;

        /// <summary>
        /// Campaign name set in Ads Manager; changes when the campaign is renamed.
        /// </summary>
        [JsonProperty("name")]
        public string Name;
    }

    /// <summary>
    /// UTM parameters attached to an <c>external_ad</c> entry source. Missing values are null.
    /// </summary>
    [Serializable]
    public class EntrySourceUtm
    {
        [JsonProperty("source")]
        public string Source;

        [JsonProperty("medium")]
        public string Medium;

        [JsonProperty("campaign")]
        public string Campaign;

        [JsonProperty("term")]
        public string Term;

        [JsonProperty("content")]
        public string Content;
    }
}
