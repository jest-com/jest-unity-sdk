// Minimal static file server for an uncompressed Unity WebGL build.
// The build directory is passed via SMOKE_BUILD_DIR; port via SMOKE_PORT.
import { createServer } from "node:http";
import { readFile, stat } from "node:fs/promises";
import { join, normalize, extname, resolve } from "node:path";

const ROOT = process.env.SMOKE_BUILD_DIR ? resolve(process.env.SMOKE_BUILD_DIR) : null;
const PORT = Number(process.env.SMOKE_PORT ?? 8123);

if (!ROOT) {
  console.error("SMOKE_BUILD_DIR is not set");
  process.exit(1);
}

const CONTENT_TYPES = {
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".mjs": "text/javascript; charset=utf-8",
  ".wasm": "application/wasm",
  ".json": "application/json; charset=utf-8",
  ".data": "application/octet-stream",
  ".css": "text/css; charset=utf-8",
  ".png": "image/png",
  ".jpg": "image/jpeg",
  ".svg": "image/svg+xml",
  ".ico": "image/x-icon",
  ".symbols": "application/octet-stream",
};

const server = createServer(async (req, res) => {
  try {
    const urlPath = decodeURIComponent((req.url ?? "/").split("?")[0]);
    let rel = normalize(urlPath);
    if (rel === "/" || rel === "\\" || rel === ".") {
      rel = "/index.html";
    }
    const filePath = join(ROOT, rel);
    if (filePath !== ROOT && !filePath.startsWith(ROOT + (process.platform === "win32" ? "\\" : "/"))) {
      res.writeHead(403);
      res.end("Forbidden");
      return;
    }
    const info = await stat(filePath).catch(() => null);
    if (!info || !info.isFile()) {
      res.writeHead(404);
      res.end("Not found");
      return;
    }
    const body = await readFile(filePath);
    res.writeHead(200, {
      "Content-Type": CONTENT_TYPES[extname(filePath).toLowerCase()] ?? "application/octet-stream",
      "Cache-Control": "no-store",
    });
    res.end(body);
  } catch (err) {
    res.writeHead(500);
    res.end(String(err));
  }
});

server.listen(PORT, "127.0.0.1", () => {
  console.log(`smoke server on http://127.0.0.1:${PORT} serving ${ROOT}`);
});
