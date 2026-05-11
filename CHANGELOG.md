# Changelog

## [1.8.0] - 2026-05-07

### Added
- `Payment.PurchaseData.price` (`decimal`) and `Payment.PurchaseData.currency`
  (`string`, ISO 4217) — populated from the platform's purchase response so
  games can display the amount paid in the original currency.
- `Payment.Product.currency` (`string`, ISO 4217) — alongside `price`, returned
  by `JestSDK.Instance.Payment.GetProducts()` so games can format prices correctly.
- `JestSDK.Instance.GetBotAvatar(username, size = 1000)` — returns a CDN URL for
  a deterministic bot avatar seeded by username. `size` accepts 64, 128, 256,
  512, or 1000; intermediate values bucket down to the next supported size. The
  hash is bit-compatible with the HTML5 SDK so the same username yields the same
  avatar across SDKs.
- `RichNotifications.BODY_CHAR_LIMIT` (2000) and `TITLE_CHAR_LIMIT` (200) public
  constants alongside the existing `CTA_CHAR_LIMIT`.

### Changed
- `RichNotifications.ScheduleNotification` now also throws `ArgumentException` when:
  - `body` exceeds 2000 characters.
  - `title` (when set) exceeds 200 characters.
  - `ctaText` limit raised from 25 to 50 characters (mirrors HTML5 SDK).
- `JestSDK.Instance.OpenPrivacyPolicy()`, `OpenTermsOfService()`, and
  `OpenCopyright()` doc-marked as internal. They remain callable but are not
  part of the supported public API surface (mirrors the HTML5 SDK).

## [1.7.0] - 2026-04-29

### Added
- Custom registration overlay.

## [1.6.0] - 2026-04-24

### Added
- `Player.username` and `Player.avatarUrl` properties — mirror the new fields
  returned by the platform's `getPlayer()` response.
- `PlayerInfo` DTO (returned from `Player.GetSigned()`) now carries `username`
  and `avatarUrl`.
- `RichNotifications.Options.title` — optional notification title.
- `RichNotifications.Options.assetReference` — preferred replacement for
  `imageReference` (now deprecated but still honored).
- `RichNotifications.Severity.Critical` — highest priority level.
- `JestSDK.Instance.RegistrationOverlay.Show(options)` — opens the platform
  registration overlay with game-rendered UI. Returns a `Handle` with
  `LoginButtonAction()`, `CloseButtonAction()`, and an `OnClose` event.
- `Internal.ValidateName(name)` — validates a player name against platform rules.
- `Internal.CaptureOnboardingEvent(eventName, properties)` — reports onboarding
  analytics events. Event-name constants live on `Internal.OnboardingEvents`.

### Changed
- `sdkVersion` sent on postMessage envelopes now tracks the package version
  dynamically (propagated via `JS_setSdkVersion` at init time).
- Version bumped from `1.5.2` to `1.6.0`. CHANGELOG reconciled with package.json.

### Deprecated
- `RichNotifications.Options.imageReference` — use `assetReference` instead.

## [1.2.0] - 2026-03-06

### Breaking Changes
- Remove `plainText` field from `RichNotifications.Options` (V2 notifications use `body` only)
- Remove `Critical` from `RichNotifications.Severity` enum (valid values: Low, Medium, High)
- Remove obsolete `image` and `data` property aliases from `RichNotifications.Options`
- `identifier` is now required when scheduling notifications
- `scheduledInDays` range reduced from 1-14 to 1-7

### Added
- Client-side validation for `ScheduleNotification` matching platform constraints:
  - `body` and `ctaText` are required
  - `ctaText` must be 1-25 characters
  - `identifier` is required
  - `scheduledInDays` must be 1-7
  - `scheduledAt` must be within 7 days

### Fixed
- Demo controller field mapping (body/plainText were swapped)

## [1.1.0] - 2026-02-25

- Update api support to include Navigation, Referrals, Legal Pages
- Update existing Player, Payment, and Notification api
- Restructure package for cleaner UPM distribution
- Add importable Demo Sample via Package Manager

## [1.0.0] - 2025-10-03

- Published initial version as Jest Unity SDK

