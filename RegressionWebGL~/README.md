# Jest Unity SDK Regression WebGL Project

This Unity project builds a WebGL regression fixture against the SDK package in this repository.

Run from the repository root:

```bash
Unity.exe -batchmode -nographics -quit -projectPath RegressionWebGL~ -buildTarget WebGL -executeMethod com.jest.sdk.regression.Editor.JestSdkRegressionBuild.BuildWebGL -outputPath <output-dir>
```

The project imports the SDK with `file:../../`, so the build uses the current checkout rather than an installed sample copy.

The regression runner advertises scenario names to the host page and executes them on demand. Current scenarios cover core initialization (including `SetLoadingProgress` and `MarkGameLoaded`), player data, commerce reads and safe error returns, notifications, social helpers, referrals, legal links, internal helpers, and validation guardrails for APIs that would otherwise trigger UI or navigation flows.

Positive navigation, login, registration overlay, debug registration, and subscription checkout flows are intentionally not launched from the shared fixture because they can navigate away from the host page or wait for user interaction. Their Unity-side argument validation and registered-player guardrails are covered instead.
