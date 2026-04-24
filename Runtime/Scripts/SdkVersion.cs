namespace com.jest.sdk
{
    /// <summary>
    /// Version of this SDK. Kept in sync with package.json and CHANGELOG.md.
    /// Propagated to the JS layer at init time so the <c>sdkVersion</c> field
    /// on outgoing postMessage envelopes identifies this exact release.
    /// </summary>
    public static class SdkVersion
    {
        public const string Value = "1.6.0";

        internal const string WireName = "unity-sdk-" + Value;
    }
}
