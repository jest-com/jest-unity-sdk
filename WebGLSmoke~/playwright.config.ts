import { defineConfig } from "@playwright/test";

const PORT = Number(process.env.SMOKE_PORT ?? 8123);

export default defineConfig({
  testDir: "./tests",
  timeout: 150_000,
  expect: { timeout: 10_000 },
  fullyParallel: false,
  retries: 0,
  reporter: [["list"], ["html", { open: "never" }]],
  use: {
    baseURL: `http://127.0.0.1:${PORT}`,
    headless: true,
    screenshot: "only-on-failure",
    trace: "retain-on-failure",
    launchOptions: {
      // Unity WebGL needs a working GL context; force SwiftShader so it runs
      // in headless CI without a GPU.
      args: [
        "--enable-unsafe-swiftshader",
        "--use-gl=angle",
        "--use-angle=swiftshader",
        "--ignore-gpu-blocklist",
      ],
    },
  },
  projects: [{ name: "chromium" }],
  webServer: {
    command: "node server.mjs",
    url: `http://127.0.0.1:${PORT}/index.html`,
    timeout: 60_000,
    reuseExistingServer: !process.env.CI,
  },
});
