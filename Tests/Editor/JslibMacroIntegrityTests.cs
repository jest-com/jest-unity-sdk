using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor.PackageManager;
using com.jest.sdk;

namespace com.jest.sdk.Tests
{
    /// <summary>
    /// Guards the Emscripten macros in JestSDK.jslib. The `{{{ makeDynCall(...) }}}`
    /// triple braces are expanded at WebGL build time; if a reformat (e.g. Prettier)
    /// strips them, the compiled framework keeps a literal `makeDynCall` reference and
    /// every WebGL build crashes at runtime with `ReferenceError: makeDynCall is not
    /// defined` during SDK init. A static check is enough to catch that class of
    /// regression — no WebGL build required.
    /// </summary>
    public class JslibMacroIntegrityTests
    {
        private static string ReadJslib()
        {
            var packageRoot = PackageInfo.FindForAssembly(typeof(SdkVersion).Assembly)?.resolvedPath
                ?? Path.GetFullPath("Packages/com.jest.sdk");
            var path = Path.Combine(packageRoot, "Runtime", "Plugins", "JestSDK.jslib");
            Assert.That(File.Exists(path), Is.True, $"JestSDK.jslib not found at {path}");
            return File.ReadAllText(path);
        }

        [Test]
        public void EveryMakeDynCallIsWrappedInExpansionBraces()
        {
            var content = ReadJslib();

            var total = Regex.Matches(content, @"\bmakeDynCall\b").Count;
            var wrapped = Regex.Matches(content, @"\{\{\{\s*makeDynCall\b").Count;

            Assert.That(total, Is.GreaterThan(0),
                "Expected makeDynCall macros in JestSDK.jslib; none found — the file may have been restructured.");
            Assert.That(wrapped, Is.EqualTo(total),
                $"{total - wrapped} of {total} makeDynCall usages are not wrapped in `{{{{{{ ... }}}}}}`. " +
                "The triple braces are a build-time Emscripten macro and must be preserved verbatim; " +
                "a Prettier-style reformat strips them and breaks every WebGL build at runtime.");
        }

        [Test]
        public void ExpansionBracesAreBalanced()
        {
            var content = ReadJslib();

            var open = Regex.Matches(content, @"\{\{\{").Count;
            var close = Regex.Matches(content, @"\}\}\}").Count;

            Assert.That(open, Is.EqualTo(close),
                $"Unbalanced Emscripten macro braces in JestSDK.jslib: {open} `{{{{{{` vs {close} `}}}}}}`.");
        }
    }
}
