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

## No drive-by styling changes

When you add a new feature, **the diff must contain only that feature**. Do not reformat, reindent, rewrap, requote, or otherwise restyle surrounding lines you didn't need to functionally change. This applies to every file type in the repo — `.cs`, `.jslib`, `.unity`, `.asmdef`, JSON, Markdown.

Specific traps to avoid:

- **`Runtime/Plugins/JestSDK.jslib`** is **not plain JavaScript**. It uses Emscripten/Unity templating macros like `{{{ makeDynCall("vi", 'successCallback') }}}` that a normal JS formatter (Prettier, ESLint --fix) will silently destroy. **Never run a JS formatter on this file.** If your editor formats on save, disable it for `*.jslib`. The existing style is: single quotes, no trailing commas in argument lists, compact single-line function signatures — match it.
- **C# files** — do not switch quote style, brace placement, `using` ordering, tab/space mix, or trailing-newline state of files you're editing. Diff-review your own changes before committing: if `git diff` shows reformatting of lines unrelated to your feature, revert those hunks.
- **Unity scene/prefab files** (`*.unity`, `*.prefab`) — never hand-edit. Open them in the Unity editor or leave them alone. A formatter or text-level "cleanup" can break GUIDs, references, and serialization order.
- **`package.json` / `manifest.json`** — preserve key order. Don't alphabetize or reformat just because you touched one entry.

If a sync script or upstream tool produced a diff with mixed feature + restyle changes, **revert the file to the base branch (`git checkout origin/main -- <file>`) and re-apply only the feature additions by hand**. The cleanup is part of the task, not optional.

## Other Unity-specific notes

- `[SerializeField] private` fields exposed to the Unity inspector use the `m_camelCase` pattern in this repo — see `Samples~/Demo/Scripts/`.
- `internal` constructors are used on SDK-public classes to prevent external instantiation (see `Referrals`, `Player`, etc.). Keep this when adding new top-level SDK classes.
- The JS bridge code in `Runtime/Plugins/JestSDK.jslib` is JavaScript and uses camelCase — that's correct for JS and must not be changed.
