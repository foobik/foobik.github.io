#!/usr/bin/env node
// tools/resolve-theme.js
//
// Bridges Config.fs's `theme` setting into the Sass build step. The Sass CLI
// runs (via `npm run build:scss` / `make build-scss`) BEFORE `dotnet run`, so
// it can't ask the F# program which theme is selected - this script reads
// Config.fs directly instead, works out which .scss file that theme maps to,
// and shells out to `sass` with the right entry file. (The HTML-fragment side
// of theming needs no such bridge - Program.fs/SkunkHtml.fs read Config.fs
// natively, since dotnet runs after this script.)

const { spawnSync } = require("node:child_process");
const fs = require("node:fs");
const path = require("node:path");

const ROOT = path.resolve(__dirname, "..");
const CONFIG_PATH = path.join(ROOT, "Config.fs");
const THEMES_DIR = path.join(ROOT, "themes");
const OUTPUT = "css/tweaks.css";
// Resolve the locally-installed Dart Sass explicitly rather than relying on
// PATH: a system/Ruby "sass" (e.g. from a Ruby gem) can shadow it there and
// silently fail on Dart-Sass-only flags like --no-source-map.
const SASS_BIN = path.join(ROOT, "node_modules", ".bin", process.platform === "win32" ? "sass.cmd" : "sass");

// Must match a line like:  let theme = "ocean"   // comment...
const THEME_LINE_RE = /^\s*let\s+theme\s*=\s*"([^"]*)"/m;

function fail(message) {
  console.error(`resolve-theme: ${message}`);
  process.exit(1);
}

function readConfiguredTheme() {
  let contents;
  try {
    contents = fs.readFileSync(CONFIG_PATH, "utf8");
  } catch (err) {
    fail(`could not read ${CONFIG_PATH}: ${err.message}`);
  }
  const match = contents.match(THEME_LINE_RE);
  if (!match) {
    fail(
      `could not find a "let theme = \\"...\\"" line in ${CONFIG_PATH}. ` +
        `Expected a line like:  let theme = "default"`
    );
  }
  return match[1];
}

function listAvailableThemes() {
  // "default" always maps to the root tweaks.scss. Every other option is
  // discovered by scanning themes/ for <name>/theme.scss, so dropping in a
  // new theme folder makes it available with no code changes here.
  let entries;
  try {
    entries = fs.readdirSync(THEMES_DIR, { withFileTypes: true });
  } catch (err) {
    fail(`could not read themes directory ${THEMES_DIR}: ${err.message}`);
  }
  const names = entries
    .filter((e) => e.isDirectory() && fs.existsSync(path.join(THEMES_DIR, e.name, "theme.scss")))
    .map((e) => e.name);
  return {
    // Spread first, "default" last: Config.fs/Program.fs treat theme = "default"
    // as "always html/, never a themes/ override" (see Config.fs's `themeDir`),
    // so this must always resolve to tweaks.scss too, even if someone creates a
    // themes/default/theme.scss folder - otherwise CSS and HTML would disagree
    // about what "default" means.
    ...Object.fromEntries(names.map((name) => [name, path.join("themes", name, "theme.scss")])),
    default: "tweaks.scss",
  };
}

function resolveEntryFile(themeName, available) {
  if (Object.prototype.hasOwnProperty.call(available, themeName)) {
    return available[themeName];
  }
  const options = Object.keys(available).sort().join(", ");
  fail(
    `Config.fs sets theme = "${themeName}", but no matching theme was found.\n` +
      `  Valid options: ${options}\n` +
      `  ("default" uses tweaks.scss directly; any other name must match a themes/<name>/theme.scss file.)`
  );
}

function main() {
  const themeName = readConfiguredTheme();
  const available = listAvailableThemes();
  const entryFile = resolveEntryFile(themeName, available);

  console.log(`resolve-theme: theme = "${themeName}" -> ${entryFile}`);

  const result = spawnSync(SASS_BIN, [`${entryFile}:${OUTPUT}`, "--no-source-map"], {
    cwd: ROOT,
    stdio: "inherit",
    shell: process.platform === "win32",
  });

  if (result.error) {
    fail(`failed to run sass: ${result.error.message}`);
  }
  process.exit(result.status ?? 1);
}

main();
