# Changelog

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

