# WebGL load smoke test

Headless browser check that a WebGL build of the SDK actually **loads** —
specifically that `JestSDK.Init()` runs without
`ReferenceError: makeDynCall is not defined`, the runtime crash that happens
when the `{{{ makeDynCall(...) }}}` macros in `Runtime/Plugins/JestSDK.jslib`
get stripped by a reformat.

It builds nothing itself; it loads an already-built fixture and asserts the
regression runner advertises `runnerReady`, which only happens after init
succeeds.

This folder ends in `~` so Unity ignores it.

## How it works

1. The CI job builds `RegressionWebGL~` with `-disableWebGLCompression` so the
   output is plain files.
2. `server.mjs` serves that directory (`SMOKE_BUILD_DIR`).
3. `tests/webgl-load.spec.ts` stubs `window.JestSDK` (so `init()` resolves and
   the success `makeDynCall` path runs), loads `index.html` in headless
   Chromium (SwiftShader for GL), and waits for the `runnerReady` postMessage.

## Run locally

```bash
# 1. Build the fixture uncompressed (from the repo root)
Unity.exe -batchmode -nographics -quit \
  -projectPath RegressionWebGL~ -buildTarget WebGL \
  -executeMethod com.jest.sdk.regression.Editor.JestSdkRegressionBuild.BuildWebGL \
  -outputPath <abs-build-dir> -disableWebGLCompression

# 2. Run the smoke test
cd WebGLSmoke~
npm install
npx playwright install --with-deps chromium
SMOKE_BUILD_DIR=<abs-build-dir> npm test
```
