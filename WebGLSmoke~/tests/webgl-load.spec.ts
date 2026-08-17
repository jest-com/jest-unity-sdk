import { test, expect } from "@playwright/test";

const MAKE_DYN_CALL_ERROR = "makeDynCall is not defined";
const LIFECYCLE_RUN_ID = "webgl-smoke-lifecycle";
const LIFECYCLE_BRIDGE_ASSERTIONS = [
  "lifecycle hide callback crosses JS bridge",
  "lifecycle show callback crosses JS bridge",
  "lifecycle exit-requested callback crosses JS bridge",
  "lifecycle hide listener remains unsubscribed",
  "lifecycle show listener remains unsubscribed",
  "lifecycle exit-requested listener remains unsubscribed",
];

type RegressionAssertion = {
  name: string;
  pass: boolean;
  actual: string | null;
  expected: string | null;
  details: string | null;
};

type ScenarioResult = {
  status: "pass" | "fail";
  assertions: RegressionAssertion[];
  error: { message: string; stack: string } | null;
};

/**
 * Loads the headless WebGL regression build and verifies the SDK boots.
 *
 * `JestSDK.Init()` runs in the Unity main loop and its success callback is a
 * `makeDynCall` site. If the jslib macros were stripped (e.g. by a reformat),
 * that callback throws `ReferenceError: makeDynCall is not defined` and the
 * regression runner never advertises readiness. So a `runnerReady` message is
 * proof the macro expansion survived and the build actually loads.
 */
test("WebGL build loads and SDK init reaches runnerReady (no makeDynCall crash)", async ({ page }) => {
  const consoleErrors: string[] = [];
  page.on("console", (msg) => {
    if (msg.type() === "error") consoleErrors.push(msg.text());
  });
  page.on("pageerror", (err) => consoleErrors.push(String(err?.message ?? err)));

  // Stub the Jest platform SDK before Unity boots. With it present, the jslib's
  // loadSdk() short-circuits and init() resolves, so the success makeDynCall
  // path runs — exactly the path that crashed in production when macros broke.
  // The lifecycle mock retains the callbacks registered by JestSDK.jslib so the
  // regression scenario can invoke them and verify the complete JS -> C# path.
  await page.addInitScript(() => {
    const lifecycleCallbacks: {
      onHide?: () => void;
      onShow?: () => void;
      onExitRequested?: () => void;
    } = {};

    const registerLifecycleCallback = (
      name: "onHide" | "onShow" | "onExitRequested",
      callback: () => void,
    ) => {
      lifecycleCallbacks[name] = callback;
      return () => {
        if (lifecycleCallbacks[name] === callback) delete lifecycleCallbacks[name];
      };
    };

    (window as unknown as { JestSDK: unknown }).JestSDK = {
      init: () => Promise.resolve(),
      lifecycle: {
        onHide: (callback: () => void) => registerLifecycleCallback("onHide", callback),
        onShow: (callback: () => void) => registerLifecycleCallback("onShow", callback),
        onExitRequested: (callback: () => void) =>
          registerLifecycleCallback("onExitRequested", callback),
      },
    };
    (
      window as unknown as { __jestSdkLifecycleCallbacks: typeof lifecycleCallbacks }
    ).__jestSdkLifecycleCallbacks = lifecycleCallbacks;
    (window as unknown as { __runnerReady: unknown }).__runnerReady = null;
    (window as unknown as { __scenarioResults: Record<string, unknown> }).__scenarioResults = {};
    window.addEventListener("message", (event) => {
      const data = event.data;
      if (
        data &&
        typeof data === "object" &&
        data.type === "runnerReady" &&
        data.engine === "unity"
      ) {
        (window as unknown as { __runnerReady: unknown }).__runnerReady = data;
      } else if (
        data &&
        typeof data === "object" &&
        data.type === "scenarioResult" &&
        typeof data.runId === "string"
      ) {
        (
          window as unknown as { __scenarioResults: Record<string, unknown> }
        ).__scenarioResults[data.runId] = data;
      }
    });
  });

  await page.goto("/index.html");

  await page
    .waitForFunction(
      () => (window as unknown as { __runnerReady: unknown }).__runnerReady !== null,
      undefined,
      { timeout: 120_000 },
    )
    .catch(() => {
      // Fall through so the assertions below produce a clearer diagnosis.
    });

  const macroCrash = consoleErrors.find((e) => e.includes(MAKE_DYN_CALL_ERROR));
  expect(
    macroCrash,
    `Detected the jslib macro-corruption crash. Console errors:\n${consoleErrors.join("\n")}`,
  ).toBeUndefined();

  const ready = await page.evaluate(
    () => (window as unknown as { __runnerReady: Record<string, unknown> | null }).__runnerReady,
  );
  expect(
    ready,
    `SDK regression runner never signalled readiness within timeout — the build did not load. Console errors:\n${consoleErrors.join("\n") || "(none)"}`,
  ).toBeTruthy();
  expect(ready?.engine).toBe("unity");
  expect(Array.isArray(ready?.scenarios) && (ready?.scenarios as unknown[]).length).toBeTruthy();

  await page.evaluate((runId) => {
    window.postMessage(
      {
        source: "textclub-sdk-regression",
        protocolVersion: 1,
        type: "runScenario",
        engine: "unity",
        scenario: "lifecycle",
        runId,
      },
      "*",
    );
  }, LIFECYCLE_RUN_ID);

  await page.waitForFunction(
    (runId) =>
      Boolean(
        (window as unknown as { __scenarioResults: Record<string, unknown> }).__scenarioResults[
          runId
        ],
      ),
    LIFECYCLE_RUN_ID,
  );

  const lifecycleResult = await page.evaluate(
    (runId) =>
      (window as unknown as { __scenarioResults: Record<string, ScenarioResult> }).__scenarioResults[
        runId
      ],
    LIFECYCLE_RUN_ID,
  );
  expect(lifecycleResult.status, JSON.stringify(lifecycleResult, null, 2)).toBe("pass");

  for (const assertionName of LIFECYCLE_BRIDGE_ASSERTIONS) {
    const assertion = lifecycleResult.assertions.find(({ name }) => name === assertionName);
    expect(assertion, `Missing lifecycle regression assertion: ${assertionName}`).toMatchObject({
      pass: true,
      actual: "1",
      expected: "1",
    });
  }
});
