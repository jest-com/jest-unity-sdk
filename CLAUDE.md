# Repository guidance for Claude Code

This repo is the **Jest Unity SDK** — a C# Unity package that exposes a JS bridge to the Jest web platform. It is a public-facing SDK, so naming and API shape matter to downstream consumers.

## C# naming — required for new code

When you add new C# code in this repo, follow standard .NET naming conventions, even if surrounding code in the same file does not. The legacy code in `Runtime/Scripts/` uses camelCase public fields; **do not propagate that pattern**. Match the language convention, not the file.

| Element | Convention | Example |
|---|---|---|
| Public field | PascalCase | `public string PlayerId;` |
| Public property | PascalCase | `public int Score { get; set; }` |
| Public method | PascalCase | `public void OpenDialog()` |
| Public class / struct / enum / interface | PascalCase (interfaces start with `I`) | `class ReferralInfo`, `interface IBridgeMock` |
| Private/internal field | `m_camelCase` or `_camelCase` (match the surrounding file) | `private string m_referenceInput;` |
| Local variable / parameter | camelCase | `var entryPayload = ...;` |
| Constant | PascalCase | `public const int MaxRetries = 3;` |

## JSON wire compatibility (load-bearing)

Most public model classes in `Runtime/Scripts/` are serialized to JSON for the JS bridge, which expects **camelCase keys**. When you rename a C# field to PascalCase, you MUST preserve the JSON key with a `[JsonProperty]` attribute, or the bridge breaks silently.

```csharp
using Newtonsoft.Json;

[Serializable]
public class ReferralNotificationVariant
{
    [JsonProperty("title")]
    public string Title;

    [JsonProperty("ctaText")]
    public string CtaText;
}
```

If a field is built into a `Dictionary<string, object>` by hand inside a `ToJson()` method (the pattern used by `OpenDialogOptions.ToJson()`), update the dictionary key string AND keep `[JsonProperty]` on the field — the same model is also deserialized by Newtonsoft elsewhere (e.g. in tests, and from bridge responses).

## Don't bulk-rename existing fields

If you are touching a file that has legacy camelCase public fields, do not opportunistically rename them. Renames to public API are breaking changes for consumers (Unity scenes serialize fields by name; downstream game code references them by name). Only rename fields the user has explicitly asked you to change, or fields you are introducing yourself.

## Other Unity-specific notes

- `[SerializeField] private` fields exposed to the Unity inspector use the `m_camelCase` pattern in this repo — see `Samples~/Demo/Scripts/`.
- `internal` constructors are used on SDK-public classes to prevent external instantiation (see `Referrals`, `Player`, etc.). Keep this when adding new top-level SDK classes.
- The JS bridge code in `Runtime/Plugins/JestSDK.jslib` is JavaScript and uses camelCase — that's correct for JS and must not be changed.
