import { copyFileSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const hookSource = join(__dirname, "check-file-lines.sh");
const hookTarget = join(__dirname, "../.git/hooks/pre-commit");

try {
  // Copy hook to .git/hooks/pre-commit
  copyFileSync(hookSource, hookTarget);
  console.log("Pre-commit hook installed.");
} catch {
  // Hook installation may fail
  console.log(
    "Note: Could not install pre-commit hook automatically. Run 'node scripts/setup-hooks.js' with appropriate permissions.",
  );
}
