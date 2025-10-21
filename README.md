# Jest SDK for Unity

A Unity package that provides seamless integration with the JestSDK platform for game analytics, player management, and notification scheduling.

## Features

- **Player Management**: Easily store and retrieve player data
- **Analytics**: Track in-game events and player behavior
- **Notifications**: Schedule in-game and push notifications
- **Mock System**: Built-in mocking capabilities for testing

## Installation

1. Open the Unity Package Manager (Window > Package Manager)
2. Click the "+" button and select "Add package from git URL"
3. Enter: `https://github.com/jest-com/jest-unity-sdk.git`

## Examples

### Entry Payload

Session can have payloads associated with them to provide additional context.
They are serialized into JSON and delivered as JSON from the platform.

The `GetEntryPayload` method fetches and returns the payload as Dictionary<string, object>.

```csharp
var jest = JestSDK.Instance;
var entryPayload = jest.GetEntryPayload();
if (entryPayload.TryGetValue("key", out var value))
{
    // Do something with the value
}
```

### Player Data Management

#### Getting Player Data

```csharp
var player = JestSDK.Instance.Player;

// Basic player info
string playerId = player.id;
bool isRegistered = player.isRegistered;

// Custom struct example
public struct PlayerStats
{
    public int wins;
    public int losses;
    public float winRate;
}

// Get custom properties
int level = player.Get<int>("level");
string nickname = player.Get<string>("nickname");
PlayerStats stats = player.Get<PlayerStats>("stats");
```

#### Setting Player Data

```csharp
var player = JestSDK.Instance.Player;

// Set individual properties
player.Set("level", 5);
player.Set("lastLogin", DateTime.Now);
player.Set("inventory", "sword");

// Set custom struct
player.Set("stats", new PlayerStats {
    wins = 10,
    losses = 2,
    winRate = 0.83f
});
```

### Analytics Events

#### Basic Events

```csharp
var analytics = JestSDK.Instance.Analytics;

// Simple event without properties
analytics.CaptureEvent("game_started");

// Event with dictionary properties
analytics.CaptureEvent("item_purchased", new Dictionary<string, object> {
    { "itemId", "power_boost" },
    { "price", 99 },
    { "currency", "coins" },
    { "balance", 901 }
});
```

#### Structured Events

```csharp
// Define event structure
public struct LevelCompleteEvent
{
    public int level;
    public int score;
    public float timeSpent;
    public bool perfectRun;
    public string difficulty;
}

// Capture structured event
analytics.CaptureEvent("level_complete", new LevelCompleteEvent {
    level = 5,
    score = 1000,
    timeSpent = 120.5f,
    perfectRun = true,
    difficulty = "hard"
});
```

### Notifications

```csharp
var notifications = JestSDK.Instance.Notifications;

// Schedule for specific time
notifications.ScheduleNotification(new Notifications.Options {
    message = "Your energy is full!",
    date = DateTime.Now.AddHours(1),
    attemptPushNotification = true
});

```


### RichNotifications

```csharp
var richNotifications = JestSDK.Instance.RichNotifications;

// Schedule for specific time
richNotifications.ScheduleNotification(new RichNotifications.Options {
    
    body = <The text of the notification>,
    plainText = <Simple formatting (for SMS)>,
    ctaText = <Call to Action text: what is shown next to the button>,
    image = <Base64 data URL for the notification image>, //Optional
    notificationPriority = <low, medium, high, critical>,
    identifier = <unique Identifier>,
    date = <When should it be scheduled for>
});

// Unschedule with unique notification key

richNotifications.UnscheduleNotification(<unique Identifier>);

```

## Testing

The SDK includes a ScriptableObject-based mock system for testing in the Unity Editor. This allows you to observe and debug SDK values directly in the Unity Inspector.

### Creating a Mock Configuration

1. In your Project window, right-click and select:
   `Create > JestSDK > Mock`

2. Name your configuration (e.g. "JestMock")

3. In the Inspector, you can:
   - Set the mock player ID
   - Configure initial player data
   - View analytics events
   - Monitor scheduled notifications
   - Toggle registered/unregistered state

This is especially useful for:

- Debugging player progression
- Verifying analytics event data
- Testing notification scheduling
- Simulating different player scenarios

The mock configuration persists in edit mode, allowing you to maintain test data between play sessions.

## Requirements and dependencies

- Unity 6000.0 or later
- Newtonsoft.Json 3.2.1 or later
