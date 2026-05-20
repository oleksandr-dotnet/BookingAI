#!/usr/bin/env bash
set -euo pipefail

: "${TEST_API_URL:?TEST_API_URL is required}"

base_url="${TEST_API_URL%/}"
health_url="${base_url}/health"
max_attempts="${HEALTH_MAX_ATTEMPTS:-40}"
sleep_seconds="${HEALTH_SLEEP_SECONDS:-15}"
initial_delay="${HEALTH_INITIAL_DELAY_SECONDS:-45}"

echo "Health check target: ${health_url}"
echo "Waiting ${initial_delay}s after deploy trigger before polling..."
sleep "${initial_delay}"

is_healthy() {
  local http_code
  local body_file
  body_file="$(mktemp)"

  http_code="$(curl -sS -o "$body_file" -w "%{http_code}" --connect-timeout 10 --max-time 30 "${health_url}" 2>/dev/null || echo "000")"

  if [[ "$http_code" != "200" ]]; then
    echo "  HTTP ${http_code}"
    if [[ -s "$body_file" ]]; then
      head -c 500 "$body_file" | tr '\n' ' '
      echo ""
    fi
    rm -f "$body_file"
    return 1
  fi

  if command -v jq >/dev/null 2>&1; then
    if jq -e '.status == "healthy"' "$body_file" >/dev/null 2>&1; then
      cat "$body_file"
      rm -f "$body_file"
      return 0
    fi
    echo "  Body not healthy: $(cat "$body_file")"
    rm -f "$body_file"
    return 1
  fi

  if grep -q '"healthy"' "$body_file"; then
    cat "$body_file"
    rm -f "$body_file"
    return 0
  fi

  echo "  Unexpected body: $(cat "$body_file")"
  rm -f "$body_file"
  return 1
}

echo "Polling up to ${max_attempts} times every ${sleep_seconds}s (max ~$((initial_delay + max_attempts * sleep_seconds))s total)..."

for ((attempt = 1; attempt <= max_attempts; attempt++)); do
  echo "Attempt ${attempt}/${max_attempts}..."
  if is_healthy; then
    echo "API healthy."
    exit 0
  fi
  if [[ "$attempt" -lt "$max_attempts" ]]; then
    sleep "${sleep_seconds}"
  fi
done

echo "::error::API did not become healthy at ${health_url}"
echo "Verify GitHub secret TEST_API_URL matches your live API (e.g. https://bookingai-api.onrender.com)."
exit 1
