#!/bin/sh
# Pre-commit hook to check file line counts
# Rejects files exceeding MAX_LINES threshold

MAX_LINES=500
ERROR_FILES=""

git diff --cached --name-only --diff-filter=ACM -z | while IFS= read -r -d '' file; do
    # Skip binary files and certain extensions
    case "$file" in
        *.lock|*.json|*.md|*.yml|*.yaml) continue ;;
    esac

    # Count lines from staged snapshot
    lines=$(git show ":$file" 2>/dev/null | wc -l) || lines=0

    if [ "$lines" -gt "$MAX_LINES" ]; then
        ERROR_FILES="$ERROR_FILES  $file ($lines lines)\n"
    fi
done

if [ -n "$ERROR_FILES" ]; then
    echo "ERROR: The following files exceed $MAX_LINES lines:"
    printf "%b" "$ERROR_FILES"
    echo "Commit rejected. Split files or increase threshold if intentional."
    exit 1
fi
