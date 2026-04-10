import { copyFileSync, chmodSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const hookSource = join(__dirname, "check-file-lines.sh");
const hookTarget = join(__dirname, "../.git/hooks/pre-commit");

try {
  // Copy hook to .git/hooks/pre-commit and make it executable
  copyFileSync(hookSource, hookTarget);
  chmodSync(hookTarget, 0o755);
  console.log("Pre-commit hook installed.");
} catch {
  // Hook installation may fail
  console.log(
    "Note: Could not install pre-commit hook automatically. Run 'node scripts/setup-hooks.js' with appropriate permissions.",
  );
}
