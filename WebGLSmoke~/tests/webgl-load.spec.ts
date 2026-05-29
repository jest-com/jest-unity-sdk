import { test, expect } from "@playwright/test";

const MAKE_DYN_CALL_ERROR = "makeDynCall is not defined";

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
  await page.addInitScript(() => {
    (window as unknown as { JestSDK: unknown }).JestSDK = {
      init: () => Promise.resolve(),
    };
    (window as unknown as { __runnerReady: unknown }).__runnerReady = null;
    window.addEventListener("message", (event) => {
      const data = event.data;
      if (
        data &&
        typeof data === "object" &&
        data.type === "runnerReady" &&
        data.engine === "unity"
      ) {
        (window as unknown as { __runnerReady: unknown }).__runnerReady = data;
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
});
