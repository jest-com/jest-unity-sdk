using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using com.jest.sdk;

namespace com.jest.sdk.Tests
{
    /// <summary>
    /// Guards the row order of <see cref="JestUtils.EncodeTextureToPng"/>. The encoder blits
    /// through a RenderTexture, and an API-conditional flip there shipped every WebGL build's
    /// referral share image upside down while looking upright in a Metal/D3D editor. Encoding
    /// a texture with distinct top and bottom rows catches that on whichever API runs.
    /// </summary>
    public class JestUtilsEncodingTests
    {
        [Test]
        public void EncodeTextureToPng_KeepsTopRowOnTop()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore("No graphics device — EncodeTextureToPng needs a RenderTexture blit.");
            }

            var source = new Texture2D(1, 2, TextureFormat.RGBA32, false);
            source.SetPixel(0, 1, Color.red);
            source.SetPixel(0, 0, Color.blue);
            source.Apply();

            byte[] png;
            try
            {
                png = JestUtils.EncodeTextureToPng(source);
            }
            finally
            {
                Object.DestroyImmediate(source);
            }

            Assert.That(png, Is.Not.Null.And.Not.Empty, "EncodeTextureToPng returned no bytes.");

            var decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(decoded.LoadImage(png), Is.True, "Encoded bytes are not a decodable PNG.");

                // Logged on pass too: only a UV-at-bottom API can catch a regression of the WebGL flip.
                var context = $"graphicsDeviceType={SystemInfo.graphicsDeviceType}, " +
                    $"graphicsUVStartsAtTop={SystemInfo.graphicsUVStartsAtTop}";
                TestContext.WriteLine(context);
                Assert.That(decoded.GetPixel(0, 1).r, Is.GreaterThan(0.5f),
                    $"Expected the source's top row (red) on top; the PNG is vertically flipped. {context}");
                Assert.That(decoded.GetPixel(0, 0).b, Is.GreaterThan(0.5f),
                    $"Expected the source's bottom row (blue) on the bottom; the PNG is vertically flipped. {context}");
            }
            finally
            {
                Object.DestroyImmediate(decoded);
            }
        }
    }
}
