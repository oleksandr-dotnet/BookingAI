#!/usr/bin/env bash
set -euo pipefail

: "${TEST_API_URL:?TEST_API_URL is required}"

base_url="${TEST_API_URL%/}"
health_url="${base_url}/health"
max_attempts="${HEALTH_MAX_ATTEMPTS:-36}"
sleep_seconds="${HEALTH_SLEEP_SECONDS:-10}"

echo "Waiting for ${health_url} (up to $((max_attempts * sleep_seconds))s)..."

for ((attempt = 1; attempt <= max_attempts; attempt++)); do
  if curl -fsS "${health_url}" >/dev/null; then
    echo "API healthy after ${attempt} attempt(s)."
    curl -fsS "${health_url}"
    exit 0
  fi
  echo "Attempt ${attempt}/${max_attempts} not ready; sleeping ${sleep_seconds}s..."
  sleep "${sleep_seconds}"
done

echo "API did not become healthy in time."
exit 1
