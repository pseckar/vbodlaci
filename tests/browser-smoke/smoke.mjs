import fs from "node:fs";
import path from "node:path";
import { chromium } from "playwright";

const baseUrl = process.env.BASE_URL ?? "http://127.0.0.1:5270";
const artifactDir = process.env.SMOKE_ARTIFACT_DIR ?? "./artifacts";
const reportPath = process.env.SMOKE_REPORT_PATH ?? path.join(artifactDir, "smoke-report.json");

const routes = [
  "/",
  "/breathwork-v-bodlaci",
  "/kone-v-bodlaci",
  "/veterina-v-bodlaci",
  "/zasady-zpracovani-osobnich-udaju",
  "/zasady-cookies",
  "/podminky-kurzu",
  "/Identity/Account/Login"
];

await fs.promises.mkdir(artifactDir, { recursive: true });

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage();

const checks = [];
for (const route of routes) {
  const url = `${baseUrl}${route}`;
  let ok = true;
  let status = -1;
  let error = null;

  try {
    const response = await page.goto(url, { waitUntil: "networkidle", timeout: 20000 });
    status = response?.status() ?? -1;
    ok = status >= 200 && status < 400;

    if (route === "/") {
      const html = await page.content();
      if (!html.includes("V bodláčí")) {
        ok = false;
        error = "Missing brand text on home page.";
      }
    }

    const shotName = route === "/" ? "home" : route.replace(/[\\/{}]/g, "_").replace(/^_+/, "");
    await page.screenshot({ path: path.join(artifactDir, `${shotName || "page"}.png`), fullPage: true });
  } catch (err) {
    ok = false;
    error = String(err);
  }

  checks.push({ route, url, status, ok, error });
}

await browser.close();

const report = {
  baseUrl,
  generatedAt: new Date().toISOString(),
  checks,
  success: checks.every((c) => c.ok)
};

await fs.promises.writeFile(reportPath, JSON.stringify(report, null, 2), "utf8");

if (!report.success) {
  process.exitCode = 1;
}
