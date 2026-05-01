#!/usr/bin/env bash

while IFS= read -r -d '' file; do
    [ -f "$file" ] || continue
    size=$(stat -c %s "$file" 2>/dev/null || stat -f %z "$file")
    if (( size >= 52428800 )); then
        echo "FAIL: '$file' is $size bytes (>= 50 MiB)"
        exit 1
    fi
done < <(git diff --staged --name-only -z) && echo "SUCCESS: All staged files are under 50 MiB"