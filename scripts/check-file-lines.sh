#!/bin/sh
# Pre-commit hook to check file line counts
# Rejects files exceeding MAX_LINES threshold

MAX_LINES=500
ERROR_FILES=""

for file in $(git diff --cached --name-only --diff-filter=ACM); do
    # Skip binary files and certain extensions
    case "$file" in
        *.lock|*.json|*.md|*.yml|*.yaml) continue ;;
    esac

    # Count lines (excluding common noise)
    lines=$(wc -l < "$file" 2>/dev/null || echo 0)

    if [ "$lines" -gt "$MAX_LINES" ]; then
        ERROR_FILES="$ERROR_FILES  $file ($lines lines)\n"
    fi
done

if [ -n "$ERROR_FILES" ]; then
    echo "ERROR: The following files exceed $MAX_LINES lines:"
    echo "$ERROR_FILES"
    echo "Commit rejected. Split files or increase threshold if intentional."
    exit 1
fi
