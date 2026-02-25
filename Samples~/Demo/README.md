# Jest SDK Demo

This sample demonstrates all features of the Jest SDK.

## Prerequisites

- **TextMesh Pro**: This sample requires TextMesh Pro. Install it via:
  - Window > Package Manager > Unity Registry > TextMesh Pro
  - After installation, import TMP Essential Resources when prompted

## Setup

1. Import this sample via Package Manager
2. Open `Scenes/GameScene.unity`
3. The JestSDKMock asset in `ScriptableObjects/` is pre-configured for testing

## Features Demonstrated

- **Player Login**: Login flow with payload
- **Player State**: Set, get, delete, and flush player data
- **Payments**: Product listing, purchase flow, incomplete purchase handling
- **Rich Notifications**: Schedule and manage notifications
- **Events/Analytics**: Track custom events
- **Referrals**: Referral system integration
- **Navigation**: In-app navigation controls
- **Legal Pages**: Open terms of service and privacy policy

## Mock Testing

The `JestSDKMock.asset` ScriptableObject allows you to test SDK functionality in the Unity Editor without a live backend. Configure mock responses for:

- Player registration state
- Purchase products
- Purchase success/failure simulation

The mock configuration persists in edit mode, allowing you to maintain test data between play sessions.
