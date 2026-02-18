# Jest SDK for Unity

A Unity package that provides seamless integration with the Jest platform for game analytics, player management, notifications, payments, referrals, and more.

## Features

- **Player Management**: Store, retrieve, and sync player data
- **Analytics**: Track in-game events and player behavior
- **Notifications**: Schedule in-game and push notifications with rich content support
- **Payments**: In-app purchases with purchase verification
- **Referrals**: Player referral system with tracking
- **Navigation**: Redirect players between games
- **Legal Pages**: Display privacy policy, terms of service, and copyright pages
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

### Login

Register the current player. The optional payload can be used to pass additional context.

```csharp
var jest = JestSDK.Instance;

// Login without payload
jest.Login();

// Login with payload
jest.Login(new Dictionary<string, object> {});
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

#### Deleting Player Data

```csharp
var player = JestSDK.Instance.Player;

// Delete a specific key
player.Delete("temporaryBoost");
```

#### Syncing Player Data

```csharp
var player = JestSDK.Instance.Player;

// Ensure all pending updates are synced to the server
await player.Flush();
```

#### Getting Signed Player Data

For server-side verification, you can get signed player data:

```csharp
var player = JestSDK.Instance.Player;

var signedTask = player.GetSigned();
await signedTask;

if (signedTask.IsCompleted)
{
    var response = signedTask.GetResult();
    string playerId = response.player.playerId;
    bool isRegistered = response.player.registered;
    string signedPayload = response.playerSigned; // JWS format for server verification
}
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

// Schedule for specific date/time
richNotifications.ScheduleNotification(new RichNotifications.Options {
    body = <The text of the notification>,
    plainText = <Simple formatting (for SMS)>,
    ctaText = <Call to Action text: what is shown next to the button>,
    imageReference = <URL for the notification image>, // Optional
    notificationPriority = <Low, Medium, High, Critical>,
    identifier = <unique Identifier>,
    date = <When should it be scheduled for>,
    entryPayloadData = <Dictionary of custom data> // Optional
});

// Or schedule using fuzzy timing (1-14 days) instead of exact date
richNotifications.ScheduleNotification(new RichNotifications.Options {
    body = <The text of the notification>,
    plainText = <Simple formatting (for SMS)>,
    ctaText = <Call to Action text>,
    notificationPriority = <Low, Medium, High, Critical>,
    identifier = <unique Identifier>,
    scheduledInDays = <Number of days from now (1-14)>
});

// Unschedule with unique notification key
richNotifications.UnscheduleNotification(<unique Identifier>);
```


### Payment

The Jest SDK provides a robust in-app purchase (IAP) system that allows you to retrieve available products, initiate purchases, and handle incomplete transactions.

#### Getting Products
```csharp
var payment = JestSDK.Instance.Payment;
var productsTask = payment.GetProducts();

await productsTask;
if (productsTask.IsCompleted)
{
    List<Payment.Product> products = productsTask.GetResult();
    foreach (var product in products)
    {
        Debug.Log($"Product: {product.name} - {product.price}");
    }
}
```

#### Making Purchase
```csharp
var purchaseTask = JestSDK.Instance.Payment.BeginPurchase("gems_100");
await purchaseTask;

if (purchaseTask.GetResult().result == "success")
{
    Debug.Log("Purchase successful!");
}
else
{
    Debug.LogError($"Purchase failed: {purchaseTask.GetResult().error}");
}
```

#### Completing an Incomplete Purchase

Note: The player must be logged in to retrieve and complete incomplete purchases.

```csharp
var incompletePurchasesTask = JestSDK.Instance.Payment.GetIncompletePurchases();
await incompletePurchasesTask;

foreach (var purchase in incompletePurchasesTask.GetResult())
{
    Debug.Log($"Incomplete Purchase: {purchase.productSku}");
    await JestSDK.Instance.Payment.CompletePurchase(purchase.purchaseToken);
}
```


### Referrals

The referral system allows players to invite others and track registrations.

#### Opening Referral Dialog

```csharp
var referrals = JestSDK.Instance.Referrals;

// Open native share dialog with referral link
referrals.OpenReferralDialog(new Referrals.OpenDialogOptions {
    reference = "summer_campaign",
    shareTitle = "Join me in this awesome game!",
    shareText = "Use my link to get bonus rewards!",
    entryPayload = new Dictionary<string, object> {
        { "referrer_level", 25 }
    }
});
```

#### Listing Referrals

```csharp
var referrals = JestSDK.Instance.Referrals;
var listTask = referrals.ListReferrals();

await listTask;
if (listTask.IsCompleted)
{
    var response = listTask.GetResult();
    foreach (var referral in response.referrals)
    {
        Debug.Log($"Reference: {referral.reference}, Registrations: {referral.registrations.Count}");
    }
    // Use response.referralsSigned for server-side verification
}
```


### Navigation

Redirect players to other games or the flagship game.

#### Redirect to Flagship Game

```csharp
var navigation = JestSDK.Instance.Navigation;

// Simple redirect
navigation.RedirectToFlagshipGame();

// Redirect with entry payload
navigation.RedirectToFlagshipGame(new Navigation.RedirectToFlagshipGameOptions {
    entryPayload = new Dictionary<string, object> {
        { "from_game", "puzzle_master" },
        { "reward_claimed", true }
    }
});
```

#### Redirect to Specific Game

```csharp
var navigation = JestSDK.Instance.Navigation;

navigation.RedirectToGame(new Navigation.RedirectToGameOptions {
    gameSlug = "adventure-quest",
    entryPayload = new Dictionary<string, object> {
        { "cross_promo", true }
    }
});
```


### Legal Pages

Display legal pages to players.

```csharp
var jest = JestSDK.Instance;

// Open privacy policy
jest.OpenPrivacyPolicy();

// Open terms of service
jest.OpenTermsOfService();

// Open copyright information
jest.OpenCopyright();
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
   - Configure purchase responses

This is especially useful for:

- Debugging player progression
- Verifying analytics event data
- Testing notification scheduling
- Simulating different player scenarios
- Testing payments
- Testing referral flows

The mock configuration persists in edit mode, allowing you to maintain test data between play sessions.

## Requirements and dependencies

- Unity 6000.0 or later
- Newtonsoft.Json 3.2.1 or later
